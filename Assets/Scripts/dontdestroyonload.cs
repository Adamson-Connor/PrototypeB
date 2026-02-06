using UnityEngine;

public class dontdestroyonload : MonoBehaviour
{
    [SerializeField] GameObject target;
    public static void DontDestroyOnLoad(Object target)
    {
        DontDestroyOnLoad(target);
    }
}
