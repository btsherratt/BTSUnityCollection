using UnityEngine;

[CreateAssetMenu(menuName = "BTS/Control Schemes/FPS", fileName = "FPSControlScheme")]
public class FPSControlScheme : ScriptableObject {
    public enum InputMethod {
        Keyboard,
        Mouse,
    }

    [Header("Movement")]
    public KeyCode m_forward;
    public KeyCode m_backward;
    public KeyCode m_left;
    public KeyCode m_right;

    [Header("Interaction")]
    public InputMethod m_interactionInputMethod;
    public KeyCode m_interactionKey;
    public int m_interactionMouseButton;
}
