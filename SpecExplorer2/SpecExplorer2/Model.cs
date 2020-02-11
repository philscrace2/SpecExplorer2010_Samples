using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Modeling;

namespace SpecExplorer2
{

    /// <summary>
    /// An example model program.
    /// </summary>
    static class ModelProgram
    {
        public enum travel_state { start, depart, arrive, end, finish };

        static MapContainer<string, travel_state> commuter_currentstatus = new MapContainer<string, travel_state>();

        static MapContainer<string, string> commuter_to_currentStation = new MapContainer<string, string>();

        static SequenceContainer<string> bobs_route = new SequenceContainer<string>() { "Lavis", "Trento", "Segantini Dogana", "Travai Al Nuoto", "Mezzocorona" };

        static int idx_for_bob;

        static string current_station;

        static int bobs_count;

        static int idx_count;

        [Rule]
        static void start(string name)
        {
            current_station = name;
            Requires(!commuter_currentstatus.Keys.Contains(name));
            commuter_currentstatus.Add(name, travel_state.depart);
            commuter_to_currentStation.Add(name, bobs_route[0]);
        }

        static void Requires(bool condition)
        {
            Condition.IsTrue(condition);
        }
    }
}
