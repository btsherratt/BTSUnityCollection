using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class StorylineStateChecker {
    static IEnumerable<Dictionary<string, bool>> GenerateStates(List<string> variableNames, int startIdx = 0) {
        if (startIdx == variableNames.Count) {
            Dictionary<string, bool> d = new Dictionary<string, bool>();
            yield return d;
        } else {
            string variableName = variableNames[startIdx];
            foreach (Dictionary<string, bool> d in GenerateStates(variableNames, startIdx + 1)) {
                d[variableName] = false;
                yield return d;
                d[variableName] = true;
                yield return d;
            }
        }
    }


    [MenuItem("Assets/SKFX/Storyline/Check Integrity", false, 100)]
    static void CheckStoryline() {
        int errors = 0;
        foreach (Object obj in Selection.objects) {
            StorylineData storylineData = obj as StorylineData;
            if (storylineData != null) {
                Storyline storyline = new Storyline(storylineData);

                // If we do this with entire states, it takes forever because we talk to everyone twice...
                List<Storyline.State> seenStates = new List<Storyline.State>();
                foreach (var initialVariables in GenerateStates(new List<string>(storyline.VariableNames(true)))) {
                    Storyline.State initialState = new Storyline.State {
                        sequenceSeenCount = new Dictionary<string, int>(),
                        variableByName = initialVariables,
                    };
                    foreach (string variableName in storyline.VariableNames()) {
                        if (initialState.variableByName.ContainsKey(variableName) == false) {
                            // Set these explicitly so we don't keep adding things over and over to the list of unseen states (struct comparison)...
                            initialState.variableByName[variableName] = false;
                        }
                    }

                    Queue<Storyline.State> remainingStates = new Queue<Storyline.State>();
                    if (seenStates.Contains(initialState) == false) {
                        seenStates.Add(initialState);
                        remainingStates.Enqueue(initialState);
                    }

                    while (remainingStates.TryDequeue(out Storyline.State state)) {
                        foreach (string characterName in storylineData.CharacterNames()) {
                            var sequence = storyline.Sequence(characterName, state);
                            if (sequence.HasValue) {
                                var events = new List<Storyline.SequenceEvent>(sequence.Value.Events());
                                if (events.Count > 0) {
                                    Storyline.State newState = sequence.Value.finalState;
                                    if (seenStates.Contains(newState) == false) {
                                        seenStates.Add(newState);
                                        remainingStates.Enqueue(newState);
                                    }


                                    var followUp = storyline.Sequence(characterName, newState);
                                    if (followUp.HasValue == false) {
                                        Debug.LogError($"Missing events for follow-up sequence for character {characterName} in storyline {storylineData.name} with following state:");
                                        foreach (var pair in state.variableByName) {
                                            Debug.LogError($"\t{pair.Key} => {pair.Value}");
                                        }
                                        ++errors;
                                    }
                                } else {
                                    Debug.LogError($"Missing events for sequence for character {characterName} in storyline {storylineData.name} with following state:");
                                    foreach (var pair in state.variableByName) {
                                        Debug.LogError($"\t{pair.Key} => {pair.Value}");
                                    }
                                    ++errors;
                                }
                            } else {
                                Debug.LogError($"Missing sequence for character {characterName} in storyline {storylineData.name} with following state:");
                                foreach (var pair in state.variableByName) {
                                    Debug.LogError($"\t{pair.Key} => {pair.Value}");
                                }
                                ++errors;
                            }
                        }
                    }
                }

                //List<string> variableNames = new List<string>(storyline.VariableNames());


                //foreach (string characterName in storylineData.CharacterNames()) {


                /*foreach (string sequenceUUID in storyline.SequenceUUIDs()) {
                    state.sequenceSeenCount.Clear();
                    var firstTimeSequence = storyline.Sequence(characterName, state);
                    if (firstTimeSequence == null) {
                        Debug.LogError($"Missing first-time sequence for character {characterName} in storyline {storylineData.name} with following state:");
                        foreach (var pair in state.variableByName) {
                            Debug.LogError($"\t{pair.Key} => {pair.Value}");
                        }
                    }

                    state.sequenceSeenCount[sequenceUUID] = 1;
                    var secondTimeSequence = storyline.Sequence(characterName, state);
                    if (secondTimeSequence == null) {
                        Debug.LogError($"Missing second-time sequence for character {characterName} in storyline {storylineData.name} with following state:");
                        foreach (var pair in state.variableByName) {
                            Debug.LogError($"\t{pair.Key} => {pair.Value}");
                        }
                    }
                }*/
                //}





                //foreach (Dictionary<string, bool> d in GenerateStates(variableNames)) {
                //    state.variableByName = d;

                //    
                //}

                /*foreach ()

                storyline.VariableNames()
                storylineData.segments[0].variableNames

                state.sequenceSeenCount*/

                /*foreach (string characterName in storylineData.CharacterNames()) {




                    foreach () {

                    }
                }

                storylineData.segments[0].characterName
                storyline.
                */

                // ...
            }
        }

        if (errors == 0) {
            Debug.Log("No errors found! Good work!");
        } else {
            Debug.LogError($"Found {errors} potential error(s).");
        }
    }

    [MenuItem("Assets/SKFX/Storyline/Check Integrity", true)]
    static bool CheckStorylineValidate() {
        foreach (Object obj in Selection.objects) {
            StorylineData storylineData = obj as StorylineData;
            if (storylineData != null) {
                return true;
            }
        }
        return false;
    }
}
