using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Finanbot.Core.Helpers
{
    public static class Ext
    {
        public static readonly Type[] EmptyTypes = { };
        public static readonly string[] EmptyString = { };
        public static readonly byte[] EmptyBytes = { };
        public static readonly object[] EmptyObjects = { };
        public static string[][] ToMatrix(params string[] lines)
        {

            var n = 1;
            while (n * n < lines.Length) n++;
            var rows = new List<string>[n];
            for (var i = 0; i < n; i++)
            {
                rows[i] = new List<string>();
            }
            for (var i = 0; i < lines.Length; i++)
            {
                rows[i / n].Add(lines[i]);
            }
            var result = new string[n][];
            for (var i = 0; i < n; i++)
            {
                result[i] = rows[i].ToArray();
            }
            return result;
        }

        public static int PriceCalc(string text, string[][] keywords)
        {
            text = text.ToLower();
            var price = 0;
            foreach(var set in keywords)
            {
                foreach (var word in set)
                {
                    if (text.Contains(word))
                    {
                        price++;
                        break;
                    }
                }
            }
            return price;
        }

        public static int ToUnixTime(DateTime dt)
        {
            return (int)(dt - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }
        public static DateTime FromUnixTime(int unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(unixTime);
        }

        public static string Safe(this string s)
        {
            return s == null ? string.Empty : s;
        }
        public static string Safe(this string s, string defaultValue)
        {
            return string.IsNullOrEmpty(s) ? defaultValue : s;
        }
        public static T Safe<T>(this object obj)
        {
            if (obj is T) return (T)obj;
            if (obj == null || obj is DBNull) return default(T);
            return (T)(dynamic)obj;
        }
        public static double Distance(this Location location, Location other)
        {
            var lt = location.Latitude - other.Latitude;
            var lg = location.Longitude - other.Longitude;
            return Math.Sqrt(lt * lt + lg * lg);
        }

        public static List<string> ParseArgs(string s)
        {
            var args = new List<string>();

            var pos = 0;
            while (pos < s.Length)
            {
                if (char.IsWhiteSpace(s[pos]))
                {
                    pos++;
                    continue;
                }
                if (pos + 1 < s.Length && s[pos] == s[pos + 1] && s[pos] == '/')
                {
                    args.Add(s.Substring(pos));
                    break;
                }
                if (s[pos] == '\"')
                {
                    var begin = pos;
                    var end = begin + 1;

                    while (s[end] != '\"')
                    {
                        if (s[end] == '\\')
                        {
                            end++;
                        }
                        end++;
                    }
                    var line = s.Substring(begin + 1, (end - begin - 1));
                    args.Add(line);
                    pos = end + 1;
                }
                else
                {
                    var end = pos;
                    while (end < s.Length && !char.IsWhiteSpace(s[end]))
                    {
                        end++;
                    }
                    var line = s.Substring(pos, end - pos);
                    args.Add(line);
                    pos = end;
                }
            }
            for (var i = 0; i < args.Count; i++)
            {
                args[i] = NDecode(args[i]);
            }
            return args;
        }
        public static string NDecode(string text, char cc = '\\')
        {
            var sb = new StringBuilder();
            var forceWrite = false;
            foreach (var ch in text)
            {
                if (forceWrite)
                {
                    forceWrite = false;
                    switch (ch)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case '*': sb.Append((char)58002); break;
                        case '0': sb.Append('\0'); break;
                        default: sb.Append(ch); break;
                    }
                    continue;
                }
                if (ch == cc)
                {
                    forceWrite = true;
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }
    }
}
