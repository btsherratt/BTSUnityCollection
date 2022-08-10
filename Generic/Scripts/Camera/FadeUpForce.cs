using UnityEngine;

public class FadeUpForce : MonoBehaviour, CameraController.IFadeSource {
    public float m_fadeUpSpeed = 1.0f;
    public bool m_fadeDown;

    void Start() {
        Camera.main.GetComponent<CameraController>().PushControlSource(this, true, 0);
    }

    public void SetupForCamera(Camera camera, bool transition) {
    }

    public float GetCameraFade(Camera camera, CameraFadeCurtain fadeCurtain) {
        float delta = m_fadeUpSpeed * (m_fadeDown ? Time.deltaTime : -Time.deltaTime);
        float fade = Mathf.Clamp01(fadeCurtain.m_fadeAmount + delta);
        return fade;
    }
}
