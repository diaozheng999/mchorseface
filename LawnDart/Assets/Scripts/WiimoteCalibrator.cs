/**
 * WiimoteCalibrator.cs
 * Simon Diao
 * 
 * Calibrates the Wiimote by default
 * Partly drawn from https://github.com/Flafla2/Unity-Wiimote/blob/master/Assets/Scripts/WiimoteDemo.cs
 **/

using UnityEngine;
using UnityCoroutine = System.Collections.IEnumerator;
using WiimoteApi;
using PGT.Core;

namespace McHorseface.LawnDart
{
    public class WiimoteCalibrator : MonoBehaviour
    {
        const string NEXT = "calib_next";


        Wiimote wiimote;
        [SerializeField] 
        float pingDuration = 1;

        [SerializeField]
        InputDataType wiimoteDataReportMode = InputDataType.REPORT_BUTTONS_ACCEL_EXT16;

        [SerializeField]
        GameObject[] calibrationSteps;

        void Start()
        {
            EventRegistry.instance.AddEventListener(WiimoteController.WIIMOTE_DETECTED, OnWiimoteDetected);
            StartCoroutine(Pingback());
            for(int i=0; i<3; i++)
            {
                calibrationSteps[i].SetActive(false);
            }
        }

        void OnWiimoteDetected()
        {
            // assume that there's 1 wiimote
            wiimote = WiimoteManager.Wiimotes[0];

            // set data reporting mode
            wiimote.SendDataReportMode(wiimoteDataReportMode);
            wiimote.ActivateWiiMotionPlus();

            // set the propagation mode
            StartCoroutine(CycleLED());

            // set initialised mode
            EventRegistry.instance.Invoke(WiimoteController.WIIMOTE_INITIALISED);

            StartCoroutine(CalibrationSequence(0));
            
        }
        
        UnityCoroutine CalibrationSequence(int wiimote_to_calibrate)
        {
            wiimote = WiimoteManager.Wiimotes[wiimote_to_calibrate];
            for(int i=0; i<3; i++)
            {
                // set the correct slide
                for(int j=0; j<3; j++)
                {
                    calibrationSteps[j].SetActive(i == j);
                }
                // wait for the button-press
                yield return new WaitForEvent(NEXT);

                // set calibration point
                wiimote.Accel.CalibrateAccel((AccelCalibrationStep)i);
            }


            // disable all slides
            for (int j = 0; j < 3; j++)
            {
                calibrationSteps[j].SetActive(false);
            }
            EventRegistry.instance.Invoke(WiimoteController.WIIMOTE_CALIBRATED, wiimote_to_calibrate);
            gameObject.SetActive(false);
        }

        UnityCoroutine Pingback()
        {
            while (!WiimoteManager.HasWiimote())
            {
                WiimoteManager.FindWiimotes();
                yield return new WaitForSeconds(pingDuration);
            }
            EventRegistry.instance.Invoke(WiimoteController.WIIMOTE_DETECTED);
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                EventRegistry.instance.Invoke(NEXT);
            }
        }


        UnityCoroutine CycleLED()
        {
            for(byte i=0; ; i++)
            {
                wiimote.SendPlayerLED((i & 8) == 8, (i & 4) == 4, (i & 2) == 2, (i & 1) == 1);
                yield return new WaitForSeconds(1);
            }
        }
    }

}
