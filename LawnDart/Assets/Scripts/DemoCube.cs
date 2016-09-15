using UnityEngine;
using System.Collections;
using PGT.Core;

namespace McHorseface.LawnDart
{
    public class DemoCube : MonoBehaviour
    {
        Rigidbody rb;
        Transform tf;

        bool wiimoteCalibrated = false;
        // Use this for initialization
        void Start()
        {
            

            enabled = false;
            rb = GetComponent<Rigidbody>();

            tf = transform;

            EventRegistry.instance.AddEventListener(WiimoteController.WIIMOTE_CALIBRATED, OnWiimoteCalibrated);
        }

        void OnWiimoteCalibrated()
        {
            EventRegistry.instance.AddEventListener(WiimoteController.WIIMOTE_BUTTON_B_UP, () =>
            {
                var dup = GameObject.Instantiate<GameObject>(gameObject);
                dup.transform.position = tf.position;
                dup.transform.rotation = tf.rotation;

                if(wiimoteCalibrated)
                    dup.GetComponent<DemoCube>().OnWiimoteCalibrated();

                //push this off
                this.rb.isKinematic = false;
                this.rb.AddForce(WiimoteController.instance.Accel);
            });
            wiimoteCalibrated = true;
        }
        
    }
}
