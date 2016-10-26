using UnityEngine;

namespace McHorseface.LawnDart
{
    public class PhaseBoard : MonoBehaviour
    {
        [SerializeField]
        Animator anim;

        void OnTriggerEnter(Collider other)
        {
            if(other.GetComponent<DartController>()!=null)
                anim.SetTrigger("phase");
        }

    }
}