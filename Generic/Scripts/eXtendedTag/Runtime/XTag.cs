using System.Collections.Generic;
using UnityEngine;

public class XTag : TrackedMonoBehaviour<XTag> {
    public static IEnumerable<GameObject> Find(string tag, bool filterActive = false) {
        foreach (XTag xTag in All(filterActive)) {
            if (xTag.m_tag == tag) {
                yield return xTag.gameObject;
            }
        }
    }

    public string m_tag;
}
