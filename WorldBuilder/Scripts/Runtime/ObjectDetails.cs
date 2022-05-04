using System;
using UnityEngine;

namespace SKFX.WorldBuilder {
    public struct ObjectDetails {
        public TransformDetails transformDetails;
        public float radius;
    }

    public static class ObjectDetailsExtensions {
        public static ObjectDetails MakeDetails(this GameObject gameObject) {
            ObjectDetails objectDetails = new ObjectDetails();
            objectDetails.transformDetails = gameObject.transform.MakeDetails();
            objectDetails.radius = gameObject.CalculateBounds().extents.magnitude;
            return objectDetails;
        }
    }
}
