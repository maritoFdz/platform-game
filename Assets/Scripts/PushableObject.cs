using UnityEditor.Rendering;
using UnityEngine;

public class PushableObject : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float pushSpeed;
    [SerializeField] private float timeAccelerate;
    [SerializeField] private float gravityScale;

    private float push;
    private Vector2 velocity;
    private Player playerPushing;

    private void Update()
    {

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