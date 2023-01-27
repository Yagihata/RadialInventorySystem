using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV4
{
    class DonatorListUpdater
    {
        const string TMP_FILE_PATH = "Temp/RISDonatorCheckedFlag";
        public static string GetDonators()
        {
            TextAsset donators = AssetDatabase.LoadAssetAtPath<TextAsset>(RIS.WorkFolderPath + "donators.txt");
            if (donators != null)
                return donators.text.Trim();
            else
                return "";
        }
        [InitializeOnLoadMethod]
        static void EditorInitialize()
        {
            if (!File.Exists(TMP_FILE_PATH) || !File.Exists(RIS.WorkFolderPath + "donators.txt"))
            {
                File.Create(TMP_FILE_PATH);
                using (var wc = new WebClient())
                {
                    try
                    {
                        string text = wc.DownloadString(RIS.DonatorListUrl);
                        var lines = new List<string>();
                        var line = "";
                        using (var sr = new StringReader(text))
                            while ((line = sr.ReadLine()) != null)
                                lines.Add(line);
                        if (lines.Count == 2 && lines[0].Trim() == "ris_donators_list")
                            File.WriteAllText(RIS.WorkFolderPath + "donators.txt", lines[1].Trim());
                        AssetDatabase.Refresh();
                    }
                    catch (WebException exc)
                    {
                        Debug.Log(exc.ToString());
                    }
                }
            }
        }
    }
}
