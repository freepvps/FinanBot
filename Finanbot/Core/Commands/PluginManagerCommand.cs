using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Finanbot.Core.Plugins;

namespace Finanbot.Core.Commands
{
    public class PluginManagerCommand : CommandHandler
    {
        public override bool Handle(Session session, Message message)
        {
            if (message.Type == MessageType.TextMessage)
            {
                var text = message.Text;
                if (text.StartsWith("/"))
                {
                    if (text == "/help")
                    {
                        var plugins = new StringBuilder();
                        plugins.AppendLine("Список доступных сервисов:");
                        foreach(var p in session.Plugins.Values)
                        {
                            plugins.AppendLine(string.Format("{1} - {2} (/{0})", p.PluginName, p.UserPluginName, p.Description));
                        }
                        session.Send(plugins.ToString());
                        return true;
                    }

                    var index = text.IndexOf(' ');
                    if (index == -1) index = text.Length;

                    var pluginName = text.Substring(0, index);
                    Plugin plugin;
                    if (session.Plugins.TryGetValue(pluginName, out plugin))
                    {
                        plugin.Push(plugin.Root);
                        return true;
                    }
                    else
                    {
                        session.Send("Сервис не найден, попробуйте выполнить команду /help для получения справки.");
                    }
                }
                else
                {
                    var query = text;
                    var set = new List<Tuple<string, int>>();

                    var sb = new StringBuilder();
                    foreach(var plugin in session.Plugins.Values)
                    {
                        var price = plugin.Query(session, message, sb);
                        if (price > 0)
                        {
                            var result = string.Format("{0}:\r\n{1}", plugin.UserPluginName, sb.ToString());
                            set.Add(Tuple.Create(result, price));
                        }
                        sb.Clear();
                    }
                    if (set.Count == 0)
                    {
                        session.Send("По вашему запросу ничего не найдено. Напишите /help для получения справки.");
                    }
                    else
                    {
                        foreach (var ans in set.OrderBy(x => x.Item2).Reverse())
                        {
                            session.Send(ans.Item1);
                        }
                    }
                    return true;


                }
            }
            return false;
        }
    }
}
