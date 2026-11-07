using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [Header("References")]
    [SerializeField] private CinemachineCamera[] roomCameras;
    [SerializeField] private CinemachineCamera startingCamera;

    [Header("Parameters")]
    [SerializeField] private float defaultShakeForce;

    private CinemachineCamera currentCamera;
    private CinemachineImpulseListener currentImpulseListener;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        if (startingCamera == null) startingCamera = roomCameras[0];

        currentCamera = startingCamera;
        currentImpulseListener = currentCamera.gameObject.GetComponent<CinemachineImpulseListener>();

        foreach (var camera in roomCameras)
        {
            if (camera == startingCamera) camera.gameObject.SetActive(true);
            else camera.gameObject.SetActive(false);
        }
    }

    public void SwitchCamera(CinemachineCamera nextCam)
    {
        if (nextCam == null || nextCam == currentCamera) return;
        currentCamera.gameObject.SetActive(false);
        nextCam.gameObject.SetActive(true);
        currentCamera = nextCam;
        currentImpulseListener = currentCamera.GetComponent<CinemachineImpulseListener>();
    }

    public void ShakaCamera(CinemachineImpulseSource impulseSource)
    {
        impulseSource.GenerateImpulseWithForce(defaultShakeForce);
    }

    public void ShakaCameraWithParameters(CameraShakeParameters parameters, CinemachineImpulseSource impulseSource, float multiplier = 1f)
    {
        SetCameraShakeSettings(parameters, impulseSource, multiplier);
        impulseSource.GenerateImpulseWithForce(parameters.impactForce);
    }

    private void SetCameraShakeSettings(CameraShakeParameters parameters, CinemachineImpulseSource impulseSource, float multiplier)
    {
        impulseSource.ImpulseDefinition.ImpulseDuration = parameters.impactTime * multiplier;
        impulseSource.DefaultVelocity = parameters.velocity * multiplier;

        if (currentImpulseListener != null)
        {
            currentImpulseListener.ReactionSettings.AmplitudeGain = parameters.listenerAmplitude * multiplier;
            currentImpulseListener.ReactionSettings.FrequencyGain = parameters.listenerFrequency * multiplier;
            currentImpulseListener.ReactionSettings.Duration = parameters.listenerDuration * multiplier;
        }
    }
}
