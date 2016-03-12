using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using IniParser;
using NLog;

namespace Finanbot
{
    public class Program
    {
        public static Logger Log = LogManager.GetLogger("main");
        public static Api Api;
        static void Main(string[] args)
        {
            Log.Trace("Started");
        }
    }
}
