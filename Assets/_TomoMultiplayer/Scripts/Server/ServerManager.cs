using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

namespace TomoClub.Multiplayer
{
    //Handles connection to the server and all callbacks from the server
    public class ServerManager : MonoBehaviourPunCallbacks
    {
        [Header("Game Settings")]
        [SerializeField] private string gameName;
        [Tooltip("Their are three types of builds, Session_Moderator: Gives access to moderator tools; " +
            "Session_Player: For players joining a moderated session; Global_Build: Anyone can create/join games")]
        [SerializeField] private BuildType buildType;
        [Tooltip("Turn off for production build")]
        [SerializeField] private bool isDevBuild = false;

        [Header("Player Settings")]
        [Tooltip("Amount of time in seconds till the player can join back a room after being kicked out")]
        [SerializeField] private int playerTimeout = 300; //5 mins

        private string customLobbyName = "TomoClub_";


        private void Awake()
        {
            //Application Setup
            Application.targetFrameRate = 30; //Constant frame rate for the game 
            QualitySettings.vSyncCount = 0; //Remove v-sync
            Application.runInBackground = true; //Keep the application running in background

            //Photon Setup
            PhotonNetwork.AutomaticallySyncScene = true; //Sync incoming players to the master client's scene
            PhotonNetwork.KeepAliveInBackground = 120000; //120000ms = 2 mins

            //Set Session Data (Persistant)
            SessionData.GameName = gameName;
            SessionData.isDevBuild = isDevBuild;
            SessionData.buildType = buildType;
            SessionData.playerTimeout = playerTimeout;
            
            //Gets whether player is timedOut or not
            //SessionData.InTimeout_Init();
            customLobbyName += gameName;

            ServerMesseges.EstablishConnectionToServer += ConnectToPhotonServer;
        }

        private void OnDestroy()
        {
            ServerMesseges.EstablishConnectionToServer -= ConnectToPhotonServer;
        }

        //Trying to establish connection with the server!
        private void ConnectToPhotonServer()
        {
            //Connect to PhotonServer
            PhotonNetwork.ConnectUsingSettings();
            PersistantUI.Instance.LogOnScreen("Connecting To Game!...");

        }

        //On Failed to Established Connection or disconnected from the server
        public override void OnDisconnected(DisconnectCause cause)
        {
            PersistantUI.Instance.LogOnScreen($"Disconnected from server; Cause: {cause}");
            SessionData.connectionEstablished = false;
            //Send data to UI based on whether in-game disconnection or just normal disconnection
        }

        //On Server Connection Established
        public override void OnConnectedToMaster()
        {
            PersistantUI.Instance.LogOnScreen("Connected To Game!");
            if(!PhotonNetwork.InLobby) Invoke(nameof(JoinCustomLobby), 0.3f);
        }

        //Join a custom defined lobby instead of default lobby
        private void JoinCustomLobby()
        {
            TypedLobby customTypedLobby = new TypedLobby(customLobbyName, LobbyType.Default);
            SessionData.gameLobby = customTypedLobby;
            PhotonNetwork.JoinLobby(customTypedLobby);
        }

        //On Joining Custom Lobby
        public override void OnJoinedLobby()
        {
            PersistantUI.Instance.LogOnScreen($"Joined Lobby: { PhotonNetwork.CurrentLobby.Name }");
            ServerMesseges.OnConnectedToPhoton?.Invoke();
            SessionData.connectionEstablished = true;
        }

        //On Created Room Successful
        public override void OnCreatedRoom()
        {
            PersistantUI.Instance.LogOnScreen("Created Room: " + PhotonNetwork.CurrentRoom);
            //PhotonNetwork.LoadLevel(Identifiers.LobbyScene);
        }

        //On Create Room Failed
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            string failMessage = "Create Room Failed. " + message;
            PersistantUI.Instance.LogOnScreen(failMessage);
            ServerMesseges.OnCreateRoomFailed?.Invoke(failMessage);
        }

        //On Joined Room Successful 
        public override void OnJoinedRoom()
        {
            PersistantUI.Instance.LogOnScreen("Joined Room: " + PhotonNetwork.CurrentRoom.Name);

            SessionData.previousGameState = SessionData.currentGameState;
            SessionData.currentGameState = GameStates.RoomLobby;
            ServerMesseges.OnJoinRoomSuccessful?.Invoke();
        }

        public override void OnLeftRoom()
        {
            SessionData.previousGameState = SessionData.currentGameState;
            SessionData.currentGameState = GameStates.MainMenu;
            ServerMesseges.OnLeaveRoom?.Invoke();
        }

        //On Join Room Failed
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            string failMessage = "Join Room Failed. " + message;
            PersistantUI.Instance.LogOnScreen(failMessage);
            ServerMesseges.OnJoinRoomFailed?.Invoke(failMessage);
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            ServerMesseges.OnRoomListUpdated?.Invoke(roomList);
            //PersistantUI.Instance.LogOnScreen("Available Room List Refreshed");
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (SessionData.buildType == BuildType.Session_Moderator)
            {
                //Keep track of room players anywhere                      
                int playerArenaNo = newPlayer.CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo] == null ? -1 :
                    (int)newPlayer.CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo];
                SessionData.cachedRoomPlayers.Add(newPlayer);
                SessionData.cachedPlayersArenaNo.Add(playerArenaNo);
            }

            ServerMesseges.OnPlayerJoinedRoom?.Invoke(newPlayer);//Player Entered
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {

            if (SessionData.buildType == BuildType.Session_Moderator)
            {
                //Keep track of room players anywhere
                int index = SessionData.cachedRoomPlayers.IndexOf(otherPlayer);
                SessionData.cachedRoomPlayers.Remove(otherPlayer);
                SessionData.cachedPlayersArenaNo.RemoveAt(index);
            }

            ServerMesseges.OnPlayerLeftRoom?.Invoke(otherPlayer);//Player Left


        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            ServerMesseges.OnPlayerPropertiesUpdate?.Invoke(targetPlayer);
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if(newMasterClient == PhotonNetwork.LocalPlayer)
            {
                UtilMessages.SetAndStartTimer?.Invoke();

            }

        }

    }
}

