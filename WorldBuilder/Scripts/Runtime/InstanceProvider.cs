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

            public AnimationCurve m_scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

        public struct InstanceDetails : ITransformDetailsProviding {
            public InstancePrefabConfiguration prefabConfiguration;
            ITransformDetailsProviding transformDetailsProvider;

            public InstanceDetails(InstancePrefabConfiguration prefabConfiguration, ITransformDetailsProviding transformDetailsProvider) {
                this.prefabConfiguration = prefabConfiguration;
                this.transformDetailsProvider = transformDetailsProvider;
            }

            public long DetailsCount => transformDetailsProvider.DetailsCount;

            public IEnumerable<TransformDetails> GenerateDetails() {
                foreach (TransformDetails td in transformDetailsProvider.GenerateDetails()) {
                    TransformDetails transformDetails = new TransformDetails();
                    transformDetails.position = td.position;
                    transformDetails.rotation = td.rotation;
                    transformDetails.uniformScale = Mathf.Lerp(prefabConfiguration.m_scale.x, prefabConfiguration.m_scale.y, prefabConfiguration.m_scaleCurve.Evaluate(Random.value));
                    yield return transformDetails;
                }
            }

            public IEnumerable<TransformDetails> GenerateSnappedDetails(int snapLayerMask) {
                foreach (TransformDetails td in transformDetailsProvider.GenerateSnappedDetails(snapLayerMask)) {
                    TransformDetails transformDetails = new TransformDetails();
                    transformDetails.position = td.position;
                    transformDetails.rotation = td.rotation;
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
            InstanceArea[] additiveAreas = GetComponents<InstanceArea>();

            int seed = m_seed;
            foreach (InstancePrefabConfiguration configuration in m_prefabConfigurations) {
                ITransformDetailsProviding transformDetailsProvider = InstanceArea.TransformDetailsProvider(additiveAreas, configuration.m_density, seed);
                InstanceDetails details = new InstanceDetails(configuration, transformDetailsProvider);
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
