using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static RaycastLayout;

public class TilesInteractionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap trailTilemap;
    [SerializeField] private Tilemap worldTilemap;
    [SerializeField] private Tilemap splashTilemap;
    [SerializeField] private Player player;
    [SerializeField] private CollisionsHandler2D playerController;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private TilesInteractionParameters tilesInteractionParameters;

    // gives a less uniform effect by not allowing to paint all tiles at once
    private Dictionary<Vector3Int, float> neighborTilesColdown;
    private Dictionary<TileEffectType, float> effectsApplied; // makes effects unique and frame independent
    private List<RaycastHit2D> hitsAbove;
    private List<RaycastHit2D> hitsBelow;
    private List<RaycastHit2D> hitsLeft;
    private List<RaycastHit2D> hitsRight;

    private void Awake()
    {
        hitsAbove = new List<RaycastHit2D>();
        hitsBelow = new List<RaycastHit2D>();
        hitsLeft = new List<RaycastHit2D>();
        hitsRight = new List<RaycastHit2D>();
        neighborTilesColdown = new Dictionary<Vector3Int, float>();
        effectsApplied = new Dictionary<TileEffectType, float>();
    }

    public bool CheckWater()
    {
        RaycastOrigins origins = playerController.GetRaycastOrigins();
        Vector2 center = (origins.bottomLeft + origins.topRight) / 2;
        float width = Vector2.Distance(origins.bottomLeft, origins.bottomRight);
        float height = Vector2.Distance(origins.bottomLeft, origins.topLeft);
        return Physics2D.OverlapBox(center, new Vector2(width, height), 0f, waterLayer);
    }

    public void HandleTilesCollision()
    {
        // restarts tiles stored
        hitsBelow.Clear();
        hitsAbove.Clear();
        hitsLeft.Clear();
        hitsRight.Clear();

        RaycastLayoutDetails layoutInfo = playerController.GetRaycastLayoutDetails();
        RaycastOrigins raycastOrigins = playerController.GetRaycastOrigins();
        float rayLength = tilesInteractionParameters.rayLength + layoutInfo.skinWidth;

        // store tiles that player is touching above and below
        for (int i = 0; i < layoutInfo.verticalRayAmount; i++)
        {
            Vector2 originBelow = raycastOrigins.bottomLeft + Vector2.right * (layoutInfo.verRaySpacing * i);
            RaycastHit2D hitBelow = Physics2D.Raycast(originBelow,
                Vector2.down,
                rayLength,
                layoutInfo.collisionMask);
            Vector2 originAbove = raycastOrigins.topLeft + Vector2.right * (layoutInfo.verRaySpacing * i);
            RaycastHit2D hitAbove = Physics2D.Raycast(originAbove,
                Vector2.down,
                rayLength,
                layoutInfo.collisionMask);
            if (hitBelow) hitsBelow.Add(hitBelow);
            if (hitAbove) hitsAbove.Add(hitAbove);
        }

        // same but left/right
        for (int i = 0; i < layoutInfo.horizontalRayAmount; i++)
        {
            Vector2 originRight = raycastOrigins.bottomRight + Vector2.up * (layoutInfo.horRaySpacing * i);
            RaycastHit2D hitRight = Physics2D.Raycast(originRight,
                Vector2.right,
                rayLength,
                layoutInfo.collisionMask);
            Vector2 originLeft = raycastOrigins.bottomLeft + Vector2.up * (layoutInfo.horRaySpacing * i);
            RaycastHit2D hitLeft = Physics2D.Raycast(originLeft,
                Vector2.left,
                rayLength,
                layoutInfo.collisionMask);

            if (hitRight) hitsRight.Add(hitRight);
            if (hitLeft) hitsLeft.Add(hitLeft);
        }
        ApplyTilesEffects();
    }

    private void ApplyTilesEffects()
    {
        player.onFreezeTile = false;
        PickTiles(hitsBelow);
        PickTiles(hitsAbove);
        PickTiles(hitsLeft);
        PickTiles(hitsRight);
    }

    private void PickTiles(List<RaycastHit2D> hits)
    {
        foreach (var hit in hits)
        {
            Vector3Int tilePos = worldTilemap.WorldToCell(hit.point - hit.normal * 0.01f);
            if (worldTilemap.GetTile(tilePos) is IInteractiveTile interactiveTile)
            {
                if (interactiveTile is FreezeTile) player.onFreezeTile = true;
                TileEffectType effect = interactiveTile.EffectType;
                // checks if effect can be applied
                if (effectsApplied.ContainsKey(effect))
                    if (Time.time - effectsApplied[effect] < interactiveTile.Cooldown)
                        continue;
                interactiveTile.OnPlayerEnter(player);
                effectsApplied[effect] = Time.time;
            }
            else if (worldTilemap.GetTile(tilePos) is SewerTile sewer)
                sewer.KillPlayer(player);
        }
    }

    public void PaintTrail()
    {
        foreach (var hit in hitsBelow)
            PaintTile(hit.point - hit.normal * 0.01f, false);

        foreach (var hit in hitsRight)
            PaintTile(hit.point - hit.normal * 0.01f, false);

        foreach (var hit in hitsLeft)
            PaintTile(hit.point - hit.normal * 0.01f, true);
    }

    public void PaintSplash(Vector3 worldPos, float rotation)
    {
        RaycastLayoutDetails layoutInfo = playerController.GetRaycastLayoutDetails();
        Vector2 direction = rotation % 180 == 0f ? Vector2.up : Vector2.right * Mathf.Sign(rotation);
        if (rotation == 0f) direction = Vector2.down;
        RaycastHit2D hit = Physics2D.Raycast(worldPos, direction, 1f, layoutInfo.collisionMask); // arbitrary raycast length to avoid painting tiles to far from player
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
                    splashTilemap.SetTile(tilePos, tilesInteractionParameters.splashTiles[Random.Range(0, tilesInteractionParameters.splashTiles.Length)]);
                    splashTilemap.SetTileFlags(tilePos, TileFlags.None);
                    rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, rotation));
                    splashTilemap.SetTransformMatrix(tilePos, rotationMatrix);
                    break;
                case SurfaceType.Wall:
                    splashTilemap.SetTile(tilePos, tilesInteractionParameters.splashTiles[Random.Range(0, tilesInteractionParameters.splashTiles.Length)]);
                    splashTilemap.SetTileFlags(tilePos, TileFlags.None);
                    rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, rotation));
                    splashTilemap.SetTransformMatrix(tilePos, rotationMatrix);
                    break;
                case SurfaceType.Slope:
                    splashTilemap.SetTile(tilePos, tilesInteractionParameters.splashTilesSlope[Random.Range(0, tilesInteractionParameters.splashTilesSlope.Length)]);
                    splashTilemap.SetTileFlags(tilePos, TileFlags.None);
                    rotationMatrix = hit.normal.x > 0
                        ? Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f))
                        : Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 0f));
                    splashTilemap.SetTransformMatrix(tilePos, rotationMatrix);
                    break;
            }
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
            if (Time.time - lastTime < tilesInteractionParameters.tileCooldown)
                return;
        }

        trailTilemap.SetTile(tileToPaint, tilesInteractionParameters.trailTile);
        neighborTilesColdown[tileToPaint] = Time.time;

        int neighbors = Random.Range(0, tilesInteractionParameters.maxSpread); // takes a random amount of neighbors
        int direction = isFacingLeft ? -1 : 1;
        for (int i = 0; i < neighbors; i++)
        {
            // selects the neighbors
            int neighborX, neighborY;
            if (Random.value <= tilesInteractionParameters.outsideToInsideDistribution)
            {
                neighborX = Random.Range(0, tilesInteractionParameters.outsideWallThickness + 1) * -direction;
                neighborY = Random.Range(0, tilesInteractionParameters.outsideFloorThickness + 1);
            }
            else
            {
                neighborX = Random.Range(0, tilesInteractionParameters.insideWallThickness + 1) * direction;
                neighborY = Random.Range(- tilesInteractionParameters.insideFloorThickness, 1);
            }


            if (neighborX == 0 && neighborY == 0) continue;

            Vector3Int neighborTile = tileToPaint + new Vector3Int(neighborX, neighborY, 0);

            // checks if neighbor has already been stored
            if (neighborTilesColdown.ContainsKey(neighborTile))
            {
                float lastTime = neighborTilesColdown[neighborTile];
                if (Time.time - lastTime < tilesInteractionParameters.tileCooldown)
                    continue;
            }

            if (trailTilemap.GetTile(neighborTile) == null)
            {
                trailTilemap.SetTile(neighborTile, tilesInteractionParameters.trailTile);
                neighborTilesColdown[neighborTile] = Time.time;
            }
        }
    }
}
