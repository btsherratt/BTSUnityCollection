using System.Collections.Generic;
using UnityEngine;

namespace SKFX.WorldBuilder {
    [ExecuteInEditMode]
    public class InstanceProvider : TrackedMonoBehaviour<InstanceProvider> {
        [System.Serializable]
        public struct TaggedInstancePrefabGroupConfiguration {
            [Tag]
            public string m_tag;
            public InstancePrefabGroupConfiguration m_instancePrefabGroupConfiguration;
        }

        public struct InstanceDetails : ITransformDetailsProviding {
            public InstancePrefabGroupConfiguration.InstancePrefabConfiguration prefabConfiguration;
            ITransformDetailsProviding transformDetailsProvider;
            float groundAngleMultiplier;
            float maxAngle;
            float objectRadius;
            int snapLayerMask;
            int seed;

            public InstanceDetails(InstancePrefabGroupConfiguration.InstancePrefabConfiguration prefabConfiguration, ITransformDetailsProviding transformDetailsProvider, float objectRadius, float groundAngleMultiplier, float maxAngle, int snapLayerMask, uint seed) {
                this.prefabConfiguration = prefabConfiguration;
                this.transformDetailsProvider = transformDetailsProvider;
                this.objectRadius = objectRadius;
                this.groundAngleMultiplier = groundAngleMultiplier;
                this.maxAngle = maxAngle;
                this.snapLayerMask = snapLayerMask;
                this.seed = (int)seed;
            }

            public long DetailsCount => transformDetailsProvider.DetailsCount;

            public long GenerateDetails(TransformDetails[] transformDetailsOut, long startIndex) {
                return GenerateDetails(transformDetailsOut, startIndex, objectRadius, groundAngleMultiplier, maxAngle, snapLayerMask);
            }

            public long GenerateDetails(TransformDetails[] transformDetailsOut, long startIndex, float objectRadius, float groundAngleMultiplier, float maxAngle, int snapLayerMask = 0) {
                long finalIndex = transformDetailsProvider.GenerateDetails(transformDetailsOut, startIndex, objectRadius, groundAngleMultiplier, maxAngle, snapLayerMask);

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

        public uint m_seed = (uint)System.DateTime.Now.Ticks;
        public TaggedInstancePrefabGroupConfiguration[] m_prefabGroupConfigurations;

        public bool m_useSnapLayer = true;

        [Layer]
        public int m_snapLayer;

        //public delegate void ChangeEvent(OldInstanceProvider instanceProvider);
        //public static event ChangeEvent ms_changeEvent;

        public IEnumerable<InstanceDetails> GenerateInstanceDetails() {
            InstanceArea[] areas = GetComponentsInChildren<InstanceArea>();

            List<InstanceArea> additiveAreas = new List<InstanceArea>();
            List<InstanceArea> subtractiveAreas = new List<InstanceArea>();

            foreach (TaggedInstancePrefabGroupConfiguration configuration in m_prefabGroupConfigurations) {
                additiveAreas.Clear();
                subtractiveAreas.Clear();

                foreach (InstanceArea area in areas) {
                    bool isAdditive = false;
                    bool isSubtractive = false;

                    switch (area.m_operation) {
                        case InstanceArea.Operation.Additive:
                            isAdditive = area.CompareTag(configuration.m_tag);
                            break;

                        case InstanceArea.Operation.Subtractive:
                            isSubtractive = true;
                            break;

                        case InstanceArea.Operation.Cutout:
                            isAdditive = area.CompareTag(configuration.m_tag);
                            isSubtractive = isAdditive == false;
                            break;
                    }

                    if (isAdditive) {
                        additiveAreas.Add(area);
                    }

                    if (isSubtractive) {
                        subtractiveAreas.Add(area);
                    }
                }

                int snapLayerMask = m_useSnapLayer ? 1 << m_snapLayer : 0;

                uint seed = m_seed;
                foreach (InstancePrefabGroupConfiguration.InstancePrefabConfiguration prefabConfiguration in configuration.m_instancePrefabGroupConfiguration.m_groupInstanceConfigurations) {
                    ITransformDetailsProviding transformDetailsProvider = InstanceArea.TransformDetailsProvider(additiveAreas, subtractiveAreas, prefabConfiguration.m_density, seed, prefabConfiguration.m_instancesPerUnit);
                    float radius = prefabConfiguration.m_prefab.CalculateBounds().extents.XZ().magnitude; // FIXME??
                    InstanceDetails details = new InstanceDetails(prefabConfiguration, transformDetailsProvider, radius, prefabConfiguration.m_groundAngleMultiplier, prefabConfiguration.m_maxAngle, snapLayerMask, seed);
                    yield return details;
                }
            }
        }



            /*foreach (InstanceArea area in areas) {
                if (area.m_operation == InstanceArea.Operation.Subtractive || (area.m_operation == InstanceArea.Operation.Additive && area.GetComponentInParent<OldInstanceProvider>() == this)) {
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
        }*/
    }
}
