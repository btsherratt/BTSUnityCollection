using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SKFX.WorldBuilder {
    [CreateAssetMenu(fileName="PrefabGroupConfig", menuName="BTS/WorldBuilder/InstancePrefabGroupConfiguration", order=1)]
    public class InstancePrefabGroupConfiguration : ScriptableObject {
        [System.Serializable]
        public class InstancePrefabConfiguration {
            public GameObject m_prefab;

            [Range(0, 1)]
            public float m_density = 0.5f;

            [MinMax(0.01f, 5.0f)]
            public Vector2 m_scale = Vector2.one;

            public bool m_randomXRotation = false;
            public bool m_randomYRotation = true;
            public bool m_randomZRotation = false;

            public AnimationCurve m_scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            public float m_maxAngle = 45.0f;
            public float m_groundAngleMultiplier = 1.0f;

            [Header("Advanced Settings")]
            public int m_instancesPerUnit = 10;
        }

//        public string[] m_groupTags;
        public InstancePrefabConfiguration[] m_groupInstanceConfigurations;
    }
}
