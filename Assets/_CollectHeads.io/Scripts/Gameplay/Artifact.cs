using Photon.Pun;
using UnityEngine;

public class Artifact : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Identifiers.Player))
            gameObject.SetActive(false);
    }

}
