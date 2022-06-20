using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    private static DontDestroyOnLoad instance;

    //Lifecycle functions
    private void Awake() => MakePhotonHandlerScenePersistant();

    // SINGLETON
    private void MakePhotonHandlerScenePersistant()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

    }


}
