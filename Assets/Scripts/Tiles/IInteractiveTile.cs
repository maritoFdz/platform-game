using UnityEngine;

public enum TileEffectType { Heat }

public interface IInteractiveTile
{
    TileEffectType EffectType { get; }
    float Cooldown { get; }
    public void OnPlayerEnter(Player player);
}
