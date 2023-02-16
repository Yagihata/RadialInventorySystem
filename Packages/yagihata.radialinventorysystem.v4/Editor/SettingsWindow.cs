#if RISV4_JSON
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using BestHTTP.JSON;
using static YagihataItems.RadialInventorySystemV4.RIS;

namespace YagihataItems.RadialInventorySystemV4
{
    public class SettingsWindow : EditorWindow
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorSettings.SettingsLoaded += AfterInitialize;
        }

        private static void AfterInitialize()
        {
            UpdateToolbar();
            EditorSettings.SettingsLoaded -= AfterInitialize;
        }
        private static void UpdateToolbar()
        {
            if (EditorSettings.HideRISV4Uninstaller)
                MenuItemUtils.RemoveMenuItem("Radial Inventory/Uninstall RISV4(Legacy)");
            MenuItemUtils.Update();
        }
        [MenuItem("Radial Inventory/RISV4 Settings", priority = 50)]
        private static void Create()
        {
            RISStrings.EditorInitialize();
            GetWindow<SettingsWindow>("RISV4 Settings");
        }
        bool initialized = false;
        private void InitializeWindow()
        {
            EditorSettings.LoadSettings();
            initialized = true;
        }
        GUIStyle titleStyle = null;
        private void OnGUI()
        {
            if (!initialized)
                InitializeWindow();

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
                    EditorGUILayout.LabelField("適用時にExpressionParametersを最適化する", GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(90);
                    EditorSettings.OptimizeParameters = EditorGUILayout.Toggle(EditorSettings.OptimizeParameters, GUILayout.Width(10));
                    GUILayout.Space(20);
                }
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("プロップの初期状態をゲームオブジェクトにも適用する", GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(90);
                    EditorSettings.ApplyEnableDefault = EditorGUILayout.Toggle(EditorSettings.ApplyEnableDefault, GUILayout.Width(10));
                    GUILayout.Space(20);
                }
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("マテリアル変更アニメーションに全てのプロパティを含める", GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    EditorSettings.MaterialAnimationType = (MaterialAnimation)EditorGUILayout.EnumPopup(EditorSettings.MaterialAnimationType, GUILayout.Width(100));
                    GUILayout.Space(15);
                }

                GUILayout.Space(5);
                EditorGUILayout.LabelField("その他の設定");
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("メニューが消えている場合に自動で再生成する", GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(90);
                    EditorSettings.AutoSetupMenu = EditorGUILayout.Toggle(EditorSettings.AutoSetupMenu, GUILayout.Width(10));
                    GUILayout.Space(20);
                }
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("\"Uninstall RISV4(Legacy)\" を非表示にする", GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(90);
                    EditorGUI.BeginChangeCheck();
                    EditorSettings.HideRISV4Uninstaller = EditorGUILayout.Toggle(EditorSettings.HideRISV4Uninstaller, GUILayout.Width(10));
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorSettings.SaveSettings();
                        UpdateToolbar();
                        EditorUtility.RequestScriptReload();
                    }
                    GUILayout.Space(20);
                }
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("排他グループ内の初期有効オブジェクト数に制限を設ける", GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(90);
                    EditorSettings.DefaultStatusLimitter = EditorGUILayout.Toggle(EditorSettings.DefaultStatusLimitter, GUILayout.Width(10));
                    GUILayout.Space(20);
                }
            }
        }

        void OnDestroy()
        {
            EditorSettings.SaveSettings();
        }
    }
}
#endif