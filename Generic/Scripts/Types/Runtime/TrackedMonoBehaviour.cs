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

    static public IEnumerable<T> All(bool filterActive = false) {
        if (ms_instances != null) {
            for (int i = 0; i < ms_currentInstanceCount; ++i) {
                T instance = ms_instances[i];

                while (instance == null) {
                    --ms_currentInstanceCount;

                    instance = ms_instances[ms_currentInstanceCount];
                    ms_instances[i] = instance;
                    ms_instances[ms_currentInstanceCount] = null;

                    if (i >= ms_currentInstanceCount) {
                        break;
                    }
                }

                if (i < ms_currentInstanceCount) {
                    if (filterActive == false || instance.isActiveAndEnabled) {
                        yield return instance;
                    }
                }
            }
        }
    }

    public TrackedMonoBehaviour() {
        RegisterInstance((T)this);
    }
}
