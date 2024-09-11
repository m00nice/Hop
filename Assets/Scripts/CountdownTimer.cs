using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountdownTimer
{

    private float initialTime { get; set; }

    private float Time { get; set; }

    public bool IsRunning { get; protected set; }

    public float Progress => Time / initialTime;

    public Action OnTimerStart = delegate { };
    public Action OnTimerStop = delegate { };


    public CountdownTimer(float value)
    {
        initialTime = value;
        IsRunning = false;
    }

    public void Start()
    {
        Time = initialTime;
        if (!IsRunning)
        {
            IsRunning = true;
            OnTimerStart.Invoke();
        }
    }

    public void Stop()
    {
        Time = initialTime;
        if (IsRunning)
        {
            IsRunning = false;
            OnTimerStop.Invoke();
        }
    }

    public void Resume() => IsRunning = true;
    public void Pause() => IsRunning = false;

    public void Tick(float deltaTime)
    {
        if (IsRunning && Time > 0)
        {
            Time -= deltaTime;
        }

        if (IsRunning && Time <= 0)
        {
            Stop();
        }

    }

    public bool IsFinished => Time <= 0;

    public void Reset() => Time = initialTime;

    public void Reset(float newTime)
    {
        Time = newTime;
        Reset();
    }



}
