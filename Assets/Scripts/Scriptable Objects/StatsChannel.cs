using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stats Channel", menuName = "Scriptable Objects/Stats Channel")]
public class StatsChannel : ScriptableObject
{
    public event Action<EventClassification> OnStatUpdated;
    public event Action OnLocalStatsReset;
    public event Action OnIdleUpdate;
    public event Action OnSplitChargeUpdate;

    public void UpdateStats(EventClassification stat)
    {
        OnStatUpdated?.Invoke(stat);
    }

    public void ResetLocalStats()
    {
        OnLocalStatsReset?.Invoke();
    }

    public void UpdateIdleTime()
    {
        OnIdleUpdate?.Invoke();
    }

    public void UpdateSplittingTime()
    {
        OnSplitChargeUpdate?.Invoke();
    }
}
