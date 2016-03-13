using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finanbot.Apis;
using Finanbot.Core.Helpers;
using IniParser.Model;
using Telegram.Bot.Types;

namespace Finanbot.Core.Plugins
{
    public class ExchangePlugin : Plugin
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
                return "Информация о курсе валют";
            }
        }
        public override string PluginName
        {
            get
            {
                return "exchange";
            }
        }
        public override string UserPluginName
        {
            get
            {
                return "Курс валют";
            }
        }
        private static Dictionary<string, CurrencyRate> Rates = new Dictionary<string, CurrencyRate>();
        private static PeriodicCaller UpdateCaller = new PeriodicCaller(0);
        private void Update()
        {
            try
            {
                UpdateCaller.Call(() =>
                {
                    var newRates = CurrencyRates.GetExchangeRates();
                    var rates = new Dictionary<string, CurrencyRate>();
                    foreach (var rate in newRates) rates[rate.CharCode] = rate;

                    Rates = rates;
                });
                Log.Trace("Exchange updated");
            }
            catch (Exception ex)
            {
                Log.Error("Exchange update error");
                Log.Error(ex);
            }
        }
        private Dictionary<string, int> InterestedRates = new Dictionary<string, int>();

        public override void Initialize(Session session)
        {
            UpdateCaller.Period = int.Parse(session.Config["main"]["exchange_interval"].Safe("1800"));
            Update();
        }
        public override void LoadDatabase(KeyDataCollection section)
        {
            lock (InterestedRates)
            {
                var interests = section["exchange_interests"].Safe().Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach(var interest in interests)
                {
                    var args = interest.Split('=');
                    if (args.Length >= 2)
                    {
                        InterestedRates[args[0]] = int.Parse(args[1]);
                    }
                }
            }
        }
        public override void SaveDatabase(KeyDataCollection section)
        {
            lock (InterestedRates)
            {
                section["exchange_interests"] = string.Join(";", InterestedRates.Select(x => x.Key + "=" + x.Value));
            }
        }

        public override int GetPriority(Message query)
        {
            if (query.Type == MessageType.TextMessage)
            {
                var msg = query.Text;
                var res =  SearchRates(msg).Count() * 100;
                if (res >= 100)
                {
                    if (query.Text.Contains("удали") || query.Text.Contains("не"))
                    {
                        Log.Trace("Priority return int.Max");
                        return int.MaxValue;
                    }
                }
                return res;
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
                var delete = query.Text.Contains("удали") || query.Text.Contains("не");

                var sb = new StringBuilder();
                var anscc = 0;
                lock (InterestedRates)
                {
                    if (!delete)
                    {
                        foreach (var rate in SearchRates(query.Text))
                        {
                            var rateValue = GetRate(rate);
                            if (rateValue > 0)
                            {
                                var cc = 0;
                                InterestedRates.TryGetValue(rate, out cc);
                                InterestedRates[rate] = cc + 1;
                                sb.AppendLine(string.Format("{0} = {1}", FormatRateCmd(rate), rateValue));
                                anscc++;
                            }
                        }
                    }
                    else
                    {
                        foreach (var rate in SearchRates(query.Text))
                        {
                            InterestedRates.Remove(rate);
                            sb.AppendLine(string.Format("Информация о {0} больше не будет приходить", FormatRateCmd(rate)));
                            anscc++;
                        }
                    }
                }
                if (anscc> 0)
                {
                    SendAnswer(session, sb.ToString());
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }
        public override void Pulse(Session session)
        {
            lock (InterestedRates)
            {
                var interests = InterestedRates.Where(x => x.Value >= 2).OrderBy(x => x.Value).Reverse();
                var sb = new StringBuilder();
                var cc = 0;
                foreach (var interest in interests.Take(5))
                {
                    var key = interest.Key.ToUpper();
                    var rate = GetRate(key);
                    if (rate > 0)
                    {
                        sb.AppendLine(string.Format("{0} = {1}", FormatRateCmd(interest.Key), rate));
                        cc++;
                    }
                }
                if (cc > 0)
                {
                    SendAnswer(session, sb.ToString());
                }
            }
        }
        public override void Start(Session session)
        {
            var examples = new string[]
            {
                "Евро к рублю и доллар к рублю", "JPY/RUB", "eur", "Курс гривны"
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
        public static IEnumerable<string> SearchRates(string row)
        {
            return SearchRates(row.Split(" ,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
        }
        private static Dictionary<string, string> KnownTypes = new Dictionary<string, string>
        {
            { "доллар", "USD" },
            { "долар", "USD" },
            { "руб", "RUB" },
            { "евро", "EUR" },
            { "гривн", "UAH" },
            { "гривен", "UAH" },
            { "стерлинг", "GBP" },
            { "иен", "JPY" },
            { "йен", "JPY" }
        };
        public static IEnumerable<string> SearchRates(string[] words)
        {
            for (var i = 0; i < words.Length; i++)
            {
                words[i] = words[i].ToLower();
                foreach (var repl in KnownTypes)
                {
                    if (words[i].Contains(repl.Key))
                    {
                        words[i] = repl.Value;
                        break;
                    }
                }
            }
            for (var i = 1; i < words.Length - 1; i++)
            {
                if (words[i].Trim() == "/" || words[i] == "к")
                {
                    var row = words[i - 1] + "/" + words[i + 1];
                    if (GetRate(row) > 0)
                    {
                        words[i - 1] = string.Empty;
                        yield return FormatRateCmd(row.ToUpper());
                        words[i + 1] = string.Empty;
                    }


                }
            }
            foreach(var word in words.Where(x => GetRate(x.ToUpper()) > 0))
            {
                yield return FormatRateCmd(word.ToUpper());
            }
        }
        public static string FormatRateCmd(string row)
        {
            if (row.IndexOf('/') == -1) return row + "/RUB";
            else return row;
        }
        public static double GetRate(string row)
        {
            row = row.ToUpper();
            if (row == "RUB") return 1;

            var index = row.IndexOf('/');
            if (index == -1)
            {
                CurrencyRate rate;
                if (!Rates.TryGetValue(row, out rate))
                {
                    return -1;
                }
                else
                {
                    return rate.Value;
                }
            }
            else
            {
                var first = row.Substring(0, index);
                var second = row.Substring(index + 1);

                if (first == "RUB") return 1 / GetRate(second);
                if (second == "RUB") return GetRate(first);

                CurrencyRate firstRate, secondRate;
                if (Rates.TryGetValue(first, out firstRate) && Rates.TryGetValue(second, out secondRate))
                {
                    return firstRate.Value / secondRate.Value;
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}
