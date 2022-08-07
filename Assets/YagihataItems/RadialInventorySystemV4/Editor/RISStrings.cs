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
        public static string CurrentAdsURL = "";
        [InitializeOnLoadMethod]
        [MenuItem("Radial Inventory/Reload Languages")]
        static void EditorInitialize()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    CurrentAdsURL = "";
                    TexAssets.AdsTexture = null;
                    var result = client.GetAsync(RIS.AdsUrl).GetAwaiter().GetResult();
                    var json = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                        var image_name = (string)response["image_name"];
                        var url = (string)response["url"];
                        var status = (bool)response["result"];
                        if (status)
                        {
                            var extension = Path.GetExtension(image_name);
                            var downloadFolderName = $"{RIS.WorkFolderPath}ad_images/";
                            var downloadPath = $"{downloadFolderName}image{extension}";
                            if (!Directory.Exists(downloadFolderName))
                                Directory.CreateDirectory(downloadFolderName);
                            if (File.Exists(downloadPath))
                                File.Delete(downloadPath);
                            using (var wc = new WebClient())
                                wc.DownloadFile($"{RIS.AdsUrl}/images/{image_name}", downloadPath);
                            AssetDatabase.ImportAsset(downloadPath);
                            var importer = AssetImporter.GetAtPath(downloadPath) as TextureImporter;
                            importer.textureType = TextureImporterType.Sprite;
                            AssetDatabase.WriteImportSettingsIfDirty(downloadPath);
                            importer.SaveAndReimport();
                            AssetDatabase.SaveAssets();
                            TexAssets.AdsTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(downloadPath);
                            CurrentAdsURL = url;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            try
            {
                TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>($"{RIS.WorkFolderPath}LanguageFiles/RISV4_{CultureInfo.CurrentCulture.Name}.json");
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
