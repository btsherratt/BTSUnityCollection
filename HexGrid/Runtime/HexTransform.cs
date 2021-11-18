using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteAlways]
public class HexTransform : MonoBehaviour {
    public HexCoordinate m_coordinate;

    HexGrid m_hexGrid;
    public HexGrid Grid {
        get {
            if (m_hexGrid == null) {
                m_hexGrid = transform.parent?.GetComponentInParent<HexGrid>();
            }
            return m_hexGrid;
        }
    }

    private void OnTransformParentChanged() {
        m_hexGrid = null;
    }

    void Update() {
        HexGrid hexGrid = Grid;
        if (hexGrid != null) {
            transform.position = hexGrid.CellPosition(m_coordinate);
        }
    }
}

public static class HexTransformExtensions {
    static ConditionalWeakTable<GameObject, HexTransform> ms_transformByGameObject;

    public static HexTransform HexTransform(this Component component) {
        if (ms_transformByGameObject == null) {
            ms_transformByGameObject = new ConditionalWeakTable<GameObject, HexTransform>();
        }

        HexTransform hexTransform = null;
        if (ms_transformByGameObject.TryGetValue(component.gameObject, out hexTransform) == false) {
            hexTransform = component.GetComponent<HexTransform>();
            if (hexTransform != null) {
                ms_transformByGameObject.Add(component.gameObject, hexTransform);
            }
        }

        return hexTransform;
    }
}
