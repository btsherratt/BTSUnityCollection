using UnityEngine;

public class SetActiveTrigger : BaseTrigger {
    public GameObject m_target;

    public bool m_targetActive;

    protected override void DoTriggerAction() {
        m_target.SetActive(m_targetActive);
    }
}
