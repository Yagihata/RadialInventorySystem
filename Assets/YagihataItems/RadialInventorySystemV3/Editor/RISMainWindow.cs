using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Graphs;
using UnityEditorInternal;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;
using YagihataItems.YagiUtils;

namespace YagihataItems.RadialInventorySystemV3
{
    public class RISMainWindow : EditorWindow
    {
        private RISSettings settings;
        private VRCAvatarDescriptor avatarRoot = null;
        private VRCAvatarDescriptor avatarRootBefore = null;
        private IndexedList indexedList = new IndexedList();
        private RISVariables variables;
        private Vector2 scrollPosition = new Vector2();
        [SerializeField] private Texture2D headerTexture = null;
        private float beforeWidth = 0f;
        private ReorderableList propGroupsReorderableList;
        private ReorderableList propsReorderableList;
        enum TabType
        {
            Simple,
            Advanced
        }

        private TabType currentTab = TabType.Simple;
        [MenuItem("RadialInventory/RISV3 Editor")]
        private static void Create()
        {
            GetWindow<RISMainWindow>("RISV3 Editor");
        }
        private void OnGUI()
        {
            using (var scrollScope = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                using (var verticalScope = new EditorGUILayout.VerticalScope())
                {
                    scrollPosition = scrollScope.scrollPosition;
                    if (headerTexture == null)
                        headerTexture = AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(RISV3.WorkFolderPath + "Textures/MenuHeader.png");

                    var newerVersion = RISVersionChecker.GetNewerVersion();
                    var showingVerticalScroll = false;
                    if (verticalScope.rect.height != 0)
                        showingVerticalScroll = verticalScope.rect.height > position.size.y;
                    var height = position.size.x / headerTexture.width * headerTexture.height;
                    if (height > headerTexture.height)
                        height = headerTexture.height;
                    var width = position.size.x - (showingVerticalScroll ? 22 : 8);
                    GUILayout.Space(2);
                    EditorGUILayoutExtra.HeaderWithVersionInfo(headerTexture, width == beforeWidth ? width: beforeWidth, height, newerVersion, RISV3.CurrentVersion, "ris");
                    beforeWidth = width;
                    EditorGUILayoutExtra.Space();
                    var avatarDescriptors = FindObjectsOfType(typeof(VRCAvatarDescriptor));
                    indexedList.list = avatarDescriptors.Select(n => n.name).ToArray();
                    indexedList.index = EditorGUILayoutExtra.IndexedStringList("対象アバター", indexedList);
                    if (indexedList.index >= 0 && indexedList.index < avatarDescriptors.Length)
                        avatarRoot = avatarDescriptors[indexedList.index] as VRCAvatarDescriptor;
                    else
                        avatarRoot = null;
                    var rootIsNull = avatarRoot == null;
                    if (rootIsNull)
                    {
                        avatarRootBefore = null;
                    }
                    using (new EditorGUI.DisabledGroupScope(rootIsNull))
                    {
                        //AvatarRootが変更されたら設定を復元
                        if (!rootIsNull && avatarRoot != avatarRootBefore)
                        {
                            RestoreSettings();
                            avatarRootBefore = avatarRoot;

                        }
                        if (!rootIsNull)
                            variables.AvatarRoot = avatarRoot;

                        EditorGUILayoutExtra.SeparatorWithSpace();

                        using (new EditorGUILayout.VerticalScope())
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.Space(5);
                                currentTab = (TabType)GUILayout.Toolbar((int)currentTab, TabStyle.GetTabToggles<TabType>(), TabStyle.TabButtonStyle, TabStyle.TabButtonSize);
                                GUILayout.FlexibleSpace();
                            }
                            var skin = GUI.skin.box;
                            skin.margin.top = 0;
                            using (new EditorGUILayout.VerticalScope(skin))
                            {
                                DrawTab(currentTab);
                            }
                        }

                        EditorGUILayoutExtra.SeparatorWithSpace();
                        if (variables != null)
                        {
                            variables.WriteDefaults = EditorGUILayout.Toggle("Write Defaults", variables.WriteDefaults);
                            UnityEditor.Animations.AnimatorController fxLayer = null;
                            if (!rootIsNull) fxLayer = avatarRoot.GetFXLayer(RISV3.AutoGeneratedFolderPath + variables.FolderID + "/", false);
                            if (fxLayer != null && !fxLayer.ValidateWriteDefaults(variables.WriteDefaults))
                            {
                                EditorGUILayout.HelpBox("WriteDefaultsがFXレイヤー内で統一されていません。\n" +
                                    "このままでも動作はしますが、表情切り替えにバグが発生する場合があります。\n" +
                                    "WriteDefaultsのチェックを切り替えてもエラーメッセージが消えない場合は使用している他のアバターギミックなどを確認してみてください。", MessageType.Warning);
                            }
                            variables.OptimizeParams = EditorGUILayout.Toggle("パラメータの最適化", variables.OptimizeParams);
                            if (!variables.OptimizeParams)
                            {
                                EditorGUILayout.HelpBox("パラメータの最適化が無効になっています。\n" +
                                    "空パラメータや重複パラメータを自動で削除したい場合は\n" +
                                    "パラメータの最適化を行ってください。", MessageType.Info);
                            }
                        }
                        else
                        {
                            EditorGUILayout.Toggle("Write Defaults", false);
                            EditorGUILayout.Toggle("パラメータの最適化", true);
                        }
                        EditorGUILayoutExtra.SeparatorWithSpace();
                        if (GUILayout.Button("適用する"))
                        {
                            SaveSettings();
                            //MakeMenu();
                            //ApplyToAvatar();
                        }
                        if (GUILayout.Button("適用を解除する"))
                        {
                            SaveSettings();
                            //RemoveAutoGenerated();
                        }
                    }
                    EditorGUILayoutExtra.SeparatorWithSpace();
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        EditorGUILayout.HelpBox("Radial Inventory Systemをダウンロードしてくださり、誠にありがとうございます！\n" +
                        "使用法がわからない場合は、下記リンクより説明書をご覧になった上で使ってみてください。\n" +
                        "もしバグや機能追加の要望などありましたら、TwitterのDMで教えていただけますと幸いです。", MessageType.None);
                        //EditorGUILayoutExtra.LinkLabel("AniPIN – Avatar Lock System 説明書", Color.blue, new Vector2(), 0, RISV3.manualUrl);
                        using(new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayoutExtra.LinkLabel("Twitter : @Yagihata4x", Color.blue, new Vector2(), 0, RISV3.TwitterUrl);
                            GUILayout.FlexibleSpace();
                        }
                    }
                }
            }
        }
        private void RestoreSettings()
        {
            variables = new RISVariables();
            settings = EditorExtSettingsTool.RestoreSettings<RISSettings>(avatarRoot, RISV3.SettingsName) as RISSettings;
            if (settings != null)
                variables = settings.GetVariables() as RISVariables;
            else
                variables.FolderID = System.Guid.NewGuid().ToString();
            InitializeGroupList();
        }
        private void SaveSettings()
        {
            Debug.Assert(variables != null);
            EditorExtSettingsTool.SaveSettings<RISSettings>(avatarRoot, RISV3.SettingsName, variables);
        }
        private void DrawTab(TabType tabType)
        {
            if (propGroupsReorderableList == null)
                InitializeGroupList();
            if (propsReorderableList == null)
                InitializePropList(null);
            var height = propGroupsReorderableList.GetHeight();
            var cellWidth = position.width / 3f - 15f;
            using (new EditorGUILayout.HorizontalScope())
            {
                var selectedGroupChangeFlag = true;
                using (new EditorGUILayout.VerticalScope())
                {
                    using (var scope = new EditorGUILayout.HorizontalScope())
                    {
                        var scopeWidth = cellWidth - 40;
                        using (new EditorGUILayout.HorizontalScope(GUILayout.Height(height), GUILayout.Width(scopeWidth)))
                        {
                            var oldSelectedIndex = propGroupsReorderableList.index;
                            propGroupsReorderableList.DoList(new Rect(scope.rect.x, scope.rect.y, scopeWidth, height));
                            if(variables != null)
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
                using (new EditorGUI.DisabledGroupScope(!groupIsSelected))
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(cellWidth + 20)))
                    {
                        EditorGUIUtility.labelWidth = 80;
                        if (groupIsSelected)
                        {
                            variables.Groups[groupIndex].GroupName = EditorGUILayout.TextField("グループ名", variables.Groups[groupIndex].GroupName);
                            variables.Groups[groupIndex].GroupIcon = 
                                (Texture2D)EditorGUILayout.ObjectField("アイコン", variables.Groups[groupIndex].GroupIcon, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            variables.Groups[groupIndex].ExclusiveMode = EditorGUILayout.Toggle("排他モード", variables.Groups[groupIndex].ExclusiveMode);
                            if(selectedGroupChangeFlag)
                                InitializePropList(variables.Groups[groupIndex]);
                            using (var scope = new EditorGUILayout.HorizontalScope())
                            {
                                var scopeWidth = cellWidth + 20;
                                var propListHeight = propsReorderableList.GetHeight();
                                using (new EditorGUILayout.HorizontalScope(GUILayout.Height(propListHeight), GUILayout.Width(scopeWidth)))
                                {
                                    propsReorderableList.DoList(new Rect(scope.rect.x, scope.rect.y, scopeWidth, propListHeight));
                                    variables.Groups[groupIndex].Props = (List<Prop>)propsReorderableList.list;
                                    GUILayout.Space(0);
                                }
                            }
                            targetProp = (variables.Groups[groupIndex].Props.Count >= 1 && propsReorderableList.index >= 0) ?
                                variables.Groups[groupIndex].Props[propsReorderableList.index] : null;
                        }
                        else
                        {
                            EditorGUILayout.TextField("グループ名", "");
                            EditorGUILayout.ObjectField("アイコン", null, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            EditorGUILayout.Toggle("排他モード", false);
                            if (selectedGroupChangeFlag)
                                InitializePropList(null);
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
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(cellWidth + 15)))
                    {
                        EditorGUILayout.LabelField("プロップ名");
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(20);
                            if (targetProp != null)
                                targetProp.PropName = EditorGUILayout.TextField(targetProp.PropName);
                            else
                                EditorGUILayout.TextField("");


                        }
                        EditorGUILayout.LabelField("オブジェクト");
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(20);
                            if (targetProp != null)
                                targetProp.TargetObject = (GameObject)EditorGUILayout.ObjectField(targetProp.TargetObject, typeof(GameObject), true);
                            else
                                EditorGUILayout.ObjectField(null, typeof(GameObject), true);
                        }
                        EditorGUILayout.LabelField("デフォルトで表示");
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(20);
                            if (targetProp != null)
                                targetProp.IsDefaultEnabled = EditorGUILayout.Toggle(targetProp.IsDefaultEnabled);
                            else
                                EditorGUILayout.Toggle(false);
                        }
                        EditorGUILayout.LabelField("切り替えのローカル化");
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(20);
                            if (targetProp != null)
                                targetProp.LocalOnly = EditorGUILayout.Toggle(targetProp.LocalOnly);
                            else
                                EditorGUILayout.Toggle(false);
                        }
                    }
                }
                GUILayout.FlexibleSpace();

            }
            EditorGUIUtility.labelWidth = 0;
        }
        private void InitializeGroupList()
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
                    EditorGUI.LabelField(rect, $"{"グループ一覧"}: {groups.Count}");
                    var position =
                        new Rect(
                            rect.x + rect.width - 20f,
                            rect.y,
                            20f,
                            13f
                        );
                    if (GUI.Button(position, ReorderableListStyle.AddContent, ReorderableListStyle.AddStyle))
                    {
                        Undo.RecordObject(settings, $"Add new PropGroup.");
                        variables.Groups.Add(new PropGroup() { GroupName = "Group" + groups.Count });
                        EditorUtility.SetDirty(settings);
                    }
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (groups.Count <= index)
                        return;

                    GUI.Label(rect, groups[index].GroupName);
                    rect.x = rect.x + rect.width - 20f;
                    rect.width = 20f;
                    if (GUI.Button(rect, ReorderableListStyle.SubContent, ReorderableListStyle.SubStyle))
                    {
                        Undo.RecordObject(settings, $"Remove PropGroup - \"{groups[index].GroupName}\".");
                        groups.RemoveAt(index);
                        EditorUtility.SetDirty(settings);
                    }
                },

                drawFooterCallback = rect => { },
                footerHeight = 0f,
                elementHeightCallback = index =>
                {
                    if (groups.Count <= index)
                        return 0;

                    return EditorGUIUtility.singleLineHeight * 1.45f;
                }

            };
        }
        private void InitializePropList(PropGroup group)
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
                    EditorGUI.LabelField(rect, $"{"プロップ一覧"}: {props.Count}");
                    var position =
                        new Rect(
                            rect.x + rect.width - 20f,
                            rect.y,
                            20f,
                            13f
                        );
                    if (GUI.Button(position, ReorderableListStyle.AddContent, ReorderableListStyle.AddStyle))
                    {
                        Undo.RecordObject(settings, $"Add new Prop.");
                        group.Props.Add(new Prop());
                        EditorUtility.SetDirty(settings);
                    }
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (props.Count <= index)
                        return;
                    var rawPropName = props[index].GetPropName();
                    var propName = !string.IsNullOrEmpty(rawPropName) ? rawPropName : $"Prop{index} (未割り当て)";
                    GUI.Label(rect, propName);
                    rect.x = rect.x + rect.width - 20f;
                    rect.width = 20f;
                    if (GUI.Button(rect, ReorderableListStyle.SubContent, ReorderableListStyle.SubStyle))
                    {
                        Undo.RecordObject(settings, $"Remove Prop - \"{propName}\".");
                        props.RemoveAt(index);
                        EditorUtility.SetDirty(settings);
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