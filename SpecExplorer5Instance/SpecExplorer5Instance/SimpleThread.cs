using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Modeling;

namespace SpecExplorer5Instance
{
    /// <summary>
    /// A model class, bound to an sample class.
    /// </summary>
    [TypeBinding("SpecExplorer5Instance.Sample.ComplexThread")]
    class SimpleThread
    {
        /// <summary>
        /// Sole variable for a thread model instance.
        /// </summary>
        bool hasStepped = false;

        /// <summary>
        /// The parameter-less constructor is considered a model action.
        /// </summary>
        [Rule(Action = "new this.ComplexThread()")]
        SimpleThread()
        { }

        /// <summary>
        /// Method PerformStep contains the rule for a thread to perform a step.
        /// Threads can perform a single step, which is controlled by instance variable hasStepped.
        /// </summary>
        [Rule(Action = "this.Step()")]
        void PerformStep()
        {
            Condition.IsTrue(!hasStepped);
            hasStepped = true;
        }

    }
}
