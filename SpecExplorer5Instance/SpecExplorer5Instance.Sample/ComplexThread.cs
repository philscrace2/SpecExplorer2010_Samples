using System;
using System.Collections.Generic;
using System.Text;

namespace SpecExplorer5Instance.Sample
{
    /// <summary>
    /// The implementation allows a thread to perform more than one step.
    /// Tests should still pass, because the model's behavior is a subset of the implementation's.
    /// </summary>
    public class ComplexThread
    {
        int stepCount = 0;

        public ComplexThread()
        { }

        public void Step()
        {
            stepCount++;
        }
    }
}
