using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Modeling;

namespace parallelTimeTraveller
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

        static string current_clock = String.Empty;

        static int offset;

        static int newTimeOffset;

        static int newTimeOffset2;

        static int arrivalSetCount;

        static SetContainer<int> list;

        static Set<string> setOfStrings;

        static IEnumerable<MapContainer<string, int>> mapclock;

        static List<MapContainer<string, int>> listclock;

        static Set<string> setclock;

        static string currentclock;        

        public static MapContainer<string, Map<string, int>> journey_map = new MapContainer<string, Map<string, int>>();       

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
            if (arrivalSet.Count > 0)
            {
                mapclock = arrivalSet.Values;
                listclock = mapclock.ToList<MapContainer<string, int>>();
                setclock = new Set<string>();

                foreach (var e in listclock)
                {
                    foreach (var d in e)
                    {
                        setclock = setclock.Add(AddMinutesToClock(d.Key, d.Value));
                    }
                }
                current_clock = setclock.ToArray()[0];
                return setclock;
            
            }

            return new Set<string>(currentclock);
        }        

        public static Set<string> Clocks()
        {
            if (arrivalSet.Count < 1)
            {
                if (firstTimeCommuter.Contains(current_commuter))
                {
                    firstTimeCommuter = firstTimeCommuter.Remove(current_commuter);
                    newTimeOffset = GetMinInt(commuters_time);
                    current_clock = AddMinutesToClock(current_clock, newTimeOffset);

                }
            }

            return new Set<string>(current_clock.ToString());
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
            return new SetContainer<Commuter>(departureSet.Keys);
        }

        public static SetContainer<Commuter> Arrivals()
        {
            return new SetContainer<Commuter>(arrivalSet.Keys);
        }

        public static Set<string> Completed()
        {
            return new Set<string>(routeCompleteCommuter);
        }

        public static MapContainer<string, int> commuters_time = new MapContainer<string, int>();

        public static MapContainer<Commuter, MapContainer<string, int>> departureSet = new MapContainer<Commuter, MapContainer<string, int>>();

        public static MapContainer<Commuter, MapContainer<string, int>> arrivalSet = new MapContainer<Commuter, MapContainer<string, int>>();

        [Rule]
        public static void start(parallelTimeTraveller.Commuter commuter, string start_time)
        {            
            Condition.IsTrue(!travellers_today.Contains(commuter));
            Condition.IsTrue(NoArrivalsNorDepartures(commuter));
            Condition.IsTrue(!routeCompleteCommuter.Contains(commuter.name));

            BuildJourneyMap();
            MapContainer<string, int> map = new MapContainer<string, int>();

            if (current_clock == String.Empty)
            {
                current_clock = start_time;
            }

            if (travellers_today.Count == 0)
            {
                map.Add(current_clock, commuter.initialOffset);
            }
            else
            {
                map.Add(current_clock, commuter.initialOffset);
            }

            departureSet.Add(commuter, map);

            commuter_to_currentStation[commuter.name] = commuter.origin;

            firstTimeCommuter = firstTimeCommuter.Add(commuter.name);

            current_commuter = commuter.name;

            travellers_today.Add(commuter);
        }

        [Rule]
        public static void travel_depart([Domain("Clocks")] string clock, [Domain("Departures")] Commuter commuter, string station)
        {
            Condition.IsTrue(travellers_today.Contains(commuter));
            Condition.IsTrue(commuter_to_currentStation[commuter.name] == station);

            departureSet.Remove(commuter);

            firstTimeCommuter.Remove(commuter.name);

            commuters_time.Remove(commuter.name);

            offset = CalculateJourneyOffset(commuter_to_currentStation[commuter.name]);
            KeyValuePair<string, int> k = new KeyValuePair<string, int>(clock, offset);
            MapContainer<string, int> m = new MapContainer<string, int>();
            m.Add(k);

            arrivalSet.Add(commuter, m);

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

                KeyValuePair<string, int> k = new KeyValuePair<string, int>(clock, 0);
                MapContainer<string, int> m = new MapContainer<string, int>();
                m.Add(k);

                departureSet.Add(commuter, m);
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
            return (!arrivalSet.Keys.Contains(user) && !departureSet.Keys.Contains(user));
        }        

        [Probe]
        public static MapContainer<string, int> CommutersProbe()
        {
            return commuters_time;
        }

        [Probe]
        public static SetContainer<Commuter> CommuterProbe()
        {
            return new SetContainer<Commuter>(departureSet.Keys);
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

        public static int CalculateJourneyOffset(string origin, string destination)
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

    
    public struct Commuter
    {
        public string name;
        public string origin;
        public string destination;
        public int initialOffset;        
    }        
}
