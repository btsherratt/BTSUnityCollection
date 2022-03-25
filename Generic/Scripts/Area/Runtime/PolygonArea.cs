using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonArea : MonoBehaviour {
    const int kPointsPerUnit = 10;

    public struct Point {
        public Vector3 position;
        public float uniformScale;
        public Quaternion rotation;
    }

    [Layer]
    public int m_snapLayer;

    public Vector3[] m_points;

    public int m_seed;

    [Range(0, 1)]
    public float m_density = 0.5f;

    [MinMax(0.0f, 10.0f)]
    public Vector2 m_scaleRange = Vector2.one;
    public AnimationCurve m_scaleDistribution = AnimationCurve.EaseInOut(0, 0, 1, 1);

    System.Guid m_version = System.Guid.NewGuid();
    public System.Guid Version => m_version;

    private void OnValidate() {
        if (m_seed == 0) {
            m_seed = (int)System.DateTime.Now.Ticks;
        }
        m_version = System.Guid.NewGuid();
    }

    public IEnumerable<Point> FillPoints() {
        Random.State oldState = Random.state;

        Random.InitState(m_seed);

        Vector3 center = Vector3.zero;
        foreach (Vector3 point in m_points) {
            center += point;
        }
        center /= m_points.Length;

        float totalArea = 0;
        float[] areas = new float[m_points.Length];
        for (int i = 0; i < m_points.Length; ++i) {
            Vector3 pointA = m_points[i];
            Vector3 pointB = m_points[(i + 1) % m_points.Length];
            Vector3 pointC = center;

            // Don't consider this component. We might want to project instead, but this is fine for now.
            pointA.y = 0;
            pointB.y = 0;
            pointC.y = 0;

            Vector3 deltaA = pointA - pointC;
            Vector3 deltaB = pointB - pointC;

            float area = Vector3.Cross(deltaA, deltaB).magnitude / 2.0f;
            areas[i] = area;
            totalArea += area;
        }

        //float area = (bounds.size.x * bounds.size.z) / 2.0f;
        int count = Mathf.FloorToInt(Mathf.Lerp(0, kPointsPerUnit * Mathf.Sqrt(totalArea), m_density));

        for (int i = 0; i < count; ++i) {
            float random = Random.Range(0, totalArea);

            int triangleAIdx = 0;

            while (random > areas[triangleAIdx]) {
                random -= areas[triangleAIdx];
                ++triangleAIdx;
            }

            triangleAIdx %= m_points.Length; // For safety coz floats are weird...
            int triangleBIdx = (triangleAIdx + 1) % m_points.Length;

            Vector3 pointA = m_points[triangleAIdx];
            Vector3 pointB = m_points[triangleBIdx];
            Vector3 pointC = center;

            // Don't consider this component. We might want to project instead, but this is fine for now.
            pointA.y = 0;
            pointB.y = 0;
            pointC.y = 0;

            Vector3 deltaA = pointA - pointC;
            Vector3 deltaB = pointB - pointC;

            float rndA = Random.value;
            float rndB = Random.value;

            Vector3 position;
            if (rndA + rndB <= 1.0f) {
                Vector3 rnd = deltaA * rndA + deltaB * rndB;
                position = pointC + rnd;
            } else {
                Vector3 rnd = deltaA * rndA + deltaB * rndB;
                position = pointC + deltaA + deltaB - rnd;
            }

            Vector3 worldPosition = transform.TransformPoint(position);
            Quaternion worldRotation = Quaternion.identity;

            RaycastHit hit;
            if (Physics.Raycast(worldPosition + Vector3.up * 3000, Vector3.down, out hit, float.MaxValue, 1 << m_snapLayer)) {
                worldPosition = hit.point;
                worldRotation = Quaternion.LookRotation(Vector3.Cross(Vector3.down, hit.normal), hit.normal);
            }

            float scaleT = Random.value;
            float scaleU = m_scaleDistribution.Evaluate(scaleT);
            float scale = Mathf.Lerp(m_scaleRange.x, m_scaleRange.y, scaleU);

            Point point = new Point();
            point.position = worldPosition;
            point.uniformScale = scale;
            point.rotation = worldRotation;

            yield return point;
        }

        Random.state = oldState;
    }
}
