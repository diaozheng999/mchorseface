using UnityEngine;
using System.Collections;
using PGT.Core;


namespace McHorseface.LawnDart
{
    [RequireComponent(typeof(AudioSource))]
    public class DartController : MonoBehaviour {
        [SerializeField]
        Rigidbody rb;

        const string WARP_TARGET = "WarpTarget";
        const string MII = "Mii";

        public bool isTryout = true;

        AudioSource hit;

        void Start()
        {
            hit = GetComponent<AudioSource>();
        }

        void OnTriggerEnter(Collider other)
        {

            if (hit != null) hit.Play();

            if (other.CompareTag(MII))
            {
                other.GetComponentInChildren<MiiAnimationController>().Fragment(transform.position);
            }else
            {
                if (rb != null) rb.isKinematic = true;
            }
            
        }

    }

}
