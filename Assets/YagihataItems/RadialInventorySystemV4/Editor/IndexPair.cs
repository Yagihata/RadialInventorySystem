#if RISV4_JSON
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YagihataItems.RadialInventorySystemV4
{
    [JsonObject]
    public class IndexPair
    {
        [JsonProperty] public int GroupIndex;
        [JsonProperty] public int PropIndex;
    }
}
#endif