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

    int[] currentTeamForArena = new int[4];

    PhotonView photonView;

    private void Awake()
    {
        if (Instance == null) Instance = this; 

        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (SessionData.buildType != BuildType.Session_Moderator) return;

        Player[] players = PhotonNetwork.PlayerListOthers;

        for (int i = 0; i < players.Length; i++)
        {
            int playerArenaNo = (int)players[i].CustomProperties[Identifiers_Mul.PlayerSettings.ArenaNo];
            //Sets the teamNo on the server
            ExitGames.Client.Photon.Hashtable thisPlayerProperties = players[i].CustomProperties;
            thisPlayerProperties.Add(Identifiers_Mul.PlayerSettings.TeamNo, currentTeamForArena[playerArenaNo - 1]);
            players[i].SetCustomProperties(thisPlayerProperties);

            photonView.RPC(nameof(SpawnPlayer), players[i], currentTeamForArena[playerArenaNo - 1]);
            currentTeamForArena[playerArenaNo - 1] = 1 - currentTeamForArena[playerArenaNo - 1];
        }

    }

    [PunRPC]
    private void SendArenaTeamsListToNetwork()
    {

    }

    [PunRPC]
    private void SpawnPlayer(int teamNo)
    {
        var playerManager = PhotonNetwork.Instantiate(Path.Combine(Identifiers.PhotonPrefabPath, Identifiers.PlayerManager), Vector3.zero, Quaternion.identity).GetComponent<PlayerManager>();
        playerManager.SetRoomManager(this);
        playerManager.InstantiatePlayerManager(teamNo);

    }
    

}




