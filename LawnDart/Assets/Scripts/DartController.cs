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
        

		public bool targetTriggerEnter = false;
		public bool dartTriggerEnter = false;



        AudioSource hit;

        void Start()
        {
            hit = GetComponent<AudioSource>();
        }

        void OnTriggerEnter(Collider other)
        {
			Debug.Log ("DartController: Collided!");
			Debug.Log ("DartController: dartTriggerEnter:" + dartTriggerEnter + ", targetTriggerEnter:" + targetTriggerEnter);
            if (hit != null) hit.Play();

			if (other.CompareTag (MII)) {
                var mii = other.GetComponentInChildren<MiiAnimationController>();
                
                mii.Fragment (transform.position);
                
                if(mii.doBlood)
				    Instantiate (blood, other.transform.position + 1f * Vector3.up + 0.3f * (transform.position - Camera.main.transform.forward).normalized, Quaternion.identity, null);

				Destroy (rb.gameObject);
                
			} else if (other.CompareTag ("Player")) {
				Destroy (rb.gameObject);
			}
            else if (rb != null)
            {
				rb.isKinematic = true;
                if (other.CompareTag("WarpTarget"))
                {
					dartTriggerEnter = true;
                }
            }
            
        }

    }

}
