using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Linq;


using Microsoft.Modeling;

namespace Travelling
{
    /// <summary>
    /// An example model program.
    /// </summary>
    public static class ModelProgram
    {
        public enum travel_state { start, depart, arrive, end, finish };

        static MapContainer<string, travel_state> commuter_currentstatus = new MapContainer<string, travel_state>();

        static MapContainer<string, string> commuter_to_currentStation = new MapContainer<string, string>();

        static SequenceContainer<string> journey_map = new SequenceContainer<string>() { "Lavis", "Trento", "Segantini Dogana", "Travai Al Nuoto", "Mezzocorona" };

        static Set<Commuter> commuters = new Set<Commuter>();        

        static string current_station;                

        public static SetContainer<string> stations()
        {
            return new SetContainer<string> { "Lavis", "Trento", "Segantini Dogana", "Travai Al Nuoto", "Mezzocorona" };            
        }

        public static Set<string> CommutersNames()
        {
            return new Set<string>(commuter_currentstatus.Keys);
        }        

        [Rule]
        public static void start(Commuter commuter)
        {
            Requires(!commuter_currentstatus.Keys.Contains(commuter.name));
            commuter_currentstatus.Add(commuter.name, travel_state.depart);
            commuter_to_currentStation.Add(commuter.name, commuter.origin);
            commuters = commuters.Add(commuter);
        }        

        [Rule]
        public static void departed([Domain("CommutersNames")]string name, [Domain("stations")] string station)
        {
            Requires(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.depart);
            Condition.IsTrue(commuter_to_currentStation[name] == station);           

            NextStation(name);
            commuter_currentstatus.Override(name, travel_state.arrive);            

        }

        [Rule]
        public static void arrive(string name, [Domain("stations")] string station)
        {
            current_station = station;
            Requires(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.arrive);
            Condition.IsTrue(commuter_to_currentStation[name] == station);

            if (IsLastStation(name, station))
            {
                commuter_currentstatus.Override(name, travel_state.end);
            }
            else
            {
                commuter_currentstatus.Override(name, travel_state.depart);
            }            
        }

        [Rule]
        public static void journey_end(string name, [Domain("stations")] string station)
        {
            Condition.IsTrue(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.end);
            Condition.IsTrue(commuter_to_currentStation[name] == station);            
            commuter_currentstatus.Override(name, travel_state.finish);
        }

        [Rule]
        public static void finish(string name)
        {
            Condition.IsTrue(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.finish);
        }
        
        
        //[AcceptingStateCondition]
        //public static void Accepting(string name)
        //{
        //    commuter_to_currentStation[name] == "Mezzocorona";
        //}        

        public static void NextStation(string commuter_name)
        {
            string stn = commuter_to_currentStation[commuter_name];
            int idx1 = journey_map.IndexOf(stn);
            idx1++;
            commuter_to_currentStation[commuter_name] = journey_map[idx1];
        }

        public static bool IsLastStation(string commuter, string station)
        {
            foreach (Commuter c in commuters)
            {                
                if (c.destination == station)
                    return true;

            }
            return false;
        }

        static void Requires(bool condition)
        {
            Condition.IsTrue(condition);
        }
    }

    //public class Commuter : CompoundValue
    //{
    //    public string name;
    //    public string origin;
    //    public string destination;

    //    public Commuter(string name, string origin, string destination)
    //    {
    //        this.name = name;
    //        this.origin = origin;
    //        this.destination = destination;
    //    }
    //}

    public struct Commuter
    {
        public string name;
        public string origin;
        public string destination;

        public Commuter(string name, string origin, string destination)
        {
            this.name = name;
            this.origin = origin;
            this.destination = destination;
        }
    }
    
}

namespace TravellingCounter
{

    public static class ModelProgram
    {
        public enum travel_state { start, depart, arrive, end, finish };

        static MapContainer<string, travel_state> commuter_currentstatus = new MapContainer<string, travel_state>();

        static MapContainer<string, string> commuter_to_currentStation = new MapContainer<string, string>();

