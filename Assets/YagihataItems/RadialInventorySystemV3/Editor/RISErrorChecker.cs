using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YagihataItems.RadialInventorySystemV3
{
    public static class RISErrorChecker
    {
        public static string[] CheckErrors(RISVariables variables)
        {
            if (variables == null)
                return new string[] { };

            var errors = new List<string>();
            var prefixText = variables.MenuMode == RISV3.RISMode.Simple ? RISMessageStrings.Strings.str_Group : RISMessageStrings.Strings.str_Menu;
            if (variables.Groups.Count == 0)
                errors.Add(prefixText + RISMessageStrings.Strings.str_NeedOnce);

            foreach (var group in variables.Groups)
            {
                var groupName = group.GroupName;
                if (string.IsNullOrEmpty(groupName))
                    groupName = "Group" + variables.Groups.IndexOf(group);

                if (!group.Props.Any())
                    errors.Add($"{prefixText}[{groupName}]" + RISMessageStrings.Strings.str_MissingProp);

                var maxPropsCount = 8;
                if (variables.MenuMode == RISV3.RISMode.Simple && group.ExclusiveMode == 1)
                    maxPropsCount = 7;
                else if (variables.MenuMode == RISV3.RISMode.Advanced && group.BaseMenu != null)
                    maxPropsCount = 8 - group.BaseMenu.controls.Count;
                if (group.Props.Count > maxPropsCount)
                    errors.Add($"{prefixText}[{groupName}]" + string.Format(RISMessageStrings.Strings.str_OverProp, maxPropsCount));

                foreach (var prop in group.Props)
                {
                    var propName = prop.PropName;
                    if (string.IsNullOrEmpty(propName))
                        propName = "Prop" + group.Props.IndexOf(prop);

                    if (variables.MenuMode == RISV3.RISMode.Simple)
                    {
                        if (prop.TargetObject == null)
                            errors.Add($"{prefixText}[{groupName}]" + RISMessageStrings.Strings.str_GroupsProp + $"[{propName}]" + RISMessageStrings.Strings.str_MissingObject);
                    }
                    else if (variables.MenuMode == RISV3.RISMode.Advanced)
                    {
                        if (!prop.TargetObjects.Any(n => n != null) && prop.DisableAnimation == null && prop.EnableAnimation == null)
                            errors.Add($"{prefixText}[{groupName}]" + RISMessageStrings.Strings.str_GroupsProp + $"[{propName}]" + RISMessageStrings.Strings.str_MissingObjectOrAnim);
                    }
                }
            }


            return errors.ToArray();
        }
    }
}
