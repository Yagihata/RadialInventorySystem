#if RISV4_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace YagihataItems.RadialInventorySystemV4
{
    [JsonObject]
    public class Parameter
    {
        [JsonProperty] public bool IsRISParam { get; set; } = true;
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))] public VRCExpressionParameters.ValueType ValueType { get; set; } = VRCExpressionParameters.ValueType.Bool;
        [JsonProperty] public string Name { get; set; } = "";
        [JsonProperty] public float Value { get; set; }
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))] public RIS.ParameterTriggerType ParameterTriggerType { get; set; } = RIS.ParameterTriggerType.TurnON;
    }
}
#endif