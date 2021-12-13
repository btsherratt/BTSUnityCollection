using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoundsExtensions {
    public static IEnumerable<Vector3> Corners(this Bounds bounds) {
        Vector3 extentsX = bounds.extents.X();
        Vector3 extentsY = bounds.extents.Y();
        Vector3 extentsZ = bounds.extents.Z();

        yield return bounds.center - extentsX - extentsY - extentsZ;
        yield return bounds.center - extentsX - extentsY + extentsZ;
        yield return bounds.center - extentsX + extentsY - extentsZ;
        yield return bounds.center - extentsX + extentsY + extentsZ;
        yield return bounds.center + extentsX - extentsY - extentsZ;
        yield return bounds.center + extentsX - extentsY + extentsZ;
        yield return bounds.center + extentsX + extentsY - extentsZ;
        yield return bounds.center + extentsX + extentsY + extentsZ;
    }

    public static Rect ToViewportRect(this Bounds bounds, Camera camera, out float nearZ) {
        Vector2 min = Vector2.one;
        Vector2 max = Vector2.zero;

        nearZ = camera.farClipPlane;

        foreach (Vector3 point in bounds.Corners()) {
            Vector3 localPoint = camera.transform.InverseTransformPoint(point);
            nearZ = Mathf.Min(nearZ, localPoint.z);

            Vector2 transformedPoint = camera.WorldToViewportPoint(point);
            min = Vector2.Min(min, transformedPoint);
            max = Vector2.Max(max, transformedPoint);
        }

        Rect rect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        return rect;
    }
}
