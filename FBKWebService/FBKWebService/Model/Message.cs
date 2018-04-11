using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FBKWebService.Model
{
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