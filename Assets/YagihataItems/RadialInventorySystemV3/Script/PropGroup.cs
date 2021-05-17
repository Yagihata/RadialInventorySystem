using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV3
{
    [Serializable]
    public class PropGroup : ScriptableObject, ICloneable
    {
        [SerializeField]
        public List<Prop> Props = new List<Prop>();
        [SerializeField]
        public string GroupName = "";
        [SerializeField]
        public Texture2D GroupIcon = null;
        [SerializeField]
        public bool ExclusiveMode = false;
        public PropGroup()
        {
            Props = new List<Prop>();
            GroupName = "";
            GroupIcon = null;
            ExclusiveMode = false;
        }
        public object Clone()
        {
            var obj = new PropGroup();
            obj.GroupName = this.GroupName;
            obj.GroupIcon = this.GroupIcon;
            obj.ExclusiveMode = this.ExclusiveMode;
            obj.Props.AddRange(this.Props.Select(n => (Prop)n.Clone()));
            return obj;
        }

        public override bool Equals(object obj)
        {
            return obj is PropGroup group &&
                   base.Equals(obj) &&
                   name == group.name &&
                   hideFlags == group.hideFlags &&
                   EqualityComparer<List<Prop>>.Default.Equals(Props, group.Props) &&
                   GroupName == group.GroupName &&
                   EqualityComparer<Texture2D>.Default.Equals(GroupIcon, group.GroupIcon) &&
                   ExclusiveMode == group.ExclusiveMode;
        }

        public override int GetHashCode()
        {
            int hashCode = -1548633703;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + hideFlags.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Prop>>.Default.GetHashCode(Props);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GroupName);
            hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(GroupIcon);
            hashCode = hashCode * -1521134295 + ExclusiveMode.GetHashCode();
            return hashCode;
        }
    }
}
