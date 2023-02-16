#if RISV4_JSON
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using BestHTTP.JSON;

namespace YagihataItems.RadialInventorySystemV4
{
    public class SettingsWindow : EditorWindow
    {
        private static bool hideRISV4Uninstaller = true;
        public static bool HideRISV4Uninstaller { get { return hideRISV4Uninstaller; } }
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.delayCall += () => DelayInitialize();
        }

        private static void DelayInitialize()
        {
            try
            {
                if(File.Exists(RIS.RISV4SettingsPath))
                {
                    using (var sr = new StreamReader(RIS.RISV4SettingsPath, Encoding.UTF8))
                    {
                        var jsonData = sr.ReadToEnd();
                        if (jsonData != null)
                        {
                            var settings = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonData);
                            if (settings.ContainsKey("HideRISV4Uninstaller"))
                                hideRISV4Uninstaller = settings["HideRISV4Uninstaller"];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            if (hideRISV4Uninstaller)
                MenuItemUtils.RemoveMenuItem("Radial Inventory/Uninstall RISV4(Legacy)");
            MenuItemUtils.Update(); 
        }
        private void SaveSettings()
        {
            var settings = new Dictionary<string, dynamic>
            {
                { "HideRISV4Uninstaller", hideRISV4Uninstaller }
            };
            var jsonData = JsonConvert.SerializeObject(settings, Formatting.Indented);
            using (var sw = new StreamWriter(RIS.RISV4SettingsPath, false, Encoding.UTF8))
            {
                sw.Write(jsonData);
            }
        }
        [MenuItem("Radial Inventory/RISV4 Settings", priority = 50)]
        private static void Create()
        {
            RISStrings.EditorInitialize();
            GetWindow<SettingsWindow>("RISV4 Settings");
        }
        GUIStyle titleStyle = null;
        private void OnGUI()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle("ProjectBrowserHeaderBgTop");
                titleStyle.fontSize = 24;
                titleStyle.stretchHeight = true;
                titleStyle.fixedHeight = 34;
                titleStyle.alignment = TextAnchor.MiddleLeft;
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("RISV4 設定画面", titleStyle, GUILayout.ExpandWidth(true), GUILayout.Height(34));
            GUILayout.Space(10);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {

                EditorGUILayout.LabelField("適用時の設定");
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("Write Defaultsを使用する（非推奨）", GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.Toggle(true, GUILayout.Width(10));
                    GUILayout.Space(20);
                }
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("適用時にExpressionParametersを最適化する", GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.Toggle(true, GUILayout.Width(10));
                    GUILayout.Space(20);
                }
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("プロップの初期状態をゲームオブジェクトにも適用する", GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.Toggle(true, GUILayout.Width(10));
                    GUILayout.Space(20);
                }
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("マテリアル変更アニメーションに全てのプロパティを含める", GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.Toggle(true, GUILayout.Width(10));
                    GUILayout.Space(20);
                }

                GUILayout.Space(5);
                EditorGUILayout.LabelField("その他の設定");
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("メニューが消えている場合に自動で再生成する", GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.Toggle(true, GUILayout.Width(10));
                    GUILayout.Space(20);
                }
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("\"Uninstall RISV4(Legacy)\" を非表示にする", GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    EditorGUI.BeginChangeCheck();
                    hideRISV4Uninstaller = EditorGUILayout.Toggle(hideRISV4Uninstaller, GUILayout.Width(10));
                    if (EditorGUI.EndChangeCheck())
                    {
                        SaveSettings();
                        EditorUtility.RequestScriptReload();
                    }
                    GUILayout.Space(20);
                }
            }
        }
    }
}
#endif