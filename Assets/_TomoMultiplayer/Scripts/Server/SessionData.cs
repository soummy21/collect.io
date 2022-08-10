using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;

public static class SessionData
{
    //Game Details
    public static string GameName { get; set; }
    public static bool connectionEstablished = false;
    public static bool isDevBuild = false;
    public static BuildType buildType;
    public static TypedLobby gameLobby;

    //Game States
    public static GameStates previousGameState = GameStates.MainMenu;
    public static GameStates currentGameState = GameStates.MainMenu;

    //Room Players Info
    public static List<Player> cachedRoomPlayers = new List<Player>();
    public static List<int> cachedPlayersArenaNo = new List<int>();
    public static int[] playersPerArena = new int[4];

    //Player Details
    public static string playerName
    {
        get; set;
        //get => PlayerPrefs.GetString(Identifiers.GameSettings.PlayerName);
        //set => PlayerPrefs.SetString(Identifiers.GameSettings.PlayerName, value);
    }
    public static int localArenaNo = -1;
    public static int sessionGameTime
    {
        get => PlayerPrefs.GetInt(Identifiers_Mul.PlayerSettings.GameTime, 600);
        set => PlayerPrefs.SetInt(Identifiers_Mul.PlayerSettings.GameTime, value);
    }

    //Player timeout
    private static bool timedOut = false;
    public static bool inTimeout
    {
        get => timedOut;
        set
        {
            timedOut = value;
            //if (value) PlayerPrefs.SetInt(Identifiers.GameSettings.PlayerTimeout, 1);
            //else PlayerPrefs.SetInt(Identifiers.GameSettings.PlayerTimeout, 0);
        }
    }
    public static int playerTimeout = 300;

    public static void InTimeout_Init()
    {
        if(PlayerPrefs.HasKey(Identifiers_Mul.PlayerSettings.PlayerTimeout))
        {
            if (PlayerPrefs.GetInt(Identifiers_Mul.PlayerSettings.PlayerTimeout) == 1) timedOut = true;
            else timedOut = false;
        }
    }

    public static void ResetPlayersPerArena()
    {
        for (int i = 0; i < playersPerArena.Length; i++)
        {
            playersPerArena[i] = 0;
        }
    }

    //Gives a valid random arena
    public static int GiveRandomArenaNo(int[] remainingSpots, int requiredArenas)
    {
        int randArenaNo = Random.Range(1, requiredArenas + 1);
        if (remainingSpots[randArenaNo - 1] == 0)
        {
            return GiveRandomArenaNo(remainingSpots, requiredArenas);
        }
        remainingSpots[randArenaNo - 1]--;
        return randArenaNo;
    }


}
