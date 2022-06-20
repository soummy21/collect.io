using UnityEngine;
using Photon.Pun;
using System.IO;

public class RoomManager : MonoBehaviour
{
    [SerializeField] Transform teamASpawnPoint;
    [SerializeField] Transform teamBSpawnPoint;

    public Teams currentTeam = Teams.A;

    public Transform TeamA_SpawnPoint { get => teamASpawnPoint; }
    public Transform TeamB_SpawnPoint { get => teamBSpawnPoint; }

    private void Start()
    {
        var playerManager = PhotonNetwork.Instantiate(Path.Combine(Identifiers.PhotonPrefabPath, Identifiers.PlayerManager), Vector3.zero, Quaternion.identity).GetComponent<PlayerManager>();
        playerManager.SetRoomManager(this);
        playerManager.InstantiatePlayerManager();

    }
 

}




