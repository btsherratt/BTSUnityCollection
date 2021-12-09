using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionNotifier : MonoBehaviour, IInteractive {
    public delegate void InteractionDelegate(GameObject interactionObject);

    public InteractionDelegate m_interactionDelegate;

    void IInteractive.OnInteraction(GameObject interactingObject) {
        if (m_interactionDelegate != null) {
            m_interactionDelegate(gameObject);
        }
    }
}
