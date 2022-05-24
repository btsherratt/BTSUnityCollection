using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            RaycastHit hit;
            if (Physics.Raycast(transform.position - m_groundingDirection * 100.0f, m_groundingDirection, out hit, float.PositiveInfinity, 1 << m_groundLayer)) {
                transform.position = hit.point;

                if (m_setXRotation || m_setYRotation || m_setZRotation) {
                    Quaternion rotation = transform.localRotation;
                    Vector3 angles = rotation.eulerAngles;

                    Quaternion hitRotation = Quaternion.LookRotation(Vector3.forward, hit.normal);
                    Vector3 hitAngles = hitRotation.eulerAngles;

                    Vector3 newAngles = Vector3.zero;
                    newAngles.x = m_setXRotation ? hitAngles.x : angles.x;
                    newAngles.y = m_setYRotation ? hitAngles.y : angles.y;
                    newAngles.z = m_setZRotation ? hitAngles.z : angles.z;

                    transform.rotation = Quaternion.Euler(newAngles);
                }
            }
            transform.hasChanged = false;
        }
    }
}
