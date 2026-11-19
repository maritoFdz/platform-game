using UnityEngine;

[CreateAssetMenu(fileName = "New Narration Set", menuName = "Scriptable Objects/Narration Set")]
public class NarrationSet : ScriptableObject
{
    public NarrationEvent[] narrationEvents;

    public NarrationEvent GetNarrationEvent(NarrationEventType type, string eventId)
    {
        if (narrationEvents == null || narrationEvents.Length == 0) return null;

        foreach (NarrationEvent narrationEvent in narrationEvents)
            if (narrationEvent.type == type && narrationEvent.id == eventId && MeetsRequirements(narrationEvent)) return narrationEvent;
        return null;
    }

    private bool MeetsRequirements(NarrationEvent narrationEvent) // mausequerramienta que luego va a ir mezclada con el tracker de estadisticas, lineas de una sola vez y etc para los campos que faltan meter
    {
        return true;
    }
}
