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
                    var index = text.IndexOf(' ');
                    if (index == -1) index = text.Length;

                    var pluginName = text.Substring(0, index);
                    Plugin plugin;
                    if (session.Plugins.TryGetValue(pluginName, out plugin))
                    {
                        plugin.Push(plugin.Root);
                        return true;
                    }
                }
                else
                {
                    var query = text;
                    var set = new List<Tuple<string, int>>();

                    var sb = new StringBuilder();
                    foreach(var plugin in session.Plugins.Values)
                    {
                        var price = plugin.Query(session, query, sb);
                    }
                }
            }
            return false;
        }
    }
}
