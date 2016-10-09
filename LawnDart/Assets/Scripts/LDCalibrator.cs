using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityCoroutine = System.Collections.IEnumerator;
using PGT.Core;

namespace McHorseface.LawnDart
{
    public class LDCalibrator : MonoBehaviour {

        public const string CALIB_TRYOUT = "calib_tryout";

        [SerializeField]
        Text gazeSlide;
        [SerializeField]
        GameObject waitSlide;
        [SerializeField]
        GameObject calibrationSlide;
        [SerializeField]
        GameObject confirmationSlide;
        [SerializeField]
        GameObject confirmationYes;
        [SerializeField]
        GameObject confirmationNo;
        [SerializeField]
        GameObject trySlide;
        [SerializeField]
        GameObject[] finalSlide;
        [SerializeField]
        GameObject mii;
        [SerializeField]
        GameObject[] warp;

        [SerializeField]
        GameObject hand0;
        [SerializeField]
        GameObject hand1;

        [SerializeField]
        GameObject Callie;

        bool doHandFlip = false;

        [SerializeField]
        AudioSource continueSound;

        AnimationController cfmYesAnim;
        AnimationController cfmNoAnim;

        Vector3 cfmYesOrigin;
        Vector3 cfmNoOrigin;

        int gazeOnListener;
        int gazeOffListener;

        enum ButtonState
        {
            None, Yes, No
        }

        ButtonState buttonState;

	    // Use this for initialization
	    void Start () {
            cfmYesAnim = confirmationYes.GetComponent<AnimationController>();
            cfmNoAnim = confirmationNo.GetComponent<AnimationController>();
            cfmYesOrigin = confirmationYes.transform.position;
            cfmNoOrigin = confirmationNo.transform.position;
            StartCoroutine(CalibrationStart());
            if (LDController.instance) 
                LDController.instance.ShowSprite();
	    }

        UnityCoroutine FlipHands()
        {
            doHandFlip = true;
            bool slide2 = true;
            while (doHandFlip)
            {
                hand0.SetActive(slide2);
                hand1.SetActive(!slide2);
                slide2 = !slide2;
                yield return new WaitForSeconds(0.7f);
            }

            hand0.SetActive(false);
            hand1.SetActive(false);
        }
        


        UnityCoroutine CalibrationStart()
        {


            waitSlide.SetActive(true);
            calibrationSlide.SetActive(false);
            confirmationSlide.SetActive(false);
            confirmationYes.SetActive(false);
            confirmationNo.SetActive(false);
            trySlide.SetActive(false);
            foreach (var s in finalSlide) s.SetActive(false);
            hand0.SetActive(false);
            hand1.SetActive(false);
            gazeSlide.gameObject.SetActive(false);
            doHandFlip = false;
            enabled = false;

            foreach (var w in warp) w.SetActive(false);

            yield return new WaitUntil(() => LDController.instance != null && LDController.instance.enableControls);
            waitSlide.SetActive(false);
            var callie = Instantiate(Callie);
            callie.transform.position = new Vector3(0f, 5f, 4f);
            gazeSlide.gameObject.SetActive(true);

            gazeOffListener = EventRegistry.instance.AddEventListener(CalibrationMiiController.GAZE_OFF, () =>
            {
                gazeSlide.text = "Look at Callie";
            }, true);

            gazeOnListener = EventRegistry.instance.AddEventListener(CalibrationMiiController.GAZE_ON, () =>
            {
                gazeSlide.text = "Hold the phone up to your ear";
            }, true);

            yield return new WaitForEvent(CalibrationMiiController.CALIB_SEQ_START);
            continueSound.Play();
            calibrationSlide.SetActive(true);
            gazeSlide.gameObject.SetActive(false);
            
            EventRegistry.instance.RemoveEventListener(CalibrationMiiController.GAZE_ON, gazeOnListener);
            EventRegistry.instance.RemoveEventListener(CalibrationMiiController.GAZE_OFF, gazeOffListener);

            yield return new WaitForEvent(CalibrationMiiController.CALIB_SEQ_END);
            continueSound.Play();
            trySlide.SetActive(true);
            doHandFlip = true;
            StartCoroutine(FlipHands());
            EventRegistry.instance.Invoke(CALIB_TRYOUT);

            trySlide.SetActive(true);
            confirmationSlide.SetActive(false);
            confirmationYes.SetActive(false);
            confirmationNo.SetActive(false);

            calibrationSlide.SetActive(false);

            for (int i=0; i < 5; i++)
            {
                yield return new WaitForEvent(LDController.BUTTON_OFF);
            }
            continueSound.Play();
            var n_mii = Instantiate(mii);
            n_mii.transform.position = new Vector3(0, 1, 4);



            // whenever a button_4_off is sent, a button_off is also sent
            yield return new WaitForEvent(MiiAnimationController.MII_HIT);
            yield return new WaitForSeconds(1f);

            doHandFlip = false;
            hand0.SetActive(false);
            hand1.SetActive(false);

            trySlide.SetActive(false);
            continueSound.Play();

            for(int i=0; i<3; i++)
            {
                warp[i].SetActive(true);
                finalSlide[i].SetActive(true);
                yield return new WaitForSeconds(0.4f);
            }

        }


        void Scale(GameObject obj, AnimationController anim, Vector3 dest)
        {
            anim.RunAnimation(anim.Lerp(
                () => obj.transform.localScale,
                (Vector3 vec) => obj.transform.localScale = vec,
                Vector3.Lerp,
                (Vector3 vec1, Vector3 vec2) => 10000 * Vector3.SqrMagnitude(vec1 - vec2),
                dest,
                1f), true, AnimationController.LOCAL_SCALE);
        }


	
	    // Update is called once per frame
	    void Update () {
            Vector3 rot = LDController.instance.GetCalibratedRotation() * Vector3.forward;
            
            if (rot.x  > 0.3 && buttonState != ButtonState.No)
            {
                Scale(confirmationNo, cfmNoAnim, 0.007f * Vector3.one);
                Scale(confirmationYes, cfmYesAnim, 0.005f * Vector3.one); 
                buttonState = ButtonState.No;

            }else if(rot.x < -0.3 && buttonState != ButtonState.Yes)
            {
                Scale(confirmationNo, cfmNoAnim, 0.005f * Vector3.one);
                Scale(confirmationYes, cfmYesAnim, 0.007f * Vector3.one);
                buttonState = ButtonState.Yes;
            }else if(rot.x < 0.3 && rot.x > -0.3 && buttonState != ButtonState.None)
            {
                Scale(confirmationNo, cfmNoAnim, 0.005f * Vector3.one);
                Scale(confirmationYes, cfmYesAnim, 0.005f * Vector3.one);
                buttonState = ButtonState.None;
            }
	    }
    }

}
