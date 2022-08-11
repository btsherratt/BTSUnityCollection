using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;

public static class SaveGameSystem {
    class SaveGameData : ISaveGameDataReading, ISaveGameDataWriting {
        JSONNode m_jsonData;

        public SaveGameData(JSONNode data = null) {
            m_jsonData = data ?? new JSONObject();
        }

        public JSONNode ToJSONNode() {
            return m_jsonData;
        }

        public bool ReadBoolean(string key, bool defaultValue) {
            JSONNode dataNode = m_jsonData[key];
            if (dataNode != null && dataNode.IsBoolean) {
                bool data = dataNode;
                return data;
            } else {
                return defaultValue;
            }
        }

        public int ReadInteger(string key, int defaultValue) {
            JSONNode dataNode = m_jsonData[key];
            if (dataNode != null && dataNode.IsNumber) {
                int data = dataNode;
                return data;
            } else {
                return defaultValue;
            }
        }

        public float ReadFloat(string key, float defaultValue) {
            JSONNode dataNode = m_jsonData[key];
            if (dataNode != null && dataNode.IsNumber) {
                float data = dataNode;
                return data;
            } else {
                return defaultValue;
            }
        }

        public string ReadString(string key, string defaultValue) {
            JSONNode dataNode = m_jsonData[key];
            if (dataNode != null && dataNode.IsString) {
                string data = dataNode;
                return data;
            } else {
                return defaultValue;
            }
        }

        public Vector4 ReadVector(string key, Vector4 defaultValue) {
            JSONNode dataNode = m_jsonData[key];
            if (dataNode != null) {
                Vector4 data = dataNode.ReadVector4(defaultValue);
                return data;
            } else {
                return defaultValue;
            }
        }

        public Quaternion ReadQuaternion(string key, Quaternion defaultValue) {
            JSONNode dataNode = m_jsonData[key];
            if (dataNode != null) {
                Quaternion data = dataNode.ReadQuaternion(defaultValue);
                return data;
            } else {
                return defaultValue;
            }
        }

        public void WriteBoolean(string key, bool value) {
            m_jsonData[key] = value;
        }

        public void WriteInteger(string key, int value) {
            m_jsonData[key] = value;
        }

        public void WriteFloat(string key, float value) {
            m_jsonData[key] = value;
        }

        public void WriteString(string key, string value) {
            m_jsonData[key] = value;
        }

        public void WriteVector(string key, Vector4 value) {
            m_jsonData[key] = value;
        }

        public void WriteQuaternion(string key, Quaternion value) {
            m_jsonData[key] = value;
        }
    }

    static string kMagicKey = "magic";
    static string kMagicValue = "skfxsav";

    static string kVersionKey = "version";
    static int kVersionValue = 1;

    static string kDataKey = "data";
    
    static string SaveGamePath(int slot) {
        string directory = Application.persistentDataPath;
        string filename = $"save-{slot:00}.save";
        string fullPath = Path.Combine(directory, filename);
        return fullPath;
    }

    public static bool SaveGameExists(int slot) {
        string path = SaveGamePath(slot);
        bool exists = File.Exists(path);
        return exists;
    }

    public static void LoadGame(int slot) {
        string path = SaveGamePath(slot);
        string data = File.ReadAllText(path);

        Debug.Log($"Loading from {path}");

        JSONNode saveDataJSON = JSONNode.Parse(data);
        string magic = saveDataJSON[kMagicKey];
        int version = saveDataJSON[kVersionKey];
        if (magic == kMagicValue && version == kVersionValue) {
            JSONNode savedObjectDataJSON = saveDataJSON[kDataKey];
            if (savedObjectDataJSON != null) {
                foreach (string objectKey in savedObjectDataJSON.Keys) {
                    JSONNode savedObjectComponentsDataJSON = savedObjectDataJSON[objectKey];
                    SaveGameObject saveGameObject = SaveGameObject.Find(objectKey);
                    if (saveGameObject != null) {
                        foreach (ISaveGameDataProviding saveGameDataProvidingComponent in saveGameObject.GetComponents<ISaveGameDataProviding>()) {
                            string componentKey = saveGameDataProvidingComponent.SaveGameIdentifier;
                            JSONNode savedObjectComponentDataJSON = savedObjectComponentsDataJSON[componentKey];
                            if (savedObjectComponentDataJSON != null) {
                                SaveGameData saveGameData = new SaveGameData(savedObjectComponentDataJSON);
                                saveGameDataProvidingComponent.SaveGameApplySaveData(saveGameData); // Bad name...
                                //Debug.Log($"{saveGameObject.name} :: ${componentKey} ${objectKey}");
                            } else {
                                Debug.Log($"Missing data for {componentKey}, this may not be a bug.");
                            }
                        }
                    } else {
                        Debug.LogError($"Unable to find save game object: path={path}, objectKey={objectKey}");
                    }
                }
            } else {
                Debug.LogError($"Save object looks corrupted: path={path}");
            }
        } else {
            Debug.LogError($"Save format looks corrupted: path={path}, version={version}, magic={magic}");
        }

        // NB: Not great here, but we don't want to keep doing this for every object...
        Physics.SyncTransforms();
    }

    public static void SaveGame(int slot) {
        JSONNode savedObjectDataJSON = null;
        foreach (SaveGameObject saveGameObject in SaveGameObject.All()) {
            JSONNode savedObjectComponentsDataJSON = null;
            foreach (ISaveGameDataProviding saveGameDataProvidingComponent in saveGameObject.GetComponents<ISaveGameDataProviding>()) {
                SaveGameData saveGameData = new SaveGameData();
                saveGameDataProvidingComponent.SaveGameGatherSaveData(saveGameData);
                JSONNode savedObjectComponentDataJSON = saveGameData.ToJSONNode();
                if (savedObjectComponentDataJSON != null) {
                    if (savedObjectDataJSON == null) {
                        savedObjectDataJSON = new JSONObject();
                    }

                    if (savedObjectComponentsDataJSON == null) {
                        savedObjectComponentsDataJSON = new JSONObject();
                        savedObjectDataJSON[saveGameObject.m_saveGameObjectUUID] = savedObjectComponentsDataJSON;
                    }

                    string componentKey = saveGameDataProvidingComponent.SaveGameIdentifier;
                    savedObjectComponentsDataJSON[componentKey] = savedObjectComponentDataJSON;

                    //Debug.Log($"{saveGameObject.name} :: ${componentKey} ${saveGameObject.m_saveGameObjectUUID}");
                }
            }
        }

        JSONNode saveDataJSON = new JSONObject();
        saveDataJSON[kMagicKey] = kMagicValue;
        saveDataJSON[kVersionKey] = kVersionValue;

        if (savedObjectDataJSON != null) {
            saveDataJSON[kDataKey] = savedObjectDataJSON;
        }

        string path = SaveGamePath(slot);
        string data = saveDataJSON.ToString();
        File.WriteAllTextAsync(path, data);

        Debug.Log($"Saving to {path}");
    }
}
