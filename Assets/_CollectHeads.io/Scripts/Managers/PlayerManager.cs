using UnityEngine;
using Photon.Pun;
using System.IO;

public enum Teams { A, B };

public class PlayerManager : MonoBehaviour
{
    private GameManager roomManager;
    private PhotonView photonView;

    public Teams myTeam { get; private set; }

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void InstantiatePlayerManager(int teamNo)
    {
        if (!photonView.IsMine) return;

        //Set Players Team
        myTeam = (Teams)teamNo;
        Vector3 spawnPoint = AssignSpawnPoint();

        //Instantiate the camera on the network
        var playerCamera = PhotonNetwork.Instantiate(Path.Combine(Identifiers.PhotonPrefabPath, Identifiers.PlayerCamera), 
            new Vector3(spawnPoint.x, spawnPoint.y, -10f), Quaternion.identity);

        //Instantiate the player on the network at given spawnpoint
        var playerController = PhotonNetwork.Instantiate(Path.Combine(Identifiers.PhotonPrefabPath, Identifiers.PlayerController),
            spawnPoint, Quaternion.identity);

        //Give the playerManager reference to the PlayerController
        playerController.GetComponent<PlayerController>().SetPlayerManager(this);

        //Set camera for playerController
        playerCamera.GetComponent<FollowCam>().SetPlayerToFollow(playerController.transform);
        
        


    }


    private Vector3 AssignSpawnPoint()
    {
        switch (myTeam)
        {
            case Teams.A:
                return roomManager.teamASpawnPoint[SessionData.arenaNo - 1].position + Vector3.up * Random.Range(-6, 6);
            case Teams.B:
                return roomManager.teamBSpawnPoint[SessionData.arenaNo - 1].position + Vector3.up * Random.Range(-6, 6);
            default: return Vector3.zero;
        }

    }

    public void SetRoomManager(GameManager roomManager) => this.roomManager = roomManager;





}


