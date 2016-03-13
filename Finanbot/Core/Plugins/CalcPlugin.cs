using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using NCalc;

namespace Finanbot.Core.Plugins
{
    public class CalcPlugin : Plugin
    {
        public override bool CanRun
        {
            get
            {
                return false;
            }
        }
        public override string Description
        {
            get
            {
                return "Калькулятор выражений";
            }
        }
        public override string PluginName
        {
            get
            {
                return "calc";
            }
        }
        public override string UserPluginName
        {
            get
            {
                return "Калькулятор";
            }
        }

        public override int GetPriority(Message query)
        {
            if (query.Type == MessageType.TextMessage)
            {
                var expr = new Expression(query.Text);
                if (expr.HasErrors()) return 0;
                else
                {
                    try
                    {
                        expr.Evaluate();
                        return short.MaxValue;
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("CalcError::GetPriority expr = {0}", query.Text);
                        Log.Warn(ex);
                        return 0;
                    }
                }
            }
            return 0;
        }
        public override bool Query(Session session, Message query)
        {
            if (query.Type == MessageType.TextMessage)
            {
                var expr = new Expression(query.Text);
                if (expr.HasErrors()) return false;
                else
                {
                    try
                    {
                        var ans = expr.Evaluate();
                        Send(session, ans.ToString());
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("CalcError::Query expr = {0}", query.Text);
                        Log.Warn(ex);
                        return false;
                    }
                }
            }
            return false;
        }
    }
}
