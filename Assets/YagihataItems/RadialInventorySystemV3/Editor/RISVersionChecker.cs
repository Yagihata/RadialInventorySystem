using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV3
{
    class RISVersionChecker
    {
        const string TMP_FILE_PATH = "Temp/RISStartupFlag";
        public static string GetNewerVersion()
        {

            TextAsset newVersionTxt = AssetDatabase.LoadAssetAtPath<TextAsset>(RISV3.WorkFolderPath + "newerVersion.txt");
            if (newVersionTxt != null)
                return newVersionTxt.text.Trim();
            else
                return "";
        }
        [InitializeOnLoadMethod]
        static void EditorInitialize()
        {
            if(!File.Exists(TMP_FILE_PATH) || !File.Exists(RISV3.WorkFolderPath + "newerVersion.txt"))
            {
                File.Create(TMP_FILE_PATH);
                CheckNewerVersion();
            }
        }
        private static void CheckNewerVersion()
        {
            using (var wc = new WebClient())
            {
                try
                {
                    string text = wc.DownloadString(RISV3.VersionUrl);
                    var newerVersion = text.Trim();
                    Debug.Log(newerVersion);
                    File.WriteAllText(RISV3.WorkFolderPath + "newerVersion.txt", newerVersion);
                }
                catch (WebException exc)
                {
                    Debug.Log(exc.ToString());
                }
            }
        }
    }
}
