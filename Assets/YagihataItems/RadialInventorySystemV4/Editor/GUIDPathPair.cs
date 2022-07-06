using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEditor;
using UnityEngine;
using YagihataItems.YagiUtils;

namespace YagihataItems.RadialInventorySystemV4
{
    [JsonObject]
    public class GUIDPathPair<T> where T : Object
    {
        [JsonProperty] public string ObjectGUID { get; set; }
        [JsonProperty] public string ObjectPath { get; set; }
        private ObjectPathStateType _objectPathState;
        [JsonConverter(typeof(StringEnumConverter))] [JsonProperty] public ObjectPathStateType ObjectPathState { get { return _objectPathState; } }
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
        private void SetObjectPath(T targetObject, GameObject parentObject = null)
        {
            if(targetObject != null)
            {
                if (ObjectPathState == ObjectPathStateType.Asset)
                    ObjectPath = AssetDatabase.GetAssetPath(targetObject);
                else if (ObjectPathState == ObjectPathStateType.Asset)
                    ObjectPath = (targetObject as GameObject).GetHierarchyPath(false);
                else if (ObjectPathState == ObjectPathStateType.RelativeFromObject)
                    ObjectPath = (targetObject as GameObject).GetRelativePath(parentObject);
            }
        }
        public T GetObject(GameObject parentObject = null)
        {
            if (objectCache != null)
            {
                if (ObjectPathState == ObjectPathStateType.Asset)
                {
                    var path = AssetDatabase.GUIDToAssetPath(ObjectPath);
                    var targetObject = AssetDatabase.LoadAssetAtPath(path, typeof(T));
                    if (targetObject == null)
                        targetObject = AssetDatabase.LoadAssetAtPath(ObjectPath, typeof(T));
                    if (targetObject is T)
                        objectCache = (T)targetObject;
                }
                else if (ObjectPathState == ObjectPathStateType.Scene)
                {
                    var instanceID = 0;
                    int.TryParse(ObjectGUID, out instanceID);
                    var targetObject = EditorUtility.InstanceIDToObject(instanceID);
                    if (targetObject == null || !(targetObject is GameObject))
                        targetObject = GameObject.Find(ObjectPath);
                    if (targetObject is T)
                        objectCache = (T)targetObject;
                }
                else if (ObjectPathState == ObjectPathStateType.RelativeFromObject)
                {
                    var instanceID = 0;
                    int.TryParse(ObjectGUID, out instanceID);
                    var targetObject = EditorUtility.InstanceIDToObject(instanceID);
                    if ((targetObject == null || !(targetObject is GameObject)) && (targetObject as GameObject).IsChildOf(parentObject))
                        targetObject = parentObject.transform.Find(ObjectPath)?.gameObject;
                    if (targetObject is T)
                        objectCache = (T)targetObject;
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
            else if(ObjectPathState == ObjectPathStateType.Scene && targetObject is GameObject)
            {
                var gameObject = targetObject as GameObject;
                ObjectPath = gameObject.GetHierarchyPath(false);
                ObjectGUID = gameObject.GetInstanceID().ToString();
            }
            else if(ObjectPathState == ObjectPathStateType.RelativeFromObject && targetObject is GameObject)
            {
                var gameObject = targetObject as GameObject;
                ObjectPath = gameObject.GetRelativePath(parentObject);
                ObjectGUID = gameObject.GetInstanceID().ToString();
            }
            else
            {
                ObjectPath = "";
                ObjectGUID = "";
            }
            objectCache = targetObject;
            Debug.Log($"SET OBJECT => {ObjectPath}, {ObjectGUID}, {ObjectPathState}");
        }
    }
    public enum ObjectPathStateType
    {
        Asset,
        Scene,
        RelativeFromObject
    }
}