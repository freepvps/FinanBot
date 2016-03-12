using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanbot.Core.Helpers
{
    public class PeriodicCaller
    {
        public DateTime LastUpdate { get; private set; }
        public int Period { get; set; }
        public PeriodicCaller(int period)
        {
            Period = period;
        }
        public void Call(Action action)
        {
            var dt = DateTime.Now - LastUpdate;
            if (dt.TotalSeconds >= Period)
            {
                LastUpdate = DateTime.Now;
                action();
            }
        }
    }
}
