using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BarName = System.String;

using Microsoft.Modeling;

namespace scheduler
{
    public enum ProcessState
    {
        INIT,
        MATCH,
        PREWAIT,
        DEFERRED,
        READY,
        RUN,
        REMATCH,
        REWAIT,
        COMPLETE
    }

    public enum SchedulerState
    {
        INIT,
        WAITING,
        EXECUTING,
        COMPLETED,
        EXPIRED
    }

    public class Bar : CompoundValue
    {
        public readonly string name;
        public readonly double duration; //in mili sec         
        public readonly double offset; //ready time for next iteration
        public readonly Set<BarName> triggers; // name of the delagates it can trigger after it is done
        public readonly double earliestStartTime;
        public readonly double deadline; // latestStartTime = deadline - duration         
        public readonly int count;
        public Bar(string name, double d, double offset, Set<BarName> cmds, double est, double lst, int count)
        {
            this.name = name;
            this.duration = d;
            this.offset = offset;
            this.triggers = cmds;
            this.earliestStartTime = est;
            this.deadline = lst;
            this.count = count;
        }
    }
    /// <summary>
    /// An example model program.
    /// </summary>
    public static class ModelProgram
    {   

        [Feature("Scheduler")]
        public static class Scheduler
        {  
            internal static double inf = 1000000;
            internal static SchedulerState state = SchedulerState.INIT;
            internal static Map<BarName, Bar> schedule = new Map<string, Bar>();
            internal static Map<int, Set<Bar>> readyQueue = new Map<int, Set<Bar>>();
            public static Set<Bar> bars = new Set<Bar>();

            [Rule]
            static void Init()
            {
                Condition.IsTrue(state == SchedulerState.INIT);
                setInitSchedule();
                readyQueue = readyQueue.Add(0, new Set<Bar>(schedule["start"]));
                bars = new Set<Bar>(schedule.Values);
                //bars = readyQueue[0];
                state = SchedulerState.WAITING;

            }            

            static void setInitSchedule()
            {
                Condition.IsTrue(schedule.Count <= 0);

                Set<BarName> cmd1 = new Set<BarName>();
                cmd1 = cmd1.Add("sample-microphone");
                cmd1 = cmd1.Add("read-commands");
                Bar bar1 = new Bar("start", 1000.0, inf, cmd1, 0, inf, 1);
                schedule = schedule.Add("start", bar1);

                Set<BarName> cmd2 = new Set<BarName>();
                cmd2 = cmd2.Add("filter");
                Bar bar2 = new Bar("sample-microphone", 1.0, 20.0, cmd2, 0, inf, (int)inf);
                schedule = schedule.Add("sample-microphone", bar2);

                Set<BarName> cmd3 = new Set<BarName>();
                cmd3 = cmd3.Add("fft-start");
                Bar bar3 = new Bar("filter", 1.0, inf, cmd3, 0, inf, (int)inf);
                schedule = schedule.Add("filter", bar3);


                Set<BarName> cmd4 = new Set<BarName>();
                cmd4 = cmd4.Add("fft");
                Bar bar4 = new Bar("fft-start", 10.0, inf, cmd4, 0, inf, (int)inf);
                schedule = schedule.Add("fft-start", bar4);

                Set<BarName> cmd5 = new Set<BarName>();
                cmd5 = cmd5.Add("fft");
                cmd5 = cmd5.Add("send-bytes");
                Bar bar5 = new Bar("fft", 10.0, inf, cmd5, 0, inf, 10);
                schedule = schedule.Add("fft", bar5);

                Set<BarName> cmd6 = new Set<BarName>();
                Bar bar6 = new Bar("send-bytes", 0.2, 1, cmd6, 0, inf, 4);
                schedule = schedule.Add("send-bytes", bar6);

                Set<BarName> cmd7 = new Set<BarName>();
                Bar bar7 = new Bar("read-commands", 1, 10, cmd7, 0, inf, (int)inf);
                schedule = schedule.Add("read-command", bar7);
                //state = SchedulerState.WAITING;
            }
            /*
            static bool setInitScheduleEnabled()
            {
                return schedule.IsEmpty;
            }*/

            [Rule(Action = "Match(b)")]
            static void Match([Domain("bars")]Bar b)
            {
                Condition.IsTrue(state == SchedulerState.WAITING);

                Set<Bar> readyBars;
                if (readyQueue.ContainsKey(TimeUpdate.time))
                {
                    readyBars = readyQueue[TimeUpdate.time];
                    bars = readyBars;
                    if (readyBars.Contains(b))
                    {
                        state = SchedulerState.WAITING;
                    }
                    else
                        state = SchedulerState.COMPLETED;
                }
                else
                    state = SchedulerState.COMPLETED;
            }            

            static void UpdateReadyQueue(Bar b, int currentTime)
            {
                readyQueue = readyQueue.Remove(currentTime);
                foreach (Bar b1 in schedule.Values)
                {
                    if (b.triggers.Contains(b1.name))
                    {
                        if (readyQueue.ContainsKey(currentTime + (int)b.duration))
                        {
                            Set<Bar> bars = readyQueue[currentTime + (int)b.duration];
                            readyQueue = readyQueue.Remove(currentTime + (int)b.duration);
                            readyQueue = readyQueue.Add(currentTime + (int)b.duration, bars.Add(b1));
                        }
                        else
                            readyQueue = readyQueue.Add(currentTime + (int)b.duration, new Set<Bar>(b1));
                    }
                }
                if (b.offset < Scheduler.inf)
                    if (readyQueue.ContainsKey(currentTime + (int)b.offset))
                    {
                        Set<Bar> bars = readyQueue[currentTime + (int)b.offset];
                        readyQueue = readyQueue.Remove(currentTime + (int)b.offset);
                        readyQueue = readyQueue.Add(currentTime + (int)b.offset, bars.Add(b));
                    }
                    else
                        readyQueue = readyQueue.Add(currentTime + (int)b.offset, new Set<Bar>(b));
            }

