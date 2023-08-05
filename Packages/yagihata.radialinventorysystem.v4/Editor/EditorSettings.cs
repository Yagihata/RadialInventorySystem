using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using static YagihataItems.RadialInventorySystemV4.RIS;

namespace YagihataItems.RadialInventorySystemV4
{
    public static class EditorSettings
    {
        public delegate void SettingsLoadedHandler();
        public static SettingsLoadedHandler SettingsLoaded;
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.delayCall += () => LoadSettings();
        }
        public static bool HideRISV4Uninstaller { get; set; } = true;
        public static bool OptimizeParameters { get; set; } = true;
        public static bool ApplyEnableDefault { get; set; } = true;
        public static bool AutoSetupMenu { get; set; } = true;
        public static bool AutoSetupParams { get; set; } = false;
        public static bool DefaultStatusLimitter { get; set; } = true;
        public static MaterialAnimationType MaterialAnimationType { get; set; } = MaterialAnimationType.Difference;
        public static void SaveSettings()
        {
            var settings = new Dictionary<string, dynamic>
            {
                { nameof(HideRISV4Uninstaller), HideRISV4Uninstaller },
                { nameof(OptimizeParameters), OptimizeParameters },
                { nameof(ApplyEnableDefault), ApplyEnableDefault },
                { nameof(AutoSetupMenu), AutoSetupMenu },
                { nameof(AutoSetupParams), AutoSetupParams },
                { nameof(DefaultStatusLimitter), DefaultStatusLimitter },
                { nameof(MaterialAnimationType), MaterialAnimationType.ToString() }
            };
            var jsonData = JsonConvert.SerializeObject(settings, Formatting.Indented);
            using (var sw = new StreamWriter(RIS.RISV4SettingsPath, false, Encoding.UTF8))
            {
                sw.Write(jsonData);
            }
        }
        public static void LoadSettings()
        {
            try
            {
                if (File.Exists(RIS.RISV4SettingsPath))
                {
                    using (var sr = new StreamReader(RIS.RISV4SettingsPath, Encoding.UTF8))
                    {
                        var jsonData = sr.ReadToEnd();
                        if (jsonData != null)
                        {
                            var settings = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonData);

                            var key = nameof(HideRISV4Uninstaller);
                            if (settings.ContainsKey(key))
                                HideRISV4Uninstaller = settings[key];

                            key = nameof(OptimizeParameters);
                            if (settings.ContainsKey(key))
                                OptimizeParameters = settings[key];

                            key = nameof(ApplyEnableDefault);
                            if (settings.ContainsKey(key))
                                ApplyEnableDefault = settings[key];
                            OptimizeParameters = settings[key];

                            key = nameof(AutoSetupMenu);
                            if (settings.ContainsKey(key))
                                AutoSetupMenu = settings[key];

                            key = nameof(AutoSetupParams);
                            if (settings.ContainsKey(key))
                                AutoSetupParams = settings[key];

                            key = nameof(DefaultStatusLimitter);
                            if (settings.ContainsKey(key))
                                DefaultStatusLimitter = settings[key];

                            key = nameof(MaterialAnimationType);
                            if (settings.ContainsKey(key))
                                MaterialAnimationType = Enum.Parse(typeof(MaterialAnimationType), settings[key]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            SettingsLoaded?.Invoke();
        }
    }
}