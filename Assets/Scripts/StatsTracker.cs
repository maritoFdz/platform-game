using System.Collections.Generic;
using UnityEngine;

public enum EventClassification
{
    PlayerDeath,
    DyeEvent,
    EnterZone,
    CorrectPuzzleStep,
    WrongPuzzleStep,
    PlayerJump,
    AllPlayersDeath,
    TooLongIdle,
    TooLongSplit
}

public class StatsTracker : MonoBehaviour
{
    public static StatsTracker instance;
    [SerializeField] private StatsChannel statsChannel;

    private float additionalPlayTime;
    private StatsData globalStats;
    private Dictionary<EventClassification, int> sessionStats;

    public float LastSavePlayTime => additionalPlayTime;
    public float GameTime => globalStats != null ? globalStats.gameTime + additionalPlayTime : 0f;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        sessionStats = new Dictionary<EventClassification, int>();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadStats();
    }

    private void Update()
    {
        additionalPlayTime += Time.deltaTime;
    }

    public int GetGlobal(EventClassification type) => (globalStats != null) ? (globalStats.eventsOcurrences.TryGetValue(type, out int v) ? v : 0) : 0;
    public int GetLocal(EventClassification type) => sessionStats.TryGetValue(type, out int v) ? v : 0;

    private void UpdateStat(EventClassification type)
    {
        if (globalStats == null && DataManager.instance != null) globalStats = DataManager.instance.LoadStats();
        if (globalStats != null)
        {
            if (globalStats.eventsOcurrences.ContainsKey(type)) globalStats.eventsOcurrences[type]++;
            else globalStats.eventsOcurrences.Add(type, 1);
        }

        if (sessionStats == null) return;
        if (sessionStats.ContainsKey(type)) sessionStats[type]++;
        else sessionStats.Add(type, 1);
    }

    private void LoadStats()
    {
        if (DataManager.instance != null) globalStats = DataManager.instance.LoadStats();
    }

    private void ResetLocalStats()
    {
        sessionStats = new Dictionary<EventClassification, int>();
    }

    private void SaveStats()
    {
        if (DataManager.instance == null) return;
        globalStats ??= DataManager.instance.LoadStats();
        if (globalStats == null) return;
        globalStats.gameTime += additionalPlayTime;
        additionalPlayTime = 0;
        DataManager.instance.SaveStats(globalStats);
    }

    private void OnEnable()
    {
        statsChannel.OnStatUpdated -= UpdateStat;
        statsChannel.OnLocalStatsReset -= ResetLocalStats;
        statsChannel.OnLocalStatsReset -= SaveStats;

        statsChannel.OnStatUpdated += UpdateStat;
        statsChannel.OnLocalStatsReset += ResetLocalStats;
        statsChannel.OnLocalStatsReset += SaveStats;
    }

    private void OnDisable()
    {
        statsChannel.OnStatUpdated -= UpdateStat;
        statsChannel.OnLocalStatsReset -= ResetLocalStats;
        statsChannel.OnLocalStatsReset -= SaveStats;
    }

    private void OnApplicationQuit()
    {
        SaveStats();
    }
}
