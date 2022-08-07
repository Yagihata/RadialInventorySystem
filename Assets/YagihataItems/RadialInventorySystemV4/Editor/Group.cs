#if RISV4_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace YagihataItems.RadialInventorySystemV4
{
    [JsonObject]
    public class Group
    {
        [JsonProperty] public List<Prop> Props { get; set; } = new List<Prop>();
        [JsonProperty] public string Name { get; set; } = "";
        [JsonProperty] public GUIDPathPair<Texture2D> Icon { get; set; } = null;
        [JsonProperty] public GUIDPathPair<VRCExpressionsMenu> BaseMenu { get; set; } = null;
        [JsonProperty] public bool UseResetButton { get; set; } = false;
        public Group()
        {
            Icon = new GUIDPathPair<Texture2D>(ObjectPathStateType.Asset);
            BaseMenu = new GUIDPathPair<VRCExpressionsMenu>(ObjectPathStateType.Asset);
        }
        public void ForceReload(GameObject parent)
        {
            Icon.ForceReload();
            BaseMenu.ForceReload();
            foreach (var v in Props)
                v.ForceReload(parent);
        }
    }
}
#endif