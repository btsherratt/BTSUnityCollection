using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderReplacement : MonoBehaviour {
    [System.Serializable]
    public struct Replacement {
        public Shader m_shader;
        public string m_tag;
    }

    public Replacement[] m_replacements;

    void Start() {
        Camera camera = GetComponent<Camera>();
        foreach (Replacement replacement in m_replacements) {
            camera.SetReplacementShader(replacement.m_shader, replacement.m_tag);
        }
    }
}
