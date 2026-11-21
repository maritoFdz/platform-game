using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public string lastRoomName;
    public List<int> unlockedPalletes;
    public StatsData stats;

    public GameData(string roomName, List<int> unlocked, StatsData stats)
    {
        lastRoomName = roomName;
        unlockedPalletes = unlocked;
        this.stats = stats;
    }
}