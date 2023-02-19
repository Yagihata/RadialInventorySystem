﻿#if RISV4_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace YagihataItems.RadialInventorySystemV4
{
    [JsonObject]
    public class Avatar
    {
        [JsonProperty] public string Version { get; private set; }
        [JsonProperty] public string UniqueID { get; private set; }
        [JsonProperty] public GUIDPathPair<VRCAvatarDescriptor> AvatarRoot { get; set; }
        [JsonProperty] public bool UseWriteDefaults { get; set; } = false;
        [JsonProperty] private bool OptimizeParameters { get; set; } = true;
        [JsonProperty] private bool ApplyEnableDefault { get; set; } = true;
        [JsonProperty] public List<Group> Groups { get; set; } = new List<Group>();
        private RIS.ExclusiveModeType[] _exclusiveModes;
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))] public RIS.ExclusiveModeType[] ExclusiveModes { get { return _exclusiveModes; } set { _exclusiveModes = value; } }
        //ExclusiveModesプロパティのシリアライズ時に名前がchangeTypesになっていたため、それを修正するためのやつ。
        //privateでデリシアイズだけ行うchangeTypesプロパティを用意して、正しいフィールドへ横流しする。
        [JsonProperty("changeTypes", ItemConverterType = typeof(StringEnumConverter))] private RIS.ExclusiveModeType[] changeTypes { get { return _exclusiveModes; } set { _exclusiveModes = value; } }
        private GUIDPathPair<AnimationClip>[] _exclusiveDisableClips;
        public GUIDPathPair<AnimationClip>[] ExclusiveDisableClips { get { return _exclusiveDisableClips; } set { _exclusiveDisableClips = value; } }

        [JsonConverter(typeof(StringEnumConverter))] [JsonProperty] public RIS.MenuModeType MenuMode = RIS.MenuModeType.Simple;
        public DateTime LastWriteDate;
        public Avatar()
        {
            AvatarRoot = new GUIDPathPair<VRCAvatarDescriptor>(ObjectPathStateType.Scene);
            UniqueID = Guid.NewGuid().ToString();
            LastWriteDate = DateTime.UtcNow;
            Version = RIS.CurrentVersion;
        }
        //changeTypesをシリアライズしないためのやつ。
        public bool ShouldSerializechangeTypes()
        {
            return false;
        }
        public VRCAvatarDescriptor GetAvatarRoot()
        {
            return AvatarRoot?.GetObject();
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
        public void SetExclusiveDisableClip(RIS.ExclusiveGroupType group, AnimationClip clip)
        {
            ResizeExclusiveDisableClipsArray();
            if (group != RIS.ExclusiveGroupType.None)
                ExclusiveDisableClips[(int)group] = new GUIDPathPair<AnimationClip>(ObjectPathStateType.Asset, clip);
        }
        public AnimationClip GetExclusiveDisableClip(RIS.ExclusiveGroupType group)
        {
            ResizeExclusiveDisableClipsArray();
            if (group == RIS.ExclusiveGroupType.None)
                return null;
            return ExclusiveDisableClips[(int)group]?.GetObject();
        }
        private void ResizeExclusiveDisableClipsArray()
        {
            var modeCount = Enum.GetNames(typeof(RIS.ExclusiveGroupType)).Length - 1;
            if (ExclusiveDisableClips == null)
                ExclusiveDisableClips = new GUIDPathPair<AnimationClip>[modeCount];
            else if (ExclusiveDisableClips.Length != modeCount)
                Array.Resize(ref _exclusiveDisableClips, modeCount);
        }
        public void SaveToJson()
        {
            if (string.IsNullOrEmpty(UniqueID))
                UniqueID = Guid.NewGuid().ToString();
            var folderPath = RIS.AutoGeneratedFolderPath + UniqueID + "/";
            var jsonPath = $"{folderPath}{RIS.SettingsFileName}";
            if (!AssetDatabase.IsValidFolder(folderPath))
                UnityUtils.CreateFolderRecursively(folderPath);

            Version = RIS.CurrentVersion;
            var jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var sw = new StreamWriter(jsonPath, false, Encoding.UTF8))
            {
                sw.Write(jsonData);
            }
        }
        public void SaveToJson(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath))
                return;
            var dirPath = Path.GetDirectoryName(jsonPath);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            Version = RIS.CurrentVersion;
            var jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var sw = new StreamWriter(jsonPath, false, Encoding.UTF8))
            {
                sw.Write(jsonData);
            }
        }
        public static Avatar LoadFromJson(string path)
        {
            var avatar = new Avatar();

            if (!File.Exists(path))
                throw new FileNotFoundException();

            using (var sr = new StreamReader(path, Encoding.UTF8))
            {
                var jsonData = sr.ReadToEnd();
                avatar = JsonConvert.DeserializeObject<Avatar>(jsonData);
            }
            if (string.IsNullOrEmpty(avatar.Version))
            {
                Debug.LogWarning($"THIS AVATAR DATA IS BUILD FOR OLD VERSION! => {avatar.UniqueID}");
                //過去バージョンのバグ修正処理
                var oldPath = avatar.AvatarRoot.GetFormattedPath();
                var lastChar = oldPath.Last();
                while (lastChar >= '0' && lastChar <= '9')
                {
                    oldPath = oldPath.Remove(oldPath.Length - 1, 1);
                    lastChar = oldPath.Last();
                }
                avatar.AvatarRoot.SetFormattedPath(oldPath);
                avatar.LastWriteDate = File.GetLastWriteTimeUtc(path);
            }
            avatar.ForceReload();
            return avatar;
        }
        public void ForceReload()
        {
            AvatarRoot.ForceReload();
            var parent = AvatarRoot?.GetObject();
            if (parent != null)
            {
                foreach (var v in Groups)
                    v.ForceReload(parent.gameObject);
            }
        }
    }
}
#endif