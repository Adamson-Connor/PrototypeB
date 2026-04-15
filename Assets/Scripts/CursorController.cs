using StarterAssets;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    private void OnLevelWasLoaded(int level)
    {
            if (Object.FindAnyObjectByType<FirstPersonController>() != null)
        {
            Cursor.visible = false;

        }
        else
        {
            Cursor.visible = true;
        }
    }

    public void MakeCursorVisible()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}

