using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
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
    [SerializeField] private Light2D[] roomLights;
    [SerializeField] private NarrationSet[] narrationSets;
    [SerializeField] private StatsChannel statsChannel;

    [Header("Parameters")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float respawnWait;

    private IResetteable[] resetteables;

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        else
        {
            instance = this;
            resetteables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IResetteable>().ToArray();
        }
    }

    private void Start()
    {
        SetPlayer();
        SettingsManager.OnPalleteChange += ChangeLightsColor;
        if (SettingsManager.instance != null)
            ChangeLightsColor(SettingsManager.instance.CurrentLightColor);
        if (NarratorManager.instance != null)
            NarratorManager.instance.AsignNarrationSet(narrationSets);
    }

    private void SetPlayer()
    {
        Player newPlayer = Instantiate(initialPlayer, spawnPoint.transform.position, Quaternion.identity);
        newPlayer.SetNormalizedScale(1f);
        newPlayer.gameObject.SetActive(true);
        newPlayer.SetActiveState(false);
    }

    public void PlayersDead()
    {
        if (statsChannel != null) statsChannel.UpdateStats(EventClassification.AllPlayersDeath);
        StartCoroutine(RespawnCo());
    }

    public void LevelPassed()
    {
        if (DataManager.instance != null)
            DataManager.instance.SaveLastRoom(nextRoomName);
        if (statsChannel != null) statsChannel.ResetLocalStats();
        if (SettingsManager.instance != null)
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
        yield return new WaitForSeconds(respawnWait);
        entry.Open();
        foreach (var resseteable in resetteables)
            resseteable.ResetEntity();
        SetPlayer();
    }

    public void ChangeLightsColor(Color color)
    {
        foreach (var light in roomLights)
            light.color = color;
    }

    private void OnDestroy()
    {
        SettingsManager.OnPalleteChange -= ChangeLightsColor;
        if (instance == this) instance = null;
    }
}
