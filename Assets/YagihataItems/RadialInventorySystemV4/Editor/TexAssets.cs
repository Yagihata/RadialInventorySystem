using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV4
{
    public class TexAssets
    {
        private static Texture2D redTexture;
        private static Texture2D blueTexture;
        private static Texture2D boxIcon;
        private static Texture2D groupIcon;
        private static Texture2D reloadIcon;
        private static Texture2D menuIcon;
        private static Texture2D bodyTexture;
        private static Texture2D circleIcon;
        private static Texture2D accessory1Icon;
        private static Texture2D accessory2Icon;
        private static Texture2D dresserIcon;
        private static Texture2D adsTexture;
        private static Texture2D headerTexture;
        public static Texture2D RedTexture
        {
            get
            {
                if (redTexture == null)
                {
                    redTexture = new Texture2D(1, 1);
                    redTexture.SetPixel(0, 0, new Color(1f, 0.5f, 0.5f, 0.5f));
                    redTexture.Apply();
                }
                return redTexture;
            }
        }
        public static Texture2D BlueTexture
        {
            get
            {
                if (blueTexture == null)
                {
                    blueTexture = new Texture2D(1, 1);
                    blueTexture.SetPixel(0, 0, new Color(0.5f, 0.5f, 1f, 0.5f));
                    blueTexture.Apply();
                }
                return blueTexture;
            }
        }
        public static Texture2D BoxIcon
        {
            get
            {
                if (boxIcon == null)
                    boxIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/box_icon_v4.png");
                return boxIcon;
            }
        }
        public static Texture2D GroupIcon
        {
            get
            {
                if (groupIcon == null)
                    groupIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/group_icon_v4.png");
                return groupIcon;
            }
        }
        public static Texture2D ReloadIcon
        {
            get
            {
                if (reloadIcon == null)
                    reloadIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/reload_icon_v4.png");
                return reloadIcon;
            }
        }
        public static Texture2D MenuIcon
        {
            get
            {
                if (menuIcon == null)
                    menuIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/ris_icon_v4.png");
                return menuIcon;
            }
        }
        public static Texture2D BodyTexture
        {
            get
            {
                if (bodyTexture == null)
                    bodyTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/ris_body.png");
                return bodyTexture;
            }
        }
        public static Texture2D CircleIcon
        {
            get
            {
                if (circleIcon == null)
                    circleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/menu_icon.png");
                return circleIcon;
            }
        }
        public static Texture2D Accessory1Icon
        {
            get
            {
                if (accessory1Icon == null)
                    accessory1Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/accessory1_icon.png");
                return accessory1Icon;
            }
        }
        public static Texture2D Accessory2Icon
        {
            get
            {
                if (accessory2Icon == null)
                    accessory2Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/accessory2_icon.png");
                return accessory2Icon;
            }
        }
        public static Texture2D DresserIcon
        {
            get
            {
                if (dresserIcon == null)
                    dresserIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/dresser_icon.png");
                return dresserIcon;
            }
        }
        public static Texture2D AdsTexture
        {
            get
            {
                return adsTexture;
            }
            set
            {
                adsTexture = value;
            }
        }
        public static Texture2D HeaderTexture
        {
            get
            {
                if (headerTexture == null)
                    headerTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/ris_logo_v4.png");
                return headerTexture;
            }
        }
    }
}
