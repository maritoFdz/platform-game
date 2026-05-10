using System;

[Serializable]
public class GameData
{
    public string lastRoomName;

    public GameData(string roomName)
    {
        lastRoomName = roomName;
    }
}