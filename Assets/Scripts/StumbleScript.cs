using UnityEngine;
using Cinemachine;
public class StumbleScript : MonoBehaviour
{
    public bool IsStumbling;
    private CinemachineImpulseSource impulseSource;
    void Start()
    {
       
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void MakeStumble()
    {
        Debug.Log("Stumblin");
        CameraShakeManager.instance.CameraShake(impulseSource);
        IsStumbling = true;
        Debug.Log("Stumblin");
    }
}