        static SequenceContainer<string> route_A = new SequenceContainer<string>() { "Lavis", "Trento", "Segantini Dogana", "Travai Al Nuoto", "Mezzocorona" };

        static Set<Commuter> commuters = new Set<Commuter>();

        static int idx_for_bob;

        static string current_station;

        static int bobs_count;

        static int idx_count;

        static bool isLastStation;

        static string dest;

        static string nom;

        static int journey_leg_duration;

        static int countDownUntilNewTicketPrice = 70;        

        public static MapContainer<string, Map<string, int>> journey_map = new MapContainer<string, Map<string, int>>();


        public static Set<string> CommutersNames()
        {
            return new Set<string>(commuter_currentstatus.Keys);
        }

        static void Requires(bool condition)
        {
            Condition.IsTrue(condition);
        }

        public static Set<int> CountDown()
        {
            return new Set<int>(countDownUntilNewTicketPrice);
        }

        [Rule]
        public static void start(Commuter commuter)
        {
            Requires(!commuter_currentstatus.Keys.Contains(commuter.name));
            commuter_currentstatus.Add(commuter.name, travel_state.depart);
            commuter_to_currentStation.Add(commuter.name, commuter.origin);
            commuters = commuters.Add(commuter);
            journey_map.Add("Lavis", new Map<string, int>().Add("Trento", 6 ));
            journey_map.Add("Trento", new Map<string, int>().Add("Segantini Dogana", 5));
            journey_map.Add("Segantini Dogana", new Map<string, int>().Add("Travai Al Nuoto", 8));
            journey_map.Add("Travai Al Nuoto", new Map<string, int>().Add("Mezzocorona", 3));            
        }        

        [Rule]
        public static int departed([Domain("CommutersNames")]string name, string station, [Domain("CountDown")] int countDown)
        {
            Requires(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.depart);
            Condition.IsTrue(commuter_to_currentStation[name] == station);

            NextStation(name);
            commuter_currentstatus.Override(name, travel_state.arrive);
            countDownUntilNewTicketPrice = countDown;
            Map<string, int> leg = journey_map[station];
            countDownUntilNewTicketPrice -= leg.ToArray()[0].Value;
            return countDownUntilNewTicketPrice;

        }

        [Rule]
        public static int arrive(string name, string station, [Domain("CountDown")] int countDown)
        {
            current_station = station;
            Requires(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.arrive);
            Condition.IsTrue(commuter_to_currentStation[name] == station);
            Condition.IsTrue(countDown >= 0 && countDown < 70);
            Condition.IsTrue(countDownUntilNewTicketPrice == countDown);

            if (IsLastStation(name, station))
            {
                commuter_currentstatus.Override(name, travel_state.end);
            }
            else
            {
                commuter_currentstatus.Override(name, travel_state.depart);
            }
            countDownUntilNewTicketPrice = countDown;            
            return countDownUntilNewTicketPrice;
        }

        [Rule]
        public static void journey_end(string name, string station, int countDown)
        {
            Condition.IsTrue(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.end);
            Condition.IsTrue(commuter_to_currentStation[name] == station);
            Condition.IsTrue(countDown >= 0 && countDown < 70);
            Condition.IsTrue(countDownUntilNewTicketPrice == countDown);
            commuter_currentstatus.Override(name, travel_state.finish);
        }

        [Rule]
        public static void finish(string name)
        {
            Condition.IsTrue(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.finish);
        }

        //[AcceptingStateCondition]
        //public static void Accepting(string name)
        //{
        //    commuter_to_currentStation[name] == "Mezzocorona";
        //}


        [Probe]
        public static bool LastStation()
        {
            return isLastStation;
        }

        [Probe]
        public static int BobCount()
        {
            return bobs_count;
        }

        [Probe]
        public static string Destination()
        {
            return dest;
        }

        [Probe]
        public static string Name()
        {
            return nom;
        }


        [Probe]
        public static int IdxCount()
        {
            return idx_count;
        }

        //[StateInvariant]
        //public static bool TicketIncreaseCounter()
        //{
        //    return countDownUntilNewTicketPrice >= 0 && countDownUntilNewTicketPrice <70;
        //}

