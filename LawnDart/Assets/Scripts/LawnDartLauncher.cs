using UnityEngine;
using UnityEngine.UI;
using UnityCoroutine = System.Collections.IEnumerator;
using PGT.Core;

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


        int launch_event_listener;
        void Start()
        {
            enabled = false;
            sprite.SetActive(false);

            innerRotation = Quaternion.Euler(new Vector3(0, -90f, 0));
            if (isTryout)
            {
                EventRegistry.instance.AddEventListener(LDCalibrator.CALIB_TRYOUT, StartListener);
            }
            else
            {
                StartListener();
            }

        }

        void StartListener()
        {
            enabled = true;
            sprite.SetActive(true);

            launch_event_listener = EventRegistry.instance.AddEventListener(LDController.BUTTON_OFF, LaunchDart, true);
        }

        void Update()
        {
            sprite.transform.rotation = LDController.instance.GetCalibratedRotation();
        }

        void OnDestroy()
        {
            EventRegistry.instance.RemoveEventListener(LDController.BUTTON_OFF, launch_event_listener);
        }

        void LaunchDart()
        {
            enabled = false;
            sprite.SetActive(false);

            var dup = Instantiate(dart);
            dup.transform.position = transform.position;
            dup.transform.rotation = LDController.instance.GetCalibratedRotation();
            var rb = dup.GetComponentInChildren<Rigidbody>();
            rb.position = transform.position;

            Vector3 gravity =  transform.InverseTransformVector(Vector3.down);

            rb.AddRelativeForce(scaleFactor * Vector3.Magnitude(LDController.instance.Accel - gravity) * Vector3.forward, ForceMode.VelocityChange);
            rb.AddTorque(transform.rotation.eulerAngles);

            // disable darts
            EventRegistry.instance.SetTimeout(1f, () =>
            {
                enabled = true;
                sprite.SetActive(true);
            });

            EventRegistry.instance.SetTimeout(10f, () =>
            {
                Destroy(dup);
            });
        }
        

    }

}
