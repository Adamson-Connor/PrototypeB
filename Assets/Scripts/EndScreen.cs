using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField]
    private FadeController fadeController;

    public void BackToMenu()
    {
        fadeController.LoadScene("MainMenu");
    }
    public void EndGame()
    {
        fadeController.LoadScene("EndScreen");
    }
}
