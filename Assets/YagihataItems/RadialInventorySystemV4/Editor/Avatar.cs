using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace YagihataItems.RadialInventorySystemV4
{
    [JsonObject]
    public class Avatar
    {
        [JsonProperty] public string UniqueID { get; set; }
        [JsonProperty] public GUIDPathPair<VRCAvatarDescriptor> AvatarRoot { get; set; }
        [JsonProperty] public bool UseWriteDefaults { get; set; } = false;
        [JsonProperty] public bool OptimizeParameters { get; set; } = true;
        [JsonProperty] public bool ApplyEnableDefault { get; set; } = true;
        [JsonProperty] public List<Group> Groups { get; set; } = new List<Group>();
        private RIS.ExclusiveModeType[] _exclusiveModes;
        [JsonConverter(typeof(StringEnumConverter))] [JsonProperty] public RIS.ExclusiveModeType[] ExclusiveModes { get { return _exclusiveModes; } set { _exclusiveModes = value; } }

        [JsonConverter(typeof(StringEnumConverter))] [JsonProperty] public RIS.MenuModeType MenuMode = RIS.MenuModeType.Simple;
        public Avatar()
        {
            AvatarRoot = new GUIDPathPair<VRCAvatarDescriptor>(ObjectPathStateType.Scene);
            
        }
        public void SetExclusiveMode(RIS.ExclusiveGroupType group, RIS.ExclusiveModeType type)
        {
            ResizeExclusiveModesArray();
            if (group != RIS.ExclusiveGroupType.None)
                ExclusiveModes[(int)group] = type;
        }
        public RIS.ExclusiveModeType GetExclusiveMode(RIS.ExclusiveGroupType group)
        {
            ResizeExclusiveModesArray();
            if (group == RIS.ExclusiveGroupType.None)
                return RIS.ExclusiveModeType.None;
            return ExclusiveModes[(int)group];
        }
        private void ResizeExclusiveModesArray()
        {
            var modeCount = Enum.GetNames(typeof(RIS.ExclusiveGroupType)).Length - 1;
            if (ExclusiveModes == null)
                ExclusiveModes = new RIS.ExclusiveModeType[modeCount];
            else if (ExclusiveModes.Length != modeCount)
                Array.Resize(ref _exclusiveModes, modeCount);
        }
    }
}