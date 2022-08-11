//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.AI;

public class AnimationLerp : StateMachineBehaviour {
    public float m_startWeight;
    public float m_endWeight;
    public AnimationCurve m_weightCurve;

    [Layer]
    public int m_layer;

    public Vector3 m_direction = Vector3.down;

    //public string m_offsetPropertyName;

    //Vector3 m_target;
    Vector3 m_uncorrectedRootPosition;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //m_target.y = animator.GetFloat(m_offsetPropertyName);
        m_uncorrectedRootPosition = animator.rootPosition;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        m_uncorrectedRootPosition += animator.deltaPosition;
        Vector3 worldPosition = m_uncorrectedRootPosition;// animator.transform.TransformPoint(m_uncorrectedRootPosition);

        RaycastHit hit;
        if (Physics.Raycast(worldPosition - m_direction * 100.0f, m_direction, out hit, float.PositiveInfinity, 1 << m_layer)) {
            float t = m_weightCurve.Evaluate(stateInfo.normalizedTime);
            float weight = Mathf.Lerp(m_startWeight, m_endWeight, t);
            Vector3 targetPosition = worldPosition;// hit.point;
            targetPosition.y = hit.point.y;
            Vector3 position = Vector3.Lerp(worldPosition, targetPosition, weight);
            animator.transform.position = position;
        } else {
            animator.transform.position = worldPosition;
        }

        //NavMeshHit meshHit;
        //if (NavMesh.SamplePosition(worldPosition, out meshHit, 8.0f, NavMesh.AllAreas)) {
        //    
        //}


        //Vector3 targetPosition = worldPosition;
        //targetPosition.y = m_target.y;
        //Vector3 position = Vector3.Lerp(worldPosition, targetPosition, weight);
        //animator.transform.position = position;
        
        //targetPosition.y += weight;
        //animator.transform.localPosition = targetPosition;

        //Debug.Log(stateInfo.normalizedTime);
        //Debug.Log(t);
        //Debug.Log(weight);
        //Debug.Log(animator.rootPosition);
        //Debug.Log(targetPosition);
       // Debug.Log(position);
        //Debug.Log("---");

        //animator.GetVector()
        //   stateInfo.normalizedTime;

        //    // Implement code that processes and affects root motion
    }

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
