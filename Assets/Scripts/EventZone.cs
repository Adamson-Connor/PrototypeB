using UnityEngine;

public class EventZone : MonoBehaviour
{
    public UnityEngine.Events.UnityEvent onTriggerEnterEvent;
    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnterEvent.Invoke();
    }
}
