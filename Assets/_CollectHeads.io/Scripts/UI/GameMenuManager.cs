using Photon.Pun;
using UnityEngine;
using TMPro;

public class GameMenuManager : MonoBehaviour
{

    [Header("Game Menu")]
    [SerializeField] GameObject inGameMenu;
    [SerializeField] GameObject endGameMenu;
    [SerializeField] GameObject leaveButton;
    [SerializeField] TextMeshProUGUI teamAScore;
    [SerializeField] TextMeshProUGUI teamBScore;
    [SerializeField] TextMeshProUGUI winnerTeam;

    public int scoreA = 0;
    public int scoreB = 0;

    private PhotonView photonView;

    private void Awake()
    {

        endGameMenu.SetActive(false);

        teamAScore.text = "Team A\n0";
        teamBScore.text = "Team B\n0";

        photonView = GetComponent<PhotonView>();
        InvokeRoomUIEvent(false);
        UpdateLeaveUI();

    }

    private void OnEnable()
    {       
        ServerEvents.OnMasterClientChanged += UpdateLeaveUI;
        GameplayEvents.UpdateScoreboard += UpdateScore;
    }

    private void OnDisable()
    {
        ServerEvents.OnMasterClientChanged -= UpdateLeaveUI;
        GameplayEvents.UpdateScoreboard -= UpdateScore;
    }

    private void UpdateLeaveUI()
    {
        leaveButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void OnLeaveButtonClick()
    {
        photonView.RPC(nameof(InvokeRoomUIEvent), RpcTarget.AllBuffered, true);
        PhotonNetwork.LoadLevel(0);
        
    }

    [PunRPC]
    private void InvokeRoomUIEvent(bool status)
    {
        MainMenuEvents.RoomMenuStatus?.Invoke(status);
    }

    private void UpdateScore()
    {
        int team = (int)PhotonNetwork.CurrentRoom.CustomProperties[Identifiers.TeamThatScored];

        switch ((Teams)team)
        {
            case Teams.A: scoreA++;
                break;
            case Teams.B: scoreB++;
                break;
        }

        teamAScore.text = "Team A\n" + scoreA.ToString();
        teamBScore.text = "Team B\n" + scoreB.ToString();
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { Identifiers.PickupHit, false} });

        if (scoreA == 15 || scoreB == 15)
        {
            GameplayEvents.EndGameplay?.Invoke();

            inGameMenu.SetActive(false);
            winnerTeam.text = "Team "+ (Teams)team  + " Won!";
            endGameMenu.SetActive(true);
            
        }


    }


}

