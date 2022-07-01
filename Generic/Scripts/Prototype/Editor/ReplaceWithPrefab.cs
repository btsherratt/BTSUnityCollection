using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ReplaceWithPrefab {
    class ScriptableWizardDisplayWizard : ScriptableWizard {
        public GameObject replacementPrefab;
        public GameObject[] objects;

        void OnWizardUpdate() {
            helpString = "Replaces objects with a prefab";
            if (replacementPrefab == false) {
                errorString = "Please assign a replacement prefab";
                isValid = false;
            } else {
                errorString = "";
                isValid = true;
            }
        }

        void OnWizardCreate() {
            foreach (GameObject gameObject in objects) {
                GameObject replacement = (GameObject)PrefabUtility.InstantiatePrefab(replacementPrefab);
                replacement.transform.Match(gameObject.transform);
                replacement.transform.SetParent(gameObject.transform.parent, true);
                GameObject.DestroyImmediate(gameObject);
            }
        }
    }

    [MenuItem("Tools/BTS/Replace")]
    static void CreateWindow() {
        var wizard = ScriptableWizard.DisplayWizard<ScriptableWizardDisplayWizard>("Replace", "Replace");
        wizard.objects = Selection.gameObjects;
    }
}
