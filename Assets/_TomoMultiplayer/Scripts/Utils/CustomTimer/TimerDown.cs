using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerDown : Timer
{

    public TimerDown(int timerEndTime) : base(timerEndTime)
    {
        //Default Constructor
    }

    public TimerDown(int timerEndTime, int arenaNo) : base(timerEndTime, arenaNo)
    {
        //Default Constructor
    }

    protected override void AssignTimerStartValue()
    {
        currentTime = timerEndTime;
    }

    protected override void UpdateTimerValue()
    {
        currentTime -= 1;
    }

    protected override bool CheckForCompletion()
    {
        if (currentTime <= 0)
            return true;
        else
            return false;
                
    }
}
