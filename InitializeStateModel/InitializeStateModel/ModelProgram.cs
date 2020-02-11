using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Modeling;

namespace InitializeStateModel
{
    /// <summary>
    /// An example model program.
    /// </summary>
    public static class ModelProgram
    {
        public static int setThis;
        public static int setThisAswell;
        public static bool setThisBool = false;

        public static SetContainer<int> Value()
        {
            return new SetContainer<int>(setThis);
        }

        [Rule]
        public static void FirstStep([Domain("Value")] int v)
        {
            Condition.IsTrue(!setThisBool);
            setThisBool = true;
        
        }

        [Rule]
        public static void SecondStep()
        {
            Condition.IsTrue(setThisBool);
            setThisBool = false;
        }
        

    }
}
