#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public class EditorExtraCallbacks {
    public delegate void ReloadScriptsCallback();

    public static event ReloadScriptsCallback onReloadScripts;

    [UnityEditor.Callbacks.DidReloadScripts]
    public static void OnReloadScripts() {
        if (onReloadScripts != null) {
            onReloadScripts();
        }
    }
}

#endif
