using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject pointerText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerText.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerText.SetActive(false);
    }

}
