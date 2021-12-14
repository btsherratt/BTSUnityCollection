using UnityEditor;
using UnityEngine;

public class GridCloneWizard : ScriptableWizard {
    public GameObject m_clonedObject;
    public Vector3Int m_clones = new Vector3Int(10, 10, 10);
    public Vector3 m_spacing = Vector3.one;

    [MenuItem("GameObject/Grid Clone")]
    static void CreateWizard() {
        GridCloneWizard instance = ScriptableWizard.DisplayWizard<GridCloneWizard>("Grid Clone", "Create");
    }

    void OnWizardCreate() {
        BoundsInt cloneBounds = new BoundsInt(Vector3Int.zero, m_clones);

        foreach (Vector3Int position in cloneBounds.allPositionsWithin) {
            if (position != Vector3Int.zero) {
                Vector3 localPosition = Vector3.Scale(position, m_spacing);
                Vector3 worldPosition = m_clonedObject.transform.TransformPoint(localPosition);
                GameObject clone = Instantiate(m_clonedObject);
                clone.name = m_clonedObject.name;
                clone.transform.parent = m_clonedObject.transform.parent;
                clone.transform.position = worldPosition;
            }
        }
    }
}