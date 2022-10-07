using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using ExcelDataReader;
using System.Security.Cryptography;
using System.Text;

[ScriptedImporter(1, "xlsx")]
public class ScriptSheetImporter : ScriptedImporter {
    static StorylineData.SequenceEvent[] EventsFromStoryData(string greetingsData, string dialogueData) {
        List<StorylineData.SequenceEvent> events = new List<StorylineData.SequenceEvent>();

        StorylineData.SequenceEvent greetingEvent = new StorylineData.SequenceEvent();
        greetingEvent.eventType = StorylineData.EventType.EventTypeDialogue;
        greetingEvent.dialogue = greetingsData;
        events.Add(greetingEvent);

        string[] dialogueDataLines = dialogueData.Split("\n");
        foreach(string line in dialogueDataLines) {
            if (line.StartsWith("//")) {
                // Comment, do nothing...
                //Debug.Log($"Skipping commented dialogue {line}");
            } else if (line.StartsWith("<") && line.EndsWith(">")) {
                StorylineData.SequenceEvent messageEvent = new StorylineData.SequenceEvent();
                messageEvent.eventType = StorylineData.EventType.EventTypeMessage;
                messageEvent.messageName = line.TrimStart('<').TrimEnd('>');
                events.Add(messageEvent);
            } else if (line.Length > 0) {
                StorylineData.SequenceEvent dialogueEvent = new StorylineData.SequenceEvent();
                dialogueEvent.eventType = StorylineData.EventType.EventTypeDialogue;
                dialogueEvent.dialogue = line;
                events.Add(dialogueEvent);
            }
        }

        return events.ToArray();
    }

