using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class SceneFade : MonoBehaviour
{
    private Image Background;
   


    private void Awake()
    {
        Background = GetComponent<Image>();
    }
    public IEnumerator FadeInCoroutine (float duration)
    {
        Color startColour = new Color(Background.color.r, Background.color.g, Background.color.b,1);
        Color targetColour = new Color(Background.color.r,Background.color.g,Background.color.b,0);

        yield return FadeCoroutine (startColour, targetColour, duration);
        gameObject.SetActive (false);
    }
    public IEnumerator FadeOutCoroutine (float duration)
    {
        Color startColour = new Color(Background.color.r, Background.color.g, Background.color.b, 0);
        Color targetColour = new Color(Background.color.r, Background.color.g, Background.color.b, 1);
        gameObject.SetActive(true);
        yield return FadeCoroutine(startColour, targetColour, duration);
        gameObject.SetActive(false);
    }
    private IEnumerator FadeCoroutine(Color startColour, Color targetColour, float duration)
    {
        float elapsedTime = 0;
        float elapsedPercentage = 0;

        while (elapsedPercentage <1)
        {
            elapsedPercentage = elapsedTime/duration;
            Background.color = Color.Lerp(startColour, targetColour, elapsedPercentage);

            yield return null;
            elapsedTime += Time.deltaTime;
        }
    }
}
