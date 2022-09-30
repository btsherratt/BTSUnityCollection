using UnityEngine;

public class FadeUpForce : MonoBehaviour, CameraController.IFadeSource {
    public float m_fadeUpSpeed = 1.0f;
    public bool m_fadeDown;

    public bool m_ignore;

    void Start() {
        Camera.main.GetComponent<CameraController>().PushControlSource(this, true, 0);
    }

    public void SetupForCamera(Camera camera, bool transition) {
    }

    public float GetCameraFade(Camera camera, CameraFadeCurtain fadeCurtain) {
        float fade = fadeCurtain.m_fadeAmount;
        
        if (m_ignore != true) {
            float delta = m_fadeUpSpeed * (m_fadeDown ? Time.deltaTime : -Time.deltaTime);
            fade = Mathf.Clamp01(fadeCurtain.m_fadeAmount + delta);
        }

        return fade;
    }
}
