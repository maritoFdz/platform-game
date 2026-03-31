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

    [Header("Detection")]
    [SerializeField] private LayerMask activationLayer;
    [SerializeField] private float checkHeight;
    [SerializeField] private int rayAmount;

    private bool pressed;

    private void Update()
    {
        Vector2 origin = new(col.bounds.min.x, col.bounds.max.y);
        float spacing = col.bounds.size.x / (rayAmount - 1);
        for (int i = 0; i < rayAmount; i++)
        {
            if (i == 0 || i == rayAmount - 1) continue;
            Vector2 rayOrigin = origin + i * spacing * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, checkHeight, activationLayer);

            if (hit)
            {
                if (!pressed)
                {
                    pressed = true;
                    spriteRenderer.sprite = statesSprite[pressedSpriteIndex];
                    foreach (var door in doorsConnected) door.Open();
                }
                return;
            }
        }
        spriteRenderer.sprite = statesSprite[unpressedSpriteIndex];
        pressed = false;
        foreach (var door in doorsConnected) door.Close();
    }
}