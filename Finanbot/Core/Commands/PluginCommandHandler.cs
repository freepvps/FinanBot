using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Finanbot.Core.Plugins;

namespace Finanbot.Core.Commands
{
    public class PluginCommandHandler : CommandHandler 
    {
        public virtual Plugin Plugin { get; set; }
    }
}
