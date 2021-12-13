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
        ReleaseRTs();
    }

    public void EnqueuePortal(QADNERPortal portal, Bounds bounds) {
        PortalDetails details = new PortalDetails();
        details.portal = portal;
        details.bounds = bounds;
        m_visiblePortals.Add(details);
    }

    void ReleaseRTs() {
        foreach (RenderTexture rt in m_renderTextures) {
            RenderTexture.ReleaseTemporary(rt);
        }

        m_renderTextures.Clear();
    }

    private void OnPostRender() {
        ReleaseRTs();

        Camera mainCamera = GetComponent<Camera>();

        RenderTexture tex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
        m_renderTextures.Add(tex);

        foreach (PortalDetails portalDetails in m_visiblePortals) {
            QADNERPortal portal = portalDetails.portal;

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

            /*if (portalDetails.bounds.Contains(m_redrawCamera.transform.position) == false) {
                Vector3[] frustumCorners = new Vector3[4];
                m_redrawCamera.CalculateFrustumCorners(mainCamera.rect, mainCamera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

                float minDistance = mainCamera.farClipPlane;
                for (int i = 0; i < frustumCorners.Length; ++i) {
                    Vector3 worldSpaceCorner = m_redrawCamera.transform.TransformVector(frustumCorners[i]);
                    Vector3 invertedNearest = portalDetails.portal.transform.TransformPoint(-portalDetails.portal.transform.InverseTransformPoint(worldSpaceCorner));
                    float distance = Vector3.Distance(worldSpaceCorner, portalDetails.bounds.ClosestPoint(invertedNearest));
                    minDistance = Mathf.Min(minDistance, distance);
                }

                m_redrawCamera.nearClipPlane = minDistance;
            }*/

            /*if (portalDetails.bounds.Contains(m_redrawCamera.transform.position) == false) {
                m_redrawCamera.nearClipPlane = Vector3.Distance(m_redrawCamera.transform.position, portalDetails.bounds.ClosestPoint(m_redrawCamera.transform.position));
            }*/

            //m_redrawCamera.rect = portalDetails.bounds.ToViewportRect(mainCamera);
            SetScissorRect(m_redrawCamera, portalDetails.bounds.ToViewportRect(mainCamera));
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


    // From https://forum.unity.com/threads/scissor-rectangle.37612/
    public static void SetScissorRect(Camera cam, Rect r) {
        if (r.x < 0) {
            r.width += r.x;
            r.x = 0;
        }

        if (r.y < 0) {
            r.height += r.y;
            r.y = 0;
        }

        r.width = Mathf.Min(1 - r.x, r.width);
        r.height = Mathf.Min(1 - r.y, r.height);
        //		print( r );

        cam.rect = new Rect(0, 0, 1, 1);
        cam.ResetProjectionMatrix();
        Matrix4x4 m = cam.projectionMatrix;
        //		print( cam.projectionMatrix );
        //		print( Mathf.Rad2Deg * Mathf.Atan( 1 / cam.projectionMatrix[ 0 ] ) * 2 );
        cam.rect = r;
        //		cam.projectionMatrix = m;
        //		print( cam.projectionMatrix );		
        //		print( Mathf.Rad2Deg * Mathf.Atan( 1 / cam.projectionMatrix[ 0 ] ) * 2 );
        //		print( cam.fieldOfView );
        //		print( Mathf.Tan( cam.projectionMatrix[ 1, 1 ] ) * 2 );
        //		cam.pixelRect = new Rect( 0, 0, Screen.width / 2, Screen.height );
        Matrix4x4 m1 = Matrix4x4.TRS(new Vector3(r.x, r.y, 0), Quaternion.identity, new Vector3(r.width, r.height, 1));
        //		Matrix4x4 m1 = Matrix4x4.TRS( Vector3.zero, Quaternion.identity, new Vector3( r.width, r.height, 1 ) );
        //		Matrix4x4 m2 = m1.inverse;
        //		print( m2 );
        Matrix4x4 m2 = Matrix4x4.TRS(new Vector3((1 / r.width - 1), (1 / r.height - 1), 0), Quaternion.identity, new Vector3(1 / r.width, 1 / r.height, 1));
        Matrix4x4 m3 = Matrix4x4.TRS(new Vector3(-r.x * 2 / r.width, -r.y * 2 / r.height, 0), Quaternion.identity, Vector3.one);
        //		m2[ 0, 3 ] = r.x;
        //		m2[ 1, 3 ] = r.y;
        //		print( m3 );
        //		print( cam.projectionMatrix );
        cam.projectionMatrix = m3 * m2 * m;
        //		print( cam.projectionMatrix );		
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
