#if RISV4_JSON
#if RISV4_V3
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YagihataItems.RadialInventorySystemV3;

namespace YagihataItems.RadialInventorySystemV4
{
    public class DataSalvager
    {
        public static void SalvageDatas(string v3SettingsName)
        {
            var avatars = new List<Avatar>();
            var settingsContainerRoot = GameObject.Find(v3SettingsName);
            if(settingsContainerRoot != null)
            {
                var containerTransform = settingsContainerRoot.transform;
                for (int i = 0; i < containerTransform.childCount; ++i)
                {
                    var child = containerTransform.GetChild(i);
                    Debug.Log($"DS:Settings name is {child.name}.");
                    try
                    {
                        var settings = child.gameObject.GetComponent<RISSettings>();
                        if (settings != null)
                        {
                            Debug.Log($"DS:Settings is not null.");
                            var v4Avatar = new Avatar();
                            if (settings.AvatarRoot == null)
                                continue;
                            Debug.Log($"DS:AvatarRoot is not null.");
                            v4Avatar.AvatarRoot.SetObject(settings.AvatarRoot);
                            v4Avatar.ApplyEnableDefault = settings.ApplyEnabled;
                            v4Avatar.UseWriteDefaults = settings.WriteDefaults;
                            v4Avatar.OptimizeParameters = settings.OptimizeParams;
                            if (settings.MenuMode == RISV3.RISMode.Simple)
                                v4Avatar.MenuMode = RIS.MenuModeType.Basic;
                            else if (settings.MenuMode == RISV3.RISMode.Advanced)
                                v4Avatar.MenuMode = RIS.MenuModeType.Advanced;
                            for (int v = 1; v < settings.AdvancedGroupMode.Length; ++v)
                            {
                                var v2Mode = settings.AdvancedGroupMode[v] == 1;
                                if (v2Mode)
                                    v4Avatar.SetExclusiveMode((RIS.ExclusiveGroupType)(v - 1), RIS.ExclusiveModeType.LegacyExclusive);
                                else
                                    v4Avatar.SetExclusiveMode((RIS.ExclusiveGroupType)(v - 1), RIS.ExclusiveModeType.None);
                            }
                            foreach (var v3GroupIndex in Enumerable.Range(0, settings.Groups.Count))
                            {
                                var v3Group = settings.Groups[v3GroupIndex];
                                if (v3Group == null)
                                    continue;
                                Debug.Log($"DS:Salvaging group from [{v3Group.GroupName}].");
                                var v4Group = new RadialInventorySystemV4.Group();
                                v4Group.BaseMenu.SetObject(v3Group.BaseMenu);
                                v4Group.Name = v3Group.GroupName;
                                v4Group.Icon.SetObject(v3Group.GroupIcon);

                                if (settings.MenuMode == RISV3.RISMode.Simple)
                                {
                                    if (v3Group.ExclusiveMode == 0)
                                    {
                                        v4Avatar.SetExclusiveMode((RIS.ExclusiveGroupType)v3GroupIndex, RIS.ExclusiveModeType.None);
                                        v4Group.UseResetButton = false;
                                    }
                                    else if (v3Group.ExclusiveMode == 1)
                                    {
                                        v4Avatar.SetExclusiveMode((RIS.ExclusiveGroupType)v3GroupIndex, RIS.ExclusiveModeType.None);
                                        v4Group.UseResetButton = true;
                                    }
                                    else if (v3Group.ExclusiveMode == 2)
                                    {
                                        v4Avatar.SetExclusiveMode((RIS.ExclusiveGroupType)v3GroupIndex, RIS.ExclusiveModeType.Exclusive);
                                        v4Group.UseResetButton = true;
                                    }
                                    else if (v3Group.ExclusiveMode == 4)
                                    {
                                        v4Avatar.SetExclusiveMode((RIS.ExclusiveGroupType)v3GroupIndex, RIS.ExclusiveModeType.LegacyExclusive);
                                        v4Group.UseResetButton = false;
                                    }
                                }
                                else
                                {
                                    v4Group.UseResetButton = v3Group.UseResetButton;
                                }
                                foreach (var v3Prop in v3Group.Props)
                                {
                                    if (v3Prop == null)
                                        continue;
                                    var v4Prop = new RadialInventorySystemV4.Prop();
                                    Debug.Log($"DS:Salvaging prop from [{v3Prop.PropName}].");
                                    v4Prop.IsDefaultEnabled = v3Prop.IsDefaultEnabled;
                                    v4Prop.IsLocalOnly = v3Prop.LocalOnly;
                                    v4Prop.Icon.SetObject(v3Prop.PropIcon);
                                    v4Prop.EnableAnimation.SetObject(v3Prop.EnableAnimation);
                                    v4Prop.DisableAnimation.SetObject(v3Prop.DisableAnimation);
                                    v4Prop.Name = v3Prop.PropName;
                                    v4Prop.MaterialOverrides = new List<GUIDPathPair<Material>>();
                                    if (v3Prop.MaterialOverride != null)
                                        v4Prop.MaterialOverrides.Add(new GUIDPathPair<Material>(ObjectPathStateType.Asset, v3Prop.MaterialOverride));
                                    v4Prop.UseResetTimer = v4Prop.UseResetTimer;
                                    v4Prop.ResetSecond = v4Prop.ResetSecond;
                                    v4Prop.UseSaveParameter = v4Prop.UseSaveParameter;
                                    v4Prop.ExclusiveGroup = (RIS.ExclusiveGroupType)((int)v3Prop.PropGroupType - 1);
                                    v4Prop.TargetObjects = new List<GUIDPathPair<GameObject>>();
                                    if (settings.MenuMode == RISV3.RISMode.Simple)
                                    {
                                        if (v3Prop.TargetObject != null)
                                            v4Prop.TargetObjects.Add(new GUIDPathPair<GameObject>(ObjectPathStateType.RelativeFromObject, v3Prop.TargetObject, settings.AvatarRoot.gameObject));
                                    }
                                    else if (settings.MenuMode == RISV3.RISMode.Advanced)
                                    {
                                        var groupType = v4Avatar.GetExclusiveMode(v4Prop.ExclusiveGroup);
                                        if (v4Prop.ExclusiveGroup != RIS.ExclusiveGroupType.None && groupType == RIS.ExclusiveModeType.None)
                                            v4Avatar.SetExclusiveMode(v4Prop.ExclusiveGroup, RIS.ExclusiveModeType.Exclusive);
                                        if (groupType == RIS.ExclusiveModeType.LegacyExclusive && v3Prop.DisableAnimation != null)
                                            v4Avatar.SetExclusiveDisableClip(v4Prop.ExclusiveGroup, v3Prop.DisableAnimation);
                                        if (v3Prop.TargetObjects != null && v3Prop.TargetObjects.Any(n => n != null))
                                        {
                                            foreach (var targetObject in v3Prop.TargetObjects)
                                                if (targetObject != null)
                                                    v4Prop.TargetObjects.Add(new GUIDPathPair<GameObject>(ObjectPathStateType.RelativeFromObject, targetObject, settings.AvatarRoot.gameObject));
                                        }
                                    }
                                    v4Group.Props.Add(v4Prop);
                                }
                                v4Avatar.Groups.Add(v4Group);
                            }
                            avatars.Add(v4Avatar);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);

                        EditorUtility.DisplayDialog("Radial Inventory System", $"設定 [{child.name}]の読み込み時にエラーが発生しました。", "OK");
                    }
                }
            }
            foreach (var avatar in avatars)
                avatar.SaveToJson();
            var dialogResult = EditorUtility.DisplayDialog("Radial Inventory System", $"V3設定の移行が終了しました。設定ファイルを削除しますか？", "はい", "いいえ");
            if (dialogResult)
                GameObject.DestroyImmediate(settingsContainerRoot);
            AssetDatabase.SaveAssets();

        }
    }
}
#endif
#endif