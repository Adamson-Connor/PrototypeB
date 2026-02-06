using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    Outline outline;
    public string message;
    [SerializeField]  private  DoorScript doorScript;
    public UnityEvent onInteraction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        outline = GetComponent<Outline>();
        DisableOutline();
    }

    public void Interact()
    {
        onInteraction.Invoke();
    }
    public void DisableOutline()
    { 
    outline.enabled = false;
    }
    public void EnableOutline()
    {
        outline.enabled = true;
    }
}
