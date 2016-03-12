using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Finanbot.Core.Plugins;

namespace Finanbot.Core.Commands
{
    public class PluginCommandHandler : CommandHandler 
    {
        public virtual Plugin Plugin { get; set; }
        
        public override bool Handle(Session session, Message message)
        {
            if (message.Type == MessageType.TextMessage)
            {
                var text = message.Text;
                CommandHandler handler;
                if (DefaultHandlers.TryGetValue(text.TrimEnd(), out handler))
                {
                    if (handler is PluginCommandHandler)
                    {
                        Plugin.Push((PluginCommandHandler)handler);
                    }
                    else
                    {
                        session.Push(handler);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
