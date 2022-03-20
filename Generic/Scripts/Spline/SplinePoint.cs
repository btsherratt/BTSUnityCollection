using UnityEngine;

[ExecuteInEditMode]
public class SplinePoint : MonoBehaviour
{
    private void Update()
    {
        if (transform.hasChanged)
        {
            SplineDesigner sd = GetComponentInParent<SplineDesigner>();
            if (sd != null && sd.snapToCollider != null)
            {
                Vector3 position = transform.position;
                position.y = position.y + sd.snapToCollider.bounds.max.y;

                Ray ray = new Ray(position, Vector3.down);

                RaycastHit hit;
                if (sd.snapToCollider.Raycast(ray, out hit, float.MaxValue))
                {
                    transform.position = hit.point;
                }
            }

            GetComponentInParent<Spline>().GenerateAndUpdate();

            transform.hasChanged = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
