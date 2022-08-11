using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System.Collections.Generic;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Transform []teamASpawnPoint;
    public Transform []teamBSpawnPoint;

    private int[] currentTeamForArena = new int[4];

    PhotonView photonView;

    private void Awake()
    {
        if (Instance == null) Instance = this; 

        photonView = GetComponent<PhotonView>();
    }


    private void Start()
    {
        if (SessionData.buildType != BuildType.Session_Moderator) return;

        ExitGames.Client.Photon.Hashtable playerProperties = SessionData.cachedRoomPlayers[0].CustomProperties;
        if(playerProperties[Identifiers_Mul.PlayerSettings.TeamNo] == null) playerProperties.Add(Identifiers_Mul.PlayerSettings.TeamNo, -1);

        List<Player> scrambledPlayerList = SessionData.GetScrambledList(SessionData.cachedRoomPlayers);

        for (int i = 0; i < scrambledPlayerList.Count; i++)
        {
            int playerArenaNo = SessionData.cachedPlayersArenaNo[SessionData.cachedRoomPlayers.IndexOf(scrambledPlayerList[i])];
            //Sets the teamNo on the server
            playerProperties[Identifiers_Mul.PlayerSettings.ArenaNo] = playerArenaNo;
            playerProperties[Identifiers_Mul.PlayerSettings.TeamNo] = currentTeamForArena[playerArenaNo - 1];
            scrambledPlayerList[i].SetCustomProperties(playerProperties);
            Debug.Log($"Player:{scrambledPlayerList[i].NickName}, Arena: {playerArenaNo}, Team:{currentTeamForArena[playerArenaNo - 1]}");
            photonView.RPC(nameof(SpawnPlayer), scrambledPlayerList[i], currentTeamForArena[playerArenaNo - 1]);
            currentTeamForArena[playerArenaNo - 1] = 1 - currentTeamForArena[playerArenaNo - 1];
        }

        PersistantUI.Instance.LogOnScreen("Game Loaded!");

    }

    [PunRPC]
    private void SpawnPlayer(int teamNo)
    {
        var playerManager = PhotonNetwork.Instantiate(Path.Combine(Identifiers.PhotonPrefabPath, Identifiers.PlayerManager), Vector3.zero, Quaternion.identity).GetComponent<PlayerManager>();
        playerManager.SetRoomManager(this);
        playerManager.InstantiatePlayerManager(teamNo);

    }
    

}




