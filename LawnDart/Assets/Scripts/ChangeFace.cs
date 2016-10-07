using UnityEngine;
using System.Collections;

namespace McHorseface.LawnDart
{

    public class ChangeFace : StateMachineBehaviour
    {

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.gameObject.GetComponent<CalibrationMiiController>().enabled = false;
            animator.gameObject.GetComponent<CalibrationMiiController>().ChangeHeadTexture();
        }


        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.gameObject.GetComponent<CalibrationMiiController>().enabled = true;
            animator.gameObject.GetComponent<CalibrationMiiController>().ChangeHeadTextureBack();
        }

    }
}
