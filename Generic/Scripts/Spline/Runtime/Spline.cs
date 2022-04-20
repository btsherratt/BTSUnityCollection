using UnityEngine;
using Unity.Collections;

public class Spline : MonoBehaviour {
    public enum Units {
        ZeroToOne,
        ZeroToSegments,
        World
    }

    struct SplinePoint {
        public Vector3 position;
        public Vector3 forward;
        public float distance;
        public float worldDistance;

        public SplinePoint(Vector3 position, Vector3 forward, float distance, float worldDistance) {
            this.position = position;
            this.forward = forward.normalized;
            this.distance = distance;
            this.worldDistance = worldDistance;
        }
    }

    struct SplineSegment {
        public float worldLength;
        public Bounds bounds;
        public int startPoint;
        public int numPoints;
        
        public SplineSegment(float worldLength, Bounds bounds, int startPoint, int numPoints) {
            this.worldLength = worldLength;
            this.bounds = bounds;
            this.startPoint = startPoint;
            this.numPoints = numPoints;
        }
    }

    public struct SplineImpl : System.IDisposable {
        public float Length => m_cachedLength;

        NativeArray<Vector3> m_controlPoints;
        NativeArray<SplineSegment> m_cachedSpline;
        NativeArray<SplinePoint> m_cachedPoints;
        float m_cachedLength;

        float m_alpha;
        float m_lengthTolerance;
        int m_lengthMaximumDepth;

        public Vector3 Lerp(float t, Units units) {
            return Lerp(out _, t, units);
        }

        public Vector3 Lerp(out Vector3 forward, float t, Units units) {
            float relativeToSegmentT = 0.0f;

            switch (units) {
                case Units.ZeroToOne:
                    relativeToSegmentT = t * m_controlPoints.Length; // FIXME, this isn't correct
                    break;

                case Units.ZeroToSegments:
                    relativeToSegmentT = t;
                    break;

                case Units.World:
                    // Just in case...
                    relativeToSegmentT = m_controlPoints.Length;

                    // FIXME, terrible...
                    for (int i = 0; i < m_cachedSpline.Length; ++i) {
                        float segmentWorldLength = m_cachedSpline[i].worldLength;
                        if (t < segmentWorldLength) {
                            relativeToSegmentT = i + t / segmentWorldLength;
                            break;
                        } else {
                            t -= segmentWorldLength;
                        }
                    }
                    break;
            }

            if (relativeToSegmentT <= 0) {
                forward = m_cachedPoints[m_cachedSpline[0].startPoint].forward;
                return m_controlPoints[0];
            } else if (relativeToSegmentT >= m_controlPoints.Length) {
                forward = m_cachedPoints[m_cachedSpline[0].startPoint].forward; // FIXME, WRONG THING
                return m_controlPoints[m_controlPoints.Length - 1];
            } else {
                int idx = Mathf.FloorToInt(relativeToSegmentT);
                relativeToSegmentT -= idx;
                SplineSegment segment = m_cachedSpline[idx];

                int pointIdx = segment.startPoint;
                for (; pointIdx < segment.startPoint + segment.numPoints - 2; ++pointIdx) {
                    float length = m_cachedPoints[pointIdx + 1].distance; // FIXME, confusing...
                    if (relativeToSegmentT < length) {
                        break;
                    } else {
                        // relativeToSegmentT -= length;
                    }
                }

                float pt = Mathf.InverseLerp(m_cachedPoints[pointIdx].distance, m_cachedPoints[pointIdx + 1].distance, relativeToSegmentT);
                forward = Vector3.Lerp(m_cachedPoints[pointIdx].forward, m_cachedPoints[pointIdx + 1].forward, pt);
                return Vector3.Lerp(m_cachedPoints[pointIdx].position, m_cachedPoints[pointIdx + 1].position, pt);
            }
        }

        public bool TestDistanceToSpline(Vector3 position, float distance) {
            return TestDistanceToSpline(position, distance, Vector3.one);
        }

