using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BTS/VectorLine/Polygon Asset")]
public class VectorLinePolygonAsset : ScriptableObject {
    public bool m_lineLoop;
    public VectorLineVertex[] m_vertices;
}