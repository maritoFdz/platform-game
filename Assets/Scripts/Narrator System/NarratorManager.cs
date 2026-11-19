using UnityEngine;

public class NarratorManager : MonoBehaviour
{
    public static NarratorManager instance;

    [Header("References")]
    [SerializeField] private NarrationSystemChannel narrationChannel;
    [SerializeField] private TesterNarrador tester;

    private NarrationSet[] narrationSets;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AsignNarrationSet(NarrationSet[] narrationSets)
    {
        this.narrationSets = narrationSets;
    }

    private void Narrate(NarrationEventType type, string eventId)
    {
        if (narrationSets == null) return;

        NarrationEvent narrationEvent = null;
        foreach (NarrationSet set in narrationSets)
        {
            narrationEvent = set.GetNarrationEvent(type, eventId);
            if (narrationEvent != null) break;
        }
        if (narrationEvent != null && narrationEvent.dialogues_Ids.Length > 0)
        {
            string id = string.Empty;
            switch (narrationEvent.selectionMode)
            {
                case DialogueSelectionType.Single:
                    id = narrationEvent.dialogues_Ids[0];
                    break;
                case DialogueSelectionType.Random:
                    id = PickRandomLine(narrationEvent.dialogues_Ids);
                    break;
            }
            if (DialogueContainer.instance != null)
                tester.TirarDialog(DialogueContainer.instance.GetDialogue(id));
        }
    }

    private string PickRandomLine(string[] lines)
    {
        int index = Random.Range(0, lines.Length);
        return lines[index];
    }

    private void OnEnable()
    {
        if (narrationChannel == null) return;
        narrationChannel.OnNarrationTriggered -= Narrate;
        narrationChannel.OnNarrationTriggered += Narrate;
    }

    private void OnDisable()
    {
        if (narrationChannel == null) return;
        narrationChannel.OnNarrationTriggered -= Narrate;
    }
}