        public static void NextStation(string commuter_name)
        {
            string stn = commuter_to_currentStation[commuter_name];
            int idx1 = route_A.IndexOf(stn);
            idx1++;
            commuter_to_currentStation[commuter_name] = route_A[idx1];
        }

        public static bool IsLastStation(string commuter, string station)
        {
            foreach (Commuter c in commuters)
            {
                if (c.destination == station)
                    return true;

            }
            return false;
        }
    }

    //public class Commuter : CompoundValue
    //{
    //    public string name;
    //    public string origin;
    //    public string destination;

    //    public Commuter(string name, string origin, string destination)
    //    {
    //        this.name = name;
    //        this.origin = origin;
    //        this.destination = destination;
    //    }
    //}

    public struct Commuter
    {
        public string name;
        public string origin;
        public string destination;

        public Commuter(string name, string origin, string destination)
        {
            this.name = name;
            this.origin = origin;
            this.destination = destination;
        }
    }




}

namespace TravellingClock
{

    public static class ModelProgram
    {
        public enum travel_state { start, depart, arrive, end, finish };

        static MapContainer<string, travel_state> commuter_currentstatus = new MapContainer<string, travel_state>();

        static MapContainer<string, string> commuter_to_currentStation = new MapContainer<string, string>();

        static MapContainer<string, Map<int, string>> commuter_to_temporal = new MapContainer<string, Map<int, string>>();

        static SequenceContainer<string> route_A = new SequenceContainer<string>() { "Lavis", "Trento", "Segantini Dogana", "Travai Al Nuoto", "Mezzocorona" };

        static Set<Commuter> commuters = new Set<Commuter>();

        static int idx_for_bob;

        static string current_station;

        static int bobs_count;

        static int idx_count;

        static bool isLastStation;

        static string dest;

        static string nom;

        static int journey_leg_duration;

        static int countDownUntilNewTicketPrice = 70;

        //static DateTime timeSpan = new DateTime(15, 00, 00);

        static string timeSpan = "15:00:00";        

        public static MapContainer<string, Map<string, int>> journey_map = new MapContainer<string, Map<string, int>>();      


        public static Set<string> CommutersNames()
        {
            return new Set<string>(commuter_currentstatus.Keys);
        }

        static void Requires(bool condition)
        {
            Condition.IsTrue(condition);
        }

        public static Set<int> CountDown()
        {
            return new Set<int>(countDownUntilNewTicketPrice);
        }

        public static Set<string> Clock()
        {
            return new Set<string>(timeSpan.ToString());
        }


        [Rule]
        public static void start(Commuter commuter)
        {
            Requires(!commuter_currentstatus.Keys.Contains(commuter.name));
            commuter_currentstatus.Add(commuter.name, travel_state.depart);
            commuter_to_currentStation.Add(commuter.name, commuter.origin);             
            commuters = commuters.Add(commuter);
            BuildJourneyMap();           
        }        

        [Rule]
        public static int departed([Domain("CommutersNames")]string name, string station, [Domain("CountDown")] int countDown, [Domain("Clock")] string clock)
        {
            Requires(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.depart);
            Condition.IsTrue(commuter_to_currentStation[name] == station);

            NextStation(name);
            commuter_currentstatus.Override(name, travel_state.arrive);            

            int offset = CalculateJourneyOffset(station);
            countDownUntilNewTicketPrice -= offset;
            timeSpan = ClockCalc(clock, offset);
            return countDownUntilNewTicketPrice;

        }

        [Rule]
        public static int arrive(string name, string station, [Domain("CountDown")] int countDown, [Domain("Clock")] string clock)
        {
            current_station = station;
            Requires(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.arrive);
            Condition.IsTrue(commuter_to_currentStation[name] == station);
            Condition.IsTrue(countDown >= 0 && countDown < 70);
            Condition.IsTrue(countDownUntilNewTicketPrice == countDown);

            if (IsLastStation(name, station))
            {
                commuter_currentstatus.Override(name, travel_state.end);
            }
            else
            {
                commuter_currentstatus.Override(name, travel_state.depart);
            }
            countDownUntilNewTicketPrice = countDown;            
            return countDownUntilNewTicketPrice;
        }

