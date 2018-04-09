
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ClassLibrary.model
{
    public class Form
    {
        /// <summary>
        /// flag to determine whether the page is loaded or not
        /// </summary>
        private bool loaded;
        /// <summary>
        /// Persist the loaded fields in memory for use in other parts of the application.
        /// </summary>
        private ObservableCollection<ClassLibrary.model.Section> sections;
        /// <summary>
        /// helps storing the sections collection. 
        /// </summary>
        public ObservableCollection<ClassLibrary.model.Section> Sections { get => sections; set => sections = value; }


        public Form()
        {
            loaded = false;
            Sections = new ObservableCollection<ClassLibrary.model.Section>();
        }




    }
    /// <summary>
    /// model class for creating an output json file that the bot understands.
    /// </summary>
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
    
    public class Node : Dictionary<string, Message>
    {

        public Node(string key, Message message)
        {
            this.Add(key, message);
        }
    }
    
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

    
    public class Message : Dictionary<string, object>
    {
        private List<string> Patterns;

        public Message(string key, List<string> value)
        {
            List<string> defaultList = value;
            Patterns = defaultList;
            this.Add(key, Patterns);
        }
    }
}
