/**
 * WiimoteController.cs
 * Simon Diao
 * 
 * Interfaces accelerometer and controls from Wiimote.
 * Partly drawn from https://github.com/Flafla2/Unity-Wiimote/blob/master/Assets/Scripts/WiimoteDemo.cs
 **/

using UnityEngine;
using UnityCoroutine = System.Collections.IEnumerator;
using WiimoteApi;
using PGT.Core;
using System.Runtime.InteropServices;

namespace McHorseface.LawnDart
{
    public class WiimoteController : MonoBehaviour
    {

        Wiimote wiimote;
        int wiimoteReturnCode;

        [SerializeField]
        float gyroFactor = 1;

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

        void Start()
        {
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
            init_model();

        }

        bool prevADown = false;
        bool prevBDown = false;
        bool calibrated = false;

        public Vector3 Accel = Vector3.zero;

        public Vector2 AccelRotation = Vector2.zero;

        // whether we are moving or not
        bool stable = false;

        public bool Stable { get { return stable; } }
        
        Quaternion rot = Quaternion.identity;

        public Quaternion Rot { get { return rot; } }


        [DllImport("WiimoteIntegration")]
        private static extern void update_model(float accel_x, float accel_y, float accel_z, float gyro_roll, float gyro_pitch, float gyro_yaw);
        [DllImport("WiimoteIntegration")]
        private static extern void dealloc();
        [DllImport("WiimoteIntegration")]
        private static extern float get_model_x();
        [DllImport("WiimoteIntegration")]
        private static extern float get_model_y();
        [DllImport("WiimoteIntegration")]
        private static extern float get_model_z();
        [DllImport("WiimoteIntegration")]
        private static extern void init_model();



        // Update is called once per frame
        void FixedUpdate()
        {
            Vector3 wmpOffset = Vector3.zero;

            wiimoteReturnCode = 1;
            for (int readings = 0; wiimoteReturnCode > 0; readings++)
            {
                wiimoteReturnCode = wiimote.ReadWiimoteData();
                if (wiimoteReturnCode > 0 && wiimote.current_ext == ExtensionController.MOTIONPLUS)
                {
                    Vector3 offset = new Vector3(wiimote.MotionPlus.PitchSpeed,
                                                  -wiimote.MotionPlus.YawSpeed,
                                                    wiimote.MotionPlus.RollSpeed); // Divide by 95Hz (average updates per second from wiimote)
                    if (readings == 0) wmpOffset = offset;
                    else wmpOffset = (wmpOffset / readings + offset) / (readings + 1f);
                }
            }

            wmpOffset /= 30f;

            if (wiimote.Button.a != prevADown)
            {
                prevADown = wiimote.Button.a;
                EventRegistry.instance.Invoke(prevADown ? WIIMOTE_BUTTON_A_DOWN : WIIMOTE_BUTTON_A_UP);
            }

            if (wiimote.Button.b != prevBDown)
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


            var norm = Accel.normalized;


            rot = Quaternion.Euler(wmpOffset) * rot;
           
            if (mag < 1.2 && mag > 0.8)
            {
                // stabalised
                var euler = rot.eulerAngles;
                var euler_accel = Quaternion.FromToRotation(Accel, Vector3.down);

                if (!stable)
                {
                    stable = true;
                    if(Accel.y < 0)
                    {
                        StartCoroutine(lerpRotation());
                    }
                    Debug.Log("Stabalised");
                }
            }
            else if (stable)
            {
                stable = false;
            }          
            
    
    
        }

        void OnDestroy()
        {
            dealloc();
        }

        
        UnityCoroutine lerpRotation()
        {
            Quaternion rot_prev;

            while (stable)
            {
                do
                {
                    rot_prev = rot;
                    rot = Quaternion.Lerp(rot, Quaternion.FromToRotation(Accel, Vector3.down), 5f * Time.deltaTime);
                    yield return new WaitForEndOfFrame();
                } while (Mathf.Abs(Quaternion.Angle(rot, rot_prev)) > 0.2f);
                yield return new WaitForSeconds(0.5f);
            }
        }

    }



}
