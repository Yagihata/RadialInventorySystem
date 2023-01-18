#if RISV4_JSON
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace YagihataItems.RadialInventorySystemV4
{
	public static class RISV3toV4Updater
	{
		public static bool HasV3SettingsOnScene(string v3SettingsName)
		{
			var settingsContainerRoot = GameObject.Find(v3SettingsName);
			return settingsContainerRoot != null;
		}
		[InitializeOnLoadMethod]
		static void EditorInitialize()
		{
#if RISV4_SALVAGED
			ScriptingDefineSymbolsUtil.Remove("RISV4_V3");
#else
			Type type = GetTypeByClassName("YagihataItems.RadialInventorySystemV3.RISSettings");
			if (type != null)
				ScriptingDefineSymbolsUtil.Add("RISV4_V3");
			else
				ScriptingDefineSymbolsUtil.Remove("RISV4_V3");
#endif
		}
		public static void SalvageDatas(string v3SettingsName)
		{
#if RISV4_SALVAGED
			ScriptingDefineSymbolsUtil.Remove("RISV4_V3");
#else
			Type type = GetTypeByClassName("YagihataItems.RadialInventorySystemV3.RISSettings");
			if(type != null)
			{
				ScriptingDefineSymbolsUtil.Add("RISV4_V3");
				Type dataSalvagerType = GetTypeByClassName("YagihataItems.RadialInventorySystemV4.DataSalvager");
				object result = dataSalvagerType.InvokeMember("SalvageDatas", BindingFlags.InvokeMethod, null, null, new object[] { v3SettingsName });

			}
			else
			{
				ScriptingDefineSymbolsUtil.Remove("RISV4_V3");
				EditorUtility.DisplayDialog(RISStrings.GetString("ris"), RISStrings.GetString("missing_v3"), RISStrings.GetString("ok"));
			}
#endif
		}
#if RISV4_V3
		[MenuItem("Radial Inventory/RISV4 Disable V3")]
		public static void DisableV3Update()
		{
			ScriptingDefineSymbolsUtil.Remove("RISV4_V3");
            ScriptingDefineSymbolsUtil.Add("RISV4_SALVAGED");
		}
#endif
		public static Type GetTypeByClassName(string className)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{
					if (type.FullName == className)
					{
						return type;
					}
				}
			}
			return null;
		}
	}
}
#endif