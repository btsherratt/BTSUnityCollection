using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractivePicker : MonoBehaviour {
    public FPSControlScheme m_controlScheme;

    [Layer]
    public int m_interactionLayer;

    public float m_interactionDistance = 3.0f;

    public Camera m_camera;

    public bool m_interactionAvailable { get; private set; }

    void Start() {
        if (m_camera == null) {
            m_camera = Camera.main;
        }
    }

    void Update() {
        Ray ray = new Ray(m_camera.transform.TransformPoint(m_camera.transform.forward * m_camera.nearClipPlane), m_camera.transform.forward);

        RaycastHit hit;
        m_interactionAvailable = Physics.Raycast(ray, out hit, m_interactionDistance, 1 << m_interactionLayer);

        bool didTryInteract = false;
        switch (m_controlScheme.m_interactionInputMethod) {
            case FPSControlScheme.InputMethod.Keyboard:
                didTryInteract = Input.GetKeyDown(m_controlScheme.m_interactionKey);
                break;

            case FPSControlScheme.InputMethod.Mouse:
                didTryInteract = Input.GetMouseButtonDown(m_controlScheme.m_interactionMouseButton);
                break;
        }

        if (m_interactionAvailable && didTryInteract) {
            foreach (IInteractive interactiveObject in hit.collider.GetComponentsInParent<IInteractive>()) {
                interactiveObject.OnInteraction(gameObject);
            }
        }
    }
}
