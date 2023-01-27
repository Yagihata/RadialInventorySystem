
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV4
{
    public static class PackageInstaller
    {
        static RemoveRequest removeRequest;
        static ListRequest listRequest;

        [MenuItem("Radial Inventory/RISV4 Reload Packages")]
        public static void ReloadPackages()
        {
            CheckPackages();
        }
        [InitializeOnLoadMethod]
        static void CheckPackages()
        {
            Debug.Log("Check Package: Newtonsoft.Json");

            Type type = GetTypeByClassName("Newtonsoft.Json.JsonPropertyAttribute");
            if (type == null)
            {
                listRequest = Client.List();
                EditorApplication.update += UpdateList;
            }
            else
                ScriptingDefineSymbolsUtil.Add("RISV4_JSON");

        }
        public static Type GetTypeByClassName(string className)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.FullName == className)
                    {
                        return type;
                    }
                }
            }
            return null;
        }
        static void RemovePackage()
        {
            if (removeRequest.IsCompleted)
            {
                if (removeRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Removed: " + removeRequest.PackageIdOrName);
                    CheckPackages();
                }
                else if (removeRequest != null && removeRequest.Status >= StatusCode.Failure)
                    Debug.Log(removeRequest.Error.message);

                EditorApplication.update -= RemovePackage;
            }
        }
        static void UpdateList()
        {

            if (listRequest.IsCompleted)
            {
                if (listRequest.Status == StatusCode.Success)
                {
                    if (listRequest.Result.Cast<UnityEditor.PackageManager.PackageInfo>().Any(value => value.name == "com.unity.nuget.newtonsoft-json"))
                    {
                        ScriptingDefineSymbolsUtil.Add("RISV4_JSON");
                    }
                    else
                    {
                        ScriptingDefineSymbolsUtil.Remove("RISV4_JSON");
                        Debug.LogError("Newtonsoft-Json package is missing.");
                    }
                }
                else if (listRequest.Status >= StatusCode.Failure)
                    Debug.Log(listRequest.Error.message);

                EditorApplication.update -= UpdateList;
            }
        }
    }
}