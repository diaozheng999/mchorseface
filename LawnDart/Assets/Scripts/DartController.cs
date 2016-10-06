using UnityEngine;
using System.Collections;
using PGT.Core;


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
            EventRegistry.instance.SetTimeout(1f, () =>
            {
                StartCoroutine(Player.instance.teleport(transform.position));
            });
        }

    }

}
