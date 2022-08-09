using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseScreen : MonoBehaviour
{
    //Enable Screen UI
    public void ShowScreen() => gameObject.SetActive(true);

    //Disable Screen UI
    public void HideScreen() => gameObject.SetActive(false);

    //Override to animate screen in 
    public virtual IEnumerator AnimateScreenIn()
    {
        ShowScreen();
        yield return null;
    }

    //Override to animate screen out
    public virtual IEnumerator AnimateScreenOut()
    {
        HideScreen();
        yield return null;
        
    }
}
