using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct SafeDisposable<T> : System.IDisposable where T : System.IDisposable {
    public static implicit operator T(SafeDisposable<T> r) {
        return r.Value;
    }

    public static implicit operator SafeDisposable<T>(T v) {
        return new SafeDisposable<T>(v);
    }

    [SerializeField]
    T m_value;

    public T Value {
        get => m_value;
        set {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(m_value, value) == false) {
                m_value.Dispose();
                m_value = value;
            }
        }
    }

    public SafeDisposable(T value) {
        m_value = value;

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
        m_value.Dispose();
        m_value = default(T);
    }
}
