#if RISV4_JSON
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV4
{
    public static class RISStrings
    {
        static Dictionary<string, string> guids = new Dictionary<string, string>();
        static Dictionary<string, string> texts = new Dictionary<string, string>();
        static Dictionary<string, int[]> labelWidth = new Dictionary<string, int[]>();
        static int[] defaultWidth = new int[] { 200, 160, 100, 120, 80, 120 };
        [InitializeOnLoadMethod]
        [MenuItem("Radial Inventory/RISV4 Reload Languages")]
        public static void EditorInitialize()
        {
            try
            {
                var path = AssetDatabase.GUIDToAssetPath(RIS.LanguageFileGUID);
                TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (json != null)
                    guids = JsonConvert.DeserializeObject<Dictionary<string, string>>(json.text);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            try
            {
                var culture = CultureInfo.CurrentCulture.Name;
                if (guids.ContainsKey(culture)) 
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[culture]);
                    TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    if (json == null && guids.ContainsKey("default"))
                        json = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guids["default"]));
                    if (json != null)
                        texts = JsonConvert.DeserializeObject<Dictionary<string, string>>(json.text);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            try
            {
                var path = AssetDatabase.GUIDToAssetPath(RIS.WidthFileGUID);
                TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (json != null)
                    labelWidth = JsonConvert.DeserializeObject<Dictionary<string, int[]>>(json.text);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        public static string GetString(string key)
        {
            if (texts.ContainsKey(key))
                return texts[key];
            return "";
        }
        public static int GetWidth(int index)
        {
            if (index < 0 || index >= defaultWidth.Length)
                return 0;

            var cultureName = CultureInfo.CurrentCulture.Name;
            if (labelWidth.ContainsKey(cultureName) && index < labelWidth[cultureName].Length)
                return labelWidth[cultureName][index];
            cultureName = "default";
            if (labelWidth.ContainsKey(cultureName) && index < labelWidth[cultureName].Length)
                return labelWidth[cultureName][index];
            return defaultWidth[index];
        }
    }
}
#endif