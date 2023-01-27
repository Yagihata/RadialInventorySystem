using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace YagihataItems.RadialInventorySystemV4
{
    public static class UnityUtils
    {
        public static AnimationClip CreateAnimationClip(string path, float keyFrameValue, Type type, string propertyName, string clipName, string animationsFolder)
        {
            var clip = new AnimationClip();
            var curve = new AnimationCurve();
            curve.AddKey(0f, keyFrameValue);
            curve.AddKey(1f / clip.frameRate, keyFrameValue);
            clip.SetCurve(path, type, propertyName, curve);
            AssetDatabase.CreateAsset(clip, animationsFolder + clipName + ".anim");
            return clip;
        }
        public static bool IsChildOf(this GameObject target, GameObject parent)
        {
            Transform targetParent = target.transform.parent;
            while (targetParent != null)
            {
                if (targetParent == parent.transform)
                    return true;
                else
                    targetParent = targetParent.parent;
            }
            return false;
        }

        public static string GetRelativePath(this GameObject target, GameObject parent, bool escape = false)
        {
            if (!target.IsChildOf(parent))
                return null;
            var list = new List<string>();
            list.Add(escape ? UnityWebRequest.EscapeURL(target.name) : target.name);
            Transform targetParent = target.transform.parent;
            while (targetParent != null)
            {
                if (targetParent == parent.transform)
                    break;
                else
                {
                    list.Insert(0, escape ? UnityWebRequest.EscapeURL(targetParent.name) : targetParent.name);
                    targetParent = targetParent.parent;
                }
            }
            return string.Join("/", list);
        }
        public static void ReCreateFolder(string path)
        {
            path = path.Trim('/');
            DeleteFolder(path);
            CreateFolderRecursively(path);
        }
        public static void DeleteFolder(string path)
        {
            path = path.Trim('/');
            if (AssetDatabase.IsValidFolder(path))
                DeleteAssetDirectory(path);
        }
        public static bool DeleteAssetDirectory(string assetPath)
        {
            if (AssetDatabase.DeleteAsset(assetPath))
            {
                Debug.Log("Delete Asset: " + assetPath);
                return true;
            }

            string[] dirpathlist = Directory.GetDirectories(assetPath);
            foreach (string path in dirpathlist)
            {
                if (false == DeleteAssetDirectory(path))
                {
                    Debug.LogError("Delete Asset Directory Error: " + path);
                    return false;
                }
            }
            string[] filepathlist = Directory.GetFiles(assetPath);
            foreach (string path in filepathlist)
            {
                if (path.EndsWith(".meta"))
                {
                    continue;
                }

                if (false == AssetDatabase.DeleteAsset(path))
                {
                    Debug.LogError("Delete Asset Files Error: " + path);
                    return false;
                }
            }

            Debug.Log("Delete Asset: " + assetPath);
            return true;
        }
        public static void CreateFolderRecursively(string path)
        {
            Debug.Assert(path.StartsWith("Assets/"), "arg `path` of CreateFolderRecursively doesn't starts with `Assets/`");
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
        public static void GetGameObjectsOfType<T>(ref List<T> list, GameObject gameObject, bool getInactive)
        {
            if(getInactive || (!getInactive && gameObject.activeInHierarchy))
            {
                var component = gameObject.GetComponent(typeof(T));
                if (component != null)
                    list.Add((T)((object)component));
            }
            if(gameObject.transform.childCount > 0)
                foreach(var v in Enumerable.Range(0, gameObject.transform.childCount))
                {
                    GetGameObjectsOfType<T>(ref list, gameObject.transform.GetChild(v).gameObject, getInactive);
                }

        }
        public static string GetHierarchyPath(this GameObject target)
        {
            string path = "";
            Transform current = target.transform;
            while (current != null)
            {
                int index = current.GetSiblingIndex();
                path = "/" + current.name + index + path;
                current = current.parent;
            }

            Scene belongScene = target.GetBelongsScene();

            return "/" + belongScene.name + path;
        }
        public static string GetObjectPath(this GameObject target, bool escape = true)
        {
            string path = "";
            Transform current = target.transform;
            while (current != null)
            {
                if(escape)
                    path = "/" + UnityWebRequest.EscapeURL(current.name) + path;
                else
                    path = "/" + current.name + path;

                current = current.parent;
            }


            return path.Trim('/');
        }
        public static Scene GetBelongsScene(this GameObject target)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid())
                {
                    continue;
                }

                if (!scene.isLoaded)
                {
                    continue;
                }

                GameObject[] roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    if (root == target.transform.root.gameObject)
                    {
                        return scene;
                    }
                }
            }

            return default(Scene);
        }
        public static UnityEngine.Object TryGetAsset(string path, Type type, bool createNew = true)
        {
            var obj = AssetDatabase.LoadAssetAtPath(path, type);
            if (obj == null && createNew)
            {
                obj = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(obj, path);
            }
            return obj;
        }
    }
}
