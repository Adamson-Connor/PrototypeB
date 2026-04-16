using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private FadeController fadeController;

    public void PlayGame()
    {
        fadeController.LoadScene("Level1 (v2)");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void BackToMenu()
    {
        fadeController.LoadScene("MainMenu");
    }
}
