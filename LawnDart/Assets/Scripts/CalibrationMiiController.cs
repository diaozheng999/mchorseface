using UnityEngine;
using UnityCoroutine = System.Collections.IEnumerator;
using PGT.Core;

namespace McHorseface.LawnDart
{
    public class CalibrationMiiController : MiiAnimationController
    {
        public const string GAZE_ON = "callie_gaze_on";
        public const string GAZE_OFF = "callie_gaze_off";
        public const string CALIB_SEQ_START = "callie_seq_start";
        public const string CALIB_SEQ_END = "callie_seq_end";
        public const string CALLIE_HIT = "callie_hit";

        public static CalibrationMiiController instance;

        [SerializeField]
        GameObject calliePrefab;

        [SerializeField]
        float CalibrationAnimProb = 0.5f;

        [SerializeField]
        Texture normalFace;

        [SerializeField]
        Texture blinkFace;

        [SerializeField]
        Texture aimFace;

        [SerializeField]
        float xViewRange = 0.1f;

        [SerializeField]
        float yViewRange = 0.5f;

        [SerializeField]
        AudioSource calibrationSound;

        bool isBlinking = false;
        bool isAiming = false;
        bool isExcited = false;

        public bool isCalibrating = false;

        Vector3 p = new Vector3(0.5f, 0.5f, 0);
        Camera cam;

        int calibration_state = 0;
        void Start ()
        {

            cam = Camera.main;
            anim = GetComponent<Animator>();

            anim.SetFloat("IdleSpeed", 2 + (Random.value - 0.5f));

            StartCoroutine(DoWave());

            instance = this;
            
        }

        public void Reposition()
        {
            collapsed.transform.position = Player.instance.transform.position + Vector3.ProjectOnPlane(3f * (Camera.main.transform.forward + Camera.main.transform.right), Vector3.up);
        }

        protected override UnityCoroutine DoWave()
        {
            yield return new WaitForEndOfFrame();
            while (doWave)
            {
                if(Random.value < waveChance)
                {
                    if(Random.value < CalibrationAnimProb)
                    {
                        anim.SetTrigger("DoCalib");
                    }else
                    {
                        anim.SetTrigger("DoWave2");
                    }
                }
                yield return new WaitForSeconds(Random.value);
                
                if(Random.value < blinkChance)
                {
                    head.material.mainTexture = blinkFace;
                    yield return new WaitForSeconds(0.2f * Random.value);
                    head.material.mainTexture = isAiming? aimFace : normalFace;
                }
            }
        }

        public void ChangeHeadTexture()
        {
            head.material.mainTexture = aimFace;
            isAiming = true;
        }

        public void ChangeHeadTextureBack()
        {
            head.material.mainTexture = normalFace;
            isAiming = false;
        }

        public override void Fragment(Vector3 position)
        {
            base.Fragment(position);

            this.SetTimeout(2f, () =>
            {
                EventRegistry.instance.Invoke(CALLIE_HIT);
                Destroy(collapsed.gameObject);
            });

        }


        void Update()
        {
            if (!alive) return;
            //compute whether the mii is in camera view

            Vector3 point = cam.WorldToViewportPoint(transform.position + 0.8f * Vector3.up) - p;

            // keep callie looking at the camera
            Vector3 dist = Camera.main.transform.position - transform.position;
            dist = Vector3.ProjectOnPlane(dist, Vector3.up);
            collapsed.transform.LookAt(transform.position - dist, Vector3.up);


            // teleport close to the player if she's too far away
            if(dist.sqrMagnitude > 100 || dist.sqrMagnitude < 0.1)
            {
                Reposition();
            }

            if (Mathf.Abs(point.x) < xViewRange && Mathf.Abs(point.y) < yViewRange)
            {
                if (!isExcited)
                {
                    anim.SetBool("LineOfSight", true);
                    isExcited = true;
                    EventRegistry.instance.Invoke(GAZE_ON);
                    Debug.Log("Callie: Gaze on");
                    isCalibrating = true;
                    
                    if(LDController.instance.enableControls)
                        StartCoroutine(CalibrationSequence());
                }
            }
            else if (isExcited)
            {
                anim.SetBool("LineOfSight", false);
                isExcited = false;
                Debug.Log("Callie: GazeOff, calibration_state=" + calibration_state);
                if(calibration_state == 1)
                {
                    Debug.Log("Callie: Gaze off");
                    EventRegistry.instance.Invoke(GAZE_OFF);
                    calibration_state = -1;
                    isCalibrating = false;
                }
            }
        }


        int current_try = -1;

        UnityCoroutine CalibrationSequence()
        {
            calibration_state = 0;
            for (int iter = 0; calibration_state >= 0 && alive; iter++)
            {
                switch (calibration_state)
                {
                    case 0:
                        
                        calibration_state = 1;
                        Debug.Log("Callie: Sending vibration sequence.");

                        LDController.instance.Vibrate();
                        yield return new WaitForSeconds(0.6f);
                        LDController.instance.Vibrate();
                        yield return new WaitForEndOfFrame();

                        Debug.Log("Callie: switching states.");
                        continue;
                    case 1:

                        var rot = LDController.instance.GetRawRotation().eulerAngles;

                        var vert_angle = rot.z % 360f;

                        if(vert_angle > 0 && vert_angle < 90)
                        {
                            calibrationSound.Play();
                            calibration_state = 4;
                            current_try = iter;
                            Debug.Log("Callie: Phone in pos. Current try="+current_try);
                            EventRegistry.instance.SetTimeout(3f, (int t) =>
                            {
                                Debug.Log("Callie: In-pose timeout calibration_state="+calibration_state+" current_try="+current_try);
                                if (calibration_state == 4 && current_try == t)
                                {
                                    Debug.Log("Callie: timer input accepted");
                                    calibration_state = 5;
                                }else
                                {
                                    Debug.Log("Callie: timer input rejected");
                                }
                            }, iter);
                            Debug.Log("Callie: Calibration Sequence started");
                            EventRegistry.instance.Invoke(CALIB_SEQ_START);
                        }
                        yield return new WaitForEndOfFrame();
                        continue;
                        
                    case 4:
                        var rot2 = LDController.instance.GetRawRotation().eulerAngles;

                        var vert_angle2 = rot2.z % 360f;

                        if (vert_angle2 < 0 || vert_angle2 > 90 || LDController.instance.Accel.sqrMagnitude > 1.2f || LDController.instance.Accel.sqrMagnitude < 0.8f)
                        {
                            Debug.Log("Callie: Phone out pos.");
                            calibration_state = 1;
                            calibrationSound.Stop();
                            current_try = -1;
                        }else
                        {
                            Debug.Log("Callie: Phone still in pos.");
                        }
                        yield return new WaitForEndOfFrame();
                        continue;

                    case 5:
                        Vector3 dist =  Camera.main.transform.position - transform.position;
                        dist = Vector3.ProjectOnPlane(dist, Vector3.up);
                        LDController.instance.Calibrate(dist.normalized);
                        EventRegistry.instance.Invoke(CALIB_SEQ_END);
                        calibration_state = -1;
                        Debug.Log("Callie: Calibration sequence done.");
                        yield return new WaitForEndOfFrame();
                        continue;
                }
            }
            calibration_state = 0;
            isCalibrating = false;
            anim.SetBool("LineOfSight", false);
        }
    }
}

