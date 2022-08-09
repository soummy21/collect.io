using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModCameraManager : MonoBehaviour
{
    private Camera[] networkCameras;
    private int currentCameraNumber = 0;

    [Header("Spectator UI")]
    [SerializeField] TextMeshProUGUI spectatorText;
    [SerializeField] GameObject leftSwipeButton;
    [SerializeField] GameObject rightSwipeButton;

    private string[] playerNames;

    private void Awake()
    {
        leftSwipeButton.SetActive(PhotonNetwork.IsMasterClient);
        rightSwipeButton.SetActive(PhotonNetwork.IsMasterClient);
        spectatorText.gameObject.SetActive(PhotonNetwork.IsMasterClient);

        if (!PhotonNetwork.IsMasterClient) return;

        networkCameras = new Camera[2];
        playerNames = new string[2];
        StartCoroutine(FindNetworkCameras());
    }

    IEnumerator FindNetworkCameras()
    {
        yield return new WaitForSeconds(1f);
        networkCameras = PhotonView.FindObjectsOfType<Camera>();
        
        if(networkCameras!=null)
        {
            networkCameras[0].depth = 1;

            for (int i = 0; i < networkCameras.Length; i++)
            {
                playerNames[i] = networkCameras[i].GetComponent<PhotonView>().Owner.NickName;
            }

            spectatorText.text = "Spectating: " + playerNames[0];

        }
 
    }


    public void OnLeftSwipeButton()
    {
        if (currentCameraNumber - 1 < 0) return;
        currentCameraNumber--;
        networkCameras[currentCameraNumber].depth = 1;
        networkCameras[currentCameraNumber + 1].depth = -1;
        spectatorText.text = "Spectating: " + playerNames[currentCameraNumber];
    }

    public void OnRightSwipeButton()
    {
        if (currentCameraNumber + 1 > networkCameras.Length) return;
        currentCameraNumber++;
        networkCameras[currentCameraNumber].depth = 1;
        networkCameras[currentCameraNumber - 1].depth = -1;
        spectatorText.text = "Spectating: " + playerNames[currentCameraNumber];
    }
}
