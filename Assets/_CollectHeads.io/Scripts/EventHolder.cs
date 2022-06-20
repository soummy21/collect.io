using System;


public static class ServerEvents
{
    public static Action OnServerConnected;
    public static Action OnFailedToJoinServer;
    public static Action OnJoinedRoom;
    public static Action<string> PlayerJoinedRoom;
    public static Action<string> PlayerLeftRoom;
    public static Action OnMasterClientChanged;
}


public static class MainMenuEvents
{
    public static Action<string> OnPlayerNameInput;
    public static Action<string> OnCreateRoomInput;
    public static Action<string> OnJoinRoomInput;
    public static Action<bool> RoomMenuStatus;

}

public static class GameplayEvents
{
    public static Action UpdateScoreboard;
    public static Action EndGameplay;
}