        public bool TestDistanceToSpline(Vector3 position, float distance, Vector3 componentContribution) {
            Vector3 testPosition = Vector3.Scale(position, componentContribution);
            foreach (SplineSegment splineSegment in m_cachedSpline) {
               Vector3 boundsTestPosition = testPosition;// testPosition.XZ(); // FIXME
               boundsTestPosition.x = Mathf.Lerp(splineSegment.bounds.center.x, testPosition.x, componentContribution.x);
               boundsTestPosition.y = Mathf.Lerp(splineSegment.bounds.center.y, testPosition.y, componentContribution.y);
               boundsTestPosition.z = Mathf.Lerp(splineSegment.bounds.center.z, testPosition.z, componentContribution.z);

               if (splineSegment.bounds.SqrDistance(boundsTestPosition) <= (distance * distance)) {
                    for (int i = splineSegment.startPoint; i < splineSegment.startPoint + splineSegment.numPoints - 1; ++i) {
                        SplinePoint point1 = m_cachedPoints[i];
                        SplinePoint point2 = m_cachedPoints[i + 1];
                        Vector3 testPoint1 = Vector3.Scale(point1.position, componentContribution);
                        Vector3 testPoint2 = Vector3.Scale(point2.position, componentContribution);
                        if (MathFFS.DistanceLineSegmentSq(testPoint1, testPoint2, testPosition) <= distance * distance) {
                            return true;
                        }
                    }
               }
            }

            return false;
        }

        Interpolater SegmentInterpolater(int segmentIdx) {
            int numSegments = m_controlPoints.Length;
            Vector3 P0 = m_controlPoints[Mathf.Clamp(segmentIdx - 1, 0, numSegments - 1)];
            Vector3 P1 = m_controlPoints[segmentIdx];
            Vector3 P2 = m_controlPoints[Mathf.Clamp(segmentIdx + 1, 0, numSegments - 1)];
            Vector3 P3 = m_controlPoints[Mathf.Clamp(segmentIdx + 2, 0, numSegments - 1)];

            Interpolater interpolater = Interpolater.Construct(P0, P1, P2, P3, m_alpha);
            return interpolater;
        }

        public SplineImpl(Allocator allocator, Vector3[] controlPoints, float alpha, float lengthTolerance, int lengthMaximumDepth) {
            m_controlPoints = new NativeArray<Vector3>(controlPoints, allocator);
            m_alpha = alpha;
            m_lengthTolerance = lengthTolerance;
            m_lengthMaximumDepth = lengthMaximumDepth;

            int numSegments = m_controlPoints.Length - 1;

            m_cachedSpline = new NativeArray<SplineSegment>(numSegments, allocator);
            m_cachedLength = 0.0f;

            m_cachedPoints = new NativeArray<SplinePoint>(numSegments * m_lengthMaximumDepth, allocator);

            int pointsStartIdx = 0;
            for (int segmentIdx = 0; segmentIdx < numSegments; ++segmentIdx) {
                Interpolater interpolater = SegmentInterpolater(segmentIdx);

                Bounds bounds;
                float length;

                int numPoints = interpolater.Generate(m_cachedPoints, pointsStartIdx, out bounds, out length, m_lengthTolerance, m_lengthMaximumDepth);
                SplineSegment segment = new SplineSegment(length, bounds, pointsStartIdx, numPoints);
                m_cachedSpline[segmentIdx] = segment;

                m_cachedLength += length;

                pointsStartIdx += numPoints;
            }
        }

        public void Dispose() {
            m_controlPoints.Dispose();
            m_cachedSpline.Dispose();
            m_cachedPoints.Dispose();
        }
    }

    struct Interpolater {
        public static Interpolater Construct(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float alpha) {
            Interpolater i;

            i.P0 = P0;
            i.P1 = P1;
            i.P2 = P2;
            i.P3 = P3;

            float distance1 = Vector3.Distance(i.P0, i.P1);
            float distance2 = Vector3.Distance(i.P1, i.P2);
            float distance3 = Vector3.Distance(i.P2, i.P3);

            i.t0 = 0.0f;
            i.t1 = i.t0 + (distance1 > float.Epsilon ? Mathf.Pow(distance1, alpha) : 0.0f);
            i.t2 = i.t1 + (distance2 > float.Epsilon ? Mathf.Pow(distance2, alpha) : 0.0f);
            i.t3 = i.t2 + (distance3 > float.Epsilon ? Mathf.Pow(distance3, alpha) : 0.0f);

            return i;
        }

        Vector3 P0;
        Vector3 P1;
        Vector3 P2;
        Vector3 P3;

        float t0;
        float t1;
        float t2;
        float t3;

        public Vector3 Lerp(float t) {
            return ValueAtT(Mathf.Lerp(t1, t2, t));
        }

