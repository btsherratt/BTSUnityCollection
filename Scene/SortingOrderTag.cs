using UnityEngine;

[ExecuteAlways]
public class SortingOrderTag : MonoBehaviour {
    [SerializeField]
    int m_relativeSortingOrder;

    public int RelativeSortingOrder {
        get => m_relativeSortingOrder;
        set {
            m_relativeSortingOrder = value;
            UpdateSortingOrder();
        }
    }

    public int SortingOrder { get => CalculateSortingOrder(); }

    bool m_parentClean;
    SortingOrderTag m_parentSortingOrderTag;

    Renderer[] m_renderers;
    
    void OnEnable() {
        UpdateSortingOrder();
    }

    void OnTransformParentChanged() {
        m_parentClean = false;
        m_parentSortingOrderTag = null;
        UpdateSortingOrder();
    }

    int CalculateSortingOrder() {
        if (m_parentClean == false) {
            Transform parent = transform.parent;
            m_parentSortingOrderTag = parent != null ? parent.GetComponentInParent<SortingOrderTag>() : null;
            m_parentClean = true;
        }

        int parentOrder = m_parentSortingOrderTag != null ? m_parentSortingOrderTag.SortingOrder : 0;
        int sortingOrder = parentOrder + m_relativeSortingOrder;

        return sortingOrder;
    }

    void UpdateSortingOrder() {
        if (m_renderers == null) {
            m_renderers = GetComponents<Renderer>();
        }

        foreach (Renderer r in m_renderers) {
            r.sortingOrder = SortingOrder;
        }
    }
}
