using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Finanbot.Core.Helpers
{
    public class ReplyLinesKeyboard : ReplyKeyboardMarkup
    {
        public ReplyLinesKeyboard(bool oneTimeKeyboard = true, params string[] args)
        {
            base.OneTimeKeyboard = oneTimeKeyboard;
            base.Keyboard = args.Select(x => new[] { x }).ToArray();
        }
    }
}
