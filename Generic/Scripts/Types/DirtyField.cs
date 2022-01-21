using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public partial struct DirtyField<T> where T : new() {
    public static implicit operator T(DirtyField<T> r) {
        return r.Value;
    }

    public static implicit operator DirtyField<T>(T v) {
        return new DirtyField<T>(v);
    }

    bool m_clean;
    public bool Dirty => !m_clean;

    [SerializeField]
    T m_value;

    public T Value {
        get => m_value;
        set {
            if (EqualityComparer<T>.Default.Equals(m_value, value) == false) {
                m_value = value;
                m_clean = false;
            }
        }
    }

    public DirtyField(T value) {
        m_value = value;
        m_clean = false;
    }

    public void MarkClean() {
        m_clean = true;
    }
}

#if UNITY_EDITOR

public interface IDirtyFieldPropertyDrawerExtensions {
    object EditedField();
}

public partial struct DirtyField<T> : IDirtyFieldPropertyDrawerExtensions where T : new() {
    public object EditedField() {
        return new DirtyField<T>();
    }
}

#endif