        [Rule]
        public static void journey_end(string name, string station, int countDown, [Domain("Clock")] string clock)
        {
            Condition.IsTrue(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.end);
            Condition.IsTrue(commuter_to_currentStation[name] == station);
            Condition.IsTrue(countDown >= 0 && countDown < 70);
            Condition.IsTrue(countDownUntilNewTicketPrice == countDown);
            commuter_currentstatus.Override(name, travel_state.finish);
        }

        [Rule]
        public static void finish(string name)
        {
            Condition.IsTrue(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.finish);
        }

        public static void NextStation(string commuter_name)
        {
            string stn = commuter_to_currentStation[commuter_name];
            int idx1 = route_A.IndexOf(stn);
            idx1++;
            commuter_to_currentStation[commuter_name] = route_A[idx1];
        }

        public static bool IsLastStation(string commuter, string station)
        {
            foreach (Commuter c in commuters)
            {
                if (c.destination == station)
                    return true;

            }
            return false;
        }

        public static string ClockCalc(string currentTimeOclockOnly, int minutes_offset)
        {
            StringBuilder sb = new StringBuilder();
            string[] timeArray = currentTimeOclockOnly.Split(new[] { ':' });
            int hours_offset = 0;

            int hours = Convert.ToInt32(timeArray[0]);
            int minutes = Convert.ToInt32(timeArray[1]);
            int seconds = Convert.ToInt32(timeArray[2]);

            if(minutes_offset > 59)
            {
                int newMinutes = minutes_offset % 60;

                if (newMinutes != 0)
                {
                    minutes_offset = newMinutes;                    
                }
                hours_offset = minutes_offset / 60;

            }
            if (minutes + minutes_offset < 10)
            {
                return sb.AppendFormat("{0}:{1}:{2}", hours + hours_offset, "0" + minutes_offset, "0" + seconds).ToString();   
            }

            sb.AppendFormat("{0}:{1}:{2}", hours + hours_offset, minutes + minutes_offset, "0" + seconds);

            return sb.ToString();
        }

        public static void BuildJourneyMap()
        {
            if (journey_map.Count <= 0)
            {
                journey_map.Add("Lavis", new Map<string, int>().Add("Trento", 6));
                journey_map.Add("Trento", new Map<string, int>().Add("Segantini Dogana", 5));
                journey_map.Add("Segantini Dogana", new Map<string, int>().Add("Travai Al Nuoto", 8));
                journey_map.Add("Travai Al Nuoto", new Map<string, int>().Add("Mezzocorona", 3));            
            }

            
        
        }

        public static int CalculateJourneyOffset(string station)
        {
            Map<string, int> leg = journey_map[station];
            return leg.ToArray()[0].Value;
        }
    }

    //public class Commuter : CompoundValue
    //{
    //    public string name;
    //    public string origin;
    //    public string destination;

    //    public Commuter(string name, string origin, string destination)
    //    {
    //        this.name = name;
    //        this.origin = origin;
    //        this.destination = destination;
    //    }
    //}

    public struct Commuter
    {
        public string name;
        public string origin;
        public string destination;
        public string startTime;

        public Commuter(string name, string origin, string destination, string startTime)
        {
            this.name = name;
            this.origin = origin;
            this.destination = destination;
            this.startTime = startTime;
        }
    }
}

namespace TravellingIsolateClocks
{
    public static class ModelProgram
    {
        public enum travel_state { start, depart, arrive, end, finish };

        static MapContainer<string, travel_state> commuter_currentstatus = new MapContainer<string, travel_state>();

        static MapContainer<string, string> commuter_to_currentStation = new MapContainer<string, string>();

        static MapContainer<string, int> commuter_to_temporal = new MapContainer<string, int>();

        static SequenceContainer<string> route_A = new SequenceContainer<string>() { "Lavis", "Trento", "Segantini Dogana", "Travai Al Nuoto", "Mezzocorona" };

        static Set<Commuter> commuters = new Set<Commuter>();

        static int idx_for_bob;

        static string current_station;

        static int bobs_count;

