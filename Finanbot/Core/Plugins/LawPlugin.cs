using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.IO;
using Finanbot.Core.Helpers;
using File = System.IO.File;

namespace Finanbot.Core.Plugins
{
    public class LawPlugin : Plugin
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
                return "Законы о защите прав потребителя";
            }
        }
        public override string PluginName
        {
            get
            {
                return "laws";
            }
        }
        public override string UserPluginName
        {
            get
            {
                return "Права потребителя";
            }
        }

        public Dictionary<string, string> Laws = new Dictionary<string, string>();
        public override void Initialize(Session session)
        {
            var lawFile = session.Config["main"]["laws"];

            var file = File.OpenText(lawFile);

            var title = string.Empty;
            var sb = new StringBuilder();
            while(true)
            {
                var line = file.ReadLine();
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;

                var index = line.IndexOf('.');
                if (index == -1)
                {
                    sb.AppendLine(line);
                }
                else
                {
                    var x = line.Substring(0, index);
                    var id = 0;
                    if (int.TryParse(x.Trim(), out id))
                    {
                        if(sb.Length > 0)
                        {
                            Laws[title] = sb.ToString();
                        }
                        title = line;
                        sb.Clear();
                    }
                    else
                    {
                        sb.AppendLine(line);
                    }
                }
            }
            if (sb.Length > 0)
                Laws[title.Trim()] = sb.ToString();

        }


        public override int GetPriority(Message query)
        {
            if (query.Type == MessageType.TextMessage)
            {
                var text = query.Text.ToLower();
                var keywords1 = new string[][]
                {
                    new string[] { "прав", "закон" },
                    new string[] { "потреб", "клиент" }
                };
                var price = 40 * Ext.PriceCalc(text, keywords1);
                return price;
            }
            return 0;
        }
        public override bool Query(Session session, Message query)
        {
            if (query.Type == MessageType.TextMessage)
            {
                var ans = Laws.Select(x => x.Key).ToList();
                ans.Add("Подробнее - /laws");
                var s = string.Join("\r\n", ans);

                SendAnswer(session, s);
                return true;
            }
            return false;
        }
        public override void Help(Session session)
        {
            var ans = Laws.Select(x => x.Key).ToList();
            var count = ans.Count;
            ans.Add("/exit - выход");
            var s = string.Join("\r\n", ans);

            Send(session, s, new ReplyQuadreKeyboard(true, Enumerable.Range(1, count).Select(x => x.ToString()).ToArray()));
            base.Help(session);
        }
        public override bool Command(Session session, Message message)
        {
            if (base.Command(session, message)) return true;

            if (message.Type == MessageType.TextMessage)
            {
                var text = message.Text;
                var id = 0;
                if (int.TryParse(text, out id))
                {
                    var res = Laws.Where(x => x.Key.Trim().StartsWith(id + "."));
                    if (res.Count() > 0)
                    {
                        Send(session, res.First().Value + "\r\n/help - вывести список еще раз\r\n/exit - завершить работу с законами");
                        return true;
                    }
                }
            }
            Help(session);
            return true;
        }
    }
}
