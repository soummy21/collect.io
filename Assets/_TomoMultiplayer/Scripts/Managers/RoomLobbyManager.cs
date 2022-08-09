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
    [Header("Common Room Lobby UI")]
    [SerializeField] TextMeshProUGUI Text_CurrentRoomName;

    [Header("Player Side Lobby UI")]
    [SerializeField] GameObject Panel_PlayerSideUI;
    [SerializeField] GameObject Popup_PlayerList;

    [Header("Player Arena")]
    [SerializeField] GameObject Button_Arena;
    [SerializeField] GameObject Popup_ArenaPlayers;
    [SerializeField] GameObject[] GO_ArenaPlayers;
    [SerializeField] TextMeshProUGUI Text_ArenaPlayersTitle;

    [Header("Tutorial UI")]
    [Tooltip("Time after which the tutorial automatically shifts")]
    [SerializeField] int transitionTime;
    [SerializeField] Image Image_Tutorial;
    [SerializeField] Sprite[] Sprites_Tutorial;
    [SerializeField] GameObject[] GO_TutorialPageNavig;

    [Header("Moderator Side Lobby UI")]
    [SerializeField] GameObject Panel_ModeratorSideUI;

    [Header("Game Settings")]
    [SerializeField] GameObject Popup_GameSettings;
    [SerializeField] GameObject[] Tabs_GameSettings;
    [SerializeField] GameObject[] Panels_GameSettins;
    [SerializeField] Slider gameTimeSlider;
    [SerializeField] TextMeshProUGUI gameTimeText;

    [Header("PlayerList")]
    [SerializeField] TextMeshProUGUI Text_PlayerListTitle_Mod;
    [SerializeField] GameObject[] GO_Players_Mod;
    [SerializeField] GameObject NoPlayers;
    [SerializeField] TextMeshProUGUI Text_PlayerListTitle_Player;
    [SerializeField] GameObject[] GO_Players;

    [Header("ArenaPlayerList")]
    [SerializeField] TextMeshProUGUI[] Text_ArenaPlayerTexts;
    [SerializeField] GameObject StartButton;

    private TimerUp tutorialTimer;
    private int currentTutorialPage = 0;

    private TextMeshProUGUI[] allPlayersText_Mod;
    private TextMeshProUGUI[] allPlayersText;

    private List<Player> cachedRoomPlayers = new List<Player>();
    private static int[] playersPerArena = new int[4];
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

        ServerMesseges.OnRoomPlayersStatusChange += UpdatePlayerListUI;
        ServerMesseges.OnPlayerPropertiesUpdate += UpdatePlayerArenaUI;
        ServerMesseges.OnLeaveRoom += GoBackToMainMenu;

        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDisable()
    {
        tutorialTimer.TimerCompleted -= Tutorial_NextButton;
        ServerMesseges.OnRoomPlayersStatusChange -= UpdatePlayerListUI;
        ServerMesseges.OnPlayerPropertiesUpdate -= UpdatePlayerArenaUI;
        ServerMesseges.OnLeaveRoom -= GoBackToMainMenu;

        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    //Recieved Network Event 
    private void NetworkingClient_EventReceived(EventData obj)
    {
        //Moderator closed room event
        if (obj.Code == Identifiers_Mul.NetworkEvents.CloseRoomForEveryone) PlayerLeaveRoom(false);

        //Kick out player event
        if (obj.Code == Identifiers_Mul.NetworkEvents.CloseRoomForPlayer)
        {
            object[] data = (object[])obj.CustomData;

            if ((string)data[0] == SessionData.playerName)
            {
                SessionData.inTimeout = true;
                PlayerLeaveRoom(true);               
            }

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

        //Mod Arena
        StartButton.SetActive(true);
        if (SessionData.buildType == BuildType.Session_Moderator) SessionData.arenaNo = 0;

        //Initialize Hashtable with arenaNo
        playerProperties = new Hashtable();
        playerProperties.Add(Identifiers_Mul.PlayerSettings.ArenaNo, SessionData.arenaNo);

        //Update initialArenaNo on the server
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

    }

    private void PlayerListLobby_Init()
    {
        Popup_PlayerList.SetActive(false);

        allPlayersText_Mod = new TextMeshProUGUI[GO_Players_Mod.Length];
        allPlayersText = new TextMeshProUGUI[GO_Players.Length];

        for (int i = 0; i < GO_Players_Mod.Length; i++)
        {
            allPlayersText_Mod[i] = GO_Players_Mod[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
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
        UpdatePlayerListUI(null, false);

    }

    //Update UI based on type of build
    private void OpenPanelBasedOnBuildType()
    {
        switch (SessionData.buildType)
        {
            case BuildType.Session_Moderator:
                Text_PlayerListTitle_Mod.text = $"0 <#b3bedb>/</color> {PhotonNetwork.CurrentRoom.MaxPlayers - 1}";
                requiredArenas = (int)PhotonNetwork.CurrentRoom.CustomProperties[Identifiers_Mul.PlayerSettings.NoOfArenas];
                ResetArenaPlayerListUI();
                RefreshModeratorArenaList();
                Panel_ModeratorSideUI.SetActive(true);
                break;
            case BuildType.Session_Player:
                Panel_PlayerSideUI.SetActive(true);
                tutorialTimer.StartTimer();
                Invoke(nameof(LogWaitingForArena), 1.2f);
                break;
            case BuildType.Global_Build:
                break;
        }
    }

    private void LogWaitingForArena() => PersistantUI.Instance.LogOnScreen("Waiting for moderator to assign you a arena");

    //Open Common Settings Menu on click
    public void SettingsButton() => PersistantUI.Instance.ShowSettingsPopup();

    //Update player list when player joins/leaves room
    private void UpdatePlayerListUI(Player player, bool newPlayer)
    {
        //Update player list

        if(player != null)
        {
            if (newPlayer) cachedRoomPlayers.Add(player);
            else cachedRoomPlayers.Remove(player);

        }

        //Player Side
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
        

        //Moderator Side
        if(SessionData.buildType == BuildType.Session_Moderator)
        {
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
                return;
            }

            SessionData.arenaNo = (int)targetPlayer.CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo];
            PersistantUI.Instance.LogOnScreen($"Joined Arena {SessionData.arenaNo}");

            Button_Arena.SetActive(true);
            Button_Arena.GetComponentInChildren<TextMeshProUGUI>().text = "Arena " + SessionData.arenaNo;
            Text_ArenaPlayersTitle.text = $"Arena {SessionData.arenaNo} <size=40><#6A6A6A> Total Players: 1";

            GO_ArenaPlayers[0].SetActive(true);
            GO_ArenaPlayers[0].GetComponentInChildren<TextMeshProUGUI>().text = PhotonNetwork.LocalPlayer.NickName;
        }

        if (SessionData.arenaNo == -1) return;

        Player[] otherPlayersInRoom = PhotonNetwork.PlayerListOthers;

        int currentPlayersInArena = 1;

        for (int i = 1; i < 6; i++)
        {
            GO_ArenaPlayers[i].SetActive(false);
        }

        for (int i = 0; i < otherPlayersInRoom.Length; i++)
        {               
            if (SessionData.arenaNo == (int)otherPlayersInRoom[i].CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo])
            {                
                GO_ArenaPlayers[currentPlayersInArena].SetActive(true);
                GO_ArenaPlayers[currentPlayersInArena].GetComponentInChildren<TextMeshProUGUI>().text = otherPlayersInRoom[i].NickName;
                currentPlayersInArena++;
            }
        }

        Text_ArenaPlayersTitle.text = $"Arena {SessionData.arenaNo} <size=40><#6A6A6A> Total Players: {currentPlayersInArena}";

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

    private void ResetArenaList()
    {
        for (int i = 0; i < playersPerArena.Length; i++)
        {
            playersPerArena[i] = 0;
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
        StartCoroutine(AssignPlayerAreans());

    }

    //To ensure that no player assignment gets lost on the network
    private IEnumerator AssignPlayerAreans()
    {
        onGoingAssignement = true;

        ResetArenaList();

        int[] remainingSpots = new int[requiredArenas]; //per arena spots for random assignment
        int arenaNoCount = 1;
        //Fairly devide the no of players in each arena
        for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers - 1; i++)
        {
            remainingSpots[arenaNoCount - 1]++;

            arenaNoCount++;
            if (arenaNoCount > requiredArenas) arenaNoCount = 1;
        }

        Player[] otherPlayers = PhotonNetwork.PlayerListOthers;

        ResetArenaPlayerListUI();

        for (int i = 0; i < otherPlayers.Length; i++)
        {
            int currentArenaNo = GiveRandomArenaNo(remainingSpots);
            //Set Arena for player
            playerProperties[Identifiers_Mul.PlayerSettings.ArenaNo] = currentArenaNo;
            otherPlayers[i].SetCustomProperties(playerProperties);
            playersPerArena[currentArenaNo - 1]++;

            //Set Moderator side UI for assigned Player
            if (Text_ArenaPlayerTexts[currentArenaNo - 1].text == "No Assigned Players") Text_ArenaPlayerTexts[currentArenaNo - 1].text = "";

            Text_ArenaPlayerTexts[currentArenaNo - 1].text +=
                string.IsNullOrEmpty(Text_ArenaPlayerTexts[currentArenaNo - 1].text) ? otherPlayers[i].NickName : $", {otherPlayers[i].NickName}";


            //0.03 gives us a range of 0.03 to 0.48 seconds to complete assignment of arenas
            yield return new WaitForSeconds(0.03f);

        }

        PersistantUI.Instance.LogOnScreen("Assigned random arenas to players!");

        onGoingAssignement = false;
    }

    public void SelectPlayerToAssign(int selectedPlayer)
    {
        if((int) cachedRoomPlayers[selectedPlayer].CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo] > 1)
        {
            PersistantUI.Instance.LogOnScreen("Player already has an arena, deselect it first to assign!");
            return;
        }
        currentSelectedPlayer = selectedPlayer;
    }

    public void AssignArenaToPlayer(int arenaNo)
    {
        if(currentSelectedPlayer == -1 )
        {
            PersistantUI.Instance.LogOnScreen("No player selected for assignment!");
            return;
        }

        if (arenaNo > requiredArenas)
        {
            PersistantUI.Instance.LogOnScreen("Arena Not active!");
            return;
        }

        //Assignment of arena
        Hashtable changedArena = cachedRoomPlayers[currentSelectedPlayer - 1].CustomProperties;
        playersPerArena[(int)changedArena[Identifiers_Mul.PlayerSettings.ArenaNo] - 1]++;
        changedArena[Identifiers_Mul.PlayerSettings.ArenaNo] = arenaNo;
        cachedRoomPlayers[currentSelectedPlayer - 1].SetCustomProperties(changedArena);

        RefreshModeratorArenaList();

        currentSelectedPlayer = -1;

    }

    public void DeassignArenaForPlayer(int selectedPlayer)
    {
        if((int)cachedRoomPlayers[selectedPlayer].CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo] < 1)
        {
            PersistantUI.Instance.LogOnScreen("Player hasn't been assigned a arena yet");
            return;
        }

        //Deassign arena for player
        Hashtable changedArena = cachedRoomPlayers[selectedPlayer - 1].CustomProperties;
        playersPerArena[(int)changedArena[Identifiers_Mul.PlayerSettings.ArenaNo] - 1]--;
        changedArena[Identifiers_Mul.PlayerSettings.ArenaNo] = -1;
        cachedRoomPlayers[selectedPlayer - 1].SetCustomProperties(changedArena);

        //Refresh UI
        RefreshModeratorArenaList();
    }

    //Gives a valid random arena
    private int GiveRandomArenaNo(int [] remainingSpots)
    {
        int randArenaNo = Random.Range(1, requiredArenas + 1);
        if(remainingSpots[randArenaNo - 1] == 0)
        {
            return GiveRandomArenaNo(remainingSpots);
        }
        remainingSpots[randArenaNo - 1]--;
        return randArenaNo;
    }

    public void StartGame()
    {
        Player[] otherPlayers = PhotonNetwork.PlayerListOthers;
        string errorMessage = "Assign arena to: ";
        bool allAssignedArenas = true;

        for (int i = 0; i < otherPlayers.Length; i++)
        {
            if((int)otherPlayers[i].CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo] == -1)
            {
                allAssignedArenas = false;
                errorMessage += errorMessage == "Assign arena to: " ?  $"{otherPlayers[i].NickName} " : $",{otherPlayers[i].NickName} ";
            }
        }

        if(!allAssignedArenas)
        {
            PersistantUI.Instance.LogOnScreen(errorMessage);
            return;
        }


        for (int i = 0; i < requiredArenas; i++)
        {

        }

        PersistantUI.Instance.LogOnScreen("Starting Game");
        PhotonNetwork.LoadLevel(Identifiers_Mul.GameScene);
    }

    public void Moderator_CloseRoom()
    {
        PersistantUI.Instance.LogOnScreen("Closing Room: " + PhotonNetwork.CurrentRoom.Name);
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            PhotonNetwork.RaiseEvent(Identifiers_Mul.NetworkEvents.CloseRoomForEveryone, new object[] { }, RaiseEventOptions.Default, SendOptions.SendReliable);
        }
        PhotonNetwork.LeaveRoom(false);
    }

    public void KickPlayer(int playerListNo)
    {
        string playerName = allPlayersText_Mod[playerListNo].text;
        object[] data = new object[] { playerName };

        PhotonNetwork.RaiseEvent(Identifiers_Mul.NetworkEvents.CloseRoomForPlayer,data, RaiseEventOptions.Default, SendOptions.SendReliable);
        
    }

    //Update the UI for a moderator when they rejoin the room lobby
    public void RefreshModeratorArenaList()
    {
        Player[] players = PhotonNetwork.PlayerListOthers;

        for (int i = 0; i < players.Length; i++)
        {
            int playerArenaNo = (int)players[i].CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo];
            if (playerArenaNo <= 0) continue;
            //Set Moderator side UI for assigned Player
            if (Text_ArenaPlayerTexts[playerArenaNo - 1].text == "No Assigned Players") Text_ArenaPlayerTexts[playerArenaNo - 1].text = "";

            Text_ArenaPlayerTexts[playerArenaNo - 1].text +=
                string.IsNullOrEmpty(Text_ArenaPlayerTexts[playerArenaNo - 1].text) ? players[i].NickName : $", {players[i].NickName}";
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
        playerProperties[Identifiers_Mul.PlayerSettings.ArenaNo] = -1;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        SessionData.arenaNo = -1;
        SceneManager.LoadScene(Identifiers_Mul.MainMenuScene);

    }

}
