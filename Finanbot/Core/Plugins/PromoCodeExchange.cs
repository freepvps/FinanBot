using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Finanbot.Core.Plugins
{
    public class PromoCodeExchange : Plugin
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
                return "Обменяйте свой ненужный промокод!";
            }
        }


        public override string PluginName
        {
            get
            {
                return "pmcexchange";
            }
        }
        public override string UserPluginName
        {
            get
            {
                return "Обмен промокодами";
            }
        }

        public override void Start(Session session)
        {
            m_lastShownCode = 0;
            m_lastShop = "";
            var examples = new string[]
            {
                 "Добавить промокод: Озон - JBQPA, скидка 15%", "Найди код для: Амазон"
            };
            var sb = new StringBuilder();
            sb.AppendLine("Примеры:");
            for (var i = 0; i < examples.Length; i++)
            {
                sb.AppendLine((i + 1).ToString() + ". " + examples[i]);
            }

            Send(session, sb.ToString());
            session.StopPlugin(this);
        }
        public static Dictionary<string, List<string>> Codes = new Dictionary<string, List<string>>();

        public override int GetPriority(Message query)
        {
            if (query.Type != MessageType.TextMessage) return 0;
            return 42; //TODO: fix it! // It's normal, lol:D
        }

        public override bool Query(Session session, Message query)
        {
            if (query.Text.Contains("работ")) return false;
            if (query.Text.ToLower().Contains("добав"))
            {
                string Arguments = query.Text.Split(":".ToCharArray())[1].Trim();
                string[] SplitedArguments = Arguments.Split("-".ToCharArray());
                if (SplitedArguments.Length == 2)
                {
                    if (Codes.ContainsKey(SplitedArguments[0].Trim()))
                    {
                        Codes[SplitedArguments[0].Trim()].Add(SplitedArguments[1].Trim());
                    }
                    else
                    {
                        Codes.Add(SplitedArguments[0].Trim(), new List<string> { SplitedArguments[1].Trim() });
                    }
                    SendAnswer(session, "Код добвален в базу.");
                    return true;
                }
            }
            else
            {
                if (query.Text.ToLower().Contains("най"))
                {
                    string Arguments = query.Text.Split(":".ToCharArray())[1].Trim();
                    if (Codes.ContainsKey(Arguments))
                    {
                        m_lastShop = Arguments;
                        List<String> codes = Codes[Arguments];
                        if (m_lastShownCode < Codes.Count)
                        {
                            SendAnswer(session, "Для магазина " + Arguments + " найден промокод: " + codes[m_lastShownCode], new Finanbot.Core.Helpers.ReplyLinesKeyboard(true, "Использовать этот промокод", "Найти другой промокод для:" + Arguments));
                            m_lastShownCode++;
                            return true;
                        }

                    }
                    SendAnswer(session, "К сожалению, промокодов для магазина " + Arguments + " добавлено не было");
                    return true;
                }
            }
            if (query.Text.ToLower() == "использовать этот промокод")
            {

                if (Codes.ContainsKey(m_lastShop) && Codes[m_lastShop].Count <= m_lastShownCode) Codes[m_lastShop].RemoveAt(m_lastShownCode);
                SendAnswer(session, "Поздравляем, промокод теперь ваш.");
                return true;
            }
            return false;
        }
        private int m_lastShownCode;
        private string m_lastShop;
    }
}