        Vector3 ValueAtT(float t) {
            Vector3 A1 = Mathf.Abs(t0 - t1) > float.Epsilon ? Vector3.LerpUnclamped(P0, P1, MathFFS.InverseLerpUnclamped(t0, t1, t)) : P0;
            Vector3 A2 = Mathf.Abs(t1 - t2) > float.Epsilon ? Vector3.LerpUnclamped(P1, P2, MathFFS.InverseLerpUnclamped(t1, t2, t)) : P1;
            Vector3 A3 = Mathf.Abs(t2 - t3) > float.Epsilon ? Vector3.LerpUnclamped(P2, P3, MathFFS.InverseLerpUnclamped(t2, t3, t)) : P2;

            Vector3 B1 = Mathf.Abs(t0 - t2) > float.Epsilon ? Vector3.LerpUnclamped(A1, A2, MathFFS.InverseLerpUnclamped(t0, t2, t)) : A1;
            Vector3 B2 = Mathf.Abs(t1 - t3) > float.Epsilon ? Vector3.LerpUnclamped(A2, A3, MathFFS.InverseLerpUnclamped(t1, t3, t)) : A2;

            Vector3 C = Mathf.Abs(t1 - t2) > float.Epsilon ? Vector3.LerpUnclamped(B1, B2, MathFFS.InverseLerpUnclamped(t1, t2, t)) : B1;
            return C;
        }

        public int Generate(NativeArray<SplinePoint> points, int pointsStartIdx, out Bounds bounds, out float length, float tolerance, int maxSteps) {
            SplinePoint firstPoint = new SplinePoint(Lerp(0.0f), Vector3.forward, 0, 0); 
            points[pointsStartIdx] = firstPoint;

            bounds = new Bounds(points[pointsStartIdx].position, Vector3.zero);

            float previousLength = 0.0f;
            int numPoints;
            for (numPoints = 2; numPoints < maxSteps; ++numPoints) {
                float currentLength = 0.0f;
                Vector3 previousPosition = points[pointsStartIdx].position;
                for (int i = 1; i < numPoints; ++i) {
                    float t = i / (float)(numPoints - 1);
                    Vector3 position = Lerp(t);
                    float distance = Vector3.Distance(previousPosition, position);
                    currentLength += distance;
                    Vector3 forward = points[pointsStartIdx + i - 1].position - position;
                    points[pointsStartIdx + i] = new SplinePoint(position, forward, t, currentLength);

                    bounds.Encapsulate(position);
                    previousPosition = position;
                }

                float test = Mathf.Abs(previousLength - currentLength);
                previousLength = currentLength;
                if (test <= tolerance) {
                    break;
                }
            }

            firstPoint.forward = -(points[pointsStartIdx + 1].position - points[pointsStartIdx].position).normalized;
            points[pointsStartIdx] = firstPoint;

            length = previousLength;

            return numPoints;
        }
    }


    public Vector3[] m_controlPoints;

    [Range(0.0f, 1.0f)]
    public float m_alpha = 0.5f;

    [Header("Length Parameters - Don't change these if you don't understand them")]
    public float m_lengthTolerance = 0.01f;
    public int m_lengthMaximumDepth = 200;

    public delegate void SplineUpdateEvent(Spline spline);
    public static event SplineUpdateEvent ms_updateEvent;

    public float Length => CachedSplineImpl().Length;

    //float[] segmentLengths;
    //float length;

    SafeDisposable<SplineImpl>? m_cachedSpline;

    public Vector3 Lerp(float t, Units units) {
        return Lerp(out _, t, units);
    }
    
    public Vector3 Lerp(out Vector3 forward, float t, Units units) {
        return CachedSplineImpl().Lerp(out forward, t, units);
    }

    //public float ClosestPositionOnSpline(Vector3 point, Units units) {
    //    CacheSpline();
    //    return 0.0f;
    /*if (cachedSpline == null)
    {
        cachedSpline = GenerateSpline();
    }




    int minPositionIdx = 0;

    float minDistance = Vector3.Distance(point, cachedSpline[0].position);
    for (int i = 1; i < cachedSpline.Length; ++i)
    {
        float distance = Vector3.Distance(point, cachedSpline[i].position);
        if (distance < minDistance)
        {
            minDistance = distance;
            minPositionIdx = i;
        }
    }

    return cachedSpline[minPositionIdx].worldDistance;*/
    //}


    public bool TestDistanceToSpline(Vector3 position, float distance) {
        return TestDistanceToSpline(position, distance, Vector3.one);
    }

    public bool TestDistanceToSpline(Vector3 position, float distance, Vector3 componentContribution) {
        return CachedSplineImpl().TestDistanceToSpline(position, distance, componentContribution);
    }


    private void OnValidate() {
        if (m_cachedSpline != null) {
            m_cachedSpline.Value.Dispose();
            m_cachedSpline = null;
        }

        if (ms_updateEvent != null) {
            ms_updateEvent(this);
        }


        //       GenerateAndUpdate();
    }


