#if RISV4_JSON
using BestHTTP;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEditor;
using UnityEditorInternal.VersionControl;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Component = UnityEngine.Component;

namespace YagihataItems.RadialInventorySystemV4
{
	public static class RISV3toV4Updater
    {
        private static BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.DeclaredOnly;
        private static void SalvageData(string v3SettingsName)
		{
			var settingsContainerRoot = GameObject.Find(v3SettingsName);
			if (settingsContainerRoot != null)
            {
                var containerTransform = settingsContainerRoot.transform;
                for (int transformIndex = 0; transformIndex < containerTransform.childCount; ++transformIndex)
                {
                    var child = containerTransform.GetChild(transformIndex);
                    Component[] components = child.GetComponents<Component>();
                    for (int i = 0; i < components.Length; ++i)
                    {
                        var component = components[i];
                        if (component.GetType().Name == "RISSettings")
                        {
                            try
                            {
                                var avatar = SalvageAvatar(component);
                                avatar.SaveToJson();
                            }
                            catch (Exception ex)
                            {
                                Debug.LogException(ex);

                                EditorUtility.DisplayDialog("Radial Inventory System", $"設定 [{child.name}]の読み込み時にエラーが発生しました。\nV3のEditorにて再度適用してからもう一度変換を試してみてください。", "OK");
                            }
                        }
                    }
                }
                var dialogResult = EditorUtility.DisplayDialog("Radial Inventory System", $"V3設定の移行が終了しました。設定ファイルを削除しますか？", "はい", "いいえ");
                if (dialogResult)
                    GameObject.DestroyImmediate(settingsContainerRoot);
                AssetDatabase.SaveAssets();
            }
        }
        private static Avatar SalvageAvatar(Component component)
        {
            var avatar = new Avatar();
            int[] advancedGroupMode = null;
            object genericGroups = null;
            var inst = Activator.CreateInstance(component.GetType());
            var avatarProperties = component.GetType().GetProperties(flag);
            foreach (PropertyInfo prop in avatarProperties)
            {
                var name = prop.Name;
                var value = prop.GetValue(component);
                if (name == "AvatarRoot")
                {
                    avatar.AvatarRoot.SetObject((VRCAvatarDescriptor)value);
                }
                else if (name == "WriteDefaults")
                {
                    avatar.UseWriteDefaults = (bool)value;
                }
                else if (name == "OptimizeParams")
                {
                    avatar.OptimizeParameters = (bool)value;
                }
                else if (name == "FolderID")
                {
                }
                else if (name == "Groups")
                {
                    genericGroups = value;
                }
                else if (name == "MenuMode")
                {
                    var menuMode = value.ToString();
                    if (menuMode == "Simple")
                        avatar.MenuMode = RIS.MenuModeType.Basic;
                    else
                        avatar.MenuMode = RIS.MenuModeType.Advanced;
                }
                else if (name == "ApplyEnabled")
                {
                    avatar.ApplyEnableDefault = (bool)value;
                }
                else if (name == "AdvancedGroupMode")
                {
                    advancedGroupMode = (int[])value;
                }
            }
            for (int v = 1; v < advancedGroupMode.Length; ++v)
            {
                var v2Mode = advancedGroupMode[v] == 1;
                if (v2Mode)
                    avatar.SetExclusiveMode((RIS.ExclusiveGroupType)(v - 1), RIS.ExclusiveModeType.LegacyExclusive);
                else
                    avatar.SetExclusiveMode((RIS.ExclusiveGroupType)(v - 1), RIS.ExclusiveModeType.None);
            }
            var itemFields = genericGroups.GetType().GetGenericArguments().Single().GetFields(flag);
            IList rawList = genericGroups as IList;
            var groupIndex = -1;
            foreach (var listItem in rawList)
            {
                ++groupIndex;
                var group = SalvageGroup(itemFields, avatar, listItem, groupIndex);

                avatar.Groups.Add(group);
            }
            return avatar;
        }
        private static Group SalvageGroup(FieldInfo[] itemFields, Avatar avatar, object listItem, int groupIndex)
        {
            var group = new Group();
            object genericProps = null;
            var exclusiveMode = 0;
            var useResetButton = false;
            foreach (var field in itemFields)
            {
                var name = field.Name;
                var value = field.GetValue(listItem);
                if (name == "Props")
                {
                    genericProps = value;
                }
                else if (name == "GroupName")
                {
                    group.Name = value.ToString();
                }
                else if (name == "GroupIcon")
                {
                    group.Icon.SetObject((Texture2D)value);
                }
                else if (name == "ExclusiveMode")
                {
                    exclusiveMode = (int)value;
                }
                else if (name == "BaseMenu")
                {
                    group.BaseMenu.SetObject((VRCExpressionsMenu)value);
                }
                else if (name == "UseResetButton")
                {
                    group.UseResetButton = (bool)value;
                }
            }
            if (avatar.MenuMode == RIS.MenuModeType.Basic)
            {
                if (exclusiveMode == 0)
                {
                    avatar.SetExclusiveMode((RIS.ExclusiveGroupType)groupIndex, RIS.ExclusiveModeType.None);
                    group.UseResetButton = false;
                }
                else if (exclusiveMode == 1)
                {
                    avatar.SetExclusiveMode((RIS.ExclusiveGroupType)groupIndex, RIS.ExclusiveModeType.None);
                    group.UseResetButton = true;
                }
                else if (exclusiveMode == 2)
                {
                    avatar.SetExclusiveMode((RIS.ExclusiveGroupType)groupIndex, RIS.ExclusiveModeType.Exclusive);
                    group.UseResetButton = true;
                }
                else if (exclusiveMode == 4)
                {
                    avatar.SetExclusiveMode((RIS.ExclusiveGroupType)groupIndex, RIS.ExclusiveModeType.LegacyExclusive);
                    group.UseResetButton = false;
                }
            }
            else
            {
                group.UseResetButton = useResetButton;
            }
            if (genericProps != null)
            {
                var propsFields = genericProps.GetType().GetGenericArguments().Single().GetFields(flag);
                IList subRawList = genericProps as IList;
                foreach (var subListItem in subRawList)
                {
                    var prop = SalvageProp(propsFields, avatar, subListItem);
                    group.Props.Add(prop);
                }
            }
            return group;
        }
        private static Prop SalvageProp(FieldInfo[] propsFields, Avatar avatar, object subListItem)
        {
            var prop = new Prop();
            GameObject targetObject = null;
            List<GameObject> targetObjects = null;

            foreach (var field in propsFields)
            {
                var name = field.Name;
                var value = field.GetValue(subListItem);
                if (name == "TargetObject")
                {
                    targetObject = (GameObject)value;
                }
                else if (name == "IsDefaultEnabled")
                {
                    prop.IsDefaultEnabled = (bool)value;
                }
                else if (name == "LocalOnly")
                {
                    prop.IsLocalOnly = (bool)value;
                }
                else if (name == "PropIcon")
                {
                    prop.Icon.SetObject((Texture2D)value);
                }
                else if (name == "EnableAnimation")
                {
                    prop.EnableAnimation.SetObject((AnimationClip)value);
                }
                else if (name == "DisableAnimation")
                {
                    prop.DisableAnimation.SetObject((AnimationClip)value);
                }
                else if (name == "PropName")
                {
                    prop.Name = value.ToString();
                }
                else if (name == "TargetObjects")
                {
                    targetObjects = (List<GameObject>)value;
                }
                else if (name == "PropGroupType")
                {
                    switch (value.ToString())
                    {
                        case "None":
                            prop.ExclusiveGroup = RIS.ExclusiveGroupType.None;
                            break;
                        case "Group1":
                            prop.ExclusiveGroup = RIS.ExclusiveGroupType.Group1;
                            break;
                        case "Group2":
                            prop.ExclusiveGroup = RIS.ExclusiveGroupType.Group2;
                            break;
                        case "Group3":
                            prop.ExclusiveGroup = RIS.ExclusiveGroupType.Group3;
                            break;
                        case "Group4":
                            prop.ExclusiveGroup = RIS.ExclusiveGroupType.Group4;
                            break;
                        case "Group5":
                            prop.ExclusiveGroup = RIS.ExclusiveGroupType.Group5;
                            break;
                        case "Group6":
                            prop.ExclusiveGroup = RIS.ExclusiveGroupType.Group6;
                            break;
                        case "Group7":
                            prop.ExclusiveGroup = RIS.ExclusiveGroupType.Group7;
                            break;
                        case "Group8":
                            prop.ExclusiveGroup = RIS.ExclusiveGroupType.Group8;
                            break;
                    }
                }
                else if (name == "MaterialOverride")
                {
                    prop.MaterialOverrides = new List<GUIDPathPair<Material>>();
                    Material material = (Material)value;
                    if (material != null)
                        prop.MaterialOverrides.Add(new GUIDPathPair<Material>(ObjectPathStateType.Asset, material));
                }
                else if (name == "UseResetTimer")
                {
                    prop.UseResetTimer = (bool)value;
                }
                else if (name == "ResetSecond")
                {
                    prop.ResetSecond = (float)value;
                }
                else if (name == "SaveParameter")
                {
                    prop.UseSaveParameter = (bool)value;
                }
            }
            if (avatar.MenuMode == RIS.MenuModeType.Basic)
            {
                prop.TargetObjects.Clear();
                if (targetObject != null)
                    prop.TargetObjects.Add(new GUIDPathPair<GameObject>(ObjectPathStateType.RelativeFromObject, targetObject, avatar.GetAvatarRoot()?.gameObject));
            }
            else
            {
                var groupType = avatar.GetExclusiveMode(prop.ExclusiveGroup);
                if (prop.ExclusiveGroup != RIS.ExclusiveGroupType.None && groupType == RIS.ExclusiveModeType.None)
                    avatar.SetExclusiveMode(prop.ExclusiveGroup, RIS.ExclusiveModeType.Exclusive);
                if (groupType == RIS.ExclusiveModeType.LegacyExclusive && prop.DisableAnimation.GetObject() != null)
                    avatar.SetExclusiveDisableClip(prop.ExclusiveGroup, prop.DisableAnimation.GetObject());
                if (targetObjects != null && targetObjects.Any(n => n != null))
                {
                    foreach (var obj in targetObjects)
                        if (obj != null)
                            prop.TargetObjects.Add(new GUIDPathPair<GameObject>(ObjectPathStateType.RelativeFromObject, obj, avatar.GetAvatarRoot()?.gameObject));
                }
            }
            return prop;
        }
		public static bool HasV3SettingsOnScene(string v3SettingsName)
		{
			var settingsContainerRoot = GameObject.Find(v3SettingsName);
			return settingsContainerRoot != null;
		}
		[InitializeOnLoadMethod]
		static void EditorInitialize()
		{
#if RISV4_SALVAGED
			ScriptingDefineSymbolsUtil.Remove("RISV4_V3");
#else
			Type type = GetTypeByClassName("YagihataItems.RadialInventorySystemV3.RISSettings");
			if (type != null)
				ScriptingDefineSymbolsUtil.Add("RISV4_V3");
			else
				ScriptingDefineSymbolsUtil.Remove("RISV4_V3");
#endif
		}
		public static void SalvageDatas(string v3SettingsName)
		{
#if RISV4_SALVAGED
			ScriptingDefineSymbolsUtil.Remove("RISV4_V3");
#else
			Type type = GetTypeByClassName("YagihataItems.RadialInventorySystemV3.RISSettings");
			if(type != null)
			{
				ScriptingDefineSymbolsUtil.Add("RISV4_V3");
				/*Type dataSalvagerType = GetTypeByClassName("YagihataItems.RadialInventorySystemV4.DataSalvager");
				object result = dataSalvagerType.InvokeMember("SalvageDatas", BindingFlags.InvokeMethod, null, null, new object[] { v3SettingsName });*/
				SalvageData(v3SettingsName);

            }
			else
			{
				ScriptingDefineSymbolsUtil.Remove("RISV4_V3");
				EditorUtility.DisplayDialog(RISStrings.GetString("ris"), RISStrings.GetString("missing_v3"), RISStrings.GetString("ok"));
			}
#endif
		}
#if RISV4_V3
		[MenuItem("Radial Inventory/RISV4 Disable V3", priority = 30)]
		public static void DisableV3Update()
		{
			ScriptingDefineSymbolsUtil.Remove("RISV4_V3");
            ScriptingDefineSymbolsUtil.Add("RISV4_SALVAGED");
		}
#endif
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
	}
}
#endif