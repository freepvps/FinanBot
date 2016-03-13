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

        public override int GetPriority(Message query)
        {
            if (query.Type == MessageType.LocationMessage)
            {
                Log.Info("Location latitude = {0}, longitude = {1}", query.Location.Latitude, query.Location.Longitude);
                return 100;
            }
            return 0;
        }
        public override bool Query(Session session, Message query)
        {
            if (query.Type == MessageType.LocationMessage)
            {
                var location = query.Location;
                var result = RatesMap.GetExchangeOffices(location.Latitude, location.Longitude, "RUB", "USD");

                var ans1 = result.OrderBy(x => x.Rate).Take(1);
                var ans2 = result.OrderBy(x =>
                    (x.Latitude - location.Latitude) * (x.Latitude - location.Latitude) +
                    (x.Longitude - location.Longitude) * (x.Longitude - location.Longitude)
                ).Take(2);
                var ans = ans1.Union(ans2);
                if (ans.Count() == 0) return false;
                foreach (var x in ans)
                {
                    session.Api.SendTextMessage(session.ChatId, "Курс - " + x.Rate).Wait();
                    session.Api.SendLocation(session.ChatId, x.Latitude, x.Longitude).Wait();
                }

                return true;
            }
            return false;
        }
    }
}
