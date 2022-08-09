using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System;

public class ArenaManager : MonoBehaviour
{
    [Header("Arena In-Game")]
    [Tooltip("Add reference to the 4 cameras within your scene")]
    [SerializeField] Camera[] Cameras_Arena;
    [Tooltip("Add refernce to the 4 UI panels within your scene")]
    [SerializeField] GameObject[] Panels_Arena;
    [Tooltip("Assign reference to the 4 arena timer text elements")]
    [SerializeField] TextMeshProUGUI[] Texts_ArenaTimer;
    [SerializeField] GameObject Panel_Pause;

    [Header("Moderator Arena UI")]
    [SerializeField] GameObject Panel_Moderator;
    [SerializeField] TextMeshProUGUI Text_Spectating;
    [SerializeField] TextMeshProUGUI Text_ArenaNo;
    [SerializeField] GameObject[] playPauseButtons;
    [SerializeField] Sprite[] playPauseSprites;

    [Header("Arena Rankings")]
    [SerializeField] GameObject Popup_EndPanel;
    [SerializeField] GameObject PlayerButtons;
    [SerializeField] GameObject ModeratorButtons;
    [SerializeField] TextMeshProUGUI[] arenaWinnerTeams;
    [SerializeField] TextMeshProUGUI[] arenaWinnerPlayers;

    private ModeratorCameraFollow[] moderatorCameraFollows;
    private Image[] playPauseImages;

    private PhotonView photonView;

    private const int maxArenas = 4; //Maximum arenas possible
    private int sessionTime = -1; //Session Time cached for this class
    private TimerDown[] arenaTimers;// Timers for all the arenas

    private int currentSelectedArena = 1; //current arena in view
    private int currentlyOccupiedArenas = -1;//amount of arenas occupied in this session

    //data cache of current time in each arena (Master client disconnection handling)
    private int[] currentArenaTimes = new int[maxArenas];
    //data cache of current pause state in each arena (Master client disconnection handling)
    private bool[] currentArenaPauseStatus = new bool[maxArenas];
    //bools to check if arena session are over
    private bool[] arenaSessionOver;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();

