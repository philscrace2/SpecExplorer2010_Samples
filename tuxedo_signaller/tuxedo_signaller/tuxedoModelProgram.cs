using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Modeling;

namespace TuxedoTicker
{
    public static class Parameters
    {
        public static SetContainer<Train> trainz;

    }

    public static class tuxedoModelProgram
    {
        //static Map<string, string> train_currentLocation = new Map<string, string>();
        public static int tickCounter = 1;
        public static bool arrived = false;
        public static SetContainer<Train> trains = new SetContainer<Train>();
        public static SetContainer<Train> destination_trains = new SetContainer<Train>();
        public static SequenceContainer<string> train_track = new SequenceContainer<string>() { "A", "B", "C", "D", "E" };
        static bool setup_complete = false;
        public static int current_location_idx = 0;

        public static Set<int> Ticker()
        {
            return new Set<int>(tickCounter);
        }

        public static SetContainer<Train> Trains()
        {
            return new SetContainer<Train>(trains);
        }

        public static SetContainer<Train> DestinationTrains()
        {
            return new SetContainer<Train>(destination_trains);
        }

        [Rule]
        public static void new_train(Train train)
        {
            Condition.IsNotNull(train);
            Condition.IsTrue(!setup_complete);
            trains.Add(train);
            setup_complete = true;
        }

        [Rule]
        public static void move([Domain("Ticker")] int tick)
        {
            Condition.IsTrue(!arrived);
            //Condition.IsNotNull(train);
            Condition.IsTrue(setup_complete);
            if (tickCounter < train_track.Count())
            {
                tickCounter++;
            }
            //MoveTrain(train);
        }

        //[Rule]
        //public static void destination_reached([Domain("Ticker")]int tick, [Domain("DestinationTrains")] Train train)
        //{
        //    Condition.IsTrue(arrived);
        //    Condition.IsNotNull(train);
        //}

        //private static void MoveTrain(Train train)
        //{
        //    if (train.destinationBlock != train.currentLocationBlock)
        //    {
        //        Train new_postioned_train = new Train(train.train_Id, train.startBlock, train.destinationBlock, train_track[tickCounter - 1]);
        //        trains.Remove(train);
        //        trains.Add(new_postioned_train);
        //    }
        //    else
        //    {
        //        trains.Remove(train);
        //        Train finished = new Train(train.train_Id, train.startBlock, train.destinationBlock, train_track[tickCounter - 1]);
        //        destination_trains.Add(finished);
        //        arrived = true;
        //    }
        //}

        //[AcceptingStateCondition]
        //public static bool AcceptingState()
        //{
        //    foreach()
        //}
    }

    public struct Train
    {
        public string train_Id;
        public string startBlock;
        public string destinationBlock;
        public string currentLocationBlock;

        public Train(string trainId, string startBlock, string dest, string currlocation)
        {
            this.train_Id = trainId;
            this.startBlock = startBlock;
            this.destinationBlock = dest;
            this.currentLocationBlock = currlocation;
        }

    }
}

namespace tuxedo_signaller
{
    public static class Parameters
    {
        public static SetContainer<Train> trainz;
    
    }
    
    public static class tuxedoModelProgram
    {
        //static Map<string, string> train_currentLocation = new Map<string, string>();
        public static int tickCounter = 1;
        public static bool arrived = false;
        public static SetContainer<Train> trains = new SetContainer<Train>();
        public static SetContainer<Train> destination_trains = new SetContainer<Train>();
        public static SequenceContainer<string> train_track = new SequenceContainer<string>() {"A", "B", "C", "D", "E"};
        static bool setup_complete = false;
        public static int current_location_idx = 0;

        public static Set<int> Ticker()
        {
            return new Set<int>(tickCounter);
        }

        public static SetContainer<Train> Trains()
        {
            return new SetContainer<Train>(trains);
        }

        public static SetContainer<Train> DestinationTrains()
        {
            return new SetContainer<Train>(destination_trains);
        }

