using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public override string Description
        {
            get
            {
                return "Тестовый плагин";
            }
        }
        public override bool CanRun
        {
            get
            {
                return false;
            }
        }

        public override int GetPriority(Message query)
        {
            if (query.Type != MessageType.TextMessage)
            {
                return 0;
            }
            else { return 1; }
        }
        public override bool Query(Session session, Message query)
        {
            if (query.Type == MessageType.TextMessage)
            {
                SendAnswer(session, "Your query: " + query.Text);
                return true;
            }
            else { return false; }
        }
        public override void Pulse(Session session)
        {
            Send(session, "Hello, World pulse!");
        }
    }
}
