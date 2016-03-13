using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finanbot.Core.Helpers;
using IniParser.Model;
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
                return "Настройка частоты уведомлений";
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
                return "Уведомления";
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
        public override void LoadDatabase(KeyDataCollection section)
        {
            var interval = int.Parse(section["interval"].Safe("14400"));
            PulseTime = interval;
        }
        public override void SaveDatabase(KeyDataCollection section)
        {
            section["interval"] = PulseTime.ToString();
        }

        public override int GetPriority(Message query)
        {
            if (query.Type == MessageType.TextMessage)
            {
                var keywords1 = new string[][]
                {
                    new string[] { "частот", "период", "периуд", "время" },
                    new string[] { "обновл", "пульс", "уведомл" }
                };
                var price1 = Ext.PriceCalc(query.Text, keywords1);
                var keywords2 = new string[][]
                {
                    new string[] { "обновл", "пульс", "уведомл" },
                    new string[] { "раз в", "кажды" }
                };
                var price2 = Ext.PriceCalc(query.Text, keywords2);
                if (price2 == 2) return int.MaxValue;

                return price1 * 50;
            }
            else
            {
                return 0;
            }
        }
        public override bool Query(Session session, Message query)
        {
            if (query.Type == MessageType.TextMessage)
            {
                var keywords = new string[][]
                {
                    new string[] { "обновл", "пульс", "уведомл" },
                    new string[] { "раз в", "кажды" }
                };
                var price = Ext.PriceCalc(query.Text, keywords);
                if (price == 2)
                {
                    SetTime(session, query.Text);
                    return true;
                }

                SendAnswer(session, "Уведомления обновляются каждые " + UserTimeParser.SecondsToString(PulseTime));
                return true;
            }
            return false;
        }

        public override void Help(Session session)
        {
            Send(session, "Как часто вы хотите получать уведомления? (выберите из списка или укажите вручную). \r\nСейчас: " + UserTimeParser.SecondsToString(PulseTime), new ReplyQuadreKeyboard(true, "1 день", "12 часов", "4 часа", "1 час"));
        }
        private bool SetTime(Session session, string text)
        {

            var time = UserTimeParser.ParseTime(text, 60);
            var nt = Math.Max(time, MinimumTime);
            if (time < MinimumTime)
            {
                Send(session, "Указанная вами частота уведомлений слишком маленькая:( Минимальная частота - " + UserTimeParser.SecondsToString(MinimumTime));
                return false;
            }
            else
            {
                PulseTime = nt;
                Send(session, "Уведомления будут приходить каждые " + UserTimeParser.SecondsToString(nt), new ReplyKeyboardHide() { HideKeyboard = true });
                return true;
            }
        }
        public override bool Command(Session session, Message message)
        {
            if (message.Type == MessageType.TextMessage)
            {
                if (SetTime(session, message.Text))
                {
                    session.StopPlugin(this);
                }
                else
                {
                    Help(session);
                }
            }
            else
            {
                Help(session);
            }
            return true;
        }
    }
}
