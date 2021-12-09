using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class MatchTransformClip : PlayableAsset {
    public ExposedReference<Transform> m_matchTransform;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
        ScriptPlayable<MatchTransformPlayable> playable = ScriptPlayable<MatchTransformPlayable>.Create(graph);
        MatchTransformPlayable matchTransformPlayable = playable.GetBehaviour();

        matchTransformPlayable.m_matchTransform = m_matchTransform.Resolve(graph.GetResolver());

        return playable;
    }
}