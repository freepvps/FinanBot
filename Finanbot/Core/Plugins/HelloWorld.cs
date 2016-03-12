using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finanbot.Core.Commands;
using Finanbot.Core.Helpers;
using Telegram.Bot.Types;

namespace Finanbot.Core.Plugins
{
    public class HelloWorld : Plugin
    {
        public override string PluginName
        {
            get
            {
                return "helloworld";
            }
        }
        public override string UserPluginName
        {
            get
            {
                return "Hello, World!";
            }
        }

        public override int Query(Session session, string query, StringBuilder ans)
        {
            Send("Your query: " + query);
            return 1;
        }
    }
}
