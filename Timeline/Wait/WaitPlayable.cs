using UnityEngine;
using UnityEngine.Playables;

public class WaitPlayable : PlayableBehaviour {
    PlayableGraph m_graph;
    bool m_fired;
    IWaitEventProviding m_eventProvider;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
        float t = (float)(playable.GetTime() / playable.GetDuration());

        GameObject trackBinding = playerData as GameObject;

        if (m_fired == false) {
            m_graph = playable.GetGraph();
            m_graph.Stop();
            m_eventProvider = trackBinding.GetComponent<IWaitEventProviding>();
            m_eventProvider.WaitEvent += Unpause;
            m_fired = true;
        }

    }

    void Unpause() {
        m_eventProvider.WaitEvent -= Unpause;
        m_graph.Play();
    }
}
