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
        public virtual string Help { get { throw new NotImplementedException(); } }
        public virtual Dictionary<string, CommandHandler> DefaultHandlers { get; private set; }
        public virtual bool Complete { get; set; }

        public CommandHandler()
        {
            DefaultHandlers = new Dictionary<string, CommandHandler>();
        }

        public virtual bool Init(Session session)
        {
            DefaultHandlers["/help"] = new StaticTextHandler(Help);
            DefaultHandlers["/start"] = new StaticTextHandler(Help);
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
