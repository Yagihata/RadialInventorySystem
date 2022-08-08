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
        static Dictionary<string, string> texts = new Dictionary<string, string>();
        static Dictionary<string, int[]> labelWidth = new Dictionary<string, int[]>();
        static int[] defaultWidth = new int[] { 200, 110, 100, 100, 80, 90 };
        [InitializeOnLoadMethod]
        [MenuItem("Radial Inventory/Reload Languages")]
        public static void EditorInitialize()
        {
            try
            {
                TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>($"{RIS.WorkFolderPath}LanguageFiles/RISV4_{CultureInfo.CurrentCulture.Name}.json");
                if (json == null)
                    json = AssetDatabase.LoadAssetAtPath<TextAsset>($"{RIS.WorkFolderPath}LanguageFiles/RISV4_default.json");
                if (json != null)
                    texts = JsonConvert.DeserializeObject<Dictionary<string, string>>(json.text);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            try
            {
                TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>($"{RIS.WorkFolderPath}LanguageFiles/RISV4_width.json");
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
            return defaultWidth[index];
        }
    }
}
#endif