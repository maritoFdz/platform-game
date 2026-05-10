using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
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
    private Resetteable[] resetteables;

    [Header("Parameters")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float respawnWait;

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        else
        {
            instance = this;
            resetteables = FindObjectsByType<Resetteable>(FindObjectsSortMode.None);
        }
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

    public void LevelPassed()
    {
        DataManager.instance.SaveGameData(new(nextRoomName));
        SceneManager.LoadScene(nextRoomName);
    }

    public void LockPlayers()
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
        foreach (var resseteable in resetteables)
            resseteable.ResetEntity();
        yield return new WaitForSeconds(respawnWait);
        SetPlayer();
    }
}
