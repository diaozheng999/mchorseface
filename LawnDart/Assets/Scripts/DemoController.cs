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
                rb.AddRelativeForce(WiimoteController.instance.Accel, ForceMode.Impulse);
                rb.useGravity = false;
                Debug.Log(WiimoteController.instance.Accel);
            }, true);
        }

        void Update()
        {
            if (WiimoteController.instance.Stable)
            {
                transform.rotation = Quaternion.FromToRotation(WiimoteController.instance.Accel, Vector3.down);
            }
        }

    }
}
