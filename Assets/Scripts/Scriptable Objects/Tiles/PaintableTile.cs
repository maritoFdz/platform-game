using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Tilemaps.Tile;

public enum SurfaceType { Floor, Wall, Slope }

[CreateAssetMenu(fileName = "Paintable Tile", menuName = "Scriptable Objects/Paintable Tile")]
public class PaintableTile : TileBase
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private SurfaceType surfaceType;
    public bool isPainted;
    public SurfaceType GetSurfaceType()
    {
        return surfaceType;
    }
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.colliderType = ColliderType.Sprite;
    }
}
