using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

//Manages In-Room Lobby functionality (Both Visual and Backend)
public class RoomLobbyManager : MonoBehaviour
{
    [Header("Lobby UI")]
    [SerializeField] TextMeshProUGUI Text_CurrentRoomName;

    [Header("Lobby UI_Player")]
    [SerializeField] GameObject Panel_PlayerSideUI;
    [SerializeField] GameObject Popup_PlayerList;

    [Header("Arena_Player")]
    [SerializeField] GameObject Button_Arena;
    [SerializeField] GameObject Popup_ArenaPlayers;
    [SerializeField] GameObject[] GO_ArenaPlayers;
    [SerializeField] TextMeshProUGUI Text_ArenaPlayersTitle;

    [Header("Tutorial_Player")]
    [Tooltip("Time after which the tutorial automatically shifts")]
    [SerializeField] int transitionTime;
    [SerializeField] Image Image_Tutorial;
    [SerializeField] Sprite[] Sprites_Tutorial;
    [SerializeField] GameObject[] GO_TutorialPageNavig;

    [Header("Lobby UI_Moderator")]
    [SerializeField] GameObject Panel_ModeratorSideUI;

    [Header("Game Settings_Moderator")]
    [SerializeField] GameObject Popup_GameSettings;
    [SerializeField] GameObject[] Tabs_GameSettings;
    [SerializeField] GameObject[] Panels_GameSettins;
    [SerializeField] Slider gameTimeSlider;
    [SerializeField] TextMeshProUGUI gameTimeText;

    [Header("Player List_Moderator")]
    [SerializeField] TextMeshProUGUI Text_PlayerListTitle_Mod;
    [SerializeField] GameObject[] GO_Players_Mod;
    [SerializeField] GameObject NoPlayers;

    [Header("Player List_Player")]
    [SerializeField] TextMeshProUGUI Text_PlayerListTitle_Player;
    [SerializeField] GameObject[] GO_Players;

    [Header("ArenaList_Moderator")]
    [SerializeField] TextMeshProUGUI[] Text_ArenaPlayerTexts;
    [SerializeField] GameObject StartButton;

    [Header("Manual Arena Selection_Moderator")]
    [SerializeField] GameObject[] GO_PlayerSelectionButtons;
    [SerializeField] GameObject[] GO_PlayersSelected;
    [SerializeField] GameObject[] GO_PlayerTexts;
    [SerializeField] GameObject[] GO_ArenaSelectionButtons;
    
    private TimerUp tutorialTimer;
    private int currentTutorialPage = 0;

    private TextMeshProUGUI[] allPlayersText_Mod;
    private TextMeshProUGUI[] allPlayersText;

    
    private int currentSelectedPlayer = -1;
    private int noOfArenasInUse = 0;

    private bool onGoingAssignement = false;    
    private int requiredArenas = -1;
    Hashtable playerProperties;

 
    private void Awake()
    {
        LobbyMenu_Init();
    }

    private void Start()
    {
        OpenLobbyPanel();
    }

    private void Update()
    {
        //Continuosly run timer for automatic tutorial swiping 
        tutorialTimer.UpdateTimer();
    }

