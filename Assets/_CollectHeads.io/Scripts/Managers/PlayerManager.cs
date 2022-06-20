using UnityEngine;
using Photon.Pun;
using System.IO;

public enum Teams { A, B };

public class PlayerManager : MonoBehaviour
{
    private RoomManager roomManager;
    private PhotonView photonView;

    public Teams myTeam { get; private set; }

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void InstantiatePlayerManager()
    {
        if (!photonView.IsMine) return;

        //Set Players Team
        SetTeam();

        //Instantiate the player on the network at given spawnpoint
        var playerController = PhotonNetwork.Instantiate(Path.Combine(Identifiers.PhotonPrefabPath, Identifiers.PlayerController),
            AssignSpawnPoint(), Quaternion.identity);

        //Give the playerManager reference to the PlayerController
        playerController.GetComponent<PlayerController>().SetPlayerManager(this);
    }

    private void SetTeam()
    {
        myTeam = photonView.CreatorActorNr % 2 == 1 ? Teams.A : Teams.B;
    }


    private Vector3 AssignSpawnPoint()
    {
        switch (myTeam)
        {
            case Teams.A:
                return roomManager.TeamA_SpawnPoint.position + Vector3.up * Random.Range(-6, 6);
            case Teams.B:
                return roomManager.TeamB_SpawnPoint.position + Vector3.up * Random.Range(-6, 6);
            default: return Vector3.zero;
        }

    }

    public void SetRoomManager(RoomManager roomManager) => this.roomManager = roomManager;





}


