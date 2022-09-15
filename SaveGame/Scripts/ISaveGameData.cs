using System;
using UnityEngine;

public interface ISaveGameDataReading : IDisposable {
    bool ReadBoolean(string key, bool defaultValue);
    int ReadInteger(string key, int defaultValue);
    float ReadFloat(string key, float defaultValue);
    string ReadString(string key, string defaultValue);
    Vector4 ReadVector(string key, Vector4 defaultValue);
    Quaternion ReadQuaternion(string key, Quaternion defaultValue);
    ISaveGameDataReading ReadNestedGroup(string key);
}

public interface ISaveGameDataWriting : IDisposable {
    void WriteBoolean(string key, bool value);
    void WriteInteger(string key, int value);
    void WriteFloat(string key, float value);
    void WriteString(string key, string value);
    void WriteVector(string key, Vector4 value);
    void WriteQuaternion(string key, Quaternion value);
    ISaveGameDataWriting WriteNestedGroup(string key);
}
