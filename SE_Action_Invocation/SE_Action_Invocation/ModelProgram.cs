using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Modeling;

namespace SE_Action_Invocation
{
    /// <summary>
    /// An example model program.
    /// </summary>
    static class ModelProgram
    {
        [Rule(Action = "GetQualifiedMinimum((Set<int>)null) / -2")]
        static void GetQualifiedMinimumNull() { }

        [Rule(Action = "GetQualifiedMinimum(Set<int>{}) / 0")]
        static void GetQualifiedMinimumEmpty() { }

        [Rule(Action = "GetQualifiedMinimum(numbers) / -1")]
        static void GetQualifiedMinimumError(Set<int> numbers)
        {
            Condition.IsNotNull(numbers);
            Condition.Exists(numbers, n => n <= 0);
        }

        [Rule(Action = "GetQualifiedMinimum(numbers) / result")]
        static int GetQualifiedMinimum(Set<int> numbers)
        {
            Condition.IsNotNull(numbers);
            Condition.IsTrue(numbers.Count > 0);
            Condition.All(numbers, n => n > 0);            
            return numbers.Min();
        }

    }
}
