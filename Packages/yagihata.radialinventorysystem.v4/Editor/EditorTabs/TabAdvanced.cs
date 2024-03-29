﻿#if RISV4_JSON
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditorInternal;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;
using UnityEditor.Presets;
using System.Text.RegularExpressions;

namespace YagihataItems.RadialInventorySystemV4
{
    public class TabAdvanced : EditorTab
    {
        private ReorderableList groupsReorderableList;
        private ReorderableList propsReorderableList;
        private ReorderableList gameObjectsReorderableList = null;
        private ReorderableList materialsReorderableList = null;
        private ReorderableList parametersReorderableList = null;
        private Group selectedGroup = null;
        private Prop selectedProp = null;
        private GUIStyle singleLinedLabelStyle = new GUIStyle(GUI.skin.label) { fixedHeight = EditorGUIUtility.singleLineHeight };
        private string[] parameterTypes =
        {
            RISStrings.GetString("param_ris"),
            RISStrings.GetString("param_custom")
        };
        private string[] triggerTypes =
        {
            RISStrings.GetString("param_turnon"),
            RISStrings.GetString("param_turnoff")
        };
        public override void InitializeTab(ref Avatar risAvatar)
        {
            InitializeGroupList(risAvatar);
            InitializePropList(null, risAvatar);
            InitializeGameObjectsList(risAvatar);
            InitializeMaterialsList(risAvatar);
        }
        public override void DrawTab(ref Avatar risAvatar, Rect position, bool showingVerticalScroll)
        {
            if (groupsReorderableList == null)
                InitializeGroupList(risAvatar);
            if (propsReorderableList == null)
                InitializePropList(null, risAvatar);
            if (gameObjectsReorderableList == null)
                InitializeGameObjectsList(risAvatar);
            if (materialsReorderableList == null)
                InitializeMaterialsList(risAvatar);
            var cellWidth = position.width / 3f - 15f;
            var selectedGroupIsChanged = false;
            var selectedPropIsChanged = false;
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    using (var scope = new EditorGUILayout.HorizontalScope())
                    {
                        var scopeWidth = cellWidth - 40;
                        var GroupsHeight = groupsReorderableList.GetHeight();
                        using (new EditorGUILayout.HorizontalScope(GUILayout.Height(GroupsHeight), GUILayout.Width(scopeWidth)))
                        {
                            var oldSelectedIndex = groupsReorderableList.index;
                            groupsReorderableList.DoList(new Rect(scope.rect.x, scope.rect.y, scopeWidth, GroupsHeight));
                            selectedGroupIsChanged = oldSelectedIndex != groupsReorderableList.index;
                            GUILayout.Space(0);
                        }

                    }
                }
                GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(1));
                var groupIndex = groupsReorderableList.index;
                var groupIsSelected = risAvatar != null && risAvatar.Groups.Count >= 1 && groupIndex >= 0;

