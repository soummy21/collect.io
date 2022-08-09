using UnityEngine;
using Photon.Pun;


public class ArtifactsManager : MonoBehaviour
{
    public static ArtifactsManager instance;

    private Artifact [] artifacts;
    private PhotonView photonView;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        photonView = GetComponent<PhotonView>();

        //Initializing artifact array based on child count
        artifacts = new Artifact[transform.childCount];
        

        //Populating the artifacts
        for (int i = 0; i < transform.childCount; i++)
        {
            artifacts[i] = transform.GetChild(i).GetComponent<Artifact>();
            artifacts[i].artifactID = i;
        }
    }


    public void ArtifactCollected(int artifactNo)
    {
        photonView.RPC(nameof(UpdateCollectedArtifactOnNetwork), RpcTarget.All, artifactNo);
    }

    [PunRPC]
    private void UpdateCollectedArtifactOnNetwork(int artifactNo)
    {
        artifacts[artifactNo].gameObject.SetActive(false);
    }



}
