using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorylineData : ScriptableObject {
    public enum VariableValue {
        Any,
        False,
        True
    }

    public enum VariableChange {
        None,
        False,
        True
    }

    public enum EventType {
        EventTypeDialogue,
        EventTypeMessage,
    }

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
        public bool[] variableIsGameControlled;
        public VariableValue[] variableValues;
        public VariableChange[] variableChanges;
        public Sequence[] sequences;
    }

    public Segment[] segments;
}
