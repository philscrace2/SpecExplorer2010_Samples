using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServer.Sample
{
    /// <summary>
    /// A dummy implementation that doesn't conform to the model (testing should fail).
    /// </summary>
    public class Accumulator
    {
        public static void Add(int i)
        {
        }

        public static int ReadAndReset()
        {
            return 4;
        }
    }
}
