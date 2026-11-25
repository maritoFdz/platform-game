using TMPro;
using UnityEngine;

public interface ITextAnimator
{
    public TMP_VertexDataUpdateFlags Flags { get; }
    public void Animate(TMP_Text text, int start, int end, float time);
}

public class ShakeAnimator : ITextAnimator
{
    private const float shakeStrength = 0.06f;
    private const float shakeFreq = 15f;

    public TMP_VertexDataUpdateFlags Flags => TMP_VertexDataUpdateFlags.Vertices;

    public void Animate(TMP_Text text, int start, int end, float time)
    {
        TMP_TextInfo textInfo = text.textInfo;
        float scaleAdjust = text.fontSize * shakeStrength;
        for (int charIndex = start; charIndex < end; charIndex++)
        {
            TMP_CharacterInfo letter = textInfo.characterInfo[charIndex];
            if (!letter.isVisible) continue;

            float x = (Mathf.PerlinNoise((charIndex + time) * shakeFreq, 0) - 0.5f) * scaleAdjust;
            float y = (Mathf.PerlinNoise((charIndex + time) * shakeFreq, 1000) - 0.5f) * scaleAdjust;
            Vector3[] verts = textInfo.meshInfo[letter.materialReferenceIndex].vertices;
            for (int v = 0; v < 4; v++)
            {
                verts[letter.vertexIndex + v].x += x;
                verts[letter.vertexIndex + v].y += y;
            }
        }
    }
}

public class WaveAnimator : ITextAnimator
{
    private const float waveStrength = 0.06f;
    private const float waveFreq = 6f;

    public TMP_VertexDataUpdateFlags Flags => TMP_VertexDataUpdateFlags.Vertices;

    public void Animate(TMP_Text text, int start, int end, float time)
    {
        TMP_TextInfo textInfo = text.textInfo;
        for (int charIndex = start; charIndex < end; charIndex++)
        {
            TMP_CharacterInfo letter = textInfo.characterInfo[charIndex];
            if (!letter.isVisible) continue;

            float y = Mathf.Sin((charIndex * 1.5f) + (time * waveFreq)) * text.fontSize * waveStrength;
            Vector3[] verts = textInfo.meshInfo[letter.materialReferenceIndex].vertices;
            for (int v = 0; v < 4; v++)
                verts[letter.vertexIndex + v].y += y;
        }
    }
}