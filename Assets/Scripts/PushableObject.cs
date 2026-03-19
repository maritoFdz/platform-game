using UnityEngine;

public class PushableObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CollisionsHandler2D controller;

    [Header("Parameters")]
    [SerializeField] private float pushSpeed;
    [SerializeField] private float timeAccelerate;
    [SerializeField] private float gravityScale;

    private float direction;
    private Vector2 velocity;
    private float velocityXSmoothing;

    private void Update()
    {
        float dt = Time.deltaTime;

        Vector2 acceleration = new(0, -gravityScale);

        // Aplicar SmoothDamp a la velocidad horizontal
        float targetVelX = pushSpeed * direction;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelX, ref velocityXSmoothing, timeAccelerate);

        // Integración tipo Verlet
        Vector2 deltaMove = velocity * dt + 0.5f * dt * dt * acceleration;

        // Aplicar colisiones
        controller.ClampDisplacement(ref deltaMove);

        // Actualizar posición
        transform.position += (Vector3)deltaMove;

        // Integrar velocidad vertical
        velocity += acceleration * dt;

        if (controller.colDetails.below)
            velocity.y = 0f;
    }

    public void SetDirection(float input)
    {
        direction = input;
    }
}