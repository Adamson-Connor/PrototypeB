using UnityEngine;

public class movecontroller : MonoBehaviour
{
    [SerializeField] private GameObject Destination;
    [SerializeField] private float ObjSpeed = 1.0f;
    private bool PlayerReady = false;



    public void CarGo()
    {
        PlayerReady = true;
    }

    private void Update()
    {
        if (PlayerReady ==true)
        {
            if (Destination == null) return;

            if (Vector3.Distance(Destination.transform.position, transform.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, Destination.transform.position, Time.deltaTime * ObjSpeed);
            }
        }
    }


}
