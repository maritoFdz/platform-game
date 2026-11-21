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
    AllPlayersDeath
}

public class StatsTracker : MonoBehaviour
{
    public static StatsTracker instance;
    [SerializeField] private StatsChannel statsChannel;

    private float playTime;
    private float idleTime;
    private float splitTime;
    private Dictionary<EventClassification, int> globalStats;
    private Dictionary<EventClassification, int> localStats;

    public float PlayTime => playTime;
    public float IdleTime => idleTime;
    public float SplitTime => splitTime;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        globalStats = new Dictionary<EventClassification, int>();
        localStats = new Dictionary<EventClassification, int>();
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        idleTime += Time.deltaTime;
        playTime += Time.deltaTime;
        splitTime += Time.deltaTime;
    }

    public int GetGlobal(EventClassification type) => globalStats.TryGetValue(type, out int v) ? v : 0;
    public int GetLocal(EventClassification type) => localStats.TryGetValue(type, out int v) ? v : 0;

    private void UpdateStat(EventClassification type)
    {
        if (globalStats == null) return;
        if (globalStats.ContainsKey(type)) globalStats[type]++;
        else globalStats.Add(type, 1);
        StoreGlobalStats();

        if (localStats == null) return;
        if (localStats.ContainsKey(type)) localStats[type]++;
        else localStats.Add(type, 1);
    }

    private void StoreGlobalStats()
    {
        // depue
    }

    private void ResetLocalStats()
    {
        localStats = new Dictionary<EventClassification, int>();
    }

    private void ResetIdleTime()
    {
        idleTime = 0f;
    }

    private void ResetSplitTime()
    {
        splitTime = 0f;
    }

    private void OnEnable()
    {
        statsChannel.OnStatUpdated -= UpdateStat;
        statsChannel.OnLocalStatsReset -= ResetLocalStats;
        statsChannel.OnIdleUpdate -= ResetIdleTime;
        statsChannel.OnSplitChargeUpdate -= ResetSplitTime;

        statsChannel.OnStatUpdated += UpdateStat;
        statsChannel.OnLocalStatsReset += ResetLocalStats;
        statsChannel.OnIdleUpdate += ResetIdleTime;
        statsChannel.OnSplitChargeUpdate += ResetSplitTime;
    }

    private void OnDisable()
    {
        statsChannel.OnStatUpdated -= UpdateStat;
        statsChannel.OnLocalStatsReset -= ResetLocalStats;
        statsChannel.OnIdleUpdate -= ResetIdleTime;
        statsChannel.OnSplitChargeUpdate -= ResetSplitTime;
    }
}
