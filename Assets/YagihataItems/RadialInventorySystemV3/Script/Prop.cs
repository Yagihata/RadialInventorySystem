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
        public bool IsDefaultEnabled = false;
        [SerializeField]
        public bool LocalOnly = false;
        [SerializeField]
        public Texture2D PropIcon = null;
        [SerializeField]
        public AnimationClip EnableAnimation = null;
        [SerializeField]
        public AnimationClip DisableAnimation = null;
        [SerializeField]
        public string PropName = "";
        [SerializeField]
        public List<GameObject> TargetObjects = new List<GameObject>();
        [SerializeField]
        public RISV3.PropGroup PropGroupType = RISV3.PropGroup.None;
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
            obj.LocalOnly = this.LocalOnly;
            obj.EnableAnimation = this.EnableAnimation;
            obj.DisableAnimation = this.DisableAnimation;
            obj.PropName = this.PropName;
            obj.TargetObjects.AddRange(this.TargetObjects);
            obj.PropGroupType = this.PropGroupType;
            return obj;
        }

        public override bool Equals(object obj)
        {
            return obj is Prop prop &&
                   base.Equals(obj) &&
                   name == prop.name &&
                   hideFlags == prop.hideFlags &&
                   EqualityComparer<GameObject>.Default.Equals(TargetObject, prop.TargetObject) &&
                   IsDefaultEnabled == prop.IsDefaultEnabled &&
                   LocalOnly == prop.LocalOnly &&
                   EqualityComparer<Texture2D>.Default.Equals(PropIcon, prop.PropIcon) &&
                   EqualityComparer<AnimationClip>.Default.Equals(EnableAnimation, prop.EnableAnimation) &&
                   EqualityComparer<AnimationClip>.Default.Equals(DisableAnimation, prop.DisableAnimation) &&
                   PropName == prop.PropName &&
                   EqualityComparer<List<GameObject>>.Default.Equals(TargetObjects, prop.TargetObjects) &&
                   PropGroupType == prop.PropGroupType;
        }

        public override int GetHashCode()
        {
            int hashCode = -1096752834;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + hideFlags.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<GameObject>.Default.GetHashCode(TargetObject);
            hashCode = hashCode * -1521134295 + IsDefaultEnabled.GetHashCode();
            hashCode = hashCode * -1521134295 + LocalOnly.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(PropIcon);
            hashCode = hashCode * -1521134295 + EqualityComparer<AnimationClip>.Default.GetHashCode(EnableAnimation);
            hashCode = hashCode * -1521134295 + EqualityComparer<AnimationClip>.Default.GetHashCode(DisableAnimation);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PropName);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<GameObject>>.Default.GetHashCode(TargetObjects);
            hashCode = hashCode * -1521134295 + PropGroupType.GetHashCode();
            return hashCode;
        }
    }
}
