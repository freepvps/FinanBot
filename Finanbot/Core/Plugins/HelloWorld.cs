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
        public override PluginCommandHandler Root
        {
            get
            {
                return new HelloWolrdCommand();
            }
        }
        public override string Description
        {
            get
            {
                return "Тестовый плагин";
            }
        }

        public override int Query(Session session, Message query, StringBuilder ans)
        {
            if (query.Type == MessageType.TextMessage)
            {
                ans.AppendLine("Your query: " + query.Text);
                return 1;
            }
            else { return 0; }
        }
    }
}
