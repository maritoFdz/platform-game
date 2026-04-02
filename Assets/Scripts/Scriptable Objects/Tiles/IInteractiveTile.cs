using UnityEngine;

public enum TileEffectType { Heat, Freeze,
    PressurePlate
}

public interface IInteractiveTile
{
    TileEffectType EffectType { get; }
    bool IsScalable { get; }
    float Cooldown { get; }
    public void OnPlayerEnter(Player player);
}
