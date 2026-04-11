using StarterAssets;
using UnityEngine;

public class TeleportManager : MonoBehaviour
{
    [SerializeField] Transform destination;
    public FirstPersonController controller;
    public void DestinationTP()
    {
       controller.Teleport(destination.position, destination.rotation);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(destination.position, .4f);
        var direction = destination.TransformDirection(Vector3.forward);
        Gizmos.DrawRay(destination.position, direction);
    }
  
}
