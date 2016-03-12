using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Finanbot.Core.Commands;
using NLog;

namespace Finanbot.Core.Plugins
{
    public abstract class Plugin
    {
        public virtual Logger Log { get; protected set; }
        public virtual string PluginName
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public virtual string UserPluginName
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public virtual string ConfigBlock
        {
            get
            {
                return "main";
            }
        }
        public virtual PluginCommandHandler Root
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public Plugin()
        {
            Log = LogManager.GetLogger(PluginName);
        }

        public virtual Session Session { get; set; }

        public virtual void Send(string text, ReplyMarkup replyMarkup = null)
        {
            text = string.Format("{0}:\r\n{1}", UserPluginName, text);
            Session.Send(text);
        }
        public virtual void Push(PluginCommandHandler handler)
        {
            handler.Plugin = this;
            Session.Push(handler);
        }
        public virtual void Complete(PluginCommandHandler handler)
        {
            Session.Complete(handler);
        }
        public virtual void CompletePlugin()
        {
            Session.CompletePlugin(this);
        }

        public virtual void Command(Session session, Message message)
        {
            var handler = session.CurrentHandler;
            handler.Handle(session, message);
        }
        public virtual int Query(Session session, string query)
        {
            return 0;
        }
        public void Pulse(Session session)
        {

        }
    }
}
