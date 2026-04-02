using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Tilemaps.Tile;

[CreateAssetMenu(fileName = "Heat Tile", menuName = "Scriptable Objects/Heat Tile")]
public class HeatTile : TileBase, IInteractiveTile
{
    [SerializeField] private Sprite sprite;

    [Header("Effect Parameters")]
    [SerializeField] private int shrinksPerTime;
    [SerializeField] private float applyTime;
    [SerializeField] private bool isScalable;

    public TileEffectType EffectType => TileEffectType.Heat;
    public float Cooldown => applyTime;
    public bool IsScalable => isScalable;

    public void OnPlayerEnter(Player player)
    {
        player.UnFreeze();
        player.Shrink(true, shrinksPerTime);
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.colliderType = ColliderType.Sprite;
    }
}
