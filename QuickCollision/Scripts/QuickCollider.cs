using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuickCollider : TrackedMonoBehaviour<QuickCollider> {
    public int m_collisionMask;

    public Component[] m_metadataComponents;
}