    private void OnEnable()
    {
        tutorialTimer.TimerCompleted += Tutorial_NextButton;

        ServerMesseges.OnPlayerJoinedRoom += UpdatePlayerListUI;
        ServerMesseges.OnPlayerLeftRoom += UpdatePlayerListUI;
        ServerMesseges.OnPlayerPropertiesUpdate += UpdatePlayerArenaUI;
        ServerMesseges.OnLeaveRoom += GoBackToMainMenu;

        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDisable()
    {
        tutorialTimer.TimerCompleted -= Tutorial_NextButton;
        ServerMesseges.OnPlayerJoinedRoom -= UpdatePlayerListUI;
        ServerMesseges.OnPlayerLeftRoom -= UpdatePlayerListUI;
        ServerMesseges.OnPlayerPropertiesUpdate -= UpdatePlayerArenaUI;
        ServerMesseges.OnLeaveRoom -= GoBackToMainMenu;

        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    //Recieved Network Event 
    private void NetworkingClient_EventReceived(EventData obj)
    {
        //Moderator closed room event
        if (obj.Code == Identifiers_Mul.NetworkEvents.CloseRoomForEveryone) PlayerLeaveRoom(false); //Closed Room -> kickPlayer = false

        //Kick out player event
        if (obj.Code == Identifiers_Mul.NetworkEvents.CloseRoomForPlayer)
        {
            SessionData.inTimeout = true;
            PlayerLeaveRoom(true); //Kick Player -> kickPlayer = true
        }
    }

    private void LobbyMenu_Init()
    {

        //Player side initializations 
        Tutorial_Init();

        //Moderator side initializations
        PlayerListLobby_Init();
        GameSettings_Init();

        //Common Arena initializations
        Arena_Init();

    }

    private void Tutorial_Init()
    {
        Image_Tutorial.sprite = Sprites_Tutorial[0];

        //Init setup for the tutorial menu
        for (int i = 0; i < GO_TutorialPageNavig.Length; i++)
        {
            if (i < Sprites_Tutorial.Length) GO_TutorialPageNavig[i].SetActive(true);
            else GO_TutorialPageNavig[i].SetActive(false);
        }

        // A new timer up counter
        tutorialTimer = new TimerUp(transitionTime);
    }

    private void GameSettings_Init()
    {
        Popup_GameSettings.SetActive(false);

        //Game timer init
        gameTimeSlider.value = SessionData.sessionGameTime / 60; //Convert to mins
        gameTimeText.text = $"{gameTimeSlider.value} mins";

    }

    private void Arena_Init()
    {
        //Player Arena
        Button_Arena.SetActive(false);
        Popup_ArenaPlayers.SetActive(false);

        for (int i = 0; i < GO_ArenaPlayers.Length; i++)
        {
            GO_ArenaPlayers[i].SetActive(false);
        }

        //Mod Arena UI

        StartButton.SetActive(true);
        RefreshManualArenaSelectionUI();

        //Mod Arena

        if (SessionData.buildType == BuildType.Session_Moderator) SessionData.localArenaNo = 0;

        //Initialize Hashtable with arenaNo
        playerProperties = new Hashtable();
        playerProperties.Add(Identifiers_Mul.PlayerSettings.ArenaNo, SessionData.localArenaNo);

        //Update initialArenaNo on the server
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        Panel_PlayerSideUI.SetActive(false);
        Panel_ModeratorSideUI.SetActive(false);

    }

    private void RefreshManualArenaSelectionUI()
    {
        for (int i = 0; i < GO_PlayerTexts.Length; i++)
        {
            GO_PlayerSelectionButtons[i].SetActive(false);
            GO_PlayerTexts[i].SetActive(true);
            GO_PlayersSelected[i].SetActive(false);
        }


    }

    private void PlayerListLobby_Init()
    {
        Popup_PlayerList.SetActive(false);

        allPlayersText_Mod = new TextMeshProUGUI[GO_Players_Mod.Length];
        allPlayersText = new TextMeshProUGUI[GO_Players.Length];

        for (int i = 0; i < GO_Players_Mod.Length; i++)
        {
            allPlayersText_Mod[i] = GO_PlayerTexts[i].GetComponent<TextMeshProUGUI>();
            GO_Players_Mod[i].SetActive(false);
        }

        for (int i = 0; i < GO_Players.Length; i++)
        {
            allPlayersText[i] = GO_Players[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            GO_Players[i].SetActive(false);
        }

        NoPlayers.SetActive(true);

    }

    //Open lobby panel on start
    private void OpenLobbyPanel()
    {
        Text_CurrentRoomName.text = PhotonNetwork.CurrentRoom.Name;
        OpenPanelBasedOnBuildType();

        //Update Player List UI based on joining lobby
        UpdatePlayerListUI(null);

    }

    //Update UI based on type of build
    private void OpenPanelBasedOnBuildType()
    {
        switch (SessionData.buildType)
        {
            case BuildType.Session_Moderator:               
                ArenaRequirement_Init();
                RefreshModeratorArenaList();
                Text_PlayerListTitle_Mod.text = $"0 <#b3bedb>/</color> {PhotonNetwork.CurrentRoom.MaxPlayers - 1}";
                Panel_ModeratorSideUI.SetActive(true);
                break;
            case BuildType.Session_Player:
                Panel_PlayerSideUI.SetActive(true);
                tutorialTimer.StartTimer();
                break;
            case BuildType.Global_Build:
                break;
        }
    }

    private void ArenaRequirement_Init()
    {
        requiredArenas = (int)PhotonNetwork.CurrentRoom.CustomProperties[Identifiers_Mul.PlayerSettings.AvailableArenas];

        for (int i = 0; i < GO_ArenaSelectionButtons.Length; i++)
        {
            GO_ArenaSelectionButtons[i].SetActive(i < requiredArenas);
        }
    }

    //Open Common Settings Menu on click
    public void SettingsButton() => PersistantUI.Instance.ShowSettingsPopup();

    //Update player list when player joins/leaves room
    private void UpdatePlayerListUI(Player player)
    {

        //Player Side Refresh
        if (SessionData.buildType == BuildType.Session_Player)
        {
            Player[] allPlayers = PhotonNetwork.PlayerList;

            Text_PlayerListTitle_Player.text = $"Lobby <size=40><#6A6A6A> Total Players: {allPlayers.Length - 1}";
            for (int i = 0; i < GO_Players.Length; i++)
            {
                if (i < allPlayers.Length)
                {
                    if (allPlayers[i].CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo] != null)
                    {
                        if ((int)allPlayers[i].CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo] == 0) continue;

                    }

                    GO_Players[i].SetActive(true);
                    allPlayersText[i].text = allPlayers[i].NickName;

                }
                else
                    GO_Players[i].SetActive(false);
            }
        }
        

        //Moderator Side Refresh
        if(SessionData.buildType == BuildType.Session_Moderator)
        {
            RefreshModeratorArenaList();

            //Player List Refresh
            Player[] otherPlayers = PhotonNetwork.PlayerListOthers;

            NoPlayers.SetActive(otherPlayers.Length == 0);
            Text_PlayerListTitle_Mod.text = $"{otherPlayers.Length} <#b3bedb>/</color> {PhotonNetwork.CurrentRoom.MaxPlayers - 1}";

            for (int i = 0; i < GO_Players_Mod.Length; i++)
            {
                if (i < otherPlayers.Length)
                {
                    GO_Players_Mod[i].SetActive(true);
                    allPlayersText_Mod[i].text = otherPlayers[i].NickName;

                }
                else
                    GO_Players_Mod[i].SetActive(false);
            }
        }

    
    }

    #region PLAYER ROOM LOBBY

    //Open/Close LobbyPlayerList
    public void UpdatePlayerLobbyList(bool status) => Popup_PlayerList.SetActive(status);

    //Open/Close ArenaPlayerList
    public void UpdateArenaPlayerListUI(bool status) => Popup_ArenaPlayers.SetActive(status);

    private void UpdatePlayerArenaUI(Player targetPlayer)
    {
        if (SessionData.buildType == BuildType.Session_Moderator ) return;

        if(targetPlayer == PhotonNetwork.LocalPlayer)
        {
            //Ignore if it's the default arenaNo on the network 
            if ((int)targetPlayer.CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo] == -1)
            {
                Button_Arena.SetActive(false);
                Popup_ArenaPlayers.SetActive(false);
                PersistantUI.Instance.LogOnScreen("Waiting for moderator to assign you a arena");
                return;
            }

            SessionData.localArenaNo = (int)targetPlayer.CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo];

            Button_Arena.SetActive(true);
            Button_Arena.GetComponentInChildren<TextMeshProUGUI>().text = "Arena " + SessionData.localArenaNo;
            Text_ArenaPlayersTitle.text = $"Arena {SessionData.localArenaNo} <size=40><#6A6A6A> Total Players: 1";

            GO_ArenaPlayers[0].SetActive(true);
            GO_ArenaPlayers[0].GetComponentInChildren<TextMeshProUGUI>().text = PhotonNetwork.LocalPlayer.NickName;
        }

