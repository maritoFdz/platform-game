using System;
using UnityEngine;

public enum NarrationEventType
{
    PlayerDeath,
    DyeEvent,
    EnterZone,
    CorrectPuzzleStep,
    WrongPuzzleStep,
    LongTimeIdle
}

public enum DialogueSelectionType { Single, Random }

[Serializable]
public class NarrationEvent
{
    public string id;
    public string[] dialogues_Ids;
    public DialogueSelectionType selectionMode;
    public NarrationEventType type; // realmente esto es pijeria pero bueno lol
}