                if (groupIsSelected && selectedGroupIsChanged)
                {
                    selectedGroup = risAvatar.Groups[groupIndex];
                }
                else if (selectedGroupIsChanged || selectedGroup == null)
                {
                    selectedGroup = new Group();
                }
                using (new EditorGUI.DisabledGroupScope(!groupIsSelected))
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(cellWidth + 20)))
                    {
                        EditorGUIUtility.labelWidth = RISStrings.GetWidth(4);
                        EditorGUILayout.LabelField(RISStrings.GetString("group_settings"), new GUIStyle("ProjectBrowserHeaderBgTop"), GUILayout.ExpandWidth(true));
                        GUILayout.Space(3);
                        selectedGroup.Name = EditorGUILayout.TextField(RISStrings.GetString("group_name"), selectedGroup.Name);

                        EditorGUI.BeginChangeCheck();

                        var icon = selectedGroup.Icon.GetObject();
                        icon = (Texture2D)EditorGUILayout.ObjectField(RISStrings.GetString("icon"), icon, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));

                        var baseMenu = selectedGroup.BaseMenu.GetObject();
                        baseMenu = (VRCExpressionsMenu)EditorGUILayout.ObjectField(RISStrings.GetString("base_menu"), baseMenu, typeof(VRCExpressionsMenu), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));

                        if (EditorGUI.EndChangeCheck())
                        {
                            selectedGroup.Icon.SetObject(icon);
                            selectedGroup.BaseMenu.SetObject(baseMenu);
                        }

                        selectedGroup.UseResetButton = EditorGUILayout.Toggle(RISStrings.GetString("reset_button"), selectedGroup.UseResetButton);
                        if (selectedGroupIsChanged)
                            InitializePropList(selectedGroup, risAvatar);
                        using (var scope = new EditorGUILayout.HorizontalScope())
                        {
                            var scopeWidth = cellWidth + 20;
                            var propListHeight = propsReorderableList.GetHeight();
                            using (new EditorGUILayout.HorizontalScope(GUILayout.Height(propListHeight), GUILayout.Width(scopeWidth)))
                            {
                                var oldSelectedIndex = propsReorderableList.index;
                                propsReorderableList.DoList(new Rect(scope.rect.x, scope.rect.y, scopeWidth, propListHeight));
                                selectedGroup.Props = (List<Prop>)propsReorderableList.list;
                                selectedPropIsChanged = oldSelectedIndex != propsReorderableList.index;
                                GUILayout.Space(0);
                            }
                        }
                    }
                }
                GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(1));
                var propIndex = propsReorderableList.index;
                var propIsSelected = groupIsSelected && groupIndex != -1 && propIndex != -1 && groupIndex < risAvatar.Groups.Count && propIndex < risAvatar.Groups[groupIndex].Props.Count;
                if (propIsSelected && selectedPropIsChanged)
                {
                    selectedProp = risAvatar.Groups[groupIndex].Props[propIndex];
                }
                else if (selectedPropIsChanged || selectedProp == null)
                {
                    propIsSelected = false;
                    selectedProp = new Prop();
                }
                using (new EditorGUI.DisabledGroupScope(!propIsSelected))
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(cellWidth + 30 - (showingVerticalScroll ? 14 : 0))))
                    {
                        GUIStyle headerStyle = new GUIStyle("HeaderLabel");
                        headerStyle.margin = new RectOffset(5, 5, 20, 20);
                        EditorGUILayout.LabelField(RISStrings.GetString("prop_settings"), new GUIStyle("ProjectBrowserHeaderBgTop"), GUILayout.ExpandWidth(true));
                        GUILayout.Space(3);
                        EditorGUIUtility.labelWidth = RISStrings.GetWidth(5);
                        selectedProp.Name = EditorGUILayout.TextField(RISStrings.GetString("prop_name"), selectedProp.Name);

                        EditorGUI.BeginChangeCheck();

                        var icon = selectedProp.Icon.GetObject();
                        icon = (Texture2D)EditorGUILayout.ObjectField(RISStrings.GetString("icon"), icon, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                        if (EditorGUI.EndChangeCheck())
                            selectedProp.Icon.SetObject(icon);

                        EditorGUI.BeginChangeCheck();
                        var beforeExclusiveGroup = selectedProp.ExclusiveGroup;
                        selectedProp.ExclusiveGroup = (RIS.ExclusiveGroupType)EditorGUILayout.EnumPopup(RISStrings.GetString("exclusive_group"), selectedProp.ExclusiveGroup);
                        var v2Mode = false;
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (selectedProp.ExclusiveGroup == RIS.ExclusiveGroupType.None && beforeExclusiveGroup != RIS.ExclusiveGroupType.None)
                                risAvatar.SetExclusiveMode(selectedProp.ExclusiveGroup, RIS.ExclusiveModeType.None);
                            else if (selectedProp.ExclusiveGroup != RIS.ExclusiveGroupType.None && beforeExclusiveGroup == RIS.ExclusiveGroupType.None)
                            {
                                v2Mode = risAvatar.GetExclusiveMode(selectedProp.ExclusiveGroup) == RIS.ExclusiveModeType.LegacyExclusive;
                                risAvatar.SetExclusiveMode(selectedProp.ExclusiveGroup, v2Mode ? RIS.ExclusiveModeType.LegacyExclusive : RIS.ExclusiveModeType.Exclusive);
                            }
                        }
                        if (selectedProp.ExclusiveGroup != RIS.ExclusiveGroupType.None)
                        {
                            v2Mode = risAvatar.GetExclusiveMode(selectedProp.ExclusiveGroup) == RIS.ExclusiveModeType.LegacyExclusive;
                            EditorGUI.BeginChangeCheck();
                            v2Mode = EditorGUILayout.Toggle("┗"+ RISStrings.GetString("legacy_mode"), v2Mode);
                            if (EditorGUI.EndChangeCheck())
                                risAvatar.SetExclusiveMode(selectedProp.ExclusiveGroup, v2Mode ? RIS.ExclusiveModeType.LegacyExclusive : RIS.ExclusiveModeType.Exclusive);
                        }
                        EditorGUI.BeginChangeCheck();
                        selectedProp.IsDefaultEnabled = EditorGUILayout.Toggle(RISStrings.GetString("default_status"), selectedProp.IsDefaultEnabled);
                        if (EditorGUI.EndChangeCheck())
                        {
                            var exclusiveGroup = selectedProp.ExclusiveGroup;
                            if (EditorSettings.DefaultStatusLimitter && selectedProp.IsDefaultEnabled && risAvatar.GetExclusiveMode(exclusiveGroup) == RIS.ExclusiveModeType.Exclusive)
                            {
                                foreach (var subGroupIndex in Enumerable.Range(0, risAvatar.Groups.Count))
                                {
                                    var subGroup = risAvatar.Groups[subGroupIndex];
                                    foreach (var subPropIndex in Enumerable.Range(0, subGroup.Props.Count))
                                    {
                                        var subProp = subGroup.Props[subPropIndex];
                                        if ((subGroupIndex != groupIndex || subPropIndex != propIndex) && subProp.ExclusiveGroup == selectedProp.ExclusiveGroup)
                                            subProp.IsDefaultEnabled = false;
                                    }
                                }
                            }
                        }
                        selectedProp.IsLocalOnly = EditorGUILayout.Toggle(RISStrings.GetString("local_mode"), selectedProp.IsLocalOnly);
                        GUILayout.Space(5);
                        EditorGUILayout.LabelField(RISStrings.GetString("additional_animation"));
                        EditorGUI.BeginChangeCheck();
                        var enableAnim = selectedProp.EnableAnimation.GetObject();
                        var disableAnim = selectedProp.DisableAnimation.GetObject();
                        var defaultAnim = risAvatar.GetExclusiveDisableClip(selectedProp.ExclusiveGroup);
                        enableAnim = (AnimationClip)EditorGUILayout.ObjectField("┣" + RISStrings.GetString("on_enabled"), enableAnim, typeof(AnimationClip), false);
                        if (v2Mode)
                            defaultAnim = (AnimationClip)EditorGUILayout.ObjectField("┗" + RISStrings.GetString("on_default"), defaultAnim, typeof(AnimationClip), false);
                        else
                            disableAnim = (AnimationClip)EditorGUILayout.ObjectField("┗" + RISStrings.GetString("on_disabled"), disableAnim, typeof(AnimationClip), false);
                        if (EditorGUI.EndChangeCheck())
                        {
                            selectedProp.EnableAnimation.SetObject(enableAnim);
                            selectedProp.DisableAnimation.SetObject(disableAnim);
                            risAvatar.SetExclusiveDisableClip(selectedProp.ExclusiveGroup, defaultAnim);
                        }
                        GUILayout.Space(5);
                        selectedProp.UseResetTimer = EditorGUILayout.Toggle(RISStrings.GetString("off_timer"), selectedProp.UseResetTimer);
                        if (selectedProp.UseResetTimer)
                            selectedProp.ResetSecond = Mathf.Min(60, Mathf.Max(0, EditorGUILayout.FloatField("┗" + RISStrings.GetString("seconds"), selectedProp.ResetSecond)));
                        GUILayout.Space(5);

                        if (parametersReorderableList == null || selectedPropIsChanged)
                            InitializeParametersList(risAvatar, selectedProp);
                        EditorGUI.BeginChangeCheck();
                        parametersReorderableList.DoLayoutList();
                        if (EditorGUI.EndChangeCheck())
                            selectedProp.Parameters = (List<Parameter>)parametersReorderableList.list;
                        
                        GUILayout.Space(5);
                        if (materialsReorderableList == null || selectedPropIsChanged)
                            InitializeMaterialsList(risAvatar, selectedProp);
                        EditorGUI.BeginChangeCheck();
                        materialsReorderableList.DoLayoutList();
                        if (EditorGUI.EndChangeCheck())
                            selectedProp.MaterialOverrides = (List<GUIDPathPair<Material>>)materialsReorderableList.list;

                        GUILayout.Space(5);
                        if (gameObjectsReorderableList == null || selectedPropIsChanged)
                            InitializeGameObjectsList(risAvatar, selectedProp);
                        EditorGUI.BeginChangeCheck();
                        gameObjectsReorderableList.DoLayoutList();
                        if (EditorGUI.EndChangeCheck())
                            selectedProp.TargetObjects = (List<GUIDPathPair<GameObject>>)gameObjectsReorderableList.list;

                    }
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUIUtility.labelWidth = 0;
        }
        private void InitializeGroupList(Avatar risAvatar)
        {
            List<Group> groups = null;
            if (risAvatar != null && risAvatar.Groups != null)
                groups = risAvatar.Groups;
            else
                groups = new List<Group>();
            groupsReorderableList = new ReorderableList(groups, typeof(Group))
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, string.Format(RISStrings.GetString("list_group"), groups.Count));
                    var position =
                        new Rect(
                            rect.x + rect.width - 20f,
                            rect.y,
                            20f,
                            13f
                        );
                    if (groups.Count < 8 && GUI.Button(position, ReorderableListStyle.AddContent, ReorderableListStyle.AddStyle))
                    {
                        var newGroup = new Group();
                        newGroup.Name = string.Format(RISStrings.GetString("defaultname_group"), groups.Count);
                        risAvatar.Groups.Add(newGroup);
                    }
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (groups.Count <= index)
                        return;

                    var style = GUI.skin.label;
                    style.fontSize = (int)(rect.height / 1.75f);
                    var name = groups[index].Name;
                    if (string.IsNullOrEmpty(name))
                        name = string.Format(RISStrings.GetString("defaultname_group"), index);
                    GUI.Label(rect, name, style);
                    style.fontSize = 0;
                    rect.x = rect.x + rect.width - 20f;
                    rect.width = 20f;
                    if (GUI.Button(rect, ReorderableListStyle.SubContent, ReorderableListStyle.SubStyle))
                    {
                        groups.RemoveAt(index);
                        if (index >= groups.Count)
                            index = groupsReorderableList.index = -1;
                    }
                },

                drawFooterCallback = rect => { },
                footerHeight = 0f,
                elementHeightCallback = index =>
                {
                    if (groups.Count <= index)
                        return 0;

                    return EditorGUIUtility.singleLineHeight * 1.6f;
                }

            };
        }
        private void InitializePropList(Group group, Avatar risAvatar)
        {
            List<Prop> props = null;
            if (group != null && group.Props != null)
                props = group.Props;
            else
                props = new List<Prop>();
            propsReorderableList = new ReorderableList(props, typeof(Prop))
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, string.Format(RISStrings.GetString("list_prop"), props.Count));
                    var position =
                        new Rect(
                            rect.x + rect.width - 20f,
                            rect.y,
                            20f,
                            13f
                        );
                    var maxPropCount = 8;
                    if (risAvatar != null && group != null)
                    {
                        if (group.UseResetButton)
                            maxPropCount = 7;
                        if (group.BaseMenu?.GetObject() != null)
                            maxPropCount -= group.BaseMenu.GetObject().controls.Count;
                    }
                    if (props.Count < maxPropCount && GUI.Button(position, ReorderableListStyle.AddContent, ReorderableListStyle.AddStyle))
                    {
                        var newProp = new Prop();
                        newProp.TargetObjects.Add(null);
                        group.Props.Add(newProp);
                    }
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (props.Count <= index)
                        return;
                    var prop = props[index];
                    var rawPropName = prop.GetPropName(risAvatar);
                    var propName = !string.IsNullOrEmpty(rawPropName) ? rawPropName : string.Format(RISStrings.GetString("defaultname_prop"), index);
                    GUI.Label(rect, propName);
                    rect.x = rect.x + rect.width - 20f;
                    rect.width = 20f;
                    if (GUI.Button(rect, ReorderableListStyle.SubContent, ReorderableListStyle.SubStyle))
                    {
                        props.RemoveAt(index);
                        if (index >= props.Count)
                            index = propsReorderableList.index = -1;
                    }
                },
                drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
                {
                    if (!isFocused)
                    {
                        var maxPropCount = 8;
                        if (risAvatar != null && group != null)
                        {
                            if (group.BaseMenu?.GetObject() != null)
                                maxPropCount = 8 - group.BaseMenu.GetObject().controls.Count;
                        }
                        if (index >= maxPropCount)
                        {
                            GUI.DrawTexture(rect, TexAssets.RedTexture);
                        }

                    }
                    else
                    {
                        GUI.DrawTexture(rect, TexAssets.BlueTexture);
                    }
                },
                drawFooterCallback = rect => { },
                footerHeight = 0f,
                elementHeightCallback = index =>
                {
                    if (props.Count <= index)
                        return 0;

                    return EditorGUIUtility.singleLineHeight;
                }

            };
        }
        private void InitializeMaterialsList(Avatar risAvatar, Prop prop = null)
        {
            List<GUIDPathPair<Material>> materials = null;
            var flag = prop != null && prop.MaterialOverrides != null;
            if (flag)
                materials = prop.MaterialOverrides;
            else
                materials = new List<GUIDPathPair<Material>>();
            var list = new ReorderableList(materials, typeof(Material));
            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, string.Format(RISStrings.GetString("material_list"), materials.Count));
                var position =
                    new Rect(
                        rect.x + rect.width - 20f,
                        rect.y,
                        20f,
                        13f
                    );
                if (GUI.Button(position, ReorderableListStyle.AddContent, ReorderableListStyle.AddStyle) && flag)
                {
                    prop.MaterialOverrides.Add(null);
                }
            };
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (materials.Count <= index)
                    return;
                rect.width -= 20;
                if (risAvatar.AvatarRoot?.GetObject() == null)
                    return;
                var parent = risAvatar.AvatarRoot?.GetObject()?.gameObject;
                var targetObject = materials[index]?.GetObject(parent);
                EditorGUI.BeginChangeCheck();
                targetObject = (Material)EditorGUI.ObjectField(rect, targetObject, typeof(Material), false);
                if (EditorGUI.EndChangeCheck())
                {
                    if (materials[index] == null)
                        materials[index] = new GUIDPathPair<Material>(ObjectPathStateType.Asset, targetObject, parent);
                    else
                        materials[index].SetObject(targetObject, parent);
                }
                rect.x = rect.x + rect.width;
                rect.width = 20f;
                if (GUI.Button(rect, ReorderableListStyle.SubContent, ReorderableListStyle.SubStyle))
                {
                    materials.RemoveAt(index);
                    if (index >= materials.Count)
                        index = list.index = -1;
                }
            };
            list.drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
            {
                if (isFocused)
                {
                    GUI.DrawTexture(rect, TexAssets.BlueTexture);
                }
            };
            list.drawFooterCallback = rect => { };
            list.footerHeight = 0f;
            list.elementHeightCallback = index =>
            {
                if (materials.Count <= index)
                    return 0;
                return EditorGUIUtility.singleLineHeight;
            };
            materialsReorderableList = list;
        }
        private void InitializeGameObjectsList(Avatar risAvatar, Prop prop = null)
        {
            List<GUIDPathPair<GameObject>> gameObjects = null;
            var propFlag = prop != null && prop.TargetObjects != null;
            if (propFlag)
                gameObjects = prop.TargetObjects;
            else
                gameObjects = new List<GUIDPathPair<GameObject>>();
            var list = new ReorderableList(gameObjects, typeof(GameObject));
            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, string.Format(RISStrings.GetString("list_object"), gameObjects.Count));
                var position =
                    new Rect(
                        rect.x + rect.width - 20f,
                        rect.y,
                        20f,
                        13f
                    );
                if (GUI.Button(position, ReorderableListStyle.AddContent, ReorderableListStyle.AddStyle) && propFlag)
                {
                    prop.TargetObjects.Add(null);
                }
            };
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (gameObjects.Count <= index)
                    return;
                rect.width -= 20;
                if (risAvatar.AvatarRoot?.GetObject() == null)
                    return;
                var parent = risAvatar.AvatarRoot?.GetObject()?.gameObject;
                var targetObject = gameObjects[index]?.GetObject(parent);
                EditorGUI.BeginChangeCheck();
                targetObject = (GameObject)EditorGUI.ObjectField(rect, targetObject, typeof(GameObject), true);
                if (EditorGUI.EndChangeCheck())
                {
                    if (gameObjects[index] == null)
                        gameObjects[index] = new GUIDPathPair<GameObject>(ObjectPathStateType.RelativeFromObject, targetObject, parent);
                    else
                        gameObjects[index].SetObject(targetObject, parent);
                    if(gameObjects[index].GetObject(parent) == null && targetObject != null)
                    {
                        EditorUtility.DisplayDialog(RISStrings.GetString("ris"), RISStrings.GetString("dlg_childprop"), RISStrings.GetString("ok"));
                    }
                }
                rect.x = rect.x + rect.width;
                rect.width = 20f;
                if (GUI.Button(rect, ReorderableListStyle.SubContent, ReorderableListStyle.SubStyle))
                {
                    gameObjects.RemoveAt(index);
                    if (index >= gameObjects.Count)
                        index = list.index = -1;
                }
            };
            list.drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
            {
                if (!isFocused)
                {
                    if (risAvatar.AvatarRoot?.GetObject() != null)
                    {
                        var parent = risAvatar.AvatarRoot?.GetObject()?.gameObject;
                        if (parent != null && index >= 0 && index < gameObjects.Count)
                        {

                            var targetObject = gameObjects[index]?.GetObject(parent);
                            if (targetObject != null && !targetObject.IsChildOf(parent))
                            {
                                GUI.DrawTexture(rect, TexAssets.RedTexture);
                            }
                        }
                    }

                }
                else
                {
                    GUI.DrawTexture(rect, TexAssets.BlueTexture);
                }
            };
            list.drawFooterCallback = rect => { };
            list.footerHeight = 0f;
            list.elementHeightCallback = index =>
            {
                if (gameObjects.Count <= index)
                    return 0;
                return EditorGUIUtility.singleLineHeight;
            };
            gameObjectsReorderableList = list;
        }
        private void InitializeParametersList(Avatar risAvatar, Prop prop = null)
        {
            List<Parameter> parameters = null;
            var propFlag = prop != null && prop.Parameters != null;
            if (propFlag)
                parameters = prop.Parameters;
            else
                parameters = new List<Parameter>();
            var list = new ReorderableList(parameters, typeof(Parameter));

            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, string.Format(RISStrings.GetString("param_title"), parameters.Count));
                var position =
                    new Rect(
                        rect.x + rect.width - 20f,
                        rect.y,
                        20f,
                        13f
                    );
                if (GUI.Button(position, ReorderableListStyle.AddContent, ReorderableListStyle.AddStyle) && propFlag)
                {
                    prop.Parameters.Add(new Parameter());
                }
            };
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (parameters.Count <= index)
                    return;
                rect.width -= 25;
                if (risAvatar.AvatarRoot?.GetObject() == null)
                    return;
                rect.y += 5;
                rect.height -= 10;
                using (new GUI.GroupScope(rect, GUI.skin.box))
                {
                    var width = 60;
                    var parameter = parameters[index];
                    var subRect = new Rect(rect);
                    subRect.x = subRect.y = 0;
                    subRect.height = EditorGUIUtility.singleLineHeight;
                    subRect.width = width;
                    GUI.Label(subRect, RISStrings.GetString("param_type"), singleLinedLabelStyle);
                    subRect.x += subRect.width;
                    subRect.width = rect.width - subRect.width;
                    EditorGUI.BeginChangeCheck();
                    var paramType = parameter.IsRISParam ? 0 : 1;
                    paramType = EditorGUI.Popup(subRect, paramType, parameterTypes);
                    var isRISParam = paramType == 0;
                    if (EditorGUI.EndChangeCheck())
                        parameter.IsRISParam = isRISParam;

                    subRect.x = 0;
                    subRect.y = EditorGUIUtility.singleLineHeight;
                    subRect.height = EditorGUIUtility.singleLineHeight;
                    subRect.width = width;
                    GUI.Label(subRect, RISStrings.GetString("param_timing"), singleLinedLabelStyle);
                    subRect.x += subRect.width;
                    subRect.width = rect.width - subRect.width;
                    EditorGUI.BeginChangeCheck();
                    var triggerType = (int)parameter.ParameterTriggerType;
                    parameter.ParameterTriggerType = (RIS.ParameterTriggerType)EditorGUI.Popup(subRect, triggerType, triggerTypes);

                    if (isRISParam)
                    {
                        var groupsCount = risAvatar?.Groups.Count ?? 0;
                        var groupNames = new string[groupsCount];
                        var selectedGroupIndex = 0;
                        var selectedPropIndex = 0;
                        var reg = new Regex("RISV4-G([0-9])P([0-9])");
                        var match = reg.Match(parameter.Name);
                        if (match.Success)
                        {
                            selectedGroupIndex = int.Parse(match.Groups[1].Value);
                            selectedPropIndex = int.Parse(match.Groups[2].Value);
                        }
                        foreach (var v in Enumerable.Range(0, groupsCount))
                        {
                            var group = risAvatar.Groups[v];
                            groupNames[v] = $"{v}:{group.Name}";
                        }

                        subRect.x = 0;
                        subRect.y = EditorGUIUtility.singleLineHeight * 2;
                        subRect.height = EditorGUIUtility.singleLineHeight;
                        subRect.width = width;
                        GUI.Label(subRect, RISStrings.GetString("group"), singleLinedLabelStyle);
                        subRect.x += subRect.width;
                        subRect.width = rect.width - subRect.width;
                        EditorGUI.BeginChangeCheck();
                        selectedGroupIndex = EditorGUI.Popup(subRect, selectedGroupIndex, groupNames);
                        if (EditorGUI.EndChangeCheck() && groupNames != null && selectedGroupIndex < groupsCount)
                        {
                            selectedPropIndex = 0;
                            parameter.Name = $"RISV4-G{selectedGroupIndex}P{selectedPropIndex}";
                        }

                        var propsCount = 0;
                        if (selectedGroupIndex < groupsCount)
                            propsCount = risAvatar?.Groups[selectedGroupIndex]?.Props.Count ?? 0;
                        var propNames = new string[propsCount];
                        foreach (var v in Enumerable.Range(0, propsCount))
                        {
                            var item = risAvatar.Groups[selectedGroupIndex].Props[v];
                            propNames[v] = $"{v}:{item.Name}";
                        }
                        subRect.x = 0;
                        subRect.y = EditorGUIUtility.singleLineHeight * 3;
                        subRect.height = EditorGUIUtility.singleLineHeight;
                        subRect.width = width;
                        GUI.Label(subRect, RISStrings.GetString("prop"), singleLinedLabelStyle);
                        subRect.x += subRect.width;
                        subRect.width = rect.width - subRect.width;
                        EditorGUI.BeginChangeCheck();
                        selectedPropIndex = EditorGUI.Popup(subRect, selectedPropIndex, propNames);
                        if (EditorGUI.EndChangeCheck() && propNames != null && selectedPropIndex < propsCount)
                        {
                            parameter.Name = $"RISV4-G{selectedGroupIndex}P{selectedPropIndex}";
                        }

                        subRect.x = 0;
                        subRect.y = EditorGUIUtility.singleLineHeight * 4;
                        subRect.height = EditorGUIUtility.singleLineHeight;
                        subRect.width = width;
                        GUI.Label(subRect, RISStrings.GetString("param_value"), singleLinedLabelStyle);
                        subRect.x += subRect.width;
                        subRect.width = rect.width - subRect.width;
                        EditorGUI.BeginChangeCheck();
                        var value = parameter.Value == 1;
                        value = EditorGUI.Toggle(subRect, value);
                        if (EditorGUI.EndChangeCheck())
                        {
                            parameter.Value = value ? 1 : 0;
                        }
                    }
                    else
                    {
                        var expParams = risAvatar.GetAvatarRoot()?.GetExpressionParameters("", false)?.parameters;
                        var paramCount = expParams?.Length ?? 0;
                        var paramNames = new string[paramCount];
                        var selectedParam = 0;
                        foreach (var v in Enumerable.Range(0, paramCount))
                        {
                            var param = expParams[v];
                            paramNames[v] = $"{param.name}[{param.valueType}]";
                            if (param.name == parameter.Name && param.valueType == parameter.ValueType)
                                selectedParam = v;
                        }

                        subRect.x = 0;
                        subRect.y = EditorGUIUtility.singleLineHeight * 2;
                        subRect.height = EditorGUIUtility.singleLineHeight;
                        subRect.width = width;
                        GUI.Label(subRect, RISStrings.GetString("param_target"), singleLinedLabelStyle);
                        subRect.x += subRect.width;
                        subRect.width = rect.width - subRect.width;
                        EditorGUI.BeginChangeCheck();
                        selectedParam = EditorGUI.Popup(subRect, selectedParam, paramNames);
                        if (EditorGUI.EndChangeCheck() && expParams != null && selectedParam < paramCount)
                        {
                            parameter.Name = expParams[selectedParam].name;
                            parameter.ValueType = expParams[selectedParam].valueType;
                        }

                        subRect.x = 0;
                        subRect.y = EditorGUIUtility.singleLineHeight * 3;
                        subRect.height = EditorGUIUtility.singleLineHeight;
                        subRect.width = width;
                        GUI.Label(subRect, RISStrings.GetString("param_value"), singleLinedLabelStyle);
                        subRect.x += subRect.width;
                        subRect.width = rect.width - subRect.width;
                        if (parameter.ValueType == VRCExpressionParameters.ValueType.Int)
                        {
                            EditorGUI.BeginChangeCheck();
                            var value = (int)parameter.Value;
                            value = EditorGUI.IntField(subRect, value);
                            if (EditorGUI.EndChangeCheck())
                            {
                                parameter.Value = value;
                            }
                        }
                        else if(parameter.ValueType == VRCExpressionParameters.ValueType.Bool)
                        {
                            EditorGUI.BeginChangeCheck();
                            var value = parameter.Value == 1;
                            value = EditorGUI.Toggle(subRect, value);
                            if (EditorGUI.EndChangeCheck())
                            {
                                parameter.Value = value ? 1 : 0;
                            }
                        }
                        else
                        {
                            EditorGUI.BeginChangeCheck();
                            var value = parameter.Value;
                            value = EditorGUI.FloatField(subRect, value);
                            if (EditorGUI.EndChangeCheck())
                            {
                                parameter.Value = value;
                            }
                        }
                    }
                    /*if (EditorGUI.EndChangeCheck())
                        parameter.PropIndex = selectedProp;*/



                }
                rect.x = rect.x + rect.width + 5;
                rect.width = 20f;
                if (GUI.Button(rect, ReorderableListStyle.SubContent, ReorderableListStyle.SubStyle))
                {
                    parameters.RemoveAt(index);
                    if (index >= parameters.Count)
                        index = list.index = -1;
                }
            };
            list.drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
            {
                if (isFocused)
                {
                    GUI.DrawTexture(rect, TexAssets.BlueTexture);
                }
            };
            list.drawFooterCallback = rect => { };
            list.footerHeight = 0f;
            list.elementHeightCallback = index =>
            {
                if (parameters.Count <= index)
                    return 0;
                var parameter = parameters[index];
                var isRISParam = parameter.IsRISParam;
                return EditorGUIUtility.singleLineHeight * (isRISParam ? 5 : 4) + 10;
            };
            parametersReorderableList = list;
        }

        public override string[] CheckErrors(ref Avatar risAvatar)
        {
            if (risAvatar == null)
                return new string[] { };

            var errors = new List<string>();
            if (risAvatar.Groups.Count == 0)
                errors.Add(RISStrings.GetString("err_addgroup"));

            foreach (var group in risAvatar.Groups)
            {
                var groupName = group.Name;
                if (string.IsNullOrEmpty(groupName))
                    groupName = string.Format(RISStrings.GetString("defaultname_group"), risAvatar.Groups.IndexOf(group));

                if (!group.Props.Any())
                    errors.Add(string.Format(RISStrings.GetString("error_emptygroup"), groupName));

                var maxPropsCount = 8;
                if (group.BaseMenu != null && group.BaseMenu?.GetObject() != null)
                    maxPropsCount = 8 - group.BaseMenu.GetObject().controls.Count;
                if (group.UseResetButton)
                    maxPropsCount--;
                if (group.Props.Count > maxPropsCount)
                    errors.Add(string.Format(RISStrings.GetString("err_overprop"), groupName, maxPropsCount));

                foreach (var prop in group.Props)
                {
                    var propName = prop.Name;
                    if (string.IsNullOrEmpty(propName))
                        propName = string.Format(RISStrings.GetString("defaultname_prop"), group.Props.IndexOf(prop));
                    if (risAvatar.GetAvatarRoot() != null)
                    {
                        var parentGameObject = risAvatar.GetAvatarRoot()?.gameObject;
                        if (!prop.TargetObjects.Any(n => n?.GetObject(parentGameObject) != null) && 
                            prop.DisableAnimation?.GetObject() == null &&
                            prop.EnableAnimation?.GetObject() == null &&
                            !prop.Parameters.Any(item => item != null))
                            errors.Add(string.Format(RISStrings.GetString("err_nullobjectandanim"), groupName, propName));
                    }
                }
            }
            return errors.ToArray();
        }

        public override void BuildExpressionParameters(ref Avatar risAvatar, string autoGeneratedFolder)
        {
            var avatar = risAvatar.GetAvatarRoot();

            if (avatar == null)
                return;

            var expParams = avatar.GetExpressionParameters(autoGeneratedFolder);
            if (EditorSettings.OptimizeParameters)
                expParams.OptimizeParameter();
            var parameters = expParams.parameters;
            foreach (var name in parameters.Where(n => n.name.StartsWith("RIS")).Select(n => n.name))
                expParams.TryRemoveParameter(name);
            parameters = expParams.parameters;
            expParams.parameters = parameters;
            foreach (var groupIndex in Enumerable.Range(0, risAvatar.Groups.Count))
            {
                var group = risAvatar.Groups[groupIndex];
                if (group.UseResetButton)
                    TryAddParam(ref risAvatar, $"{RIS.Prefix}-G{groupIndex}RESET", 0f, false);
                foreach (var propIndex in Enumerable.Range(0, group.Props.Count))
                {
                    var prop = group.Props[propIndex];
                    var v2Mode = prop.ExclusiveGroup != RIS.ExclusiveGroupType.None && risAvatar.GetExclusiveMode(prop.ExclusiveGroup) == RIS.ExclusiveModeType.LegacyExclusive;
                    TryAddParam(ref risAvatar, $"{RIS.Prefix}-G{groupIndex}P{propIndex}", prop.IsDefaultEnabled && !v2Mode ? 1f : 0f, prop.UseSaveParameter);
                }
            }
            TryAddParam(ref risAvatar, $"{RIS.Prefix}-Initialize", 1f, true);
            avatar.expressionParameters = expParams;
            EditorUtility.SetDirty(avatar);
            EditorUtility.SetDirty(avatar.expressionParameters);
            AssetDatabase.SaveAssets();
        }

        public override void BuildExpressionsMenu(ref Avatar risAvatar, string autoGeneratedFolder)
        {
            var avatar = risAvatar.AvatarRoot?.GetObject();

            if (avatar == null)
                return;

            VRCExpressionsMenu menu = null;
            avatar.customExpressions = true;
            var rootMenu = avatar.expressionsMenu;
            if (rootMenu == null)
                rootMenu = avatar.expressionsMenu = UnityUtils.TryGetAsset(autoGeneratedFolder + "AutoGeneratedMenu.asset", typeof(VRCExpressionsMenu)) as VRCExpressionsMenu;

            //v3.1までのtypoメニューを消す処理
            var controls = rootMenu.controls;
            var risControl = controls.FirstOrDefault(n => n.name == "Radial Inventory");
            if (risControl == null)
                controls.Add(risControl = new VRCExpressionsMenu.Control() { name = "Radial Inventory" });
            rootMenu.controls = controls;

            risControl.icon = TexAssets.MenuIcon;
            risControl.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
            risControl.subMenu = menu = UnityUtils.TryGetAsset(autoGeneratedFolder + $"RadInvMainMenu.asset", typeof(VRCExpressionsMenu)) as VRCExpressionsMenu;

            var subMenuFolder = autoGeneratedFolder + "SubMenus/";
            UnityUtils.ReCreateFolder(subMenuFolder);
            var menuControls = menu.controls;
            menuControls.Clear();
            foreach (var groupIndex in Enumerable.Range(0, risAvatar.Groups.Count))
            {
                var group = risAvatar.Groups[groupIndex];
                var groupName = group.Name;
                if (string.IsNullOrWhiteSpace(groupName))
                    groupName = "Group" + groupIndex.ToString();

                VRCExpressionsMenu.Control control = new VRCExpressionsMenu.Control();
                control.name = groupName;
                control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                control.icon = group.Icon?.GetObject();
                if (control.icon == null)
                    control.icon = TexAssets.GroupIcon;
                if (group.BaseMenu?.GetObject() != null)
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(group.BaseMenu.GetObject()), subMenuFolder + $"Group{groupIndex}Menu.asset");
                VRCExpressionsMenu subMenu = control.subMenu = UnityUtils.TryGetAsset(subMenuFolder + $"Group{groupIndex}Menu.asset", typeof(VRCExpressionsMenu)) as VRCExpressionsMenu;
                var subMenuControls = subMenu.controls;
                if (group.UseResetButton)
                {
                    var propControl = new VRCExpressionsMenu.Control();
                    propControl.name = RISStrings.GetString("reset");
                    propControl.type = VRCExpressionsMenu.Control.ControlType.Toggle;
                    propControl.value = 1f;
                    propControl.icon = TexAssets.ReloadIcon;
                    propControl.parameter = new VRCExpressionsMenu.Control.Parameter() { name = $"{RIS.Prefix}-G{groupIndex}RESET" };
                    subMenuControls.Add(propControl);
                }
                foreach (var propIndex in Enumerable.Range(0, group.Props.Count))
                {
                    var prop = group.Props[propIndex];
                    var propName = prop.GetPropName(risAvatar);
                    if (string.IsNullOrWhiteSpace(propName))
                        propName = string.Format(RISStrings.GetString("defaultname_prop"), propIndex);
                    var propControl = new VRCExpressionsMenu.Control();
                    propControl.name = propName;
                    propControl.type = VRCExpressionsMenu.Control.ControlType.Toggle;
                    propControl.value = 1f;
                    propControl.parameter = new VRCExpressionsMenu.Control.Parameter() { name = $"{RIS.Prefix}-G{groupIndex}P{propIndex}" };
                    propControl.icon = prop.Icon?.GetObject();
                    if (propControl.icon == null)
                        propControl.icon = TexAssets.BoxIcon;
                    subMenuControls.Add(propControl);
                }
                subMenu.controls = subMenuControls;
                control.subMenu = subMenu;
                menuControls.Add(control);
                EditorUtility.SetDirty(control.subMenu);
            }
            menu.controls = menuControls;
            avatar.expressionsMenu = rootMenu;
            EditorUtility.SetDirty(menu);
            EditorUtility.SetDirty(avatar.expressionsMenu);
            EditorUtility.SetDirty(avatar);
            AssetDatabase.SaveAssets();
        }
        protected override void BuildFXLayer(ref Avatar risAvatar, string autoGeneratedFolder)
        {
            var avatar = risAvatar.GetAvatarRoot();
            if (avatar == null)
                return;
            avatar.customizeAnimationLayers = true;
            var fxLayer = avatar.GetFXLayer(autoGeneratedFolder);
            var animationsFolder = autoGeneratedFolder + "Animations/";
            UnityUtils.ReCreateFolder(animationsFolder);
            foreach (var name in fxLayer.layers.Where(n => n.name.StartsWith("RIS")).Select(n => n.name))
                fxLayer.TryRemoveLayer(name);
            var parameters = fxLayer.parameters;
            foreach (var name in parameters.Where(n => n.name.StartsWith("RIS")).Select(n => n.name))
                fxLayer.TryRemoveParameter(name);
            parameters = fxLayer.parameters;

            List<int> resetButtonGroups = new List<int>();
            Dictionary<IndexPair, Prop> propLayer_OffTimer = new Dictionary<IndexPair, Prop>();
            Dictionary<IndexPair, Prop> propLayer_PropToggles = new Dictionary<IndexPair, Prop>();
            Dictionary<IndexPair, Prop> propLayer_PropTogglesLE = new Dictionary<IndexPair, Prop>();
            Dictionary<IndexPair, Prop> propLayer_MaterialToggles = new Dictionary<IndexPair, Prop>();
            Dictionary<IndexPair, Prop> propLayer_AnimationToggles = new Dictionary<IndexPair, Prop>();
            Dictionary<IndexPair, Prop> propLayer_AnimationTogglesLE = new Dictionary<IndexPair, Prop>();

            var exclusiveGroups = Enum.GetNames(typeof(RIS.ExclusiveGroupType)).ToList();
            exclusiveGroups.Remove(RIS.ExclusiveGroupType.None.ToString());
            List<IndexPair>[] exclusiveGroupIndexes = exclusiveGroups.Select(n => new List<IndexPair>()).ToArray();
            var fallbackClip = new AnimationClip();
            var fallbackParamName = $"{RIS.Prefix}-Initialize";

            var noneClip = new AnimationClip();
            var noneClipName = "RIS_None";
            AssetDatabase.CreateAsset(noneClip, animationsFolder + $"{noneClipName}.anim");
            EditorUtility.SetDirty(noneClip);

            foreach (var groupIndex in Enumerable.Range(0, risAvatar.Groups.Count))
            {
                var group = risAvatar.Groups[groupIndex];
                if (group.UseResetButton)
                    resetButtonGroups.Add(groupIndex);
                foreach (var propIndex in Enumerable.Range(0, group.Props.Count))
                {
                    var prop = group.Props[propIndex];

                    foreach (var targetObject in prop.TargetObjects)
                    {
                        var gameObject = targetObject?.GetObject(avatar.gameObject);
                        if (gameObject != null)
                        {
                            if(!prop.MaterialOverrides.Any(item => item != null && item.GetObject() != null))
                            {
                                if (EditorSettings.ApplyEnableDefault)
                                    gameObject.SetActive(prop.IsDefaultEnabled);


                                var relativePath = gameObject.GetRelativePath(avatar.gameObject, false);
                                var curve = new AnimationCurve();
                                var frameValue = prop.IsDefaultEnabled ? 1 : 0;
                                curve.AddKey(0f, frameValue);
                                curve.AddKey(1f / fallbackClip.frameRate, frameValue);
                                fallbackClip.SetCurve(relativePath, typeof(GameObject), "m_IsActive", curve);
                            }
                        }
                    }


                    var pair = new IndexPair() { GroupIndex = groupIndex, PropIndex = propIndex };

                    if (prop.UseResetTimer)
                        propLayer_OffTimer.Add(pair, prop);

                    var exclusiveMode = risAvatar.GetExclusiveMode(prop.ExclusiveGroup);
                    if(prop.ExclusiveGroup != RIS.ExclusiveGroupType.None && exclusiveMode == RIS.ExclusiveModeType.None)
                    {
                        exclusiveMode = RIS.ExclusiveModeType.Exclusive;
                        risAvatar.SetExclusiveMode(prop.ExclusiveGroup, exclusiveMode);
                    }
                    var paramOperate = prop.Parameters.Any(item => item != null);
                    var containGameObject = prop.TargetObjects.Any(v => v?.GetObject(avatar.gameObject) != null);
                    var containAnimation = prop.EnableAnimation?.GetObject() != null || prop.DisableAnimation?.GetObject() != null;
                    var containMaterialOverride = prop.MaterialOverrides.Any(v => v?.GetObject() != null);
                    if (exclusiveMode == RIS.ExclusiveModeType.LegacyExclusive)
                    {
                        if (containGameObject || paramOperate)
                        {
                            if (containMaterialOverride)
                                propLayer_MaterialToggles.Add(pair, prop);
                            if(!containMaterialOverride || (containMaterialOverride && paramOperate))
                                propLayer_PropTogglesLE.Add(pair, prop);
                            if (prop.ExclusiveGroup != RIS.ExclusiveGroupType.None && !exclusiveGroupIndexes[(int)prop.ExclusiveGroup].Any(v => v.GroupIndex == pair.GroupIndex && v.PropIndex == pair.PropIndex))
                                exclusiveGroupIndexes[(int)prop.ExclusiveGroup].Add(pair);
                        }
                        if (containAnimation)
                        {
                            propLayer_AnimationTogglesLE.Add(pair, prop);
                            if (prop.ExclusiveGroup != RIS.ExclusiveGroupType.None && !exclusiveGroupIndexes[(int)prop.ExclusiveGroup].Any(v => v.GroupIndex == pair.GroupIndex && v.PropIndex == pair.PropIndex))
                                exclusiveGroupIndexes[(int)prop.ExclusiveGroup].Add(pair);
                        }
                    }
                    else
                    {
                        if (containGameObject || paramOperate)
                        {
                            if (containMaterialOverride)
                                propLayer_MaterialToggles.Add(pair, prop);
                            if (!containMaterialOverride || (containMaterialOverride && paramOperate))
                                propLayer_PropToggles.Add(pair, prop);
                            if (prop.ExclusiveGroup != RIS.ExclusiveGroupType.None && !exclusiveGroupIndexes[(int)prop.ExclusiveGroup].Any(v => v.GroupIndex == pair.GroupIndex && v.PropIndex == pair.PropIndex))
                                exclusiveGroupIndexes[(int)prop.ExclusiveGroup].Add(pair);
                        }
                        if (containAnimation)
                        {
                            propLayer_AnimationToggles.Add(pair, prop);
                            if (prop.ExclusiveGroup != RIS.ExclusiveGroupType.None && !exclusiveGroupIndexes[(int)prop.ExclusiveGroup].Any(v => v.GroupIndex == pair.GroupIndex && v.PropIndex == pair.PropIndex))
                                exclusiveGroupIndexes[(int)prop.ExclusiveGroup].Add(pair);
                        }
                    }

                }

            }

            CheckParam(avatar, fxLayer, fallbackParamName, false);
            EditorUtility.SetDirty(fallbackClip);

            //Off Timer
            foreach (var pair in propLayer_OffTimer)
            {
                var groupIndex = pair.Key.GroupIndex;
                var propIndex = pair.Key.PropIndex;
                var prop = pair.Value;
                var paramName = $"{RIS.Prefix}-G{groupIndex}P{propIndex}";

                var timerLayerName = $"{RIS.Prefix}-TIMER-G{groupIndex}P{propIndex}";
                var timerLayer = fxLayer.FindAnimatorControllerLayer(timerLayerName);
                if (timerLayer == null)
                    timerLayer = fxLayer.AddAnimatorControllerLayer(timerLayerName);
                var timerStateMachine = timerLayer.stateMachine;
                timerStateMachine.Clear();

                var waitState = timerStateMachine.AddState("WaitTimer", new Vector3(300, 100, 0));
                waitState.writeDefaultValues = risAvatar.UseWriteDefaults;
                waitState.motion = noneClip;
                var countdownState = timerStateMachine.AddState("Countdown", new Vector3(300, 200, 0));
                countdownState.writeDefaultValues = risAvatar.UseWriteDefaults;
                countdownState.motion = noneClip;
                var stopState = timerStateMachine.AddState("StopTimer", new Vector3(600, 200, 0));
                stopState.writeDefaultValues = risAvatar.UseWriteDefaults;
                stopState.motion = noneClip;

                var transition = countdownState.MakeTransition(stopState);
                transition.exitTime = prop.ResetSecond;
                transition.hasExitTime = true;
                transition.hasFixedDuration = true;

                transition = timerStateMachine.MakeAnyStateTransition(waitState);
                transition.CreateSingleCondition(prop.IsDefaultEnabled ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, paramName, 1f, false, false);
                transition = timerStateMachine.MakeAnyStateTransition(countdownState);
                transition.CreateSingleCondition(prop.IsDefaultEnabled ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, paramName, 1f, false, false);

                var driver = stopState.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                var driverParameters = driver.parameters;
                CheckParam(avatar, fxLayer, $"{RIS.Prefix}-G{groupIndex}P{propIndex}", prop.IsDefaultEnabled);
                driverParameters.Add(new VRC_AvatarParameterDriver.Parameter()
                {
                    name = $"{RIS.Prefix}-G{groupIndex}P{propIndex}",
                    type = VRC_AvatarParameterDriver.ChangeType.Set,
                    value = prop.IsDefaultEnabled ? 1 : 0
                });
                driver.parameters = driverParameters;
                EditorUtility.SetDirty(timerLayer.stateMachine);
            }
            //ResetButton
            foreach(var groupIndex in resetButtonGroups)
            {
                var group = risAvatar.Groups[groupIndex];
                var layerName = $"{RIS.Prefix}-RESET-G{groupIndex}";

                var paramName = $"{RIS.Prefix}-G{groupIndex}RESET";
                CheckParam(avatar, fxLayer, paramName, false);

                var layer = fxLayer.FindAnimatorControllerLayer(layerName);
                if (layer == null)
                    layer = fxLayer.AddAnimatorControllerLayer(layerName);
                var stateMachine = layer.stateMachine;
                stateMachine.Clear();

                var onState = stateMachine.AddState("Reset", new Vector3(300, 100, 0));
                onState.writeDefaultValues = risAvatar.UseWriteDefaults;
                onState.motion = noneClip;
                var offState = stateMachine.AddState("Wait", new Vector3(300, 200, 0));
                offState.writeDefaultValues = risAvatar.UseWriteDefaults;
                offState.motion = noneClip;

                var transition = stateMachine.MakeAnyStateTransition(onState);
                transition.CreateSingleCondition(AnimatorConditionMode.If, paramName, 1f, false, false);
                transition = stateMachine.MakeAnyStateTransition(offState);
                transition.CreateSingleCondition(AnimatorConditionMode.IfNot, paramName, 1f, false, false);

                stateMachine.defaultState = offState;

                var driver = onState.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                var driverParameters = driver.parameters;
                foreach(var num in Enumerable.Range(0, group.Props.Count))
                {
                    var subParamName = $"{RIS.Prefix}-G{groupIndex}P{num}";
                    CheckParam(avatar, fxLayer, subParamName, group.Props[num].IsDefaultEnabled);
                    driverParameters.Add(new VRC_AvatarParameterDriver.Parameter()
                    {
                        name = subParamName,
                        type = VRC_AvatarParameterDriver.ChangeType.Set,
                        value = group.Props[num].IsDefaultEnabled ? 1 : 0
                    });
                }
                CheckParam(avatar, fxLayer, $"{RIS.Prefix}-G{groupIndex}RESET", false);
                driverParameters.Add(new VRC_AvatarParameterDriver.Parameter()
                {
                    name = $"{RIS.Prefix}-G{groupIndex}RESET",
                    type = VRC_AvatarParameterDriver.ChangeType.Set,
                    value = 0
                });
                driver.parameters = driverParameters;
                EditorUtility.SetDirty(layer.stateMachine);

            }
            //ExclusiveToggles
            foreach (var exclusiveGroupIndex in Enumerable.Range(0, exclusiveGroupIndexes.Length))
            {
                var exclusiveGroup = (RIS.ExclusiveGroupType)exclusiveGroupIndex;
                var indexes = exclusiveGroupIndexes[exclusiveGroupIndex];

                if (!indexes.Any())
                    continue;

                var layerName = $"{RIS.Prefix}-PARAM-G{exclusiveGroupIndex}";
                var animLayer = fxLayer.FindAnimatorControllerLayer(layerName);
                if (animLayer == null)
                    animLayer = fxLayer.AddAnimatorControllerLayer(layerName);
                var stateMachine = animLayer.stateMachine;
                stateMachine.Clear();

                var stateName = "DEFAULT";
                var defaultState = stateMachine.AddState(stateName, new Vector3(300, 150, 0));
                defaultState.writeDefaultValues = risAvatar.UseWriteDefaults;
                defaultState.motion = noneClip;
                var defaultTransition = stateMachine.MakeAnyStateTransition(defaultState);
                stateMachine.defaultState = defaultState;

                foreach (var pairIndex in Enumerable.Range(0, indexes.Count))
                {
                    var pair = indexes[pairIndex];
                    var groupIndex = pair.GroupIndex;
                    var propIndex = pair.PropIndex;
                    var paramName = $"{RIS.Prefix}-G{groupIndex}P{propIndex}";
                    CheckParam(avatar, fxLayer, paramName, false);
                    defaultTransition.AddCondition(AnimatorConditionMode.IfNot, paramName, 1f, false, true);


                    stateName = $"G{pair.GroupIndex}P{pair.PropIndex}ON";
                    var state = stateMachine.AddState(stateName, new Vector3(300, 200 + (pairIndex * 50), 0));
                    state.writeDefaultValues = risAvatar.UseWriteDefaults;
                    state.motion = noneClip;
                    var driver = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                    var transition = stateMachine.MakeAnyStateTransition(state);
                    transition.CreateSingleCondition(AnimatorConditionMode.If, paramName, 1f, false, true);
                    foreach (var subPairIndex in Enumerable.Range(0, indexes.Count))
                    {
                        var subPair = indexes[subPairIndex];
                        var subGroupIndex = subPair.GroupIndex;
                        var subPropIndex = subPair.PropIndex;
                        if (groupIndex != subGroupIndex || propIndex != subPropIndex)
                        {
                            var driverParameters = driver.parameters;
                            var subParamName = $"{RIS.Prefix}-G{subGroupIndex}P{subPropIndex}";
                            CheckParam(avatar, fxLayer, subParamName, false);
                            driverParameters.Add(new VRC_AvatarParameterDriver.Parameter()
                            {
                                name = subParamName,
                                type = VRC_AvatarParameterDriver.ChangeType.Set,
                                value = 0
                            });
                            driver.parameters = driverParameters;
                        }
                    }
                    EditorUtility.SetDirty(driver);

                }
                EditorUtility.SetDirty(animLayer.stateMachine);
            }

            //Prop Toggles(Exclusive=None)
            foreach (var pair in propLayer_PropToggles)
            {
                var groupIndex = pair.Key.GroupIndex;
                var propIndex = pair.Key.PropIndex;
                var prop = pair.Value;
                var paramName = $"{RIS.Prefix}-G{groupIndex}P{propIndex}";

                var layerName = $"{RIS.Prefix}-MAIN-G{groupIndex}P{propIndex}";
                Debug.Log(layerName);
                var animLayer = fxLayer.FindAnimatorControllerLayer(layerName);
                if (animLayer == null)
                    animLayer = fxLayer.AddAnimatorControllerLayer(layerName);
                var stateMachine = animLayer.stateMachine;
                stateMachine.Clear();

                var onState = stateMachine.AddState("PropON", new Vector3(300, 100, 0));
                onState.writeDefaultValues = risAvatar.UseWriteDefaults;
                var offState = stateMachine.AddState("PropOFF", new Vector3(300, 200, 0));
                offState.writeDefaultValues = risAvatar.UseWriteDefaults;

                CheckParam(avatar, fxLayer, paramName, prop.IsDefaultEnabled);
                if (prop.IsLocalOnly)
                    CheckParam(avatar, fxLayer, "IsLocal", false);

                var transition = stateMachine.MakeAnyStateTransition(onState);
                transition.CreateSingleCondition(AnimatorConditionMode.If, paramName, 1f, prop.IsLocalOnly && !prop.IsDefaultEnabled, true);
                transition = stateMachine.MakeAnyStateTransition(offState);
                transition.CreateSingleCondition(AnimatorConditionMode.IfNot, paramName, 1f, prop.IsLocalOnly && prop.IsDefaultEnabled, true);

                stateMachine.defaultState = prop.IsDefaultEnabled ? onState : offState;

                var containGameObject = prop.TargetObjects.Any(v => v?.GetObject(avatar.gameObject) != null);
                var containMaterialOverride = prop.MaterialOverrides.Any(v => v?.GetObject() != null);
                if (containGameObject && !containMaterialOverride)
                {
                    var clipON = new AnimationClip();
                    var clipOFF = new AnimationClip();

                    var clipName = "G" + groupIndex.ToString() + "P" + propIndex.ToString();
                    foreach (var gameObject in prop.TargetObjects)
                    {
                        var targetObject = gameObject?.GetObject(avatar.gameObject);
                        if (targetObject != null)
                        {
                            var relativePath = targetObject.GetRelativePath(avatar.gameObject, false);
                            var curve = new AnimationCurve();
                            curve.AddKey(0f, 1);
                            curve.AddKey(1f / clipON.frameRate, 1);
                            clipON.SetCurve(relativePath, typeof(GameObject), "m_IsActive", curve);

                            curve = new AnimationCurve();
                            curve.AddKey(0f, 0);
                            curve.AddKey(1f / clipOFF.frameRate, 0);
                            clipOFF.SetCurve(relativePath, typeof(GameObject), "m_IsActive", curve);
                        }
                    }
                    AssetDatabase.CreateAsset(clipON, animationsFolder + $"{clipName}ON" + ".anim");
                    onState.motion = clipON;
                    EditorUtility.SetDirty(clipON);

                    AssetDatabase.CreateAsset(clipOFF, animationsFolder + $"{clipName}OFF" + ".anim");
                    offState.motion = clipOFF;
                    EditorUtility.SetDirty(clipOFF);
                }
                else
                {
                    onState.motion = noneClip;
                    offState.motion = noneClip;
                }
                var onParameter = prop.Parameters.Where(item => item != null && item.ParameterTriggerType == RIS.ParameterTriggerType.TurnON);
                if (onParameter.Any())
                {
                    var stateDriver = onState.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                    var stateDriverParameters = stateDriver.parameters;
                    foreach (var parameter in onParameter)
                    {
                        CheckParam(avatar, fxLayer, parameter);
                        stateDriverParameters.Add(new VRC_AvatarParameterDriver.Parameter()
                        {
                            name = parameter.Name,
                            type = VRC_AvatarParameterDriver.ChangeType.Set,
                            value = parameter.Value
                        });
                    }
                    stateDriver.parameters = stateDriverParameters;
                    EditorUtility.SetDirty(stateDriver);
                }
                var offParameter = prop.Parameters.Where(item => item != null && item.ParameterTriggerType == RIS.ParameterTriggerType.TurnOFF);
                if (offParameter.Any())
                {
                    var stateDriver = offState.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                    var stateDriverParameters = stateDriver.parameters;
                    foreach (var parameter in offParameter)
                    {
                        CheckParam(avatar, fxLayer, parameter);
                        stateDriverParameters.Add(new VRC_AvatarParameterDriver.Parameter()
                        {
                            name = parameter.Name,
                            type = VRC_AvatarParameterDriver.ChangeType.Set,
                            value = parameter.Value
                        });
                    }
                    stateDriver.parameters = stateDriverParameters;
                    EditorUtility.SetDirty(stateDriver);
                }
                EditorUtility.SetDirty(animLayer.stateMachine);
            }
            //Props Toggle(LegacyExclusive)
            var exclusiveGroupTypes = Enum.GetValues(typeof(RIS.ExclusiveGroupType));
            Dictionary <RIS.ExclusiveGroupType, List<IndexPair>> exclusiveIndexPairs = new Dictionary<RIS.ExclusiveGroupType, List<IndexPair>>();
            foreach (RIS.ExclusiveGroupType enumValue in exclusiveGroupTypes)
                exclusiveIndexPairs.Add(enumValue, new List<IndexPair>());
            foreach (var pair in propLayer_PropTogglesLE)
                exclusiveIndexPairs[pair.Value.ExclusiveGroup].Add(pair.Key);


            foreach (RIS.ExclusiveGroupType enumValue in exclusiveGroupTypes)
            {
                if (enumValue == RIS.ExclusiveGroupType.None)
                    continue;
                var indexPairs = exclusiveIndexPairs[enumValue];
                if (indexPairs.Count <= 0)
                    continue;

                var layerName = $"{RIS.Prefix}-EXCLUSIVE-{(int)enumValue}";
                var animLayer = fxLayer.FindAnimatorControllerLayer(layerName);
                if (animLayer == null)
                    animLayer = fxLayer.AddAnimatorControllerLayer(layerName);
                var stateMachine = animLayer.stateMachine;
                stateMachine.Clear();

                var stateName = "DEFAULT";
                var defaultState = stateMachine.AddState(stateName, new Vector3(300, 150, 0));
                defaultState.writeDefaultValues = risAvatar.UseWriteDefaults;
                var defaultTransition = stateMachine.MakeAnyStateTransition(defaultState);
                stateMachine.defaultState = defaultState;
                var defaultClip = new AnimationClip();
                var indexPairCounts = Enumerable.Range(0, indexPairs.Count);
                var risAvatar1 = risAvatar;
                var operateParams = indexPairs.Any(item =>
                {
                    var prop = risAvatar1.Groups[item.GroupIndex].Props[item.PropIndex];
                    return prop.Parameters.Any(subItem => subItem != null);
                });

                VRCAvatarParameterDriver stateDriver = null;
                List<VRCAvatarParameterDriver.Parameter> stateDriverParameters = null;
                if (operateParams)
                {
                    stateDriver = defaultState.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                    stateDriverParameters = stateDriver.parameters;
                }
                foreach (var pairIndex in indexPairCounts)
                {
                    var pair = indexPairs[pairIndex];
                    var groupIndex = pair.GroupIndex;
                    var propIndex = pair.PropIndex;
                    var group = risAvatar.Groups[groupIndex];
                    var prop = group.Props[propIndex];
                    var paramName = $"{RIS.Prefix}-G{groupIndex}P{propIndex}";

                    var containMaterialOverride = prop.MaterialOverrides.Any(v => v?.GetObject() != null);
                    if (!containMaterialOverride)
                    {
                        var defaultCurve = new AnimationCurve();
                        var defaultFrameValue = prop.IsDefaultEnabled ? 1 : 0;
                        defaultCurve.AddKey(0f, defaultFrameValue);
                        defaultCurve.AddKey(1f / defaultClip.frameRate, defaultFrameValue);
                        foreach (var gameObject in prop.TargetObjects)
                        {
                            var targetObject = gameObject?.GetObject(avatar.gameObject);
                            if (targetObject != null)
                            {
                                defaultClip.SetCurve(targetObject.GetRelativePath(avatar.gameObject, false), typeof(GameObject), "m_IsActive", defaultCurve);
                            }
                        }
                    }
                    if (operateParams && prop.Parameters.Any(item => item != null))
                    {
                        foreach (var parameter in prop.Parameters)
                        {
                            if (prop.IsDefaultEnabled == (parameter.ParameterTriggerType == RIS.ParameterTriggerType.TurnON))
                            {
                                CheckParam(avatar, fxLayer, parameter);
                                stateDriverParameters.Add(new VRC_AvatarParameterDriver.Parameter()
                                {
                                    name = parameter.Name,
                                    type = VRC_AvatarParameterDriver.ChangeType.Set,
                                    value = parameter.Value
                                });
                            }
                        }
                    }
                    CheckParam(avatar, fxLayer, paramName, prop.IsDefaultEnabled);
                    defaultTransition.AddCondition(AnimatorConditionMode.IfNot, paramName, 1f, false, true);
                }
                if (operateParams)
                {
                    stateDriver.parameters = stateDriverParameters;
                    EditorUtility.SetDirty(stateDriver);
                }

                var clipName = $"PAIR{(int)enumValue}-{stateName}";
                AssetDatabase.CreateAsset(defaultClip, animationsFolder + clipName + ".anim");
                EditorUtility.SetDirty(defaultClip);
                defaultState.motion = defaultClip;

                foreach (var pairIndex in indexPairCounts)
                {
                    var mainPair = indexPairs[pairIndex];
                    stateName = $"G{mainPair.GroupIndex}P{mainPair.PropIndex}ON";
                    var state = stateMachine.AddState(stateName, new Vector3(300, 200 + (pairIndex * 50), 0));
                    state.writeDefaultValues = risAvatar.UseWriteDefaults;
                    var clip = new AnimationClip();
                    AnimatorStateTransition transition = null;
                    stateDriver = null;
                    stateDriverParameters = null;
                    if (operateParams)
                    {
                        stateDriver = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                        stateDriverParameters = stateDriver.parameters;
                    }
                    foreach (var subPairIndex in Enumerable.Range(0, indexPairs.Count))
                    {
                        var pair = indexPairs[subPairIndex];
                        var groupIndex = pair.GroupIndex;
                        var propIndex = pair.PropIndex;
                        var subProp = risAvatar.Groups[groupIndex].Props[propIndex];

                        var containMaterialOverride = subProp.MaterialOverrides.Any(v => v?.GetObject() != null);
                        if (!containMaterialOverride)
                        {
                            var curve = new AnimationCurve();
                            var frameValue = pairIndex == subPairIndex ? 1 : 0;
                            foreach (var gameObject in subProp.TargetObjects)
                            {
                                var targetObject = gameObject?.GetObject(avatar.gameObject);
                                if (targetObject != null)
                                {
                                    curve.AddKey(0f, frameValue);
                                    curve.AddKey(1f / clip.frameRate, frameValue);
                                    clip.SetCurve(targetObject.GetRelativePath(avatar.gameObject, false), typeof(GameObject), "m_IsActive", curve);
                                }
                            }
                        }

                        if (operateParams && subProp.Parameters.Any(item => item != null))
                        {
                            foreach (var parameter in subProp.Parameters)
                            {
                                //default : isTurnOn
                                //false : on -> off>on (nocall>call)
                                //false : off -> off>on (call>nocall)
                                //true : on -> on>off (call>nocall)
                                //true : off -> on>off (nocall>call)
                                var correct = subProp.IsDefaultEnabled == (parameter.ParameterTriggerType == RIS.ParameterTriggerType.TurnON);
                                var flag = correct == (pairIndex == subPairIndex);
                                if (flag)
                                {
                                    CheckParam(avatar, fxLayer, parameter);
                                    stateDriverParameters.Add(new VRC_AvatarParameterDriver.Parameter()
                                    {
                                        name = parameter.Name,
                                        type = VRC_AvatarParameterDriver.ChangeType.Set,
                                        value = parameter.Value
                                    });
                                }
                            }
                        }
                    }
                    var paramName = $"{RIS.Prefix}-G{mainPair.GroupIndex}P{mainPair.PropIndex}";
                    transition = stateMachine.MakeAnyStateTransition(state);
                    var prop = risAvatar.Groups[mainPair.GroupIndex].Props[mainPair.PropIndex];
                    CheckParam(avatar, fxLayer, paramName, prop.IsDefaultEnabled);
                    if (prop.IsLocalOnly)
                        CheckParam(avatar, fxLayer, "IsLocal", false);
                    transition.CreateSingleCondition(AnimatorConditionMode.If, paramName, 1f, prop.IsLocalOnly && !prop.IsDefaultEnabled, true);

                    if (operateParams)
                    {
                        stateDriver.parameters = stateDriverParameters;
                        EditorUtility.SetDirty(stateDriver);
                    }

                    clipName = $"PAIR{(int)enumValue}-{stateName}";
                    AssetDatabase.CreateAsset(clip, animationsFolder + clipName + ".anim");
                    EditorUtility.SetDirty(clip);
                    state.motion = clip;
                }
                EditorUtility.SetDirty(animLayer.stateMachine);
            }
            //Material Toggles
            var materialDatas = new Dictionary<int, List<MaterialOverrideData>>();
            foreach (var pair in propLayer_MaterialToggles)
            {
                var groupIndex = pair.Key.GroupIndex;
                var propIndex = pair.Key.PropIndex;
                var prop = pair.Value;
                foreach (var gameObject in prop.TargetObjects)
                {
                    var targetObject = gameObject?.GetObject(avatar.gameObject);
                    if (targetObject != null)
                    {
                        var instanceID = targetObject.GetInstanceID();
                        if (!materialDatas.ContainsKey(instanceID))
                            materialDatas.Add(instanceID, new List<MaterialOverrideData>());
                        materialDatas[instanceID].Add(new MaterialOverrideData() { materials = prop.MaterialOverrides.Select(v => v?.GetObject()).ToList(), index = new IndexPair() { GroupIndex = groupIndex, PropIndex = propIndex } });
                    }
                }
            }
            foreach (var v in Enumerable.Range(0, materialDatas.Count))
            {
                
                var materialOverride = materialDatas.ToList()[v];
                var materials = materialOverride.Value;
                var instanceID = materialOverride.Key;
                var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                var path = gameObject.GetRelativePath(avatar.gameObject, false);
                var renderType = typeof(Renderer);
                if (gameObject.GetComponent<MeshRenderer>() != null)
                    renderType = typeof(MeshRenderer);
                if (gameObject.GetComponent<SkinnedMeshRenderer>() != null)
                    renderType = typeof(SkinnedMeshRenderer);

                var renderer = gameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var baseMaterialPath = AssetDatabase.GetAssetPath(renderer.sharedMaterial);
                    var baseMaterial = AssetDatabase.LoadAssetAtPath(baseMaterialPath, typeof(Material)) as Material;
                    if (baseMaterial != null)
                    {
                        var layerName = $"{RIS.Prefix}-MATERIAL-{v}";


                        var layer = fxLayer.FindAnimatorControllerLayer(layerName);
                        if (layer == null)
                            layer = fxLayer.AddAnimatorControllerLayer(layerName);
                        var stateMachine = layer.stateMachine;
                        stateMachine.Clear();

                        var baseMaterialProperties = MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { baseMaterial });
                        var properties = new List<string>();
                        foreach (var materialIndex in Enumerable.Range(0, materials.Count))
                        {
                            var materialReference = materials[materialIndex];
                            //var material = materialReference.material;
                            var clipON = new AnimationClip();
                            var prop = risAvatar.Groups[materialReference.index.GroupIndex].Props[materialReference.index.PropIndex];
                            var onState = stateMachine.AddState($"MATERIAL-{materialIndex}", new Vector3(300, 50 * (materialIndex + 1), 0));
                            onState.writeDefaultValues = risAvatar.UseWriteDefaults;
                            var onTransition = stateMachine.MakeAnyStateTransition(onState);

                            var paramName = $"{RIS.Prefix}-G{materialReference.index.GroupIndex}P{materialReference.index.PropIndex}";
                            CheckParam(avatar, fxLayer, paramName, prop.IsDefaultEnabled);
                            if (prop.IsLocalOnly)
                                CheckParam(avatar, fxLayer, "IsLocal", false);
                            onTransition.CreateSingleCondition(AnimatorConditionMode.If, paramName, 1f, prop.IsLocalOnly && !prop.IsDefaultEnabled, true);

                            var dict = new Dictionary<string, int>();
                            if (prop.ExclusiveGroup != RIS.ExclusiveGroupType.None)
                            {
                                foreach (var subGroupIndex in Enumerable.Range(0, risAvatar.Groups.Count))
                                {
                                    var subGroup = risAvatar.Groups[subGroupIndex];
                                    foreach (var subPropIndex in Enumerable.Range(0, subGroup.Props.Count))
                                    {
                                        var subProp = subGroup.Props[subPropIndex];
                                        var groupIndex = materialReference.index.GroupIndex;
                                        var propIndex = materialReference.index.PropIndex;
                                        if (!(groupIndex == subGroupIndex && propIndex == subPropIndex) && subProp.ExclusiveGroup == prop.ExclusiveGroup)
                                        {
                                            var groupMode = risAvatar.GetExclusiveMode(prop.ExclusiveGroup);
                                            var isV2Mode = groupMode == RIS.ExclusiveModeType.LegacyExclusive;
                                            var paramKey = $"{RIS.Prefix}-G{subGroupIndex}P{subPropIndex}";
                                            var paramValue = isV2Mode && subProp.IsDefaultEnabled ? 1 : 0;
                                            if (dict.ContainsKey(paramKey))
                                                dict[paramKey] = paramValue;
                                            else
                                                dict.Add(paramKey, paramValue);
                                        }
                                    }
                                }
                            }
                            foreach (var subMaterialIndex in Enumerable.Range(0, materials.Count))
                            {
                                if (subMaterialIndex != materialIndex)
                                {
                                    var subMaterial = materials[subMaterialIndex];
                                    var groupIndex = subMaterial.index.GroupIndex;
                                    var propIndex = subMaterial.index.PropIndex;

                                    var subProp = risAvatar.Groups[groupIndex].Props[propIndex];
                                    var paramKey = $"{RIS.Prefix}-G{groupIndex}P{propIndex}";
                                    CheckParam(avatar, fxLayer, paramName, subProp.IsDefaultEnabled);
                                    if (subProp.IsLocalOnly)
                                        CheckParam(avatar, fxLayer, "IsLocal", false);

                                    if (dict.ContainsKey(paramKey))
                                        dict[paramKey] = 0;
                                    else
                                        dict.Add(paramKey, 0);
                                }
                            }
                            var removeKey = $"{RIS.Prefix}-G{materialReference.index.GroupIndex}P{materialReference.index.PropIndex}";
                            if (dict.ContainsKey(removeKey))
                                dict.Remove(removeKey);
                            var driver = onState.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                            foreach (var driverItem in dict)
                            {
                                CheckParam(avatar, fxLayer, driverItem.Key, driverItem.Value);
                                driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter()
                                {
                                    name = driverItem.Key,
                                    type = VRC_AvatarParameterDriver.ChangeType.Set,
                                    value = driverItem.Value
                                });
                            }

                            foreach (var index in Enumerable.Range(0, materialReference.materials.Count))
                            {
                                var material = materialReference.materials[index];
                                var addMaterialProperties = MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { material });
                                EditorCurveBinding curveBindingOn = EditorCurveBinding.PPtrCurve(path, renderType, $"m_Materials.Array.data[{index}]");
                                ObjectReferenceKeyframe[] keyFramesOn = new ObjectReferenceKeyframe[2];
                                keyFramesOn[0] = new ObjectReferenceKeyframe() { time = 0f, value = material };
                                keyFramesOn[1] = new ObjectReferenceKeyframe() { time = 1f / clipON.frameRate, value = material };
                                AnimationUtility.SetObjectReferenceCurve(clipON, curveBindingOn, keyFramesOn);
                                if (EditorSettings.MaterialAnimationType != RIS.MaterialAnimationType.MaterialOnly)
                                {
                                    var useAllProperty = EditorSettings.MaterialAnimationType == RIS.MaterialAnimationType.All;
                                    foreach (var property in addMaterialProperties)
                                    {
                                        var addMaterialPropertyName = property.name;
                                        var containFlag = baseMaterialProperties.Any(item => item.name == addMaterialPropertyName);
                                        if (useAllProperty || containFlag)
                                        {
                                            properties.Add(addMaterialPropertyName);
                                            MaterialProperty baseMatProperty = null;
                                            if(containFlag)
                                                baseMatProperty = baseMaterialProperties.First(n => n.name == addMaterialPropertyName);

                                            if (property.type == MaterialProperty.PropType.Color)
                                            {
                                                if (useAllProperty || baseMatProperty.colorValue.r != property.colorValue.r || baseMatProperty.colorValue.g != property.colorValue.g ||
                                                    baseMatProperty.colorValue.g != property.colorValue.b || baseMatProperty.colorValue.g != property.colorValue.a)
                                                {
                                                    curveBindingOn = EditorCurveBinding.FloatCurve(path, renderType, $"material.{addMaterialPropertyName}.r");
                                                    var curve = new AnimationCurve();
                                                    curve.AddKey(0f, property.colorValue.r);
                                                    curve.AddKey(1f / clipON.frameRate, property.colorValue.r);
                                                    AnimationUtility.SetEditorCurve(clipON, curveBindingOn, curve);

                                                    curveBindingOn = EditorCurveBinding.FloatCurve(path, renderType, $"material.{addMaterialPropertyName}.g");
                                                    curve = new AnimationCurve();
                                                    curve.AddKey(0f, property.colorValue.g);
                                                    curve.AddKey(1f / clipON.frameRate, property.colorValue.g);
                                                    AnimationUtility.SetEditorCurve(clipON, curveBindingOn, curve);

                                                    curveBindingOn = EditorCurveBinding.FloatCurve(path, renderType, $"material.{addMaterialPropertyName}.b");
                                                    curve = new AnimationCurve();
                                                    curve.AddKey(0f, property.colorValue.b);
                                                    curve.AddKey(1f / clipON.frameRate, property.colorValue.b);
                                                    AnimationUtility.SetEditorCurve(clipON, curveBindingOn, curve);

                                                    curveBindingOn = EditorCurveBinding.FloatCurve(path, renderType, $"material.{addMaterialPropertyName}.a");
                                                    curve = new AnimationCurve();
                                                    curve.AddKey(0f, property.colorValue.a);
                                                    curve.AddKey(1f / clipON.frameRate, property.colorValue.a);
                                                    AnimationUtility.SetEditorCurve(clipON, curveBindingOn, curve);
                                                }
                                            }
                                            if (property.type == MaterialProperty.PropType.Float)
                                            {
                                                if (useAllProperty || baseMatProperty.floatValue != property.floatValue)
                                                {
                                                    curveBindingOn = EditorCurveBinding.FloatCurve(path, renderType, $"material.{addMaterialPropertyName}");
                                                    var curve = new AnimationCurve();
                                                    curve.AddKey(0f, property.floatValue);
                                                    curve.AddKey(1f / clipON.frameRate, property.floatValue);
                                                    AnimationUtility.SetEditorCurve(clipON, curveBindingOn, curve);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            var clipONName = $"MAT-G{v}P{materialIndex}ON";
                            AssetDatabase.CreateAsset(clipON, animationsFolder + clipONName + ".anim");
                            onState.motion = clipON;
                            EditorUtility.SetDirty(clipON);
                        }

                        var clipOFF = new AnimationClip();
                        var offState = stateMachine.AddState(baseMaterial.name, new Vector3(300, 0, 0));
                        offState.writeDefaultValues = risAvatar.UseWriteDefaults;

                        var curveBindingOff = EditorCurveBinding.PPtrCurve(path, renderType, "m_Materials.Array.data[0]");
                        var keyFramesOff = new ObjectReferenceKeyframe[2];
                        keyFramesOff[0] = new ObjectReferenceKeyframe() { time = 0f, value = baseMaterial };
                        keyFramesOff[1] = new ObjectReferenceKeyframe() { time = 1f / clipOFF.frameRate, value = baseMaterial };
                        AnimationUtility.SetObjectReferenceCurve(clipOFF, curveBindingOff, keyFramesOff);

                        if (EditorSettings.MaterialAnimationType != RIS.MaterialAnimationType.MaterialOnly)
                        {
                            foreach (var property in baseMaterialProperties)
                            {
                                var baseMaterialPropertyName = property.name;
                                if (property.type == MaterialProperty.PropType.Color)
                                {
                                    curveBindingOff = EditorCurveBinding.FloatCurve(path, renderType, $"material.{baseMaterialPropertyName}.r");
                                    var curve = new AnimationCurve();
                                    curve.AddKey(0f, property.colorValue.r);
                                    curve.AddKey(1f / clipOFF.frameRate, property.colorValue.r);
                                    AnimationUtility.SetEditorCurve(clipOFF, curveBindingOff, curve);

                                    curveBindingOff = EditorCurveBinding.FloatCurve(path, renderType, $"material.{baseMaterialPropertyName}.g");
                                    curve = new AnimationCurve();
                                    curve.AddKey(0f, property.colorValue.g);
                                    curve.AddKey(1f / clipOFF.frameRate, property.colorValue.g);
                                    AnimationUtility.SetEditorCurve(clipOFF, curveBindingOff, curve);

                                    curveBindingOff = EditorCurveBinding.FloatCurve(path, renderType, $"material.{baseMaterialPropertyName}.b");
                                    curve = new AnimationCurve();
                                    curve.AddKey(0f, property.colorValue.b);
                                    curve.AddKey(1f / clipOFF.frameRate, property.colorValue.b);
                                    AnimationUtility.SetEditorCurve(clipOFF, curveBindingOff, curve);

                                    curveBindingOff = EditorCurveBinding.FloatCurve(path, renderType, $"material.{baseMaterialPropertyName}.a");
                                    curve = new AnimationCurve();
                                    curve.AddKey(0f, property.colorValue.a);
                                    curve.AddKey(1f / clipOFF.frameRate, property.colorValue.a);
                                    AnimationUtility.SetEditorCurve(clipOFF, curveBindingOff, curve);
                                }
                                if (property.type == MaterialProperty.PropType.Float)
                                {
                                    curveBindingOff = EditorCurveBinding.FloatCurve(path, renderType, $"material.{baseMaterialPropertyName}");
                                    var curve = new AnimationCurve();
                                    curve.AddKey(0f, property.floatValue);
                                    curve.AddKey(1f / clipOFF.frameRate, property.floatValue);
                                    AnimationUtility.SetEditorCurve(clipOFF, curveBindingOff, curve);
                                }
                            }
                        }
                        var clipOFFName = $"MAT-G{v}DEFAULT";
                        AssetDatabase.CreateAsset(clipOFF, animationsFolder + clipOFFName + ".anim");
                        offState.motion = clipOFF;
                        stateMachine.defaultState = offState;
                        var offTransition = stateMachine.MakeAnyStateTransition(offState);
                        foreach (var material in materials)
                        {
                            var prop = risAvatar.Groups[material.index.GroupIndex].Props[material.index.PropIndex];
                            if (prop.IsLocalOnly)
                                CheckParam(avatar, fxLayer, "IsLocal", false);
                            offTransition.AddCondition(AnimatorConditionMode.IfNot, $"{RIS.Prefix}-G{material.index.GroupIndex}P{material.index.PropIndex}", 0f, prop.IsLocalOnly && !prop.IsDefaultEnabled, true);
                        }
                        EditorUtility.SetDirty(clipOFF);
                        layer.stateMachine = stateMachine;
                        EditorUtility.SetDirty(layer.stateMachine);
                    }
                }
            }
            //Animation Toggles
            foreach (var pair in propLayer_AnimationToggles)
            {
                var groupIndex = pair.Key.GroupIndex;
                var propIndex = pair.Key.PropIndex;
                var prop = pair.Value;
                var paramName = $"{RIS.Prefix}-G{groupIndex}P{propIndex}";

                CheckParam(avatar, fxLayer, paramName, prop.IsDefaultEnabled);

                var layerName = $"{RIS.Prefix}-ANIM-G{groupIndex}P{propIndex}";
                var animLayer = fxLayer.FindAnimatorControllerLayer(layerName);
                if (animLayer == null)
                    animLayer = fxLayer.AddAnimatorControllerLayer(layerName);
                var stateMachine = animLayer.stateMachine;
                stateMachine.Clear();

                var onState = stateMachine.AddState("PropON", new Vector3(300, 100, 0));
                onState.writeDefaultValues = risAvatar.UseWriteDefaults;
                var offState = stateMachine.AddState("PropOFF", new Vector3(300, 200, 0));
                offState.writeDefaultValues = risAvatar.UseWriteDefaults;

                if (prop.IsLocalOnly)
                    CheckParam(avatar, fxLayer, "IsLocal", false);

                var transition = stateMachine.MakeAnyStateTransition(onState);
                transition.CreateSingleCondition(AnimatorConditionMode.If, paramName, 1f, prop.IsLocalOnly && !prop.IsDefaultEnabled, true);
                transition = stateMachine.MakeAnyStateTransition(offState);
                transition.CreateSingleCondition(AnimatorConditionMode.IfNot, paramName, 1f, prop.IsLocalOnly && prop.IsDefaultEnabled, true);

                stateMachine.defaultState = prop.IsDefaultEnabled ? onState : offState;

                onState.motion = prop.EnableAnimation.GetObject();
                offState.motion = prop.DisableAnimation.GetObject();
                if (onState.motion == null)
                    onState.motion = noneClip;
                if (offState.motion == null)
                    offState.motion = noneClip;
                EditorUtility.SetDirty(animLayer.stateMachine);
            }
            //Animations Toggle(LegacyExclusive)
            exclusiveIndexPairs = new Dictionary<RIS.ExclusiveGroupType, List<IndexPair>>();
            foreach (RIS.ExclusiveGroupType enumValue in exclusiveGroupTypes)
                exclusiveIndexPairs.Add(enumValue, new List<IndexPair>());
            foreach (var pair in propLayer_AnimationTogglesLE)
                exclusiveIndexPairs[pair.Value.ExclusiveGroup].Add(pair.Key);


            foreach (RIS.ExclusiveGroupType enumValue in exclusiveGroupTypes)
            {
                if (enumValue == RIS.ExclusiveGroupType.None)
                    continue;
                var indexPairs = exclusiveIndexPairs[enumValue];
                if (indexPairs.Count <= 0)
                    continue;

                var layerName = $"{RIS.Prefix}-EXANIM-{(int)enumValue}";
                var animLayer = fxLayer.FindAnimatorControllerLayer(layerName);
                if (animLayer == null)
                    animLayer = fxLayer.AddAnimatorControllerLayer(layerName);
                var stateMachine = animLayer.stateMachine;
                stateMachine.Clear();

                var stateName = "DEFAULT";
                var defaultState = stateMachine.AddState(stateName, new Vector3(300, 150, 0));
                defaultState.writeDefaultValues = risAvatar.UseWriteDefaults;
                var defaultTransition = stateMachine.MakeAnyStateTransition(defaultState);
                stateMachine.defaultState = defaultState;

                foreach (var pair in indexPairs)
                {
                    var groupIndex = pair.GroupIndex;
                    var propIndex = pair.PropIndex;
                    var group = risAvatar.Groups[groupIndex];
                    var prop = group.Props[propIndex];
                    var paramName = $"{RIS.Prefix}-G{groupIndex}P{propIndex}";

                    CheckParam(avatar, fxLayer, paramName, prop.IsDefaultEnabled);
                    defaultTransition.AddCondition(AnimatorConditionMode.IfNot, paramName, 1f, false, true);
                }

                defaultState.motion = risAvatar.GetExclusiveDisableClip(enumValue);
                if (defaultState.motion == null)
                    defaultState.motion = noneClip;

                foreach (var pairIndex in Enumerable.Range(0, indexPairs.Count))
                {
                    var mainPair = indexPairs[pairIndex];
                    stateName = $"G{mainPair.GroupIndex}P{mainPair.PropIndex}ON";
                    var state = stateMachine.AddState(stateName, new Vector3(300, 200 + (pairIndex * 50), 0));
                    state.writeDefaultValues = risAvatar.UseWriteDefaults;
                    AnimatorStateTransition transition = null;
                    var paramName = $"{RIS.Prefix}-G{mainPair.GroupIndex}P{mainPair.PropIndex}";
                    transition = stateMachine.MakeAnyStateTransition(state);
                    var prop = risAvatar.Groups[mainPair.GroupIndex].Props[mainPair.PropIndex];
                    CheckParam(avatar, fxLayer, paramName, prop.IsDefaultEnabled);
                    if (prop.IsLocalOnly)
                        CheckParam(avatar, fxLayer, "IsLocal", false);
                    transition.CreateSingleCondition(AnimatorConditionMode.If, paramName, 1f, prop.IsLocalOnly && !prop.IsDefaultEnabled, true);

                    state.motion = prop.EnableAnimation?.GetObject();
                    if (state.motion == null)
                        state.motion = noneClip;
                }
                EditorUtility.SetDirty(animLayer.stateMachine);
            }

            AssetDatabase.CreateAsset(fallbackClip, animationsFolder + $"FallbackClip" + ".anim");
            AddFallbackDriver(ref fxLayer, ref risAvatar, fallbackClip);

            AssetDatabase.SaveAssets();

        }
       
    }
}
#endif