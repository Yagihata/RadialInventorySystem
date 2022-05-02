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

namespace YagihataItems.RadialInventorySystemV4
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
        private bool showingVerticalScroll;
        private Rect tabScopeRect = new Rect();
        private Dictionary<RIS.RISMode, EditorTab> tabItems = null;
        [MenuItem("Radial Inventory/RISV4 Editor")]
        private static void Create()
        {
            GetWindow<RISMainWindow>("RISV4 Editor");
        }
        private void OnGUI()
        {
            if(tabItems == null)
            {
                tabItems = new Dictionary<RIS.RISMode, EditorTab>();
                tabItems.Add(RIS.RISMode.Simple, new TabSimple());
                tabItems.Add(RIS.RISMode.Basic, new TabBasic());
                tabItems.Add(RIS.RISMode.Advanced, new TabAdvanced());
            }
            using (var scrollScope = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                using (var verticalScope = new EditorGUILayout.VerticalScope())
                {
                    scrollPosition = scrollScope.scrollPosition;
                    if (headerTexture == null)
                        headerTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/ris_logo.png");
                    var newerVersion = RISVersionChecker.GetNewerVersion();
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
                    EditorGUILayoutExtra.HeaderWithVersionInfo(headerTexture, width == beforeWidth ? width: beforeWidth, height, newVersion, RIS.CurrentVersion, "ris", RISMessageStrings.Strings.str_NewVersion, RIS.DownloadUrl);
                    
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
                    var avatarDescriptors = FindObjectsOfType(typeof(VRCAvatarDescriptor));
                    indexedList.list = avatarDescriptors.Select(n => n.name).ToArray();
                    indexedList.index = EditorGUILayoutExtra.IndexedStringList(RISMessageStrings.Strings.str_TargetAvatar, indexedList, RISMessageStrings.Strings.str_Unselected);
                    if (avatarDescriptors.Count() > 0 && indexedList.index >= 0 && indexedList.index < avatarDescriptors.Length)
                        avatarRoot = avatarDescriptors[indexedList.index] as VRCAvatarDescriptor;
                    else
                    {
                        avatarRoot = null;
                        settings = null;
                        variables = null;
                        /*InitializeGroupList();
                        InitializePropList(null);
                        InitializeGameObjectsList(true);*/
                    }
                    var rootIsNull = avatarRoot == null;
                    if (rootIsNull)
                    {
                        avatarRootBefore = null;
                    }
                    int memoryAdded = 0;
                    using (new EditorGUI.DisabledGroupScope(rootIsNull))
                    {
                        if(!rootIsNull)
                        {
                            if (avatarRoot != avatarRootBefore)
                            {
                                RestoreSettings();
                                avatarRootBefore = avatarRoot;
                            }
                            variables.AvatarRoot = avatarRoot;
                        }

                        EditorGUILayoutExtra.SeparatorWithSpace();

                        using (new EditorGUILayout.VerticalScope())
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.Space(5);
                                if(variables != null)
                                    variables.MenuMode = (RIS.RISMode)GUILayout.Toolbar((int)variables.MenuMode, TabStyle.GetTabToggles<RIS.RISMode>(), TabStyle.TabButtonStyle, TabStyle.TabButtonSize);
                                GUILayout.FlexibleSpace();
                            }
                            var skin = new GUIStyle(GUI.skin.box);
                            skin.margin.top = 0;
                            using (var scope = new EditorGUILayout.VerticalScope(skin))
                            {
                                tabScopeRect = scope.rect;
                                if (variables != null)
                                    DrawTab(showingVerticalScroll && showingVerticalScrollOld);
                                else
                                    tabItems[RIS.RISMode.Simple].DrawTab(ref variables, ref settings, position, showingVerticalScroll);
                            }
                            int memoryNow = 0;
                            int memoryUseFromScript = 0;

                            if (avatarRoot != null && variables != null)
                            {
                                var expressionParameter = avatarRoot.GetExpressionParameters(RIS.AutoGeneratedFolderPath + variables.FolderID + "/", false);
                                if(expressionParameter != null)
                                {
                                    var paramsTemp = new List<VRCExpressionParameters.Parameter>();
                                    foreach(var groupIndex in Enumerable.Range(0, variables.Groups.Count))
                                    {
                                        var group = variables.Groups[groupIndex];
                                        if(variables.MenuMode == RIS.RISMode.Simple && group.ExclusiveMode == 1)
                                            paramsTemp.Add(new VRCExpressionParameters.Parameter() { name = $"RIS-G{groupIndex}RESET", valueType = VRCExpressionParameters.ValueType.Bool });
                                        foreach (var propIndex in Enumerable.Range(0, group.Props.Count))
                                            paramsTemp.Add(new VRCExpressionParameters.Parameter() { name = $"RIS-G{groupIndex}P{propIndex}", valueType = VRCExpressionParameters.ValueType.Bool });
                                    }
                                    var arr = paramsTemp.ToArray();
                                    memoryNow = expressionParameter.CalculateMemoryCount(arr, variables.OptimizeParams, "RIS", true);
                                    memoryAdded = expressionParameter.CalculateMemoryCount(arr, variables.OptimizeParams, "RIS");
                                    memoryUseFromScript = paramsTemp.Sum(n => VRCExpressionParameters.TypeCost(n.valueType));
                                }
                            }
                            EditorGUILayoutExtra.CostViewer(memoryNow, memoryAdded, memoryUseFromScript, RISMessageStrings.Strings.str_UsedMemory, RISMessageStrings.Strings.str_RemainMemory, RIS.CountBarStyleL, RIS.CountBarStyleR);
                        }

                        EditorGUILayoutExtra.SeparatorWithSpace();
                        string[] errors = new string[0];
                        if (variables != null && tabItems.ContainsKey(variables.MenuMode))
                            tabItems[variables.MenuMode].CheckErrors(variables);
                        var memoryOver = false;
                        if (variables != null)
                        {
                            variables.WriteDefaults = EditorGUILayout.Toggle(RISMessageStrings.Strings.str_WriteDefaults, variables.WriteDefaults);
                            UnityEditor.Animations.AnimatorController fxLayer = null;
                            if (!rootIsNull) fxLayer = avatarRoot.GetFXLayer(RIS.AutoGeneratedFolderPath + variables.FolderID + "/", false);
                            variables.OptimizeParams = EditorGUILayout.Toggle(RISMessageStrings.Strings.str_OptimizeParameter, variables.OptimizeParams);
                            variables.ApplyEnabled = EditorGUILayout.Toggle(RISMessageStrings.Strings.str_ApplyDefaults, variables.ApplyEnabled);
                            EditorGUILayoutExtra.Space();
                            var showFXWarning = fxLayer != null && !fxLayer.ValidateWriteDefaults(variables.WriteDefaults);
                            var showParamInfo = !variables.OptimizeParams;
                            memoryOver = memoryAdded > VRCExpressionParameters.MAX_PARAMETER_COST;
                            if (showFXWarning || showParamInfo || errors.Any() || memoryOver)
                                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                                {
                                    if(memoryOver)
                                    {
                                        EditorGUILayout.HelpBox(string.Format(RISMessageStrings.Strings.str_ErrorCostOver, VRCExpressionParameters.MAX_PARAMETER_COST), MessageType.Error);
                                    }
                                    foreach (var error in errors)
                                    {
                                        EditorGUILayout.HelpBox(error, MessageType.Error);
                                    }
                                    if (showFXWarning)
                                    {
                                        EditorGUILayout.HelpBox(RISMessageStrings.Strings.str_WarnWriteDefaults, MessageType.Warning);
                                    }
                                    if (showParamInfo)
                                    {
                                        EditorGUILayout.HelpBox(RISMessageStrings.Strings.str_InfoOptimizeParameter, MessageType.Info);
                                    }
                                }
                        }
                        else
                        {
                            EditorGUILayout.Toggle(RISMessageStrings.Strings.str_WriteDefaults, false);
                            EditorGUILayout.Toggle(RISMessageStrings.Strings.str_OptimizeParameter, true);
                            EditorGUILayout.Toggle(RISMessageStrings.Strings.str_ApplyDefaults, true);
                        }

                        EditorGUILayoutExtra.SeparatorWithSpace();
                        using(new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUI.DisabledGroupScope(errors.Any() || memoryOver))
                            {
                                if (GUILayout.Button(RISMessageStrings.Strings.str_Apply, new GUIStyle("ButtonLeft")))
                                {
                                    SaveSettings();
                                    RISGimmickBuilder.ApplyToAvatar(variables);
                                }
                            }
                            if (GUILayout.Button(RISMessageStrings.Strings.str_Remove, new GUIStyle("ButtonRight")))
                            {
                                SaveSettings();
                                RISGimmickBuilder.RemoveFromAvatar(variables);
                            }
                        }
                    }
                    EditorGUILayoutExtra.SeparatorWithSpace();
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        EditorGUILayout.HelpBox(RISMessageStrings.Strings.str_RISMessage, MessageType.None);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayoutExtra.LinkLabel(RISMessageStrings.Strings.str_ManualLink, Color.blue, new Vector2(), 0, RIS.ManualUrl);
                            GUILayout.FlexibleSpace();
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayoutExtra.LinkLabel(RISMessageStrings.Strings.str_TwitterLink, Color.blue, new Vector2(), 0, RIS.TwitterUrl);
                            GUILayout.FlexibleSpace();
                        }
                    }
                    var donators = RISDonatorListUpdater.GetDonators();
                    if(!string.IsNullOrWhiteSpace(donators))
                    {
                        GUILayout.Space(10);
                        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                        {
                            EditorGUILayout.LabelField(RISMessageStrings.Strings.str_Donator, new GUIStyle("ProjectBrowserHeaderBgTop"), GUILayout.ExpandWidth(true));
                            EditorGUILayout.LabelField(donators, RIS.DonatorLabelStyle, GUILayout.ExpandWidth(true));
                        }
                    }
                    beforeWidth = width;
                    GUILayout.FlexibleSpace();
                }
            }
        }
        private void RestoreSettings()
        {
            variables = new RISVariables();
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

            //InitializeGroupList();
        }
        private void SaveSettings()
        {
            Debug.Assert(variables != null);
            EditorExtSettingsTool.SaveSettings<RISSettings>(avatarRoot, RIS.SettingsNameV4, variables);
        }
        private void DrawTab(bool showingVerticalScroll)
        {
            if(tabItems.ContainsKey(variables.MenuMode))
                tabItems[variables.MenuMode].DrawTab(ref variables, ref settings, position, showingVerticalScroll);
        }
    }
}