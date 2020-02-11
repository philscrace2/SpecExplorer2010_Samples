using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructComparison
{
    class Program
    {
        static List<Commuter> l;
        static bool result;

        static void Main(string[] args)
        {
            l = new List<Commuter>();

            Commuter c = new Commuter("alice", "Lavis", "Mezzacorona");
            l.Add(c);

            result = !IsLastStation("alice", "Mezzacorona");

            Console.WriteLine(result);
            Console.Read();

        }

        public static bool IsLastStation(string commuter, string station)
        {
            foreach (Commuter c in l)
            {
                if (c.name == commuter && c.destination == station)
                    return true;

            }
            return false;
        }
    }

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
