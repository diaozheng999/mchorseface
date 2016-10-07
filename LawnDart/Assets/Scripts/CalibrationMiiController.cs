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


            EventRegistry.instance.AddEventListener(LDCalibrator.CALIB_TRYOUT, () =>
            {
                collapsed.transform.position = new Vector3(2.5f, 0f, 1f);
            });
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
            var dup = Instantiate(calliePrefab);
            dup.transform.position = transform.position;
            dup.transform.rotation = transform.rotation;
        }

        void Update()
        {
            if (!alive) return;
            //compute whether the mii is in camera view

            Vector3 point = cam.WorldToViewportPoint(transform.position + 0.8f * Vector3.up) - p;


            if (Mathf.Abs(point.x) < xViewRange && Mathf.Abs(point.y) < yViewRange)
            {
                if (!isExcited)
                {
                    anim.SetBool("LineOfSight", true);
                    isExcited = true;
                    StartCoroutine(CalibrationSequence());
                    EventRegistry.instance.Invoke(GAZE_ON);
                    Debug.Log("Callie: Gaze on");
                    isCalibrating = true;
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
                    calibration_state = 0;
                    isCalibrating = false;
                }
            }
        }


        UnityCoroutine CalibrationSequence()
        {
            calibration_state = 0;
            while (calibration_state >= 0)
            {
                switch (calibration_state)
                {
                    case 0:
                        calibration_state = 1;
                        yield return new WaitForEndOfFrame();
                        continue;
                    case 1:
                        yield return new WaitForEvent(LDController.BUTTON_OFF);
                        Debug.Log("Callie: Calibration Sequence started");
                        calibration_state = 2;
                        EventRegistry.instance.Invoke(CALIB_SEQ_START);
                        continue;
                    case 2:
                        yield return new WaitForEvent(LDController.BUTTON_OFF);


                        var fwd =  collapsed.transform.position - cam.transform.position;
                        LDController.instance.Calibrate(fwd.normalized);

                        EventRegistry.instance.Invoke(CALIB_SEQ_END);
                        calibration_state = -1;
                        Debug.Log("Callie: Calibration sequence done.");
                        continue;

                }
            }
            calibration_state = 0;
            isCalibrating = false;
        }
    }
}

