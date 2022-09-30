using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoWait {
    public delegate void TickCallback(float completion);

    public static IEnumerator Seconds(float time, TickCallback tickCallback = null) {
        float startTime = Time.time;
        float lastTime = 0.0f;
        for (;;) {
            float currentTime = Time.time;
            float delta = currentTime - startTime;
            float completion = Mathf.InverseLerp(0, time, delta);
            if (tickCallback != null) {
                tickCallback(completion);
            }
            if (delta > time) {
                if (lastTime != 1.0f && tickCallback != null) {
                    yield return null;
                    tickCallback(1.0f);
                }
                yield return null;
                break;
            }
            lastTime = delta;
            yield return null;
        }
    }

    public static IEnumerator Combine(IEnumerator first, IEnumerator second) {
        Stack<IEnumerator> firstEnumerators = new Stack<IEnumerator>();
        firstEnumerators.Push(first);

        Stack<IEnumerator> secondEnumerators = new Stack<IEnumerator>();
        secondEnumerators.Push(second);

        bool a, b;
        do {
            a = ProcessEnumerators(firstEnumerators);
            b = ProcessEnumerators(secondEnumerators);
            yield return null;
        } while (a || b);
    }

    static bool ProcessEnumerators(Stack<IEnumerator> enumerators) {
        if (enumerators.Count > 0) {
            IEnumerator enumerator = enumerators.Peek();
            if (enumerator.MoveNext()) {
                IEnumerator nestedEnumerator = enumerator.Current as IEnumerator;
                if (nestedEnumerator != null) {
                    enumerators.Push(nestedEnumerator);
                }
            } else {
                enumerators.Pop();
            }
        }

        return enumerators.Count > 0;
    }
}
