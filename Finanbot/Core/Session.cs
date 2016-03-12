using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using NLog;
using Finanbot.Core.Commands;
using Finanbot.Core.Plugins;

namespace Finanbot.Core
{
    public class Session
    {
        public CommandHandler Root { get; private set; }
        public Stack<CommandHandler> Handlers { get; private set; }
        public Dictionary<string, Plugin> Plugins { get; private set; }

        public Logger Log = LogManager.GetLogger("session");


        public bool HandlerIsRoot
        {
            get
            {
                return Handlers.Count == 0;
            }
        }
        public bool HandlerIs(CommandHandler handler)
        {
            if (handler == Root)
            {
                return Handlers.Count == 0;
            }
            else
            {
                return Handlers.Peek() == handler;
            }
        }
        public CommandHandler CurrentHandler
        {
            get
            {
                return Handlers.Count == 0 ? Root : Handlers.Peek();
            }
        }
            

        public Api Api { get; private set; }
        public long ChatId { get; private set; }

        public Session()
        {
            Handlers = new Stack<CommandHandler>();
            Plugins = new Dictionary<string, Plugin>();
            foreach(var plugin in PluginManager.GetPlugins())
            {
                Plugins["/" + plugin.PluginName] = plugin;
            }

            Root = new PluginManagerCommand();
        }

        public void Push(CommandHandler handler)
        {
            try
            {
                var handled = handler.Init(this);
                if (!handled)
                {
                    handler.Complete = false;
                    Handlers.Push(handler);
                }
            }
            catch (Exception ex)
            {
                Log.Error("InitError chatid = {0}, handler = {1}", ChatId, handler);
                Log.Error(ex);

                throw;
            }
        }
        public void Flush()
        {
            while (Handlers.Count > 0 && Handlers.Peek().Complete)
            {
                Handlers.Pop();
            }
        }
        public void Complete(CommandHandler handler)
        {
            handler.Complete = true;
            Flush();
        }
        public void CompletePlugin(Plugin plugin)
        {
            while(Handlers.Count > 0)
            {
                var peek = Handlers.Peek();
                if (peek is PluginCommandHandler && ((PluginCommandHandler)peek).Plugin == plugin)
                {
                    Complete(peek);
                }
            }
        }

        public void Send(string chatMessage, ReplyMarkup replyMarkup = null)
        {
            try
            {
                Api.SendTextMessage(ChatId, chatMessage, replyMarkup: replyMarkup);
            }
            catch (Exception ex)
            {
                Log.Error("SendError chatId = {0}, message = {1}", ChatId, chatMessage);
                Log.Error(ex);

                throw;
            }
        }

        public void Process(Api api, Message message)
        {
            Flush();
            Api = api;
            ChatId = message.Chat.Id;
            var handler = CurrentHandler;
            handler.Handle(this, message);
        }
    }
}
