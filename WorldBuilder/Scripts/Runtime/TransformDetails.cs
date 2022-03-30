using System;
using UnityEngine;

namespace SKFX.WorldBuilder {
    [Serializable]
    public struct TransformDetails {
        public Vector3 position;
        public float uniformScale;
        public Quaternion rotation;
    }

    public static class TransformDetailsExtensions {
        public static TransformDetails MakeDetails(this Transform transform) {
            TransformDetails transformDetails = new TransformDetails();
            transformDetails.position = transform.localPosition;
            transformDetails.rotation = transform.localRotation;
            transformDetails.uniformScale = Mathf.Min(transform.localScale.x, transform.localScale.y, transform.localScale.z);
            return transformDetails;
        }

        public static Transform ApplyDetails(this Transform transform, TransformDetails transformDetails) {
            transform.localScale = Vector3.one * transformDetails.uniformScale;
            transform.localRotation = transformDetails.rotation;
            transform.localPosition = transformDetails.position;
            return transform;
        }
    }
}
