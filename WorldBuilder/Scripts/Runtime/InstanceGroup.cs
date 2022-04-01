using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceGroup : ScriptableObject {
    [System.Serializable]
    public struct InstanceGroupPrefab {
        public GameObject m_prefab;

        [Range(0, 1)]
        public float m_density;

        [MinMax(0.01f, 5.0f)]
        public Vector2 m_scale;

        public AnimationCurve m_scaleCurve;

        public InstanceGroupPrefab(float abs)  {
            m_prefab = null;
            m_density = 0.5f;
            m_scale = Vector2.one;
            m_scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
    }

    public InstanceGroupPrefab[] m_groupPrefabs;
}