    private void OnDrawGizmos() {
        CachedSplineImpl();

        if (m_cachedSpline != null) {
            SplineImpl spline = m_cachedSpline.Value;

            for (int i = 1; i < spline.Length; ++i) {
                Gizmos.DrawLine(transform.TransformPoint(spline.Lerp(i - 1, Units.World)), transform.TransformPoint(spline.Lerp(i, Units.World)));
            }
        }
    }

    /*public SplinePoint[] GenerateSpline() {
        int numSegments = controlPoints.Length - 3;
        SplinePoint[] spline = new SplinePoint[numSegments * lengthMaximumDepth];

        segmentLengths = new float[numSegments];
        length = 0.0f;

        int splinePoints = 0;
        for (int segmentIdx = 0; segmentIdx < numSegments; ++segmentIdx)
        {
            float segmentLength = 0.0f;

            Interpolater interpolater = Interpolater.Construct(controlPoints, segmentIdx, alpha);
            interpolater.GenerateInPlace(ref spline, ref splinePoints, out segmentLength, lengthTolerance, lengthMaximumDepth,            segmentIdx, length);

            segmentLengths[segmentIdx] = segmentLength;
            length += segmentLength;
        }

        // FIXME, better memory?
        SplinePoint[] splineSlice = new SplinePoint[splinePoints];
        for (int i = 0; i < splineSlice.Length; ++i) {
            splineSlice[i] = spline[i];
        }

        return splineSlice;
    }*/

    public SplineImpl CachedSplineImpl() {
        if (m_cachedSpline == null) {
            m_cachedSpline = GetSplineImpl(Allocator.Persistent);
        }
        return m_cachedSpline.Value;
    }

    public SplineImpl GetSplineImpl(Allocator allocator) {
        return new SplineImpl(allocator, m_controlPoints, m_alpha, m_lengthTolerance, m_lengthMaximumDepth);
    }
}













#if false



using UnityEngine;

public class Spline : MonoBehaviour {
    struct Interpolater {
        public static Interpolater Construct(Vector3[] controlPoints, int startIdx, float alpha) {
            Interpolater i;

            i.P0 = controlPoints[startIdx];
            i.P1 = controlPoints[startIdx + 1];
            i.P2 = controlPoints[startIdx + 2];
            i.P3 = controlPoints[startIdx + 3];

            i.t0 = 0.0f;
            i.t1 = i.t0 + Mathf.Pow(Vector3.Distance(i.P0, i.P1), alpha);
            i.t2 = i.t1 + Mathf.Pow(Vector3.Distance(i.P1, i.P2), alpha);
            i.t3 = i.t2 + Mathf.Pow(Vector3.Distance(i.P2, i.P3), alpha);

            return i;
        }

        Vector3 P0;
        Vector3 P1;
        Vector3 P2;
        Vector3 P3;

        float t0;
        float t1;
        float t2;
        float t3;

        public Vector3 Lerp(float t) {
            return ValueAtT(Mathf.Lerp(t1, t2, t));
        }

        Vector3 ValueAtT(float t) {
            Vector3 A1 = Vector3.Lerp(P0, P1, Mathf.InverseLerp(t0, t1, t));
            Vector3 A2 = Vector3.Lerp(P1, P2, Mathf.InverseLerp(t1, t2, t));
            Vector3 A3 = Vector3.Lerp(P2, P3, Mathf.InverseLerp(t2, t3, t));

            Vector3 B1 = Vector3.Lerp(A1, A2, Mathf.InverseLerp(t0, t2, t));
            Vector3 B2 = Vector3.Lerp(A2, A3, Mathf.InverseLerp(t1, t3, t));

            Vector3 C = Vector3.Lerp(B1, B2, Mathf.InverseLerp(t1, t2, t));
            return C;
        }

        public void GenerateInPlace(ref SplinePoint[] points, ref int pointsHead, out float length, float tolerance, int maxSteps, float lengthOffset, float worldLengthOffset) {
            Vector3 firstPoint = Lerp(0.0f);
            if (points != null) {
                points[pointsHead] = new SplinePoint(firstPoint, lengthOffset, worldLengthOffset);
            }

            float previousLength = 0.0f;
            int numPoints;
            for (numPoints = 2; numPoints <= maxSteps; ++numPoints) {
                float currentLength = 0.0f;
                Vector3 previousPoint = firstPoint;
                for (int j = 1; j < numPoints; ++j) {
                    float t = (float)j / (float)numPoints;
                    Vector3 p = Lerp(t);
                    float distance = Vector3.Distance(previousPoint, p);
                    currentLength += distance;
                    previousPoint = p;

                    if (points != null) {
                        points[pointsHead + j] = new SplinePoint(p, t + lengthOffset, currentLength + worldLengthOffset);
                    }
                }

                float test = Mathf.Abs(previousLength - currentLength);
                previousLength = currentLength;
                if (test <= tolerance) {
                    break;
                }
            }

            pointsHead += numPoints;
            length = previousLength;
        }
    }

