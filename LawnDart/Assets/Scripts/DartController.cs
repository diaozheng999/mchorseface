using UnityEngine;
using System.Collections;


namespace McHorseface.LawnDart
{
    public class DartController : MonoBehaviour {
        [SerializeField]
        Rigidbody rb;


        void OnTriggerEnter()
        {
            if (rb != null) rb.isKinematic = true;
        }

    }

}
