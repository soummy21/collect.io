using Photon.Pun;
using UnityEngine;
using TMPro;

public class GameMenuManager : MonoBehaviour
{
    public static GameMenuManager Instance;


    [Header("Per Arena Scoring UI")]
    [SerializeField] TextMeshProUGUI[] teamAScores;
    [SerializeField] TextMeshProUGUI[] teamBScores;

    private int[] perArena_TeamA_Scores = new int[4];
    private int[] perArena_TeamB_Scores = new int[4];

    private PhotonView photonView;

    private void Awake()
    {
        if (Instance == null) Instance = this;


        photonView = GetComponent<PhotonView>();

        foreach (var teamAScore in teamAScores)
        {
            teamAScore.text = "Team A\n0";
        }

        foreach (var teamBScore in teamBScores)
        {
            teamBScore.text = "Team B\n0";
        }
    }


    private void OnEnable()
    {       
        GameplayEvents.UpdateScoreboard += UpdateScoreOnNetwork;
    }

    private void OnDisable()
    {
        GameplayEvents.UpdateScoreboard -= UpdateScoreOnNetwork;
    }


    private void UpdateScoreOnNetwork(Teams team)
    {
        int teamNo = (int)team;

        object[] data = new object[] { teamNo, SessionData.localArenaNo };
        photonView.RPC(nameof(UpdateScore), RpcTarget.All, data);
    }


    [PunRPC]
    private void UpdateScore(object [] data)
    {
        int team = (int)data[0];
        int arenaNo = (int)data[1];

        switch ((Teams)team)
        {
            case Teams.A: perArena_TeamA_Scores[arenaNo - 1]++;
                break;
            case Teams.B: perArena_TeamB_Scores[arenaNo - 1]++;
                break;
        }

        teamAScores[arenaNo - 1].text = "Team A\n" + perArena_TeamA_Scores[arenaNo -1].ToString();
        teamBScores[arenaNo - 1].text = "Team B\n" + perArena_TeamB_Scores[arenaNo - 1].ToString();

        if (perArena_TeamA_Scores[arenaNo - 1] == 1 || perArena_TeamB_Scores[arenaNo - 1] == 1)
        {
            UserMessages.UpdateLeaderBoard?.Invoke(arenaNo, team);

            if (SessionData.localArenaNo == arenaNo)
            {
                GameMessages.OnGameSessionEnded?.Invoke();
                ArenaManager.Instance.PlayerBecomesSpectator();
            }

            if (SessionData.buildType == BuildType.Session_Moderator) GameMessages.OnCompleteArena?.Invoke(arenaNo);

        }


    }

    public int GetTeamScore(int arenaNo, int team)
    {
        Teams teamName = (Teams)team;

        switch (teamName)
        {
            case Teams.A: return perArena_TeamA_Scores[arenaNo - 1];
            case Teams.B: return perArena_TeamB_Scores[arenaNo -1 ];
            default: return -1;
        }

    }

}

