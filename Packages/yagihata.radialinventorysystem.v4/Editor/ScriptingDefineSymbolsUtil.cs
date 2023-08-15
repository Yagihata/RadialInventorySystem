using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace YagihataItems.RadialInventorySystemV4
{
    static class ScriptingDefineSymbolsUtil
    {
        private static readonly BuildTargetGroup[] buildTargetGroup = new[]
        {
            BuildTargetGroup.Android,
            BuildTargetGroup.Standalone,
        };

        public static void Add(params string[] symbols) => EditSymbols(x => x.Concat(symbols).Distinct());

        public static void Remove(params string[] symbols) => EditSymbols(x => x.Except(symbols));

        private static void EditSymbols(Func<string[], IEnumerable<string>> editor)
        {
            foreach (var target in buildTargetGroup)
            {
                var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Split(';');
                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", editor(symbols)));
            }
        }
    }
}
