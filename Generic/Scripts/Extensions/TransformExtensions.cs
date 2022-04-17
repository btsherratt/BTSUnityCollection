using UnityEngine;

public static class TransformExtensions {
    public static void Match(this Transform transform, Transform matchTransform) {
        // Take a snapshot to make sure that changing the transform doesn't affect things
        Vector3 position = matchTransform.position;
        Quaternion rotation = matchTransform.rotation;

        transform.position = position;
        transform.rotation = rotation;
    }

    public static void MatchPositionXZ(this Transform transform, Transform matchTransform) {
        float y = transform.position.y;
        Vector3 position = matchTransform.position;
        transform.position = new Vector3(position.x, y, position.z);
    }
}
