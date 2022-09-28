using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveGameDataProviding {
    string SaveGameIdentifier { get; }

    void SaveGameApplySaveData(ISaveGameDataReading data);
    void SaveGameGatherSaveData(ISaveGameDataWriting data);
}

public interface IOrderedSaveGameDataProviding : ISaveGameDataProviding {
    int SaveGameOrder { get; }
}
