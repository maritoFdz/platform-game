using System;
using System.Collections.Generic;
using UnityEngine;

public static class DialogueLoader
{
    public static Dictionary<string, string> LoadDialogues(TextAsset rawDialogues)
    {
        Dictionary<string, string> cookedDialogues = new();
        string rawTrimmedDialogues = rawDialogues.text.TrimStart('\uFEFF');
        string[] rawLines = rawTrimmedDialogues.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < rawLines.Length; i++)
        {
            string[] rawValues = rawLines[i].Split('\t');
            if (rawValues.Length != 2)
            {
                Debug.LogWarning("Papo estas en la boberia se te fue en: " + rawLines[i]);
                continue;
            }
            else
            {
                bool isValidLine = true;
                for (int j = 0; j < rawValues.Length; j++)
                {
                    if (string.IsNullOrWhiteSpace(rawValues[j]))
                    {
                        Debug.LogWarning("Papo estas en la boberia se te fue en: " + rawLines[i]);
                        isValidLine = false;
                        break;
                    }
                }
                if (!isValidLine) continue;
            }
            rawValues[0] = rawValues[0].Trim();
            if (cookedDialogues.ContainsKey(rawValues[0]))
            {
                Debug.LogWarning("Papo hay una repe en: " + rawLines[i]);
                continue;
            }
            cookedDialogues.Add(rawValues[0], rawValues[1]);
        }
        Debug.Log("Total: " + cookedDialogues.Count);
        return cookedDialogues;
    }
}
