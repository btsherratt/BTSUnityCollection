using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoWait {
    public delegate void TickCallback(float completion);

    public static IEnumerator Seconds(float time, TickCallback tickCallback = null) {
        float startTime = Time.time;
        for (;;) {
            float currentTime = Time.time;
            float delta = currentTime - startTime;
            float completion = Mathf.InverseLerp(0, time, delta);
            if (tickCallback != null) {
                tickCallback(completion);
            }
            if (delta > time) {
                break;
            }
            yield return null;
        }
    }
}
