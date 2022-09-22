using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StorylineDataExtensions {
    public static IEnumerable<string> CharacterNames(this StorylineData storylineData) {
        List<string> characterNames = new List<string>();
        for (int i = 0; i < storylineData.segments.Length; ++i) {
            StorylineData.Segment segment = storylineData.segments[i];
            if (characterNames.Contains(segment.characterName) == false) {
                characterNames.Add(segment.characterName);
                yield return segment.characterName;
            }
        }

    }
}
