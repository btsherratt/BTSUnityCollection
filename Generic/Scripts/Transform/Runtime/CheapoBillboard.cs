using UnityEngine;

public class CheapoBillboard : MonoBehaviour
{
    private void OnWillRenderObject() {
        transform.rotation = Camera.current.transform.rotation;
    }
}