            [Rule(Action = "StartExecute(b)")]
            public static void StartExecute([Domain("bars")]Bar b)
            {
                Condition.IsTrue(state == SchedulerState.WAITING && Process.state == ProcessState.PREWAIT);

                if (TimeUpdate.time >= b.earliestStartTime &&
                    TimeUpdate.time < (b.deadline - b.duration))
                {
                    state = SchedulerState.EXECUTING;
                    UpdateReadyQueue(b, TimeUpdate.time);
                    Process.state = ProcessState.DEFERRED;
                }
                else
                {
                    TimeUpdate.tickEnabled = true;
                }
            }

            [Rule]
            static void Completed()
            {
                Condition.IsTrue(state == SchedulerState.COMPLETED);
                bars = readyQueue[TimeUpdate.time];
                state = SchedulerState.WAITING;
            }
            
        }
        [Feature("TimeUpdate")]
        public static class TimeUpdate
        {
            internal static int time = 0;
            internal static bool tickEnabled = false;
            [Rule]
            static void Tick(int i)
            {
                Condition.IsTrue(tickEnabled);
                time = time + i;
                tickEnabled = false;
            }
        }

        [Probe]
        public static int Clock()
        {
            return TimeUpdate.time;
        }

        [Feature("Process")]
        public static class Process
        {           
            public static Set<Bar> bars = new Set<Bar>();
            public static ProcessState state = ProcessState.INIT;
            public static Map<BarName, int> frequncy = new Map<string, int>();
            public static Set<Bar> currentBar = new Set<Bar>();
            static Bar globalBar;
            static int readyQueueCount;
            static bool readyQueueExists;
            static Set<Bar> readyBars;

            [Rule]
            public static void Init()
            {
                Condition.IsTrue(state == ProcessState.INIT);
                state = ProcessState.MATCH;
            }

            [Rule(Action = "Match(b)")]
            static void Match(Bar b)
            {
                Condition.IsTrue((state == ProcessState.MATCH) || (state == ProcessState.REMATCH));
                
                readyQueueCount= Scheduler.readyQueue.Count;
                readyQueueExists = Scheduler.readyQueue.ContainsKey(TimeUpdate.time);                
                
                if (Scheduler.readyQueue.ContainsKey(TimeUpdate.time))
                {
                    readyBars = Scheduler.readyQueue[TimeUpdate.time];
                    if (readyBars.Contains(b))
                    {
                        currentBar = new Set<Bar>(b);
                        switch (state)
                        {
                            case ProcessState.MATCH:
                                state = ProcessState.PREWAIT;
                                break;
                            case ProcessState.REMATCH:
                                state = ProcessState.REWAIT;
                                break;
                        }
                    }
                    else
                        state = ProcessState.COMPLETE;
                }
                else state = ProcessState.COMPLETE;
            }

            //[Rule(Action = "StartExecute(b)")]
            //public static void StartExecute(Bar b)
            //{
            //    //Condition.IsTrue(state == ProcessState.PREWAIT && Scheduler.state == SchedulerState.WAITING);

            //    globalBar = b;

            //    //state = ProcessState.DEFERRED;

            //}

            [Rule(Action = "GetStack(b)")]
            static void GetStack([Domain("currentBar")]Bar b)
            {
                Condition.IsTrue((state == ProcessState.DEFERRED));
                if (Resources.stacks > 0)
                    state = ProcessState.READY;
                else
                    TimeUpdate.tickEnabled = true;
            }

            [Rule]
            static void GetProcessor([Domain("currentBar")]Bar b)
            {
                Condition.IsTrue((state == ProcessState.READY));

                if (Resources.cpus > 0)
                    state = ProcessState.RUN;
                else
                    TimeUpdate.tickEnabled = true;
            }

            [Rule]
            static void Run([Domain("currentBar")]Bar b)
            {
                Condition.IsTrue(state == ProcessState.RUN);

                TimeUpdate.time = TimeUpdate.time + (int)b.duration;
                Resources.cpus = Resources.cpus + 1;
                Resources.stacks = Resources.stacks + 1;
                state = ProcessState.COMPLETE;
                currentBar = currentBar.Remove(b);
            }

            [Rule]
            static void Completed()
            {
                Condition.IsTrue(state == ProcessState.COMPLETE);
                state = ProcessState.MATCH;
            }
        }
        [Feature("Resources")]
        public static class Resources
        {
            public static ProcessState state = ProcessState.INIT;
            public static int stacks = 1;
            public static int cpus = 1;
            public static Bar resourceBar;

            [Rule(Action = "GetStack(b)")]
            public static void GetStack(Bar b)
            {
                resourceBar = b;                

                if (stacks > 0)
                    stacks = stacks - 1;
            }
            [Rule(Action = "GetProcessor(b)")]
            public static void GetProcessor(Bar b)
            { 
                if (cpus > 0)
                    cpus = cpus - 1;
            }
        }
        
    }
    
}
