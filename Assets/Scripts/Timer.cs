using UnityEngine;
using TMPro;
using System;
using UnityEngine.Rendering;
using UnityEngine.ProBuilder.MeshOperations;

public class Timer : MonoBehaviour
{
    // public float TimerTime = 120;
    public HudController hudController;
    public float LateTime;
    public TextMeshProUGUI timerText;
    [Range(0, 60)]
    public int minutes;
    [Range (0,60)]
    public int seconds;
    public Color fontColour;

    private float currentSeconds;
    private int timerDefault;


    private void Start()
    {
        timerText.color = fontColour;
        timerDefault = 0;
        timerDefault += (seconds + (minutes * 60));
        currentSeconds = timerDefault;
    }
    void Update()
    {
      if((currentSeconds -= Time.deltaTime) <= 0) {
        TimerEnded();
                    
        /* TimerTime -= Time.deltaTime;
        if (TimerTime <= 0)
        {
            TimerEnded();
        }
        else
        {
            if (TimerTime <= LateTime)
            {
                //Trigger stuff for end game score
            }*/
        }
      else
        {
            timerText.text = TimeSpan.FromSeconds(currentSeconds).ToString(@"mm\:ss");
        }

        if (currentSeconds <= LateTime)
        {
            // HudController.ScoreStuff
        }

    }
    void TimerEnded()
    {
        //stuff for timer hitting 0 should include late behaviours but allow for edge cases at 0
        timerText.text = "00:00";
        hudController.Lose();
    }
}
