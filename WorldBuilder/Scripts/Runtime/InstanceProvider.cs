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

            [Header("Advanced Settings")]
            public int m_instancesPerUnit = 10;
        }

        public struct InstanceDetails : ITransformDetailsProviding {
            public InstancePrefabConfiguration prefabConfiguration;
            ITransformDetailsProviding transformDetailsProvider;
            int snapLayerMask;

            public InstanceDetails(InstancePrefabConfiguration prefabConfiguration, ITransformDetailsProviding transformDetailsProvider, int snapLayerMask) {
                this.prefabConfiguration = prefabConfiguration;
                this.transformDetailsProvider = transformDetailsProvider;
                this.snapLayerMask = snapLayerMask;
            }

            public long DetailsCount => transformDetailsProvider.DetailsCount;

            public IEnumerable<TransformDetails> GenerateDetails() {
                foreach (TransformDetails td in transformDetailsProvider.GenerateDetails()) {
                    TransformDetails transformDetails = new TransformDetails();
                    transformDetails.position = td.position;

                    float rotationX = prefabConfiguration.m_randomXRotation ? Random.Range(0.0f, 360.0f) : 0;
                    float rotationY = prefabConfiguration.m_randomYRotation ? Random.Range(0.0f, 360.0f) : 0;
                    float rotationZ = prefabConfiguration.m_randomZRotation ? Random.Range(0.0f, 360.0f) : 0;
                    transformDetails.rotation = td.rotation * Quaternion.Euler(rotationX, rotationY, rotationZ); // fixme, seed

                    transformDetails.uniformScale = Mathf.Lerp(prefabConfiguration.m_scale.x, prefabConfiguration.m_scale.y, prefabConfiguration.m_scaleCurve.Evaluate(Random.value));
                    yield return transformDetails;
                }
            }

            public IEnumerable<TransformDetails> GenerateSnappedDetails() {
                return GenerateSnappedDetails(snapLayerMask);
            }

            public IEnumerable<TransformDetails> GenerateSnappedDetails(int snapLayerMask) {
                foreach (TransformDetails td in transformDetailsProvider.GenerateSnappedDetails(snapLayerMask)) {
                    TransformDetails transformDetails = new TransformDetails();
                    transformDetails.position = td.position;

                    float rotationX = prefabConfiguration.m_randomXRotation ? Random.Range(0.0f, 360.0f) : 0;
                    float rotationY = prefabConfiguration.m_randomYRotation ? Random.Range(0.0f, 360.0f) : 0;
                    float rotationZ = prefabConfiguration.m_randomZRotation ? Random.Range(0.0f, 360.0f) : 0;
                    transformDetails.rotation = td.rotation * Quaternion.Euler(rotationX, rotationY, rotationZ); // fixme, seed

                    transformDetails.uniformScale = Mathf.Lerp(prefabConfiguration.m_scale.x, prefabConfiguration.m_scale.y, prefabConfiguration.m_scaleCurve.Evaluate(Random.value));
                    yield return transformDetails;
                }
            }
        }

        public int m_seed;

        public List<InstancePrefabConfiguration> m_prefabConfigurations;

        [Layer]
        public int m_snapLayer;

        public delegate void ChangeEvent(InstanceProvider instanceProvider);
        public static event ChangeEvent ms_changeEvent;

        public IEnumerable<InstanceDetails> GenerateInstanceDetails() {
            InstanceArea[] areas = GetComponentsInChildren<InstanceArea>();

            int seed = m_seed;
            foreach (InstancePrefabConfiguration configuration in m_prefabConfigurations) {
                ITransformDetailsProviding transformDetailsProvider = InstanceArea.TransformDetailsProvider(areas, configuration.m_density, seed, configuration.m_instancesPerUnit);
                InstanceDetails details = new InstanceDetails(configuration, transformDetailsProvider, 1 << m_snapLayer);
                yield return details;
                ++seed;
            }
        }

        private void OnValidate() {
            if (m_seed == 0) {
                m_seed = (int)System.DateTime.Now.Ticks;
            }
            if (ms_changeEvent != null) {
                ms_changeEvent(this);
            }
        }

        /*private void OnDrawGizmos() {
            ITransformDetailsProviding transformDetailsProvider = InstanceArea.TransformDetailsProvider(m_additiveAreas, m_density, m_seed);
            foreach (TransformDetails transformDetails in transformDetailsProvider.GenerateSnappedDetails(1 << m_snapLayer)) {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transformDetails.position + transformDetails.rotation * Vector3.forward * transformDetails.uniformScale, transformDetails .position+ transformDetails.rotation * Vector3.back * transformDetails.uniformScale);
                Gizmos.DrawLine(transformDetails.position + transformDetails.rotation * Vector3.left * transformDetails.uniformScale, transformDetails.position + transformDetails.rotation * Vector3.right * transformDetails.uniformScale);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(transformDetails.position, transformDetails.position + (transformDetails.rotation * Vector3.up * transformDetails.uniformScale));
            }
        }*/

#if false


        private void OnValidate() {
            if (m_seed == 0) {
                m_seed = (int)System.DateTime.Now.Ticks;
            }
            m_version = System.Guid.NewGuid();
        }





        Vector3 worldPosition = transform.TransformPoint(position);
        Quaternion worldRotation = Quaternion.identity;

        RaycastHit hit;
                if (Physics.Raycast(worldPosition + Vector3.up* 3000, Vector3.down, out hit, float.MaxValue, 1 << m_snapLayer)) {
                    worldPosition = hit.point;
                    worldRotation = Quaternion.LookRotation(Vector3.Cross(Vector3.down, hit.normal), hit.normal);
                }

    //float scaleT = Random.value;
    //float scaleU = m_scaleDistribution.Evaluate(scaleT);
    float scale = 1.0f;// Mathf.Lerp(m_scaleRange.x, m_scaleRange.y, scaleU);

    InstanceDetails details = new InstanceDetails();
    details.position = worldPosition;
                details.uniformScale = scale;
                details.rotation = worldRotation;

                yield return details;



#endif
    }
}
