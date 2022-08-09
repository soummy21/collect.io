using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace TomoClub.Multiplayer
{
    //Handles Room Creation and Joining
    public class RoomManager : MonoBehaviour
    {
        [Header("Create Room - Data")]
        [SerializeField] Vector2Int totalPlayersRange;
        [SerializeField] Vector2Int totalArenaRange;
        [SerializeField] Vector2Int perArenaPlayersRange;
        [SerializeField] int defaultTotalPlayers = 4;
        [SerializeField] int defaultArenas = 2;


        [Header("Create Room - UI Updations")]
        [SerializeField] TextMeshProUGUI totalPlayersText;
        [SerializeField] TextMeshProUGUI arenaPlayersText;
        [SerializeField] TextMeshProUGUI createErrorText;

        [Header("Join Room - UI Updations")]
        [SerializeField] GameObject noRoomsAvailable;
        [SerializeField] GameObject[] joinRoomObjects;
        [SerializeField] TextMeshProUGUI joinErrorText;

        private TextMeshProUGUI[] joinRoomObjectTexts;
        private string[] currentRoomNames;

        private bool canCreateRoom = true;
        private bool canJoinRoom = true;


        private void Awake()
        {
            UpdateCreateRoomData();
            JoinRoom_Init();
        }

        private void OnEnable()
        {
            ServerMesseges.OnJoinRoomSuccessful += GoToRoomLobbyMenu;
            ServerMesseges.OnCreateRoomFailed += UpdateCreateRoomUI;
            ServerMesseges.OnJoinRoomFailed += UpdateJoinRoomUI;
            ServerMesseges.OnRoomListUpdated += UpdateRoomListUI;
        }

        private void OnDisable()
        {
            ServerMesseges.OnJoinRoomSuccessful -= GoToRoomLobbyMenu;
            ServerMesseges.OnCreateRoomFailed -= UpdateCreateRoomUI;
            ServerMesseges.OnJoinRoomFailed -= UpdateJoinRoomUI;
            ServerMesseges.OnRoomListUpdated -= UpdateRoomListUI;
        }

        //Initial create room setup
        private void UpdateCreateRoomData()
        {
            totalPlayersText.text = defaultTotalPlayers.ToString();
            arenaPlayersText.text = defaultArenas.ToString();

            createErrorText.text = "";

        }

        private void JoinRoom_Init()
        {
            joinRoomObjectTexts = new TextMeshProUGUI[joinRoomObjects.Length];
            currentRoomNames = new string[joinRoomObjects.Length];
        }

        //Initial join room setup
        private void UpdateJoinRoomData()
        {
            noRoomsAvailable.SetActive(true);

            for (int i = 0; i < joinRoomObjects.Length; i++)
            {
                joinRoomObjects[i].SetActive(false);
                joinRoomObjectTexts[i] = joinRoomObjects[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            }

            joinErrorText.text = "";
        }

        //Update message to show on failing to create room
        private void UpdateCreateRoomUI(string message)
        {
            createErrorText.text = message;
            canCreateRoom = true;
        }

        //Update message to show on failing to join room
        private void UpdateJoinRoomUI(string message)
        {
            joinErrorText.text = message;
            canJoinRoom = true;
        }

        #region CREATE ROOM PANEL BUTTON FUNCTIONS

        public void DecrementTotalPlayers()
        {
            createErrorText.text = "";

            if (defaultTotalPlayers - 1 < totalPlayersRange.x)
            {
                createErrorText.text = $"Can't have less than {totalPlayersRange.x} players in a room";
                //Error Sound
                return;
            }

            if(defaultTotalPlayers - 1 < defaultArenas * perArenaPlayersRange.x)
            {
                createErrorText.text = $"Can't have less than {perArenaPlayersRange.x} in an arena!";
                //Error Sound
                return;
            }

            defaultTotalPlayers--;
            totalPlayersText.text = defaultTotalPlayers.ToString();
        }

        public void IncrementTotalPlayers()
        {
            createErrorText.text = "";

            if (defaultTotalPlayers + 1 > totalPlayersRange.y )
            {
                createErrorText.text = $"Can't have more than {totalPlayersRange.y} players in a room";
                //Error Sound
                return;
            }

            if(defaultTotalPlayers + 1 > defaultArenas * perArenaPlayersRange.y)
            {
                createErrorText.text = $"Can't have more than {perArenaPlayersRange.y} players per room";
                //Error Sound
                return;
            }


            defaultTotalPlayers++;
            totalPlayersText.text = defaultTotalPlayers.ToString();
        }

        public void DecrementArenaPlayers()
        {
            createErrorText.text = "";

            if (defaultArenas - 1 < totalArenaRange.x)
            {
                createErrorText.text = $"Can't have less than {totalArenaRange.x} arena";
                //Error Sound
                return;
            }

            if(defaultArenas - 1 < Mathf.CeilToInt((float)defaultTotalPlayers / perArenaPlayersRange.y))
            {
                createErrorText.text = $"Max players per arena is {perArenaPlayersRange.y}";
            }

            defaultArenas--;
            arenaPlayersText.text = defaultArenas.ToString();
        }

        public void IncrementArenaPlayers()
        {
            createErrorText.text = "";

            if (defaultArenas + 1 > totalArenaRange.y)
            {
                createErrorText.text = $"Can't have more than {totalArenaRange.y} arenas";
                //Error Sound
                return;
            }

            if(defaultArenas + 1 * perArenaPlayersRange.x > defaultTotalPlayers)
            {
                createErrorText.text = $"Minimum {perArenaPlayersRange.x} players are required per room";
                //Error Sound
                return;
            }

            defaultArenas++;
            arenaPlayersText.text = defaultArenas.ToString();
        }

        public void CreateRoom()
        {
            if(!canCreateRoom)
            {
                //Error Sound 
                return;
            }

            //Reset func on valid click
            canCreateRoom = false;
            createErrorText.text = "";

            //Room options {Note: these values are used to keep player disconnection to the absolute minimum}
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = (byte)( defaultTotalPlayers+1 );
            roomOptions.CleanupCacheOnLeave = false;
            roomOptions.PlayerTtl = -1;
            roomOptions.EmptyRoomTtl = 60000;

            //Add the essential room properties {no. of arenas }
            Hashtable roomProperties = new Hashtable();
            roomProperties.Add(Identifiers_Mul.PlayerSettings.NoOfArenas, defaultArenas);
            roomOptions.CustomRoomProperties = roomProperties;

            //Create a room of random name
            string roomName = GenerateRandomRoomName();
            PersistantUI.Instance.LogOnScreen("Creating Room: " + roomName);
            PhotonNetwork.CreateRoom(roomName, roomOptions, SessionData.gameLobby);
        }

        private string GenerateRandomRoomName()
        {
            string randomRoomName =  $"Room #{Random.Range(1, 1000)}";
            if (SessionData.buildType == BuildType.Session_Moderator) randomRoomName = "Mod_" + randomRoomName;
            return randomRoomName;
        }

        #endregion

        //Update Room UI when a room is created or destroy
        private void UpdateRoomListUI(List<RoomInfo> roomInfoList)
        {
            UpdateJoinRoomData();

            //If no rooms in cache then return
            if (roomInfoList.Count == 0) return;

            //If any room has availability then show that room else return 
            bool availableRooms = false;
            for (int i = 0; i < roomInfoList.Count; i++)
            {
                if (roomInfoList[i].PlayerCount > 0)
                {
                    availableRooms = true;
                    break;
                }

            }

            if (!availableRooms) return;

            noRoomsAvailable.SetActive(false);
            for (int i = 0; i < roomInfoList.Count; i++)
            {
                if (roomInfoList[i].PlayerCount == 0) continue;

                currentRoomNames[i] = roomInfoList[i].Name;
                joinRoomObjects[i].SetActive(true);
                joinRoomObjectTexts[i].text = roomInfoList[i].Name + $"\n<#E56F47><size= 38> Players: " +
                    $"({roomInfoList[i].PlayerCount}/{roomInfoList[i].MaxPlayers})";
                                       
            }
        }

        //Join a room
        public void JoinRoom(int roomNo)
        {
            if(!canJoinRoom)
            {
                //Error Sound
                return;
            }


            canJoinRoom = false;
            PhotonNetwork.JoinRoom(currentRoomNames[roomNo]);
        }

        public void GoToRoomLobbyMenu()
        {
            if(SessionData.previousGameState == GameStates.MainMenu && SessionData.buildType == BuildType.Session_Moderator)
            {
                PhotonNetwork.LoadLevel(Identifiers_Mul.LobbyScene);
            }

        }
    }
}

