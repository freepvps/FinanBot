using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanbot.Core.Helpers
{
    public static class Ext
    {
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
    }
}
