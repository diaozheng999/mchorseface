using UnityEngine;
using UnityEngine.UI;
using UnityCoroutine = System.Collections.IEnumerator;
using PGT.Core;

namespace McHorseface.LawnDart
{
    public class LDCalibrator : MonoBehaviour {

        [SerializeField]
        GameObject calibrationSlide;
        [SerializeField]
        GameObject confirmationSlide;
        [SerializeField]
        GameObject confirmationYes;
        [SerializeField]
        GameObject confirmationNo;

        AnimationController cfmYesAnim;
        AnimationController cfmNoAnim;

        enum ButtonState
        {
            None, Yes, No
        }

        ButtonState buttonState;

	    // Use this for initialization
	    void Start () {
            cfmYesAnim = confirmationYes.GetComponent<AnimationController>();
            cfmNoAnim = confirmationNo.GetComponent<AnimationController>();

            StartCoroutine(CalibrationStart());
	    }


        UnityCoroutine CalibrationStart()
        {

            calibrationSlide.SetActive(true);
            confirmationSlide.SetActive(false);
            confirmationYes.SetActive(false);
            confirmationNo.SetActive(false);

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
                confirmationSlide.SetActive(true);
                confirmationYes.gameObject.SetActive(true);
                confirmationNo.gameObject.SetActive(true);
            
                enabled = true;

                yield return new WaitForEvent(LDController.BUTTON_OFF);
            }
            if(buttonState == ButtonState.Yes)
            {
                gameObject.SetActive(false);
            }else
            {
                StartCoroutine(CalibrationStart());
            }

        }
	
	    // Update is called once per frame
	    void Update () {
            Vector3 rot = LDController.instance.GetCalibratedRotation() * Vector3.forward;
            
            if (rot.x < -0.3 && buttonState != ButtonState.No)
            {
                confirmationNo.transform.localScale = 0.007f * Vector3.one;
                confirmationYes.transform.localScale = 0.005f * Vector3.one;
                buttonState = ButtonState.No;

            }else if(rot.x > 0.3 && buttonState != ButtonState.Yes)
            {
                confirmationNo.transform.localScale = 0.005f * Vector3.one;
                confirmationYes.transform.localScale = 0.007f * Vector3.one;
                buttonState = ButtonState.Yes;
            }else if(rot.x < 0.3 && rot.x > -0.3 && buttonState != ButtonState.None)
            {
                confirmationNo.transform.localScale = 0.005f * Vector3.one;
                confirmationYes.transform.localScale = 0.005f * Vector3.one;
                buttonState = ButtonState.None;
            }
	    }
    }

}