    public override void OnImportAsset(AssetImportContext ctx) {
        FileStream fs = File.Open(ctx.assetPath, FileMode.Open, FileAccess.Read);
        IExcelDataReader dataReader = ExcelReaderFactory.CreateReader(fs);

        do {
            if (dataReader.VisibleState == "hidden") {
                continue;
            }
            
            List<StorylineData.Segment> segments = new List<StorylineData.Segment>();

            for (;;) {
                string characterName = null;

                while (characterName == null && dataReader.Read()) {
                    characterName = dataReader.GetString(0);
                }

                if (characterName != null) {
                    List<string> orderedVariableNames = new List<string>();
                    List<bool> gameControlledVariables = new List<bool>();
                    Dictionary<string, int> variableNameByCol = new Dictionary<string, int>();
                    int greetingCol = -1;
                    int secondGreetingCol = -1;
                    int dialogueCol = -1;
                    int secondDialogueCol = -1;

                    dataReader.Read();
                    for (int col = 0; col < dataReader.FieldCount; ++col) {
                        string parameterName = dataReader.GetString(col);
                        string parameterNameLower = parameterName?.ToLower();

                        if (parameterName == null || parameterNameLower == "location") {
                            continue;
                        } else if (parameterNameLower == "greeting") {
                            greetingCol = col;
                        } else if (parameterNameLower == "2nd greeting") {
                            secondGreetingCol = col;
                        } else if (parameterNameLower == "dialogue") {
                            dialogueCol = col;
                        } else if (parameterNameLower == "second-time dialogue") {
                            secondDialogueCol = col;
                        } else {
                            parameterName = parameterNameLower.Replace(" ", "_");

                            if (parameterName.StartsWith('<') && parameterName.EndsWith('>')) {
                                parameterName = parameterName.Trim('<', '>');
                                gameControlledVariables.Add(true);
                            } else {
                                gameControlledVariables.Add(false);
                            }

                            orderedVariableNames.Add(parameterName);
                            variableNameByCol[parameterName] = col;
                        }
                    }

                    if (dialogueCol == -1) {
                        Debug.LogError($"Didn't find dialogue {dataReader.Name}::{characterName}");
                    }

                    int idx = 0;
                    while (dataReader.Read()) {
                        ++idx;

                        string dialogue = dataReader.GetString(dialogueCol);

                        if (dialogue != null) {
                            StorylineData.Segment segment = new StorylineData.Segment();
                            segment.characterName = characterName;
                            segment.variableNames = orderedVariableNames.ToArray();
                            segment.variableIsGameControlled = gameControlledVariables.ToArray();

                            List<StorylineData.VariableValue> variableValues = new List<StorylineData.VariableValue>();
                            List<StorylineData.VariableChange> variableChanges = new List<StorylineData.VariableChange>();
                            foreach (string variableName in orderedVariableNames) {
                                int variableIdx = variableNameByCol[variableName];
                                string[] valueOrTransition = dataReader.GetString(variableIdx)?.Replace(" ", "")?.Split("=>");

                                StorylineData.VariableValue value = StorylineData.VariableValue.Any;
                                StorylineData.VariableChange change = StorylineData.VariableChange.None;

                                string valueString = valueOrTransition?.Length > 0 ? valueOrTransition[0]?.ToLower() : null;
                                if (valueString == "yes") {
                                    value = StorylineData.VariableValue.True;
                                } else if (valueString == "no") {
                                    value = StorylineData.VariableValue.False;
                                }

                                string changeString = valueOrTransition?.Length > 1 ? valueOrTransition[1]?.ToLower() : null;
                                if (changeString == "yes") {
                                    change = StorylineData.VariableChange.True;
                                } else if (changeString == "no") {
                                    change = StorylineData.VariableChange.False;
                                }

                                variableValues.Add(value);
                                variableChanges.Add(change);
                            }

                            segment.variableValues = variableValues.ToArray();
                            segment.variableChanges = variableChanges.ToArray();

                            string greeting = greetingCol >= 0 ? dataReader.GetString(greetingCol) : null;
                            string secondGreeting = secondGreetingCol >= 0 ? dataReader.GetString(secondGreetingCol) : null;
                            string secondDialogue = secondDialogueCol >= 0 ? dataReader.GetString(secondDialogueCol) : null;

                            StorylineData.Sequence primarySequence = new StorylineData.Sequence();
                            primarySequence.order = 0;
                            primarySequence.events = EventsFromStoryData(greeting, dialogue);

                            if (secondGreeting != null && secondDialogue != null) {
                                StorylineData.Sequence secondarySequence = new StorylineData.Sequence();
                                secondarySequence.order = 1;
                                secondarySequence.events = EventsFromStoryData(secondGreeting, secondDialogue);

                                segment.sequences = new StorylineData.Sequence[] { primarySequence, secondarySequence };
                            } else {
                                segment.sequences = new StorylineData.Sequence[] { primarySequence };
                            }

                            using (HashAlgorithm algorithm = SHA256.Create()) {
                                byte[] textData = System.Text.Encoding.UTF8.GetBytes($"{dataReader.Name}::{characterName}::{idx}");
                                byte[] hash = algorithm.ComputeHash(textData);

                                StringBuilder sb = new StringBuilder();
                                foreach (byte b in hash) {
                                    sb.Append(b.ToString("X2"));
                                }

                                segment.segmentUUID = sb.ToString().Substring(0, 16);
                            }

                            segments.Add(segment);
                        } else {
                            break;
                        }
                    }
                } else {
                    break;
                }
            }

            StorylineData storyline = ScriptableObject.CreateInstance<StorylineData>();
            storyline.name = dataReader.Name;
            storyline.segments = segments.ToArray();

            ctx.AddObjectToAsset(dataReader.Name, storyline);
        } while (dataReader.NextResult());

        fs.Close();

        GameObject gameObject = new GameObject("Storyline");
        ctx.AddObjectToAsset("GameObject", gameObject);
        ctx.SetMainObject(gameObject);

        /*using (var stream = File.Open(ctx.assetPath, FileMode.Open, FileAccess.Read)) {
            using (var reader = ExcelReaderFactory.CreateReader(stream)) {
                do {
                    while (reader.Read()) {
                        reader.get
                        Debug.Log(reader.GetString(0));
                    }
                } while (reader.NextResult());
            }
        }

        GameObject gameObject = new GameObject("Data");
        ctx.AddObjectToAsset("GameObject", gameObject);
        ctx.SetMainObject(gameObject);*/
    }
}