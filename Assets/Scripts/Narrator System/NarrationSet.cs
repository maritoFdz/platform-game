using UnityEngine;

[CreateAssetMenu(fileName = "New Narration Set", menuName = "Scriptable Objects/Narration Set")]
public class NarrationSet : ScriptableObject
{
    public NarrationEvent[] narrationEvents;

    public NarrationEvent GetNarrationEvent(EventClassification type, string eventId)
    {
        if (narrationEvents == null || narrationEvents.Length == 0) return null;

        NarrationEvent eventReturned = null;
        foreach (NarrationEvent narrationEvent in narrationEvents)
            if (narrationEvent.type == type && narrationEvent.id == eventId && MeetsRequirements(narrationEvent))
            {
                if (eventReturned == null || eventReturned.preference < narrationEvent.preference)
                    eventReturned = narrationEvent;
            }
        return eventReturned;
    }

    private bool MeetsRequirements(NarrationEvent narrationEvent)
    {
        if (!narrationEvent.usesStatistics) return true;
        if (StatsTracker.instance == null) return false;

        int val = (narrationEvent.useGlobal) ? StatsTracker.instance.GetGlobal(narrationEvent.type) : StatsTracker.instance.GetLocal(narrationEvent.type);
        if (narrationEvent.needsMin && val < narrationEvent.minValue) return false;
        if (narrationEvent.needsMax && val > narrationEvent.maxValue) return false;
        return true;
    }
}
