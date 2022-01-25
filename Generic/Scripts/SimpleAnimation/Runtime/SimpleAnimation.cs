using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SimpleAnimation<T> {
    public interface LerpProviding {
        T LerpTo(T end, float t);
    }

    public enum AnimationType {
        Continuous,
        Loop,
        Bounce,
    };

    delegate T LerpDelegate(T from, T to, float t);

    public AnimationType m_type;
    public float m_duration;
    public T m_change;

    LerpDelegate m_lerpDelegate;

    public T Evaluate(float time) {
        //default(T).LerpTo(m_change, time);
        return m_lerpDelegate(default(T), m_change, time);
    }

    float FloatLerp(float start, float end, float t) {
        return Mathf.Lerp(start, end, t);
    }

    Vector3 Lerp(Vector3 start, Vector3 end, float t) {
        Debug.Log("B");
        return Vector3.Lerp(start, end, t);
    }

    Color Lerp(Color start, Color end, float t) {
        return Color.Lerp(start, end, t);
    }
}
