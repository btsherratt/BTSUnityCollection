using UnityEngine;

public class TransformSnapToTag : MonoBehaviour {
    [Tag]
    public string m_tagName;

    public bool m_snapOnStart;
    public bool m_handleCharacterController;

    void Start() {
        if (m_snapOnStart) {
            PerformSnapToTag();
        }
    }

    public void PerformSnapToTag() {
        CharacterController characterController = m_handleCharacterController ? GetComponent<CharacterController>() : null;

        if (characterController != null) {
            characterController.enabled = false;
        }

        GameObject taggedObject = GameObject.FindGameObjectWithTag(m_tagName);
        transform.Match(taggedObject.transform);

        if (characterController != null) {
            characterController.enabled = true;
        }
    }
}
