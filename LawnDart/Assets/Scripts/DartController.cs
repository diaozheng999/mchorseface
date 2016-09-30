using UnityEngine;
using System.Collections;


namespace McHorseface.LawnDart
{
    
    [RequireComponent(typeof(AudioSource))]
    public class DartController : MonoBehaviour {
        [SerializeField]
        Rigidbody rb;

        [SerializeField]
        private GameObject blood;

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

        void MakeBlood()
        {
            GameObject newBlood = Instantiate(blood);
            newBlood.transform.parent = gameObject.transform;
            newBlood.transform.localPosition = new Vector3(0, 0, 0);
        }

    }



}
