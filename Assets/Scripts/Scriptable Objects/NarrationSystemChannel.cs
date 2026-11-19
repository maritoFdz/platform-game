using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Narration Channel", menuName = "Scriptable Objects/Narration Channel")]
public class NarrationSystemChannel : ScriptableObject
{
    public event Action<NarrationEventType, string> OnNarrationTriggered;

    public void Raise(NarrationEventType type, string eventId)
    {
        OnNarrationTriggered?.Invoke(type, eventId);
    }
}
