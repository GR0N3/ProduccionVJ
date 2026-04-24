using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthHeart : MonoBehaviour
{
    // Start is called before the first frame update
    public Sprite fullheart, halfheart, emptyheart;
    Image heartImage;
    private void Awake()
    {
        heartImage = GetComponent<Image>();
    }

    public void SetHeartImage(HeartStatus Status)
    {
        switch (Status)
        {
            case HeartStatus.Empty:
                heartImage.sprite = emptyheart;
                break;
            case HeartStatus.Half:
                heartImage.sprite = halfheart;
                break;
            case HeartStatus.Full:
                heartImage.sprite = fullheart;
                break;
        }
    }
}
public enum HeartStatus
{
    Empty = 0,
    Half = 1,
    Full = 2
}