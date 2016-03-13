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
using Finanbot.Core.Plugins;
using Finanbot.Core.Helpers;

namespace Finanbot
{
    public class Program
    {
        public static Logger Log = LogManager.GetLogger("main");
        public static IniData Config;
        public static IniData Database;
        public static Api Api;
        static void Main(string[] args)
        {
            foreach(var exchange in Apis.CurrencyRates.GetExchangeRates())
            {
                Log.Trace("{0} - {1} - {2}", exchange.CharCode, exchange.Name, exchange.Value);
            }
            foreach (var exchange in Apis.RatesMap.GetExchangeOffices(55.5f, 37.5f, "RUB", "USD"))
            {
                Log.Trace("{0} - {1} : {2}", exchange.Rate, exchange.Latitude, exchange.Longitude);
            }
            Log.Trace("OK");

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
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
            Config = new IniDataParser().Parse(File.ReadAllText(path, Encoding.UTF8));

            var dbPath = Config["main"]["database"];
            if (File.Exists(dbPath))
            {
                Database = new IniDataParser().Parse(File.ReadAllText(dbPath, Encoding.UTF8));
                Log.Trace("Load db from {0}", dbPath);
            }
            else
            {
                Database = new IniData();
            }
            if (!Database.Sections.ContainsSection("main"))
            {
                Database.Sections.Add(new SectionData("main"));
            }

            Log.Trace("Started with config: {0}", path);
            var apiKey = Config["main"]["apikey"];
#if !DEBUG
            try
            {
#endif
                new Thread(Pulsar).Start();
                RunBot(apiKey);
#if !DEBUG
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
            }
#endif
            Environment.Exit(0);
        }
        static string GetSectionName(long chatId)
        {
            return "chat" + chatId.ToString();
        }
        static SectionData GetSection(long chatId)
        {
            return new SectionData(GetSectionName(chatId));
        }
        static void InitIds(Api api, params long[] chats)
        {
            foreach (var chat in chats)
            {
                if (!Database.Sections.ContainsSection(GetSectionName(chat)))
                {
                    Database.Sections.Add(GetSection(chat));
                }
                if (!Sessions.ContainsKey(chat))
                {
                    var ses = new Session(Config);
                    ses.ChatId = chat;
                    ses.Api = api;
                    var sectionName = GetSectionName(chat);
                    var section = Database[sectionName];
                    foreach(var plugin in ses.Plugins.Values)
                    {
                        plugin.LoadDatabase(section);
                    }
                    Sessions[chat] = ses;
                }
            }
        }
        static void SaveDatabase()
        {
            var path = Config["main"]["database"];

            lock (Sessions)
            {
                var dataBase = Database;
                lock(dataBase)
                {
                    if (!dataBase.Sections.ContainsSection("main"))
                        dataBase.Sections.Add(new SectionData("main"));
                    dataBase["main"]["chats"] = string.Join(";", Sessions.Select(x => x.Value.ChatId));
                    foreach (var session in Sessions.Values)
                    {
                        var sectionName = GetSectionName(session.ChatId);
                        if (!dataBase.Sections.ContainsSection(sectionName))
                        {
                            var s = GetSection(session.ChatId);
                            dataBase.Sections.Add(s);
                        }
                        var section = dataBase[sectionName];
                        foreach (var plugin in session.Plugins.Values)
                        {
                            plugin.SaveDatabase(section);
                        }
                    }
                    Database = dataBase;
                    File.WriteAllText(Config["main"]["database"], Database.ToString());
                    Log.Info("Database saved");
                }
            }
        }
        static void Pulsar()
        {
            Thread.Sleep(15000);
            Log.Trace("Pulsar started");
            while(true)
            {
                try
                {
                    lock (Sessions)
                    {
                        foreach( var session in Sessions.Values)
                        {
                            PulseSession(session);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("PulsarException");
                    Log.Error(ex);
                }
                try
                {
                    SaveDatabase();
                }
                catch (Exception ex)
                {
                    Log.Error("Save db error");
                    Log.Error(ex);
                }
                Thread.Sleep(60000);
            }
        }
        static void PulseSession(Session session)
        {
            try
            {
                foreach(var plugin in session.Plugins.Values)
                {
                    if (plugin is PulsePlugin)
                        ((PulsePlugin)plugin).SafePulse(session, false);
                }
            }
            catch (Exception ex)
            {
                Log.Error("SessionPulseError");
                Log.Error(ex);
            }
        }

        static Dictionary<long, Session> Sessions = new Dictionary<long, Session>();
        static void RunBot(string apiKey)
        {
            var bot = new Api(apiKey);
            var me = bot.GetMe().Result;
            Log.Trace("Bot name: " + me.FirstName);
            Log.Trace("Bot login: " + me.Username);

            try
            {
                var ids = Database["main"]["chats"].Safe().Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray();
                InitIds(bot, ids);
            }
            catch (Exception ex)
            {
                Log.Error("Load db chats error");
                Log.Error(ex);
            }

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
                            lock (Sessions)
                            {
                                InitIds(bot, update.Message.Chat.Id);
                                session = Sessions[update.Message.Chat.Id];
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
