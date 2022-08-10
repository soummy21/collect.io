

public static class Identifiers_Mul
{
    public static readonly string MainMenuScene = "MainMenu";
    public static readonly string LobbyScene = "Lobby";
    public static readonly string GameScene = "Game";
    
    public static class PlayerSettings
    {
        public static readonly string PlayerName = "PlayerName";
        public static readonly string PlayerTimeout = "PlayerTimeOut";
        public static readonly string GameTime = "GameTime";
        public static readonly string ArenaNo = "ArenaNo";
        public static readonly string TeamNo = "TeamNo";
        public static readonly string AvailableArenas = "NoOfArenas";
        public static readonly string OccupiedArenas = "OccupiedArenas";
        public static readonly string ArenaPosition = "ArenaPosition";

    }

    public static class NetworkEvents
    {
        public static readonly byte CloseRoomForEveryone = 1;
        public static readonly byte CloseRoomForPlayer = 2;
    }


}

public enum BuildType { Session_Moderator, Session_Player, Global_Build }

public enum GameStates { MainMenu, RoomLobby, InGame }




