using Photon.Pun;
using UnityEngine;

public class Artifact : MonoBehaviour
{
    public int artifactID;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Identifiers.Player))
            ArtifactsManager.instance.ArtifactCollected(artifactID);
    }

}
