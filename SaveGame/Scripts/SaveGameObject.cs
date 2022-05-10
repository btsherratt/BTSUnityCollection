using UnityEngine;

public class SaveGameObject : TrackedMonoBehaviour<SaveGameObject> {
    public string m_saveGameObjectUUID;

    private void OnValidate() {
        if (m_saveGameObjectUUID == null || m_saveGameObjectUUID.Length == 0) {
            m_saveGameObjectUUID = System.Guid.NewGuid().ToString();
        }
    }
}
