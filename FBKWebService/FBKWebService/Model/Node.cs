using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FBKWebService.Model
{
    public class Node : Dictionary<string, Message>
    {
        public Node(string key, Message message)
        {
            this.Add(key, message);
        }
    }
}