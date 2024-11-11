using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{


    [SerializeField] private TextMeshProUGUI timerText;

    private float timerTime;

    void Start()
    {
        
    }

    
    void Update()
    {
        timerTime += Time.deltaTime;
        UpdateTimerDisplay(timerTime);

    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        // Convert time to hours, minutes, seconds, and centiseconds
        int hours = Mathf.FloorToInt(timeToDisplay / 3600);
        int minutes = Mathf.FloorToInt((timeToDisplay % 3600) / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);
        int centiseconds = Mathf.FloorToInt((timeToDisplay * 100) % 100);

        // Format the time as HH:MM:SS:CS
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}:{3:00}", hours, minutes, seconds, centiseconds);
    }
}
