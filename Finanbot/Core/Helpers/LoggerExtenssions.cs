using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using System.IO;

namespace Finanbot.Core.Helpers
{
    public static class LoggerExtensions
    {
        /*
        
  Fatal	Highest level: important stuff down
  Error	For example application crashes / exceptions.
  Warn	Incorrect behavior but the application can continue
  Info	Normal behavior like mail sent, user updated profile etc.
  Debug	Executed queries, user authenticated, session expired
  Trace	Begin method X, end method X etc
        */
        private static JsonSerializer serializer = JsonSerializer.Create();
        public static void Trace(this Logger logger, Message telegramMessage, bool full = false)
        {
            logger.Log(LogLevel.Trace, telegramMessage, full);
        }
        public static void Debug(this Logger logger, Message telegramMessage, bool full = false)
        {
            logger.Log(LogLevel.Debug, telegramMessage, full);
        }
        public static void Info(this Logger logger, Message telegramMessage, bool full = false)
        {
            logger.Log(LogLevel.Info, telegramMessage, full);
        }
        public static void Warn(this Logger logger, Message telegramMessage, bool full = false)
        {
            logger.Log(LogLevel.Warn, telegramMessage, full);
        }
        public static void Error(this Logger logger, Message telegramMessage, bool full = false)
        {
            logger.Log(LogLevel.Error, telegramMessage, full);
        }
        public static void Fatal(this Logger logger, Message telegramMessage, bool full = false)
        {
            logger.Log(LogLevel.Fatal, telegramMessage, full);
        }
        public static void Log(this Logger logger, LogLevel level, Message telegramMessage, bool full = false)
        {
            if (telegramMessage == null)
            {
                logger.Log(level, "telegramNullMessage");
                return;
            }
            var chatId = telegramMessage.Chat.Id;
            var messageId = telegramMessage.MessageId;
            var messageType = telegramMessage.Type;
            var from = telegramMessage.From;
            var text = telegramMessage.Text;
            var json = string.Empty;

            var fromStr = string.Format("{0}({1} {2}, {3})", from.Id, from.FirstName, from.LastName, from.Username);

            var format = "{0} {1} {2} {3}";
            if (messageType == MessageType.TextMessage)
                format += " {4}";
            if (full)
            {
                format += " {5}";
                using (var sw = new StringWriter())
                {
                    serializer.Serialize(sw, telegramMessage);
                    json = sw.ToString();
                }
            }

            logger.Log(level, string.Format(format, chatId, messageId, messageType, fromStr, text, json));
        }
    }
}
