using UnityEngine;
using System.Collections;
using PGT.Core;

namespace McHorseface.LawnDart
{
    public class DemoController : MonoBehaviour
    {
        [SerializeField] GameObject throwable;
            
        void Start()
        {
            EventRegistry.instance.AddEventListener(WiimoteController.WIIMOTE_CALIBRATED, OnWiimoteCalibrated);
        }

        void OnWiimoteCalibrated()
        {
            EventRegistry.instance.AddEventListener(WiimoteController.WIIMOTE_BUTTON_B_UP, () =>
            {
                var dup = Instantiate(throwable);
                var rb = dup.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.transform.rotation = transform.rotation;

                Vector3 gravity = rb.transform.InverseTransformVector(Vector3.down);

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