        [Rule]
        public static void new_train(Train train)
        {
            Condition.IsNotNull(train);
            Condition.IsTrue(!setup_complete);
            trains.Add(train);
            setup_complete = true;
        }
        
        [Rule]
        public static void move([Domain("Ticker")]int tick, [Domain("Trains")] Train train)
        {            
            Condition.IsTrue(!arrived);
            Condition.IsNotNull(train);
            Condition.IsTrue(setup_complete);
            if (tickCounter < train_track.Count())
            {
                tickCounter++;
            }
            MoveTrain(train);
        }

        [Rule]
        public static void destination_reached([Domain("Ticker")]int tick, [Domain("DestinationTrains")] Train train)
        {
            Condition.IsTrue(arrived);
            Condition.IsNotNull(train);
        }

        private static void MoveTrain(Train train)
        {
            if (train.destinationBlock != train.currentLocationBlock)
            {
                Train new_postioned_train = new Train(train.train_Id, train.startBlock, train.destinationBlock, train_track[tickCounter - 1]);
                trains.Remove(train);
                trains.Add(new_postioned_train);
            }
            else
            { 
                trains.Remove(train);
                Train finished = new Train(train.train_Id, train.startBlock, train.destinationBlock, train_track[tickCounter - 1]);
                destination_trains.Add(finished);
                arrived = true;
            }
        }

        //[AcceptingStateCondition]
        //public static bool AcceptingState()
        //{
        //    foreach()
        //}
    }

    public struct Train
    {
        public string train_Id;
        public string startBlock;
        public string destinationBlock;
        public string currentLocationBlock;

        public Train(string trainId, string startBlock, string dest, string currlocation)
        {
            this.train_Id = trainId;
            this.startBlock = startBlock;
            this.destinationBlock = dest;
            this.currentLocationBlock = currlocation;
        }
    
    }
}

namespace two_trains
{
    public static class Parameters
    {
        public static SetContainer<Train> trainz;

    }

    public static class tuxedoModelProgram
    {
        //static Map<string, string> train_currentLocation = new Map<string, string>();
        public static int tickCounter = 1;
        public static bool arrived = false;
        public static SetContainer<Train> trains = new SetContainer<Train>();
        public static Sequence<string> train_name = new Sequence<string>();
        public static Map<string, string> start_Block = new Map<string, string>();
        public static Map<string, string> current_Block = new Map<string, string>();
        public static Map<string, string> destination_Block = new Map<string, string>();
        public static SetContainer<string> destination_trains = new SetContainer<string>();
        public static SequenceContainer<string> train_track = new SequenceContainer<string>();
        public static Map<string, SequenceContainer<string>> routes = new Map<string, SequenceContainer<string>>();
        static bool setup_complete = false;
        public static int current_location_idx = 0;
        static int timescalled = 0;
        public static int trainNumber = 0;
        public static int start_idx;
        public static int end_idx;
        public static SequenceContainer<string> actual_route;

        public static Set<int> Ticker()
        {
            return new Set<int>(tickCounter);
        }

        public static SetContainer<Train> Trains()
        {
            return new SetContainer<Train>(trains);
        }

        public static IEnumerable<Sequence<string>> TrainsFirstName()
        {
            yield return new Sequence<string>(train_name);
        }

        public static IEnumerable<Sequence<string>> StartBlock()
        {            
            yield return new Sequence<string>(start_Block.Values.Reverse<string>());
        }

        public static IEnumerable<Sequence<string>> CurrentBlock()
        {
            yield return new Sequence<string>(current_Block.Values.Reverse<string>());
        }

        public static IEnumerable<Sequence<string>> DestinationBlock()
        {
            yield return new Sequence<string>(destination_Block.Values.Reverse<string>());
        }

        //public static IEnumerable<Sequence<byte>> ReadDataDomain()
        //{
        //    yield return new Sequence<byte>(); // we can read from the empty file
        //    foreach (Sequence<byte> data in WriteDataDomain())
        //        yield return data;
        //}

        public static SetContainer<string> DestinationTrains()
        {
            return new SetContainer<string>(destination_trains);
        }

