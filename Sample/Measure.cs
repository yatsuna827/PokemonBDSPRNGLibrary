using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sample
{
    class ExecutionTimer
    {
        public static long Measure(Action execution, int times = 10, bool div = true)
        {
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < times; i++)
                execution();
            sw.Stop();
            if (div)
                return sw.ElapsedMilliseconds / times;
            else
                return sw.ElapsedMilliseconds;
        }
    }
}
