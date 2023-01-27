using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using static UnityEditor.EditorGUILayout;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace YagihataItems.RadialInventorySystemV4
{
    public class EditorGUILayoutExtra
    {

        /// <summary>
        /// インデントレベル設定を考慮した仕切り線.
        /// </summary>
        /// <param name="useIndentLevel">インデントレベルを考慮するか.</param>
        public static void Separator(bool useIndentLevel = false)
        {
            EditorGUILayout.BeginHorizontal();
            if (useIndentLevel)
            {
                GUILayout.Space(EditorGUI.indentLevel * 15);
            }
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// インデントレベルを設定する仕切り線.
        /// </summary>
        /// <param name="indentLevel">インデントレベル</param>
        public static void Separator(int indentLevel)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indentLevel * 15);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.EndHorizontal();
        }
        public static void SeparatorWithSpace()
        {
            EditorGUILayoutExtra.Space();
            EditorGUILayoutExtra.Separator();
            EditorGUILayoutExtra.Space();
        }
        public static int IndexedStringList(string label, IndexedList indexedList, string unselectedStr = "(未選択)")
        {
            EditorGUI.BeginChangeCheck();
            var property = indexedList.list;
            Func<string, int, string> selector = (string name, int number) => $"{number}: \r{name}";
            var list = indexedList.list.Select(selector).ToList();
            var divider = string.Empty;
            list.Add(divider);
            list.Add(unselectedStr);
            var selectedIndex = indexedList.list.Length == 0 ? -1 : indexedList.index < 0 ? indexedList.list.Length + 1 : indexedList.index;
            var displayOptions = list.ToArray();
            var listIndex = EditorGUILayout.Popup(new GUIContent(label), indexedList.list.Length == 0 ? 1 : selectedIndex, displayOptions);
            var index = indexedList.list.Length > 0 ? listIndex : selectedIndex;
            if (EditorGUI.EndChangeCheck())
                indexedList.index = index;
            indexedList.index = index > indexedList.list.Length ? -1 : index;
            return indexedList.index;
        }
        public static void Space()
        {
            EditorGUILayout.LabelField("");
        }
        //Alternate Method
        public static bool LinkLabel(string labelText)
        {
            return LinkLabel(labelText, Color.black, new Vector2(), 0);
        }

        //Alternate Method
        public static bool LinkLabel(string labelText, Color labelColor)
        {
            return LinkLabel(labelText, labelColor, new Vector2(), 0);
        }

        //Alternate Method
        public static bool LinkLabel(string labelText, Color labelColor, Vector2 contentOffset)
        {
            return LinkLabel(labelText, labelColor, contentOffset, 0);
        }

        //The Main Method
        public static bool LinkLabel(string labelText, Color labelColor, Vector2 contentOffset, int fontSize)
        {
            //Let's use Unity's label style for this
            GUIStyle stl = EditorStyles.label;
            //Next let's record the settings for Unity's label style because we will have to make sure these settings get returned back to
            //normal after we are done changing them and drawing our LinkLabel.
            Color col = stl.normal.textColor;
            Vector2 os = stl.contentOffset;
            int size = stl.fontSize;
            //Now we can modify the label's settings via the editor style : EditorStyles.label (stl).
            stl.normal.textColor = labelColor;
            stl.contentOffset = contentOffset;
            stl.fontSize = fontSize;
            //We are now ready to draw our Linklabel. I will actually use a GUILayout.Button to do this and our "stl" style will
            //make the button appear as a label.

            //Note : You may include a web address parameter in this method and open a URL at this point if the button is clicked,
            //however, I am going to just return bool based on weather or not the link was clicked. This gives me more control over
            //what actually happens when a link label is used. I also will instead include a "URL version" of this method below.

            //Since the button already returns bool, I will just return that result straight across like this.

            try
            {
                Rect rect;
                var result = false;

                using (var scope = new EditorGUILayout.VerticalScope())
                {
                    result = GUILayout.Button(labelText, stl);
                    rect = scope.rect;
                }
                rect.y += rect.height - 3;
                rect.height = 1;
                GUIStyle stl2 = GUI.skin.box;
                EditorGUI.DrawRect(rect, labelColor);
                return result;
            }
            finally
            {
                //Remember to set the editor style (stl) back to normal here. A try / finally clause will work perfectly for this!!!

                stl.normal.textColor = col;
                stl.contentOffset = os;
                stl.fontSize = size;
            }
        }

        //This is a modified version of link label that opens a URL automatically. Note : this can also return bool if you want.
        public static void LinkLabel(string labelText, Color labelColor, Vector2 contentOffset, int fontSize, string webAddress)
        {
            if (LinkLabel(labelText, labelColor, contentOffset, fontSize))
            {
                try
                {
                    Application.OpenURL(@webAddress);
                    //if returning bool, return true here.
                }
                catch
                {
                    //In most cases, the catch clause would not happen but in the interest of being thorough I will log an
                    //error and have Unity "beep" if an exception gets thrown for any reason.
                    Debug.LogError("Could not open URL. Please check your network connection and ensure the web address is correct.");
                    EditorApplication.Beep();
                }
            }
            //if returning bool, return false here.
        }
        public static void LinkLabel(string labelText, Color labelColor, Vector2 contentOffset, int fontSize, Action action)
        {
            if (LinkLabel(labelText, labelColor, contentOffset, fontSize))
            {
                try
                {
                    action();
                    //if returning bool, return true here.
                }
                catch
                {
                    //In most cases, the catch clause would not happen but in the interest of being thorough I will log an
                    //error and have Unity "beep" if an exception gets thrown for any reason.
                    Debug.LogError("Could not open URL. Please check your network connection and ensure the web address is correct.");
                    EditorApplication.Beep();
                }
            }
            //if returning bool, return false here.
        }
        public static void HeaderWithVersionInfo(Texture2D headerTexture, float width, float height, string newVersion, string currentVersion, string versionPrefix, string newVersionText, string linkUrl, int y = 10)
        {
            GUILayout.Box(headerTexture, GUILayout.Width(width), GUILayout.Height(height));
            var rect = new Rect();
            rect.x = 10;
            rect.y = y;
            rect.width = width - 10;
            rect.height = height - 10;
            using (new GUILayout.AreaScope(rect))
            {
                using (new GUILayout.VerticalScope())
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(currentVersion);
                    }
                    GUILayout.FlexibleSpace();
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (newVersion.StartsWith(versionPrefix) && currentVersion != newVersion)
                        {
                            var beforeColor = GUI.backgroundColor;
                            GUI.backgroundColor = new Color(beforeColor.r, beforeColor.g, beforeColor.b, 0.7f);
                            using (new GUILayout.HorizontalScope(GUI.skin.box))
                                EditorGUILayoutExtra.LinkLabel(newVersionText, Color.blue, new Vector2(), 0, linkUrl);
                            GUI.backgroundColor = beforeColor;
                        }
                    }
                }
            }
        }
        public static void CostViewer(int memoryNow, int memoryAdded, int memoryUseFromScript, string nowMemStr, string remainMemStr, GUIStyle CountBarStyleL, GUIStyle CountBarStyleR)
        {
            Rect rect;
            using (var scope = new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(nowMemStr, CountBarStyleL, GUILayout.Height(15));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(remainMemStr, CountBarStyleR, GUILayout.Height(15));
                }
                GUILayout.Space(30);
                rect = scope.rect;
            }
            var baseRectHeight = 14;
            rect.x += 1;
            rect.width -= 2;
            rect.y += rect.height - 18;
            rect.height = baseRectHeight;
            var baseRectWidth = rect.width;
            var baseX = rect.x;
            var baseY = rect.y;
            EditorGUI.DrawRect(rect, Color.gray);

            var maxParamVRC = VRCExpressionParameters.MAX_PARAMETER_COST;
            var memDif = memoryAdded - memoryNow;
            var memMax = memoryAdded > maxParamVRC ? memoryAdded : maxParamVRC;
            var pixPerMem = rect.width / memMax;
            rect.width = pixPerMem * memoryNow;
            EditorGUI.DrawRect(rect, memoryNow <= maxParamVRC ? new Color(0.37f, 0.66f, 0.09f) : new Color(0.95f, 0.44f, 0.81f));
            rect.x = rect.x + rect.width;
            rect.width = pixPerMem * memDif;
            EditorGUI.DrawRect(rect, memoryAdded <= maxParamVRC ? new Color(0.10f, 0.63f, 0.88f) : new Color(0.89f, 0.07f, 0.00f));
            if (memoryAdded > maxParamVRC)
            {
                rect.x = baseX + (maxParamVRC * pixPerMem) - 2;
                rect.width = 4;
                rect.y -= 2;
                rect.height += 4;
                EditorGUI.DrawRect(rect, Color.black);
            }
            rect.x = baseX + 10;
            rect.y = baseY - baseRectHeight;
            rect.width = baseRectWidth - 20;
            rect.height = baseRectHeight;
            EditorGUI.LabelField(rect, $"{memoryNow}+{memDif}", CountBarStyleL);
            EditorGUI.LabelField(rect, $"{maxParamVRC - memoryAdded}", CountBarStyleR);
        }
    }
}
