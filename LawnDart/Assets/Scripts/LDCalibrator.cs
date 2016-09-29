﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityCoroutine = System.Collections.IEnumerator;
using PGT.Core;

namespace McHorseface.LawnDart
{
    public class LDCalibrator : MonoBehaviour {

        public const string CALIB_TRYOUT = "calib_tryout";

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
        GameObject hand;
        [SerializeField]
        GameObject finalSlide;

        AnimationController cfmYesAnim;
        AnimationController cfmNoAnim;

        Vector3 cfmYesOrigin;
        Vector3 cfmNoOrigin;

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


        UnityCoroutine CalibrationStart()
        {

            calibrationSlide.SetActive(true);
            confirmationSlide.SetActive(false);
            confirmationYes.SetActive(false);
            confirmationNo.SetActive(false);
            trySlide.SetActive(false);
            finalSlide.SetActive(false);
            hand.SetActive(true);
            enabled = false;
            EventRegistry.instance.AddEventListener(LDController.BUTTON_OFF, () =>
            {
                LDController.instance.Calibrate();
            });

            yield return new WaitForEvent(LDController.BUTTON_OFF);
            buttonState = ButtonState.None;

            while(buttonState == ButtonState.None)
            {
                calibrationSlide.SetActive(false);
                hand.SetActive(false);
                confirmationSlide.SetActive(true);
                confirmationYes.gameObject.SetActive(true);
                confirmationNo.gameObject.SetActive(true);
            
                enabled = true;

                confirmationNo.transform.localScale = 0.005f * Vector3.one;
                confirmationNo.transform.position = cfmNoOrigin;

                confirmationYes.transform.localScale = 0.005f * Vector3.one;
                confirmationYes.transform.position = cfmYesOrigin;

                yield return new WaitForEvent(LDController.BUTTON_OFF);
            }
            if(buttonState == ButtonState.Yes)
            {
                trySlide.SetActive(true);
                confirmationSlide.SetActive(false);
                confirmationYes.SetActive(false);
                confirmationNo.SetActive(false);
                EventRegistry.instance.Invoke(CALIB_TRYOUT);

                // whenever a button_4_off is sent, a button_off is also sent
                yield return new WaitForEvent(LDController.BUTTON_4_OFF);
                yield return new WaitForEvent(LDController.BUTTON_OFF);

                trySlide.SetActive(false);
                finalSlide.SetActive(true);
                yield return new WaitForEvent(LDController.BUTTON_OFF);
                SceneManager.LoadScene(LDController.instance.nextScene);
            }
            else
            {
                StartCoroutine(CalibrationStart());
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
