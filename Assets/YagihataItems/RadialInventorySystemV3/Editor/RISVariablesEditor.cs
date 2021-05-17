using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace YagihataItems.RadialInventorySystemV3
{
    [CustomEditor(typeof(RISSettings))]
    public class RISSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var settings = target as RISSettings;

            var avatarRoot = settings.AvatarRoot;
            settings.AvatarRoot = EditorGUILayout.ObjectField("Avatar Root", settings.AvatarRoot, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
            if (settings.AvatarRoot != avatarRoot)
                EditorUtility.SetDirty(settings);

            var writeDefaults = settings.WriteDefaults;
            settings.WriteDefaults = EditorGUILayout.Toggle("Write Defaults", settings.WriteDefaults);
            if (settings.WriteDefaults != writeDefaults)
                EditorUtility.SetDirty(settings);

            var folderID = settings.FolderID;
            settings.FolderID = EditorGUILayout.TextField("Folder ID", settings.FolderID);
            if (settings.FolderID != folderID)
                EditorUtility.SetDirty(settings);

            serializedObject.ApplyModifiedProperties();
        }
    }
}