        static int idx_count;

        static bool isLastStation;

        static string dest;

        static string nom;

        static int journey_leg_duration;

        static int countDownUntilNewTicketPrice = 70;

        //static DateTime timeSpan = new DateTime(15, 00, 00);

        static string timeSpan = "15:00:00";

        //static string timeSpan;

        public static MapContainer<string, Map<string, int>> journey_map = new MapContainer<string, Map<string, int>>();


        public static Set<string> CommutersNames()
        {
            return new Set<string>(commuter_currentstatus.Keys);
        }

        static void Requires(bool condition)
        {
            Condition.IsTrue(condition);
        }

        public static Set<int> CountDown()
        {
            return new Set<int>(countDownUntilNewTicketPrice);
        }

        public static Set<string> Clock()
        {
            return new Set<string>(timeSpan.ToString());
        }


        [Rule]
        public static void start(Commuter commuter)
        {
            Requires(!commuter_currentstatus.Keys.Contains(commuter.name));
            commuter_currentstatus.Add(commuter.name, travel_state.depart);
            commuter_to_currentStation.Add(commuter.name, commuter.origin);
            commuters = commuters.Add(commuter);            
            
            BuildJourneyMap();
        }

        [Rule]
        public static int departed([Domain("CommutersNames")]string name, string station, [Domain("CountDown")] int countDown, [Domain("Clock")] string clock)
        {
            Requires(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.depart);
            Condition.IsTrue(commuter_to_currentStation[name] == station);

            NextStation(name);
            commuter_currentstatus.Override(name, travel_state.arrive);

            int offset = CalculateJourneyOffset(station);
            countDownUntilNewTicketPrice -= offset;
            timeSpan = ClockCalc(clock, offset);
            return countDownUntilNewTicketPrice;

        }

        [Rule]
        public static int arrive(string name, string station, [Domain("CountDown")] int countDown, [Domain("Clock")] string clock)
        {
            current_station = station;
            Requires(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.arrive);
            Condition.IsTrue(commuter_to_currentStation[name] == station);
            Condition.IsTrue(countDown >= 0 && countDown < 70);
            Condition.IsTrue(countDownUntilNewTicketPrice == countDown);

            if (IsLastStation(name, station))
            {
                commuter_currentstatus.Override(name, travel_state.end);
            }
            else
            {
                commuter_currentstatus.Override(name, travel_state.depart);
            }
            countDownUntilNewTicketPrice = countDown;
            return countDownUntilNewTicketPrice;
        }

        [Rule]
        public static void journey_end(string name, string station, int countDown, [Domain("Clock")] string clock)
        {
            Condition.IsTrue(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.end);
            Condition.IsTrue(commuter_to_currentStation[name] == station);
            Condition.IsTrue(countDown >= 0 && countDown < 70);
            Condition.IsTrue(countDownUntilNewTicketPrice == countDown);
            commuter_currentstatus.Override(name, travel_state.finish);            
        }

        [Rule]
        public static void finish(string name)
        {
            Condition.IsTrue(commuter_currentstatus.Keys.Contains(name));
            Condition.IsTrue(commuter_currentstatus[name] == travel_state.finish);
        }

        public static void NextStation(string commuter_name)
        {
            string stn = commuter_to_currentStation[commuter_name];
            int idx1 = route_A.IndexOf(stn);
            idx1++;
            commuter_to_currentStation[commuter_name] = route_A[idx1];
        }

        public static bool IsLastStation(string commuter, string station)
        {
            foreach (Commuter c in commuters)
            {
                if (c.destination == station)
                    return true;

            }
            return false;
        }

