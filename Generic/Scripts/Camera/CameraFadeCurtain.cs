using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFadeCurtain : MonoBehaviour {
    public Color m_fadeColor = Color.black;

    [Range(0.0f, 1.0f)]
    public float m_fadeAmount = 0.0f;

    Material m_material;

    void Start() {
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        m_material = new Material(shader);
        m_material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m_material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m_material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        m_material.SetInt("_ZWrite", (int)UnityEngine.Rendering.CompareFunction.Disabled);
        m_material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
    }

    public void OnPostRender() {
        if (m_fadeAmount > 0.0f) {
            GL.PushMatrix();
            GL.LoadOrtho();

            m_material.SetPass(0);

            GL.Begin(GL.QUADS);

            GL.Color(Color.Lerp(Color.clear, m_fadeColor, m_fadeAmount));

            GL.Vertex3(0, 0, 0);
            GL.Vertex3(1, 0, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(0, 1, 0);
            GL.End();

            GL.PopMatrix();
        }
    }
}
