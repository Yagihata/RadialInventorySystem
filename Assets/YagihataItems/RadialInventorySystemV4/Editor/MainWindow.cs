﻿using System;
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

namespace YagihataItems.RadialInventorySystemV4
{
    public class MainWindow : EditorWindow
    {
        //private RISSettings settings;
        private IndexedList indexedList = new IndexedList();
        private Vector2 scrollPosition = new Vector2();
        [SerializeField] private Texture2D headerTexture = null;
        private float beforeWidth = 0f;
        private bool showingVerticalScroll;
        private Rect tabScopeRect = new Rect();
        private Dictionary<RIS.MenuModeType, EditorTab> tabItems = null;
        private GUIStyle countBarStyleL;
        private GUIStyle countBarStyleR;
        private GUIStyle donatorLabelStyle;
        private VRCAvatarDescriptor avatarRoot = null;
        private Avatar risAvatar = null;
        [MenuItem("Radial Inventory/RISV4 Editor")]
        private static void Create()
        {
            GetWindow<MainWindow>("RISV4 Editor");
        }
        private void OnGUI()
        {
            if(tabItems == null)
            {
                tabItems = new Dictionary<RIS.MenuModeType, EditorTab>();
                //tabItems.Add(RIS.MenuModeType.Simple, new TabSimple());
                //tabItems.Add(RIS.MenuModeType.Basic, new TabBasic());
                tabItems.Add(RIS.MenuModeType.Advanced, new TabAdvanced());
            }
            if (countBarStyleL == null)
                countBarStyleL = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.UpperLeft, margin = new RectOffset(10, 10, 0, 0) };
            if (countBarStyleR == null)
                countBarStyleR = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.UpperRight, margin = new RectOffset(10, 10, 0, 0) };
            if (donatorLabelStyle == null)
                donatorLabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperCenter, margin = new RectOffset(10, 10, 20, 20), wordWrap = true };

            using (var scrollScope = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                using (var verticalScope = new EditorGUILayout.VerticalScope())
                {
                    scrollPosition = scrollScope.scrollPosition;
                    if (headerTexture == null)
                        headerTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/ris_logo.png");
                    var newerVersion = VersionChecker.GetNewerVersion();
                    var showingVerticalScrollOld = showingVerticalScroll;
                    if (verticalScope.rect.height != 0)
                        showingVerticalScroll = verticalScope.rect.height >= position.size.y;
                    var height = position.size.x / headerTexture.width * headerTexture.height;
                    if (height > headerTexture.height)
                        height = headerTexture.height;
                    var width = position.size.x - (showingVerticalScroll ? 22 : 8);
                    GUILayout.Space(2);
                    var newVersion = newerVersion;
                    if (!newerVersion.StartsWith("ris_"))
                        newVersion = RIS.CurrentVersion;
                    EditorGUILayoutExtra.HeaderWithVersionInfo(headerTexture, width == beforeWidth ? width: beforeWidth, height, newVersion, RIS.CurrentVersion, "ris", "新しいバージョンがあります", RIS.DownloadUrl);
                    
                    EditorGUILayoutExtra.Space();

                    if(RISV3toV4Updater.HasV3SettingsOnScene(RIS.SettingsNameV3))
                    {
                        EditorGUILayoutExtra.Separator();
                        EditorGUILayout.HelpBox("V3の設定オブジェクトが見つかりました。\nこの設定をV4に移行する場合は下のボタンを押してください。", MessageType.Info);
                        if(GUILayout.Button("RIS V3 -> RIS V4 設定の移行"))
                        {
                        }
                        EditorGUILayoutExtra.Separator();
                        EditorGUILayoutExtra.Space();
                    }
                    EditorGUI.BeginChangeCheck();
                    var avatarDescriptors = FindObjectsOfType(typeof(VRCAvatarDescriptor));
                    indexedList.list = avatarDescriptors.Select(n => n.name).ToArray();
                    indexedList.index = EditorGUILayoutExtra.IndexedStringList("対象アバター", indexedList, "（未選択）");

                    if (EditorGUI.EndChangeCheck() || (avatarRoot == null && indexedList.index != -1))
                    {
                        if (avatarDescriptors.Count() > 0 && indexedList.index >= 0 && indexedList.index < avatarDescriptors.Length)
                        {
                            avatarRoot = avatarDescriptors[indexedList.index] as VRCAvatarDescriptor;
                            risAvatar = new Avatar();
                            foreach (var v in tabItems.Values)
                                v.InitializeTab(ref risAvatar);
                        }
                        else
                        {
                            avatarRoot = null;
                            risAvatar = null;
                        }
                    }
                    var isRootNull = avatarRoot == null;
                    if (risAvatar == null)
                        risAvatar = new Avatar();
                    if (!isRootNull && risAvatar != null && risAvatar.AvatarRoot?.GetObject() == null)
                        risAvatar.AvatarRoot.SetObject(avatarRoot);
                    var memoryAdded = 0;
                    using (new EditorGUI.DisabledGroupScope(isRootNull))
                    {
                        EditorGUILayoutExtra.SeparatorWithSpace();

                        using (new EditorGUILayout.VerticalScope())
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.Space(5);
                                risAvatar.MenuMode = (RIS.MenuModeType)GUILayout.Toolbar((int)risAvatar.MenuMode, TabStyle.GetTabToggles<RIS.MenuModeType>(), TabStyle.TabButtonStyle, TabStyle.TabButtonSize);
                                GUILayout.FlexibleSpace();
                            }
                            var skin = new GUIStyle(GUI.skin.box);
                            skin.margin.top = 0;
                            using (var scope = new EditorGUILayout.VerticalScope(skin))
                            {
                                tabScopeRect = scope.rect;
                                if (tabItems.ContainsKey(risAvatar.MenuMode))
                                    tabItems[risAvatar.MenuMode].DrawTab(ref risAvatar, position, showingVerticalScroll && showingVerticalScrollOld);
                            }
                            var memoryNow = 0;
                            var memoryUseFromScript = 0;
                            if (!isRootNull)
                            {
                                var expressionParameter = avatarRoot.GetExpressionParameters(RIS.AutoGeneratedFolderPath + risAvatar.UniqueID + "/", false);
                                if (expressionParameter != null)
                                {
                                    var paramsTemp = new List<VRCExpressionParameters.Parameter>();
                                    foreach (var groupIndex in Enumerable.Range(0, risAvatar.Groups.Count))
                                    {
                                        var group = risAvatar.Groups[groupIndex];
                                        if (risAvatar.MenuMode == RIS.MenuModeType.Simple && group.UseResetButton)
                                            paramsTemp.Add(new VRCExpressionParameters.Parameter() { name = $"RISV3-G{groupIndex}RESET", valueType = VRCExpressionParameters.ValueType.Bool });
                                        foreach (var propIndex in Enumerable.Range(0, group.Props.Count))
                                            paramsTemp.Add(new VRCExpressionParameters.Parameter() { name = $"RISV3-G{groupIndex}P{propIndex}", valueType = VRCExpressionParameters.ValueType.Bool });
                                    }
                                    var arr = paramsTemp.ToArray();
                                    memoryNow = expressionParameter.CalculateMemoryCount(arr, risAvatar.OptimizeParameters, "RISV3", true);
                                    memoryAdded = expressionParameter.CalculateMemoryCount(arr, risAvatar.OptimizeParameters, "RISV3");
                                    memoryUseFromScript = paramsTemp.Sum(n => VRCExpressionParameters.TypeCost(n.valueType));
                                }
                            }
                            EditorGUILayoutExtra.CostViewer(memoryNow, memoryAdded, memoryUseFromScript, "使用メモリ", "残メモリ", countBarStyleL, countBarStyleR);
                        }

                        EditorGUILayoutExtra.SeparatorWithSpace();
                        string[] errors;
                        if (tabItems.ContainsKey(risAvatar.MenuMode))
                            errors = tabItems[risAvatar.MenuMode].CheckErrors(ref risAvatar);
                        else
                            errors = new string[] { };
                        var memoryOver = false;

                        risAvatar.UseWriteDefaults = EditorGUILayout.Toggle("Write Defaultsを使用(非推奨)", risAvatar.UseWriteDefaults);
                        UnityEditor.Animations.AnimatorController fxLayer = null;
                        if (!isRootNull) fxLayer = avatarRoot.GetFXLayer(RIS.AutoGeneratedFolderPath + risAvatar.UniqueID + "/", false);
                        risAvatar.OptimizeParameters = EditorGUILayout.Toggle("パラメータの最適化", risAvatar.OptimizeParameters);
                        risAvatar.ApplyEnableDefault = EditorGUILayout.Toggle("Propの初期状態を反映", risAvatar.ApplyEnableDefault);
                        EditorGUILayoutExtra.Space();
                        var showFXWarning = fxLayer != null && !fxLayer.ValidateWriteDefaults(risAvatar.UseWriteDefaults);
                        var showParamInfo = !risAvatar.OptimizeParameters;
                        memoryOver = memoryAdded > VRCExpressionParameters.MAX_PARAMETER_COST;
                        if (showFXWarning || showParamInfo || errors.Any() || memoryOver)
                            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                            {
                                if (memoryOver)
                                {
                                    EditorGUILayout.HelpBox(string.Format("使用メモリの合計は{0}以下でなければなりません。", VRCExpressionParameters.MAX_PARAMETER_COST), MessageType.Error);
                                }
                                foreach (var error in errors)
                                {
                                    EditorGUILayout.HelpBox(error, MessageType.Error);
                                }
                                if (showFXWarning)
                                {
                                    EditorGUILayout.HelpBox("WriteDefaultsがFXレイヤー内で統一されていません。\nこのままでも動作はしますが、表情切り替えにバグが発生する場合があります。\nWriteDefaultsのチェックを切り替えてもエラーメッセージが消えない場合は使用している他のアバターギミックなどを確認してみてください。", MessageType.Warning);
                                }
                                if (showParamInfo)
                                {
                                    EditorGUILayout.HelpBox("パラメータの最適化が無効になっています。空パラメータや重複パラメータを自動で削除したい場合は\nパラメータの最適化を行ってください。", MessageType.Info);
                                }
                            }

                        EditorGUILayoutExtra.SeparatorWithSpace();
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUI.DisabledGroupScope(errors.Any() || memoryOver))
                            {
                                if (GUILayout.Button("適用する", new GUIStyle("ButtonLeft")))
                                {
                                    SaveSettings();
                                    if (tabItems.ContainsKey(risAvatar.MenuMode))
                                        GimmickBuilder.ApplyToAvatar(risAvatar, tabItems[risAvatar.MenuMode]);
                                }
                            }
                            if (GUILayout.Button("適用を解除する", new GUIStyle("ButtonRight")))
                            {
                                SaveSettings();
                                GimmickBuilder.RemoveFromAvatar(risAvatar);
                            }
                        }
                    }

                    EditorGUILayoutExtra.SeparatorWithSpace();
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        EditorGUILayout.HelpBox("Radial Inventory Systemをダウンロードしてくださり、誠にありがとうございます！\n" +
                            "使用法がわからない場合は、下記リンクより説明書をご覧になった上で使ってみてください。\n" +
                            "もしバグや機能追加の要望などありましたら、TwitterのDMで教えていただけますと幸いです。", MessageType.None);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayoutExtra.LinkLabel("Radial Inventory System V3 説明書", Color.blue, new Vector2(), 0, RIS.ManualUrl);
                            GUILayout.FlexibleSpace();
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayoutExtra.LinkLabel("Twitter : @Yagihata4x", Color.blue, new Vector2(), 0, RIS.TwitterUrl);
                            GUILayout.FlexibleSpace();
                        }
                    }
                    var donators = DonatorListUpdater.GetDonators();
                    if(!string.IsNullOrWhiteSpace(donators))
                    {
                        GUILayout.Space(10);
                        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                        {
                            EditorGUILayout.LabelField("寄付していただいた方々！（敬称略）", new GUIStyle("ProjectBrowserHeaderBgTop"), GUILayout.ExpandWidth(true));
                            EditorGUILayout.LabelField(donators, donatorLabelStyle, GUILayout.ExpandWidth(true));
                        }
                    }
                    beforeWidth = width;
                    GUILayout.FlexibleSpace();
                }
            }
        }
        private void RestoreSettings()
        {
            /*variables = new RISVariables();
            // AvatarRootが一致するRISSettingsもしくは、nameが一致するRISSettingsを取得。
            settings = EditorExtSettingsTool.RestoreSettings<RISSettings>(avatarRoot, RIS.SettingsNameV4) as RISSettings;

            if (settings != null){
                variables = settings.GetVariables() as RISVariables;
                if(avatarRoot != variables.AvatarRoot)
                {
                    // 指定したAvatarRootとRISSettingsのAvatarRootが異なる場合、インスタンスをCloneした上でTargetObjectsを割り当てしなおす。
                    // (AvatarRootが異なる場合、他のAvatarRootの設定をコピーしていることが想定される。インスタンスをCloneしないと元の設定に影響してしまう。)
                    // 異なるAvatarRootに属するTargetObjectsは指定したAvatarRootの子ではないためNullになってしまうので、
                    // hierarchyが一致するobjectにremapすることで回避する。(元のAvatarRootを残しておく必要あり。)
                    // remap終了後AvatarRootを置換して設定を保存する。
                    variables.FolderID = System.Guid.NewGuid().ToString();
                    if(variables.Groups == null)
                    {
                        EditorUtility.DisplayDialog("Radial Inventory System", "グループリストが破損していたため、\r\nリストの初期化を行いました。", "OK");
                        variables.Groups = new List<PropGroup>();
                    }
                    foreach(var groupIndex in Enumerable.Range(0, variables.Groups.Count))
                    {
                        if(variables.Groups[groupIndex] == null)
                        {
                            EditorUtility.DisplayDialog("Radial Inventory System", $"グループ{groupIndex}が破損していたため、\r\nグループの初期化を行いました。", "OK");
                            variables.Groups[groupIndex] = ScriptableObject.CreateInstance<PropGroup>();
                        }
                        else if (variables.Groups[groupIndex].Props == null)
                        {
                            EditorUtility.DisplayDialog("Radial Inventory System", $"グループ{groupIndex}のプロップリストが破損していたため、\r\nプロップリストの初期化を行いました。", "OK");
                            variables.Groups[groupIndex].Props = new List<Prop>();
                        }
                        variables.Groups[groupIndex] = (PropGroup)variables.Groups[groupIndex].Clone();
                        foreach (var propIndex in Enumerable.Range(0, variables.Groups[groupIndex].Props.Count))
                        {
                            if (variables.Groups[groupIndex].Props[propIndex] == null)
                            {
                                EditorUtility.DisplayDialog("Radial Inventory System", $"グループ{groupIndex}のプロップ{propIndex}が破損していたため、\r\nプロップの初期化を行いました。", "OK");
                                variables.Groups[groupIndex].Props[propIndex] = ScriptableObject.CreateInstance<Prop>();
                            }
                            else if(variables.Groups[groupIndex].Props[propIndex].TargetObjects == null)
                            {
                                EditorUtility.DisplayDialog("Radial Inventory System", $"グループ{groupIndex}のプロップ{propIndex}の\r\nターゲットリストが破損していたため、リストの初期化を行いました。", "OK");
                                variables.Groups[groupIndex].Props[propIndex].TargetObjects = new List<GameObject>();
                            }
                            // PropはPropGroupのCloneの中でClone済
                            // variables.Groups[groupIndex].Props[propIndex] = (Prop)variables.Groups[groupIndex].Props[propIndex].Clone();

                            GameObject targetObject = null;
                            // AdvancedModeのTargetObjectsのチェック
                            foreach (var objIndex in Enumerable.Range(0, variables.Groups[groupIndex].Props[propIndex].TargetObjects.Count))
                            {
                                targetObject = variables.Groups[groupIndex].Props[propIndex].TargetObjects[objIndex];
                                if (targetObject != null && !targetObject.IsChildOf(avatarRoot.gameObject))
                                {
                                    // 指定したavatarRootの子でない場合、元のavatarRootを起点としてパスを取得。
                                    var objPath = YagiAPI.GetGameObjectPath(targetObject, variables.AvatarRoot.gameObject);
                                    // 指定したavatarRootの子に同じパスのObjectが存在すれば置換。
                                    var targetTransform = avatarRoot.transform.Find(objPath);
                                    if (targetTransform != null)
                                    {
                                        targetObject = targetTransform.gameObject;
                                        variables.Groups[groupIndex].Props[propIndex].TargetObjects[objIndex] = targetObject;
                                    }
                                }
                            }

                            // SimpleModeのTargetObjectのチェック
                            targetObject = variables.Groups[groupIndex].Props[propIndex].TargetObject;
                            if(targetObject != null && !targetObject.IsChildOf(avatarRoot.gameObject))
                            {
                                // 指定したavatarRootの子でない場合、元のavatarRootを起点としてパスを取得。
                                var objPath = YagiAPI.GetGameObjectPath(targetObject, variables.AvatarRoot.gameObject);
                                // 指定したavatarRootの子に同じパスのObjectが存在すれば置換。
                                var targetTransform = avatarRoot.transform.Find(objPath);
                                if (targetTransform != null)
                                {
                                    targetObject = targetTransform.gameObject;
                                    variables.Groups[groupIndex].Props[propIndex].TargetObject = targetObject;
                                }

                            }
                        }
                    }
                    variables.AvatarRoot = avatarRoot;
                    SaveSettings();
                }
            }
            else{
                // 保存された設定なし
                variables.FolderID = System.Guid.NewGuid().ToString();
            }
            */
            //InitializeGroupList();
        }
        private void SaveSettings()
        {
            //Debug.Assert(variables != null);
            //EditorExtSettingsTool.SaveSettings<RISSettings>(avatarRoot, RIS.SettingsNameV4, variables);
        }
    }
}