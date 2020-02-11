using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Modeling;

namespace StopWatch
{
    /// <summary>
    /// An example model program.
    /// </summary>
    static class StopwatchModelProgram
    {
        public enum TimerMode {Reset, Running, Stopped}

        static bool displayTimer = false;
        static TimerMode timerMode = TimerMode.Reset;
        static bool timerFrozen = false;        

        [Probe]
        public static string DisplayState()
        {
            return String.Format("{0}|{1}{2}",
                displayTimer ? "Timer" : "DateTime",
                timerMode,
                timerFrozen ? "|Frozen" : "");
        
        }

        [Rule(Action = "ModeButton()")]
        static void ModeButton()
        {
            displayTimer = !displayTimer;
        }

        [Rule(Action = "StartStopButton()")]
        static void StartStopButton()
        {
            Condition.IsTrue(displayTimer);
            if (timerMode == TimerMode.Stopped || timerMode == TimerMode.Reset)
                timerMode = TimerMode.Running;
            else
                timerMode = TimerMode.Stopped;
        }

        [Rule(Action = "ResetLapButton()")]
        static void ResetLapButton()
        {
            Condition.IsTrue(displayTimer);
            Condition.IsFalse(timerMode == TimerMode.Reset);
            if (timerMode == TimerMode.Stopped)
                timerMode = TimerMode.Reset;
            else
                timerFrozen = !timerFrozen;
        }

        [Rule(Action = "IsTimerReset()/result")]
        static bool IsTimerReset()
        {
            return timerMode == TimerMode.Reset;
        }


    }
}
