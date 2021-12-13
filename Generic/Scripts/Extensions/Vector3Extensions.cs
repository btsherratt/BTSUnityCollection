using UnityEngine;

public static class Vector3Extensions {
    public static Vector3 X(this Vector3 vector3) {
        return new Vector3(vector3.x, 0, 0);
    }

    public static Vector3 Y(this Vector3 vector3) {
        return new Vector3(0, vector3.y, 0);
    }

    public static Vector3 Z(this Vector3 vector3) {
        return new Vector3(0, 0, vector3.z);
    }

    public static Vector2 XY(this Vector3 vector3) {
        return new Vector2(vector3.x, vector3.y);
    }

    public static Vector2 XZ(this Vector3 vector3) {
        return new Vector2(vector3.x, vector3.z);
    }
}
