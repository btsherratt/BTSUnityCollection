using UnityEngine;

public static class Vector3Extensions {
    public static Vector2 XY(this Vector3 vector3) {
        return new Vector2(vector3.x, vector3.y);
    }
    public static Vector2 XZ(this Vector3 vector3) {
        return new Vector2(vector3.x, vector3.z);
    }
}
