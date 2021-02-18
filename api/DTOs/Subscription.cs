using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs
{
    public class Subscription
    {
        public string adddress { get; set; }
        [JsonProperty("private")] 
        [System.Text.Json.Serialization.JsonPropertyName("private")]
        public bool? _private { get; set; }
    }
}
