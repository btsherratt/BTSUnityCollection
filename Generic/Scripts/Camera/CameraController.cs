using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public interface ISource {
        void SetupForCamera(Camera camera, bool transition);
    }

    public interface ISourceUpdating {
        void UpdateForCamera(Camera camera);
    }

    public interface IPositionSource : ISource {
        Vector3 GetCameraPosition(Camera camera);
    }

    public interface IRotationSource : ISource {
        Quaternion GetCameraRotation(Camera camera);
    }

    public interface IFOVSource : ISource {
        float GetCameraFOV(Camera camera);
    }

    public interface IFadeSource : ISource {
        float GetCameraFade(Camera camera, CameraFadeCurtain fadeCurtain);
    }

    public Camera m_camera;

    CameraFadeCurtain m_cameraFadeCurtain;
    Stack<ISource> m_controlStack = new Stack<ISource>();

    public AnimationCurve m_transitionCurve;
    Vector3 m_transitionStartPosition;
    Quaternion m_transitionStartRotation;
    float m_transitionStartTime;
    float m_transitionDuration;
    //public float m_transitionTime = 1.0f;

    void Start() {
        if (m_camera == null) {
            m_camera = GetComponent<Camera>();
        }
        if (m_camera == null) {
            m_camera = Camera.main;
        }
        m_cameraFadeCurtain = m_camera.GetComponent<CameraFadeCurtain>();
    }

    void LateUpdate() {
        foreach (ISource source in m_controlStack) {
            ISourceUpdating updatingSource = source as ISourceUpdating;
            if (updatingSource != null) {
                updatingSource.UpdateForCamera(m_camera);
                break;
            }
        }

        foreach (ISource source in m_controlStack) {
            IPositionSource positionSource = source as IPositionSource;
            if (positionSource != null) {
                ApplyPosition(positionSource.GetCameraPosition(m_camera));
                break;
            }
        }

        foreach (ISource source in m_controlStack) {
            IRotationSource rotationSource = source as IRotationSource;
            if (rotationSource != null) {
                ApplyRotation(rotationSource.GetCameraRotation(m_camera));
                break;
            }
        }

        foreach (ISource source in m_controlStack) {
            IFOVSource fovSource = source as IFOVSource;
            if (fovSource != null) {
                m_camera.fieldOfView = fovSource.GetCameraFOV(m_camera);
                break;
            }
        }

        if (m_cameraFadeCurtain != null) {
            foreach (ISource source in m_controlStack) {
                IFadeSource fadeSource = source as IFadeSource;
                if (fadeSource != null) {
                    m_cameraFadeCurtain.m_fadeAmount = fadeSource.GetCameraFade(m_camera, m_cameraFadeCurtain);
                    break;
                }
            }
        }
    }

    void ApplyPosition(Vector3 position) {
        Vector3 finalPosition = position;
        if (m_transitionDuration > 0) {
            float progress = Mathf.InverseLerp(m_transitionStartTime, m_transitionStartTime + m_transitionDuration, Time.time);
            float t = m_transitionCurve.Evaluate(progress);
            finalPosition = Vector3.Lerp(m_transitionStartPosition, position, t);
        }
        m_camera.transform.position = finalPosition;
    }

    void ApplyRotation(Quaternion rotation) {
        Quaternion finalRotation = rotation;
        if (m_transitionDuration > 0) {
            float progress = Mathf.InverseLerp(m_transitionStartTime, m_transitionStartTime + m_transitionDuration, Time.time);
            float t = m_transitionCurve.Evaluate(progress);
            finalRotation = Quaternion.Lerp(m_transitionStartRotation, rotation, t);
        }
        m_camera.transform.rotation = finalRotation;
    }

    public void PushControlSource(ISource source, bool transition, float transitionTime) {
        m_transitionStartPosition = m_camera.transform.position;
        m_transitionStartRotation = m_camera.transform.rotation;
        m_transitionStartTime = Time.time;
        m_transitionDuration = transitionTime;

        source.SetupForCamera(m_camera, transition);
        m_controlStack.Push(source);
    }

    public void PopControlSource(ISource source, bool transition, float transitionTime) {
        m_transitionStartPosition = m_camera.transform.position;
        m_transitionStartRotation = m_camera.transform.rotation;
        m_transitionStartTime = Time.time;
        m_transitionDuration = transitionTime;

        if (m_controlStack.Contains(source)) { // FIXME, this is pretty bad
            while (m_controlStack.Pop() != source) {
                if (m_controlStack.Count == 0) {
                    break;
                }
                m_controlStack.Peek().SetupForCamera(m_camera, transition);
            }
        }
    }

    public void PushAllControlSources(GameObject gameObject, bool transition, float transitionTime) {
        ISource[] cameraSources = gameObject.GetComponents<ISource>();
        foreach (ISource source in cameraSources) {
            PushControlSource(source, transition, transitionTime);
        }
    }

    public void PopAllControlSources(GameObject gameObject, bool transition, float transitionTime) {
        ISource[] cameraSources = gameObject.GetComponents<ISource>();
        foreach (ISource source in cameraSources) {
            PopControlSource(source, transition, transitionTime);
        }
    }
}
