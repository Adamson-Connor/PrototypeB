using System.Collections;
using UnityEngine;
using UnityEngine.Animations;

public class WeirdStareController : MonoBehaviour
{
    private bool Attention = false;
    [SerializeField] Transform Player;
    [SerializeField] float Speed;
    private Coroutine LookCoroutine;
    public void GainAttention()
    {
        Attention = true;
        if (LookCoroutine != null)
        {
            StopCoroutine(LookCoroutine);
        }

        LookCoroutine = StartCoroutine(LookAt());
    }

    private IEnumerator LookAt()
    {
        Quaternion lookRotate = Quaternion.LookRotation(Player.position - transform.position);

        float time = 0;

            while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotate, time);

            time += Time.deltaTime * Speed;

            yield return null;
        }

    }
    // Update is called once per frame
    void Update()
    {
        if (Attention == true)
        {
            transform.LookAt(Player);
        }

        else return;
    }
}
