using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QADNERPortal : MonoBehaviour {
    static Material ms_portalMaterial;

    public QADNERPortal m_linkedPortal;

    MeshRenderer m_meshRenderer;

    void Start() {
        if (ms_portalMaterial == null) {
            Shader portalShader = Shader.Find("Hidden/QADNERPortal");
            ms_portalMaterial = new Material(portalShader);
        }

        m_meshRenderer = GetComponent<MeshRenderer>();
        m_meshRenderer.material = ms_portalMaterial;
    }

    void OnWillRenderObject() {
        m_meshRenderer.enabled = (m_linkedPortal != null);

        if (Camera.current.gameObject == QADNERCamera.Instance.gameObject && m_linkedPortal != null) {
            //Debug.Log("Door will render: " + this);
            QADNERCamera.Instance.EnqueuePortal(this);
        }
    }

    public void PortalRenderStart(QADNERCamera camera) {
        //m_linkedPortal.m_meshRenderer.enabled = false;
    }

    public void PortalRenderEnd(QADNERCamera camera) {
        //m_linkedPortal.m_meshRenderer.enabled = true;
    }
}
