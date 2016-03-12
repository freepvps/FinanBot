using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanbot.Core.Commands
{
    public class ExitCommand : PluginCommandHandler
    {
        public override bool Init(Session session)
        {
            Plugin.CompletePlugin();
            return true;
        }
    }
}
