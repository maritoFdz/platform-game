using System.Collections;
using UnityEngine;

public class PushableObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private CollisionsHandler2D controller;

    [Header("Parameters")]
    [SerializeField] private float pushSpeed = 3f;
    [SerializeField] private float accelerationTime = 0.1f;
    [SerializeField] private float slopeSlideMultiplier = 0.1f;

    private float push;
    private Vector2 velocity;
    private float velocityXSmoothing;
    private Player playerPushing;


    private void Update()
    {
        float dt = Time.deltaTime;
        if (playerPushing != null && push != 0f)
        {
            float targetX = push * pushSpeed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, accelerationTime);
            if (controller.colDetails.onSlopeSlide)
            {
                float pushbackAmount = pushSpeed * slopeSlideMultiplier * dt;
                Vector2 deltaMove = new(-Mathf.Sign(push) * pushbackAmount, 0f);
                controller.ClampDisplacement(ref deltaMove);
                transform.Translate(deltaMove);
                push = 0f;
                velocity.x = 0f;
                velocityXSmoothing = 0f;
                playerPushing = null;
                //transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                return;
            }
            Vector2 moveDelta = velocity * dt;
            controller.ClampDisplacement(ref moveDelta);
            transform.Translate(moveDelta);
            if (playerPushing)
                playerPushing.ApplyExternalDisplacement(moveDelta);
        }
        else
        {
            velocity.x = 0f;
            velocityXSmoothing = 0f;
        }
    }

    public void SetAsTargetOf(Player player)
    {
        playerPushing = player;
    }

    public void SetDirection(float push)
    {
        this.push = push;
    }
}