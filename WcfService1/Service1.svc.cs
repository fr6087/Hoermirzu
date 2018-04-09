using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ClassLibrary.model;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Linq;
using System.ServiceModel.Activation;
using WcfService1.Properties;

namespace WcfService1
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Klassennamen "Service1" sowohl im Code als 
    //auch in der SVC- und der Konfigurationsdatei ändern.
    // HINWEIS: Wählen Sie zum Starten des WCF-Testclients zum Testen dieses Diensts Service1.svc oder Service1.svc.cs im 
    //Projektmappen-Explorer aus, und starten Sie das Debuggen.
    /// <summary>
    /// the class implements the webservice parsing the form. ther is a debug and no-debug mode. If Debug is true, the form is build from a json text fine. If not it's
    /// delivered fróm a Selenium Browser <see cref="WebsiteCrawler"/>
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)] // see https://www.youtube.com/watch?v=_-dcOHCZ1cM
    public class Service1: IService1
    {
        /// <summary>
        /// Factory returning the mock document (param true) or the real document (param false). runs in translation mode if language is diffrent from German
        /// and the real document is accessed
        /// </summary>
        private Service1Factory factory = new Service1Factory(true, "de");

        /// <summary>
        /// real login to the web form
        /// </summary>
        /// <param name="_userName"></param>
        /// <param name="_password"></param>
        /// <returns></returns>
        public string Login(string _userName, string _password)
        {
            return factory.GetImplementation().Login(_userName, _password);
        }
        /// <summary>
        /// is telling the headless browser to navigate to a specific domain in order to login and view the form
        /// </summary>
        /// <param name="_username"></param>
        /// <param name="_password"></param>
        /// <returns></returns>
        public string GetForm(string _username, string _password)
        {
            return factory.GetImplementation().GetForm(_username, _password);
        }
        /// <summary>
        /// returns a list of all labels on the web form. Those can then be translated into textBoxes by the App
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public List<string> GetInputs(string username, string password)
        {
            return factory.GetImplementation().GetInputs(username,password);
        }

        /// <summary>
        /// Is returning headings slightly modified, cuts the number from the heading e.g
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public List<string> GetSpecificElements(string username, string password, string xpath)
        {
            return factory.GetImplementation().GetHeadings(username,password, xpath);
        }

        

        /// <summary>
        /// could help at converting the html to xml. xml was originally used in the formstore.
        /// </summary>
        /// <param name="doc">the html document to be converted</param>
        /// <returns></returns>
        private string ConvertHTMLDocToXMLDoc(HtmlDocument doc)
        {
            doc.OptionOutputAsXml = true;
            System.IO.StringWriter sw = new System.IO.StringWriter();
            System.Xml.XmlTextWriter xw = new System.Xml.XmlTextWriter(sw);
            doc.Save(xw);
            return sw.ToString();
        }

        /// <summary>
        /// parses the website form and returns a json string simulating radio buttons, check-boxec, input fields, headings, dopdown lists, date pickers and labels. This is specialized
        /// for these control types and other types will not be recognized
        /// this is not working correctly. In the inputs and headings region old values are kept
        ///don't use this in debug mude, returns only partial output
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="isDebug"></param>
        /// <returns>returns Json string containing structure for a Section</returns>
        public List<Section> GetAllElements(string username, string password)
        {
             return factory.GetImplementation().GetAllElements(username, password);
        }

        /// <summary>
        /// serializes a Dictionary to a json string with indented formatting
        /// not needed with current factory pattern
        /// </summary>
        /// <param name="jsonDictionary">the</param>
        /// <returns></returns>
        public string SerializeToJson(string username, string password)
        {
            var jsonDictionary = GetAllElements(username,password);
            return JsonConvert.SerializeObject(jsonDictionary,
                            Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
        }
        
        /// <summary>
        /// reads the json sections from a file. This is used in debugging if the web form is not available
        /// </summary>
        /// <returns></returns>
        public string ReadJasonFromFile(string filename)//"Sections.json"
        {
            return factory.GetImplementation().ReadFile(filename);
        }


        /*toBeDone: post Section back to online form
        public bool UploadSection(Section section)
        {
            return factory.GetImplementation().UploadSection(section);
        }*/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list_id"></param>
        /// <param name="filter">if the dropdownlines should contain this string, exclude matching lines with it from the return</param>
        /// <returns></returns>
        public string[] GetCSVDropdownString(string list_id, string filter)
        {
            return factory.GetImplementation().GetCSVDropdownString(list_id, filter);
        }

        public Task<string> TalkToTheBotAsync(string message)
        {
            return factory.GetImplementation().TalkToTheBotAsync(message);
        }

        public bool UploadSection(Section section)
        {
            return factory.GetImplementation().UploadSection(section);
        }

        public string JsonForBot(string username, string password, short sectionindex)
        {
            var sections = GetAllElements(username, password); //toDo: filter disabled input fields
            if (sections == null | sectionindex < 1 | sectionindex > sections.Count)
                return "error. No sections or invalid sectionindex. index is valid from 1 to sections.count";
            var SecondSection = sections[sectionindex].InputsAndHeadings;
            var AllElemsExceptHeadings = SecondSection.Where(Element => Element.Type.Equals("label") && Element.Subelems!=null && Element.Subelems.First().Disabled==null);

            //set up outer json
            BotJson OuterHeaders = new BotJson();
            OuterHeaders.References = new List<string>() { "LuisBot.dll"};
            OuterHeaders.Imports = new List<string>() { "LuisBot.Resource"};
            OuterHeaders.Type = "object";

            Node jsonProperty = new Node("NotUnderstood", new Message("Patterns", new List<string>() { Resources.NotUnderstood, Resources.NotUnderstood2 }));
            OuterHeaders.Templates = jsonProperty;

            //var NotUnderstood = new JsonProperty(PropertyName ="NotUnderstood", DefaultValue = OuterHeaders.Templates = NotUnderstood;
            
            //set up properties the bot is going to ask for
            Dictionary<string, Property> PropertyDictionary = new Dictionary<string, Property>();

            List<string> RequiredList = new List<string>();
           
            foreach (var Element in AllElemsExceptHeadings)
            {
                Property Property = new Property();
                Property.Type = new List<string>() { "string", "null"};
                Property.Prompt = new Message("Patterns", new List<string>() { Resources.AskStringValue+"?" });

                //if there is a dropdown tell the bot about the available options
                if (Element.Subelems != null)
                {
                    Element FirstSubElem = Element.Subelems.First();
                    //check if element is dropdown list
                    if (FirstSubElem.List_id != null)
                    {//this might fail if the html node label contains more than one child

                        //filter the first array entry from the array
                        string[] SourceList = GetCSVDropdownString(FirstSubElem.List_id,"----");
                        
                        //throw away the first array entry that contains the dropdownlist's name
                        int Length = SourceList.Length - 1;
                        string[] DestinationToCopyTo = new string[Length];//cut the first line
                        Array.Copy(SourceList, 1, DestinationToCopyTo, 0, Length);
                        Property.Enum = DestinationToCopyTo;

                        //set a diffrent Prompt type that shows all available options to the user
                        Property.Prompt = new Message("Patterns", new List<string>() { Resources.AskEnumValue });
                    }
                    //if it is no dropdown, check if we have a validating pattern
                    else if (FirstSubElem.Pattern != null)
                    {
                        Property.Pattern = FirstSubElem.Pattern;
                    }
                        
                }

                //set the key for the property. the problem here is that only one word entries are allowed as keys -> botframework parses them internally to variable names
                string Name = Element.Text;
                if (Name.EndsWith("*"))
                {
                    Name = Name.Substring(0,Name.Length-1);//cut that last sign for aestetic purposes
                    RequiredList.Add(Name);
                }
                var words = "";
                if (Name.Contains(" "))
                {
                    words = Name.Substring(Name.IndexOf(" "));//last part of string
                    Name = Name.Substring(0, Name.IndexOf(" ")).Trim();
                    Property.Prompt = new Message("Patterns", new List<string>() { Resources.AskStringValue+words+ " ?" });
          
                }

                //if there is already avariable called Name in the Dictionary, then add a Number to the end of the name
                //and change the prompt so that it doesn't use the key for input
                if (PropertyDictionary.Keys.Contains(Name))
                {
                    Property.Prompt = new Message("Patterns", new List<string>() { Resources.AskStringVariableless+Name + words + " ?" });
                    PropertyDictionary.Add(Name+PropertyDictionary.Count, Property);
                }
                else
                {
                    PropertyDictionary.Add(Name, Property);
                }
                
            }
            List<Message> Greeting = new List<Message>();
            Greeting.Add(new Message("Message", new List<string> { Resources.WelcomeBot+sectionindex }));
            PropertyDictionary.First().Value.Before = Greeting;

            OuterHeaders.Properties = PropertyDictionary;
            OuterHeaders.Required = RequiredList;

            return JsonConvert.SerializeObject(OuterHeaders,
                           Formatting.Indented,
                           new JsonSerializerSettings
                           {
                               NullValueHandling = NullValueHandling.Ignore
                           });

        }
        
    }
    
}
