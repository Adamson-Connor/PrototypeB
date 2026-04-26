using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeController : MonoBehaviour
{
    [SerializeField]
    private float FadeDuration = 1;
    [SerializeField] private SceneFade sceneFade;

    private void Awake()
    {
       // sceneFade = GetComponentInChildren<SceneFade>();
    }
    IEnumerator Start()
    {

        yield return sceneFade.FadeInCoroutine(FadeDuration);
    }
    public void LoadScene(string  sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        yield return sceneFade.FadeOutCoroutine(FadeDuration);
        yield return SceneManager.LoadSceneAsync(sceneName);
    }
    
}
