using UnityEngine;
using System.Collections;
using PGT.Core;
using System.Runtime.InteropServices;

namespace McHorseface.LawnDart
{
    public class DemoController : MonoBehaviour
    {
        [SerializeField] GameObject throwable;
		[SerializeField] float scaleFactor;
        [SerializeField] float frequency;

        void Start()
        {
            EventRegistry.instance.AddEventListener(WiimoteController.WIIMOTE_CALIBRATED, OnWiimoteCalibrated);
        }

        void OnWiimoteCalibrated()
        {
            float force = 0f, time = 0f;
            EventRegistry.instance.AddEventListener(WiimoteController.WIIMOTE_BUTTON_B_DOWN, () =>
            {
                force = 0;
                time = 0;
            }, true);

            EventRegistry.instance.AddEventListener(WiimoteController.WIIMOTE_BUTTON_B, () =>
            {
                time += Time.deltaTime;
                force = scaleFactor - scaleFactor * Mathf.Cos(time * frequency);
                Debug.Log(force);
            }, true);

            EventRegistry.instance.AddEventListener(WiimoteController.WIIMOTE_BUTTON_B_UP, () =>
            {
                Debug.Log("Hello World!");
                var dup = Instantiate(throwable);
                var rb = dup.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.transform.rotation = transform.rotation;
				rb.transform.position = transform.position;

                Vector3 gravity = rb.transform.InverseTransformVector(Vector3.down);

                //rb.AddRelativeForce(force * rb.transform.forward, ForceMode.Impulse);

                rb.AddRelativeForce(WiimoteController.instance.Accel - gravity, ForceMode.Impulse);
                Debug.Log(WiimoteController.instance.Accel);

                EventRegistry.instance.SetTimeout(20f, () =>
                {
                    Destroy(dup);
                });
            }, true);
        }
        

        void Update()
        {
            //transform.rotation = Quaternion.FromToRotation(WiimoteController.instance.Accel, Vector3.down);
             transform.rotation = WiimoteController.instance.Rot;
        }

    }
}
