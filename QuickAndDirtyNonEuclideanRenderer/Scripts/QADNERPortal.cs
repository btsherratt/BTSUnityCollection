using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QADNERPortal : MonoBehaviour {
    public QADNERPortal m_linkedPortal;

    MeshRenderer m_meshRenderer;

    void Start() {
        m_meshRenderer = GetComponent<MeshRenderer>();
    }

    void OnWillRenderObject() {
        if (Camera.current.gameObject == QADNERCamera.Instance.gameObject) {
            Debug.Log("Door will render: " + this);
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