    public Vector3[] controlPoints;

    [Range(0.0f, 1.0f)]
    public float alpha = 0.5f;

    [Header("Length Parameters - Don't change these if you don't understand them")]
    public float lengthTolerance = 0.01f;
    public int lengthMaximumDepth = 200;

    public delegate void SplineUpdateEvent(Spline spline);
    public static event SplineUpdateEvent ms_updateEvent;

    public float Length => length;

    //    float[] segmentLengths;
    float length;

    public struct SplineSegment {
        public float length;
        public float worldLength;
        public Bounds bounds;
        public SplinePoint[] points;
    }

    public struct SplinePoint {
        public Vector3 position;
        public float distance;
        public float worldDistance;

        public SplinePoint(Vector3 position, float distance, float worldDistance) {
            this.position = position;
            this.distance = distance;
            this.worldDistance = worldDistance;
        }
    }


    SplineSegment[] cachedSpline;
    //SplinePoint[] cachedSpline;




    public enum Units {
        ZeroToOne,
        ZeroToSegments,
        World
    }

    public Vector3 Lerp(float t, Units units) {
        float relativeToSegmentT = 0.0f;

        switch (units) {
            case Units.ZeroToOne:
                relativeToSegmentT = t * (controlPoints.Length - 2) + 1; // FIXME, this isn't correct
                break;

            case Units.ZeroToSegments:
                relativeToSegmentT = t;
                break;

            case Units.World:
                // FIXME, terrible...
                for (int i = 1; i < cachedSpline.Length; ++i) {
                    if (cachedSpline[i - 1].worldDistance <= t && cachedSpline[i].worldDistance > t) {
                        relativeToSegmentT = cachedSpline[i - 1].distance;
                        break;
                    }
                }
                break;
        }

        int idx = Mathf.FloorToInt(relativeToSegmentT);
        Interpolater interpolater = Interpolater.Construct(controlPoints, idx, alpha);

        return interpolater.Lerp(relativeToSegmentT - idx);
    }

    public float ClosestPositionOnSpline(Vector3 point, Units units) {
        if (cachedSpline == null) {
            cachedSpline = GenerateSpline();
        }




        int minPositionIdx = 0;

        float minDistance = Vector3.Distance(point, cachedSpline[0].position);
        for (int i = 1; i < cachedSpline.Length; ++i) {
            float distance = Vector3.Distance(point, cachedSpline[i].position);
            if (distance < minDistance) {
                minDistance = distance;
                minPositionIdx = i;
            }
        }

        return cachedSpline[minPositionIdx].worldDistance;
    }




    private void OnValidate() {
        GenerateAndUpdate();
    }


    private void OnDrawGizmos() {
        if (cachedSpline == null) {
            cachedSpline = GenerateSpline();
        }

        for (int i = 1; i < cachedSpline.Length; ++i) {
            Gizmos.DrawLine(transform.TransformPoint(cachedSpline[i - 1].position), transform.TransformPoint(cachedSpline[i].position));
        }
    }


    public void GenerateAndUpdate() {
        cachedSpline = GenerateSpline();

        if (ms_updateEvent != null) {
            ms_updateEvent(this);
        }
    }

    public SplinePoint[] GenerateSpline() {
        int numSegments = controlPoints.Length - 3;
        SplinePoint[] spline = new SplinePoint[numSegments * lengthMaximumDepth];

        segmentLengths = new float[numSegments];
        length = 0.0f;

        int splinePoints = 0;
        for (int segmentIdx = 0; segmentIdx < numSegments; ++segmentIdx) {
            float segmentLength = 0.0f;

            Interpolater interpolater = Interpolater.Construct(controlPoints, segmentIdx, alpha);
            interpolater.GenerateInPlace(ref spline, ref splinePoints, out segmentLength, lengthTolerance, lengthMaximumDepth, segmentIdx, length);

            segmentLengths[segmentIdx] = segmentLength;
            length += segmentLength;
        }

        // FIXME, better memory?
        SplinePoint[] splineSlice = new SplinePoint[splinePoints];
        for (int i = 0; i < splineSlice.Length; ++i) {
            splineSlice[i] = spline[i];
        }

        return splineSlice;
    }
}
#endif