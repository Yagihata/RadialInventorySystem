using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace YagihataItems.RadialInventorySystemV4
{
    public static class AnimatorControllerUtils
    {
        public static AnimatorControllerLayer AddAnimatorControllerLayer(this AnimatorController controller, string name)
        {
            var layer = new AnimatorControllerLayer();
            layer.defaultWeight = 1f;
            layer.blendingMode = AnimatorLayerBlendingMode.Override;
            layer.name = name;
            layer.stateMachine = new AnimatorStateMachine();
            controller.AddLayer(layer);
            if (!AssetDatabase.IsSubAsset(layer.stateMachine))
                AssetDatabase.AddObjectToAsset(layer.stateMachine, controller);
            layer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
            layer.stateMachine.name = name;
            return layer;
        }
        public static AnimatorControllerLayer FindAnimatorControllerLayer(this AnimatorController controller, string name)
        {
            var layer = controller.layers.FirstOrDefault(n => n.name == name);
            if (layer != null && layer.stateMachine == null)
                layer.stateMachine = new AnimatorStateMachine();
            return layer;
        }
        public static AnimatorStateTransition MakeAnyStateTransition(this AnimatorStateMachine stateMachine, AnimatorState destState)
        {
            AnimatorStateTransition transition;
            transition = stateMachine.AddAnyStateTransition(destState);
            transition.hasExitTime = false;
            transition.exitTime = 0;
            transition.hasFixedDuration = true;
            transition.duration = 0;
            transition.offset = 0;
            transition.canTransitionToSelf = false;
            return transition;
        }
        public static AnimatorStateTransition MakeTransition(this AnimatorState srcState, AnimatorState destState, string name = "")
        {
            AnimatorStateTransition transition;
            transition = srcState.AddTransition(destState);
            transition.hasExitTime = false;
            transition.exitTime = 0;
            transition.hasFixedDuration = true;
            transition.duration = 0;
            transition.offset = 0;
            transition.name = name;
            return transition;
        }
        public static AnimatorCondition[] CreateSingleCondition(this AnimatorStateTransition transition, AnimatorConditionMode mode, string param, float threshold, bool localOnly, bool localMode)
        {
            AnimatorCondition[] condition = null;
            if (!localOnly)
                condition = new AnimatorCondition[]
                {
                    new AnimatorCondition(){ mode = mode, parameter = param, threshold = threshold }
                };
            else
                condition = new AnimatorCondition[]
                {
                    new AnimatorCondition(){ mode = mode, parameter = param, threshold = threshold },
                    new AnimatorCondition(){ mode = localMode ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, parameter = "IsLocal", threshold = 0 }
                };
            transition.conditions = condition;
            return condition;
        }
        public static AnimatorCondition[] AddCondition(this AnimatorStateTransition transition, AnimatorConditionMode mode, string param, float threshold, bool localOnly, bool localMode)
        {
            AnimatorCondition[] condition = transition.conditions;
            Array.Resize(ref condition, transition.conditions.Length + (localOnly ? 2 : 1));

            if (!localOnly)
                condition[condition.Length - 1] = new AnimatorCondition() { mode = mode, parameter = param, threshold = threshold };
            else
            {
                condition[condition.Length - 2] = new AnimatorCondition() { mode = mode, parameter = param, threshold = threshold };
                condition[condition.Length - 1] = new AnimatorCondition() { mode = localMode ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, parameter = "IsLocal", threshold = 0 };
            }
            transition.conditions = condition;
            return condition;
        }
        public static void Clear(this AnimatorStateMachine stateMachine)
        {
            var states = stateMachine.states.Clone() as ChildAnimatorState[];
            foreach (var v in states)
            {
                stateMachine.RemoveState(v.state);
            }
        }
        public static void TryRemoveParameter(this AnimatorController controller, string name)
        {
            var param = controller.parameters.FirstOrDefault(n => n.name == name);
            if (param != null)
                controller.RemoveParameter(param);
        }
        public static void TryRemoveLayer(this AnimatorController controller, string name)
        {
            var index = controller.layers.ToList().FindIndex(n => n.name == name);
            if (index >= 0)
                controller.RemoveLayer(index);
        }
    }
}
