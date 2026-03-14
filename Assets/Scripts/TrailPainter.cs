using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class TrailPainter : RaycastLayout
{
    [Header("References")]
    [SerializeField] private Tilemap trailTilemap;
    [SerializeField] private Tilemap worldTilemap;
    [SerializeField] private Tilemap splashTilemap;
    [SerializeField] private Tile[] splashTiles;
    [SerializeField] private Tile[] splashTilesSlope;

    [Header("Trail Parameters")]
    [SerializeField] private Tile trailTile;
    [SerializeField] private float rayLength;
    [SerializeField] private int maxSpread;
    [SerializeField] private float tileCooldown;
    [SerializeField] private int outsideFloorThickness;
    [SerializeField] private int insideFloorThickness;
    [SerializeField] private int outsideWallThickness;
    [SerializeField] private int insideWallThickness;
    [SerializeField, Range(0, 1)] private float outsideToInsideDistribution;

    // gives a less uniform effect by not allowing to paint all tiles at once
    private Dictionary<Vector3Int, float> neighborTilesColdown;

    protected override void Awake()
    {
        neighborTilesColdown = new Dictionary<Vector3Int, float>();
        base.Awake();
    }

    public void UpdateCollisionsDescale(float normalizedScale)
    {
        SetRaySpacing(normalizedScale);
        UpdateRaycast();
    }

    public void PaintSplash(Vector3 worldPos, float rotation)
    {
        UpdateRaycast();
        Vector2 direction = rotation % 180 == 0f ? Vector2.up : Vector2.right * Mathf.Sign(rotation);
        if (rotation == 0f) direction = Vector2.down;
        RaycastHit2D hit = Physics2D.Raycast(worldPos, direction, 1f, collisionMask); // arbitrary raycast length to avoid painting tiles to far from player
        if (hit)
        {
            Vector3Int tilePos = worldTilemap.WorldToCell(hit.point - hit.normal * 0.01f);
            PaintableTile tile = worldTilemap.GetTile(tilePos) as PaintableTile;
            if (tile == null) return;
            SurfaceType surface = tile.GetSurfaceType();
            if (splashTilemap.GetTile(tilePos) != null)
                return;

            Matrix4x4 rotationMatrix;
            switch (surface)
            {
                case SurfaceType.Floor:
                    splashTilemap.SetTile(tilePos, splashTiles[Random.Range(0, splashTiles.Length)]);
                    splashTilemap.SetTileFlags(tilePos, TileFlags.None);
                    rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, rotation));
                    splashTilemap.SetTransformMatrix(tilePos, rotationMatrix);
                    break;
                case SurfaceType.Wall:
                    splashTilemap.SetTile(tilePos, splashTiles[Random.Range(0, splashTiles.Length)]);
                    splashTilemap.SetTileFlags(tilePos, TileFlags.None);
                    rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, rotation));
                    splashTilemap.SetTransformMatrix(tilePos, rotationMatrix);
                    break;
                case SurfaceType.Slope:
                    splashTilemap.SetTile(tilePos, splashTilesSlope[Random.Range(0, splashTilesSlope.Length)]);
                    splashTilemap.SetTileFlags(tilePos, TileFlags.None);
                    rotationMatrix = hit.normal.x > 0
                        ? Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f))
                        : Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 0f));
                    splashTilemap.SetTransformMatrix(tilePos, rotationMatrix);
                    break;
            }
        }
    }

    public void PaintTrail()
    {
        UpdateRaycast();
        float rayLength = this.rayLength + scaledSkinWidth;

        for (int i = 0; i < verticalRayAmount; i++)
        {
            // paints below
            Vector2 origin = raycastOrigins.bottomLeft + Vector2.right * (verRaySpacing * i);
            RaycastHit2D hitDown = Physics2D.Raycast(origin, 
                Vector2.down,
                rayLength,
                collisionMask);
            if (hitDown) PaintTile(hitDown.point - hitDown.normal * 0.01f, raycastOrigins.bottomLeft + Vector2.right * (verRaySpacing * i) == raycastOrigins.bottomRight);
        }

        for (int i = 0; i < horizontalRayAmount; i++)
        {
            // paints right
            Vector2 origin = raycastOrigins.bottomRight + Vector2.up * (verRaySpacing * i);
            RaycastHit2D hitRight = Physics2D.Raycast(origin,
                Vector2.right,
                rayLength,
                collisionMask);
            if (hitRight) PaintTile(hitRight.point - hitRight.normal * 0.01f, false);

            // paints left
            origin = raycastOrigins.bottomLeft + Vector2.up * (verRaySpacing * i);
            RaycastHit2D hitLeft = Physics2D.Raycast(origin,
                Vector2.left,
                rayLength,
                collisionMask);
            if (hitLeft) PaintTile(hitLeft.point - hitLeft.normal * 0.01f, true);
        }
    }

    private void PaintTile(Vector3 worldPos, bool isFacingLeft)
    {
        Vector3Int tileToPaint = trailTilemap.WorldToCell(worldPos);
        Vector3Int worldTile = worldTilemap.WorldToCell(worldPos);
        PaintableTile tile = worldTilemap.GetTile(worldTile) as PaintableTile;
        if (tile == null) return;
        if (neighborTilesColdown.ContainsKey(tileToPaint))
        {
            float lastTime = neighborTilesColdown[tileToPaint];
            if (Time.time - lastTime < tileCooldown)
                return;
        }

        trailTilemap.SetTile(tileToPaint, trailTile);
        neighborTilesColdown[tileToPaint] = Time.time;

        int neighbors = Random.Range(0, maxSpread); // takes a random amount of neighbors
        int direction = isFacingLeft ? -1 : 1;
        for (int i = 0; i < neighbors; i++)
        {
            // selects the neighbors
            int neighborX, neighborY;
            if (Random.value <= outsideToInsideDistribution)
            {
                neighborX = Random.Range(0, outsideWallThickness + 1) * -direction;
                neighborY = Random.Range(0, outsideFloorThickness + 1);
            }
            else
            {
                neighborX = Random.Range(0, insideWallThickness + 1) * direction;
                neighborY = Random.Range(-insideFloorThickness, 1);
            }


            if (neighborX == 0 && neighborY == 0) continue;

            Vector3Int neighborTile = tileToPaint + new Vector3Int(neighborX, neighborY, 0);

            // checks if neighbor has already been stored
            if (neighborTilesColdown.ContainsKey(neighborTile))
            {
                float lastTime = neighborTilesColdown[neighborTile];
                if (Time.time - lastTime < tileCooldown)
                    continue;
            }

            if (trailTilemap.GetTile(neighborTile) == null)
            {
                trailTilemap.SetTile(neighborTile, trailTile);
                neighborTilesColdown[neighborTile] = Time.time;
            }
        }
    }
}
