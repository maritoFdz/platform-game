using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public string lastRoomName;
    public List<int> unlockedPalletes;

    public GameData(string roomName, List<int> unlocked)
    {
        lastRoomName = roomName;
        unlockedPalletes = unlocked;
    }
}