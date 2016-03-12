using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using IniParser;
using IniParser.Model;
using IniParser.Parser;
using NLog;
using Finanbot.Core;
using File = System.IO.File;
using System.Threading;

namespace Finanbot
{
    public class Program
    {
        public static Logger Log = LogManager.GetLogger("main");
        public static IniData Config;
        public static Api Api;
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new[] { "fin.conf" };
            }
            var path = args.First();
            if (!File.Exists(path))
            {
                Log.Error("File {0} not found", path);
                return;
            }
            Config = new IniDataParser().Parse(File.ReadAllText(path, Encoding.UTF8));

            Log.Trace("Started with config: {0}", path);
            var apiKey = Config["main"]["apikey"];
#if !DEBUG
            try
            {
#endif
                RunBot(apiKey);
#if !DEBUG
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
            }
#endif
        }

        static Dictionary<long, Session> Sessions = new Dictionary<long, Session>();
        static void RunBot(string apiKey)
        {
            var bot = new Api(apiKey);
            var me = bot.GetMe().Result;
            Log.Trace("Bot name: " + me.FirstName);
            Log.Trace("Bot login: " + me.Username);

            var lastUpdateId = 0;
            while (true)
            {
                Update[] updates = null;

                try
                {
                    updates = bot.GetUpdates(lastUpdateId).Result;
                }
                catch (Exception ex)
                {
                    Log.Error("GetUpdates error");
                    Log.Error(ex);
                }

                if (updates != null)
                {
                    foreach (var update in updates)
                    {
                        lastUpdateId = update.Id + 1;
#if !DEBUG
                        try
                        {
#endif
                            Session session;
                            if (!Sessions.TryGetValue(update.Message.Chat.Id, out session))
                            {
                                session = new Session();
                                Sessions[update.Message.Chat.Id] = session;
                            }
                            session.Process(bot, update.Message);
#if !DEBUG
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Handle update error update.Id = {0}", update.Id);
                            Log.Error(ex);
                        }
#endif
                    }
                }
                Task.Delay(10).Wait();
            }
        }
    }
}
