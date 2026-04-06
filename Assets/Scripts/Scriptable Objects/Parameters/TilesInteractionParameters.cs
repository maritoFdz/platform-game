using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Tiles Interaction Parameters", menuName = "Scriptable Objects/Tiles Interaction Parameters")]
public class TilesInteractionParameters : ScriptableObject
{
    public Tile trailTile;
    public Tile[] splashTiles;
    public Tile[] splashTilesSlope;
    public Tile[] splashTilesSteepSlope;
    public Tile[] splashTilesSteepSlopeSmall;
    public float rayLength;
    public int maxSpread;
    public float tileCooldown;
    public int outsideFloorThickness;
    public int insideFloorThickness;
    public int outsideWallThickness;
    public int insideWallThickness;
    [Range(0, 1)] public float outsideToInsideDistribution;
}
