using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    [Header("References")]
    [SerializeField] private CinemachineCamera[] roomCameras;
    [SerializeField] private CinemachineCamera startingCamera;
    private CinemachineCamera currentCamera;

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
    }
}