        if (SessionData.localArenaNo == -1) return;

        Player[] otherPlayersInRoom = PhotonNetwork.PlayerListOthers;

        int currentPlayersInArena = 1;

        for (int i = 1; i < 6; i++)
        {
            GO_ArenaPlayers[i].SetActive(false);
        }

        for (int i = 0; i < otherPlayersInRoom.Length; i++)
        {               
            if (SessionData.localArenaNo == (int)otherPlayersInRoom[i].CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo])
            {                
                GO_ArenaPlayers[currentPlayersInArena].SetActive(true);
                GO_ArenaPlayers[currentPlayersInArena].GetComponentInChildren<TextMeshProUGUI>().text = otherPlayersInRoom[i].NickName;
                currentPlayersInArena++;
            }
        }

        Text_ArenaPlayersTitle.text = $"Arena {SessionData.localArenaNo} <size=40><#6A6A6A> Total Players: {currentPlayersInArena}";

    }


    #region TUTORIAL UI

    //Update tutorial to previous
    public void Tutorial_PrevButton()
    {
        GO_TutorialPageNavig[currentTutorialPage].transform.GetChild(0).gameObject.SetActive(false);

        currentTutorialPage--;
        if (currentTutorialPage == -1) currentTutorialPage = Sprites_Tutorial.Length - 1; 

        Image_Tutorial.sprite = Sprites_Tutorial[currentTutorialPage];
        GO_TutorialPageNavig[currentTutorialPage].transform.GetChild(0).gameObject.SetActive(true);

        tutorialTimer.RestartTimer();
    }

    //Update tutorial to next
    public void Tutorial_NextButton()
    {
        GO_TutorialPageNavig[currentTutorialPage].transform.GetChild(0).gameObject.SetActive(false);
        currentTutorialPage++;
        if (currentTutorialPage == Sprites_Tutorial.Length) currentTutorialPage = 0; 

        Image_Tutorial.sprite = Sprites_Tutorial[currentTutorialPage];
        GO_TutorialPageNavig[currentTutorialPage].transform.GetChild(0).gameObject.SetActive(true);

        tutorialTimer.RestartTimer();
    }

    #endregion

    #endregion

    #region MODERATOR ROOM LOBBY

    private void ResetArenaPlayerListUI()
    {
        for (int i = 0; i < Text_ArenaPlayerTexts.Length; i++)
        {
            if (i < requiredArenas) Text_ArenaPlayerTexts[i].text = "No Assigned Players";
            else Text_ArenaPlayerTexts[i].text = "Inactive, not in use";
        }
    }

    //Assign Arenas to all the players currently in room
    public void AssignArenasToPlayersInLobby()
    {
        if (onGoingAssignement)
        {
            PersistantUI.Instance.LogOnScreen("Cannot assign arenas as its ongoing!");
            return;
        }



        if (PhotonNetwork.CurrentRoom.PlayerCount - 1 < requiredArenas*2)
        {
            PersistantUI.Instance.LogOnScreen($"Minimum {requiredArenas*2} players required for random arenas");
            //Error Sound
            return;
        }
        
        //Assign arenas to players
        StartCoroutine(AssignRandomArenas());

    }

    //To ensure that no player assignment gets lost on the network
    private IEnumerator AssignRandomArenas()
    {
        onGoingAssignement = true;

        currentSelectedPlayer = -1;
        RefreshManualArenaSelectionUI();
        ResetArenaPlayerListUI();
        SessionData.ResetPlayersPerArena();

        List<Player> scrambledPlayerList = SessionData.GetScrambledList(SessionData.cachedRoomPlayers);

        int currentArenaNo = 1;
        for (int i = 0; i < scrambledPlayerList.Count; i++)
        {
            //Set Arena for player
            playerProperties[Identifiers_Mul.PlayerSettings.ArenaNo] = currentArenaNo;
            scrambledPlayerList[i].SetCustomProperties(playerProperties);           
            SessionData.cachedPlayersArenaNo[SessionData.cachedRoomPlayers.IndexOf(scrambledPlayerList[i])] = currentArenaNo;
            SessionData.playersPerArena[currentArenaNo - 1]++;

            //Set Moderator side UI for assigned Player
            if (Text_ArenaPlayerTexts[currentArenaNo - 1].text == "No Assigned Players") Text_ArenaPlayerTexts[currentArenaNo - 1].text = "";

            Text_ArenaPlayerTexts[currentArenaNo - 1].text +=
                string.IsNullOrEmpty(Text_ArenaPlayerTexts[currentArenaNo - 1].text) ? 
                scrambledPlayerList[i].NickName : $", {scrambledPlayerList[i].NickName}";

            //Sequential increment of currentArenaNo
            currentArenaNo = currentArenaNo + 1 > requiredArenas ? 1 : currentArenaNo + 1;

            //0.03 gives us a range of 0.03 to 0.48 seconds to complete assignment of arenas
            yield return new WaitForSeconds(0.03f);

        }

        PersistantUI.Instance.LogOnScreen("Assigned random arenas to players!");
        onGoingAssignement = false;
    }

    public void FlipPlayerOptions(int playerNo)
    {
        GO_PlayerTexts[playerNo - 1].SetActive(!GO_PlayerTexts[playerNo - 1].activeSelf);
        GO_PlayerSelectionButtons[playerNo - 1].SetActive(!GO_PlayerSelectionButtons[playerNo - 1].activeSelf);
    }

    public void SelectPlayerToAssign(int selectedPlayer)
    {
        if( SessionData.cachedPlayersArenaNo[selectedPlayer - 1] >= 1)
        {
            PersistantUI.Instance.LogOnScreen("Player already has an arena, deselect it first to assign!");
            return;
        }

        if(currentSelectedPlayer == selectedPlayer)
        {
            GO_PlayersSelected[currentSelectedPlayer - 1].SetActive(false);
            currentSelectedPlayer = - 1;
            return;
        }

        if(currentSelectedPlayer != -1) GO_PlayersSelected[currentSelectedPlayer - 1].SetActive(false);

        currentSelectedPlayer = selectedPlayer;
        PersistantUI.Instance.LogOnScreen($"Player: {SessionData.cachedRoomPlayers[currentSelectedPlayer - 1].NickName} selected!");
        GO_PlayersSelected[currentSelectedPlayer - 1].SetActive(true);
    }

    public void AssignArenaToPlayer(int arenaNo)
    {
        if(currentSelectedPlayer == -1 )
        {
            PersistantUI.Instance.LogOnScreen("No player selected for assignment!");
            return;
        }

        //Assignment of arena
        SessionData.playersPerArena[arenaNo - 1]++;
        playerProperties[Identifiers_Mul.PlayerSettings.ArenaNo] = arenaNo;
        SessionData.cachedRoomPlayers[currentSelectedPlayer - 1].SetCustomProperties(playerProperties);
        SessionData.cachedPlayersArenaNo[currentSelectedPlayer - 1] = arenaNo;

        RefreshModeratorArenaList();
        PersistantUI.Instance.LogOnScreen($"Player: {SessionData.cachedRoomPlayers[currentSelectedPlayer - 1].NickName} is assigned Arena {arenaNo}");
        GO_PlayersSelected[currentSelectedPlayer - 1].SetActive(false);
        currentSelectedPlayer = -1;

    }

    public void DeassignArenaForPlayer(int selectedPlayer)
    {
        int playerArenaNo = SessionData.cachedPlayersArenaNo[selectedPlayer - 1];
        if (playerArenaNo < 1)
        {
            PersistantUI.Instance.LogOnScreen("Player hasn't been assigned a arena yet");
            return;
        }

        //Deassign arena for player
        SessionData.playersPerArena[playerArenaNo - 1]--;
        playerProperties[Identifiers_Mul.PlayerSettings.ArenaNo] = -1;
        SessionData.cachedRoomPlayers[selectedPlayer - 1].SetCustomProperties(playerProperties);
        SessionData.cachedPlayersArenaNo[selectedPlayer - 1] = -1;
        PersistantUI.Instance.LogOnScreen($"Deassigned Arena for Player: {SessionData.cachedRoomPlayers[selectedPlayer - 1].NickName}");

        //Refresh UI
        RefreshModeratorArenaList();
    }

    public void StartGame()
    {
        string errorMessage = "Assign arena to: ";
        bool allAssignedArenas = true;
        noOfArenasInUse = 0;

        for (int i = 0; i < SessionData.cachedRoomPlayers.Count; i++)
        {
            int playerArenaNo = SessionData.cachedPlayersArenaNo[i];

            if (playerArenaNo < 1)
            {
                allAssignedArenas = false;
                errorMessage += errorMessage == "Assign arena to: " ?  $"{SessionData.cachedRoomPlayers[i].NickName} " 
                    : $",{SessionData.cachedRoomPlayers[i].NickName} ";
            }
        }

        if(!allAssignedArenas)
        {
            PersistantUI.Instance.LogOnScreen(errorMessage);
            return;
        }

        if(SessionData.playersPerArena[0] < 2)
        {
            PersistantUI.Instance.LogOnScreen($"Can't start game as Arena 1 doesn't meet minimum players");
            return;
        }

        for (int i = 0; i < requiredArenas; i++)
        {
            if (SessionData.playersPerArena[i] == 1)
            {
                PersistantUI.Instance.LogOnScreen($"Can't start game as Arena {i + 1} doesn't meet minimum players");
                return;
            }

            if (SessionData.playersPerArena[i] >= 2) noOfArenasInUse++;

            if(i >= 1)
            {
                if (SessionData.playersPerArena[i] >= 2 && SessionData.playersPerArena[i - 1] == 0)
                {
                    PersistantUI.Instance.LogOnScreen($"Can't start game as Arena {i + 1} is filled but Arena{i} isn't");
                    return;
                }
            }


        }

        //Change occupied arenas 
        if(noOfArenasInUse != (int)PhotonNetwork.CurrentRoom.CustomProperties[Identifiers_Mul.PlayerSettings.OccupiedArenas])
        {
            Hashtable changedOccupiedArenas = PhotonNetwork.CurrentRoom.CustomProperties;
            changedOccupiedArenas[Identifiers_Mul.PlayerSettings.OccupiedArenas] = noOfArenasInUse;
            PhotonNetwork.CurrentRoom.SetCustomProperties(changedOccupiedArenas);
        }


        PersistantUI.Instance.LogOnScreen("Starting Game");
        PhotonNetwork.LoadLevel(Identifiers_Mul.GameScene);
    }

    public void Moderator_CloseRoom()
    {
        //Reset arenaNos on the network
        for (int i = 0; i < SessionData.cachedRoomPlayers.Count; i++)
        {
            playerProperties[Identifiers_Mul.PlayerSettings.ArenaNo] = -1;
            SessionData.cachedRoomPlayers[i].SetCustomProperties(playerProperties);
        }

        PersistantUI.Instance.LogOnScreen("Closing Room: " + PhotonNetwork.CurrentRoom.Name);
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            PhotonNetwork.RaiseEvent(Identifiers_Mul.NetworkEvents.CloseRoomForEveryone, new object[] { }, RaiseEventOptions.Default, SendOptions.SendReliable);
        }

        Invoke(nameof(ModeratorLeaveRoom), 0.5f);
    }

    private void ModeratorLeaveRoom()
    {
        PhotonNetwork.LeaveRoom(false);
    }

    public void KickPlayer(int playerListNo)
    {
        currentSelectedPlayer = -1;
        if(SessionData.cachedPlayersArenaNo[playerListNo - 1] >= 1) SessionData.playersPerArena[SessionData.cachedPlayersArenaNo[playerListNo - 1] - 1]--;
        RefreshManualArenaSelectionUI();

        //Reset player arena No
        playerProperties[Identifiers_Mul.PlayerSettings.ArenaNo] = -1;
        SessionData.cachedRoomPlayers[playerListNo - 1].SetCustomProperties(playerProperties);

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.TargetActors = new int[1] { SessionData.cachedRoomPlayers[playerListNo - 1].ActorNumber };
        object[] obj = new object[] { };

        PhotonNetwork.RaiseEvent(Identifiers_Mul.NetworkEvents.CloseRoomForPlayer, obj, raiseEventOptions, SendOptions.SendReliable);
        
    }

    //Update the UI for a moderator when they rejoin the room lobby
    public void RefreshModeratorArenaList()
    {
        ResetArenaPlayerListUI();

        for (int i = 0; i < SessionData.cachedRoomPlayers.Count; i++)
        {
            int playerArenaNo = SessionData.cachedPlayersArenaNo[i];
            if (playerArenaNo <= 0) continue;
            //Set Moderator side UI for assigned Player
            if (Text_ArenaPlayerTexts[playerArenaNo - 1].text == "No Assigned Players") Text_ArenaPlayerTexts[playerArenaNo - 1].text = "";

            Text_ArenaPlayerTexts[playerArenaNo - 1].text +=
                string.IsNullOrEmpty(Text_ArenaPlayerTexts[playerArenaNo - 1].text) ? 
                SessionData.cachedRoomPlayers[i].NickName : $", {SessionData.cachedRoomPlayers[i].NickName}";
        }
    }

    //Open/Close game settings menu and save settings to player pref while closing
    public void UpdateGameSettingsUI(bool status)
    {
        Popup_GameSettings.SetActive(status);
        if (!status)
        {
            SessionData.sessionGameTime = (int)gameTimeSlider.value * 60;
        }
    }
    
    //Switch Tabs Based on tabNo in game settings menu
    public void OnClickMenu(int tabNo)
    {
        //Turn off current panel
        Panels_GameSettins[1 - tabNo].SetActive(false);
        Tabs_GameSettings[1 - tabNo].transform.GetChild(0).gameObject.SetActive(false);
        Tabs_GameSettings[1 - tabNo].transform.GetChild(1).gameObject.SetActive(true);

        //Turn on hit panel
        Panels_GameSettins[tabNo].SetActive(true);
        Tabs_GameSettings[tabNo].transform.GetChild(0).gameObject.SetActive(true);
        Tabs_GameSettings[tabNo].transform.GetChild(1).gameObject.SetActive(false);

    }

    //Update UI on moving the game time slider
    public void OnChangeGameTime()
    {
        gameTimeText.text = $"{gameTimeSlider.value} mins";
    }


    #endregion

    //Player leaves room when moderator closes the room
    private void PlayerLeaveRoom(bool kicked)
    {
        if (SessionData.buildType == BuildType.Session_Player)
        {
            if (kicked) PersistantUI.Instance.LogOnScreen("Kicked out of room");
            else PersistantUI.Instance.LogOnScreen("Moderator closed room.");
            PhotonNetwork.LeaveRoom(false);
        }
    }

    //On Leave Room go back to main menu
    private void GoBackToMainMenu()
    {
        SceneManager.LoadScene(Identifiers_Mul.MainMenuScene);
    }

}
