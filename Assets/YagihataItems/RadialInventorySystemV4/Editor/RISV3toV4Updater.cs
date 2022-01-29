using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV4
{
	public static class RISV3toV4Updater
	{
		public static bool HasV3SettingsOnScene(string v3SettingsName)
        {
			var settingsContainerRoot = GameObject.Find(v3SettingsName);
			return settingsContainerRoot != null;
		}
	}
}
