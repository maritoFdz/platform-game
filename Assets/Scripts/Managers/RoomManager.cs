using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    [Header("References")]
    public Tilemap worldTilemap;
    public Tilemap trailTilemap;
    public Tilemap splashTilemap;
    [SerializeField] private Player initialPlayer;
    [SerializeField] private string nextRoomName;

    [Header("Parameters")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform endPoint;
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
        Player newPlayer = Instantiate(initialPlayer, spawnPoint.position, Quaternion.identity);
        newPlayer.SetNormalizedScale(1f);
        newPlayer.gameObject.SetActive(true);
    }

    public void PlayersDead()
    {
        StartCoroutine(RespawnCo());
    }

    public void LevelPased()
    {
        SceneManager.LoadScene(nextRoomName);
    }

    private IEnumerator RespawnCo()
    {
        yield return new WaitForSeconds(respawnWait);
        SetPlayer();
    }
}
