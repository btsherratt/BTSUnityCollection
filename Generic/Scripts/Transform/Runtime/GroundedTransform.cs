using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GroundedTransformExtensions {
    public static void SnapToGround(this Transform transform, Vector3 direction, int layer, bool setXRotation = false, bool setYRotation = false, bool setZRotation = false) {
        RaycastHit hit;
        if (Physics.Raycast(transform.position - direction * 100.0f, direction, out hit, float.PositiveInfinity, 1 << layer)) {
            transform.position = hit.point;

            if (setXRotation || setYRotation || setZRotation) {
                Quaternion rotation = transform.localRotation;
                Vector3 angles = rotation.eulerAngles;

                Vector3 right = Vector3.Cross(Vector3.forward, hit.normal);
                Vector3 forward = Vector3.Cross(right, hit.normal);

                Quaternion hitRotation = Quaternion.LookRotation(forward, hit.normal);
                Vector3 hitAngles = hitRotation.eulerAngles;

                Vector3 newAngles = Vector3.zero;
                newAngles.x = setXRotation ? hitAngles.x : angles.x;
                newAngles.y = setYRotation ? hitAngles.y : angles.y;
                newAngles.z = setZRotation ? hitAngles.z : angles.z;

                transform.rotation = Quaternion.Euler(newAngles);
            }
        }
    }
}

[ExecuteInEditMode]
public class GroundedTransform : MonoBehaviour {
    [Layer]
    public int m_groundLayer;

    public Vector3 m_groundingDirection = Vector3.down;

    public bool m_setXRotation = false;
    public bool m_setYRotation = false;
    public bool m_setZRotation = false;

    private void Update() {
        if (transform.hasChanged) {
            transform.SnapToGround(m_groundingDirection, m_groundLayer, m_setXRotation, m_setYRotation, m_setZRotation);
            transform.hasChanged = false;
        }
    }
}
