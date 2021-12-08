using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QADNERCamera : MonoBehaviour {
    struct PortalDetails {
        public QADNERPortal portal;
        public Bounds bounds;
    }

    public static QADNERCamera Instance { get; private set; }

    Camera m_redrawCamera;
    Material m_mergeMaterial;
    List<PortalDetails> m_visiblePortals = new List<PortalDetails>();

    List<RenderTexture> m_renderTextures = new List<RenderTexture>();

    void Start() {
        Instance = this;

        GameObject redrawCameraHost = new GameObject("Redraw Camera");
        m_redrawCamera = redrawCameraHost.AddComponent<Camera>();
        m_redrawCamera.enabled = false;

        Shader mergeShader = Shader.Find("Hidden/QADNERPortalEffect");
        m_mergeMaterial = new Material(mergeShader);
    }

    private void OnDisable() {
        foreach (RenderTexture rt in m_renderTextures) {
            RenderTexture.ReleaseTemporary(rt);
        }
        m_renderTextures.Clear();
    }

    public void EnqueuePortal(QADNERPortal portal, Bounds bounds) {
        PortalDetails details = new PortalDetails();
        details.portal = portal;
        details.bounds = bounds;
        m_visiblePortals.Add(details);
    }

    private void OnPostRender() {
        foreach (RenderTexture rt in m_renderTextures) {
            RenderTexture.ReleaseTemporary(rt);
        }

        m_renderTextures.Clear();

        Camera mainCamera = GetComponent<Camera>();
        foreach (PortalDetails portalDetails in m_visiblePortals) {
            QADNERPortal portal = portalDetails.portal;

            RenderTexture tex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
            m_renderTextures.Add(tex);

            portal.PortalRenderStart(this);
            m_redrawCamera.CopyFrom(mainCamera);
            Quaternion inverseQuaternion = Quaternion.Euler(0, 180, 0);

            Vector3 cameraLocalPosition = portal.transform.InverseTransformPoint(mainCamera.transform.position);
            Vector3 invertedLocalPosition = inverseQuaternion * cameraLocalPosition;
            m_redrawCamera.transform.position = portal.m_linkedPortal.transform.TransformPoint(invertedLocalPosition);

            Vector3 cameraLocalForward = portal.transform.InverseTransformDirection(mainCamera.transform.forward);
            Vector3 invertedLocalForward = inverseQuaternion * cameraLocalForward;
            Vector3 invertedForward = portal.m_linkedPortal.transform.TransformDirection(invertedLocalForward);
            m_redrawCamera.transform.rotation = Quaternion.LookRotation(invertedForward);

            //if (portalDetails.bounds.Contains(m_redrawCamera.transform.position) == false) {
                //m_redrawCamera.nearClipPlane = Vector3.Distance(m_redrawCamera.transform.position, portalDetails.bounds.ClosestPoint(m_redrawCamera.transform.position)) - 10;
            //}

            m_redrawCamera.targetTexture = tex;
            m_redrawCamera.cullingMask = (1 << portal.m_linkedPortal.gameObject.layer);
            m_redrawCamera.Render();
            portal.PortalRenderEnd(this);

            GL.PushMatrix();
            GL.LoadOrtho();

            m_mergeMaterial.SetTexture("_MainTex", tex);
            m_mergeMaterial.SetInt("_PortalID", portal.m_portalID);
            m_mergeMaterial.SetPass(0);

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

            GL.Flush();

            //RenderTexture.ReleaseTemporary(tex);
        }

        m_visiblePortals.Clear();

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
