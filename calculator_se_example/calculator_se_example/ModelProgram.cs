using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Modeling;

[assembly: ModelingAssembly]
namespace calculator_se_example
{
    /// <summary>
    /// An example model program.
    /// </summary>
    public static class ModelProgram
    {
        /// <summary>
        /// An example model program.
        /// </summary>
        static bool hello = false;

        [Feature("RunningMode")]
        public static class RunningMode
        {
            // State variable
            public static bool running;

            // Actions
            [Rule(Action = "SwitchRunning()")]
            public static void SwitchRunning()
            {
                running = !running;
            }
        }

        [Feature("Programmer")]
        public static class ProgrammerMode
        {
            static bool programmer;

            // Actions
            [Rule(Action = "SwitchMode()")]
            public static void SwitchMode()
            {
                Condition.IsTrue(RunningMode.running);
                programmer = !programmer;
            }
        
        }

        [Feature("ScientificMode")]
        public static class ScientificMode
        {
            static bool scientific;

            [Rule(Action = "SwitchMode()")]
            public static void SwitchMode()
            {
                Condition.IsTrue(RunningMode.running);
                // action enabled only when running is true
                scientific = !scientific;
            }
        }

        //[Feature("UpAndRunning")]
        //public static class HelloAndRunning
        //{
        //    [Rule]
        //    public static void DoSomething()
        //    {
        //        Condition.IsTrue(hello);

        //    }
        //}
    }
}
