using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(BoxCollider2D))]
public class PressurePlate : MonoBehaviour, IResetteable
{
    private const int unpressedSpriteIndex = 0;
    private const int pressedSpriteIndex = 1;

    [Header("References")]
    [SerializeField] private Door[] doorsToOpen;
    [SerializeField] private Door[] doorsToClose;
    [SerializeField] private Sprite[] statesSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D col;

    [Header("Detection")]
    [SerializeField] private LayerMask activationLayer;
    [SerializeField] private float checkHeight;
    [SerializeField] private int rayAmount;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckLength;

    private bool pressed;
    private readonly Dictionary<TileEffectType, float> effectsApplied = new();
    private IInteractiveTile tileBelow;
    private Player playerOnPlate;
    private Tilemap worldTilemap;

    private void Start()
    {
        worldTilemap = RoomManager.instance.worldTilemap;
        RaycastHit2D hit = Physics2D.Raycast(col.bounds.center, Vector2.down, groundCheckLength, groundLayer);
        if (hit && worldTilemap)
        {
            Vector3Int tilePos = worldTilemap.WorldToCell(hit.point - hit.normal * 0.01f);
            tileBelow = worldTilemap.GetTile(tilePos) as IInteractiveTile;
        }
    }

    private void Update()
    {
        bool detected = false;
        Collider2D onTop = null;

        Vector2 origin = new(col.bounds.min.x, col.bounds.max.y);
        float spacing = col.bounds.size.x / (rayAmount - 1);
        for (int i = 0; i < rayAmount; i++)
        {
            if (i == 0 || i == rayAmount - 1) continue;
            
            Vector2 rayOrigin = origin + i * spacing * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, checkHeight, activationLayer);
            if (hit)
            {
                detected = true;
                onTop = hit.collider;
                break;
            }
        }

        if (detected && !pressed)
        {
            pressed = true;
            playerOnPlate = onTop.GetComponent<Player>();
            spriteRenderer.sprite = statesSprite[pressedSpriteIndex];
            foreach (var door in doorsToOpen)
                door.Open();
            foreach (var door in doorsToClose)
                door.Close();
        }
        else if (!detected && pressed)
        {
            pressed = false;
            playerOnPlate = null;
            spriteRenderer.sprite = statesSprite[unpressedSpriteIndex];
            foreach (var door in doorsToOpen)
                door.Close();
            foreach (var door in doorsToClose)
                door.Open();
        }

        if (detected && playerOnPlate != null)
            ApplyTile(playerOnPlate);
    }

    private void ApplyTile(Player player)
    {
        if (tileBelow == null) return;

        TileEffectType effect = tileBelow.EffectType;
        if (effectsApplied.TryGetValue(effect, out float lastTime) && Time.time - lastTime < tileBelow.Cooldown)
            return;

        tileBelow.OnPlayerEnter(player);
        effectsApplied[effect] = Time.time;
    }

    public void ResetEntity()
    {
        playerOnPlate = null;
    }
}