        [Rule]
        public static void new_train(string train, string startBlock, string currentBlock, string destinationBlock)
        {            
            Condition.IsNotNull(train);
            Condition.IsNotNull(startBlock);
            Condition.IsTrue(!setup_complete);            
            train_name = train_name.Add(train);
            start_Block = start_Block.Add(train, startBlock);
            current_Block = current_Block.Add(train, currentBlock);
            destination_Block = destination_Block.Add(train, destinationBlock);
            timescalled++;
            BuildRoutes(train);
            if (timescalled == trainNumber)
            {
                setup_complete = true;
            }
        }

        [Rule]
        public static void move([Domain("Ticker")]int tick, [Domain("TrainsFirstName")] Sequence<string> train, [Domain("StartBlock")] Sequence<string> startBlock, [Domain("CurrentBlock")] Sequence<string> currentBlock, [Domain("DestinationBlock")] Sequence<string> destinationBlock)
        {
            if (tickCounter < train_track.Count())
            {
                tickCounter++;
            }

            foreach (string tr in train)
            {
                Condition.IsTrue(!destination_trains.Contains(tr));
                Condition.IsNotNull(tr);
                Condition.IsTrue(setup_complete);
                
                MoveTrain(tr);            
            }            
            
        }

        [Rule]
        public static void destination_reached([Domain("Ticker")]int tick, [Domain("DestinationTrains")] string train)
        {
            Condition.IsTrue(destination_trains.Contains(train));
            Condition.IsNotNull(train);

            RemoveTrainFromNetwork(train);
            destination_trains.Remove(train);

            if (tickCounter < train_track.Count())
            {
                tickCounter++;
            }

        }

        [AcceptingStateCondition]
        public static bool EnsureCrash()
        {
            foreach (string block_id in current_Block.Values)
            { 
                
            }
        }

        private static void MoveTrain(string train)
        {
            if (IsTrainDestinationReached(train))
            {                
                current_Block = current_Block.Override(train, train_track[tickCounter - 1]);
                destination_trains.Add(train);                                
            }
            else
            {
                SequenceContainer<string> trainRoute = routes[train];
                current_Block = current_Block.Override(train, trainRoute[tickCounter - 1]);                
            }
        }

        private static bool IsTrainDestinationReached(string train)
        {
            return current_Block[train] == destination_Block[train];
        }

        private static void RemoveTrainFromNetwork(string train)
        {
            train_name = train_name.Remove(train);
            start_Block = start_Block.Remove(train);
            current_Block = current_Block.Remove(train);
            destination_Block = destination_Block.Remove(train);            
        }

        private static void BuildRoutes(string train)
        {
            actual_route = new SequenceContainer<string>();
            IEnumerable<string> configured_track = new SequenceContainer<string>();

            bool visited = false;

            if (train_track.Contains(start_Block[train]))
            {                
                start_idx = train_track.IndexOf(start_Block[train]);
            }

            if (train_track.Contains(destination_Block[train]))
            {
                end_idx = train_track.IndexOf(destination_Block[train]);
            }

            if (start_idx > end_idx)
            {
                configured_track = train_track.Reverse<string>();
            }
            else
            {
                configured_track = train_track;
            }


            foreach (string block in configured_track)
            {
                if (block == start_Block[train])
                {                    
                    visited = true;
                }
                else if (block == destination_Block[train])
                {
                    actual_route.Add(block);
                    visited = false;
                }
                if (visited)
                {
                    actual_route.Add(block);
                }
            }

            routes = routes.Add(new KeyValuePair<string, SequenceContainer<string>>(train, actual_route));           
            
        }


        //[AcceptingStateCondition]
        //public static bool AcceptingState()
        //{
        //    foreach()
        //}
    }

    public struct Train
    {
        public string train_Id;
        public string startBlock;
        public string destinationBlock;
        public string currentLocationBlock;

        public Train(string trainId, string startBlock, string dest, string currlocation)
        {
            this.train_Id = trainId;
            this.startBlock = startBlock;
            this.destinationBlock = dest;
            this.currentLocationBlock = currlocation;
        }

    }
}
