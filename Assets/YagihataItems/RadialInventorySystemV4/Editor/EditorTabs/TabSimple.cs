using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using YagihataItems.YagiUtils;

namespace YagihataItems.RadialInventorySystemV4
{
    public class TabSimple : EditorTab
    {
        private ReorderableList propsReorderableList;
        private Texture2D redTexture = null;
        private Texture2D blueTexture = null;
        private Texture2D bodyTexture;
        private Texture2D circleTexture;
        private const float zoomLevel = 0.6f;
        private GUIStyle buttonStyle = null;
        private Rect[] buttonRects = null;
        private short targetId = -1;
        private string[] targetName = { "頭のアイテム", "手のアイテム", "足のアイテム", "胸のアイテム", "腰のアイテム", "アクセサリー1", "アクセサリー2", "衣装セット" };
        private Vector2 targetPosition = Vector2.zero;
        private GUIStyle targetNameLabelStyle = null;
        private Prop targetProp;

        public override string[] CheckErrors(RISVariables variables)
        {
            return new string[0];
        }

        public override void DrawTab(ref RISVariables variables, ref RISSettings settings, Rect position, bool showingVerticalScroll)
        {
            var cellWidth = position.width / 4f - 15f;
            if (propsReorderableList == null)
                InitializePropList(null, variables, settings);
            if (bodyTexture == null)
                bodyTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/ris_body.png");
            if (circleTexture == null)
                circleTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/circle-menu@3x.png");
            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle();
                buttonStyle.normal.background = null;
            }
            if (targetNameLabelStyle == null)
            {
                targetNameLabelStyle = new GUIStyle("ProjectBrowserHeaderBgTop");
                targetNameLabelStyle.fontSize = (int)(EditorGUIUtility.singleLineHeight * 1.3f);
                targetNameLabelStyle.alignment = TextAnchor.MiddleLeft;
                targetNameLabelStyle.fixedHeight = EditorGUIUtility.singleLineHeight * 2f;
            }

