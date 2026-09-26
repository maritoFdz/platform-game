public interface ICrushable
{
    public bool IsCrushed { get; }
    public bool IsColidingVer();
    public bool IsColidingHor();
    public void Crush();
}