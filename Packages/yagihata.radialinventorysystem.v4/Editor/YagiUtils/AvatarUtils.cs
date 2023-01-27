using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;
using static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;

namespace YagihataItems.RadialInventorySystemV4
{
    public static class AvatarUtils
    {
        public static bool ValidateWriteDefaults(this VRCAvatarDescriptor avatar, bool writeDefaults)
        {
            if (avatar.baseAnimationLayers != null)
            {
                return !avatar.baseAnimationLayers.Any(baseAnimationLayer =>
                {
                    if (baseAnimationLayer.animatorController == null)
                        return false;
                    var controller = (AnimatorController)baseAnimationLayer.animatorController;
                    return controller.layers
                        .Any(layer => layer.stateMachine != null && layer.stateMachine.states.Any(n => n.state.writeDefaultValues != writeDefaults));
                });
            }
            return true;
        }
        public static bool ValidateWriteDefaults(this AnimatorController controller, bool writeDefaults)
        {
            return !controller.layers.Any(layer => layer.stateMachine != null && layer.stateMachine.states.Any(n => n.state.writeDefaultValues != writeDefaults));
        }
        public static bool CheckParameterSpaces(this VRCExpressionParameters expressionParameters, string name, VRCExpressionParameters.ValueType valueType)
        {
            int currentCost = expressionParameters.CalcTotalCost();
            var hasParam = expressionParameters.parameters.Any(n => n.name == name && n.valueType == valueType);
            if (!hasParam)
                currentCost += VRCExpressionParameters.TypeCost(valueType);
            if (currentCost <= VRCExpressionParameters.MAX_PARAMETER_COST)
            {
                return true;
            }
            return false;
        }
        public static void OptimizeParameter(this VRCExpressionParameters expressionParameters)
        {
            var newParams = new List<VRCExpressionParameters.Parameter>();
            foreach(var v in expressionParameters.parameters)
            {
                if (!string.IsNullOrWhiteSpace(v.name) && !newParams.Any(n => n.name == v.name && n.valueType == v.valueType))
                    newParams.Add(v);
            }
            expressionParameters.parameters = newParams.ToArray();
        }
        public static void CalculateMemoryCount(this VRCExpressionParameters expressionParameters, out int costNow, out int costSum, List<VRCExpressionParameters.Parameter> additionalParams = null, bool optimize = false, string removablePrefix = "", bool compareOnly = false)
        {
            if (additionalParams == null && !optimize)
            {
                costNow = costSum = expressionParameters.CalcTotalCost();
                return;
            }

            var newParams = new List<VRCExpressionParameters.Parameter>();
            foreach (var v in expressionParameters.parameters)
            {
                var nullName = string.IsNullOrWhiteSpace(v.name);
                var duplicateName = newParams.Any(n => n.name == v.name && n.valueType == v.valueType);
                var prefixContain = v.name.StartsWith(removablePrefix);
                var additionalContain = additionalParams.Any(n => n.name == v.name);
                if (!optimize || (!nullName && !duplicateName &&
                    (!prefixContain || additionalContain)))
                    newParams.Add(v);
            }
            costNow = newParams.Sum(n => VRCExpressionParameters.TypeCost(n.valueType));
            if (!compareOnly && additionalParams != null && additionalParams.Any())
            {
                foreach (var v in additionalParams)
                {
                    if (!newParams.Any(n => n.name == v.name && n.valueType == v.valueType))
                        newParams.Add(v);
                }
            }
            costSum = newParams.Sum(n => VRCExpressionParameters.TypeCost(n.valueType));
        }
        public static int CalculateMemoryCount(this VRCExpressionParameters expressionParameters, VRCExpressionParameters.Parameter[] additionalParams = null, bool optimize = false, string removablePrefix = "", bool compareOnly = false)
        {
            if (additionalParams == null && !optimize)
                return expressionParameters.CalcTotalCost();

            var newParams = new List<VRCExpressionParameters.Parameter>();
            foreach (var v in expressionParameters.parameters)
            {
                var nullName = string.IsNullOrWhiteSpace(v.name);
                var duplicateName = newParams.Any(n => n.name == v.name && n.valueType == v.valueType);
                var prefixContain = v.name.StartsWith(removablePrefix);
                var additionalContain = additionalParams.Any(n => n.name == v.name);
                if (!optimize || (!nullName && !duplicateName &&
                    (!prefixContain || additionalContain)))
                    newParams.Add(v);
            }
            if (!compareOnly && additionalParams != null && additionalParams.Any())
            {
                foreach (var v in additionalParams)
                {
                    if (!newParams.Any(n => n.name == v.name && n.valueType == v.valueType))
                        newParams.Add(v);
                }
            }
            return newParams.Sum(n => VRCExpressionParameters.TypeCost(n.valueType));
        }
        public static AnimatorController GetFXLayer(this VRCAvatarDescriptor avatar, string createFolderDest, bool createNew = true)
        {
            AnimatorController controller = null;
            if (avatar.baseAnimationLayers != null && avatar.baseAnimationLayers.Length >= 5 && avatar.baseAnimationLayers[4].animatorController != null)
                controller = (AnimatorController)avatar.baseAnimationLayers[4].animatorController;
            else
            {
                if(createNew)
                {
                    var path = createFolderDest + "GeneratedFXLayer.controller";
                    YagiAPI.CreateFolderRecursively(createFolderDest);
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    if (asset != null)
                        AssetDatabase.DeleteAsset(path);

                    controller = AnimatorController.CreateAnimatorControllerAtPath(path);
                    if (avatar.baseAnimationLayers == null || avatar.baseAnimationLayers.Length < 5)
                    {
                        avatar.baseAnimationLayers = new CustomAnimLayer[]
                        {
                            new CustomAnimLayer(),
                            new CustomAnimLayer(),
                            new CustomAnimLayer(),
                            new CustomAnimLayer(){ isEnabled = true, animatorController = controller, type = AnimLayerType.FX }
                        };
                    }
                    else
                    {
                        avatar.baseAnimationLayers[4] = new CustomAnimLayer() { isEnabled = true, animatorController = controller, type = AnimLayerType.FX };
                    }
                }
            }
            return controller;
        }
        public static VRCExpressionParameters GetExpressionParameters(this VRCAvatarDescriptor avatar, string createFolderDest, bool createNew = true)
        {
            if (avatar.expressionParameters != null)
                return avatar.expressionParameters;
            else if (createNew)
            {
                var param = ScriptableObject.CreateInstance<VRCExpressionParameters>();
                param.parameters = new VRCExpressionParameters.Parameter[16];
                param.parameters[0] = new VRCExpressionParameters.Parameter() { name = "VRCEmote", valueType = VRCExpressionParameters.ValueType.Int };
                param.parameters[1] = new VRCExpressionParameters.Parameter() { name = "VRCFaceBlendH", valueType = VRCExpressionParameters.ValueType.Float };
                param.parameters[2] = new VRCExpressionParameters.Parameter() { name = "VRCFaceBlendV", valueType = VRCExpressionParameters.ValueType.Float };
                var path = createFolderDest + "GeneratedExpressionParameters.asset";
                YagiAPI.CreateFolderRecursively(createFolderDest);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (asset != null)
                    AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(param, path);
                avatar.expressionParameters = param;
                return param;
            }
            else
                return null;
        }
        public static void AddParameter(this VRCExpressionParameters expressionParameters, string name, VRCExpressionParameters.ValueType valueType, bool saveValue = false, float defaultValue = 0f )
        {
            var len = expressionParameters.parameters.Length;
            Array.Resize(ref expressionParameters.parameters, len + 1);
            expressionParameters.parameters[len] = new VRCExpressionParameters.Parameter() { name = name, valueType = valueType, saved = saveValue, defaultValue = defaultValue };
        }
        public static void TryRemoveParameter(this VRCExpressionParameters expressionParameters, string name)
        {
            expressionParameters.parameters = expressionParameters.parameters.Where(n => n.name != name).ToArray();
        }
        public static VRCExpressionParameters.Parameter FindParameter(this VRCExpressionParameters expressionParameters, string name, VRCExpressionParameters.ValueType valueType)
        {
            return expressionParameters.parameters.FirstOrDefault(n => n.name == name && n.valueType == valueType);
        }
        public static VRCAvatarParameterDriver AddParameterDriver(this AnimatorState state, string name, float value = 0)
        {
            var driver = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            driver.parameters = new List<VRC_AvatarParameterDriver.Parameter>
            {
                new VRC_AvatarParameterDriver.Parameter { name = name, value = value }
            };
            EditorUtility.SetDirty(driver);
            return driver;
        }
    }
}
