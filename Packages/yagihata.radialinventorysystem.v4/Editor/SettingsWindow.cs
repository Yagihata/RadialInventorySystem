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
        Vector2 windowSizeMin = new Vector2(480, 320);
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
            {
                MenuItemUtils.RemoveMenuItem("Radial Inventory/Uninstall RISV4(Legacy)");
                MenuItemUtils.RemoveMenuItem("Radial Inventory/Uninstall Legacy RISV4");
            }
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
            minSize = this.windowSizeMin;
            GUILayout.Space(10);
            EditorGUILayout.LabelField(RISStrings.GetString("settings_title"), titleStyle, GUILayout.ExpandWidth(true), GUILayout.Height(34));
            GUILayout.Space(10);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {

                EditorGUILayout.LabelField(RISStrings.GetString("settings_applysettings"));
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField(RISStrings.GetString("settings_optimizesettings"), GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(90);
                    EditorSettings.OptimizeParameters = EditorGUILayout.Toggle(EditorSettings.OptimizeParameters, GUILayout.Width(10));
                    GUILayout.Space(20);
                }
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField(RISStrings.GetString("settings_applyenabledefault"), GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(90);
                    EditorSettings.ApplyEnableDefault = EditorGUILayout.Toggle(EditorSettings.ApplyEnableDefault, GUILayout.Width(10));
                    GUILayout.Space(20);
                }
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField(RISStrings.GetString("settings_materialanimtype"), GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    EditorSettings.MaterialAnimationType = (MaterialAnimationType)EditorGUILayout.EnumPopup(EditorSettings.MaterialAnimationType, GUILayout.Width(100));
                    GUILayout.Space(15);
                }

                GUILayout.Space(5);
                EditorGUILayout.LabelField(RISStrings.GetString("settings_othersettings"));
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField(RISStrings.GetString("settings_autosetupmenu"), GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(90);
                    EditorSettings.AutoSetupMenu = EditorGUILayout.Toggle(EditorSettings.AutoSetupMenu, GUILayout.Width(10));
                    GUILayout.Space(20);
                }
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField(RISStrings.GetString("settings_autosetupparams"), GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(90);
                    EditorSettings.AutoSetupParams = EditorGUILayout.Toggle(EditorSettings.AutoSetupParams, GUILayout.Width(10));
                    GUILayout.Space(20);
                }
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField(RISStrings.GetString("settings_hideuninstallmenu"), GUILayout.Width(300));
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
                    EditorGUILayout.LabelField(RISStrings.GetString("settings_defaultstatuslimitter"), GUILayout.Width(300));
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