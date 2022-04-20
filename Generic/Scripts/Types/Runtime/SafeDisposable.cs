using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SafeDisposable<T> : System.IDisposable where T : System.IDisposable {
    public static implicit operator T(SafeDisposable<T> r) {
        return r.Value;
    }

    public static implicit operator SafeDisposable<T>(T v) {
        return new SafeDisposable<T>(v);
    }

    [SerializeField]
    T m_value;

    bool m_disposed;

    public T Value {
        get => m_value;
        set {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(m_value, value) == false) {
                Dispose();
                m_value = value;
                m_disposed = false;
            }
        }
    }

    public SafeDisposable(T value) {
        m_value = value;
        m_disposed = false;

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += DisposeOnStateChange;
#endif
    }

#if UNITY_EDITOR
    private void DisposeOnStateChange(PlayModeStateChange state) {
        switch (state) { 
            case PlayModeStateChange.ExitingEditMode:
            case PlayModeStateChange.ExitingPlayMode:
                Dispose();
                break;

            default:
                // Do nothing
                break;
        }
    }
#endif

    public void Dispose() {
        if (m_disposed == false) {
            m_disposed = true;
            m_value.Dispose();
        }
    }
}
