using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using UnityEditor;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV4
{
    public class AdsUpdater : ScriptableObject
    {
        public static string CurrentAdsURL = "";
        private static DateTime lastUpdateTime;
        [InitializeOnLoadMethod]
        public static void DoUpdate()
        {
            var now = DateTime.UtcNow;
            if (lastUpdateTime == null)
                lastUpdateTime = now;
            else if (now - lastUpdateTime < new TimeSpan(0, 10, 0))
                return;

            lastUpdateTime = now;
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
        }
    }
}