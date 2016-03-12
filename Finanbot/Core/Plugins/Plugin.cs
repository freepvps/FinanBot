using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using NLog;
using IniParser.Model;

namespace Finanbot.Core.Plugins
{
    public delegate void CommandHandler(Session session, Message message);
    public abstract class Plugin
    {
        public virtual Logger Log { get; protected set; }
        public abstract string PluginName { get; }
        public abstract string UserPluginName { get; }
        public abstract string Description { get; }
        public virtual string ConfigBlock
        {
            get
            {
                return "main";
            }
        }
        public abstract bool CanRun { get; }

        protected string State { get; set; }
        protected readonly Dictionary<string, CommandHandler> Handlers = new Dictionary<string, CommandHandler>();
        protected readonly Dictionary<string, CommandHandler> Helpers = new Dictionary<string, CommandHandler>();
        
        public Plugin()
        {
            Log = LogManager.GetLogger(PluginName);
            State = string.Empty;
        }

        public virtual void LoadDatabase(KeyDataCollection section)
        {

        }
        public virtual void SaveDatabase(KeyDataCollection section)
        {

        }

        public virtual void SendAnswer(Session session, string text, ReplyMarkup replyMarkup = null)
        { 
            text = string.Format("{0}:\r\n{1}", UserPluginName, text);
            Send(session, text, replyMarkup);
        }
        public virtual void Send(Session session, string text, ReplyMarkup replyMarkup = null)
        {
            session.Send(text, replyMarkup);
        }

        public virtual void Initialize(Session session)
        {

        }

        public virtual void Command(Session session, Message message)
        {
            if (message.Type == MessageType.TextMessage && message.Text == "/help")
            {
                Help(session);
            }

            CommandHandler handler;
            if (Handlers.TryGetValue(State, out handler))
            {
                handler(session, message);
                return;
            }
        }
        public virtual void Help(Session session)
        {
            CommandHandler handler;
            if (Helpers.TryGetValue(State, out handler))
            {
                handler(session, null);
            }
        }
        public virtual void Start(Session session)
        {
            Help(session);
            State = string.Empty;
        }
        public virtual void Stop(Session session)
        {
            State = string.Empty;
        }
        public virtual int GetPriority(Message query)
        {
            return 0;
        }
        public virtual bool Query(Session session, Message query)
        {
            return false;
        }
        public virtual void Pulse(Session session)
        {

        }
    }
}
