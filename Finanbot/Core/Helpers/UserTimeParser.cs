using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanbot.Core.Helpers
{
    public class UserTimeParser
    {
        public static DateTime ParseDatetime(string row)
        {
            var tokens = new Queue<string>(GetTokens(row));

            var dateTime = DateTime.Now;
            while(tokens.Count > 0)
            {
                var token = tokens.Dequeue();

                var br = false;
                switch(token)
                {
                    case "завтра": dateTime = dateTime.AddDays(1); continue;
                    case "послезавтра": dateTime = dateTime.AddDays(2); continue;

                    case "вчера": dateTime = dateTime.AddDays(-1); continue;
                    case "позавчера": dateTime = dateTime.AddDays(-2); continue;

                    case "утром": dateTime = dateTime.AddHours(8 - dateTime.Hour); continue;
                    case "днем": dateTime = dateTime.AddHours(12 - dateTime.Hour); continue;
                    case "вечером": dateTime = dateTime.AddHours(17 - dateTime.Hour); continue;
                    case "ночью": dateTime = dateTime.AddHours(12 - dateTime.Hour); continue;

                    case "в": dateTime = dateTime.AddHours(-dateTime.Hour).AddMinutes(-dateTime.Minute); br = true; break;
                    case "через": br = true; break;
                }
                if (br) break;
            }

            dateTime = dateTime.AddSeconds(ParseTime(tokens));
            return dateTime;
        }
        private static int GetTime(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return 0;
            }
            switch (token)
            {
                case "полчаса": return 30 * 60;
                case "полминуты": return 30;
            }
            var set = new HashSet<string>
            {
                "сек", "мин", "час", "дн", "нед"
            };
            var ok = false;
            foreach (var x in set)
            {
                if (token.StartsWith(x))
                {
                    ok = true; break;
                }
            }
            if (!ok) return 0;
            // с, м, ч, д
            // секунда
            // минута
            // час
            // день
            var ch = token[0];
            switch(ch)
            {
                case 'с': return 1;
                case 'м': return 60;
                case 'ч': return 60 * 60;
                case 'д': return 60 * 60 * 24;
                case 'н': return 60 * 60 * 24 * 7;
            }
            return 0;
        }
        public static string[] GetTokens(string s)
        {

            s = s + " ";

            var tokensList = new List<string>();
            var i = 0;

            for (var j = 0; j < s.Length; j++)
            {
                var isdg = char.IsDigit(s[j]);
                var islt = char.IsLetter(s[j]);

                var lisdg = char.IsDigit(s[i]);
                var lislt = char.IsLetter(s[i]);
                var length = j - i;

                if (!isdg && !islt)
                {
                    if (length > 0)
                    {
                        tokensList.Add(s.Substring(i, length));
                    }
                    i = j + 1;
                    continue;
                }
                if (lislt != islt || lisdg != isdg)
                {
                    tokensList.Add(s.Substring(i, length));
                    i = j;
                }
            }
            return tokensList.ToArray();
        }
        public static int ParseTime(string s, int defaultTime = 60)
        {
            var tokens = new Queue<string>(GetTokens(s));
            return ParseTime(tokens);
        }
        public static int ParseTime(Queue<string> tokens, int defaultTime = 60)
        {
            var ans = 0;
            while (tokens.Count > 0)
            {
                var token = tokens.Dequeue();
                var time = 0;
                if (int.TryParse(token, out time))
                {
                    var mod = defaultTime;
                    if (tokens.Count > 0)
                    {
                        mod = GetTime(tokens.Peek());
                        if (mod != 0)
                        {
                            tokens.Dequeue();
                        }
                        else
                        {
                            mod = defaultTime;
                        }
                    }
                    ans += time * mod;
                }
                else
                {
                    ans += GetTime(token);
                }
            }
            return ans;
        }
        public static string SecondsToString(int seconds)
        {
            var d = new Dictionary<string, int>
            {
                { "сек.", 60 },
                { "мин.", 60 },
                { "ч.", 60 },
                { "дн.", 24 },
                { "нед.", 7 }
            };

            var ans = string.Empty;
            foreach (var x in d)
            {
                var t = seconds % x.Value;
                if (x.Value == 7) t = seconds;

                seconds /= x.Value;

                if (t != 0)
                {
                    if (!string.IsNullOrWhiteSpace(ans)) ans = " " + ans;
                    ans = string.Format("{0} {1}", t, x.Key) + ans;
                }
            }
            if (string.IsNullOrWhiteSpace(ans)) return "0 сек";
            return ans;
        }
    }
}
