using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Controlers.Interface;

namespace WindowsFormsApp1.Helpers
{
    public class StopWatch : IStopWatch
    {
        private Stopwatch watch;
        public TimeSpan Elapsed => watch.Elapsed;

        public void Start()
        {
            watch = new Stopwatch();
            watch.Start();
        }

        public void Stop()
        {
            watch.Stop();
        }
    }
}
