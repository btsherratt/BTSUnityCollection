using UnityEngine;

public class AnimationEvent : StateMachineBehaviour {
    public string m_message;

    public bool m_sendOnEnter;
    public bool m_sendOnExit;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (m_sendOnEnter) {
            animator.SendMessage(m_message);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (m_sendOnExit) {
            animator.SendMessage(m_message);
        }
    }
}