        Arena_Init();
        ArenaUI_Init();
        ArenaRankings_Init();
    }



    private void OnEnable()
    {
        ServerMesseges.OnLeaveRoom += GoToMainMenu;

        GameMessages.OnGameSessionEnded += TurnOnEndPanel;
        GameMessages.OnCompleteArena += UpdateArenaUIOnModerator;
        UserMessages.UpdateLeaderBoard += UpdateAreanRankings;

        UtilMessages.SetAndStartTimer += SetTimer;

        for (int i = 0; i < currentlyOccupiedArenas; i++)
        {
            arenaTimers[i].TimerUpdatePerSecondForArena += UpdateArenaTimerUI;
            arenaTimers[i].ArenaTimerCompleted += ArenaSessionOver;
        }
    }

    private void OnDisable()
    {
        ServerMesseges.OnLeaveRoom -= GoToMainMenu;

        GameMessages.OnGameSessionEnded -= TurnOnEndPanel;
        GameMessages.OnCompleteArena -= UpdateArenaUIOnModerator;
        UserMessages.UpdateLeaderBoard -= UpdateAreanRankings;

        UtilMessages.SetAndStartTimer -= SetTimer;

        for (int i = 0; i < currentlyOccupiedArenas; i++)
        {
            arenaTimers[i].TimerUpdatePerSecondForArena -= UpdateArenaTimerUI;
            arenaTimers[i].ArenaTimerCompleted -= ArenaSessionOver;
        }
    }

    private void Update()
    {
        //Only run on the masterClient
        if(PhotonNetwork.IsMasterClient)
        {
            //Only run currently occupiedArenas
            for (int i = 0; i < currentlyOccupiedArenas; i++)
            {
                arenaTimers[i].UpdateTimer();
            }
        }
    }


    private void ArenaRankings_Init()
    {
        PlayerButtons.SetActive(SessionData.buildType == BuildType.Session_Player);
        ModeratorButtons.SetActive(SessionData.buildType == BuildType.Session_Moderator);

        //Initialize Arena Rankings
        for (int i = 0; i < maxArenas; i++)
        {
            if (i < currentlyOccupiedArenas)
            {
                arenaWinnerTeams[i].text = "In Progress";
                arenaWinnerPlayers[i].text = "In Progress";
            }
            else
            {
                arenaWinnerPlayers[i].text = "-";
                arenaWinnerTeams[i].text = "-";
            }
        }

        Popup_EndPanel.SetActive(false);
    }

    private void ArenaUI_Init()
    {
        Panel_Moderator.SetActive(SessionData.buildType == BuildType.Session_Moderator);
        Panel_Pause.SetActive(false);

        Text_Spectating.text = "In Session!";
        Text_ArenaNo.text = $"Arena {currentSelectedArena}";

        playPauseImages = new Image[playPauseButtons.Length];

        for (int i = 0; i < maxArenas; i++)
        {
            playPauseImages[i] = playPauseButtons[i].GetComponentInChildren<Image>();
            playPauseImages[i].sprite = playPauseSprites[0];
            playPauseButtons[i].SetActive(false);
        }

        playPauseButtons[currentSelectedArena-1].SetActive(true);
    }

    private void Arena_Init()
    {
        //Init Arenas 
        currentlyOccupiedArenas = (int)PhotonNetwork.CurrentRoom.CustomProperties[Identifiers_Mul.PlayerSettings.NoOfArenas];

        arenaSessionOver = new bool[currentlyOccupiedArenas];

        //Initialize Timers
        sessionTime = SessionData.sessionGameTime;
        arenaTimers = new TimerDown[] { new TimerDown(sessionTime, 1), new TimerDown(sessionTime, 2), new TimerDown(sessionTime, 3), new TimerDown(sessionTime, 4) };

        for (int i = 0; i < maxArenas; i++)
        {
            if (i < currentlyOccupiedArenas) arenaSessionOver[i] = false;

            currentArenaPauseStatus[i] = false;
            Texts_ArenaTimer[i].text = Utilities.CovertTimeToString(sessionTime);
            arenaTimers[i].StartTimer(); //Start Arena Timer
            
        }

        //Reset all cameras
        foreach (var camera in Cameras_Arena)
        {
            camera.depth = 0;
        }

        moderatorCameraFollows = new ModeratorCameraFollow[maxArenas];

        for (int i = 0; i < Cameras_Arena.Length; i++)
        {
            moderatorCameraFollows[i] = Cameras_Arena[i].GetComponent<ModeratorCameraFollow>();
        }

        //Reset all panels
        foreach (var panel in Panels_Arena)
        {
            panel.SetActive(false);
        }

        AssignArenaToLookAt();

    }

    private void AssignArenaToLookAt()
    {
        switch (SessionData.buildType)
        {
            case BuildType.Session_Moderator:
                // Render current selected arena 
                Cameras_Arena[currentSelectedArena - 1].depth = 1;
                Panels_Arena[currentSelectedArena - 1].SetActive(true);
                moderatorCameraFollows[currentSelectedArena - 1].UpdateCameraFollow(true);
                break;
            case BuildType.Session_Player:
                // Render based on local players arenaNo
                //Cameras_Arena[SessionData.arenaNo - 1].depth = 1;
                foreach (var camera in Cameras_Arena) camera.gameObject.SetActive(false);
                Panels_Arena[SessionData.arenaNo - 1].SetActive(true);
                break;
        }
    }

    private void UpdateArenaTimerUI(int time, int arenaNo)
    {
        object[] timerData = new object[] { time, arenaNo };
        if (PhotonNetwork.IsMasterClient) photonView.RPC(nameof(UpdateArenaTimerUIOnNetwork), RpcTarget.All, timerData);
    }

    [PunRPC]
    private void UpdateArenaTimerUIOnNetwork(object [] data)
    {
        int time = (int) data[0];
        int arenaNo = (int)data[1];

        if(SessionData.arenaNo == 0 || SessionData.arenaNo == arenaNo)
        {
            currentArenaTimes[arenaNo - 1] = time;
            string timeInMins = Utilities.CovertTimeToString(time);
            Texts_ArenaTimer[arenaNo - 1].text = timeInMins;
        }

    }

    public void InGameSettingsButton()
    {
        PersistantUI.Instance.ShowSettingsPopup();
    }


    #region MOD PANEL

    public void GoToPreviousArena()
    {
        Cameras_Arena[currentSelectedArena - 1].depth = 0;
        Panels_Arena[currentSelectedArena - 1].SetActive(false);
        playPauseButtons[currentSelectedArena - 1].SetActive(false);
        moderatorCameraFollows[currentSelectedArena - 1].UpdateCameraFollow(false);

        currentSelectedArena--;
        if (currentSelectedArena == 0) currentSelectedArena = currentlyOccupiedArenas;

        if (arenaTimers[currentSelectedArena - 1].IsPaused()) Text_Spectating.text = "Session Paused";
        else Text_Spectating.text = "In Session!";

        Text_ArenaNo.text = $"Arena {currentSelectedArena}";

        if (arenaSessionOver[currentSelectedArena - 1]) Text_Spectating.text = "Session Finished!";

        Cameras_Arena[currentSelectedArena - 1].depth = 1;
        Panels_Arena[currentSelectedArena - 1].SetActive(true);
        playPauseButtons[currentSelectedArena - 1].SetActive(!arenaSessionOver[currentSelectedArena - 1]);
        moderatorCameraFollows[currentSelectedArena - 1].UpdateCameraFollow(true);

    }

    public void GoToNextArena()
    {
        Cameras_Arena[currentSelectedArena - 1].depth = 0;
        Panels_Arena[currentSelectedArena - 1].SetActive(false);
        playPauseButtons[currentSelectedArena - 1].SetActive(false);
        moderatorCameraFollows[currentSelectedArena - 1].UpdateCameraFollow(false);

        currentSelectedArena++;
        if (currentSelectedArena == currentlyOccupiedArenas + 1) currentSelectedArena = 1;

        if (arenaTimers[currentSelectedArena - 1].IsPaused()) Text_Spectating.text = "Session Paused";
        else Text_Spectating.text = "In Session!";

        Text_ArenaNo.text = $"Arena {currentSelectedArena}";

        if (arenaSessionOver[currentSelectedArena - 1]) Text_Spectating.text = "Session Finished!";

        Cameras_Arena[currentSelectedArena - 1].depth = 1;
        Panels_Arena[currentSelectedArena - 1].SetActive(true);
        playPauseButtons[currentSelectedArena - 1].SetActive(!arenaSessionOver[currentSelectedArena - 1]);
        moderatorCameraFollows[currentSelectedArena - 1].UpdateCameraFollow(true);
    }

    public void OnPlayPauseButton(int arenaNo)
    {
        if (arenaTimers[arenaNo - 1].IsPaused())
        {
            playPauseImages[arenaNo - 1].sprite = playPauseSprites[0];
            Text_Spectating.text = "In Session";
            arenaTimers[arenaNo - 1].PlayTimer();       
        }
        else
        {
            playPauseImages[arenaNo - 1].sprite = playPauseSprites[1];
            Text_Spectating.text = "Session Paused";
            arenaTimers[arenaNo - 1].PauseTimer();          
        }

        photonView.RPC(nameof(SendArenaPauseStateToNetwork), RpcTarget.All, arenaTimers[arenaNo - 1].IsPaused(), arenaNo);


    }

    [PunRPC]
    private void SendArenaPauseStateToNetwork(bool pauseState, int arenaNo)
    {
        currentArenaPauseStatus[arenaNo - 1] = pauseState;

        if(arenaNo == SessionData.arenaNo)
        {
            if(pauseState)
            {
                Panel_Pause.SetActive(true);
                GameMessages.OnPauseGame?.Invoke();
            }
            else
            {
                Panel_Pause.SetActive(false);
                GameMessages.OnPlayGame?.Invoke();
            }
        }
    }


    public void GoToLobby()
    {
        PhotonNetwork.LoadLevel(Identifiers_Mul.LobbyScene);
    }


    public void ReloadLevel()
    {
        PhotonNetwork.LoadLevel(3);
    }

    public void RandomTeams()
    {

    }

    public void CloseRoom()
    {
        photonView.RPC(nameof(LeaveRoomOnClient), RpcTarget.All);
    }

    [PunRPC]
    private void LeaveRoomOnClient()
    {
        PhotonNetwork.LeaveRoom(false);
    }

    #endregion


    #region GAME_END

    private void ArenaSessionOver(int arenaNo)
    {
        photonView.RPC(nameof(ArenaSessionOverOnTheNetwork), RpcTarget.All, arenaNo);
    }

    [PunRPC]
    private void ArenaSessionOverOnTheNetwork(int arenaNo)
    {
        CalculateWinnerOnTimerEnd(arenaNo);

        if (arenaNo == SessionData.arenaNo) GameMessages.OnGameSessionEnded?.Invoke();

        if (SessionData.buildType == BuildType.Session_Moderator) UpdateArenaUIOnModerator(arenaNo);
    }

    private void CalculateWinnerOnTimerEnd(int arenaNo)
    {
        int teamAScore = GameMenuManager.Instance.GetTeamScore(arenaNo, 0);
        int teamBScore = GameMenuManager.Instance.GetTeamScore(arenaNo, 1);

        if (teamAScore == teamBScore) UpdateArenaRankingsForDraw(arenaNo);
        else if (teamAScore > teamBScore) UpdateAreanRankings(arenaNo, 0);
        else if (teamBScore > teamAScore) UpdateAreanRankings(arenaNo, 1);

    }

    private void TurnOnEndPanel()
    {
        Panels_Arena[SessionData.arenaNo - 1].SetActive(false);
        Popup_EndPanel.SetActive(true);
    }

    private void UpdateAreanRankings(int arenaNo, int team)
    {
        arenaWinnerTeams[arenaNo - 1].text = $"TEAM {(Teams)team}";
        arenaWinnerPlayers[arenaNo - 1].text = GetWinnerPlayerList(arenaNo, team);

    }

    private void UpdateArenaRankingsForDraw(int arenaNo)
    {
        arenaWinnerPlayers[arenaNo - 1].text = "DRAW";

        //All players in the arena
        string winnerPlayerList = "";
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if((int)players[i].CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo] == arenaNo)
                winnerPlayerList += string.IsNullOrEmpty(winnerPlayerList) ? players[i].NickName : $", {players[i].NickName}";
        }

        arenaWinnerPlayers[arenaNo - 1].text = winnerPlayerList;

    }

    private string GetWinnerPlayerList(int arenaNo, int team)
    {
        string winnerPlayerList = "";
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if((int)players[i].CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo] == arenaNo 
                && (int)players[i].CustomProperties[Identifiers_Mul.PlayerSettings.TeamNo] == team)
            {
                winnerPlayerList += string.IsNullOrEmpty(winnerPlayerList) ? players[i].NickName : $", {players[i].NickName}";
            }
        }

        return winnerPlayerList;

    }

    private void UpdateArenaUIOnModerator(int arenaNo)
    {
        arenaSessionOver[arenaNo - 1] = true;
        arenaTimers[arenaNo - 1].PauseTimer();
        playPauseButtons[arenaNo-1].SetActive(false);
        if (currentSelectedArena == arenaNo) Text_Spectating.text = "Session Finished!";

        bool allArenasFinished = true;
        for (int i = 0; i < currentlyOccupiedArenas; i++)
        {
            if (!arenaSessionOver[i])
            {
                allArenasFinished = false;
                break;
            }
        }

        if (allArenasFinished)
        {
            Panels_Arena[currentSelectedArena - 1].SetActive(false);
            Popup_EndPanel.SetActive(true);
        }
    }

    #endregion

    private void SetTimer()
    {
        for (int i = 0; i < currentlyOccupiedArenas; i++)
        {
            arenaTimers[i].SetAndStartTimer(currentArenaTimes[i]);
        }
    }

    private void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(Identifiers_Mul.MainMenuScene);
    }


}