        public static string ClockCalc(string currentTimeOclockOnly, int minutes_offset)
        {
            StringBuilder sb = new StringBuilder();
            string[] timeArray = currentTimeOclockOnly.Split(new[] { ':' });
            int hours_offset = 0;

            int hours = Convert.ToInt32(timeArray[0]);
            int minutes = Convert.ToInt32(timeArray[1]);
            int seconds = Convert.ToInt32(timeArray[2]);

            if (minutes_offset > 59)
            {
                int newMinutes = minutes_offset % 60;

                if (newMinutes != 0)
                {
                    minutes_offset = newMinutes;
                }
                hours_offset = minutes_offset / 60;

            }
            if (minutes + minutes_offset < 10)
            {
                return sb.AppendFormat("{0}:{1}:{2}", hours + hours_offset, "0" + minutes_offset, "0" + seconds).ToString();
            }

            sb.AppendFormat("{0}:{1}:{2}", hours + hours_offset, minutes + minutes_offset, "0" + seconds);

            return sb.ToString();
        }

        public static void BuildJourneyMap()
        {
            if (journey_map.Count <= 0)
            {
                journey_map.Add("Lavis", new Map<string, int>().Add("Trento", 6));
                journey_map.Add("Trento", new Map<string, int>().Add("Segantini Dogana", 5));
                journey_map.Add("Segantini Dogana", new Map<string, int>().Add("Travai Al Nuoto", 8));
                journey_map.Add("Travai Al Nuoto", new Map<string, int>().Add("Mezzocorona", 3));
            }



        }

        public static int CalculateJourneyOffset(string station)
        {
            Map<string, int> leg = journey_map[station];
            return leg.ToArray()[0].Value;
        }
    }

    //public class Commuter : CompoundValue
    //{
    //    public string name;
    //    public string origin;
    //    public string destination;

    //    public Commuter(string name, string origin, string destination)
    //    {
    //        this.name = name;
    //        this.origin = origin;
    //        this.destination = destination;
    //    }
    //}

    public struct Commuter
    {
        public string name;
        public string origin;
        public string destination;        

        public Commuter(string name, string origin, string destination)
        {
            this.name = name;
            this.origin = origin;
            this.destination = destination;
            
        }
    }

}
namespace TravellingSchedule
{
    public static class ModelProgram
    { 
        static MapContainer<string, string> commuter_to_currentStation = new MapContainer<string, string>();        

        static SequenceContainer<string> route_A = new SequenceContainer<string>() { "Lavis", "Trento", "Segantini Dogana", "Travai Al Nuoto", "Mezzocorona" };  
      
        static Set<string> firstTimeCommuter = new Set<string>();

        static Set<string> routeCompleteCommuter = new Set<string>();

        static Set<string> repeatTraveller = new Set<string>();

        static SetContainer<Commuter> travellers_today = new SetContainer<Commuter>();

        static string clockedCommuter; 

        static string current_commuter;

        static int countDownUntilNewTicketPrice = 70;

        static string timeSpan = String.Empty;        

        static int offset;

        static int newTimeOffset;

        static int newTimeOffset2;

        static int arrivalSetCount;

        static SetContainer<int> list; 

        public static MapContainer<string, Map<string, int>> journey_map = new MapContainer<string, Map<string, int>>();

        static SetContainer<Commuter> arrivalSet = new SetContainer<Commuter>();

        static void Requires(bool condition)
        {
            Condition.IsTrue(condition);
        }

        public static Set<int> CountDown()
        {
            return new Set<int>(countDownUntilNewTicketPrice);
        }

        public static Set<string> Clock()
        {
            if (commuters_time.Count > 0)
            {                
                newTimeOffset2 = GetMinInt(commuters_time);
                timeSpan = AddMinutesToClock(timeSpan, newTimeOffset2);
            }
            
            return new Set<string>(timeSpan.ToString());
        }

        public static Set<string> GlobalClock()
        {
            if (arrivalSet.Count < 1 && firstTimeCommuter.Contains(current_commuter))
            {
                firstTimeCommuter = firstTimeCommuter.Remove(current_commuter);
                newTimeOffset = GetMinInt(commuters_time);
                timeSpan = AddMinutesToClock(timeSpan, newTimeOffset);
            }
            else if (arrivalSet.Count > 0)
            {
                if (commuters_time.Count > 0)
                {
                    newTimeOffset2 = GetMinInt(commuters_time);
                    timeSpan = AddMinutesToClock(timeSpan, newTimeOffset2);
                }            
            }

            return new Set<string>(timeSpan.ToString());
        }

