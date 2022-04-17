using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class LocalWorldSphere : MonoBehaviour {
    public float m_radius = 100.0f;

    public GameObject m_prefab;
    public int m_numPooledObjects;

    ObjectPool<GameObject> m_prefabPool;
    GameObject[] m_activeInstances;

    void Start() {
        m_prefabPool = new ObjectPool<GameObject>(CreatePrefab, defaultCapacity: m_numPooledObjects, maxSize: m_numPooledObjects);
        m_activeInstances = new GameObject[m_numPooledObjects];

        for (int i = 0; i < m_numPooledObjects; ++i) {
            m_activeInstances[i] = SpawnInstance();
        }
    }

    GameObject CreatePrefab() {
        return Instantiate(m_prefab);
    }

    void Update() {
        for (int i = 0; i < m_numPooledObjects; ++i) {
            if (m_activeInstances[i] != null && Vector3.Distance(m_activeInstances[i].transform.position, transform.position) > m_radius * 2.0f) {
                m_prefabPool.Release(m_activeInstances[i]);
                m_activeInstances[i] = null;
            }
        }

        for (int i = 0; i < m_numPooledObjects; ++i) {
            if (m_activeInstances[i] == null && m_prefabPool.CountInactive > 0) {
                m_activeInstances[i] = SpawnInstance();
            }
        }
    }

    GameObject SpawnInstance() {
        GameObject instance = m_prefabPool.Get();
        Vector2 direction = Random.insideUnitCircle.normalized;
        Vector2 position = direction * m_radius * (Random.value + 1.0f);
        instance.transform.position = transform.TransformPoint(new Vector3(position.x, 0, position.y));
        return instance;
    }


    /*int m_currentPoolOffset;
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
    }*/
}
