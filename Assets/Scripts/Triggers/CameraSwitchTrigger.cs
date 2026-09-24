using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class CameraSwitchTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera target;

    [Header("Parameters")]
    [SerializeField] private float switchSpeed;
    [SerializeField] private string[] activationTags;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (CameraManager.instance == null) return;
        foreach (var tag in activationTags)
            if (collision.gameObject.CompareTag(tag))
                Activate();
    }
    public void Activate()
    {
        if (CameraManager.instance == null) return;
        CameraManager.instance.SwitchCamera(target);
    }
}
