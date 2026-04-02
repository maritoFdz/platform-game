using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Tilemaps.Tile;

[CreateAssetMenu(fileName = "Freeze Tile", menuName = "Scriptable Objects/Freeze Tile")]
public class FreezeTile : TileBase, IInteractiveTile
{
    [SerializeField] private Sprite sprite;

    [Header("Effect Parameters")]
    [SerializeField] private float freezeTime;
    [SerializeField] private float applyTime;
    [SerializeField] private bool isScalable;

    public TileEffectType EffectType => TileEffectType.Freeze;
    public float Cooldown => applyTime;
    public bool IsScalable => isScalable;

    public void OnPlayerEnter(Player player)
    {
        player.Freeze(freezeTime);
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.colliderType = ColliderType.Sprite;
    }
}
