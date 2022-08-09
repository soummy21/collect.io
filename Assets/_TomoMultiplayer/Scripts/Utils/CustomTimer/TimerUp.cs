using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerUp : Timer
{
    public TimerUp(int timerEndTime) : base(timerEndTime)
    {
        //Default Constructor
    }

    protected override void AssignTimerStartValue()
    {
        currentTime = 0;
       
    }

    protected override void UpdateTimerValue()
    {
        currentTime += 1;
    }

    protected override bool CheckForCompletion()
    {
        return (currentTime >= timerEndTime);

    }

}
