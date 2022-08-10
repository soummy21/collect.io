using System;
using System.Collections.Generic;
using Photon.Realtime;

public static class ServerMesseges 
{
    public static Action EstablishConnectionToServer;
    public static Action OnConnectedToPhoton;
    public static Action OnDissconnectedFromPhoton;
    public static Action OnCreateRoomSuccessful;
    public static Action OnJoinRoomSuccessful;
    public static Action<string> OnCreateRoomFailed;
    public static Action<string> OnJoinRoomFailed;
    public static Action OnLeaveRoom;
    public static Action<List<RoomInfo>> OnRoomListUpdated;
    public static Action<Player> OnPlayerJoinedRoom;
    public static Action<Player> OnPlayerLeftRoom;
    public static Action<Player> OnPlayerPropertiesUpdate;
    public static Action<Player> OnPlayerStateUpdate;
    public static Action OnMasterClientSwitched;
 }

public static class UserMessages
{
    public static Action<string> OnUpdateUsername;
    public static Action<int, int> UpdateLeaderBoard;
    public static Action<Dictionary<int[], string>> SendArenaPlayerList;
    
}

public static class UtilMessages
{
    public static Action SetAndStartTimer;
}

public static class GameMessages
{
    public static Action<int> OnCompleteArena;
    public static Action OnPauseGame;
    public static Action OnPlayGame;
    public static Action OnGameSessionEnded;
}

