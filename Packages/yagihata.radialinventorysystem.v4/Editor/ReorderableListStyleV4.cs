using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV4
{
    public static class ReorderableListStyle
    {
        public static GUIContent AddContent;
        public static GUIStyle AddStyle;
        public static GUIContent SubContent;
        public static GUIStyle SubStyle;
        static ReorderableListStyle()
        {
            AddContent = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add to list");
            AddStyle = "RL FooterButton";
            SubContent = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove selection from list");
            SubStyle = "RL FooterButton";
        }
    }
}
