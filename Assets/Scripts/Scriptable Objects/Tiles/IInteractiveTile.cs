using UnityEngine;

public enum TileEffectType { Heat, Freeze }

public interface IInteractiveTile
{
    TileEffectType EffectType { get; }
    float Cooldown { get; }
    public void OnPlayerEnter(Player player);
}
