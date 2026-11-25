using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;


public class DialogueDisplayer : MonoBehaviour
{
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private float displayTime;
    [SerializeField] private float typeDelay;
    [SerializeField] private float defaultAnimationsValue;

    private static readonly Regex tagsRegex = new(@"<(?<name>p|s):(?<par>[^<>/]*)/?>|<(?<close>/)?(?<name>a):(?<par>[^<>/]+)>");
    private static readonly Regex TMPRegex = new(@"<[^>]*>");
    private readonly List<TextCommand> commands = new();
    private static readonly Dictionary<string, ITextAnimation> animations = new(StringComparer.OrdinalIgnoreCase)
        {
            { "shake", new ShakeAnimation() },
            { "wave",  new WaveAnimation() },
        };

    private Coroutine dialogueRoutine;
    private int currentDialoguePriority = int.MinValue;


    private void Start()
    {
        if (dialogueText != null) dialogueText.text = string.Empty;
    }

    public void DisplayDialog(string line, int priority)
    {
        if (currentDialoguePriority > priority) return;
        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.text = Parse(line);
        }
        if (dialogueRoutine != null) StopCoroutine(dialogueRoutine);
        currentDialoguePriority = priority;
        dialogueRoutine = StartCoroutine(DisplayTextCo());
    }

    private string Parse(string rawText)
    {
        commands.Clear();
        StringBuilder bbqText = new();
        int lastEnd = 0;
        int visibleCharIndex = 0;
        HashSet<string> pendAnims = new(StringComparer.OrdinalIgnoreCase);
        MatchCollection matches = tagsRegex.Matches(rawText);
        foreach (Match match in matches)
        {
            string piece = rawText[lastEnd..match.Index];
            bbqText.Append(piece);
            lastEnd = match.Index + match.Length;
            visibleCharIndex += CountVisibleChars(piece);
            switch (match.Groups["name"].Value)
            {
                case "p":
                    if (float.TryParse(match.Groups["par"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float pause) && pause >= 0f)
                        commands.Add(new TextCommand(visibleCharIndex, TextCommandType.Pause, pause));
                    break;
                case "s":
                    if (float.TryParse(match.Groups["par"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float speed) && speed > 0f)
                        commands.Add(new TextCommand(visibleCharIndex, TextCommandType.TextSpeedChange, speed));
                    break;
                case "a":
                    string animationType = match.Groups["par"].Value;
                    if (!string.IsNullOrEmpty(animationType))
                    {
                        if (string.IsNullOrEmpty(match.Groups["close"].Value))
                        {
                            if (animations.TryGetValue(animationType, out ITextAnimation animationObj) && !pendAnims.Contains(animationType))
                            {
                                commands.Add(new TextCommand(visibleCharIndex, TextCommandType.AnimStart, match.Groups["par"].Value, animationObj));
                                pendAnims.Add(animationType);
                            }
                        }
                        else if (pendAnims.Contains(animationType))
                        {
                            commands.Add(new TextCommand(visibleCharIndex, TextCommandType.AnimEnd, match.Groups["par"].Value));
                            pendAnims.Remove(animationType);
                        }
                    }
                    break;
            }
        }
        bbqText.Append(rawText[lastEnd..]);
        visibleCharIndex += rawText.Length - lastEnd;
        foreach (string anim in pendAnims) commands.Add(new TextCommand(visibleCharIndex, TextCommandType.AnimEnd, anim));

        return bbqText.ToString();
    }

    private int CountVisibleChars(string text)
    {
        MatchCollection matches = TMPRegex.Matches(text);
        int extraChars = 0;
        foreach (Match match in matches)
        {
            extraChars += match.Value.Length;
        }
        return text.Length - extraChars;
    }

    private IEnumerator DisplayTextCo()
    {
        if (dialogueText != null)
        {
            while (dialogueText.maxVisibleCharacters < dialogueText.text.Length)
            {
                dialogueText.maxVisibleCharacters++;
                yield return new WaitForSeconds(typeDelay);
            }
            yield return new WaitForSeconds(displayTime);
            dialogueRoutine = null;
            if (dialogueText != null) dialogueText.text = string.Empty;
        }
        currentDialoguePriority = int.MinValue;
    }

    private enum TextCommandType
    {
        Pause,
        TextSpeedChange,
        AnimStart,
        AnimEnd
    }

    private struct TextCommand
    {
        public int index;
        public TextCommandType type;
        public float floatValue;
        public string stringValue;
        public ITextAnimation animation;
        public TextCommand(int index, TextCommandType type, float floatValue)
        {
            this.index = index;
            this.type = type;
            this.floatValue = floatValue;
            stringValue = null;
            animation = null;
        }
        public TextCommand(int index, TextCommandType type, string stringValue, ITextAnimation animation = null, float floatValue = 1f) // por si me da por agregar otro parametro con la fuerza del efecto
        {
            this.index = index;
            this.type = type;
            this.floatValue = floatValue;
            this.stringValue = stringValue;
            this.animation = animation;
        }
    }
}
