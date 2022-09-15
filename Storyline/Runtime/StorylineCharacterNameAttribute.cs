using UnityEngine;

public class StorylineCharacterNameAttribute : PropertyAttribute {
    public interface IStorylineDataProviding {
        StorylineData StorylineData { get; }
    }
}
