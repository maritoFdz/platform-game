using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stats Channel", menuName = "Scriptable Objects/Stats Channel")]
public class StatsChannel : ScriptableObject
{
    public event Action<EventClassification> OnStatUpdated;
    public event Action OnLocalStatsReset;
    public event Action<EventClassification> OnEventOcurred;

    public void UpdateStats(EventClassification stat)
    {
        OnStatUpdated?.Invoke(stat);
    }

    public void ResetLocalStats()
    {
        OnLocalStatsReset?.Invoke();
    }

    public void RaiseEvent(EventClassification eventOcurred)
    {
        OnEventOcurred?.Invoke(eventOcurred);
    }
}
