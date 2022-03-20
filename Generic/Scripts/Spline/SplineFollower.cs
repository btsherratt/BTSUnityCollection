using UnityEngine;

public class SplineFollower : MonoBehaviour
{
    public Transform target;
    public Spline spline;

    public Spline.Units trackingUnits;

    public float position = 0.0f;

    void Start()
    {
        spline = (spline != null) ? spline : GetComponent<Spline>();
        target = (target != null) ? target : transform;

        Update();
    }

    void Update()
    {
        target.position = spline.Lerp(position, trackingUnits);
    }
}
