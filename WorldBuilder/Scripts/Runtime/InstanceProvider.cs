using System.Collections.Generic;
using UnityEngine;

namespace SKFX.WorldBuilder {
    [ExecuteInEditMode]
    public class InstanceProvider : TrackedMonoBehaviour<InstanceProvider> {
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

        public struct InstanceDetails : ITransformDetailsProviding {
            public InstancePrefabConfiguration prefabConfiguration;
            ITransformDetailsProviding transformDetailsProvider;
            float groundAngleMultiplier;
            float maxAngle;
            int snapLayerMask;
            int seed;

            public InstanceDetails(InstancePrefabConfiguration prefabConfiguration, ITransformDetailsProviding transformDetailsProvider, float groundAngleMultiplier, float maxAngle, int snapLayerMask, uint seed) {
                this.prefabConfiguration = prefabConfiguration;
                this.transformDetailsProvider = transformDetailsProvider;
                this.groundAngleMultiplier = groundAngleMultiplier;
                this.maxAngle = maxAngle;
                this.snapLayerMask = snapLayerMask;
                this.seed = (int)seed;
            }

            public long DetailsCount => transformDetailsProvider.DetailsCount;

            public long GenerateDetails(TransformDetails[] transformDetailsOut, long startIndex) {
                return GenerateDetails(transformDetailsOut, startIndex, groundAngleMultiplier, maxAngle, snapLayerMask);
            }

            public long GenerateDetails(TransformDetails[] transformDetailsOut, long startIndex, float groundAngleMultiplier, float maxAngle, int snapLayerMask = 0) {
                long finalIndex = transformDetailsProvider.GenerateDetails(transformDetailsOut, startIndex, groundAngleMultiplier, maxAngle, snapLayerMask);

                var randomState = Random.state;
                Random.InitState(seed);

                for (long i = startIndex; i < finalIndex; ++i) {
                    ref TransformDetails transformDetails = ref transformDetailsOut[i];

                    //if () {
                        float rotationX = prefabConfiguration.m_randomXRotation ? Random.Range(0.0f, 360.0f) : 0;
                        float rotationY = prefabConfiguration.m_randomYRotation ? Random.Range(0.0f, 360.0f) : 0;
                        float rotationZ = prefabConfiguration.m_randomZRotation ? Random.Range(0.0f, 360.0f) : 0;
                        transformDetails.rotation = transformDetails.rotation * Quaternion.Euler(rotationX, rotationY, rotationZ); // fixme, seed
                        transformDetails.uniformScale = Mathf.Lerp(prefabConfiguration.m_scale.x, prefabConfiguration.m_scale.y, prefabConfiguration.m_scaleCurve.Evaluate(Random.value));
                    //}
                }

                Random.state = randomState;

                return finalIndex;
            }
        }

        public uint m_seed;

        public List<InstancePrefabConfiguration> m_prefabConfigurations;

        public bool m_useSnapLayer = true;

        [Layer]
        public int m_snapLayer;

        public delegate void ChangeEvent(InstanceProvider instanceProvider);
        public static event ChangeEvent ms_changeEvent;

        public IEnumerable<InstanceDetails> GenerateInstanceDetails() {
            InstanceArea[] areas = GetComponentsInChildren<InstanceArea>();

            List<InstanceArea> filteredAreas = new List<InstanceArea>();
            foreach (InstanceArea area in areas) {
                if (area.m_operation == InstanceArea.Operation.Subtractive || (area.m_operation == InstanceArea.Operation.Additive && area.GetComponentInParent<InstanceProvider>() == this)) {
                    filteredAreas.Add(area);
                }
            }
            areas = filteredAreas.ToArray();

            uint seed = m_seed;
            foreach (InstancePrefabConfiguration configuration in m_prefabConfigurations) {
                ITransformDetailsProviding transformDetailsProvider = InstanceArea.TransformDetailsProvider(areas, configuration.m_density, seed, configuration.m_instancesPerUnit);
                int snapLayerMask = m_useSnapLayer ? 1 << m_snapLayer : 0;
                InstanceDetails details = new InstanceDetails(configuration, transformDetailsProvider, configuration.m_groundAngleMultiplier, configuration.m_maxAngle, snapLayerMask, seed);
                yield return details;
                ++seed;
            }
        }

        private void OnValidate() {
            if (m_seed == 0) {
                m_seed = (uint)System.DateTime.Now.Ticks;
            }
            if (ms_changeEvent != null) {
                ms_changeEvent(this);
            }
        }
    }
}
