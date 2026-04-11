using UnityEngine;
using UnityEngine.UI;
public class SpoonScript : MonoBehaviour
{
    public Sprite fullSpoon, halfSpoon, emptySpoon;
    Image SpoonImage;
    private void Awake()
    {
        SpoonImage = GetComponent<Image>();
    }

    public void SetSpoonImage(SpoonStatus status)
    {
        switch (status)
        {
            case SpoonStatus.Empty:
                SpoonImage.sprite = emptySpoon;
                break;
            case SpoonStatus.Half:
                SpoonImage.sprite = halfSpoon;
                break;
            case SpoonStatus.Full:
                SpoonImage.sprite = fullSpoon;
                break;
        }
    }
}
public enum SpoonStatus
{
    Empty = 0,
    Half = 1,
    Full = 2,
}