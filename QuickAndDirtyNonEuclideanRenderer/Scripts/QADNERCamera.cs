using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QADNERCamera : MonoBehaviour {
    public static QADNERCamera Instance { get; private set; }

    Camera m_redrawCamera;
    Material m_mergeMaterial;
    List<QADNERPortal> m_visiblePortals = new List<QADNERPortal>();

    void Start() {
        Instance = this;

        GameObject redrawCameraHost = new GameObject("Redraw Camera");
        m_redrawCamera = redrawCameraHost.AddComponent<Camera>();
        m_redrawCamera.enabled = false;

        Shader mergeShader = Shader.Find("Hidden/QADNERPortalEffect");
        m_mergeMaterial = new Material(mergeShader);
    }

    public void EnqueuePortal(QADNERPortal portal) {
        m_visiblePortals.Add(portal);
    }

    private void OnPostRender() {
        RenderTexture tex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);

        Camera mainCamera = GetComponent<Camera>();
        foreach (QADNERPortal portal in m_visiblePortals) {
            portal.PortalRenderStart(this);
            m_redrawCamera.CopyFrom(mainCamera);
            m_redrawCamera.transform.position = portal.m_linkedPortal.transform.TransformPoint(portal.transform.InverseTransformPoint(mainCamera.transform.position));
            m_redrawCamera.transform.rotation = Quaternion.LookRotation(portal.m_linkedPortal.transform.TransformDirection(portal.transform.InverseTransformDirection(mainCamera.transform.forward)));
            //m_redrawCamera.nearClipPlane = Vector3.Distance(m_redrawCamera.transform.position, portal.m_linkedPortal.transform.position);
            m_redrawCamera.targetTexture = tex;
            m_redrawCamera.Render();
            portal.PortalRenderEnd(this);
        }

        m_visiblePortals.Clear();

        GL.PushMatrix();
        GL.LoadOrtho();
        
        m_mergeMaterial.SetPass(0);
        m_mergeMaterial.SetTexture("_MainTex", tex);
        GL.Begin(GL.QUADS);
        GL.TexCoord2(0, 0);
        GL.Vertex3(0, 0, 0);
        GL.TexCoord2(1, 0);
        GL.Vertex3(1, 0, 0);
        GL.TexCoord2(1, 1);
        GL.Vertex3(1, 1, 0);
        GL.TexCoord2(0, 1);
        GL.Vertex3(0, 1, 0);
        GL.End();

        GL.PopMatrix();

        RenderTexture.ReleaseTemporary(tex);
    }

    //private void OnPostRender(RenderTexture source, RenderTexture destination) {
    //  RenderTexture tmpTexture2 = RenderTexture.GetTemporary(source.width, source.height, 24, source.format);

        // foreach (QADNERPortal portal in m_visiblePortals) {
        //Graphics.SetRenderTarget(tmpTexture2.colorBuffer, tmpTexture2.depthBuffer);
        //     m_redrawCamera.transform.position = portal.m_linkedPortal.transform.position;
        //    m_redrawCamera.targetTexture = tmpTexture2;
        //   m_redrawCamera.forceIntoRenderTexture = true;
        //  m_redrawCamera.Render();
        //Graphics.SetRenderTarget(tmpTexture.colorBuffer, source.depthBuffer);
        //        }

        //RenderTexture buffer = RenderTexture.GetTemporary(source.width, source.height);

        //Graphics.SetRenderTarget(buffer.colorBuffer, source.depthBuffer);

        /*GL.PushMatrix();
        GL.LoadOrtho();

        // activate the first shader pass (in this case we know it is the only pass)
      //  m_mergeMaterial.SetPass(0);
        // draw a quad over whole screen
        GL.Begin(GL.QUADS);
        GL.Color(Color.red);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(1, 0, 0);
        GL.Vertex3(1, 1, 0);
        GL.Vertex3(0, 1, 0);
        GL.End();

        GL.PopMatrix();*/


        //Graphics.Blit(source, destination);

        //Graphics.Blit(source, buffer);
        //Graphics.Blit(source, buffer, m_mergeMaterial, -1, );
        // RenderTexture.ReleaseTemporary(tmpTexture2);

        //m_redrawCamera.CopyFrom(GetComponent<Camera>());

        //     m_visiblePortals.Clear();

        //Graphics.Blit(buffer, destination);
        //RenderTexture.ReleaseTemporary(buffer);
        //}
}