        public static Set<string> Clocks()
        {            
            if (arrivalSet.Count < 1)
            {
                if (firstTimeCommuter.Contains(current_commuter))
                {
                    firstTimeCommuter = firstTimeCommuter.Remove(current_commuter);
                    newTimeOffset = GetMinInt(commuters_time);
                    timeSpan = AddMinutesToClock(timeSpan, newTimeOffset);                   
                
                }  
            }
            
            return new Set<string>(timeSpan.ToString());
        }

        public static int GetMinInt(MapContainer<string, int> map)
        {
            list = new SetContainer<int>(map.Values);

            int lowestValue = 0;

            int[] ar = list.ToArray();

            if (ar.Length > 0)
            {
                lowestValue = ar[ar.Length - 1];
            }

            return lowestValue;
        }

        public static SetContainer<Commuter> Departures()
        {
            return new SetContainer<Commuter>(departureSet);
        }

        public static SetContainer<Commuter> Arrivals()
        {
            return new SetContainer<Commuter>(arrivalSet);
        }

        public static Set<string> Completed()
        {
            return new Set<string>(routeCompleteCommuter);
        }

        public static MapContainer<string, int> commuters_time = new MapContainer<string, int>();

        public static SetContainer<Commuter> departureSet = new SetContainer<Commuter>();   

        [Rule]
        public static void start(string name, string origin, string destination, int initialoffset, string start_time)
        {
            Commuter commuter = new Commuter(name, origin, destination, initialoffset);

            Requires(!travellers_today.Contains(commuter) && NoArrivalsNorDepartures(commuter));

            Condition.IsTrue(!routeCompleteCommuter.Contains(name));            

            BuildJourneyMap();

            if (timeSpan == String.Empty)
            {
                timeSpan = start_time;
            }

            if(travellers_today.Count == 0)
            {
                commuters_time.Add(commuter.name, initialoffset);
            }

            departureSet.Add(commuter);

            commuter_to_currentStation[commuter.name] = commuter.origin;            
  
            firstTimeCommuter = firstTimeCommuter.Add(name);

            current_commuter = commuter.name;

            travellers_today.Add(commuter);           
        }

        [Rule]
        public static void travel_depart([Domain("Clocks")] string clock, [Domain("Departures")] Commuter commuter, string station)
        {
            Requires(travellers_today.Contains(commuter));
            Condition.IsNotNull(commuter);
            Condition.IsTrue(commuter_to_currentStation[commuter.name] == station);

            departureSet.Remove(commuter);
            arrivalSet.Add(commuter);

            firstTimeCommuter.Remove(commuter.name);

            commuters_time.Remove(commuter.name);

            offset = CalculateJourneyOffset(commuter_to_currentStation[commuter.name]);

            commuters_time.Override(commuter.name, offset);

            NextStation(commuter.name);
        }

        [Rule]
        public static void travel_arrive([Domain("Clock")] string clock, [Domain("Arrivals")] Commuter commuter, string station)
        {
            Requires(travellers_today.Contains(commuter));
            Condition.IsNotNull(commuter);
            Condition.IsTrue(commuter_to_currentStation[commuter.name] == station);

            if (commuter_to_currentStation[commuter.name] == commuter.destination)
            {
                arrivalSet.Remove(commuter);
                commuters_time.Remove(commuter.name);
                routeCompleteCommuter = routeCompleteCommuter.Add(commuter.name);
            }
            else
            {
                arrivalSet.Remove(commuter);
                departureSet.Add(commuter);
            }
        }

        [Rule]
        public static void journey_end([Domain("Clocks")] string clock, [Domain("Completed")] string commuter)
        {
            Condition.IsTrue(routeCompleteCommuter.Contains(commuter));
        }

        //[Rule]
        //public static void finish([Domain("Clocks")] string clock, [Domain("Arrivals")] Commuter commuter)
        //{

        //}

        public static MapContainer<int, string> Override(MapContainer<int, string> map, int offset, string name)
        {            
            int key;
            string value;
            MapContainer<int, string> newMap = new MapContainer<int, string>();
            newMap.Add(offset, name);
            foreach (var c in map)
            {
                key = c.Key;
                value = c.Value;

                if (value == name)
                {
                    map.Override(newMap);
                }
                
            }
            return map;       
        }        

