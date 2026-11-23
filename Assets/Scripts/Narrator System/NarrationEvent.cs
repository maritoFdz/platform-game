using System;
using UnityEngine;

public enum DialogueSelectionType { Single, Random }

[Serializable]
public class NarrationEvent
{
    public string id;
    public string[] dialogues_Ids;
    public DialogueSelectionType selectionMode;
    public EventClassification type; // yo fui un visionario definitivamente
    public int preference;
    public int displayPriority;

    [Header("Statistics")]
    public bool usesStatistics;
    public bool useGlobal;

    [Header("Statistics Min")] // que no se note que no se usar los editores del inspector
    public bool needsMin;
    public int minValue;

    [Header("Statistics Max")]
    public bool needsMax;
    public int maxValue;
}