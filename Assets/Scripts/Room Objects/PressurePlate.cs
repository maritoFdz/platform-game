using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PressurePlate : MonoBehaviour
{
    private const int unpressedSpriteIndex = 0;
    private const int pressedSpriteIndex = 1;

    [Header("References")]
    [SerializeField] private Door[] doorsConnected;
    [SerializeField] private Sprite[] statesSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D col;
    [SerializeField] private bool keepsDoorClosed;

    [Header("Detection")]
    [SerializeField] private LayerMask activationLayer;
    [SerializeField] private float checkHeight;
    [SerializeField] private int rayAmount;

    private bool pressed;

    private void Update()
    {
        bool detected = false;
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
                break;
            }
        }

        if (detected && !pressed)
        {
            pressed = true;
            spriteRenderer.sprite = statesSprite[pressedSpriteIndex];
            foreach (var door in doorsConnected)
                if (!keepsDoorClosed) door.Open();
                else door.Close();
        }
        else if (!detected && pressed)
        {
            pressed = false;
            spriteRenderer.sprite = statesSprite[unpressedSpriteIndex];
            foreach (var door in doorsConnected)
                if (!keepsDoorClosed) door.Close();
                else door.Open();
        }
    }
}