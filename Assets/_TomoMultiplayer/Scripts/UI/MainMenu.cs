using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Main Menu - Updations")]
    [SerializeField] TextMeshProUGUI gameNameText;
    [SerializeField] TextMeshProUGUI dateText;
    [SerializeField] TextMeshProUGUI playerNameText;

    [Header("Player-Name Input")]
    [SerializeField] GameObject Popup_Username;
    [SerializeField] TMP_InputField Username_InputField;

    [Header("Create_Room")]
    [SerializeField] GameObject Button_CreateRoom;
    [SerializeField] GameObject Popup_CreateRoom;

    [Header("Join_Room")]
    [SerializeField] GameObject Popup_JoinRoom;

    private TimerDown playerTimeOutTimer;
    private int playerTimeOutLeft;

    private void Start()
    {
        MainMenu_Init();
        UpdateMainMenuData();

        UpdateUsername();

        playerTimeOutTimer.TimerCompleted += KickOutTimerExpired;
        playerTimeOutTimer.TimerUpdatePerSecond += UpdateLeftOutTime;
    }

    private void OnDestroy()
    {
        playerTimeOutTimer.TimerCompleted -= KickOutTimerExpired;
        playerTimeOutTimer.TimerUpdatePerSecond -= UpdateLeftOutTime;

    }

    private void UpdateLeftOutTime(int timeLeft)
    {
        playerTimeOutLeft = timeLeft;
        PersistantUI.Instance.LogOnScreen($"Player in timeout for {playerTimeOutLeft} seconds");
    }

    private void Update()
    {
        if (SessionData.inTimeout) playerTimeOutTimer.UpdateTimer();
    }

    //Set username if it exists else popup username panel
    private void UpdateUsername()
    {

        if (!string.IsNullOrEmpty(SessionData.playerName))
        {
            PhotonNetwork.NickName = SessionData.playerName;
            if (!SessionData.connectionEstablished) ServerMesseges.EstablishConnectionToServer?.Invoke();
        }
        else 
            Popup_Username.SetActive(true);

    }

    //Initial main menu data updates { date, game name, player name }
    private void UpdateMainMenuData()
    {
        //Update Game Name
        gameNameText.text = SessionData.GameName;

        //Update Current Date
        DateTime currentDate = DateTime.Today;
        dateText.text = currentDate.ToShortDateString();

        //Update User Name        
        playerNameText.text = $"Hey {SessionData.playerName} !";

    }

    //Initializatoins for the main menu
    private void MainMenu_Init()
    {
        if (SessionData.buildType == BuildType.Session_Player) Button_CreateRoom.SetActive(false);

        playerTimeOutTimer = new TimerDown(SessionData.playerTimeout);
        playerTimeOutTimer.StartTimer();
    }

    //On click confirm username button
    public void ConfirmUsername()
    {
        if(string.IsNullOrEmpty(Username_InputField.text))
        {
            //Error Sound
        }
        else
        {
            playerNameText.text = $"Hey { Username_InputField.text } !";
            SessionData.playerName = Username_InputField.text;
            PhotonNetwork.NickName = SessionData.playerName;
            Popup_Username.SetActive(false);
            if (!SessionData.connectionEstablished) ServerMesseges.EstablishConnectionToServer?.Invoke();
        }

        
    }

    //On click open/close create room popup
    public void MainMenu_CreateRoom(bool action)
    {
        if(!SessionData.connectionEstablished || SessionData.inTimeout)
        {
            //Error sound 
            return;
        }
        Popup_CreateRoom.SetActive(action);
    }

    //On click open/close join room popup
    public void MainMenu_JoinRoom(bool action)
    {
        if (!SessionData.connectionEstablished || SessionData.inTimeout)
        {
            //Error sound 
            return;
        }

        Popup_JoinRoom.SetActive(action);
    }


    //On kick out timer expired 
    private void KickOutTimerExpired()
    {
        PersistantUI.Instance.LogOnScreen($"Player Timeout ended, can rejoin rooms now!");
        SessionData.inTimeout = false;
        playerTimeOutTimer.RestartTimer();
    }

    public void RedirectToWebsite()
    {
        Application.OpenURL("https://tomoclub.org/");
    }

    public void OpenProfile() => Popup_Username.SetActive(true);
}
