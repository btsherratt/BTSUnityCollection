using System;
using UnityEngine;

[Serializable]
public struct ProbabilityList<T> {
    [Serializable]
    public struct Item {
        public float weighting;
        public T value;
    }

    public Item[] m_items;

    public T RandomValue() {
        float count = 0;
        for (int i = 0; i < m_items.Length; ++i) {
            count += m_items[i].weighting;
        }

        float rnd = UnityEngine.Random.Range(0, count);
        for (int i = 0; i < m_items.Length; ++i) {
            rnd -= m_items[i].weighting;
            if (rnd <= 0) {
                return m_items[i].value;
            }
        }

        return m_items[0].value;
    }
}
