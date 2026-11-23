using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueDisplayer : MonoBehaviour
{
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private float displayTime;

    private Coroutine dialogueRoutine;
    private int currentDialoguePriority = int.MinValue;

    private void Start()
    {
        if (dialogueText != null) dialogueText.text = string.Empty;
    }

    public void DisplayDialog(string line, int priority)
    {
        if (currentDialoguePriority > priority) return;
        if (dialogueText != null) dialogueText.text = line;
        if (dialogueRoutine != null) StopCoroutine(dialogueRoutine);
        currentDialoguePriority = priority;
        dialogueRoutine = StartCoroutine(DisplayTextCo());
    }

    private IEnumerator DisplayTextCo()
    {
        yield return new WaitForSeconds(displayTime);
        dialogueRoutine = null;
        if(dialogueText != null) dialogueText.text = string.Empty;
        currentDialoguePriority = int.MinValue;
    }
}
