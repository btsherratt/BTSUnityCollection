using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour {
    public float m_hexCellRadius;

    public Vector3 CellPosition(HexCoordinate cellCoordinate) {
        Vector3 localPosition = cellCoordinate.Position(m_hexCellRadius);
        Vector3 position = transform.TransformPoint(localPosition);
        return position;
    }

    public Vector3 CellCornerPosition(HexCoordinate cellCoordinate, HexCoordinate.Corner corner) {
        Vector3 localPosition = cellCoordinate.Corner(corner, m_hexCellRadius);
        Vector3 position = transform.TransformPoint(localPosition);
        return position;
    }

    public IEnumerable<Vector3> EdgeRing(/*HexCoordinate cellCoordinate, */int radius) {
        foreach (Vector3 localPosition in HexCoordinateExtensions.EdgeRing(radius, m_hexCellRadius)) {
            Vector3 position = transform.TransformPoint(localPosition);
            yield return position;
        }
    }
}
