using NUnit;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class EventPoint : MonoBehaviour
{
    public enum EventType { EndLevel, AutoMove, CloseDoor }

    [SerializeField] private EventType eventType;

    [Header("AutoMove Settings")]
    [SerializeField] private Transform targetPoint;
    [SerializeField] private float extraTime;

    [Header("Door Settings")]
    [SerializeField] private Door[] doors;

    [Header("For Door and End")]
    [SerializeField] private bool waitUntilDoorLocked;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (!player) return;

        switch (eventType)
        {
            case EventType.EndLevel:
                if (waitUntilDoorLocked) StartCoroutine(HandlePlayerLock(true));
                else
                {
                    RoomManager.instance.LevelPased();
                    gameObject.SetActive(false);
                }
                break;

            case EventType.AutoMove:
                float deltaX = targetPoint.position.x - player.transform.position.x;

                float direction = Mathf.Sign(deltaX);
                float distance = Mathf.Abs(deltaX);
                float duration = (player.playerParameters.moveSpeed > 0 ? distance / player.playerParameters.moveSpeed : 0f) + extraTime;

                if (distance < 0.01f) break; // in case they are too close
                player.StartAutoMove(direction, player.playerParameters.moveSpeed, duration);
                gameObject.SetActive(false);
                break;

            case EventType.CloseDoor:
                foreach (Door door in doors)
                    door.Close();
                if (waitUntilDoorLocked) StartCoroutine(HandlePlayerLock(false));
                else gameObject.SetActive(false);
                break;
        }
    }

    private IEnumerator HandlePlayerLock(bool end)
    {
        RoomManager.instance.LockPLayers();
        yield return new WaitUntil(() =>
        {
            foreach (Door door in doors)
                if (!door.locked)
                    return false;
            return true;
        });
        yield return new WaitForSeconds(0.25f); // a little bit of extra time
        RoomManager.instance.UnlockPlayers();
        if (end) RoomManager.instance.LevelPased();
        gameObject.SetActive(false);
    }
}