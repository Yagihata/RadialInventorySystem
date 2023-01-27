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
                {
                    var path = AssetDatabase.GUIDToAssetPath("1cdb98c668245314ea279110b5eb6e1d");
                    boxIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                return boxIcon;
            }
        }
        public static Texture2D GroupIcon
        {
            get
            {
                if (groupIcon == null)
                {
                    var path = AssetDatabase.GUIDToAssetPath("0ba359225e947f64e9bd71e0f38a8daf");
                    groupIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                return groupIcon;
            }
        }
        public static Texture2D ReloadIcon
        {
            get
            {
                if (reloadIcon == null)
                {
                    var path = AssetDatabase.GUIDToAssetPath("b83ac1a9fd1bd2742a721e5d3b5dac55");
                    reloadIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                return reloadIcon;
            }
        }
        public static Texture2D MenuIcon
        {
            get
            {
                if (menuIcon == null)
                {
                    var path = AssetDatabase.GUIDToAssetPath("c29e85901754f4948b5b9c020fd5b196");
                    menuIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                return menuIcon;
            }
        }
        public static Texture2D BodyTexture
        {
            get
            {
                if (bodyTexture == null)
                {
                    var path = AssetDatabase.GUIDToAssetPath("13dc598c65a895e43bd081b9cd84518f");
                    bodyTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                return bodyTexture;
            }
        }
        public static Texture2D CircleIcon
        {
            get
            {
                if (circleIcon == null)
                {
                    var path = AssetDatabase.GUIDToAssetPath("593ca3e79491a4f468a9d30597e69b50");
                    circleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                return circleIcon;
            }
        }
        public static Texture2D Accessory1Icon
        {
            get
            {
                if (accessory1Icon == null)
                {
                    var path = AssetDatabase.GUIDToAssetPath("4014542cc57aa334bbf40dcfc89997cb");
                    accessory1Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                return accessory1Icon;
            }
        }
        public static Texture2D Accessory2Icon
        {
            get
            {
                if (accessory2Icon == null)
                {
                    var path = AssetDatabase.GUIDToAssetPath("b105fcc399c8d2142b6330ebcc3cc615");
                    accessory2Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                return accessory2Icon;
            }
        }
        public static Texture2D DresserIcon
        {
            get
            {
                if (dresserIcon == null)
                {
                    var path = AssetDatabase.GUIDToAssetPath("202ec6db02c0fe2468ec6d08daa0adc8");
                    dresserIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
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
                {
                    var path = AssetDatabase.GUIDToAssetPath("ba6d797b16bb608448b90f0dbdcf30cc");
                    headerTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                return headerTexture;
            }
        }
    }
}
