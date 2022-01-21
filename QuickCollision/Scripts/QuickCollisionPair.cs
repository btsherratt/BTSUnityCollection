using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct QuickCollisionPair {
    public QuickCollider m_colliderA;
    public QuickCollider m_colliderB;

    public QuickCollisionPair(QuickCollider colliderA, QuickCollider colliderB) {
        m_colliderA = colliderA;
        m_colliderB = colliderB;
    }
}
