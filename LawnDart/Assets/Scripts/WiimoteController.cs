/**
 * WiimoteController.cs
 * Simon Diao
 * 
 * Interfaces accelerometer and controls from Wiimote.
 * Partly drawn from https://github.com/Flafla2/Unity-Wiimote/blob/master/Assets/Scripts/WiimoteDemo.cs
 **/

using UnityEngine;
using System.Collections;
using WiimoteApi;
using PGT.Core;

namespace McHorseface.LawnDart
{
    public class WiimoteController : MonoBehaviour {
        
        Wiimote wiimote;
        int wiimoteReturnCode;

        // singleton
        public static WiimoteController instance;

        public const string WIIMOTE_CALIBRATED = "wiimote_calibrated";
        public const string WIIMOTE_INITIALISED = "wiimote_initialised";
        public const string WIIMOTE_DETECTED = "wiimote_detected";
        public const string WIIMOTE_BUTTON_A_DOWN = "wiimote_a_down";
        public const string WIIMOTE_BUTTON_A_UP = "wiimote_a_up";
        public const string WIIMOTE_BUTTON_A = "wiimote_a";
        public const string WIIMOTE_BUTTON_B_DOWN = "wiimote_b_down";
        public const string WIIMOTE_BUTTON_B_UP = "wiimote_b_up";
        public const string WIIMOTE_BUTTON_B = "wiimote_b";

        void Start () {
            enabled = false;
            EventRegistry.instance.AddEventListener(WIIMOTE_INITIALISED, () =>
            {
                wiimote = WiimoteManager.Wiimotes[0];
                enabled = true;
            });
            EventRegistry.instance.AddEventListener(WIIMOTE_CALIBRATED, () =>
            {
                calibrated = true;
                Debug.Log("Calibrated!");
            });

            instance = this;
	    }

        bool prevADown = false;
        bool prevBDown = false;
        bool calibrated = false;

        public Vector3 Accel = Vector3.zero;

        // whether we are moving or not
        bool stable = false;

        public bool Stable { get { return stable; } }

        Quaternion rot = Quaternion.identity;

        public Quaternion Rot { get { return rot; } }

	    // Update is called once per frame
	    void Update () {
            Vector3 wmpOffset = Vector3.zero;
            do
            {
                wiimoteReturnCode = wiimote.ReadWiimoteData();
                if(wiimoteReturnCode > 0 && wiimote.current_ext == ExtensionController.MOTIONPLUS) {
                    Vector3 offset = new Vector3(   wiimote.MotionPlus.PitchSpeed,
                                                  - wiimote.MotionPlus.YawSpeed,
                                                    wiimote.MotionPlus.RollSpeed) / 95f; // Divide by 95Hz (average updates per second from wiimote)
                    wmpOffset += offset;
                    
                }
            } while (wiimoteReturnCode > 0);
            rot = Quaternion.Euler(wmpOffset) * rot;

            if (wiimote.Button.a != prevADown)
            {
                prevADown = wiimote.Button.a;
                EventRegistry.instance.Invoke(prevADown ? WIIMOTE_BUTTON_A_DOWN : WIIMOTE_BUTTON_A_UP);
            }

            if(wiimote.Button.b != prevBDown)
            {
                prevBDown = wiimote.Button.b;
                EventRegistry.instance.Invoke(prevBDown ? WIIMOTE_BUTTON_B_DOWN : WIIMOTE_BUTTON_B_UP);
            }

            if (prevADown)
            {
                EventRegistry.instance.Invoke(WIIMOTE_BUTTON_A);
            }

            if (prevBDown)
            {
                EventRegistry.instance.Invoke(WIIMOTE_BUTTON_B);
            }

            var accel = wiimote.Accel.GetCalibratedAccelData();

            // a bit of smoothing
            var _accel = Vector3.zero;

            _accel.x = accel[0];
            _accel.y = -accel[2];
            _accel.z = accel[1];

            Accel = (Accel + _accel) / 2;

            var mag = Accel.sqrMagnitude;

            if(mag < 1.2 && mag > 0.8)
            {
                // stabalised
                rot = Quaternion.Slerp(rot, Quaternion.FromToRotation(Accel, Vector3.down), 5f*Time.deltaTime);
                if (!stable)
                {
                    stable = true;
                }
            }else if (stable)
            {
                stable = false;
            }

            
	    }
    }

}
