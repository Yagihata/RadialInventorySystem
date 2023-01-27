#if RISV4_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace YagihataItems.RadialInventorySystemV4
{
    [JsonObject]
    public class GUIDPathPair<T> : System.IEquatable<GUIDPathPair<T>> where T : Object
    {
        [JsonProperty] public string ObjectGUID { get; set; }
        [JsonProperty] private string ObjectPath { get; set; }
        private ObjectPathStateType _objectPathState;
        [JsonConverter(typeof(StringEnumConverter))] [JsonProperty] public ObjectPathStateType ObjectPathState { get { return _objectPathState; } private set { _objectPathState = value; } }
        private T objectCache = null;
        public GUIDPathPair(ObjectPathStateType type)
        {
            _objectPathState = type;
        }
        public GUIDPathPair()
        {
            _objectPathState = ObjectPathStateType.Asset;
        }
        public GUIDPathPair(ObjectPathStateType type, T initializeObject, GameObject parentObject = null)
        {
            _objectPathState = type;
            SetObject(initializeObject, parentObject);
        }
        public GUIDPathPair(T initializeObject, GameObject parentObject = null)
        {
            _objectPathState = ObjectPathStateType.Asset;
            SetObject(initializeObject, parentObject);
        }
        public string GetFormattedPath()
        {
            return UnityWebRequest.UnEscapeURL(ObjectPath);
        }
        public void SetFormattedPath(string path)
        {
            if (ObjectPathState == ObjectPathStateType.Asset)
                ObjectPath = path;
            else if (ObjectPathState == ObjectPathStateType.Scene)
                UnityWebRequest.EscapeURL(path);
            else if (ObjectPathState == ObjectPathStateType.RelativeFromObject)
                UnityWebRequest.EscapeURL(path);
        }
        private void SetObjectPath(T targetObject, GameObject parentObject = null)
        {
            if(targetObject != null)
            {
                if (ObjectPathState == ObjectPathStateType.Asset)
                    ObjectPath = AssetDatabase.GetAssetPath(targetObject);
                else if (ObjectPathState == ObjectPathStateType.Scene)
                {
                    if (targetObject is GameObject)
                        ObjectPath = (targetObject as GameObject).GetObjectPath();
                    else if(targetObject is MonoBehaviour)
                        ObjectPath = (targetObject as MonoBehaviour).gameObject.GetObjectPath();
                }
                else if (ObjectPathState == ObjectPathStateType.RelativeFromObject)
                {
                    if (targetObject is GameObject)
                        ObjectPath = (targetObject as GameObject).GetRelativePath(parentObject, true);
                    else if (targetObject is MonoBehaviour)
                        ObjectPath = (targetObject as MonoBehaviour).gameObject.GetRelativePath(parentObject, true);
                }
            }
        }
        public T GetObject(GameObject parentObject = null)
        {
            if (string.IsNullOrEmpty(ObjectPath))
                return null;
            if (objectCache == null)
            {
                if (ObjectPathState == ObjectPathStateType.Asset)
                {
                    var path = AssetDatabase.GUIDToAssetPath(ObjectGUID);
                    var targetObject = AssetDatabase.LoadAssetAtPath(path, typeof(T));
                    if (targetObject == null)
                        targetObject = AssetDatabase.LoadAssetAtPath(ObjectPath, typeof(T));
                    if (targetObject is T)
                        objectCache = targetObject as T;
                }
                else if (ObjectPathState == ObjectPathStateType.Scene)
                {
                    var instanceID = 0;
                    int.TryParse(ObjectGUID, out instanceID);
                    var targetObject = EditorUtility.InstanceIDToObject(instanceID);
                    if (targetObject != null && targetObject is GameObject && typeof(MonoBehaviour).IsAssignableFrom(typeof(T)))
                    {
                        var gameObj = (targetObject as GameObject);
                        targetObject = gameObj.GetComponent<T>();
                    }
                    if (targetObject == null || !(targetObject is T))
                    {
                        var names = ObjectPath.Split('/');
                        GameObject gameObject = null;
                        if (names.Length > 0)
                        {
                            gameObject = GameObject.Find(UnityWebRequest.UnEscapeURL(names[0]));
                            if (gameObject != null)
                            {
                                for (int i = 1; i < names.Length; i++)
                                {
                                    var name = names[i];
                                    var decodedName = UnityWebRequest.UnEscapeURL(name);
                                    gameObject = gameObject.transform.Find(decodedName)?.gameObject;
                                    if (gameObject == null)
                                        break;
                                }
                            }
                        }
                        targetObject = gameObject;
                    }
                    if (targetObject is T)
                        objectCache = targetObject as T;
                    else if(targetObject != null && targetObject is GameObject && typeof(MonoBehaviour).IsAssignableFrom(typeof(T)))
                    {
                        var gameObj = (targetObject as GameObject);
                        targetObject = gameObj.GetComponent<T>();
                        objectCache = targetObject as T;
                    }
                }
                else if (ObjectPathState == ObjectPathStateType.RelativeFromObject)
                {
                    var instanceID = 0;
                    int.TryParse(ObjectGUID, out instanceID);
                    var targetObject = EditorUtility.InstanceIDToObject(instanceID);
                    if (targetObject != null && targetObject is GameObject && typeof(MonoBehaviour).IsAssignableFrom(typeof(T)))
                    {
                        var gameObj = (targetObject as GameObject);
                        targetObject = gameObj.GetComponent<T>();
                    }
                    if (targetObject == null)
                    {
                        var names = ObjectPath.Split('/');
                        GameObject gameObject = null;
                        if (names.Length > 0)
                        {
                            gameObject = parentObject;
                            if (gameObject != null)
                            {
                                for (int i = 0; i < names.Length; i++)
                                {
                                    var name = names[i];
                                    var decodedName = UnityWebRequest.UnEscapeURL(name);
                                    gameObject = gameObject.transform.Find(decodedName)?.gameObject;
                                    if (gameObject == null)
                                        break;
                                }
                            }
                            targetObject = gameObject;
                        }
                    }
                    if (!(targetObject is T))
                    {
                        GameObject gameObj = null;
                        if (targetObject is GameObject)
                            gameObj = targetObject as GameObject;
                        else if (targetObject is MonoBehaviour)
                            gameObj = (targetObject as MonoBehaviour).gameObject;
                        if (gameObj != null && gameObj.IsChildOf(parentObject))
                        {
                            var names = ObjectPath.Split('/');
                            GameObject gameObject = null;
                            if (names.Length > 0)
                            {
                                gameObject = parentObject;
                                if (gameObject != null)
                                {
                                    for (int i = 0; i < names.Length; i++)
                                    {
                                        var name = names[i];
                                        var decodedName = UnityWebRequest.UnEscapeURL(name);
                                        gameObject = gameObject.transform.Find(decodedName)?.gameObject;
                                        if (gameObject == null)
                                            break;
                                    }
                                }
                                targetObject = gameObject;
                            }
                        }
                    }
                    if (targetObject is T)
                        objectCache = targetObject as T;
                    else if (targetObject != null && targetObject is GameObject && typeof(MonoBehaviour).IsAssignableFrom(typeof(T)))
                    {
                        var gameObj = (targetObject as GameObject);
                        targetObject = gameObj.GetComponent<T>();
                        objectCache = targetObject as T;
                    }
                }
            }
            SetObjectPath(objectCache, parentObject);
            return objectCache;

        }
        public T ForceReload(GameObject parentObject = null)
        {
            if (string.IsNullOrEmpty(ObjectPath))
                return null;
            if (ObjectPathState == ObjectPathStateType.Asset)
            {
                var targetObject = AssetDatabase.LoadAssetAtPath(ObjectPath, typeof(T));
                if (targetObject is T)
                    objectCache = targetObject as T;
            }
            else if (ObjectPathState == ObjectPathStateType.Scene)
            {
                var names = ObjectPath.Split('/');
                GameObject gameObject = null;
                if (names.Length > 0)
                {
                    gameObject = GameObject.Find(UnityWebRequest.UnEscapeURL(names[0]));
                    if (gameObject != null)
                    {
                        for (int i = 1; i < names.Length; i++)
                        {
                            var name = names[i];
                            var decodedName = UnityWebRequest.UnEscapeURL(name);
                            gameObject = gameObject.transform.Find(decodedName)?.gameObject;
                            if (gameObject == null)
                                break;
                        }
                    }
                }
                Object targetObject = gameObject;
                if (targetObject is T)
                    objectCache = targetObject as T;
                else if (targetObject != null && targetObject is GameObject && typeof(MonoBehaviour).IsAssignableFrom(typeof(T)))
                {
                    var gameObj = (targetObject as GameObject);
                    targetObject = gameObj.GetComponent<T>();
                    objectCache = targetObject as T;
                }
            }
            else if (ObjectPathState == ObjectPathStateType.RelativeFromObject)
            {
                Object targetObject = parentObject.transform.Find(ObjectPath)?.gameObject; 
                var names = ObjectPath.Split('/');
                GameObject gameObject = null;
                if (names.Length > 0)
                {
                    gameObject = parentObject;
                    if (gameObject != null)
                    {
                        for (int i = 0; i < names.Length; i++)
                        {
                            var name = names[i];
                            var decodedName = UnityWebRequest.UnEscapeURL(name);
                            gameObject = gameObject.transform.Find(decodedName)?.gameObject;
                            if (gameObject == null)
                                break;
                        }
                    }
                    targetObject = gameObject;
                }
                if (targetObject == null)
                {
                    var foundObjects = GameObject.FindObjectsOfType<T>();
                    GameObject foundObjectFixed = null;
                    GameObject foundObjectLoose = null;
                    var pathLooseName = UnityWebRequest.UnEscapeURL(ObjectPath.Split('/').Last());
                    foreach (var obj in foundObjects)
                    {
                        GameObject gameObj = null;
                        if (obj is GameObject)
                            gameObj = obj as GameObject;
                        else if (targetObject is MonoBehaviour)
                            gameObj = (obj as MonoBehaviour).gameObject;
                        if (gameObj != null && parentObject != null && gameObj.IsChildOf(parentObject))
                        {
                            var path = gameObj.GetRelativePath(parentObject, true);
                            var pathArr = path.Split('/');
                            if (pathArr.Length > 0 && pathArr.Last() == ObjectPath)
                            {
                                foundObjectFixed = gameObj;
                                break;
                            }
                            if(gameObj.name == pathLooseName)
                            {
                                foundObjectLoose = gameObj;
                            }
                        }
                    }
                    if (foundObjectFixed != null)
                    {
                        targetObject = foundObjectFixed;
                    }
                    else if (foundObjectLoose != null)
                    {
                        targetObject = foundObjectLoose;
                    }
                }
                if (!(targetObject is T))
                {
                    GameObject gameObj = null;
                    if (targetObject is GameObject)
                        gameObj = targetObject as GameObject;
                    else if (targetObject is MonoBehaviour)
                        gameObj = (targetObject as MonoBehaviour).gameObject;
                    if (gameObj != null && gameObj.IsChildOf(parentObject))
                    {
                        names = ObjectPath.Split('/');
                        gameObject = null;
                        if (names.Length > 0)
                        {
                            gameObject = parentObject;
                            if (gameObject != null)
                            {
                                for (int i = 0; i < names.Length; i++)
                                {
                                    var name = names[i];
                                    var decodedName = UnityWebRequest.UnEscapeURL(name);
                                    gameObject = gameObject.transform.Find(decodedName)?.gameObject;
                                    if (gameObject == null)
                                        break;
                                }
                            }
                            targetObject = gameObject;
                        }
                    }
                }
                if (targetObject is T)
                    objectCache = targetObject as T;
                else if (targetObject != null && targetObject is GameObject && typeof(MonoBehaviour).IsAssignableFrom(typeof(T)))
                {
                    var gameObj = (targetObject as GameObject);
                    targetObject = gameObj.GetComponent<T>();
                    objectCache = targetObject as T;
                }
            }
            SetObjectPath(objectCache, parentObject);
            return objectCache;
        }
        public void SetObject(T targetObject, GameObject parentObject = null)
        {
            if (targetObject == null)
            {
                ObjectPath = "";
                ObjectGUID = "";
            }
            else if(ObjectPathState == ObjectPathStateType.Asset)
            {
                ObjectPath = AssetDatabase.GetAssetPath(targetObject);
                ObjectGUID = AssetDatabase.AssetPathToGUID(ObjectPath);
            }
            else if(ObjectPathState == ObjectPathStateType.Scene)
            {
                GameObject gameObject = null;
                if(targetObject is GameObject)
                    gameObject = targetObject as GameObject;
                else if(targetObject is MonoBehaviour)
                {
                    gameObject = (targetObject as MonoBehaviour).gameObject;
                }
                ObjectPath = gameObject.GetObjectPath();
                ObjectGUID = gameObject.GetInstanceID().ToString();
            }
            else if(ObjectPathState == ObjectPathStateType.RelativeFromObject)
            {
                GameObject gameObject = null;
                if (targetObject is GameObject)
                    gameObject = targetObject as GameObject;
                else if (targetObject is MonoBehaviour)
                {
                    gameObject = (targetObject as MonoBehaviour).gameObject;
                }
                ObjectPath = gameObject.GetRelativePath(parentObject, true);
                ObjectGUID = gameObject.GetInstanceID().ToString();
            }
            else
            {
                ObjectPath = "";
                ObjectGUID = "";
            }
            objectCache = targetObject;
        }

        public override bool Equals(object obj)
        {
            return this == obj;

        }

        public override int GetHashCode()
        {
            int hashCode = -1314013034;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ObjectGUID);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ObjectPath);
            hashCode = hashCode * -1521134295 + ObjectPathState.GetHashCode();
            return hashCode;
        }

        public bool Equals(GUIDPathPair<T> other)
        {
            return this == other;
        }

        public static bool operator ==(GUIDPathPair<T> a, object valueB)
        {
            // 同一のインスタンスを参照している場合は true
            if (System.Object.ReferenceEquals(a, valueB))
            {
                return true;
            }
            // どちらか片方でも null なら false
            if (((object)a == null) || ((object)valueB == null))
            {
                return false;
            }
            if (valueB is GUIDPathPair<T>)
            {
                var b = (GUIDPathPair<T>)valueB;
                if (a.ObjectGUID == b.ObjectGUID)
                    return true;
                else if (a.ObjectPath == b.ObjectPath)
                    return true;
            }
            else if(valueB is T)
            {
                var b = (T)valueB;
                if (a.GetObject() == b)
                    return true;
                else
                {
                    if (a.ObjectPathState == ObjectPathStateType.Asset)
                    {
                        var objectPath = AssetDatabase.GetAssetPath(b);
                        var objectGUID = AssetDatabase.AssetPathToGUID(objectPath);
                        if (a.ObjectGUID == objectGUID)
                            return true;
                        else if (a.ObjectPath == objectPath)
                            return true;
                    }
                    else if (a.ObjectPathState == ObjectPathStateType.Scene)
                    {
                        GameObject gameObject = null;
                        if (b is GameObject)
                            gameObject = b as GameObject;
                        else if (b is MonoBehaviour)
                        {
                            gameObject = (b as MonoBehaviour).gameObject;
                        }
                        var objectPath = gameObject.GetObjectPath();
                        var objectGUID = gameObject.GetInstanceID().ToString();
                        if (a.ObjectGUID == objectGUID)
                            return true;
                        else if (a.ObjectPath == objectPath)
                            return true;
                    }
                    else if (a.ObjectPathState == ObjectPathStateType.RelativeFromObject)
                    {
                        GameObject gameObject = null;
                        if (b is GameObject)
                            gameObject = b as GameObject;
                        else if (b is MonoBehaviour)
                        {
                            gameObject = (b as MonoBehaviour).gameObject;
                        }
                        var objectPath = gameObject.GetObjectPath();
                        var objectGUID = gameObject.GetInstanceID().ToString();
                        if (a.ObjectGUID == objectGUID)
                            return true;
                        else if (objectPath.Contains(a.ObjectPath))
                            return true;
                    }
                }
            }
            return false;
        }
        public static bool operator !=(GUIDPathPair<T> a, object b)
        {
            return !(a == b);
        }
    }
    public enum ObjectPathStateType
    {
        Asset,
        Scene,
        RelativeFromObject
    }
}
#endif