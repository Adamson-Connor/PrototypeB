using System.Collections;
using UnityEngine;

public class LiftMovement : MonoBehaviour
{
    [SerializeField] private LiftPath liftPath;
    [SerializeField] private float liftSpeed;
    private int targetLiftPathIndex;

    private Transform targetLiftPath;
    private Transform previousLiftPath;

    private float timetoLiftPath;
    private float elapsedTime;

    private bool alreadyMoving = false;
    [SerializeField] bool startMoving = false;

    private void Start()
    {
        //TargetNextLiftPath();
        if (startMoving)
        {
            StartMoving();
        }
    }
    public void StartMoving()
    {
        if(alreadyMoving)
        {
            return;
        }
        alreadyMoving = true;
        StartCoroutine(MovePlatform());
    }
    IEnumerator MovePlatform()
    {
        while (true)
        {
            TargetNextLiftPath();

        }
        }
    private void FixedUpdate()
    {
        elapsedTime += Time.deltaTime;

        float elapsedTimePercentage = elapsedTime / timetoLiftPath;
        elapsedTimePercentage = Mathf.SmoothStep (0,1,elapsedTimePercentage);
        transform.position = Vector3.Lerp(previousLiftPath.position,targetLiftPath.position,elapsedTimePercentage);

        if (elapsedTimePercentage >=1)
        {
            TargetNextLiftPath();

        }
    }
    private void TargetNextLiftPath()
    {
        previousLiftPath = liftPath.GetLiftPath(targetLiftPathIndex);
        targetLiftPathIndex = liftPath.GetNextPathIndex(targetLiftPathIndex);
        targetLiftPath = liftPath.GetLiftPath(targetLiftPathIndex);

        elapsedTime = 0;

        float distanceToLiftPath = Vector3.Distance (previousLiftPath.position, targetLiftPath.position);
        timetoLiftPath = distanceToLiftPath/liftSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        other.transform.SetParent(transform);

    }
    private void OnTriggerExit(Collider other)
    {
        other.transform.SetParent(null);
    }
}