            using (new GUILayout.VerticalScope())
            {
                var rect = GUILayoutUtility.GetRect(0, 0);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Box(bodyTexture, GUILayout.Width((int)(200 * zoomLevel)), GUILayout.Height((int)(480 * zoomLevel)));
                    using (new GUILayout.VerticalScope())
                    {
                        var infoAreaRect = GUILayoutUtility.GetRect(0, 0);
                        if (targetId != -1)
                        {
                        }
                        var height = (int)(EditorGUIUtility.singleLineHeight * 2f);
                        GUILayout.Label(" " + (targetId != -1 ? targetName[targetId] : "アイテム未選択"), targetNameLabelStyle, GUILayout.Height(height));
                        targetPosition.x = infoAreaRect.x;
                        targetPosition.y = infoAreaRect.y + (height / 2f) + 2;
                        if (targetId != -1 && variables.Groups.Count <= targetId)
                        {
                            var count = targetId - variables.Groups.Count;
                            for (int i = 0; i <= count; i++)
                                variables.Groups.Add(ScriptableObject.CreateInstance<PropGroup>());
                            InitializePropList(variables.Groups[targetId], variables, settings);
                        }
                        using (new GUILayout.HorizontalScope())
                        {
                            using (var scope = new EditorGUILayout.HorizontalScope())
                            {
                                var scopeWidth = cellWidth + 20;
                                var propListHeight = propsReorderableList.GetHeight();
                                using (new GUILayout.HorizontalScope(GUILayout.Height(propListHeight), GUILayout.Width(scopeWidth)))
                                {
                                    var propIndex = propsReorderableList.index;
                                    propsReorderableList.DoList(new Rect(scope.rect.x, scope.rect.y, scopeWidth, propListHeight));
                                    if (targetId != -1 && variables.Groups.Count > targetId)
                                        variables.Groups[targetId].Props = (List<Prop>)propsReorderableList.list;
                                    GUILayout.Space(0);
                                }
                            }
                            if (targetId != -1 && variables.Groups.Count > targetId && variables.Groups[targetId].Props.Count > propsReorderableList.index && propsReorderableList.index >= 0)
                                targetProp = variables.Groups[targetId].Props[propsReorderableList.index];
                            else
                                targetProp = null;

                            using (new EditorGUI.DisabledGroupScope(targetProp == null))
                            {
                                using (new GUILayout.VerticalScope())
                                {
                                    if (targetId != 7)
                                    {
                                        using (new GUILayout.HorizontalScope())
                                        {
                                            using (new GUILayout.VerticalScope(GUILayout.Width(1)))
                                            {
                                                GUILayout.Label(RISMessageStrings.Strings.str_Icon);
                                                if (targetProp != null)
                                                    targetProp.PropIcon = (Texture2D)EditorGUILayout.ObjectField(targetProp.PropIcon, typeof(Texture2D), false, GUILayout.Width(EditorGUIUtility.singleLineHeight * 3), GUILayout.Height(EditorGUIUtility.singleLineHeight * 3));
                                                else
                                                    EditorGUILayout.ObjectField(null, typeof(Texture2D), false, GUILayout.Width(EditorGUIUtility.singleLineHeight * 3), GUILayout.Height(EditorGUIUtility.singleLineHeight * 3));
                                            }
                                            using (new GUILayout.VerticalScope())
                                            {
                                                GUILayout.Label(RISMessageStrings.Strings.str_Prop + RISMessageStrings.Strings.str_Name);
                                                if (targetProp != null)
                                                    targetProp.PropName = EditorGUILayout.TextField(targetProp.PropName);
                                                else
                                                    EditorGUILayout.TextField("");

                                                GUILayout.Label(RISMessageStrings.Strings.str_Object);
                                                if (targetProp != null)
                                                    targetProp.TargetObject = (GameObject)EditorGUILayout.ObjectField(targetProp.TargetObject, typeof(GameObject), true);
                                                else
                                                    EditorGUILayout.ObjectField(null, typeof(GameObject), true);
                                            }
                                        }
                                        GUILayout.Space(5);
                                        using (new GUILayout.HorizontalScope())
                                        {
                                            GUILayout.Label("アイテムの追従先");
                                            GUILayout.FlexibleSpace();
                                            var bone = RIS.BoneType.None;
                                            if (targetProp != null)
                                                bone = (RIS.BoneType)EditorGUILayout.EnumPopup("", RIS.BoneType.Head, GUILayout.MinWidth(1));
                                            else
                                                bone = (RIS.BoneType)EditorGUILayout.EnumPopup("", RIS.BoneType.Head, GUILayout.MinWidth(1));
                                        }
                                        GUILayout.Space(5);
                                        using (new GUILayout.HorizontalScope())
                                        {
                                            GUILayout.Label("はじめから出しておく");
                                            GUILayout.FlexibleSpace();
                                            if (targetProp != null)
                                                targetProp.IsDefaultEnabled = EditorGUILayout.Toggle(targetProp.IsDefaultEnabled, GUILayout.Width(20));
                                            else
                                                EditorGUILayout.Toggle(false, GUILayout.Width(20));
                                        }
                                    }
                                    else
                                    {
                                        using (new GUILayout.HorizontalScope())
                                        {
                                            using (new GUILayout.VerticalScope(GUILayout.Width(1)))
                                            {
                                                GUILayout.Label(RISMessageStrings.Strings.str_Icon);
                                                if (targetProp != null)
                                                    targetProp.PropIcon = (Texture2D)EditorGUILayout.ObjectField(targetProp.PropIcon, typeof(Texture2D), false, GUILayout.Width(EditorGUIUtility.singleLineHeight * 3), GUILayout.Height(EditorGUIUtility.singleLineHeight * 3));
                                                else
                                                    EditorGUILayout.ObjectField(null, typeof(Texture2D), false, GUILayout.Width(EditorGUIUtility.singleLineHeight * 3), GUILayout.Height(EditorGUIUtility.singleLineHeight * 3));
                                            }
                                            using (new GUILayout.VerticalScope())
                                            {
                                                GUILayout.Label("");
                                                GUILayout.Label(RISMessageStrings.Strings.str_Prop + RISMessageStrings.Strings.str_Name);
                                                if (targetProp != null)
                                                    targetProp.PropName = EditorGUILayout.TextField(targetProp.PropName);
                                                else
                                                    EditorGUILayout.TextField("");

                                                using (new GUILayout.HorizontalScope())
                                                {
                                                    GUILayout.Label("はじめから出しておく");
                                                    GUILayout.FlexibleSpace();
                                                    if (targetProp != null)
                                                        targetProp.IsDefaultEnabled = EditorGUILayout.Toggle(targetProp.IsDefaultEnabled, GUILayout.Width(20));
                                                    else
                                                        EditorGUILayout.Toggle(false, GUILayout.Width(20));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (buttonRects == null) 
                {
                    buttonRects = new Rect[]
                    {
                        new Rect(rect.x + 7, rect.y + 20, 30, 30),
                        new Rect(rect.x + 76, rect.y + 130, 30, 30),
                        new Rect(rect.x + 18, rect.y + 220, 30, 30),
                        new Rect(rect.x + 12, rect.y + 90, 30, 30),
                        new Rect(rect.x + 12, rect.y + 160, 30, 30),
                        new Rect(rect.x + 80, rect.y + 180, 30, 30),
                        new Rect(rect.x + 80, rect.y + 210, 30, 30),
                        new Rect(rect.x + 80, rect.y + 250, 30, 30)
                    };
                }
                else
                {
                    buttonRects[0].x = rect.x + 7;
                    buttonRects[1].x = rect.x + 76;
                    buttonRects[2].x = rect.x + 18;
                    buttonRects[3].x = rect.x + 12;
                    buttonRects[4].x = rect.x + 12;
                    buttonRects[5].x = rect.x + 80;
                    buttonRects[6].x = rect.x + 80;
                    buttonRects[7].x = rect.x + 80;

                    buttonRects[0].y = rect.y + 20;
                    buttonRects[1].y = rect.y + 130;
                    buttonRects[2].y = rect.y + 220;
                    buttonRects[3].y = rect.y + 90;
                    buttonRects[4].y = rect.y + 160;
                    buttonRects[5].y = rect.y + 180;
                    buttonRects[6].y = rect.y + 210;
                    buttonRects[7].y = rect.y + 250;
                }
                if (GUI.Button(buttonRects[0], circleTexture, buttonStyle))
                    SetTargetID(0, variables, settings);
                if (GUI.Button(buttonRects[1], circleTexture, buttonStyle))
                    SetTargetID(1, variables, settings);
                if (GUI.Button(buttonRects[2], circleTexture, buttonStyle))
                    SetTargetID(2, variables, settings);
                if (GUI.Button(buttonRects[3], circleTexture, buttonStyle))
                    SetTargetID(3, variables, settings);
                if (GUI.Button(buttonRects[4], circleTexture, buttonStyle))
                    SetTargetID(4, variables, settings);
                if (GUI.Button(buttonRects[5], circleTexture, buttonStyle))
                    SetTargetID(5, variables, settings);
                if (GUI.Button(buttonRects[6], circleTexture, buttonStyle))
                    SetTargetID(6, variables, settings);
                if (GUI.Button(buttonRects[7], circleTexture, buttonStyle))
                    SetTargetID(7, variables, settings);
                if (targetId != -1 && targetPosition != Vector2.zero)
                {
                    var midPosition = targetPosition;
                    midPosition.x += -20;
                    DrawOutlinedLine(buttonRects[targetId].center, midPosition, targetPosition);
                }
            }
        }
        private void SetTargetID(short id, RISVariables variables, RISSettings settings)
        {
            if (targetId == id)
            {
                targetId = -1;
                InitializePropList(null, variables, settings);
            }
            else
            {
                targetId = id;
                if (variables != null && targetId < variables.Groups.Count)
                    InitializePropList(variables.Groups[targetId], variables, settings);
            }
        }
        private void DrawOutlinedLine(Vector2 dest, Vector2 mid, Vector2 src)
        {
            Drawing.DrawLine(dest, mid, Color.cyan, 5, true);
            Drawing.DrawLine(mid, src, Color.cyan, 5, true);
            Drawing.DrawLine(dest, mid, Color.white, 3, false, 2f);
            Drawing.DrawLine(mid, src, Color.white, 3, false, 2f);
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
                    EditorGUI.LabelField(rect, "アイテム" + RISMessageStrings.Strings.str_List + $": {props.Count}");
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
                    var propName = !string.IsNullOrEmpty(rawPropName) ? rawPropName : $"空";
                    var style = GUI.skin.label;
                    style.fontSize = (int)(rect.height / 1.75f);
                    GUI.Label(rect, propName, style);
                    style.fontSize = 0;
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
                    return EditorGUIUtility.singleLineHeight * 1.55f;

                }

            };
        }
    }
}
