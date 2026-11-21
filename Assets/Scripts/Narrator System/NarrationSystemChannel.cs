using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Narration Channel", menuName = "Scriptable Objects/Narration Channel")]
public class NarrationSystemChannel : ScriptableObject
{
    public event Action<EventClassification, string> OnNarrationTriggered;

    public void Raise(EventClassification type, string eventId)
    {
        OnNarrationTriggered?.Invoke(type, eventId);
    }
}
