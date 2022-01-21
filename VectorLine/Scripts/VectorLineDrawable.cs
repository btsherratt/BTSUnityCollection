using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VectorLineDrawable : MonoBehaviour {
    public const int INVALID_ID = 0;

    const int MAX_INSTANCES = 1024;
    static VectorLineDrawable[] ms_instances;
    static int ms_currentInstanceCount;

    static void RegisterInstance(VectorLineDrawable instance) {
        if (ms_instances == null) {
            ms_instances = new VectorLineDrawable[MAX_INSTANCES];
            ms_currentInstanceCount = 0;
        }

        ms_instances[ms_currentInstanceCount] = instance;
        ++ms_currentInstanceCount;
    }

    static public IEnumerable<VectorLineDrawable> All(bool filterActive) {
        if (ms_instances != null) {
            for (int i = 0; i < ms_currentInstanceCount; ++i) {
                VectorLineDrawable drawable = ms_instances[i];

                while (drawable == null) {
                    --ms_currentInstanceCount;

                    drawable = ms_instances[ms_currentInstanceCount];
                    ms_instances[i] = drawable;
                    ms_instances[ms_currentInstanceCount] = null;

                    if (i >= ms_currentInstanceCount) {
                        break;
                    }
                }

                if (i < ms_currentInstanceCount) {
                    if (filterActive == false || drawable.isActiveAndEnabled) {
                        yield return drawable;
                    }
                }
            }
        }
    }

    public VectorLineDrawable() {
        RegisterInstance(this);
    }

    public abstract bool IsDirty { get; }

    public virtual int DrawableID => GetInstanceID();

    public abstract int VertexCount { get; }

    public abstract IEnumerable<VectorLineVertex> GetVertices();

    private void OnEnable() {
        // We just want the enable box to show up... Thanks... :)
    }
}
