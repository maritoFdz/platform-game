using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Tilemaps.Tile;

[CreateAssetMenu(fileName = "Heat Tile", menuName = "Scriptable Objects/Heat Tile")]
public class HeatTile : TileBase, IInteractiveTile
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private int shrinksPerSecond;

    public void OnPlayerEnter(Player player)
    {
        player.Shrink(true, shrinksPerSecond);
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.colliderType = ColliderType.Sprite;
    }
}
