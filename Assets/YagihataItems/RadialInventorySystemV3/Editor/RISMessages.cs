using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV3
    
{
    public class RISMessageStrings
    {
        [Serializable]
        public class LanguageStrings
        {
            public string str_TargetAvatar = "Target Avatar";
            public string str_WriteDefaults = "Write Defaults";
            public string str_OptimizeParameter = "Optimize Params";
            public string str_ApplyEnabled = "Apply Defaults";
            public string str_ErrorCostOver = "Total memory is need to keep under {0}.";
            public string str_WarnWriteDefaults = "WriteDefaults are not unified in the FX layer.\n" +
                "It will work as-is, but there may be a bug in switching facial expressions.\n" +
                "If the error message does not disappear after toggling the WriteDefaults checkbox, please check other avatar gimmicks you are using.";
            public string str_InfoOptimizeParameter = "Parameter optimization is disabled. \n" +
                "If you want empty or duplicate parameters to be removed automatically, please use parameter optimization.";
            public string str_Apply = "Apply to avatar.";
            public string str_Remove = "Remove from avatar.";
            public string str_RISMessage = "Thank you very much for downloading the Radial Inventory System!\n" +
                "If you don't know how to use it, please try to use it after reading the instructions from the link below.\n" +
                "If you have any bugs or requests for additional features, please let us know via Twitter DM.";
            public string str_ManualLink = "Radial Inventory System V3 Docs.(JP)";
            public string str_TwitterLink = "Twitter : @Yagihata4x";
            public string str_Donator = "Donators! (Titles omitted)";
            public string str_Menu = "Menu";
            public string str_Group = "Group";
            public string str_Settings = "Settings";
            public string str_Name = "Name";
            public string str_Icon = "Icon";
            public string str_Exclusive = "Exc.";
            public string str_Mode = "Mode";
            public string str_Base = "Base";
            public string str_Prop = "Prop";
            public string str_Object = "Object";
            public string str_ShowDefault = "Show Defaults";
            public string str_LocalOnly = "Local Only";
            public string str_SaveParam = "Save Status";
            public string str_DefaultStatus = "Show Default";
            public string str_LocalFunc = "Local Only";
            public string str_AdditionalAnimation = "Additional Animations";
            public string str_OnEnable = "OnEnable";
            public string str_OnDisable = "OnDisable";
            public string str_OffTimer = "Off Timer";
            public string str_Sec = "Sec.";
            public string str_Material = "Material";
            public string str_List = "List";
            public string str_NewVersion = "A new version is available.";
            public string str_UsedMemory = "Used Memory";
            public string str_RemainMemory = "Remain Memory";
            public string str_NeedOnce = ": Need at least one.";
            public string str_MissingProp = ": Missing any props.";
            public string str_OverProp = ": Prop Count is over.(Max:{0})";
            public string str_GroupsProp = "'s Prop'";
            public string str_MissingObject = ": Missing target object.";
            public string str_MissingObjectOrAnim = ": Missing target objects or animations.";
            public string str_ExclusiveType0 = "Disable(Without Reset)";
            public string str_ExclusiveType1 = "Disable(With Reset)";
            public string str_ExclusiveType2 = "Enable";
            public string str_Unselected = "(Unselected)";
        }
        private static bool Initialize = false;
        public static string[] ExclusiveType = new string[3];
        private static LanguageStrings languageStrings = null;
        public static LanguageStrings Strings
        {
            get
            {
                if (!Initialize)
                {
                    EditorInitialize();
                    Initialize = true;
                }
                return languageStrings;
            }
            private set { languageStrings = value; }
        }
        [InitializeOnLoadMethod]
        static void EditorInitialize()
        {
            TextAsset languageJSON = AssetDatabase.LoadAssetAtPath<TextAsset>(RISV3.WorkFolderPath + "Languages/" + CultureInfo.CurrentCulture.Name + ".json");
            languageStrings = new LanguageStrings();
            EditorJsonUtility.FromJsonOverwrite(languageJSON.text, languageStrings);
            ExclusiveType[0] = languageStrings.str_ExclusiveType0;
            ExclusiveType[1] = languageStrings.str_ExclusiveType1;
            ExclusiveType[2] = languageStrings.str_ExclusiveType2;

        }
    }
}
