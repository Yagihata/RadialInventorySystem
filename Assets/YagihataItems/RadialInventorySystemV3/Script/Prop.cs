using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV3
{
    [Serializable]
    public class Prop : ScriptableObject, ICloneable
    {
        [SerializeField]
        public GameObject TargetObject = null;
        [SerializeField]
        public string TargetObjectPath = "";
        [SerializeField]
        public bool IsDefaultEnabled = false;
        [SerializeField]
        public bool LocalOnly = false;
        [SerializeField]
        public Texture2D PropIcon = null;
        [SerializeField]
        public AnimationClip CustomAnim = null;
        [SerializeField]
        public string PropName = "";
        public Prop(GameObject gameObject)
        {
            TargetObject = gameObject;
            IsDefaultEnabled = false;
            PropIcon = null;
        }
        public Prop()
        {
            TargetObject = null;
            IsDefaultEnabled = false;
            PropIcon = null;
        }
        public string GetPropName()
        {
            if (string.IsNullOrEmpty(PropName))
                return TargetObject != null ? TargetObject.name : "";
            else
                return PropName;
        }
        public object Clone()
        {
            var obj = new Prop(TargetObject);
            obj.IsDefaultEnabled = this.IsDefaultEnabled;
            obj.PropIcon = this.PropIcon;
            obj.TargetObjectPath = this.TargetObjectPath;
            obj.LocalOnly = this.LocalOnly;
            obj.CustomAnim = this.CustomAnim;
            obj.PropName = this.PropName;
            return obj;
        }

        public override bool Equals(object obj)
        {
            return obj is Prop prop &&
                   base.Equals(obj) &&
                   name == prop.name &&
                   hideFlags == prop.hideFlags &&
                   EqualityComparer<GameObject>.Default.Equals(TargetObject, prop.TargetObject) &&
                   TargetObjectPath == prop.TargetObjectPath &&
                   IsDefaultEnabled == prop.IsDefaultEnabled &&
                   LocalOnly == prop.LocalOnly &&
                   EqualityComparer<Texture2D>.Default.Equals(PropIcon, prop.PropIcon) &&
                   EqualityComparer<AnimationClip>.Default.Equals(CustomAnim, prop.CustomAnim) &&
                   PropName == prop.PropName;
        }

        public override int GetHashCode()
        {
            int hashCode = 57088559;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + hideFlags.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<GameObject>.Default.GetHashCode(TargetObject);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TargetObjectPath);
            hashCode = hashCode * -1521134295 + IsDefaultEnabled.GetHashCode();
            hashCode = hashCode * -1521134295 + LocalOnly.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(PropIcon);
            hashCode = hashCode * -1521134295 + EqualityComparer<AnimationClip>.Default.GetHashCode(CustomAnim);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PropName);
            return hashCode;
        }
    }
}
