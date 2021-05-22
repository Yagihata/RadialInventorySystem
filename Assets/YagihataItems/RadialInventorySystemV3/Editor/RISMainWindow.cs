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
        private ReorderableList gameObjectsReorderableList = null;
        private ReorderableList gameObjectsDummyReorderableList = null;
        private bool showingVerticalScroll;
        private Rect tabScopeRect = new Rect();
        private Texture2D redTexture = null;
        private Texture2D blueTexture = null;
        [MenuItem("Radial Inventory/RISV3 Editor")]
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
                        headerTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RISV3.WorkFolderPath + "Textures/ris_logo.png");
                    var newerVersion = RISVersionChecker.GetNewerVersion();
                    var showingVerticalScrollOld = showingVerticalScroll;
                    if (verticalScope.rect.height != 0)
                        showingVerticalScroll = verticalScope.rect.height >= position.size.y;
                    var height = position.size.x / headerTexture.width * headerTexture.height;
                    if (height > headerTexture.height)
                        height = headerTexture.height;
                    var width = position.size.x - (showingVerticalScroll ? 22 : 8);
                    GUILayout.Space(2);
                    EditorGUILayoutExtra.HeaderWithVersionInfo(headerTexture, width == beforeWidth ? width: beforeWidth, height, newerVersion, RISV3.CurrentVersion, "ris", RISMessageStrings.Strings.str_NewVersion, RISV3.DownloadUrl);
                    
                    EditorGUILayoutExtra.Space();
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
                        InitializeGroupList();
                        InitializePropList(null);
                        InitializeGameObjectsList(true);
                    }
                    var rootIsNull = avatarRoot == null;
                    if (rootIsNull)
                    {
                        avatarRootBefore = null;
                    }
                    int memoryAdded = 0;
                    using (new EditorGUI.DisabledGroupScope(rootIsNull))
                    {
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
                                if(variables != null)
                                    variables.MenuMode = (RISV3.RISMode)GUILayout.Toolbar((int)variables.MenuMode, TabStyle.GetTabToggles<RISV3.RISMode>(), TabStyle.TabButtonStyle, TabStyle.TabButtonSize);
                                else
                                    GUILayout.Toolbar((int)RISV3.RISMode.Simple, TabStyle.GetTabToggles<RISV3.RISMode>(), TabStyle.TabButtonStyle, TabStyle.TabButtonSize);
                                GUILayout.FlexibleSpace();
                            }
                            var skin = new GUIStyle(GUI.skin.box);
                            skin.margin.top = 0;
                            using (var scope = new EditorGUILayout.VerticalScope(skin))
                            {
                                tabScopeRect = scope.rect;
                                DrawTab(showingVerticalScroll && showingVerticalScrollOld);
                            }
                            int memoryNow = 0;
                            int memoryUseFromScript = 0;

                            if (avatarRoot != null && variables != null)
                            {
                                var expressionParameter = avatarRoot.GetExpressionParameters(RISV3.AutoGeneratedFolderPath + variables.FolderID + "/", false);
                                if(expressionParameter != null)
                                {
                                    var paramsTemp = new List<VRCExpressionParameters.Parameter>();
                                    foreach(var groupIndex in Enumerable.Range(0, variables.Groups.Count))
                                    {
                                        var group = variables.Groups[groupIndex];
                                        if(variables.MenuMode == RISV3.RISMode.Simple && group.ExclusiveMode == 1)
                                            paramsTemp.Add(new VRCExpressionParameters.Parameter() { name = $"RISV3-G{groupIndex}RESET", valueType = VRCExpressionParameters.ValueType.Bool });
                                        foreach (var propIndex in Enumerable.Range(0, group.Props.Count))
                                            paramsTemp.Add(new VRCExpressionParameters.Parameter() { name = $"RISV3-G{groupIndex}P{propIndex}", valueType = VRCExpressionParameters.ValueType.Bool });
                                    }
                                    var arr = paramsTemp.ToArray();
                                    memoryNow = expressionParameter.CalculateMemoryCount(arr, variables.OptimizeParams, "RISV3", true);
                                    memoryAdded = expressionParameter.CalculateMemoryCount(arr, variables.OptimizeParams, "RISV3");
                                    memoryUseFromScript = paramsTemp.Sum(n => VRCExpressionParameters.TypeCost(n.valueType));
                                }
                            }
                            EditorGUILayoutExtra.CostViewer(memoryNow, memoryAdded, memoryUseFromScript, RISMessageStrings.Strings.str_UsedMemory, RISMessageStrings.Strings.str_RemainMemory, RISV3.CountBarStyleL, RISV3.CountBarStyleR);
                        }

                        EditorGUILayoutExtra.SeparatorWithSpace();
                        var errors = RISErrorChecker.CheckErrors(variables);
                        var memoryOver = false;
                        if (variables != null)
                        {
                            variables.WriteDefaults = EditorGUILayout.Toggle(RISMessageStrings.Strings.str_WriteDefaults, variables.WriteDefaults);
                            UnityEditor.Animations.AnimatorController fxLayer = null;
                            if (!rootIsNull) fxLayer = avatarRoot.GetFXLayer(RISV3.AutoGeneratedFolderPath + variables.FolderID + "/", false);
                            variables.OptimizeParams = EditorGUILayout.Toggle(RISMessageStrings.Strings.str_OptimizeParameter, variables.OptimizeParams);
                            variables.ApplyEnabled = EditorGUILayout.Toggle(RISMessageStrings.Strings.str_ShowDefault, variables.ApplyEnabled);
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
                            EditorGUILayout.Toggle(RISMessageStrings.Strings.str_ShowDefault, true);
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
                            EditorGUILayoutExtra.LinkLabel(RISMessageStrings.Strings.str_ManualLink, Color.blue, new Vector2(), 0, RISV3.ManualUrl);
                            GUILayout.FlexibleSpace();
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayoutExtra.LinkLabel(RISMessageStrings.Strings.str_TwitterLink, Color.blue, new Vector2(), 0, RISV3.TwitterUrl);
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
                            EditorGUILayout.LabelField(donators, RISV3.DonatorLabelStyle, GUILayout.ExpandWidth(true));
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
        private void DrawTab(bool showingVerticalScroll)
        {
            if (propGroupsReorderableList == null)
                InitializeGroupList();
            if (propsReorderableList == null)
                InitializePropList(null);
            if (gameObjectsDummyReorderableList == null)
                InitializeGameObjectsList(true);
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
                var advanceMode = (variables != null && variables.MenuMode == RISV3.RISMode.Advanced);
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
                            if (variables.MenuMode == RISV3.RISMode.Simple)
                            {
                                //variables.Groups[groupIndex].ExclusiveMode = EditorGUILayout.Toggle("排他モード", variables.Groups[groupIndex].ExclusiveMode);
                                variables.Groups[groupIndex].ExclusiveMode = EditorGUILayout.Popup(RISMessageStrings.Strings.str_Exclusive + RISMessageStrings.Strings.str_Mode, variables.Groups[groupIndex].ExclusiveMode, RISMessageStrings.ExclusiveType);
                            }
                            else
                                variables.Groups[groupIndex].BaseMenu = 
                                    (VRCExpressionsMenu)EditorGUILayout.ObjectField(RISMessageStrings.Strings.str_Base + RISMessageStrings.Strings.str_Menu, variables.Groups[groupIndex].BaseMenu, typeof(VRCExpressionsMenu), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            if (selectedGroupChangeFlag)
                                InitializePropList(variables.Groups[groupIndex]);
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
                            targetProp = (variables.Groups[groupIndex].Props.Count >= 1 && propsReorderableList.index >= 0) ?
                                variables.Groups[groupIndex].Props[propsReorderableList.index] : null;
                        }
                        else
                        {
                            EditorGUILayout.TextField(RISMessageStrings.Strings.str_Group + RISMessageStrings.Strings.str_Name, "");
                            EditorGUILayout.ObjectField(RISMessageStrings.Strings.str_Icon, null, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            if ((variables != null && variables.MenuMode == RISV3.RISMode.Simple) || variables == null)
                                EditorGUILayout.Popup(RISMessageStrings.Strings.str_Exclusive + RISMessageStrings.Strings.str_Mode, 0, RISMessageStrings.ExclusiveType);
                            else
                                EditorGUILayout.ObjectField(RISMessageStrings.Strings.str_Base + RISMessageStrings.Strings.str_Menu, null, typeof(VRCExpressionsMenu), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
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
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(cellWidth + 30 - (showingVerticalScroll ? 14 : 0))))
                    {
                        GUIStyle headerStyle = new GUIStyle("HeaderLabel");
                        headerStyle.margin = new RectOffset(5, 5, 20, 20);
                        EditorGUILayout.LabelField(RISMessageStrings.Strings.str_Prop+ RISMessageStrings.Strings.str_Settings, new GUIStyle("ProjectBrowserHeaderBgTop"), GUILayout.ExpandWidth(true));
                        GUILayout.Space(3);
                        if (!advanceMode)
                        {
                            EditorGUILayout.LabelField(RISMessageStrings.Strings.str_Prop+ RISMessageStrings.Strings.str_Name);
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
                            EditorGUIUtility.labelWidth = 80;
                            if (targetProp != null)
                            {
                                targetProp.PropName = EditorGUILayout.TextField(RISMessageStrings.Strings.str_Prop + RISMessageStrings.Strings.str_Name, targetProp.PropName);
                                targetProp.PropIcon =
                                    (Texture2D)EditorGUILayout.ObjectField(RISMessageStrings.Strings.str_Icon, targetProp.PropIcon, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                                targetProp.PropGroupType = (RISV3.PropGroup)EditorGUILayout.EnumPopup(RISMessageStrings.Strings.str_Exclusive + RISMessageStrings.Strings.str_Group, targetProp.PropGroupType);
                                targetProp.IsDefaultEnabled = EditorGUILayout.Toggle(RISMessageStrings.Strings.str_DefaultStatus, targetProp.IsDefaultEnabled);
                                targetProp.LocalOnly = EditorGUILayout.Toggle(RISMessageStrings.Strings.str_LocalFunc, targetProp.LocalOnly);
                                GUILayout.Space(5);
                                EditorGUILayout.LabelField(RISMessageStrings.Strings.str_AdditionalAnimation);
                                targetProp.EnableAnimation = (AnimationClip)EditorGUILayout.ObjectField("　" + RISMessageStrings.Strings.str_OnEnable, targetProp.EnableAnimation, typeof(AnimationClip), false);
                                targetProp.DisableAnimation = (AnimationClip)EditorGUILayout.ObjectField("　" + RISMessageStrings.Strings.str_OnDisable, targetProp.DisableAnimation, typeof(AnimationClip), false);
                                GUILayout.Space(5);
                                targetProp.UseResetTimer = EditorGUILayout.Toggle(RISMessageStrings.Strings.str_OffTimer, targetProp.UseResetTimer);
                                if (targetProp.UseResetTimer)
                                    targetProp.ResetSecond = Mathf.Min(60, Mathf.Max(0, EditorGUILayout.FloatField("　" + RISMessageStrings.Strings.str_Sec, targetProp.ResetSecond)));
                                GUILayout.Space(5);
                                targetProp.MaterialOverride =
                                    (Material)EditorGUILayout.ObjectField(RISMessageStrings.Strings.str_Material, targetProp.MaterialOverride, typeof(Material), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                                GUILayout.Space(5);
                                if (gameObjectsReorderableList == null || propIsChanged)
                                    InitializeGameObjectsList(false, targetProp);
                                gameObjectsReorderableList.DoLayoutList();
                                targetProp.TargetObjects = (List<GameObject>)gameObjectsReorderableList.list;
                            }
                            else
                            {
                                EditorGUILayout.TextField(RISMessageStrings.Strings.str_Prop + RISMessageStrings.Strings.str_Name, "");
                                EditorGUILayout.ObjectField(RISMessageStrings.Strings.str_Icon, null, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                                EditorGUILayout.EnumPopup(RISMessageStrings.Strings.str_Exclusive + RISMessageStrings.Strings.str_Group, RISV3.PropGroup.None);
                                EditorGUILayout.Toggle(RISMessageStrings.Strings.str_DefaultStatus, false);
                                EditorGUILayout.Toggle(RISMessageStrings.Strings.str_LocalFunc, false);
                                GUILayout.Space(5);
                                EditorGUILayout.LabelField(RISMessageStrings.Strings.str_AdditionalAnimation);
                                EditorGUILayout.ObjectField("　" + RISMessageStrings.Strings.str_OnEnable, null, typeof(AnimationClip), false);
                                EditorGUILayout.ObjectField("　" + RISMessageStrings.Strings.str_OnDisable, null, typeof(AnimationClip), false);
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
                    if(variables != null && variables.MenuMode == RISV3.RISMode.Advanced)
                        EditorGUI.LabelField(rect, RISMessageStrings.Strings.str_Menu + RISMessageStrings.Strings.str_List + $": {groups.Count}");
                    else
                        EditorGUI.LabelField(rect, RISMessageStrings.Strings.str_Group + RISMessageStrings.Strings.str_List + $"一覧: {groups.Count}");
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
                        variables.Groups.Add(new PropGroup() { GroupName = "Group" + groups.Count });
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
                        if (variables.MenuMode == RISV3.RISMode.Simple && group.ExclusiveMode == 1)
                            maxPropCount = 7;
                        else if (variables.MenuMode == RISV3.RISMode.Advanced && group.BaseMenu != null)
                            maxPropCount = 8 - group.BaseMenu.controls.Count;
                    }
                    if (props.Count < maxPropCount && GUI.Button(position, ReorderableListStyle.AddContent, ReorderableListStyle.AddStyle))
                    {
                        if (settings != null)
                            Undo.RecordObject(settings, $"Add new Prop.");
                        group.Props.Add(new Prop());
                        if (settings != null)
                            EditorUtility.SetDirty(settings);
                    }
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (props.Count <= index)
                        return;
                    var rawPropName = props[index].GetPropName();
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
                            if (variables.MenuMode == RISV3.RISMode.Simple && group.ExclusiveMode == 1)
                                maxPropCount = 7;
                            else if (variables.MenuMode == RISV3.RISMode.Advanced && group.BaseMenu != null)
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
        private void InitializeGameObjectsList(bool dummyFlag, Prop prop = null)
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
                if (targetObject != null && !targetObject.IsChildOf(variables.AvatarRoot.gameObject))
                    targetObject = null;
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
    }
}