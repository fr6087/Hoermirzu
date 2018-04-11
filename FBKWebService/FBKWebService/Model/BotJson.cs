using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FBKWebService.Model
{
    public class BotJson
    {
        [JsonProperty(PropertyName = "References")]
        public List<string> References { get; set; }
        [JsonProperty(PropertyName = "Imports")]
        public List<string> Imports { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "required")]
        public List<string> Required { get; set; }
        [JsonProperty(PropertyName = "Templates")]
        public Node Templates { get; set; }
        [JsonProperty(PropertyName = "properties")]
        public Dictionary<string, Property> Properties { get; set; }
    }
}