using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalWorldSphere : MonoBehaviour {
    RaycastHit[] ms_raycastHits;

    public float m_radius = 100.0f;

    [Layer]
    public int m_groundLayer;

    public Transform m_monitorParentTransform;

    public int m_transformsPerPool = 10;

    Transform[] m_monitoredTransforms;
    int m_currentPoolOffset;
    int m_poolCount;

    void Start() {
        m_monitoredTransforms = new Transform[m_monitorParentTransform.childCount];
        for (int i = 0; i < m_monitoredTransforms.Length; ++i) {
            m_monitoredTransforms[i] = m_monitorParentTransform.GetChild(i);
        }
        m_poolCount = Mathf.CeilToInt((float)m_monitoredTransforms.Length / m_transformsPerPool);
    }

    void Update() {
        for (int i = 0; i < m_monitoredTransforms.Length; ++i) {
            if ((i + m_currentPoolOffset) % m_poolCount == 0) {
                Transform monitoredTransform = m_monitoredTransforms[i];

                if (Vector3.Distance(monitoredTransform.position, transform.position) > m_radius) {
                    // FIXME, make it wrap rather than re-random...
                    Vector3 newPosition = transform.position + Random.onUnitSphere * m_radius;// collider.transform.TransformPoint(collider.sharedMesh.vertices[i]);

                    Ray ray = new Ray(newPosition + Vector3.up * 1000, Vector3.down);

                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, float.MaxValue, 1 << m_groundLayer)) {
                        monitoredTransform.position = hit.point;
                        monitoredTransform.up = hit.normal;
                    }
                }
            }
        }
        ++m_currentPoolOffset;
    }
}
