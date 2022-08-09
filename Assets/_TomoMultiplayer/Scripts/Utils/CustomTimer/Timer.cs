using UnityEngine;
using System;

public abstract class Timer
{
    protected int currentTime; //in seconds
    protected int timerEndTime; // in seconds
    protected int arenaNo;

    protected float perFrameTime;//timer per frame (for frame rate independence)

    private bool hasStarted = false;// to check for timer start
    private bool isPaused = false;// to check for timer pause

    public Action<int> TimerUpdatePerSecond; //to update per second activity for that timer
    public Action<int, int> TimerUpdatePerSecondForArena; //to update per second for a particular arena
    public Action<int> ArenaTimerCompleted;
    public Action TimerCompleted; //to update when timer is completed

    //Setup Timer
    public Timer(int timerEndTime)
    {
        this.timerEndTime = timerEndTime;
        AssignTimerStartValue();
    }

    public Timer(int timerEndTime, int arenaNo)
    {
        this.timerEndTime = timerEndTime;
        this.arenaNo = arenaNo;
        AssignTimerStartValue();
    }

    //Update Timer should run per frame
    public void UpdateTimer()
    {
        if (!hasStarted || isPaused) return;

        TimerCount();

        if(CheckForCompletion())
        {
            hasStarted = false;
            AssignTimerStartValue();
            TimerCompleted?.Invoke();
            ArenaTimerCompleted?.Invoke(arenaNo);
        }
            
    }

    private void TimerCount()
    {
        perFrameTime += Time.deltaTime;

        if (perFrameTime >= 1f)
        {
            perFrameTime = 0f;
            UpdateTimerValue();
            TimerUpdatePerSecond?.Invoke(currentTime);
            TimerUpdatePerSecondForArena?.Invoke(currentTime, arenaNo);
        }
    }

    protected abstract void UpdateTimerValue();

    protected abstract bool CheckForCompletion();


    public void PauseTimer()
    {
        if (hasStarted) isPaused = true;
    }

    public void PlayTimer()
    {
        if (hasStarted) isPaused = false;
    }


    public void ResetTimer()
    {
        hasStarted = false;
        AssignTimerStartValue();
    }

    //Resets from a different point and start the timer
    public void SetAndStartTimer(int timerResetFrom) 
    {
        //Stop Timer
        hasStarted = false;
        timerEndTime = timerResetFrom;
        AssignTimerStartValue();
        StartTimer();
    }

    public void RestartTimer()
    {
        AssignTimerStartValue();
        StartTimer();
    }

    public void StartTimer() => hasStarted = true;


    protected abstract void AssignTimerStartValue();

    public bool IsPaused()
    {
        return isPaused;
    }
  

}
