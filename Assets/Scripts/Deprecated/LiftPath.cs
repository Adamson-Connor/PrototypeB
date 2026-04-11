using UnityEngine;

public class LiftPath : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform GetLiftPath(int pathIndex)
    {

        return transform.GetChild(pathIndex);
    }

    public int GetNextPathIndex(int currentPathIndex)
    {
        int nextPathIndex = currentPathIndex + 1;

        if (nextPathIndex == transform.childCount)
        {
            nextPathIndex = 0;
        }

        return nextPathIndex;
    }
}
