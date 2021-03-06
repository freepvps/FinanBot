﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using NLog;
using Finanbot.Core.Plugins;
using IniParser.Model;

namespace Finanbot.Core
{
    public class Session
    {
        public Dictionary<string, Plugin> Plugins { get; private set; }

        public Logger Log = LogManager.GetLogger("session");
        public IniData Config { get; set; }
        public IniData Database { get; set; }
        
        public Plugin RunnedPlugin { get; set; }

        public Api Api { get; set; }
        public long ChatId { get; set; }

        public Location Location { get; set; }

        public Session(IniData config)
        {
            Config = config;

            var plugins = new HashSet<string>(config["main"]["plugins"].Split(", ;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

            Plugins = new Dictionary<string, Plugin>();
            var pluginsPre = new Dictionary<string, Plugin>();
            foreach(var plugin in PluginManager.GetPlugins())
            {
                if (plugins.Contains(plugin.PluginName))
                {
                    pluginsPre[plugin.PluginName] = plugin;
                    plugin.Initialize(this);
                    Log.Trace("Initialize plugin: /{0}, canrun = {1}", plugin.PluginName, plugin.CanRun);
                }
            }
            foreach(var plugin in plugins)
            {
                if (pluginsPre.ContainsKey(plugin))
                {
                    Plugins["/" + plugin] = pluginsPre[plugin];
                }
            }
        }
        
        public void Send(string chatMessage, ReplyMarkup replyMarkup = null)
        {
            try
            {
                Api.SendTextMessage(ChatId, chatMessage, replyMarkup: replyMarkup).Wait();
            }
            catch (Exception ex)
            {
                Log.Error("SendError chatId = {0}, message = {1}", ChatId, chatMessage);
                Log.Error(ex);

                throw;
            }
        }

        public void StartPlugin(Plugin plugin)
        {
            RunnedPlugin = plugin;
            plugin.Start(this);
        }
        public void StopPlugin(Plugin plugin)
        {
            plugin.Stop(this);
            RunnedPlugin = null;
        }
        public void Process(Api api, Message message)
        {
            Api = api;
            ChatId = message.Chat.Id;
            if (message.Type == MessageType.TextMessage && message.Text.StartsWith("/"))
            {
                var trim = message.Text.Trim();
                Plugin plugin;
                if (Plugins.TryGetValue(trim, out plugin) && plugin.CanRun)
                {
                    StartPlugin(plugin);
                    return;
                }
                switch(message.Text)
                {
                    case "/exit": if (RunnedPlugin != null) StopPlugin(RunnedPlugin); return;
                }
            }
            if (RunnedPlugin != null)
            {
                RunnedPlugin.Command(this, message);
            }
            else
            {
                if (message.Type == MessageType.TextMessage)
                {
                    switch (message.Text)
                    {
                        case "/start":
                        case "/help":
                            {
                                var pp = 0;
                                var sb = new StringBuilder();
                                sb.AppendLine("Список доступных сервисов:");
                                foreach (var plugin in Plugins.Values)
                                {
                                    if (plugin.CanRun)
                                    {
                                        sb.AppendLine(++pp + string.Format(". {0} - {1} (/{2})", plugin.UserPluginName, plugin.Description, plugin.PluginName));
                                    }
                                    else
                                    {
                                        sb.AppendLine(++pp + string.Format(". {0} - {1}", plugin.UserPluginName, plugin.Description));
                                    }
                                }
                                api.SendTextMessage(ChatId, sb.ToString());
                            }
                            return;
                    }
                }


                var priorities = new List<Tuple<Plugin, int>>();
                foreach (var plugin in Plugins.Values)
                {
                    var priority = plugin.GetPriority(message);
                    if (priority > 0)
                        priorities.Add(Tuple.Create(plugin, priority));
                }
                var count = 3;
                var empty = true;
                var big = 0;
                if (priorities.Count > 0) big = priorities.Select(x => x.Item2).Max();
                foreach (var plugin in priorities.OrderBy(x => x.Item2))
                {
                    if (big >= short.MaxValue && plugin.Item2 < big) continue;

                    var handled = plugin.Item1.Query(this, message);
                    if (handled)
                    {
                        if (plugin.Item2 > big)
                        {
                            big = plugin.Item2;
                        }
                        empty = false;
                        count--;
                        if (count == 0) break;
                    }
                }
                if (!empty) return;
                api.SendTextMessage(ChatId, "Я не знаю, что вы хотите, попробуйте вызвать команду /help");
            }
        }
    }
}
