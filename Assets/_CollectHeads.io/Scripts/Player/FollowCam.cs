using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public static FollowCam instance;

    private Transform playerToFollow;
    private bool canFollow = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
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
