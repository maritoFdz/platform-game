using System.Collections.Generic;
using UnityEngine;

public class DialogueContainer : MonoBehaviour
{
    public static DialogueContainer instance;

    [Header("References")]
    [SerializeField] private TextAsset dialoguesFile;

    private Dictionary<string, string> dialogues;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        if (dialoguesFile != null ) dialogues = DialogueLoader.LoadDialogues(dialoguesFile);
    }

    public string GetDialogue(string id)
    {
        if (dialogues == null) return null; // ya el que pida el texto hara la pincha de ver que hace con null
        dialogues.TryGetValue(id, out string line);
        return line;
    }
}