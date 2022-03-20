using UnityEngine;

[ExecuteInEditMode]
public class Spline : MonoBehaviour
{
    struct Interpolater
    {
        public static Interpolater Construct(Vector3[] controlPoints, int startIdx, float alpha)
        {
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

        public Vector3 Lerp(float t)
        {
            return ValueAtT(Mathf.Lerp(t1, t2, t));
        }

        Vector3 ValueAtT(float t)
        {
            Vector3 A1 = P0 * (t1 - t) / (t1 - t0) + P1 * (t - t0) / (t1 - t0);
            Vector3 A2 = P1 * (t2 - t) / (t2 - t1) + P2 * (t - t1) / (t2 - t1);
            Vector3 A3 = P2 * (t3 - t) / (t3 - t2) + P3 * (t - t2) / (t3 - t2);

            Vector3 B1 = A1 * (t2 - t) / (t2 - t0) + A2 * (t - t0) / (t2 - t0);
            Vector3 B2 = A2 * (t3 - t) / (t3 - t1) + A3 * (t - t1) / (t3 - t1);

            Vector3 C = B1 * (t2 - t) / (t2 - t1) + B2 * (t - t1) / (t2 - t1);
            return C;
        }

        public void GenerateInPlace(ref SplinePoint[] points, ref int pointsHead, out float length, float tolerance, int maxSteps,    float lengthOffset, float worldLengthOffset)
        {
            Vector3 firstPoint = Lerp(0.0f);
            if (points != null)
            {
                points[pointsHead] = new SplinePoint(firstPoint, lengthOffset, worldLengthOffset);
            }

            float previousLength = 0.0f;
            int numPoints;
            for (numPoints = 2; numPoints <= maxSteps; ++numPoints)
            {
                float currentLength = 0.0f;
                Vector3 previousPoint = firstPoint;
                for (int j = 1; j < numPoints; ++j)
                {
                    float t = (float)j / (float)numPoints;
                    Vector3 p = Lerp(t);
                    float distance = Vector3.Distance(previousPoint, p);
                    currentLength += distance;
                    previousPoint = p;

                    if (points != null)
                    {
                        points[pointsHead + j] = new SplinePoint(p, t + lengthOffset, currentLength + worldLengthOffset);
                    }
                }

                float test = Mathf.Abs(previousLength - currentLength);
                previousLength = currentLength;
                if (test <= tolerance)
                {
                    break;
                }
            }

            pointsHead += numPoints;
            length = previousLength;
        }
    }

    [Range(0.0f, 1.0f)]
    public float alpha = 0.5f;

    [Header("Length Parameters - Don't change these if you don't understand them")]
    public float lengthTolerance = 0.01f;
    public int lengthMaximumDepth = 200;

    Vector3[] controlPoints;

    float[] segmentLengths;
    float length;

    public struct SplinePoint
    {
        public Vector3 position;
        public float distance;
        public float worldDistance;

        public SplinePoint(Vector3 position, float distance, float worldDistance)
        {
            this.position = position;
            this.distance = distance;
            this.worldDistance = worldDistance;
        }
    }

    SplinePoint[] cachedSpline;




    public enum Units
    {
        ZeroToOne,
        ZeroToSegments,
        World
    }

    public Vector3 Lerp(float t, Units units)
    {
        float relativeToSegmentT = 0.0f;

        switch (units)
        {
            case Units.ZeroToOne:
                relativeToSegmentT = t * (controlPoints.Length - 2); // FIXME, this isn't correct
                break;

            case Units.ZeroToSegments:
                relativeToSegmentT = t;
                break;

            case Units.World:
                // FIXME, terrible...
                for (int i = 1; i < cachedSpline.Length; ++i)
                {
                    if (cachedSpline[i - 1].worldDistance <= t && cachedSpline[i].worldDistance > t)
                    {
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

    public float ClosestPositionOnSpline(Vector3 point, Units units)
    {
        if (cachedSpline == null)
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

        return cachedSpline[minPositionIdx].worldDistance;
    }




    private void OnValidate()
    {
       // GenerateAndUpdate();
    }


    private void OnDrawGizmos()
    {
        if (cachedSpline == null)
        {
            cachedSpline = GenerateSpline();
        }

        for (int i = 1; i < cachedSpline.Length; ++i)
        {
            Gizmos.DrawLine(cachedSpline[i - 1].position, cachedSpline[i].position);
        }
    }


    public void GenerateAndUpdate()
    {
        cachedSpline = GenerateSpline();

        SplineModel[] models = GetComponents<SplineModel>();
        foreach (SplineModel model in models)
        {
            model.Generate(cachedSpline);
        }
    }

    public SplinePoint[] GenerateSpline()
    {
        FindControlPoints();

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
        for (int i = 0; i < splineSlice.Length; ++i)
        {
            splineSlice[i] = spline[i];
        }

        return splineSlice;
    }


    private void FindControlPoints()
    {
        controlPoints = new Vector3[transform.childCount];
        for (int i = 0; i < controlPoints.Length; ++i)
        {
            controlPoints[i] = transform.GetChild(i).position;
        }
    }


}
