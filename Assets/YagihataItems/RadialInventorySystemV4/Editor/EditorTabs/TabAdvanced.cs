using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
        private ReorderableList GroupsReorderableList;
        private ReorderableList propsReorderableList;
        private ReorderableList gameObjectsReorderableList = null;
        private ReorderableList gameObjectsDummyReorderableList = null;
        private Texture2D redTexture = null;
        private Texture2D blueTexture = null;
        private Group selectedGroup = null;
        private Prop selectedProp = null;
        public override void InitializeTab(ref Avatar risAvatar)
        {
            InitializeGroupList(risAvatar);
            InitializePropList(null, risAvatar);
            InitializeGameObjectsList(true, risAvatar);
        }
        public override void DrawTab(ref Avatar risAvatar, Rect position, bool showingVerticalScroll)
        {
            if (GroupsReorderableList == null)
                InitializeGroupList(risAvatar);
            if (propsReorderableList == null)
                InitializePropList(null, risAvatar);
            if (gameObjectsDummyReorderableList == null)
                InitializeGameObjectsList(true, risAvatar);
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
                        var GroupsHeight = GroupsReorderableList.GetHeight();
                        using (new EditorGUILayout.HorizontalScope(GUILayout.Height(GroupsHeight), GUILayout.Width(scopeWidth)))
                        {
                            var oldSelectedIndex = GroupsReorderableList.index;
                            GroupsReorderableList.DoList(new Rect(scope.rect.x, scope.rect.y, scopeWidth, GroupsHeight));
                            selectedGroupIsChanged = oldSelectedIndex != GroupsReorderableList.index;
                            GUILayout.Space(0);
                        }

                    }
                }
                GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(1));
                var groupIndex = GroupsReorderableList.index;
                var groupIsSelected = risAvatar != null && risAvatar.Groups.Count >= 1 && groupIndex >= 0;
                var advanceMode = (risAvatar != null && risAvatar.MenuMode == RIS.MenuModeType.Advanced);

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
                        EditorGUIUtility.labelWidth = 80;
                        var prefixText = advanceMode ? "メニュー" : "グループ";
                        EditorGUILayout.LabelField(prefixText + "設定", new GUIStyle("ProjectBrowserHeaderBgTop"), GUILayout.ExpandWidth(true));
                        GUILayout.Space(3);
                        selectedGroup.Name = EditorGUILayout.TextField(prefixText + "名", selectedGroup.Name);

                        EditorGUI.BeginChangeCheck();

                        var icon = selectedGroup.Icon.GetObject();
                        icon = (Texture2D)EditorGUILayout.ObjectField("アイコン", icon, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));

                        var baseMenu = selectedGroup.BaseMenu.GetObject();
                        baseMenu = (VRCExpressionsMenu)EditorGUILayout.ObjectField("基礎メニュー", baseMenu, typeof(VRCExpressionsMenu), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));

                        if (EditorGUI.EndChangeCheck())
                        {
                            selectedGroup.Icon.SetObject(icon);
                            selectedGroup.BaseMenu.SetObject(baseMenu);
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
                        EditorGUIUtility.labelWidth = 80;
                        selectedProp.Name = EditorGUILayout.TextField("プロップ名", selectedProp.Name);

                        EditorGUI.BeginChangeCheck();

                        var icon = selectedProp.Icon.GetObject();
                        icon = (Texture2D)EditorGUILayout.ObjectField("アイコン", icon, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                        if (EditorGUI.EndChangeCheck())
                            selectedProp.Icon.SetObject(icon);

                        selectedProp.ExclusiveGroup = (RIS.ExclusiveGroupType)EditorGUILayout.EnumPopup("排他グループ", selectedProp.ExclusiveGroup);
                        if (selectedProp.ExclusiveGroup != RIS.ExclusiveGroupType.None)
                        {
                            var v2Mode = risAvatar.GetExclusiveMode(selectedProp.ExclusiveGroup) == RIS.ExclusiveModeType.ExclusiveV2;
                            EditorGUI.BeginChangeCheck();
                            v2Mode = EditorGUILayout.Toggle("┗V2モード", v2Mode);
                            if (EditorGUI.EndChangeCheck())
                                risAvatar.SetExclusiveMode(selectedProp.ExclusiveGroup, v2Mode ? RIS.ExclusiveModeType.ExclusiveV2 : RIS.ExclusiveModeType.Exclusive);
                        }
                        selectedProp.IsDefaultEnabled = EditorGUILayout.Toggle("初期状態", selectedProp.IsDefaultEnabled);
                        selectedProp.IsLocalOnly = EditorGUILayout.Toggle("ローカル動作", selectedProp.IsLocalOnly);
                        GUILayout.Space(5);
                        EditorGUILayout.LabelField("追加アニメーション");
                        EditorGUI.BeginChangeCheck();
                        var enableAnim = selectedProp.EnableAnimation.GetObject();
                        var disableAnim = selectedProp.DisableAnimation.GetObject();
                        enableAnim = (AnimationClip)EditorGUILayout.ObjectField("┣有効化時", enableAnim, typeof(AnimationClip), false);
                        disableAnim = (AnimationClip)EditorGUILayout.ObjectField("┗無効化時", disableAnim, typeof(AnimationClip), false);
                        if (EditorGUI.EndChangeCheck())
                        {
                            selectedProp.EnableAnimation.SetObject(enableAnim);
                            selectedProp.DisableAnimation.SetObject(disableAnim);
                        }
                        GUILayout.Space(5);
                        selectedProp.UseResetTimer = EditorGUILayout.Toggle("オフタイマー", selectedProp.UseResetTimer);
                        if (selectedProp.UseResetTimer)
                            selectedProp.ResetSecond = Mathf.Min(60, Mathf.Max(0, EditorGUILayout.FloatField("┗秒数", selectedProp.ResetSecond)));
                        GUILayout.Space(5);

                        EditorGUI.BeginChangeCheck();
                        var materialOverride = selectedProp.MaterialOverride.GetObject();
                        materialOverride = (Material)EditorGUILayout.ObjectField("マテリアル", materialOverride, typeof(Material), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                        if (EditorGUI.EndChangeCheck())
                            selectedProp.MaterialOverride.SetObject(materialOverride);
                        GUILayout.Space(5);
                        if (gameObjectsReorderableList == null || selectedPropIsChanged)
                            InitializeGameObjectsList(false, risAvatar, selectedProp);
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
            GroupsReorderableList = new ReorderableList(groups, typeof(Group))
            {
                drawHeaderCallback = rect =>
                {
                    if (risAvatar != null && risAvatar.MenuMode == RIS.MenuModeType.Advanced)
                        EditorGUI.LabelField(rect, "メニュー一覧" + $": {groups.Count}");
                    else
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
                            index = GroupsReorderableList.index = -1;
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
                        if (risAvatar.MenuMode == RIS.MenuModeType.Advanced && group.UseResetButton)
                            maxPropCount = 7;
                        if (risAvatar.MenuMode == RIS.MenuModeType.Advanced && group.BaseMenu?.GetObject() != null)
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
                    var rawPropName = props[index].GetPropName(risAvatar);
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
                            if (risAvatar.MenuMode == RIS.MenuModeType.Advanced && group.BaseMenu?.GetObject() != null)
                                maxPropCount = 8 - group.BaseMenu.GetObject().controls.Count;
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
        private void InitializeGameObjectsList(bool dummyFlag, Avatar risAvatar, Prop prop = null)
        {
            List<GUIDPathPair<GameObject>> gameObjects = null;
            var propFlag = prop != null && prop.TargetObjects != null;
            if (!dummyFlag && propFlag)
                gameObjects = prop.TargetObjects;
            else
                gameObjects = new List<GUIDPathPair<GameObject>>();
            var list = new ReorderableList(gameObjects, typeof(GameObject));
            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "オブジェクト一覧" + $": {gameObjects.Count}");
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
                    var parent = risAvatar.AvatarRoot?.GetObject()?.gameObject;
                    if (parent != null && index >= 0 && index < gameObjects.Count)
                    {

                        var targetObject = gameObjects[index]?.GetObject(parent);
                        if (targetObject != null && !targetObject.IsChildOf(parent))
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

        public override string[] CheckErrors(ref Avatar risAvatar)
        {
            if (risAvatar == null)
                return new string[] { };

            var errors = new List<string>();
            var prefixText = "グループ";
            if (risAvatar.Groups.Count == 0)
                errors.Add(prefixText + "を追加してください。");

            foreach (var group in risAvatar.Groups)
            {
                var groupName = group.Name;
                if (string.IsNullOrEmpty(groupName))
                    groupName = "Group" + risAvatar.Groups.IndexOf(group);

                if (!group.Props.Any())
                    errors.Add($"{prefixText}[{groupName}]にプロップが登録されていません。");

                var maxPropsCount = 8;
                if (group.BaseMenu != null && group.BaseMenu?.GetObject() != null)
                    maxPropsCount = 8 - group.BaseMenu.GetObject().controls.Count;
                if (group.Props.Count > maxPropsCount)
                    errors.Add($"{prefixText}[{groupName}]" + string.Format("のプロップ数がオーバーしています。(最大{0})", maxPropsCount));

                foreach (var prop in group.Props)
                {
                    var propName = prop.Name;
                    if (string.IsNullOrEmpty(propName))
                        propName = "Prop" + group.Props.IndexOf(prop);

                    if (!prop.TargetObjects.Any(n => n != null) && prop.DisableAnimation == null && prop.EnableAnimation == null)
                        errors.Add($"{prefixText}[{groupName}]のプロップ" + $"[{propName}]にオブジェクトもアニメーションも登録されていません。");
                }
            }


            return errors.ToArray();
        }

        public override void BuildFXLayer(ref Avatar risAvatar, string autoGeneratedFolder)
        {
            var jsonData = JsonConvert.SerializeObject(risAvatar);
            using (var sw = new StreamWriter($"{autoGeneratedFolder}\\test.json", false, Encoding.UTF8))
            {
                sw.Write(jsonData);
            }
        }
    }
}