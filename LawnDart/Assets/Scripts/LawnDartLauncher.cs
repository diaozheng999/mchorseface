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


        int launch_event_listener;
        void Start()
        {
            enabled = false;
            sprite.SetActive(false);

            EventRegistry.instance.AddEventListener(LDCalibrator.CALIB_TRYOUT, Tryout);

        }

        void Tryout()
        {
            enabled = true;
            sprite.SetActive(true);

            launch_event_listener = EventRegistry.instance.AddEventListener(LDController.BUTTON_OFF, LaunchDart);
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
            dup.transform.rotation = transform.rotation;
            var rb = dup.GetComponentInChildren<Rigidbody>();

            Vector3 gravity = transform.InverseTransformVector(Vector3.down);

            rb.AddRelativeForce(scaleFactor * (LDController.instance.Accel - gravity), ForceMode.Impulse);

            // disable darts
            EventRegistry.instance.SetTimeout(1f, () =>
            {
                enabled = true;
                sprite.SetActive(true);
            });
        }
        

    }

}
