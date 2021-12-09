using UnityEngine;

public class TriggerDelegate : MonoBehaviour {
    public delegate void TriggerEnterDelegate(Collider trigger, Collider other);
    public delegate void TriggerExitDelegate(Collider trigger, Collider other);
    public delegate void TriggerStayDelegate(Collider trigger, Collider other);

    public TriggerEnterDelegate m_triggerEnterDelegate;
    public TriggerExitDelegate m_triggerExitDelegate;
    public TriggerStayDelegate m_triggerStayDelegate;

    Collider m_collider;

    void Start() {
        m_collider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other) {
        if (m_triggerEnterDelegate != null) {
            m_triggerEnterDelegate(m_collider, other);
        }
    }

    void OnTriggerExit(Collider other) {
        if (m_triggerExitDelegate != null) {
            m_triggerExitDelegate(m_collider, other);
        }
    }

    void OnTriggerStay(Collider other) {
        if (m_triggerStayDelegate != null) {
            m_triggerStayDelegate(m_collider, other);
        }
    }
}
