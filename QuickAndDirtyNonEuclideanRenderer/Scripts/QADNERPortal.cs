using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QADNERPortal : MonoBehaviour {
    static int ms_portalCount;
    static Material ms_portalMaterial;

    public QADNERPortal m_linkedPortal;

    public int m_portalID { get; private set; }

    //Material m_portalMaterial;
    MeshRenderer m_meshRenderer;

    void Start() {
        if (ms_portalMaterial == null) {
            Shader portalShader = Shader.Find("Hidden/QADNERPortal");
            ms_portalMaterial = new Material(portalShader);
        }

        ++ms_portalCount;
        m_portalID = ms_portalCount;

        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetFloat("_PortalID", (float)m_portalID);

        m_meshRenderer = GetComponent<MeshRenderer>();
        m_meshRenderer.material = new Material(Shader.Find("Hidden/QADNERPortal")); //ms_portalMaterial;
        m_meshRenderer.material.SetInt("_PortalID", m_portalID);
        //m_meshRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    void OnWillRenderObject() {
        m_meshRenderer.enabled = (m_linkedPortal != null);

        if (Camera.current.gameObject == QADNERCamera.Instance.gameObject && m_linkedPortal != null) {
            //Debug.Log("Door will render: " + this);
            QADNERCamera.Instance.EnqueuePortal(this, m_meshRenderer.bounds);

            //Bounds bounds = m_meshRenderer.bounds;
            //Vector3 tl = Camera.current.worldToCameraMatrix * bounds.min;
            //Vector3 br = Camera.current.worldToCameraMatrix * bounds.max;
            //bounds = Camera.current.worldToCameraMatrix * bounds;
            //Debug.Log($"{tl} .... {br}");
        }
    }

    public void PortalRenderStart(QADNERCamera camera) {
        //m_linkedPortal.m_meshRenderer.enabled = false;
    }

    public void PortalRenderEnd(QADNERCamera camera) {
        //m_linkedPortal.m_meshRenderer.enabled = true;
    }
}
