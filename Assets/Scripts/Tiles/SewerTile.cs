using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Tilemaps.Tile;

[CreateAssetMenu(fileName = "Sewer Tile", menuName = "Scriptable Objects/Sewer Tile")]
public class SewerTile : TileBase
{
    [SerializeField] private Sprite sprite;

    public void KillPlayer(Player player)
    {
        player.KillPlayer();
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.colliderType = ColliderType.Sprite;
    }
}
