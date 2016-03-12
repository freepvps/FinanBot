using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Finanbot.Core.Commands
{
    public class StaticTextCommand : CommandHandler
    {
        public virtual string Text { get; set; }
        public override string Help
        {
            get
            {
                return Text;
            }
        }

        public override bool Init(Session session)
        {
            session.Send(Text);
            return true;
        }
        public override bool Handle(Session session, Message message)
        {
            session.Send(Text);
            return true;
        }

        public StaticTextCommand(string text = "")
        {
            Text = text;
        }
    }
}
