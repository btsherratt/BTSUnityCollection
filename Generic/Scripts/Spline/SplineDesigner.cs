using UnityEngine;

public class SplineDesigner : MonoBehaviour
{
    public Collider snapToCollider;

    public Vector3 ProjectToCollider(Vector3 point)
    {
        Vector3 projectedPoint = point;

        if (snapToCollider != null)
        {
            Vector3 position = projectedPoint;
            position.y = position.y + snapToCollider.bounds.max.y;

            Ray ray = new Ray(position, Vector3.down);

            RaycastHit hit;
            if (snapToCollider.Raycast(ray, out hit, float.MaxValue))
            {
                projectedPoint = hit.point + Vector3.up * 0.01f;
            }
        }

        return projectedPoint;
    }
}
