using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV4
{
    public abstract class EditorTab
    {
        public abstract void DrawTab(ref RISVariables variables, ref RISSettings settings, Rect position, bool showingVerticalScroll);
        public abstract string[] CheckErrors(RISVariables variables);
    }
}
