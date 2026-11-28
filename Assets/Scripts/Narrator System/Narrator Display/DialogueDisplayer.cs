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
    [Header("References")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private DialogueVoicer voicer;

    [Header("Text Settings")]
    [SerializeField] private float displayTime;
    [SerializeField] private float typeDelay;

    [Header("Voice Settings")]
    [SerializeField, Min(1)] private float mumbleFrequency;

    private static readonly Regex tagsRegex = new(@"<(?<name>p|s):(?<par>[^<>/]*)/?>|<(?<close>/)?(?<name>a):(?<par>[^<>/]+)>");
    private static readonly Regex TMPRegex = new(@"<[^>]*>");
    private readonly List<TextCommand> commands = new();
    private readonly List<TextAnimation> textAnimations = new();
    private static readonly Dictionary<string, ITextAnimator> animations = new(StringComparer.OrdinalIgnoreCase)
        {
            { "shake", new ShakeAnimator() },
            { "wave",  new WaveAnimator() },
        };

    private Coroutine dialogueRoutine;
    private int currentDialoguePriority = int.MinValue;


    private void Start()
    {
        if (dialogueText != null) dialogueText.text = string.Empty;
    }

    public void DisplayDialog(string line, int priority)
    {
        if (dialogueText == null || currentDialoguePriority >= priority) return;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.text = Parse(line);
        if (dialogueRoutine != null) StopCoroutine(dialogueRoutine);
        currentDialoguePriority = priority;
        dialogueRoutine = StartCoroutine(DisplayTextCo());
    }

    private string Parse(string rawText)
    {
        commands.Clear();
        textAnimations.Clear();
        StringBuilder bbqText = new();
        int lastEnd = 0;
        int visibleCharIndex = 0;
        Dictionary<string, (ITextAnimator animator, int start)> pendAnims = new(StringComparer.OrdinalIgnoreCase);
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
                    if (!match.Groups["close"].Success)
                    {
                        if (animations.TryGetValue(animationType, out ITextAnimator animatorObj) && !pendAnims.ContainsKey(animationType))
                            pendAnims[animationType] = (animatorObj, visibleCharIndex);
                    }
                    else if (pendAnims.TryGetValue(animationType, out var open))
                    {
                        textAnimations.Add(new TextAnimation { startIndex = open.start, endIndex = visibleCharIndex, animator = open.animator });
                        pendAnims.Remove(animationType);
                    }
                    break;
            }
        }
        bbqText.Append(rawText[lastEnd..]);
        visibleCharIndex += rawText.Length - lastEnd;
        foreach (var (animator, start) in pendAnims.Values) textAnimations.Add(new TextAnimation { startIndex = start, endIndex = visibleCharIndex, animator = animator });

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
        dialogueText.ForceMeshUpdate();
        int commandIndex = 0, visibleIndex = 0, nonSpaces = 0;
        float speed = 1f, nextCharTime = Time.unscaledTime, endTime = -1f;
        while (true)
        {
            float currentTime = Time.unscaledTime;
            while (visibleIndex < dialogueText.textInfo.characterCount && currentTime >= nextCharTime)
            {
                while (commandIndex < commands.Count && commands[commandIndex].index <= visibleIndex)
                {
                    switch (commands[commandIndex].type)
                    {
                        case TextCommandType.TextSpeedChange:
                            speed = commands[commandIndex].value;
                            break;
                        case TextCommandType.Pause:
                            nextCharTime = currentTime + commands[commandIndex].value;
                            break;
                    }
                    commandIndex++;
                }
                if (currentTime < nextCharTime) break;
                visibleIndex++;
                char revealed = dialogueText.textInfo.characterInfo[visibleIndex - 1].character;
                if (!char.IsWhiteSpace(revealed))
                {
                    nonSpaces++;
                    if (voicer != null && (nonSpaces - 1) % mumbleFrequency == 0) voicer.PlayMumble(revealed);
                }
                nextCharTime += typeDelay / speed;
            }
            dialogueText.maxVisibleCharacters = visibleIndex;

            if (textAnimations.Count > 0)
            {
                dialogueText.ForceMeshUpdate();
                ApplyAnimations(visibleIndex, currentTime);
            }
            if (visibleIndex >= dialogueText.textInfo.characterCount)
            {
                if (endTime < 0f) endTime = currentTime + displayTime;
                else if (currentTime >= endTime) break;
            }

            yield return null;
        }

        dialogueRoutine = null;
        dialogueText.text = string.Empty;
        currentDialoguePriority = int.MinValue;
    }

    private void ApplyAnimations(int visibleIndex, float currentTime)
    {
        TMP_VertexDataUpdateFlags flags = TMP_VertexDataUpdateFlags.None;
        foreach (TextAnimation anim in textAnimations)
        {
            anim.animator.Animate(dialogueText, anim.startIndex, Mathf.Min(anim.endIndex, visibleIndex), currentTime);
            flags |= anim.animator.Flags;
        }
        dialogueText.UpdateVertexData(flags);
    }

    private enum TextCommandType
    {
        Pause,
        TextSpeedChange
    }

    private struct TextCommand
    {
        public int index;
        public TextCommandType type;
        public float value;
        public TextCommand(int index, TextCommandType type, float value)
        {
            this.index = index;
            this.type = type;
            this.value = value;
        }
    }

    private struct TextAnimation
    {
        public int startIndex;
        public int endIndex;
        public ITextAnimator animator;
    }
}