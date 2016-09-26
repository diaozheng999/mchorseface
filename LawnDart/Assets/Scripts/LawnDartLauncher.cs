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


        int launch_event_listener;
        void Start()
        {
            enabled = false;
            sprite.SetActive(false);

            innerRotation = Quaternion.Euler(new Vector3(0, -90f, 0));
            EventRegistry.instance.AddEventListener(LDCalibrator.CALIB_TRYOUT, Tryout);

        }

        void Tryout()
        {
            enabled = true;
            sprite.SetActive(true);

            launch_event_listener = EventRegistry.instance.AddEventListener(LDController.BUTTON_OFF, LaunchDart, true);
        }

        void Update()
        {
            sprite.transform.rotation = LDController.instance.GetCalibratedRotation();
        }

        void LaunchDart()
        {
            enabled = false;
            sprite.SetActive(false);

            var dup = Instantiate(dart);
            dup.transform.position = transform.position;
            dup.transform.rotation = LDController.instance.GetCalibratedRotation();
            var rb = dup.GetComponent<Rigidbody>();
            rb.position = transform.position;
            rb.rotation = LDController.instance.GetCalibratedRotation();

            Vector3 gravity =  transform.InverseTransformVector(Vector3.down);

            rb.AddRelativeForce(scaleFactor * Vector3.Magnitude(LDController.instance.Accel - gravity) * Vector3.forward, ForceMode.VelocityChange);
            //rb.AddTorque(transform.rotation.eulerAngles);

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
