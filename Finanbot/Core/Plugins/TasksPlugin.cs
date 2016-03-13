using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Finanbot.Core.Helpers;
using System.Threading;

namespace Finanbot.Core.Plugins
{
    public class TasksPlugin : Plugin
    {
        public override bool CanRun
        {
            get
            {
                return false;
            }
        }
        public override string Description
        {
            get
            {
                return "Отправляет напоминания через заданный промежуток времени";
            }
        }
        public override string PluginName
        {
            get
            {
                return "tasks";
            }
        }
        public override string UserPluginName
        {
            get
            {
                return "Напоминания";
            }
        }

        public override int GetPriority(Message query)
        {
            if (query.Type == MessageType.TextMessage)
            {
                if (query.Text.ToLower().IndexOf("напомни") != -1) return int.MaxValue;
            }
            return 0;
        }
        public override bool Query(Session session, Message query)
        {
            if (query.Type == MessageType.TextMessage)
            {
                var index = query.Text.ToLower().IndexOf("напомни");
                if (index != -1)
                {
                    index += 7;
                    var q = query.Text.Substring(index).Trim();

                    var time = UserTimeParser.ParseDatetime(q);
                    var now = DateTime.Now;
                    if (now.AddMinutes(1) > time) time = time.AddMinutes(10);

                    if (now < time)
                    {
                        new Thread(() =>
                        {
                            var dt = time - DateTime.Now;
                            var quer = q;
                            Thread.Sleep((int)dt.TotalMilliseconds);
                            SendAnswer(session, quer);

                        }).Start();
                        SendAnswer(session, "Задача добавлена!\r\n" + time + "\r\n" + q);
                    }
                    else
                    {
                        SendAnswer(session, "Задачу надо выполнить немного раньше :/");
                    }
                }
            }
            return true;
        }
    }
}
