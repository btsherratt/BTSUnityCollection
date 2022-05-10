using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSave : MonoBehaviour, ISaveGameDataProviding {
    public string SaveGameIdentifier => "transform";

    public void SaveGameApplySaveData(ISaveGameDataReading data) {
        transform.position = data.ReadVector("position", transform.position);
        transform.rotation = data.ReadQuaternion("rotation", transform.rotation);
    }

    public void SaveGameGatherSaveData(ISaveGameDataWriting data) {
        data.WriteVector("position", transform.position);
        data.WriteQuaternion("rotation", transform.rotation);
    }
}
