using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV4
{
    public static class TabStyle
    {
        private static Dictionary<Type, GUIContent[]> tabToggles = new Dictionary<Type, GUIContent[]>();
        public static GUIContent[] GetTabToggles<T>() where T : struct
        {
            if (!tabToggles.ContainsKey(typeof(T)))
            {
                tabToggles.Add(typeof(T), Enum.GetNames(typeof(T)).Select(x => new GUIContent(x)).ToArray());
            }
            return tabToggles[typeof(T)];
        }
        public static readonly GUIStyle TabButtonStyle = "PreButton";
        public static readonly GUI.ToolbarButtonSize TabButtonSize = GUI.ToolbarButtonSize.Fixed;
    }
}
