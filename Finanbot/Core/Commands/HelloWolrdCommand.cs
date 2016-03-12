﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Finanbot.Core.Commands
{
    public class HelloWolrdCommand : PluginCommandHandler
    {
        public override string Help
        {
            get
            {
                return "It's test plugin, enter /test to use test command";
            }
        }
        public override bool Init(Session session)
        {
            DefaultHandlers["/test"] = new StaticTextCommand("Thx:) /exit - to stop helloworld");
            DefaultHandlers["/exit"] = new ExitCommand();
            return base.Init(session);
        }
    }
}
