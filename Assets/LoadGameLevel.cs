using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadGameLevel : MonoBehaviour
{
    private void Awake()
    {
        if (SessionData.buildType == BuildType.Session_Moderator) Photon.Pun.PhotonNetwork.LoadLevel(Identifiers_Mul.GameScene);
    }
}
