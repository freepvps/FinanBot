using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finanbot.Core.Helpers;
using Telegram.Bot.Types;

namespace Finanbot.Core.Plugins
{
    public class PulsePlugin : Plugin
    {
        public const int MinimumTime = 60;

        public int PulseTime { get; set; }

        public PulsePlugin()
        {
            PulseTime = MinimumTime;
        }
        public override bool CanRun
        {
            get
            {
                return true;
            }
        }
        public override string Description
        {
            get
            {
                return "Настройка частоты обновлений";
            }
        }
        public override string PluginName
        {
            get
            {
                return "pulse";
            }
        }
        public override string UserPluginName
        {
            get
            {
                return "Pulse";
            }
        }

        public DateTime LastPulse { get; set; }
        public void SafePulse(Session session, bool force)
        {
            var time = DateTime.Now;
            var dt = time - LastPulse;
            if (force || dt.TotalSeconds > PulseTime)
            {
                LastPulse = DateTime.Now;
                foreach(var plugin in session.Plugins)
                {
                    plugin.Value.Pulse(session);
                }
            }
        }

        public override int GetPriority(Message query)
        {
            if (query.Type == MessageType.TextMessage)
            {
                var keywords = new string[][]
                {
                    new string[] { "частот", "период", "периуд" },
                    new string[] { "обновл", "пульс", "уведомл" }
                };
                var price = Ext.PriceCalc(query.Text, keywords);
                return price * 50;
            }
            else
            {
                return 0;
            }
        }
        public override bool Query(Session session, Message query)
        {
            SendAnswer(session, "Уведомления обновляются каждые " + UserTimeParser.SecondsToString(PulseTime));
            return true;
        }

        public override void Help(Session session)
        {
            Send(session, "Как часто вы хотите получать обновления? (выберите из списка или укажите вручную)", new ReplyQuadreKeyboard(true, "1 день", "12 часов", "4 часа", "1 час"));
        }
        public override void Command(Session session, Message message)
        {
            if (message.Type == MessageType.TextMessage)
            {
                var time = UserTimeParser.ParseTime(message.Text, 60);
                var nt = Math.Max(time, MinimumTime);
                if (time < MinimumTime)
                {
                    Send(session, "Указанная вами частота обновления слишком маленькая:( Минимальная частота - " + UserTimeParser.SecondsToString(MinimumTime));
                    Help(session);
                    return;
                }
                else
                {
                    PulseTime = nt;
                    Send(session, "Обновления будут приходить каждые " + UserTimeParser.SecondsToString(nt));
                    session.StopPlugin(this);
                    return;
                }
            }
            else
            {
                Help(session);
            }
        }
    }
}
