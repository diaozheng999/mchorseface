using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityCoroutine = System.Collections.IEnumerator;
using PGT.Core;
using PGT.Core.Func;
using System.Collections.Generic;

namespace McHorseface.LawnDart
{
    public class LawnDartLauncher : MonoBehaviour
    {
        [SerializeField]
        GameObject sprite;
        [SerializeField]
        GameObject dart;
        [SerializeField]
        float scaleFactor;
        [SerializeField]
        Quaternion innerRotation;
        [SerializeField]
        bool isTryout;

        float length = 0.1f, aerodynamicFactor = 1000f;

        List<Tuple<string, int>> eventListeners;


        int launch_event_listener;
        void Start()
        {
            enabled = false;
            sprite.SetActive(false);

            eventListeners = new List<Tuple<string, int>>();

            innerRotation = Quaternion.Euler(new Vector3(0, -90f, 0));
            if (isTryout)
            {
                eventListeners.Add(new Tuple<string, int>(
                    LDCalibrator.CALIB_TRYOUT,
                    EventRegistry.instance.AddEventListener(LDCalibrator.CALIB_TRYOUT, StartListener)));
                
            }
            else
            {
                int evt = EventRegistry.instance.AddEventListener(LDController.BUTTON_4_ON, () =>
                {
                    LDController.instance.nextScene = SceneManager.GetActiveScene().name;
                    SceneManager.LoadScene("Calibration");
                });

                eventListeners.Add(new Tuple<string, int>(LDController.BUTTON_4_ON, evt));
                StartListener();
            }

        }

        void StartListener()
        {
            enabled = true;
            sprite.SetActive(true);

            launch_event_listener = EventRegistry.instance.AddEventListener("FIRE", LaunchDart, true);
            eventListeners.Add(new Tuple<string, int>("FIRE", launch_event_listener));
        }

        void Update()
        {
            sprite.transform.rotation = LDController.instance.GetCalibratedRotation();
        }

        void OnDestroy()
        {
            foreach(var listener in eventListeners)
            {
                EventRegistry.instance.RemoveEventListener(listener.car, listener.cdr);
            }
        }

        bool isLaunching = false;
        void LaunchDart(object p)
        {
            StartCoroutine(_LaunchDart(p));
        }

        UnityCoroutine _LaunchDart(object packet)
        {
            var Packet = (Tuple<Vector3, Quaternion>)packet;
            enabled = false;
            sprite.SetActive(false);

            LDController.instance.SetOrientationForce(Packet.cdr);

            yield return new WaitForEvent("QSet");


            var dup = (GameObject)Instantiate(dart, transform.position, LDController.instance.GetCalibratedRotation(), null);
            
            var rb = dup.GetComponent<Rigidbody>();
            rb.position = transform.position;

            Debug.Log(transform.position);

            Vector3 gravity =  transform.InverseTransformVector(Vector3.down);

            rb.AddRelativeForce(scaleFactor * Vector3.Magnitude(Packet.car - gravity) * Vector3.forward, ForceMode.VelocityChange);
            rb.AddTorque(transform.rotation.eulerAngles);

            // disable darts
            eventListeners.Add(EventRegistry.instance.SetTimeout(1f, () =>
            {
                enabled = true;
                sprite.SetActive(true);
            }));

            eventListeners.Add(EventRegistry.instance.SetTimeout(10f, () =>
            {
                Destroy(dup);
            }));
        }
        

    }

}
