using UnityEngine;
using System.Collections;
using PGT.Core;


namespace McHorseface.LawnDart
{
    [RequireComponent(typeof(AudioSource))]
    public class DartController : MonoBehaviour {
        [SerializeField]
        Rigidbody rb;

        [SerializeField]
        GameObject blood;

        const string WARP_TARGET = "WarpTarget";
        const string MII = "Mii";

        public bool isTryout = true;
        public bool hitGround = false;

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
                
                Instantiate(blood, other.transform.position + 1f * Vector3.up + 0.3f *(transform.position - Camera.main.transform.forward).normalized, Quaternion.identity, null);

                Destroy(rb.gameObject);
                
            }
            else
            {
                if (rb != null)
                {
                    rb.isKinematic = true;
                    hitGround = true;
                }
            }
            
        }

    }

}
