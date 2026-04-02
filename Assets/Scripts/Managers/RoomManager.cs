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
    [SerializeField] private Door entry;
    [SerializeField] private EventPoint[] eventPoints;

    [Header("Parameters")]
    [SerializeField] private Transform spawnPoint;
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
        Player newPlayer = Instantiate(initialPlayer, spawnPoint.transform.position, Quaternion.identity);
        newPlayer.SetNormalizedScale(1f);
        newPlayer.gameObject.SetActive(true);
        newPlayer.SetActiverState(false);
    }

    public void PlayersDead()
    {
        StartCoroutine(RespawnCo());
    }

    public void LevelPased()
    {
        SceneManager.LoadScene(nextRoomName);
    }

    public void LockPLayers()
    {
        PlayerSwitchManager.instance.DisableAll();
    }

    public void UnlockPlayers()
    {
        PlayerSwitchManager.instance.EnableAll();
    }

    private IEnumerator RespawnCo()
    {
        entry.Open();
        foreach (var eventPoint in eventPoints)
            eventPoint.gameObject.SetActive(true);
        yield return new WaitForSeconds(respawnWait);
        SetPlayer();
    }
}
