using UnityEngine;

[RequireComponent (typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class NarrationZoneTrigger : MonoBehaviour
{
    [Header("Narration Settings")]
    [SerializeField] private NarrationSystemChannel narrationChannel;
    [SerializeField] private string zoneId;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerWithPlayer;
    [SerializeField] private string playerTag;
    [SerializeField] private Collider2D triggerObject;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (narrationChannel == null) return;
        if (triggerWithPlayer && collision.CompareTag(playerTag) || triggerObject != null && triggerObject == collision)
        {
            narrationChannel.Raise(EventClassification.EnterZone, zoneId);
        }
    }
}
