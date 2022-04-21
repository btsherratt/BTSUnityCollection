using UnityEngine;

public class TriggerDelegate : MonoBehaviour {
    public delegate void TriggerEnterDelegate(Collider trigger, Collider other);
    public delegate void TriggerExitDelegate(Collider trigger, Collider other);
    public delegate void TriggerStayDelegate(Collider trigger, Collider other);

    public event TriggerEnterDelegate m_triggerEnterDelegate;
    public event TriggerExitDelegate m_triggerExitDelegate;
    public event TriggerStayDelegate m_triggerStayDelegate;

    public Collider m_collider { get; private set; }

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
