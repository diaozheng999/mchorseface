using UnityEngine;
using System.Collections;
using PGT.Core;

namespace McHorseface.LawnDart
{
    public class DemoCube : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            EventRegistry.instance.AddEventListener(WiimoteController.WIIMOTE_BUTTON_A_UP, () =>
            {
                transform.position = Vector3.zero;
            }, true);

            enabled = false;

            EventRegistry.instance.AddEventListener(WiimoteController.WIIMOTE_CALIBRATED, () => enabled = true);
        }

        // Update is called once per frame
        void Update()
        {
            transform.position =  2f * WiimoteController.instance.Accel;
        }
    }
}
