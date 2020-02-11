using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Modeling;

namespace lightswitch
{
    /// <summary>
    /// An example model program.
    /// </summary>
    static class lightswitchModelProgram
    {
        static lightBulbState lightBulb = lightBulbState.off;

        [Rule]
        static void SwitchDown()
        {
            Condition.IsTrue(lightBulb == lightBulbState.off);
            lightBulb = lightBulbState.on;
        }

        [Rule]
        static void SwitchUp()
        {
            Condition.IsTrue(lightBulb == lightBulbState.on);
            lightBulb = lightBulbState.off;
        }
    }

    public enum lightBulbState
    { 
        on,
        off
    }
}
