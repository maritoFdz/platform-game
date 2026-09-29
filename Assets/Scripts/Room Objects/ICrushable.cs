using System.Collections;

public interface ICrushable
{
    public bool IsPendingCrush { get; }
    public bool IsColidingVer();
    public bool IsColidingHor();
    public void Crush(bool isVertical, float delay);
    public void PushAwayFromCrushVer();
    public void PushAwayFromCrushHor();
    public IEnumerator CrushCo(bool isVertical, float delay);
}