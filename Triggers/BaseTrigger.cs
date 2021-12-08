using UnityEngine;

public abstract class BaseTrigger : MonoBehaviour {
    public enum TriggerPhase {
        OnEnter,
        OnExit,
    }
    
    public TriggerDelegate m_trigger;

    public TriggerPhase m_triggerPhase;

    void Start() {
        if (m_trigger == null) {
            m_trigger = GetComponent<TriggerDelegate>();

            if (m_trigger == null) {
                m_trigger = gameObject.AddComponent<TriggerDelegate>();
            }
        }

        switch (m_triggerPhase) {
            case TriggerPhase.OnEnter:
                m_trigger.m_triggerEnterDelegate += PerformAction;
                break;

            case TriggerPhase.OnExit:
                m_trigger.m_triggerExitDelegate += PerformAction;
                break;
        }
    }

    void PerformAction(Collider trigger, Collider other) {
        DoTriggerAction();
    }

    protected abstract void DoTriggerAction();
}
