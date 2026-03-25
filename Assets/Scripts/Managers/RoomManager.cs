using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    [Header("References")]
    public Tilemap worldTilemap;
    public Tilemap trailTilemap;
    public Tilemap splashTilemap;
    [SerializeField] private Player initialPlayer;

    [Header("Parameters")]
    [SerializeField] private Transform SpawnPoint;
    [SerializeField] private float respawnWait;

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        else instance = this;
    }

    private void Start()
    {
        SetPlayer();
    }

    private void SetPlayer()
    {
        Player newPlayer = Instantiate(initialPlayer, SpawnPoint.position, Quaternion.identity);
        newPlayer.SetNormalizedScale(1f);
        newPlayer.gameObject.SetActive(true);
    }

    public void PlayersDead()
    {
        StartCoroutine(RespawnCo());
    }

    private IEnumerator RespawnCo()
    {
        yield return new WaitForSeconds(respawnWait);
        SetPlayer();
    }
}
