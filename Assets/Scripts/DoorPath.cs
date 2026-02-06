using UnityEngine;

public class DoorPath : MonoBehaviour
{
    public Transform GetDoorPath(int doorIndex)
    {
        return transform.GetChild(doorIndex);
    }

    public int GetNextDoorIndex(int currentDoorIndex)
    {
        int nextDoorIndex = currentDoorIndex + 1;

        if (nextDoorIndex == transform.childCount)
        {
            nextDoorIndex = 0;
        }

        return nextDoorIndex;
    }
}
