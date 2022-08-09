using UnityEngine;
using TMPro;

public class PersistantUI : MonoBehaviour
{
    public static PersistantUI Instance;

    [Header("Logger")]
    [SerializeField] TextMeshProUGUI screenLogTextElement;

    [Header("Settings")]
    [SerializeField] GameObject Popup_Settings;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (!SessionData.isDevBuild) transform.GetChild(0).gameObject.SetActive(false);

    }


    public void LogOnScreen(string logText)
    {
        if (!SessionData.isDevBuild) return;
        screenLogTextElement.text = logText;

    }

    #region SETTINGS MENU

    public void ShowSettingsPopup()
    {
        Popup_Settings.SetActive(true);
    }

    public void HideSettingsPopup()
    {
        Popup_Settings.SetActive(false);
    }

    #endregion

}
