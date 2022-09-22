using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storyline {
    public struct State {
        public Dictionary<string, int> sequenceSeenCount;
        public Dictionary<string, bool> variableByName;

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            State otherState = (State)obj;

        //    foreach (var pair in sequenceSeenCount) {
        //        if (otherState.sequenceSeenCount[pair.Key] != pair.Value) {
        //            return false;
        //        }
        //    }

            foreach (var pair in variableByName) {
                if (otherState.variableByName.TryGetValue(pair.Key, out bool value) && value != pair.Value) {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode() {
            return sequenceSeenCount.GetHashCode() ^ variableByName.GetHashCode();
        }
    }

    public struct SequenceEvent {
        public enum Type {
            Dialogue,
            SendMessage,
        }

        public Type type;
        public string dialogue;
        public string messageName;

        public SequenceEvent(Type type, string dialogue, string messageName) {
            this.type = type;
            this.dialogue = dialogue;
            this.messageName = messageName;
        }
    }

    public struct SequenceDetails {
        public State finalState;
        public StorylineData.Sequence sequence;

        public IEnumerable<SequenceEvent> Events() {
            foreach (StorylineData.SequenceEvent e in sequence.events) {
                switch (e.eventType) {
                    case StorylineData.EventType.EventTypeDialogue:
                        yield return new SequenceEvent(SequenceEvent.Type.Dialogue, e.dialogue, null);
                        break;

                    case StorylineData.EventType.EventTypeMessage:
                        yield return new SequenceEvent(SequenceEvent.Type.SendMessage, null, e.messageName);
                        break;

                    default:
                        // Do nothing.
                        break;
                }
            }
        }
    }

    StorylineData m_data;

    public Storyline(StorylineData data) {
        m_data = data;
    }

    public IEnumerable<string> SequenceUUIDs() {
        foreach (StorylineData.Segment segment in m_data.segments) {
            yield return segment.segmentUUID;
        }
    }

    public IEnumerable<string> VariableNames(bool onlyIncludeGameControlled = false) {
        List<string> variableNames = new List<string>();
        foreach (StorylineData.Segment segment in m_data.segments) {
            for (int i = 0; i < segment.variableNames.Length; ++i) {
                string variableName = segment.variableNames[i];
                bool gameControlled = segment.variableIsGameControlled[i];
                if (variableNames.Contains(variableName) == false && (onlyIncludeGameControlled == false || gameControlled)) {
                    yield return variableName;
                    variableNames.Add(variableName);
                }
            }
        }
    }

    public IEnumerable<string> GameChangedVariableNames() {
        List<string> variableNames = new List<string>();
        foreach (StorylineData.Segment segment in m_data.segments) {
            foreach (string variableName in segment.variableNames) {
                if (variableNames.Contains(variableName) == false) {
                    yield return variableName;
                    variableNames.Add(variableName);
                }
            }
        }
    }

    public SequenceDetails? Sequence(string characterName, State inState) {
        StorylineData.Segment? bestMatchSegment = null;
        foreach (StorylineData.Segment segment in m_data.segments) {
            if (segment.characterName == characterName && ValidSegmentState(segment, inState)) {
                bestMatchSegment = segment;
            }
        }
        if (bestMatchSegment != null) {
            StorylineData.Segment segment = bestMatchSegment.Value;

            int entryCount;
            if (inState.sequenceSeenCount.TryGetValue(segment.segmentUUID, out entryCount) == false) {
                entryCount = 0;
            }

            for (int i = segment.sequences.Length - 1; i >= 0; --i) {
                StorylineData.Sequence s = segment.sequences[i];
                if (s.order <= entryCount) {
                    Dictionary<string, bool> variables = new Dictionary<string, bool>(inState.variableByName);
                    for (int j = 0; j < segment.variableNames.Length; ++j) {
                        string variableName = segment.variableNames[j];
                        StorylineData.VariableChange variableChange = segment.variableChanges[j];
                        switch (variableChange) {
                            case StorylineData.VariableChange.False:
                                variables[variableName] = false;
                                break;

                            case StorylineData.VariableChange.True:
                                variables[variableName] = true;
                                break;

                            case StorylineData.VariableChange.None:
                            default:
                                // Nothing
                                break;
                        }
                    }

                    Dictionary<string, int> sequenceSeenCount = new Dictionary<string, int>(inState.sequenceSeenCount);
                    sequenceSeenCount[segment.segmentUUID] = entryCount + 1;

                    return new SequenceDetails {
                        finalState = new State {
                            sequenceSeenCount = sequenceSeenCount,
                            variableByName = variables,
                        },
                        sequence = s
                    };
                }
            }
        }

        return null;
    }

    bool ValidSegmentState(StorylineData.Segment segment, State state) {
        for (int i = 0; i < segment.variableNames.Length; ++i) {
            string variableName = segment.variableNames[i];
            StorylineData.VariableValue variableValue = segment.variableValues[i];
            switch (variableValue) {
                case StorylineData.VariableValue.False: {
                    if (state.variableByName.TryGetValue(variableName, out bool value)) {
                        // Null values will count as false in this.
                        if (value == true) {
                            return false;
                        }
                    }
                    break;
                }

                case StorylineData.VariableValue.True: {
                    if (state.variableByName.TryGetValue(variableName, out bool value)) {
                        if (value == false) {
                            return false;
                        }
                    } else {
                        // Null values will count as unset in this.
                        return false;
                    }
                    break;
                }

                case StorylineData.VariableValue.Any:
                default:
                    /* Free */
                    break;
            }
        }

        return true;
    }
}
