using System.Collections;

public interface ICrushable
{
    public bool IsPendingCrush { get; }
    public bool IsColidingVer();
    public bool IsColidingHor();
    public void Crush(bool isVertical, float delay);
    public IEnumerator CrushCo(bool isVertical, float delay);
}