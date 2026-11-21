using System;
using UnityEngine;

public enum DialogueSelectionType { Single, Random }

[Serializable]
public class NarrationEvent
{
    public string id;
    public string[] dialogues_Ids;
    public DialogueSelectionType selectionMode;
    public EventClassification type; // realmente esto es pijeria pero bueno lol
}