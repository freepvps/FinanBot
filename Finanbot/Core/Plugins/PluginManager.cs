using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;

namespace Finanbot.Core.Plugins
{
    public class PluginManager
    {
        private static Type pluginType = typeof(Plugin);
        private static Type[] emptyTypes = { };
        private static object[] emptyObjects = { };

        private static bool IsPlugin(Type type)
        {
            return type.IsSubclassOf(pluginType) && type != pluginType;
        }
        public static IEnumerable<Plugin> GetPlugins()
        {
            return GetPlugins(Assembly.GetExecutingAssembly());
        }
        public static IEnumerable<Plugin> GetPlugins(Assembly asm)
        {
            foreach(var type in asm.GetTypes().Where(IsPlugin))
            {
                yield return (Plugin)type.GetConstructor(emptyTypes).Invoke(emptyObjects);
            }
        }
    }
}
