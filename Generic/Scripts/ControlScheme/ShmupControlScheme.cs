using UnityEngine;

[CreateAssetMenu(menuName = "BTS/Control Schemes/Shmup", fileName = "ShmupControlScheme")]
public class ShmupControlScheme : ScriptableObject {
    [Header("Movement")]
    public KeyCode m_up;
    public KeyCode m_down;
    public KeyCode m_left;
    public KeyCode m_right;
}
