using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Finanbot.Core.Helpers
{
    public class ReplyQuadreKeyboard : ReplyKeyboardMarkup
    {
        public ReplyQuadreKeyboard(bool oneTimeKeyboard = false, params string[] args)
        {
            base.OneTimeKeyboard = oneTimeKeyboard;
            base.Keyboard = Ext.ToMatrix(args);
        }
    }
}
