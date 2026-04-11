using UnityEngine;
using UnityEngine.UI;
public class PainScript : MonoBehaviour
{
    public Sprite NoHurt, YellowHurt, OrangeHurt, RedHurt;
    Image PainImage;
    private void Awake()
    {
        PainImage = GetComponent<Image>();
    }

    public void SetPainImage(PainStatus status)
    {
        switch (status)
        {
            case PainStatus.NoHurt:
                PainImage.sprite = NoHurt;
                break;
            case PainStatus.Yellow:
                PainImage.sprite = YellowHurt;
                break;
            case PainStatus.Orange:
                PainImage.sprite = OrangeHurt;
                break;
                case PainStatus.Red:
                PainImage.sprite= RedHurt;
                break;
        }
    }
}
public enum PainStatus
{
    NoHurt = 0,
    Yellow = 1,
    Orange = 2,
    Red = 3,
}