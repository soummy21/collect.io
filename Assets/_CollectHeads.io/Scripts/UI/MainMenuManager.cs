using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject loadingScreen;
    [SerializeField] GameObject lobbyMenu;
    [SerializeField] GameObject roomMenu;

    [Header("Connect UI Elements")]
    [SerializeField] GameObject tryLoadingAgainButton;
    [SerializeField] TextMeshProUGUI loadingText;

    [Header("Lobby UI Elements")]
    [SerializeField] GameObject lobbyButtonHolder;
    [SerializeField] TMP_InputField playerNameField;
    [SerializeField] Button createRoomButton;
    [SerializeField] TMP_InputField createRoomInput;
    [SerializeField] TMP_InputField joinRoomInput;
    [SerializeField] Button joinRoomButtton;

    [Header("Room UI Elements")]
    [SerializeField] TextMeshProUGUI roomNameText;
    [SerializeField] TextMeshProUGUI playerNameList;
    [SerializeField] GameObject gameStartButton;


    private List<string> activePlayers = new List<string>();

    private void Awake()
    { 

        //Loading Setup
        loadingScreen.SetActive(!PhotonNetwork.InRoom);
        tryLoadingAgainButton.SetActive(false);

        //Lobby Setup
        lobbyMenu.SetActive(false);
        lobbyButtonHolder.SetActive(false);

        //Room Setup
        roomMenu.SetActive(PhotonNetwork.InRoom);

    }

    private void OnEnable()
    {
        ServerEvents.OnServerConnected += OpenLobbyMenu;
        ServerEvents.OnFailedToJoinServer += TryAgain;
        ServerEvents.OnJoinedRoom += GoToRoomMenu;
        ServerEvents.PlayerJoinedRoom += AddToPlayerList;
        ServerEvents.PlayerLeftRoom += RemoveFromPlayerList;
        ServerEvents.OnMasterClientChanged += UpdateStartUI;

        MainMenuEvents.RoomMenuStatus += RoomMenuUIStatus;

    }

    private void OnDisable()
    {
        ServerEvents.OnServerConnected -= OpenLobbyMenu;
        ServerEvents.OnFailedToJoinServer -= TryAgain;
        ServerEvents.OnJoinedRoom -= GoToRoomMenu;
        ServerEvents.PlayerJoinedRoom -= AddToPlayerList;
        ServerEvents.PlayerLeftRoom -= RemoveFromPlayerList;
        ServerEvents.OnMasterClientChanged -= UpdateStartUI;

        MainMenuEvents.RoomMenuStatus -= RoomMenuUIStatus;

    }

    #region LOADING

    private void TryAgain()
    {
        loadingText.text = "Failed To Connect";
        tryLoadingAgainButton.SetActive(true);
    }

    public void OnTryAgainButtonClicked()
    {
        tryLoadingAgainButton.SetActive(false);
        loadingText.text = "Loading....";
    }

    #endregion

    private void OpenLobbyMenu()
    {
        loadingScreen.SetActive(false);
        tryLoadingAgainButton.SetActive(false);
        lobbyMenu.SetActive(true);
    }

    public void OnNameInput()
    {
        if (string.IsNullOrEmpty(playerNameField.text)) return;

        //Set Player Name
        MainMenuEvents.OnPlayerNameInput?.Invoke(playerNameField.text);
        //Can now make lobbies
        lobbyButtonHolder.SetActive(true);
    }

    public void OnCreateRoomInput()
    {
        if (string.IsNullOrEmpty(createRoomInput.text)) return;
        createRoomButton.interactable = true;
     }

    public void OnCreateRoomButtonClick()
    {
        MainMenuEvents.OnCreateRoomInput?.Invoke(createRoomInput.text);
    }

    public void OnJoinRoomInput()
    {
        if (string.IsNullOrEmpty(joinRoomInput.text)) return;
        joinRoomButtton.interactable = true;
    }

    public void OnJoinRoomButtonClick()
    {
        MainMenuEvents.OnJoinRoomInput?.Invoke(joinRoomInput.text);
    }

    private void GoToRoomMenu()
    {
        //Room Initialize Stuff
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        InitilizeRoomPlayerList();
        UpdateStartUI();

        lobbyMenu.SetActive(false);
        roomMenu.SetActive(true);
    }

    private void RoomMenuUIStatus(bool enable)
    {
        roomMenu.SetActive(enable);
    }


    private void InitilizeRoomPlayerList()
    {
        activePlayers.Clear();
        Player[] currentPlayers = PhotonNetwork.PlayerList;
        for (int i = 0; i < currentPlayers.Length; i++)
        {
            activePlayers.Add(currentPlayers[i].NickName);
        }

        UpdatePlayerListText();
    }

    //WITHIN ROOM

    public void StartGame(int minPlayerToStart)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < minPlayerToStart) return;
        //Load the game scene (index = 1)
        
        PhotonNetwork.LoadLevel(1);

    }

    private void AddToPlayerList(string playerName)
    {
        activePlayers.Add(playerName);
        UpdatePlayerListText();

    }

    private void RemoveFromPlayerList(string playerName)
    {
        activePlayers.Remove(playerName);
        UpdatePlayerListText();
    }

    private void UpdatePlayerListText()
    {
        playerNameList.text =  string.Join(Environment.NewLine, activePlayers.ToArray());
    }

    private void UpdateStartUI() => gameStartButton.SetActive(PhotonNetwork.IsMasterClient);

    public void OnBackButtonClick()
    {
        PhotonNetwork.LeaveRoom();
        roomMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }

}
