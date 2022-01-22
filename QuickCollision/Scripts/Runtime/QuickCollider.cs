using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuickCollider : TrackedMonoBehaviour<QuickCollider> {
    [QuickCollisionLayer]
    public int m_collisionMask;

    public Object[] m_metadata;

    public T GetMetadata<T>(int idx = 0) where T : class {
        T safeObj = (m_metadata != null && m_metadata.Length > idx) ? (m_metadata[idx] as T) : null;
        return safeObj;
    }

    public void OnEnable() {
        // We need to be able to disable this...
    }
}
