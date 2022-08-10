using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System.Collections.Generic;
using Random = System.Random;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Transform []teamASpawnPoint;
    public Transform []teamBSpawnPoint;

    private int[] currentTeamForArena = new int[4];

    //private List<Player> copyCachedPlayerList = new List<Player>();

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

        //Shuffle List using Linq
        //for (int i = 0; i < SessionData.cachedRoomPlayers.Count; i++)
        //{
        //    copyCachedPlayerList.Add(SessionData.cachedRoomPlayers[i]);
        //}

        //var rnd = new Random();
        //var randomized = copyCachedPlayerList.OrderBy(item => rnd.Next());
        //List<Player> listRandomized = randomized.ToList();

        for (int i = 0; i < SessionData.cachedRoomPlayers.Count; i++)
        {
            int playerArenaNo = SessionData.cachedPlayersArenaNo[i];

            //Sets the teamNo on the server
            playerProperties[Identifiers_Mul.PlayerSettings.ArenaNo] = playerArenaNo;
            playerProperties[Identifiers_Mul.PlayerSettings.TeamNo] = currentTeamForArena[playerArenaNo - 1];
            SessionData.cachedRoomPlayers[i].SetCustomProperties(playerProperties);
            //Debug.Log($"Player:{listRandomized[i].NickName}, Arena: {playerArenaNo}, Team:{currentTeamForArena[playerArenaNo - 1]}");
            photonView.RPC(nameof(SpawnPlayer), SessionData.cachedRoomPlayers[i], currentTeamForArena[playerArenaNo - 1]);
            currentTeamForArena[playerArenaNo - 1] = 1 - currentTeamForArena[playerArenaNo - 1];
        }

    }

    [PunRPC]
    private void SpawnPlayer(int teamNo)
    {
        var playerManager = PhotonNetwork.Instantiate(Path.Combine(Identifiers.PhotonPrefabPath, Identifiers.PlayerManager), Vector3.zero, Quaternion.identity).GetComponent<PlayerManager>();
        playerManager.SetRoomManager(this);
        playerManager.InstantiatePlayerManager(teamNo);

    }
    

}




