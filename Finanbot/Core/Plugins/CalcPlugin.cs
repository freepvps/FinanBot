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
                return true;
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
                        SendAnswer(session, ans.ToString());
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
        public override void Start(Session session)
        {
            var examples = new string[]
            {
                "10 + 5", "1.5 * 100", "100 / 70", "Sqrt(1234)"
            };

            var sb = new StringBuilder();
            sb.AppendLine("Примеры:");
            for(var i = 0; i < examples.Length; i++)
            {
                sb.AppendLine((i + 1).ToString() + ". " + examples[i]);
            }

            Send(session, sb.ToString(), new Finanbot.Core.Helpers.ReplyQuadreKeyboard(true, examples));
            session.StopPlugin(this);
        }
    }
}
