using UnityEngine;
using PGT.Core;
using UnityCoroutine = System.Collections.IEnumerator;

namespace McHorseface.LawnDart
{
    public class Hint : MonoBehaviour
    {
        [SerializeField]
        Animator anim;

        [SerializeField]
        AnimationController anim2;

        [SerializeField]
        float timeout = 5f;

        [SerializeField]
        float velocity = 5f;

        UnityCoroutine Start()
        {
            yield return new WaitForSeconds(timeout);
            anim.SetTrigger("hide");
            yield return new WaitForSeconds(1f);
            var dest = anim2.transform.position + 48 *  Vector3.forward;

            anim2.RunAnimation(
                anim2.Linear(
                    () => anim2.transform.position,
                    (Vector3 pos) => anim2.transform.position = pos,
                    Vector3.Lerp,
                    dest,
                    velocity
                )
            );
        }
    }
}
