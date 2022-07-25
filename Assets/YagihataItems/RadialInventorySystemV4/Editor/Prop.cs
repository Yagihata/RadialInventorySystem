using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV4
{
    [JsonObject]
    public class Prop
    {
        [JsonProperty] public bool IsDefaultEnabled { get; set; } = false;
        [JsonProperty] public bool IsLocalOnly { get; set; } = false;
        [JsonProperty] public GUIDPathPair<Texture2D> Icon { get; set; } = null;
        [JsonProperty] public GUIDPathPair<AnimationClip> EnableAnimation { get; set; } = null;
        [JsonProperty] public GUIDPathPair<AnimationClip> DisableAnimation { get; set; } = null;
        [JsonProperty] public string Name { get; set; } = "";
        [JsonProperty] public List<GUIDPathPair<GameObject>> TargetObjects { get; set; } = new List<GUIDPathPair<GameObject>>();
        [JsonConverter(typeof(StringEnumConverter))] [JsonProperty] public RIS.ExclusiveGroupType ExclusiveGroup { get; set; } = RIS.ExclusiveGroupType.None;
        [JsonProperty] public GUIDPathPair<Material> MaterialOverride { get; set; } = null;
        [JsonProperty] public bool UseResetTimer { get; set; } = false;
        [JsonProperty] public float ResetSecond { get; set; } = 0f;
        [JsonProperty] public bool UseSaveParameter { get; set; } = true;
        public string GetPropName(Avatar avatar)
        {
            if (!string.IsNullOrEmpty(Name))
                return Name;
            if(TargetObjects.Any() && TargetObjects.First() != null && avatar.AvatarRoot != null && avatar.AvatarRoot.GetObject() != null && avatar.AvatarRoot.GetObject().gameObject != null &&
                TargetObjects.First().GetObject(avatar.AvatarRoot.GetObject().gameObject) != null)
                return TargetObjects.First().GetObject(avatar.AvatarRoot.GetObject().gameObject).name;
            return "";
        }

        public Prop()
        {
            Icon = new GUIDPathPair<Texture2D>(ObjectPathStateType.Asset);
            EnableAnimation = new GUIDPathPair<AnimationClip>(ObjectPathStateType.Asset);
            DisableAnimation = new GUIDPathPair<AnimationClip>(ObjectPathStateType.Asset);
            MaterialOverride = new GUIDPathPair<Material>(ObjectPathStateType.Asset);
        }
        public void ForceReload(GameObject parent)
        {
            Icon.ForceReload();
            DisableAnimation.ForceReload();
            MaterialOverride.ForceReload();
            foreach(var v in TargetObjects)
                v.ForceReload(parent);
        }
    }
}