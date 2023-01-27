using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV4
{
    public class YagiAPI
    {
        public static void TryDeleteAsset(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
                AssetDatabase.DeleteAsset(path);
        }
        public static void CreateFolderRecursively(string path)
        {
            UnityEngine.Debug.Assert(path.StartsWith("Assets/"), "arg `path` of CreateFolderRecursively doesn't starts with `Assets/`");
            if (AssetDatabase.IsValidFolder(path)) return;
            if (path[path.Length - 1] == '/')
            {
                path = path.Substring(0, path.Length - 1);
            }
            var names = path.Split('/');
            for (int i = 1; i < names.Length; i++)
            {
                var parent = string.Join("/", names.Take(i).ToArray());
                var target = string.Join("/", names.Take(i + 1).ToArray());
                var child = names[i];
                if (!AssetDatabase.IsValidFolder(target))
                {
                    AssetDatabase.CreateFolder(parent, child);
                }
            }
        }
        public static string GetGameObjectPath(GameObject obj, GameObject parent = null, int recCount = -1)
        {
            string path = "/" + obj.name;
            int i = 0;
            string parentpath = "";
            if (parent != null)
                parentpath = GetGameObjectPath(parent);
            while (obj.transform.parent != null && (recCount == -1 || i < recCount))
            {
                ++i;
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            if (!string.IsNullOrEmpty(parentpath))
            {
                if (path.Contains(parentpath + "/"))
                    path = path.Replace(parentpath + "/", "");
                else
                    path = "";
            }
            return path.TrimStart('/');
        }
        public static void UpdateProgressBar(string text, int count, int max)
        {
            float progress = (float)count / max;
            EditorUtility.DisplayProgressBar(text, count.ToString() + "/" + max.ToString() + " (" + (progress * 100) + "%)", progress);
        }
        public static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
