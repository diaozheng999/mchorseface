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
        Animator anim;
        [SerializeField]
        AnimationController anim2;

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
        GameObject driftSlide;
        [SerializeField]
        GameObject[] finalSlide;
        [SerializeField]
        Text killSlide;
        [SerializeField]
        GameObject mii;
        [SerializeField]
        GameObject[] warp;

        [SerializeField]
        GameObject hand0;
        [SerializeField]
        GameObject hand1;
		[SerializeField]
		GameObject hand2;
		[SerializeField]
		GameObject hand3;
		[SerializeField]
		GameObject hand4;

        [SerializeField]
        GameObject Callie;

        [SerializeField]
        GameObject Pacifist;

        bool doHandFlip = false;
        bool doCountMisses = false;

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
            StartCoroutine(CalibrationStart());
            if (LDController.instance) 
                LDController.instance.ShowSprite();
	    }

        UnityCoroutine FlipHands()
        {
            doHandFlip = true;
			int current = 0;
            while (doHandFlip)
            {
				hand0.SetActive(current == 0);
				hand1.SetActive(current == 1);
				hand2.SetActive (current == 2);
				hand3.SetActive (current == 3);
				hand4.SetActive (current == 4);
				current = (current + 1) % 5;
                yield return new WaitForSeconds(current == 0 ? 0.7f : 0.3f);
            }

            hand0.SetActive(false);
            hand1.SetActive(false);
			hand2.SetActive(false);
			hand3.SetActive(false);
			hand4.SetActive(false);
        }
        
        UnityCoroutine CountMisses()
        {
            doCountMisses = true;
            var misses = 0;
            while (doCountMisses)
            {
                yield return new WaitForEvent(LawnDartLauncher.DART_LAUNCH);
                misses++;

                Debug.Log("CountMisses: "+ misses + " miss(es).");

                if(misses > 20)
                {
                    Instantiate(Pacifist, new Vector3(3f, 1f, 4f), Quaternion.identity, null);
                    doCountMisses = false;
                }
                
            }
            Debug.Log("CountMisses: done");
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
			hand2.SetActive(false);
			hand3.SetActive(false);
			hand4.SetActive(false);
            driftSlide.SetActive(false);
            killSlide.gameObject.SetActive(false);
            gazeSlide.gameObject.SetActive(false);
            doHandFlip = false;
            enabled = false;

            foreach (var w in warp) w.SetActive(false);

            yield return new WaitUntil(() => LDController.instance != null && LDController.instance.enableControls);
            waitSlide.SetActive(false);

            anim.SetTrigger("flip");


            var callie = Instantiate(Callie);
            callie.transform.position = new Vector3(1f, 0f, 4f);
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

            anim.SetTrigger("flip");

            continueSound.Play();
            calibrationSlide.SetActive(true);
            gazeSlide.gameObject.SetActive(false);
            
            EventRegistry.instance.RemoveEventListener(CalibrationMiiController.GAZE_ON, gazeOnListener);
            EventRegistry.instance.RemoveEventListener(CalibrationMiiController.GAZE_OFF, gazeOffListener);

            yield return new WaitForEvent(CalibrationMiiController.CALIB_SEQ_END);

            anim.SetTrigger("flip");

            continueSound.Play();
            trySlide.SetActive(true);
            doHandFlip = true;
            StartCoroutine(FlipHands());
            EventRegistry.instance.Invoke(CALIB_TRYOUT);

            CalibrationMiiController.instance.Reposition();

            trySlide.SetActive(true);
            confirmationSlide.SetActive(false);
            confirmationYes.SetActive(false);
            confirmationNo.SetActive(false);

            calibrationSlide.SetActive(false);

            for (int i=0; i < 5; i++)
            {
				yield return new WaitForEvent(LawnDartLauncher.DART_LAUNCH);
				Debug.Log ("Throws done: " + i);
            }


            continueSound.Play();

            driftSlide.SetActive(true);
            doHandFlip = false;
            hand0.SetActive(false);
            hand1.SetActive(false);
			hand2.SetActive(false);
			hand3.SetActive(false);
			hand4.SetActive(false);
            trySlide.SetActive(false);
            anim.SetTrigger("flip");
            //apply an arbitrary rotation to to force calibration
            LDController.instance.StartDrift();
            yield return new WaitForEvent(CalibrationMiiController.CALIB_SEQ_END);
            LDController.instance.StopDrift();

            var n_mii = Instantiate(mii);
            var _c = n_mii.GetComponentInChildren<MiiAnimationController>();
            if (_c.gender == MiiGender.Random) _c.gender = Random.value > 0.5f ? MiiGender.Male : MiiGender.Female;
            n_mii.transform.position = new Vector3(0, 1, 4);

            killSlide.text = "Kill " + (_c.gender == MiiGender.Male ? "him" : "her") + "!";
            killSlide.gameObject.SetActive(true);
            driftSlide.SetActive(false);
            anim.SetTrigger("flip");
            doHandFlip = true;
            StartCoroutine(FlipHands());

            StartCoroutine(CountMisses());
            
            yield return new WaitForEvent(MiiAnimationController.MII_HIT);
            continueSound.Play();
            doHandFlip = false;
            hand0.SetActive(false);
            hand1.SetActive(false);
            hand2.SetActive(false);
            hand3.SetActive(false);
            hand4.SetActive(false);

            doCountMisses = false;
            anim.SetTrigger("hide");

            anim2.RunAnimation(
                anim2.Lerp(
                    () => anim2.transform.position,
                    (Vector3 pos) => anim2.transform.position = pos,
                    Vector3.Lerp,
                    Vector3.Distance,
                    new Vector3(0f, 0f, 14f),
                    2f, (object p) => { anim.SetTrigger("show"); }));

            yield return new WaitForSeconds(1.5f);

            doHandFlip = false;
            hand0.SetActive(false);
            hand1.SetActive(false);
			hand2.SetActive(false);
			hand3.SetActive(false);
			hand4.SetActive(false);
            killSlide.gameObject.SetActive(false);
            trySlide.SetActive(false);
            continueSound.Play();

            for(int i=0; i<3; i++)
            {
                warp[i].SetActive(true);
                finalSlide[i].SetActive(true);
                yield return new WaitForSeconds(0.4f);
            }

        }

        
    }

}
