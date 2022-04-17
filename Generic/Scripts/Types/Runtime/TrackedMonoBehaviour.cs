using System.Collections.Generic;
using UnityEngine;

// This one is basically a stolen idea... Thanks Chris.
public class TrackedMonoBehaviour<T> : MonoBehaviour where T : TrackedMonoBehaviour<T> {
    const int MAX_INSTANCES = 1024;

    static T[] ms_instances;
    static int ms_currentInstanceCount;

    static void RegisterInstance(T instance) {
        if (ms_instances == null) {
            ms_instances = new T[MAX_INSTANCES];
            ms_currentInstanceCount = 0;
        }

        ms_instances[ms_currentInstanceCount] = instance;
        ++ms_currentInstanceCount;
    }

    static T GetInstance(int idx) {
        T instance = ms_instances[idx];

        while (instance == null) {
            --ms_currentInstanceCount;

            instance = ms_instances[ms_currentInstanceCount];
            ms_instances[idx] = instance;
            ms_instances[ms_currentInstanceCount] = null;

            if (idx >= ms_currentInstanceCount) {
                break;
            }
        }

        if (idx >= ms_currentInstanceCount) {
            instance = null;
        }

#if UNITY_EDITOR
        // Helper to get rid of prefabs that have poisoned the pool
        if (instance != null && instance.gameObject.scene.rootCount == 0) {
            instance = null;
            ms_instances[idx] = null;
        }
#endif

        return instance;
    }

    public static IEnumerable<T> All(bool filterActive = false) {
        if (ms_instances != null) {
            for (int i = 0; i < ms_currentInstanceCount; ++i) {
                T instance = GetInstance(i);
                if (instance != null && (filterActive == false || instance.isActiveAndEnabled)) {
                    yield return instance;
                }
            }
        }
    }

    public static int Populate(T[] outArray, bool filterActive = false) {
        int populatedCount = 0;

        if (ms_instances != null) {
            for (int i = 0; i < ms_currentInstanceCount; ++i) {
                T instance = GetInstance(i);
                if (instance != null && (filterActive == false || instance.isActiveAndEnabled)) {
                    outArray[populatedCount] = instance;
                    populatedCount += 1;
                }
            }
        }

        return populatedCount;
    }

    public static T Nearest(Vector3 position, bool filterActive = false, ComponentExtensions.FilterDelegate<T> filter = null) {
        return ComponentExtensions.Nearest(All(filterActive), position, filter);
    }

    public TrackedMonoBehaviour() {
        RegisterInstance((T)this);
    }
}
