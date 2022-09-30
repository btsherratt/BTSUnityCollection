using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SKFX.WorldBuilder {
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct TransformDetails {
        public const int Size = (3 * sizeof(float)) + (1 * sizeof(float)) + (4 * sizeof(float)) + (sizeof(int));

        public Vector3 position;
        public float uniformScale;
        public Quaternion rotation;
        public int cullingBin;
    }

    public static class TransformDetailsExtensions {
        public static TransformDetails MakeDetails(this Transform transform) {
            TransformDetails transformDetails = new TransformDetails();
            transformDetails.position = transform.localPosition;
            transformDetails.rotation = transform.localRotation;
            transformDetails.uniformScale = Mathf.Min(transform.localScale.x, transform.localScale.y, transform.localScale.z);
            transformDetails.cullingBin = 0;
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
