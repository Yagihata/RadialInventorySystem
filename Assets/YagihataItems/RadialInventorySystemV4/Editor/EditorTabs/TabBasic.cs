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
    public class TabBasic : EditorTab
    {
        /*private ReorderableList propGroupsReorderableList;
        private ReorderableList propsReorderableList;
        private Texture2D redTexture = null;
        private Texture2D blueTexture = null;
        public override void DrawTab(ref RISVariables variables, ref RISSettings settings, Rect position, bool showingVerticalScroll)
        {
            if (propGroupsReorderableList == null)
                InitializeGroupList(variables, settings);
            if (propsReorderableList == null)
                InitializePropList(null, variables, settings);
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
                var propIsChanged = false;
                using (new EditorGUI.DisabledGroupScope(!groupIsSelected))
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(cellWidth + 20)))
                    {
                        EditorGUIUtility.labelWidth = 80;
                        var prefixText = "グループ";
                        EditorGUILayout.LabelField(prefixText + "設定", new GUIStyle("ProjectBrowserHeaderBgTop"), GUILayout.ExpandWidth(true));
                        GUILayout.Space(3);
                        if (groupIsSelected)
                        {
                            variables.Groups[groupIndex].GroupName = EditorGUILayout.TextField(prefixText + "名", variables.Groups[groupIndex].GroupName);
                            variables.Groups[groupIndex].GroupIcon =
                                (Texture2D)EditorGUILayout.ObjectField("アイコン", variables.Groups[groupIndex].GroupIcon, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            variables.Groups[groupIndex].ExclusiveMode = EditorGUILayout.Popup("排他モード", variables.Groups[groupIndex].ExclusiveMode, MessageStrings.ExclusiveType);
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
                            EditorGUILayout.TextField("グループ名", "");
                            EditorGUILayout.ObjectField("アイコン", null, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            EditorGUILayout.Popup("排他モード", 0, MessageStrings.ExclusiveType);
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
                        EditorGUILayout.LabelField("プロップ設定", new GUIStyle("ProjectBrowserHeaderBgTop"), GUILayout.ExpandWidth(true));
                        GUILayout.Space(3);
                        EditorGUILayout.LabelField("プロップ名");
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(20);
                            if (targetProp != null)
                                targetProp.PropName = EditorGUILayout.TextField(targetProp.PropName);
                            else
                                EditorGUILayout.TextField("");


                        }
                        EditorGUILayout.LabelField("アイコン");
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(20);
                            if (targetProp != null)
                                targetProp.PropIcon = (Texture2D)EditorGUILayout.ObjectField(targetProp.PropIcon, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            else
                                EditorGUILayout.ObjectField(null, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));


                        }
                        EditorGUILayout.LabelField("オブジェクト");
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
                            EditorGUILayout.LabelField(MessageStrings.Strings.str_ShowDefault);
                            GUILayout.FlexibleSpace();
                            if (targetProp != null)
                                targetProp.IsDefaultEnabled = EditorGUILayout.Toggle(targetProp.IsDefaultEnabled, GUILayout.Width(20));
                            else
                                EditorGUILayout.Toggle(false, GUILayout.Width(20));
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(MessageStrings.Strings.str_LocalOnly);
                            GUILayout.FlexibleSpace();
                            if (targetProp != null)
                                targetProp.LocalOnly = EditorGUILayout.Toggle(targetProp.LocalOnly, GUILayout.Width(20));
                            else
                                EditorGUILayout.Toggle(false, GUILayout.Width(20));
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(MessageStrings.Strings.str_SaveParam);
                            GUILayout.FlexibleSpace();
                            if (targetProp != null)
                                targetProp.SaveParameter = EditorGUILayout.Toggle(targetProp.SaveParameter, GUILayout.Width(20));
                            else
                                EditorGUILayout.Toggle(true, GUILayout.Width(20));
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
                    EditorGUI.LabelField(rect, "グループ一覧" + $": {groups.Count}");
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
                    EditorGUI.LabelField(rect, "プロップ一覧" + $": {props.Count}");
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
                        if (group.ExclusiveMode == 1)
                            maxPropCount = 7;
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
                            if (group.ExclusiveMode == 1)
                                maxPropCount = 7;
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
        */
        private ReorderableList groupsReorderableList;
        private ReorderableList propsReorderableList;
        private Texture2D redTexture = null;
        private Texture2D blueTexture = null;
        private Group selectedGroup;
        private Prop selectedProp;

        public override string[] CheckErrors(ref Avatar risAvatar)
        {
            if (risAvatar == null)
                return new string[] { };

            var errors = new List<string>();
            var prefixText = "グループ";
            if (risAvatar.Groups.Count == 0)
                errors.Add(prefixText + "を追加してください。");

            foreach (var groupIndex in Enumerable.Range(0, risAvatar.Groups.Count))
            {
                var group = risAvatar.Groups[groupIndex];
                var groupName = group.Name;
                if (string.IsNullOrEmpty(groupName))
                    groupName = "Group" + risAvatar.Groups.IndexOf(group);

                if (!group.Props.Any())
                    errors.Add($"{prefixText}[{groupName}]にプロップが登録されていません。");

                var maxPropsCount = 8;
                if (group.UseResetButton)
                    maxPropsCount = 7;
                if (group.Props.Count > maxPropsCount)
                    errors.Add($"{prefixText}[{groupName}]" + string.Format("のプロップ数がオーバーしています。(最大{0})", maxPropsCount));

                foreach (var prop in group.Props)
                {
                    var propName = prop.Name;
                    if (string.IsNullOrEmpty(propName))
                        propName = "Prop" + group.Props.IndexOf(prop);
                    var parentGameObject = risAvatar.AvatarRoot?.GetObject()?.gameObject;
                    if (prop.TargetObjects.Count <= 0 || prop.TargetObjects[0]?.GetObject(parentGameObject) == null)
                        errors.Add($"{prefixText}[{groupName}]のプロップ" + $"[{propName}]にオブジェクトが登録されていません。");
                }
            }


            return errors.ToArray();
        }
        public override void BuildFXLayer(ref Avatar risAvatar, string autoGeneratedFolder)
        {
        }

        public override void DrawTab(ref Avatar risAvatar, Rect position, bool showingVerticalScroll)
        {
            if (groupsReorderableList == null)
                InitializeGroupList(risAvatar);
            if (propsReorderableList == null)
                InitializePropList(null, risAvatar);
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
                var advanceMode = (risAvatar != null && risAvatar.MenuMode == RIS.MenuModeType.Advanced);

                if (groupIsSelected && selectedGroupIsChanged)
                {
                    selectedGroup = risAvatar.Groups[groupIndex];
                    selectedProp = new Prop();
                }
                else if (selectedGroupIsChanged || selectedGroup == null)
                {
                    selectedGroup = new Group();
                    selectedProp = new Prop();
                }
                RIS.ExclusiveModeType exclusiveMode = RIS.ExclusiveModeType.None;
                using (new EditorGUI.DisabledGroupScope(!groupIsSelected))
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(cellWidth + 20)))
                    {
                        EditorGUIUtility.labelWidth = 100;
                        EditorGUILayout.LabelField("グループ設定", new GUIStyle("ProjectBrowserHeaderBgTop"), GUILayout.ExpandWidth(true));
                        GUILayout.Space(3);
                        selectedGroup.Name = EditorGUILayout.TextField("グループ名", selectedGroup.Name);

                        EditorGUI.BeginChangeCheck();

                        var icon = selectedGroup.Icon.GetObject();
                        icon = (Texture2D)EditorGUILayout.ObjectField("アイコン", icon, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));

                        exclusiveMode = risAvatar.GetExclusiveMode((RIS.ExclusiveGroupType)groupIndex);
                        exclusiveMode = (RIS.ExclusiveModeType)EditorGUILayout.EnumPopup("排他モード", exclusiveMode);

                        if (EditorGUI.EndChangeCheck())
                        {
                            selectedGroup.Icon.SetObject(icon);
                            risAvatar.SetExclusiveMode((RIS.ExclusiveGroupType)groupIndex, exclusiveMode);
                        }

                        selectedGroup.UseResetButton = EditorGUILayout.Toggle("リセットボタン", selectedGroup.UseResetButton);
                        if (selectedGroupIsChanged)
                            InitializePropList(selectedGroup, risAvatar);
                        using (var scope = new EditorGUILayout.HorizontalScope())
                        {
                            var scopeWidth = cellWidth + 20;
                            var propListHeight = propsReorderableList.GetHeight();
                            using (new EditorGUILayout.HorizontalScope(GUILayout.Height(propListHeight), GUILayout.Width(scopeWidth)))
                            {
                                var propIndex = propsReorderableList.index;
                                propsReorderableList.DoList(new Rect(scope.rect.x, scope.rect.y, scopeWidth, propListHeight));
                                selectedGroup.Props = (List<Prop>)propsReorderableList.list;
                                selectedPropIsChanged = propIndex != propsReorderableList.index;
                                GUILayout.Space(0);
                            }
                        }
                    }
                }
                GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(1));
                var propIsSelected = groupIsSelected && groupIndex != -1 && propsReorderableList.index != -1 && groupIndex < risAvatar.Groups.Count &&
                        propsReorderableList.index < risAvatar.Groups[groupIndex].Props.Count;
                if (propIsSelected && selectedPropIsChanged)
                {
                    selectedProp = risAvatar.Groups[groupIndex].Props[propsReorderableList.index];
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
                        EditorGUILayout.LabelField("プロップ設定", new GUIStyle("ProjectBrowserHeaderBgTop"), GUILayout.ExpandWidth(true));
                        GUILayout.Space(3);
                        EditorGUIUtility.labelWidth = 100;
                        selectedProp.Name = EditorGUILayout.TextField("プロップ名", selectedProp.Name);

                        EditorGUI.BeginChangeCheck();

                        var icon = selectedProp.Icon.GetObject();
                        icon = (Texture2D)EditorGUILayout.ObjectField("アイコン", icon, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                        if (EditorGUI.EndChangeCheck())
                            selectedProp.Icon.SetObject(icon);

                        if (exclusiveMode == RIS.ExclusiveModeType.None)
                            selectedProp.ExclusiveGroup = RIS.ExclusiveGroupType.None;
                        else
                            selectedProp.ExclusiveGroup = (RIS.ExclusiveGroupType)groupIndex;

                        selectedProp.IsDefaultEnabled = EditorGUILayout.Toggle("初期状態", selectedProp.IsDefaultEnabled);
                        selectedProp.IsLocalOnly = EditorGUILayout.Toggle("ローカル動作", selectedProp.IsLocalOnly);
                        selectedProp.UseSaveParameter = EditorGUILayout.Toggle("有効状態を保存", selectedProp.UseSaveParameter);

                        if(selectedProp.TargetObjects.Count <= 0)
                            selectedProp.TargetObjects.Add(null);

                        var parent = risAvatar.AvatarRoot?.GetObject()?.gameObject;
                        var targetObject = selectedProp.TargetObjects[0]?.GetObject(parent);
                        EditorGUI.BeginChangeCheck();
                        targetObject = (GameObject)EditorGUILayout.ObjectField(targetObject, typeof(GameObject), true);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (selectedProp.TargetObjects[0] == null)
                                selectedProp.TargetObjects[0] = new GUIDPathPair<GameObject>(ObjectPathStateType.RelativeFromObject, targetObject, parent);
                            else
                                selectedProp.TargetObjects[0].SetObject(targetObject, parent);
                            if (selectedProp.TargetObjects[0].GetObject(parent) == null && targetObject != null)
                            {
                                EditorUtility.DisplayDialog("Radial Inventory System", $"対象アバターとして指定しているオブジェクトの子オブジェクトのみ登録できます。", "OK");
                            }
                        }

                    }
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUIUtility.labelWidth = 0;
        }

        public override void InitializeTab(ref Avatar risAvatar)
        {
            InitializeGroupList(risAvatar);
            InitializePropList(null, risAvatar);
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
                    EditorGUI.LabelField(rect, "グループ一覧" + $": {groups.Count}");
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
                        newGroup.Name = "Group" + groups.Count;
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
                        name = "Group" + index;
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
                    EditorGUI.LabelField(rect, "プロップ一覧" + $": {props.Count}");
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
                    var propName = !string.IsNullOrEmpty(rawPropName) ? rawPropName : $"Prop{index}";
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
                            if (group.UseResetButton)
                                maxPropCount = 7;
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
    }
}