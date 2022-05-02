using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using YagihataItems.YagiUtils;

namespace YagihataItems.RadialInventorySystemV4
{
    public class TabAdvanced : EditorTab
    {
        private ReorderableList propGroupsReorderableList;
        private ReorderableList propsReorderableList;
        private ReorderableList gameObjectsReorderableList = null;
        private ReorderableList gameObjectsDummyReorderableList = null;
        private Texture2D redTexture = null;
        private Texture2D blueTexture = null;
        public override void DrawTab(ref RISVariables variables, ref RISSettings settings, Rect position, bool showingVerticalScroll)
        {
            if (propGroupsReorderableList == null)
                InitializeGroupList(variables, settings);
            if (propsReorderableList == null)
                InitializePropList(null, variables, settings);
            if (gameObjectsDummyReorderableList == null)
                InitializeGameObjectsList(true, variables, settings);
            var cellWidth = position.width / 3f - 15f;
            using (new EditorGUILayout.HorizontalScope())
            {
                var selectedGroupChangeFlag = true;
                using (new EditorGUILayout.VerticalScope())
                {
                    using (var scope = new EditorGUILayout.HorizontalScope())
                    {
                        var scopeWidth = cellWidth - 40;
                        var propGroupsHeight = propGroupsReorderableList.GetHeight();
                        using (new EditorGUILayout.HorizontalScope(GUILayout.Height(propGroupsHeight), GUILayout.Width(scopeWidth)))
                        {
                            var oldSelectedIndex = propGroupsReorderableList.index;
                            propGroupsReorderableList.DoList(new Rect(scope.rect.x, scope.rect.y, scopeWidth, propGroupsHeight));
                            if (variables != null)
                                variables.Groups = (List<PropGroup>)propGroupsReorderableList.list;
                            selectedGroupChangeFlag = oldSelectedIndex != propGroupsReorderableList.index;
                            GUILayout.Space(0);
                        }

                    }
                }
                GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(1));
                var groupIndex = propGroupsReorderableList.index;
                var groupIsSelected = variables != null && variables.Groups.Count >= 1 && groupIndex >= 0;
                Prop targetProp = null;
                var advanceMode = (variables != null && variables.MenuMode == RIS.RISMode.Advanced);
                var propIsChanged = false;
                using (new EditorGUI.DisabledGroupScope(!groupIsSelected))
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(cellWidth + 20)))
                    {
                        EditorGUIUtility.labelWidth = 80;
                        var prefixText = advanceMode ? RISMessageStrings.Strings.str_Menu : RISMessageStrings.Strings.str_Group;
                        EditorGUILayout.LabelField(prefixText + RISMessageStrings.Strings.str_Settings, new GUIStyle("ProjectBrowserHeaderBgTop"), GUILayout.ExpandWidth(true));
                        GUILayout.Space(3);
                        if (groupIsSelected)
                        {
                            variables.Groups[groupIndex].GroupName = EditorGUILayout.TextField(prefixText + RISMessageStrings.Strings.str_Name, variables.Groups[groupIndex].GroupName);
                            variables.Groups[groupIndex].GroupIcon =
                                (Texture2D)EditorGUILayout.ObjectField(RISMessageStrings.Strings.str_Icon, variables.Groups[groupIndex].GroupIcon, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            if (variables.MenuMode == RIS.RISMode.Simple)
                            {
                                //variables.Groups[groupIndex].ExclusiveMode = EditorGUILayout.Toggle("排他モード", variables.Groups[groupIndex].ExclusiveMode);
                                variables.Groups[groupIndex].ExclusiveMode = EditorGUILayout.Popup(RISMessageStrings.Strings.str_Exclusive + RISMessageStrings.Strings.str_Mode, variables.Groups[groupIndex].ExclusiveMode, RISMessageStrings.ExclusiveType);
                            }
                            else
                            {
                                variables.Groups[groupIndex].BaseMenu =
                                    (VRCExpressionsMenu)EditorGUILayout.ObjectField(RISMessageStrings.Strings.str_Base + RISMessageStrings.Strings.str_Menu, variables.Groups[groupIndex].BaseMenu, typeof(VRCExpressionsMenu), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                                variables.Groups[groupIndex].UseResetButton = EditorGUILayout.Toggle(RISMessageStrings.Strings.str_UseReset, variables.Groups[groupIndex].UseResetButton);
                            }
                            if (selectedGroupChangeFlag)
                                InitializePropList(variables.Groups[groupIndex], variables, settings);
                            using (var scope = new EditorGUILayout.HorizontalScope())
                            {
                                var scopeWidth = cellWidth + 20;
                                var propListHeight = propsReorderableList.GetHeight();
                                using (new EditorGUILayout.HorizontalScope(GUILayout.Height(propListHeight), GUILayout.Width(scopeWidth)))
                                {
                                    var propIndex = propsReorderableList.index;
                                    propsReorderableList.DoList(new Rect(scope.rect.x, scope.rect.y, scopeWidth, propListHeight));
                                    variables.Groups[groupIndex].Props = (List<Prop>)propsReorderableList.list;
                                    propIsChanged = propIndex != propsReorderableList.index;
                                    GUILayout.Space(0);
                                }
                            }
                            if (variables.Groups[groupIndex].Props.Count > propsReorderableList.index && propsReorderableList.index >= 0)
                                targetProp = variables.Groups[groupIndex].Props[propsReorderableList.index];
                            else
                                targetProp = null;
                        }
                        else
                        {
                            EditorGUILayout.TextField(RISMessageStrings.Strings.str_Group + RISMessageStrings.Strings.str_Name, "");
                            EditorGUILayout.ObjectField(RISMessageStrings.Strings.str_Icon, null, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            if ((variables != null && variables.MenuMode == RIS.RISMode.Simple) || variables == null)
                                EditorGUILayout.Popup(RISMessageStrings.Strings.str_Exclusive + RISMessageStrings.Strings.str_Mode, 0, RISMessageStrings.ExclusiveType);
                            else
                                EditorGUILayout.ObjectField(RISMessageStrings.Strings.str_Base + RISMessageStrings.Strings.str_Menu, null, typeof(VRCExpressionsMenu), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            if (selectedGroupChangeFlag)
                                InitializePropList(null, variables, settings);
                            using (var scope = new EditorGUILayout.HorizontalScope())
                            {
                                var scopeWidth = cellWidth + 20;
                                var propListHeight = propsReorderableList.GetHeight();
                                using (new EditorGUILayout.HorizontalScope(GUILayout.Height(propListHeight), GUILayout.Width(scopeWidth)))
                                {
                                    propsReorderableList.DoList(new Rect(scope.rect.x, scope.rect.y, scopeWidth, propListHeight));
                                    GUILayout.Space(0);
                                }
                            }
                        }
                    }
                }
                GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(1));
                using (new EditorGUI.DisabledGroupScope(targetProp == null))
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(cellWidth + 30 - (showingVerticalScroll ? 14 : 0))))
                    {
                        GUIStyle headerStyle = new GUIStyle("HeaderLabel");
                        headerStyle.margin = new RectOffset(5, 5, 20, 20);
                        EditorGUILayout.LabelField(RISMessageStrings.Strings.str_Prop + RISMessageStrings.Strings.str_Settings, new GUIStyle("ProjectBrowserHeaderBgTop"), GUILayout.ExpandWidth(true));
                        GUILayout.Space(3);
                        if (!advanceMode)
                        {
                            EditorGUILayout.LabelField(RISMessageStrings.Strings.str_Prop + RISMessageStrings.Strings.str_Name);
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.Space(20);
                                if (targetProp != null)
                                    targetProp.PropName = EditorGUILayout.TextField(targetProp.PropName);
                                else
                                    EditorGUILayout.TextField("");


                            }
                            EditorGUILayout.LabelField(RISMessageStrings.Strings.str_Icon);
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.Space(20);
                                if (targetProp != null)
                                    targetProp.PropIcon = (Texture2D)EditorGUILayout.ObjectField(targetProp.PropIcon, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                                else
                                    EditorGUILayout.ObjectField(null, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));


                            }
                            EditorGUILayout.LabelField(RISMessageStrings.Strings.str_Object);
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.Space(20);
                                if (targetProp != null)
                                {
                                    var targetObject = targetProp.TargetObject;
                                    if (targetObject != null && !targetObject.IsChildOf(variables.AvatarRoot.gameObject))
                                        targetObject = null;
                                    targetProp.TargetObject = (GameObject)EditorGUILayout.ObjectField(targetObject, typeof(GameObject), true);
                                }
                                else
                                    EditorGUILayout.ObjectField(null, typeof(GameObject), true);
                            }
                            GUILayout.Space(5);
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField(RISMessageStrings.Strings.str_ShowDefault);
                                GUILayout.FlexibleSpace();
                                if (targetProp != null)
                                    targetProp.IsDefaultEnabled = EditorGUILayout.Toggle(targetProp.IsDefaultEnabled, GUILayout.Width(20));
                                else
                                    EditorGUILayout.Toggle(false, GUILayout.Width(20));
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField(RISMessageStrings.Strings.str_LocalOnly);
                                GUILayout.FlexibleSpace();
                                if (targetProp != null)
                                    targetProp.LocalOnly = EditorGUILayout.Toggle(targetProp.LocalOnly, GUILayout.Width(20));
                                else
                                    EditorGUILayout.Toggle(false, GUILayout.Width(20));
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField(RISMessageStrings.Strings.str_SaveParam);
                                GUILayout.FlexibleSpace();
                                if (targetProp != null)
                                    targetProp.SaveParameter = EditorGUILayout.Toggle(targetProp.SaveParameter, GUILayout.Width(20));
                                else
                                    EditorGUILayout.Toggle(true, GUILayout.Width(20));
                            }
                        }
                        else
                        {
                            // AdvancedMode
                            EditorGUIUtility.labelWidth = 80;
                            if (targetProp != null)
                            {
                                targetProp.PropName = EditorGUILayout.TextField(RISMessageStrings.Strings.str_Prop + RISMessageStrings.Strings.str_Name, targetProp.PropName);
                                targetProp.PropIcon =
                                    (Texture2D)EditorGUILayout.ObjectField(RISMessageStrings.Strings.str_Icon, targetProp.PropIcon, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                                targetProp.PropGroupType = (RIS.PropGroup)EditorGUILayout.EnumPopup(RISMessageStrings.Strings.str_Exclusive + RISMessageStrings.Strings.str_Group, targetProp.PropGroupType);
                                if (targetProp.PropGroupType != RIS.PropGroup.None)
                                {
                                    var modeCount = Enum.GetNames(typeof(RIS.PropGroup)).Length;
                                    if (variables.AdvancedGroupMode.Length != modeCount)
                                        Array.Resize(ref variables.AdvancedGroupMode, modeCount);
                                    var v2Mode = variables.AdvancedGroupMode[(int)targetProp.PropGroupType] == 1;
                                    v2Mode = EditorGUILayout.Toggle("┗V2" + RISMessageStrings.Strings.str_Mode, v2Mode);
                                    variables.AdvancedGroupMode[(int)targetProp.PropGroupType] = v2Mode ? 1 : 0;
                                }
                                targetProp.IsDefaultEnabled = EditorGUILayout.Toggle(RISMessageStrings.Strings.str_DefaultStatus, targetProp.IsDefaultEnabled);
                                targetProp.LocalOnly = EditorGUILayout.Toggle(RISMessageStrings.Strings.str_LocalFunc, targetProp.LocalOnly);
                                GUILayout.Space(5);
                                EditorGUILayout.LabelField(RISMessageStrings.Strings.str_AdditionalAnimation);
                                targetProp.EnableAnimation = (AnimationClip)EditorGUILayout.ObjectField("┣" + RISMessageStrings.Strings.str_OnEnable, targetProp.EnableAnimation, typeof(AnimationClip), false);
                                targetProp.DisableAnimation = (AnimationClip)EditorGUILayout.ObjectField("┗" + RISMessageStrings.Strings.str_OnDisable, targetProp.DisableAnimation, typeof(AnimationClip), false);
                                GUILayout.Space(5);
                                targetProp.UseResetTimer = EditorGUILayout.Toggle(RISMessageStrings.Strings.str_OffTimer, targetProp.UseResetTimer);
                                if (targetProp.UseResetTimer)
                                    targetProp.ResetSecond = Mathf.Min(60, Mathf.Max(0, EditorGUILayout.FloatField("┗" + RISMessageStrings.Strings.str_Sec, targetProp.ResetSecond)));
                                GUILayout.Space(5);
                                targetProp.MaterialOverride =
                                    (Material)EditorGUILayout.ObjectField(RISMessageStrings.Strings.str_Material, targetProp.MaterialOverride, typeof(Material), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                                GUILayout.Space(5);
                                if (gameObjectsReorderableList == null || propIsChanged)
                                    InitializeGameObjectsList(false, variables, settings, targetProp);
                                gameObjectsReorderableList.DoLayoutList();
                                targetProp.TargetObjects = (List<GameObject>)gameObjectsReorderableList.list;
                            }
                            else
                            {
                                EditorGUILayout.TextField(RISMessageStrings.Strings.str_Prop + RISMessageStrings.Strings.str_Name, "");
                                EditorGUILayout.ObjectField(RISMessageStrings.Strings.str_Icon, null, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                                EditorGUILayout.EnumPopup(RISMessageStrings.Strings.str_Exclusive + RISMessageStrings.Strings.str_Group, RIS.PropGroup.None);
                                EditorGUILayout.Toggle(RISMessageStrings.Strings.str_DefaultStatus, false);
                                EditorGUILayout.Toggle(RISMessageStrings.Strings.str_LocalFunc, false);
                                GUILayout.Space(5);
                                EditorGUILayout.LabelField(RISMessageStrings.Strings.str_AdditionalAnimation);
                                EditorGUILayout.ObjectField("┣" + RISMessageStrings.Strings.str_OnEnable, null, typeof(AnimationClip), false);
                                EditorGUILayout.ObjectField("┗" + RISMessageStrings.Strings.str_OnDisable, null, typeof(AnimationClip), false);
                                GUILayout.Space(5);
                                EditorGUILayout.Toggle(RISMessageStrings.Strings.str_OffTimer, false);
                                GUILayout.Space(5);
                                EditorGUILayout.ObjectField(RISMessageStrings.Strings.str_Material, null, typeof(Material), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                                GUILayout.Space(5);
                                gameObjectsReorderableList = null;
                                gameObjectsDummyReorderableList.DoLayoutList();

                            }
                        }
                    }
                }
                GUILayout.FlexibleSpace();

            }
            EditorGUIUtility.labelWidth = 0;
        }
        private void InitializeGroupList(RISVariables variables, RISSettings settings)
        {
            List<PropGroup> groups = null;
            if (variables != null && variables.Groups != null)
                groups = variables.Groups;
            else
                groups = new List<PropGroup>();
            propGroupsReorderableList = new ReorderableList(groups, typeof(PropGroup))
            {
                drawHeaderCallback = rect =>
                {
                    if (variables != null && variables.MenuMode == RIS.RISMode.Advanced)
                        EditorGUI.LabelField(rect, RISMessageStrings.Strings.str_Menu + RISMessageStrings.Strings.str_List + $": {groups.Count}");
                    else
                        EditorGUI.LabelField(rect, RISMessageStrings.Strings.str_Group + RISMessageStrings.Strings.str_List + $": {groups.Count}");
                    var position =
                        new Rect(
                            rect.x + rect.width - 20f,
                            rect.y,
                            20f,
                            13f
                        );
                    if (groups.Count < 8 && GUI.Button(position, ReorderableListStyle.AddContent, ReorderableListStyle.AddStyle))
                    {
                        if (settings != null)
                            Undo.RecordObject(settings, $"Add new PropGroup.");
                        var newPropGroup = ScriptableObject.CreateInstance<PropGroup>();
                        newPropGroup.GroupName = "Group" + groups.Count;
                        variables.Groups.Add(newPropGroup);
                        if (settings != null)
                            EditorUtility.SetDirty(settings);
                    }
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (groups.Count <= index)
                        return;

                    var style = GUI.skin.label;
                    style.fontSize = (int)(rect.height / 1.75f);
                    var name = groups[index].GroupName;
                    if (string.IsNullOrEmpty(name))
                        name = "Group" + index;
                    GUI.Label(rect, name, style);
                    style.fontSize = 0;
                    rect.x = rect.x + rect.width - 20f;
                    rect.width = 20f;
                    if (GUI.Button(rect, ReorderableListStyle.SubContent, ReorderableListStyle.SubStyle))
                    {
                        if (settings != null)
                            Undo.RecordObject(settings, $"Remove PropGroup - \"{groups[index].GroupName}\".");
                        groups.RemoveAt(index);
                        if (index >= groups.Count)
                            index = propGroupsReorderableList.index = -1;
                        if (settings != null)
                            EditorUtility.SetDirty(settings);
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
        private void InitializePropList(PropGroup group, RISVariables variables, RISSettings settings)
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
                    EditorGUI.LabelField(rect, RISMessageStrings.Strings.str_Prop + RISMessageStrings.Strings.str_List + $": {props.Count}");
                    var position =
                        new Rect(
                            rect.x + rect.width - 20f,
                            rect.y,
                            20f,
                            13f
                        );
                    var maxPropCount = 8;
                    if (variables != null && group != null)
                    {
                        if ((variables.MenuMode == RIS.RISMode.Simple && group.ExclusiveMode == 1) ||
                        (variables.MenuMode == RIS.RISMode.Advanced && group.UseResetButton))
                            maxPropCount = 7;
                        if (variables.MenuMode == RIS.RISMode.Advanced && group.BaseMenu != null)
                            maxPropCount -= group.BaseMenu.controls.Count;
                    }
                    if (props.Count < maxPropCount && GUI.Button(position, ReorderableListStyle.AddContent, ReorderableListStyle.AddStyle))
                    {
                        if (settings != null)
                            Undo.RecordObject(settings, $"Add new Prop.");
                        var newProp = ScriptableObject.CreateInstance<Prop>();
                        newProp.TargetObjects.Add(null);
                        group.Props.Add(newProp);
                        if (settings != null)
                            EditorUtility.SetDirty(settings);
                    }
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (props.Count <= index)
                        return;
                    var rawPropName = props[index].GetPropName(variables.MenuMode);
                    var propName = !string.IsNullOrEmpty(rawPropName) ? rawPropName : $"Prop{index}";
                    GUI.Label(rect, propName);
                    rect.x = rect.x + rect.width - 20f;
                    rect.width = 20f;
                    if (GUI.Button(rect, ReorderableListStyle.SubContent, ReorderableListStyle.SubStyle))
                    {
                        if (settings != null)
                            Undo.RecordObject(settings, $"Remove Prop - \"{propName}\".");
                        props.RemoveAt(index);
                        if (index >= props.Count)
                            index = propsReorderableList.index = -1;
                        if (settings != null)
                            EditorUtility.SetDirty(settings);
                    }
                },
                drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
                {
                    if (!isFocused)
                    {
                        var maxPropCount = 8;
                        if (variables != null && group != null)
                        {
                            if (variables.MenuMode == RIS.RISMode.Simple && group.ExclusiveMode == 1)
                                maxPropCount = 7;
                            else if (variables.MenuMode == RIS.RISMode.Advanced && group.BaseMenu != null)
                                maxPropCount = 8 - group.BaseMenu.controls.Count;
                        }
                        if (index >= maxPropCount)
                        {
                            if (redTexture == null)
                            {
                                redTexture = new Texture2D(1, 1);
                                redTexture.SetPixel(0, 0, new Color(1f, 0.5f, 0.5f, 0.5f));
                                redTexture.Apply();
                            }
                            GUI.DrawTexture(rect, redTexture);
                        }

                    }
                    else
                    {
                        if (blueTexture == null)
                        {
                            blueTexture = new Texture2D(1, 1);
                            blueTexture.SetPixel(0, 0, new Color(0.5f, 0.5f, 1f, 0.5f));
                            blueTexture.Apply();
                        }
                        GUI.DrawTexture(rect, blueTexture);
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
        private void InitializeGameObjectsList(bool dummyFlag, RISVariables variables, RISSettings settings, Prop prop = null)
        {
            List<GameObject> gameObjects = null;
            var propFlag = prop != null && prop.TargetObjects != null;
            if (!dummyFlag && propFlag)
                gameObjects = prop.TargetObjects;
            else
                gameObjects = new List<GameObject>();
            var list = new ReorderableList(gameObjects, typeof(GameObject));
            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, RISMessageStrings.Strings.str_Object + RISMessageStrings.Strings.str_List + $": {gameObjects.Count}");
                var position =
                    new Rect(
                        rect.x + rect.width - 20f,
                        rect.y,
                        20f,
                        13f
                    );
                if (GUI.Button(position, ReorderableListStyle.AddContent, ReorderableListStyle.AddStyle) && propFlag)
                {
                    if (settings != null)
                        Undo.RecordObject(settings, $"Add new TargetObject.");
                    prop.TargetObjects.Add(null);
                    if (settings != null)
                        EditorUtility.SetDirty(settings);
                }
            };
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (gameObjects.Count <= index)
                    return;
                rect.width -= 20;
                var targetObject = gameObjects[index];
                // Debug.Log(targetObject);
                // Debug.Log(variables.AvatarRoot);
                if (targetObject != null && !targetObject.IsChildOf(variables.AvatarRoot.gameObject))
                {
                    Debug.LogWarning(targetObject + " is not child of " + variables.AvatarRoot);
                    targetObject = null;
                }
                gameObjects[index] = (GameObject)EditorGUI.ObjectField(rect, targetObject, typeof(GameObject), true);
                rect.x = rect.x + rect.width;
                rect.width = 20f;
                if (GUI.Button(rect, ReorderableListStyle.SubContent, ReorderableListStyle.SubStyle))
                {
                    if (settings != null)
                        Undo.RecordObject(settings, $"Remove TargetObject - \"{index}\".");
                    gameObjects.RemoveAt(index);
                    if (index >= gameObjects.Count)
                        index = list.index = -1;
                    if (settings != null)
                        EditorUtility.SetDirty(settings);
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
            if (dummyFlag)
                gameObjectsDummyReorderableList = list;
            else
                gameObjectsReorderableList = list;
        }

        public override string[] CheckErrors(RISVariables variables)
        {
            if (variables == null)
                return new string[] { };

            var errors = new List<string>();
            var prefixText = RISMessageStrings.Strings.str_Group;
            if (variables.Groups.Count == 0)
                errors.Add(prefixText + RISMessageStrings.Strings.str_NeedOnce);

            foreach (var group in variables.Groups)
            {
                var groupName = group.GroupName;
                if (string.IsNullOrEmpty(groupName))
                    groupName = "Group" + variables.Groups.IndexOf(group);

                if (!group.Props.Any())
                    errors.Add($"{prefixText}[{groupName}]" + RISMessageStrings.Strings.str_MissingProp);

                var maxPropsCount = 8;
                if (group.BaseMenu != null)
                    maxPropsCount = 8 - group.BaseMenu.controls.Count;
                if (group.Props.Count > maxPropsCount)
                    errors.Add($"{prefixText}[{groupName}]" + string.Format(RISMessageStrings.Strings.str_OverProp, maxPropsCount));

                foreach (var prop in group.Props)
                {
                    var propName = prop.PropName;
                    if (string.IsNullOrEmpty(propName))
                        propName = "Prop" + group.Props.IndexOf(prop);

                    if (!prop.TargetObjects.Any(n => n != null) && prop.DisableAnimation == null && prop.EnableAnimation == null)
                        errors.Add($"{prefixText}[{groupName}]" + RISMessageStrings.Strings.str_GroupsProp + $"[{propName}]" + RISMessageStrings.Strings.str_MissingObjectOrAnim);
                }
            }


            return errors.ToArray();
        }
    }
}
