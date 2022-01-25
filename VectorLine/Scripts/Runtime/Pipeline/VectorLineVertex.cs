using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct VectorLineVertex {
    public Vector3 position;
    public Color color;

    public VectorLineVertex(Vector3 position, Color color) {
        this.position = position;
        this.color = color;
    }
}