using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FBKWebService.Model
{
    public class Property
    {
        [JsonProperty(PropertyName = "Before")]
        public List<Message> Before { get; set; }
        [JsonProperty(PropertyName = "type")]
        public List<string> Type { get; set; }
        [JsonProperty(PropertyName = "enum")]
        public string[] Enum { get; set; }
        [JsonProperty(PropertyName = "Pattern")]
        public string Pattern { get; set; }
        [JsonProperty(PropertyName = "minimum")]
        public string Minimum { get; set; }
        [JsonProperty(PropertyName = "maximum")]
        public string Maximum { get; set; }
        [JsonProperty(PropertyName = "After")]
        public string After { get; set; }
        [JsonProperty(PropertyName = "Prompt")]
        public Message Prompt { get; set; }
    }
}