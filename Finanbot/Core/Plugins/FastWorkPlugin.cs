using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Finanbot.Core.Helpers;

namespace Finanbot.Core.Plugins
{
    public class FastWorkPlugin : Plugin
    {
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
                return "Поиск быстрой работы поблизости";
            }
        }
        public override string PluginName
        {
            get
            {
                return "fastwork";
            }
        }
        public override string UserPluginName
        {
            get
            {
                return "Быстрая работа";
            }
        }

        public static Dictionary<int, Tuple<string, Location, string>> Tasks = new Dictionary<int, Tuple<string, Location, string>>();

        public override int GetPriority(Message query)
        {
            if (query.Type == MessageType.TextMessage)
            {
                var keywords = new string[][]
                {
                    new string[] { "добав", "удал", "найти", "поиск" },
                    new string[] { "работ", "задач", "поруч" }
                };
                var price = Ext.PriceCalc(query.Text, keywords);

                return price * 30;
            }
            return 0;
        }
        public override void Start(Session session)
        {

            var examples = new string[]
            {
                "Добавить работу принести мне кофе", "Найти работу", "Удалить работу"
            };

            var sb = new StringBuilder();
            sb.AppendLine("Примеры:");
            for (var i = 0; i < examples.Length; i++)
            {
                sb.AppendLine((i + 1).ToString() + ". " + examples[i]);
            }

            Send(session, sb.ToString(), new Finanbot.Core.Helpers.ReplyQuadreKeyboard(true, examples));
            session.StopPlugin(this);
        }
        public override bool Query(Session session, Message query)
        {
            if (query.Type == MessageType.TextMessage)
            {
                var low = query.Text.ToLower();
                var keywords = new string[] { "работ", "задач", "поруч" };
                foreach(var key in keywords)
                {
                    var index = low.IndexOf(key);
                    if (index == -1) continue;

                    if (low.Contains("добав"))
                    {
                        var endindex = query.Text.IndexOf(' ', index);
                        if (endindex == -1)
                        {
                            SendAnswer(session, "Не найдено описание задачи");
                            return true;
                        }
                        if (string.IsNullOrWhiteSpace(query.From.Username))
                        {
                            SendAnswer(session, "Задайте логин в настройках, по которому другие пользователи смогут вас найти");
                            return true;
                        }
                        var task = query.Text.Substring(endindex + 1);
                        Tasks[query.From.Id] = Tuple.Create(task, session.Location, query.From.Username);
                        SendAnswer(session, "Задача успешно добавлена");
                        return true;
                    }
                    else if (low.Contains("удал"))
                    {
                        Tasks.Remove(query.From.Id);
                        SendAnswer(session, "Задача удалена");
                        return true;
                    }
                    else if (low.Contains("найти") || low.Contains("поиск"))
                    {
                        var set = Tasks.Where(x => x.Key != query.From.Id).OrderBy(x => x.Value.Item2.Distance(session.Location)).Take(5);
                        var sb = new StringBuilder();
                        foreach(var s in set)
                        {
                            sb.AppendLine(string.Format("@{0}: {1}", s.Value.Item3, s.Value.Item1));
                        }
                        if (sb.Length > 0)
                        {
                            SendAnswer(session, sb.ToString());
                            return true;
                        }
                        else
                        {
                            SendAnswer(session, "Никакой работы не найдено");
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
