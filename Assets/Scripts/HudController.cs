using UnityEngine;
using TMPro;
public class HudController : MonoBehaviour
{
    public static HudController instance;

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] TMP_Text interactiontext;

    public void EnableInteractionText (string text)
    {
        interactiontext.text = text + " (Interact) ";
        interactiontext.gameObject.SetActive(true);
    }
    public void DisableInteractionText ()
    {
        interactiontext.gameObject.SetActive(false);
    }
}
