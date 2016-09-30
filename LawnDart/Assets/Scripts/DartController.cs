using UnityEngine;
using System.Collections;


namespace McHorseface.LawnDart
{
    [RequireComponent(typeof(AudioSource))]
    public class DartController : MonoBehaviour {
        [SerializeField]
        Rigidbody rb;

        public bool isTryout = true;

        AudioSource hit;

        void Start()
        {
            hit = GetComponent<AudioSource>();
        }

        void OnTriggerEnter()
        {
            if (rb != null) rb.isKinematic = true;
            if (hit != null) hit.Play();
        }

    }

}
