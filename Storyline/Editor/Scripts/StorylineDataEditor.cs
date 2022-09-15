using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(StorylineData))]
public class StorylineDataEditor : Editor {
    StorylineData StorylineData => (StorylineData)target;

    public override void OnInspectorGUI() {
        foreach (StorylineData.Segment segment in StorylineData.segments) {
            //            GUILayout.
            GUILayout.Label(segment.characterName);
            
            for (int i = 0; i < segment.variableNames.Length; ++i) {
                if (segment.variableValues[i] != StorylineData.VariableValue.Any) {
                    string expectedValue = segment.variableValues[i] == StorylineData.VariableValue.True ? "YES" : "NO";
                    GUILayout.Label($"{segment.variableNames[i]} == {expectedValue}");
                }
            }

//            EditorGUILayout.text
        }

        /*
        StorylineData.SequenceEvent







    [System.Serializable]
    public struct SequenceEvent {
        public EventType eventType;
        public string dialogue;
        public string messageName;
    }

    [System.Serializable]
    public struct Sequence {
        public int order;
        public SequenceEvent[] events;
    }

    [System.Serializable]
    public struct Segment {
        public string segmentUUID;
        public string characterName;
        public string[] variableNames;
        public VariableValue[] variableValues;
        public VariableChange[] variableChanges;
        public Sequence[] sequences;
    }

    public Segment[] segments;*/
}
}
