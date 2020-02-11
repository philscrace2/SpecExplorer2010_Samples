using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Modeling;

namespace calculator
{
    /// <summary>
    /// An example model program.
    /// </summary>
    static class ModelProgram
    {
        public enum mode { not_started, standard, scientific, programmer };

        public static mode calc_state = mode.not_started;
                       
        [Rule]
        static void start_in_standard()
        {
            Condition.IsTrue(calc_state == mode.not_started);
            calc_state = mode.standard;
        }

        [Rule]
        static void close_from_standard()
        {
            Condition.IsTrue(calc_state == mode.standard);
            calc_state = mode.not_started;
        }

        [Rule]
        static void navigate_to_scientific()
        {
            Condition.IsTrue(calc_state == mode.standard);
            calc_state = mode.scientific;
        }

        [Rule]
        static void navigate_to_programmer()
        {
            Condition.IsTrue(calc_state == mode.standard);
            calc_state = mode.programmer;
        }

    }
}
