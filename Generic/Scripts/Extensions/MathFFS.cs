using UnityEngine;

public static class MathFFS {
    public static float InverseLerpUnclamped(float a, float b, float v) {
        float t = (v - a) / (b - a);
        return t;
	}

	public static float DistanceLineSegmentSq(Vector3 p1, Vector3 p2, Vector3 testPoint) {
		Vector3 lineDelta = p2 - p1;
		Vector3 lineDirection = lineDelta.normalized;
		float lineLength = lineDelta.magnitude;
		Vector3 pointDelta = testPoint - p1;
		float pointDot = Vector3.Dot(pointDelta, lineDirection);
		Vector3 lineTestPoint = Vector3.Lerp(p1, p2, pointDot / lineLength);
		float distanceSq = (testPoint - lineTestPoint).sqrMagnitude;
		return distanceSq;
	}

	public static float DistanceLineSegment(Vector3 p1, Vector3 p2, Vector3 testPoint) {
		float distance = Mathf.Sqrt(DistanceLineSegmentSq(p1, p2, testPoint));
		return distance;
    }
}
