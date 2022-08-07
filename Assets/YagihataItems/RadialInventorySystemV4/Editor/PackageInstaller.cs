
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV4
{
    public static class PackageInstaller
    {
        static AddRequest addRequest;
        static ListRequest listRequest;
        [InitializeOnLoadMethod]
        static void CheckPackages()
        {
            Debug.Log("Check Package: Newtonsoft.Json");

            listRequest = Client.List();
            EditorApplication.update += UpdateList;

        }
        static void AddPackage()
        {
            if (addRequest.IsCompleted)
            {
                if (addRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Installed: " + addRequest.Result.packageId);
                    ScriptingDefineSymbolsUtil.Add("RISV4_JSON");
                }
                else if (addRequest.Status >= StatusCode.Failure)
                    Debug.Log(addRequest.Error.message);

                EditorApplication.update -= AddPackage;
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
                        addRequest = Client.Add("com.unity.nuget.newtonsoft-json");
                        EditorApplication.update += AddPackage;
                    }
                }
                else if (listRequest.Status >= StatusCode.Failure)
                    Debug.Log(listRequest.Error.message);

                EditorApplication.update -= UpdateList;
            }
        }
    }
}