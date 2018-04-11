using ClassLibrary.model;
using FBKWebService.Properties;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace FBKWebService
{
    public abstract class ServiceClass
    {
        /// <summary>
        /// connects to the bot using DirectLine
        /// </summary>
       private CortanaDirectLineClient botClient = new CortanaDirectLineClient();

        /// <summary>
        /// simulates manual login of the user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public abstract string Login(string username, string password);

        /// <summary>
        /// returns the Formularbaukasten form
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public abstract string GetForm(string username, string password);

        /// <summary>
        /// returns a list of all elements prensent in the Formularbaukasten form.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public abstract List<Section> GetAllElements(string username, string password);

        /// <summary>
        /// sends a message to the bot via DirectLine. Returns its answer.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<string> TalkToTheBotAsync(string message)
        {
            await botClient.ConnectAsync();
            var answer = await botClient.TalkToTheBotAsync(message);
            return answer;
        }


        /// <summary>
        /// retrieves a list of h present in the form. Debugging method for Cortana.
        /// this function doesn't run in mock mode because of missing html tags
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public List<string> GetHeadings(string username, string password, string xpath)
        {
            Login(username, password);
            string form = GetForm(username, password);
            List<string> headings = new List<string>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(form);
            var nodecollection = doc.DocumentNode.SelectNodes(xpath);// or self::h4 or self::h5 or self::span[@ng-bind='::text.label']] ";
            foreach (var node in nodecollection)
            {
                //this separates the headings into string and Number part
                string headingWithOrdinalNumber = HttpUtility.HtmlDecode(node.InnerHtml);
                string headingWithoutOrdinalNumber = "";
                Regex regex = new Regex("^(\\d+\\.)((.)*)"); //^search Matches at the beginning [d]of type digit that have a following dot after (.) no or * many signs into two groups ()()
                if (regex.IsMatch(headingWithOrdinalNumber))
                {
                    MatchCollection matches = regex.Matches(node.InnerHtml);
                    string number = "";
                    foreach (Match match in matches)
                    {
                        // Debug.WriteLine("Number:        {0}", match.Groups[1].Value);
                        number = match.Groups[1].Value;
                        //Debug.WriteLine("heading: {0}", match.Groups[2].Value);
                        //Debug.WriteLine("");
                    }

                    headingWithoutOrdinalNumber = headingWithOrdinalNumber.Substring(number.Length).Trim();

                    headings.Add(headingWithoutOrdinalNumber);
                }
                //this means, that a heading without leading digits is not recognized as a heading
            }
            return headings;
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
            Login(username, password);
            string form = GetForm(username, password);
            List<string> headings = new List<string>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(form);
            var xpath = "//*[self::span[@ng-bind='::text.label']] ";
            foreach (var node in doc.DocumentNode.SelectNodes(xpath))
            {
                string inputLabel = HttpUtility.HtmlDecode(node.InnerHtml);

                if (!string.IsNullOrWhiteSpace(inputLabel))
                    headings.Add(node.InnerHtml);

            }
            return headings;
        }
        public string JsonForBot(string username, string password, short sectionindex)
        {
            var sections = GetAllElements(username, password); //toDo: filter disabled input fields
            if (sections == null | sectionindex < 1 | sectionindex > sections.Count)
                return "error. No sections or invalid sectionindex. index is valid from 1 to sections.count";
            var SecondSection = sections[sectionindex].InputsAndHeadings;
            var AllElemsExceptHeadings = SecondSection.Where(Element => Element.Type.Equals("label") && Element.Subelems != null && Element.Subelems.First().Disabled == null);

            //set up outer json
            BotJson OuterHeaders = new BotJson();
            OuterHeaders.References = new List<string>() { "LuisBot.dll" };
            OuterHeaders.Imports = new List<string>() { "LuisBot.Resource" };
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
                Property.Type = new List<string>() { "string", "null" };
                Property.Prompt = new Message("Patterns", new List<string>() { Resources.AskStringValue + "?" });

                //if there is a dropdown tell the bot about the available options
                if (Element.Subelems != null)
                {
                    Element FirstSubElem = Element.Subelems.First();
                    //check if element is dropdown list
                    if (FirstSubElem.List_id != null)
                    {//this might fail if the html node label contains more than one child

                        //filter the first array entry from the array
                        string[] SourceList = GetCSVDropdownString(FirstSubElem.List_id, "----");

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
                    Name = Name.Substring(0, Name.Length - 1);//cut that last sign for aestetic purposes
                    RequiredList.Add(Name);
                }
                var words = "";
                if (Name.Contains(" "))
                {
                    words = Name.Substring(Name.IndexOf(" "));//last part of string
                    Name = Name.Substring(0, Name.IndexOf(" ")).Trim();
                    Property.Prompt = new Message("Patterns", new List<string>() { Resources.AskStringValue + words + " ?" });

                }

                //if there is already avariable called Name in the Dictionary, then add a Number to the end of the name
                //and change the prompt so that it doesn't use the key for input
                if (PropertyDictionary.Keys.Contains(Name))
                {
                    Property.Prompt = new Message("Patterns", new List<string>() { Resources.AskStringVariableless + Name + words + " ?" });
                    PropertyDictionary.Add(Name + PropertyDictionary.Count, Property);
                }
                else
                {
                    PropertyDictionary.Add(Name, Property);
                }

            }
            List<Message> Greeting = new List<Message>();
            Greeting.Add(new Message("Message", new List<string> { Resources.WelcomeBot + sectionindex }));
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
        /// <summary>
        /// helps reading in a file in the current base directory. If the file is in a sub directory, that path must be added to the filename, e.g. \Dropdowns\nbank.csv
        /// </summary>
        /// <param name="filename">e.g. README.txt</param>
        /// <returns></returns>
        public string ReadFile(string filename)
        {
            string pathFile = System.AppDomain.CurrentDomain.BaseDirectory + filename;
            string s = string.Empty;

            using (StreamReader sr = new StreamReader(pathFile))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    s = (s == string.Empty) ? line : s + "\r\n" + line;
                }
            }
            return s;
        }
        /// <summary>
        /// searches in the directory Dropdowns for the first file containing a list_id text.
        /// </summary>
        /// <param name="list_id"></param>
        /// <returns>lines in the file that contain the search text. If no file contained the searchtext, a string 'error. list_id not found in dropdown menus'</returns>
        public string[] GetCSVDropdownString(string list_id, string filter)
        {
            string[] s = { "error. list_id not found in dropdown menus" };
            string filepath = System.AppDomain.CurrentDomain.BaseDirectory + "Dropdowns";
            DirectoryInfo d = new DirectoryInfo(filepath);
            foreach (var file in d.GetFiles())
            {
                string path = file.FullName;
                if (File.ReadAllText(path).Contains(list_id))
                {

                    return ReadLinesContainingString(list_id, file, filter).Replace("\r\n", "\n").Split('\n');
                }
            }
            return s;
        }

        /// <summary>
        /// retrieves the dropdown menu lines for a specific dropdown menu.
        /// Assumes that the csv lists are tab separated and that the last column contains the value of interest for the dropdown menu.
        /// </summary>
        /// <param name="list_id"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private string ReadLinesContainingString(string list_id, FileInfo file, string filter)
        {
            string s = file.Name;
            using (StreamReader sr = file.OpenText())
            {
                String line;
                sr.ReadLine();//overgo first line that contains the column names
                while ((line = sr.ReadLine()) != null)
                {

                    if (line.Contains(list_id))
                    {
                        int index = line.LastIndexOf('\t') + 1;
                        string listentry = line.Substring(index).Trim(';').Trim('"').Trim();
                        if (string.IsNullOrWhiteSpace(filter))
                            filter = listentry + "b";

                        s = (listentry.Contains(filter)) ? s : s + "\r\n" + listentry;

                    }

                }
            }
            return s;
        }

    }
}