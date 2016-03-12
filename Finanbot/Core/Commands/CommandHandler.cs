using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Finanbot.Core.Commands
{
    public class CommandHandler
    {
        public virtual string Help { get { return string.Empty; } }
        public virtual Dictionary<string, CommandHandler> DefaultHandlers { get; private set; }
        public virtual bool Complete { get; set; }

        public CommandHandler()
        {
            DefaultHandlers = new Dictionary<string, CommandHandler>();
        }

        public virtual bool Init(Session session)
        {
            if (string.IsNullOrWhiteSpace(Help)) return false;
            DefaultHandlers["/help"] = new StaticTextCommand(Help);
            DefaultHandlers["/start"] = new StaticTextCommand(Help);
            
            session.Send(Help);
            return false;
        }

        public virtual bool Handle(Session session, Message message)
        {
            if (message.Type == MessageType.TextMessage)
            {
                var text = message.Text;
                CommandHandler handler;
                if (DefaultHandlers.TryGetValue(text.TrimEnd(), out handler))
                {
                    session.Push(handler);
                    return true;
                }
            }
            return false;
        }
    }
}