        public static bool NoArrivalsNorDepartures(Commuter user)
        {
            return (!arrivalSet.Contains(user) && !departureSet.Contains(user));    
        }        

        [Probe]
        public static MapContainer<string, int> CommutersProbe()
        {
            return commuters_time;
        }

        [Probe]
        public static SetContainer<Commuter> CommuterProbe()
        {
            return departureSet;        
        }

        
        [Probe]
        public static int JourneyOffset()
        {
            return offset;
        }

        public static void NextStation(string commuter_name)
        {
            string stn = commuter_to_currentStation[commuter_name];
            int idx1 = route_A.IndexOf(stn);
            idx1++;
            commuter_to_currentStation[commuter_name] = route_A[idx1];
        }

        public static string ReturnNextStation(string station)
        {
            int idx1 = route_A.IndexOf(station);
            idx1++;
            return route_A[idx1];
        }

        public static MapContainer<string, int> SortCommuters(MapContainer<string, int> commuters)
        {
            List<KeyValuePair<string, int>> c = commuters.ToArray().ToList<KeyValuePair<string, int>>();
            c.Sort((x, y) => (y.Value.CompareTo(x.Value)));

            MapContainer<string, int> newCommuters = new MapContainer<string, int>();
            foreach (KeyValuePair<string, int> kvp in c)
            {
                newCommuters.Add(kvp);
            }

            return newCommuters;
        }        

        public static string AddMinutesToClock(string currentTimeOclockOnly, int minutes_offset)
        {
            StringBuilder sb = new StringBuilder();
            string[] timeArray = currentTimeOclockOnly.Split(new[] { ':' });
            int hours_offset = 0;

            int hours = Convert.ToInt32(timeArray[0]);
            int minutes = Convert.ToInt32(timeArray[1]);
            int seconds = Convert.ToInt32(timeArray[2]);

            if (minutes_offset > 59)
            {
                int newMinutes = minutes_offset % 60;

                if (newMinutes != 0)
                {
                    minutes_offset = newMinutes;
                }
                hours_offset = minutes_offset / 60;

            }
            if (minutes + minutes_offset < 10)
            {
                return sb.AppendFormat("{0}:{1}:{2}", hours + hours_offset, "0" + (minutes + minutes_offset), "0" + seconds).ToString();
            }

            sb.AppendFormat("{0}:{1}:{2}", hours + hours_offset, (minutes + minutes_offset), "0" + seconds);

            return sb.ToString();
        }

        public static bool CheckClockMatches(string clock, int offset)
        {
            string[] timeArray = clock.Split(new[] { ':' });            

            int hours = Convert.ToInt32(timeArray[0]);
            int minutes = Convert.ToInt32(timeArray[1]);
            int seconds = Convert.ToInt32(timeArray[2]);

            return minutes == offset;
        
        }

        public static void BuildJourneyMap()
        {
            if (journey_map.Count <= 0)
            {
                journey_map.Add("Lavis", new Map<string, int>().Add("Trento", 6));
                journey_map.Add("Trento", new Map<string, int>().Add("Segantini Dogana", 5));
                journey_map.Add("Segantini Dogana", new Map<string, int>().Add("Travai Al Nuoto", 8));
                journey_map.Add("Travai Al Nuoto", new Map<string, int>().Add("Mezzocorona", 3));
            }
        }

        public static int CalculateJourneyOffset(string origin , string destination)
        {
            Map<string, int> leg = journey_map[origin];
            return leg[destination];
        }

        public static int CalculateJourneyOffset(string station)
        {
            Map<string, int> leg = journey_map[station];
            return leg.ToArray()[0].Value;
        }
        
    }   

    public class Commuter : CompoundValue
    {
        public string name;
        public string origin;
        public string destination;
        public int initialOffset;

        public Commuter(string name, string origin, string destination, int initialOffset)
        {
            this.name = name;
            this.origin = origin;
            this.destination = destination;
            this.initialOffset = initialOffset;
        }
    }

}
