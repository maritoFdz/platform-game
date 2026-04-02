using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class EndPoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Player>())
            RoomManager.instance.LevelPased();
    }
}
