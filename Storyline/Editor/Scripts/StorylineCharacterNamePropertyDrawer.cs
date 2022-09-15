using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(StorylineCharacterNameAttribute))]
public class StorylineCharacterNamePropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        Component component = property.serializedObject.targetObject as Component;
        StorylineCharacterNameAttribute.IStorylineDataProviding storylineDataProvider = component?.GetComponentInParent<StorylineCharacterNameAttribute.IStorylineDataProviding>();
        StorylineData storylineData = storylineDataProvider?.StorylineData;
        if (storylineData != null) {
            int selectionIdx = 0;
            
            List<string> characterNames = new List<string>();
            characterNames.Add("<NONE>");
            
            for (int i = 0; i < storylineData.segments.Length; ++i) {
                StorylineData.Segment segment = storylineData.segments[i];
                if (characterNames.Contains(segment.characterName) == false) {
                    characterNames.Add(segment.characterName);
                    if (property.stringValue == segment.characterName) {
                        selectionIdx = characterNames.Count - 1;
                    }
                }
            }

            int selectedIdx = EditorGUI.Popup(position, label.text, selectionIdx, characterNames.ToArray()); // FIXME, label

            property.stringValue = selectedIdx > 0 ? characterNames[selectedIdx] : null;
        } else {
            property.stringValue = EditorGUI.TextField(position, label, property.stringValue);
        }

        EditorGUI.EndProperty();

    }
}
