using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ServerHandler : MonoBehaviourPunCallbacks
{


    void Start()
    {

        MainMenuEvents.OnPlayerNameInput += GivePlayerName;
        MainMenuEvents.OnCreateRoomInput += CreateRoomOnPhoton;
        MainMenuEvents.OnJoinRoomInput += ConnectToPhotonRoom;

        //Connect to photon server once at the beginning
        ConnectToServer();
    }

    private void OnDestroy()
    {
        MainMenuEvents.OnPlayerNameInput -= GivePlayerName;
        MainMenuEvents.OnCreateRoomInput -= CreateRoomOnPhoton;
        MainMenuEvents.OnJoinRoomInput -= ConnectToPhotonRoom;
    }


    //CONNECTING TO SERVER

    //If Failed then use this button
    public void ConnectToServer()
    {
        Debug.Log("Connecting to server");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Server! Welcome");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        ServerEvents.OnServerConnected?.Invoke();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        ServerEvents.OnFailedToJoinServer?.Invoke();
        Debug.Log("Failed to join server due to " + cause);
    }


    //JOINING ROOMS

    public override void OnCreatedRoom()
    {


        Debug.Log("Created Room! ");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
    }

    public override void OnJoinedRoom()
    {
        OnJoiningRoom();
        Debug.Log("Joined Room! ");
    }
 

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
    }

    private void OnJoiningRoom()
    {

        ServerEvents.OnJoinedRoom?.Invoke();
        
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        ServerEvents.OnMasterClientChanged?.Invoke();
    }

    #region ASSIGNER METHODS
    private void GivePlayerName(string name) => PhotonNetwork.NickName = name;

    private void CreateRoomOnPhoton(string roomName)
    {
        Hashtable roomData = new Hashtable();
        roomData.Add(Identifiers.TeamThatScored, 0);
        roomData.Add(Identifiers.PickupHit, false);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 6;
        roomOptions.CustomRoomProperties = roomData;
        PhotonNetwork.CreateRoom(roomName,roomOptions);
    }

    private void ConnectToPhotonRoom(string roomName) => PhotonNetwork.JoinRoom(roomName);

    #endregion

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        bool pickupHit = (bool)PhotonNetwork.CurrentRoom.CustomProperties[Identifiers.PickupHit];
        if(pickupHit) GameplayEvents.UpdateScoreboard?.Invoke();

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        ServerEvents.PlayerJoinedRoom?.Invoke(newPlayer.NickName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ServerEvents.PlayerLeftRoom?.Invoke(otherPlayer.NickName);
    }

    public override void OnLeftRoom()
    {
        ServerEvents.PlayerLeftRoom?.Invoke(PhotonNetwork.NickName);
    }


}
