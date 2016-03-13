using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Helpers;
using Finanbot.Apis;

namespace Finanbot.Core.Plugins
{
    public class NearestExchange : Plugin
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
                return "Ближайшие обменники с выгодным курсом";
            }
        }
        public override string PluginName
        {
            get
            {
                return "exchangesearch";
            }
        }
        public override string UserPluginName
        {
            get
            {
                return "Обменники";
            }
        }

        public Location Location { get; private set; }
        public override int GetPriority(Message query)
        {
            if (query.Type == MessageType.LocationMessage)
            {
                Log.Info("Location latitude = {0}, longitude = {1}", query.Location.Latitude, query.Location.Longitude);
                return 100;
            }
            else if (query.Type == MessageType.TextMessage)
            {
                return ExchangePlugin.SearchRates(query.Text.ToLower()).Count() == 1 ? 101 : 0;
            }
            return 0;
        }
        public override bool Query(Session session, Message query)
        {
            if (query.Type == MessageType.LocationMessage)
            {
                Location = query.Location;
                session.Location = query.Location;

                SendAnswer(session, "Теперь вы можете запросить расположение какого-нибудь пункта обмена валют. Например, командой: \"Доллар к рублю\"");
                return true;
            }
            else if (query.Type == MessageType.TextMessage)
            {
                if (Location == null)
                {
                    SendAnswer(session, "Прежде, чем искать пункты обмена валют, отправьте мне свои координаты");
                    return true;
                }
                var exchs = ExchangePlugin.SearchRates(query.Text.ToLower());
                foreach(var exch in exchs)
                {
                    var exchrow = ExchangePlugin.FormatRateCmd(exch);

                    var index = exch.IndexOf('/');
                    var from = exch.Substring(0, index);
                    var to = exch.Substring(index + 1);

                    var result = SearchExchange(session, from, to);
                    if (!result)
                    {
                        SendAnswer(session, "К сожалению, подходящего пункта обмена валют не найдено:(");
                        return true;
                    }
                }
            }
            return false;
        }

        public bool SearchExchange(Session session, string from, string to)
        {

            var result = RatesMap.GetExchangeOffices(Location.Latitude, Location.Longitude, from, to);

            var ans1 = result.OrderBy(x => x.Rate).Take(1);
            var ans2 = result.OrderBy(x =>
                (x.Latitude - Location.Latitude) * (x.Latitude - Location.Latitude) +
                (x.Longitude - Location.Longitude) * (x.Longitude - Location.Longitude)
            ).Take(2);
            var ans = ans1.Union(ans2);
            if (ans.Count() == 0) return false;
            foreach (var x in ans)
            {
                session.Api.SendTextMessage(session.ChatId, "Курс " + from  + "/" + to + " - " + x.Rate).Wait();
                session.Api.SendLocation(session.ChatId, x.Latitude, x.Longitude).Wait();
            }
            return true;
        }
    }
}
