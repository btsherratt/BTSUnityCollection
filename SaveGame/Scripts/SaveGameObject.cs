using UnityEngine;

public class SaveGameObject : TrackedMonoBehaviour<SaveGameObject> {
    public static SaveGameObject Find(string uuid, SaveGameObject skip = null) {
        foreach (SaveGameObject saveGameObject in All()) {
            if (saveGameObject != skip && saveGameObject.m_saveGameObjectUUID == uuid) {
                return saveGameObject;
            }
        }
        return null;
    }

    public string m_saveGameObjectUUID;

    //private void OnValidate() {
    //    if (m_saveGameObjectUUID == null || m_saveGameObjectUUID.Length == 0 || Find(m_saveGameObjectUUID, this) != null) {
    //        m_saveGameObjectUUID = System.Guid.NewGuid().ToString();
    //    }
    //}
}
