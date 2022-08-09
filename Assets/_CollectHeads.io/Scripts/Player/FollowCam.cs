using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class FollowCam : MonoBehaviour
{

    private Transform playerToFollow;
    private bool canFollow = false;

    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();

        //Makes sure local camera renders for local player
        if (photonView.IsMine) GetComponent<Camera>().depth = 10;
    }

    void LateUpdate()
    {
        if (!canFollow) return;

        Vector2 newCamPos = new Vector2(playerToFollow.position.x, playerToFollow.position.y);
        transform.position = new Vector3(newCamPos.x, newCamPos.y, transform.position.z);
    }

    public void SetPlayerToFollow(Transform player)
    {
        playerToFollow = player;
        canFollow = true;
    }
    
}
