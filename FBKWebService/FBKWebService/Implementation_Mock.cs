using ClassLibrary.model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FBKWebService
{
    public class Implementation_Mock : ServiceClass { 
        
        private string language;

        public Implementation_Mock()
        {
            language = "de";

        }
        public Implementation_Mock(string lang)
        {
            language = lang;
        }

        /// <summary>
        /// calls the Formularbaukasten website and converts its html nodes to Section and Elements objects. 
        /// </summary>
        /// <param name="username">The username for Fromularbaukasten website</param>
        /// <param name="password">The password for Formularbaukasten website</param>
        /// <returns>A list of Sections. Each Section contains a list of Elements</returns>
        public override List<Section> GetAllElements(string username, string password)
        {
            string pathFile = System.AppDomain.CurrentDomain.BaseDirectory + "\\Sections\\Sections_" + language + ".json";
            string s = string.Empty;
            bool start = false;
            List<Section> sections = new List<Section>();
            using (StreamReader sr = new StreamReader(pathFile))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {

                    if (line.Contains("SECTION"))
                    {
                        if (start)//use a start flag to ignore the first line containing a section
                        {

                            Section section = ConvertStringToSection(s);
                            sections.Add(section);
                            s = "";
                        }
                        s = "";
                        start = true;
                    }
                    s = (s == string.Empty) ? line : s + "\r\n" + line;
                }
            }
            return sections;
        }

        /// <summary>
        /// processes the json string contained in the sectionsList into a Section from the PortableClassLibrary model
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        private Section ConvertStringToSection(string section)
        {
            string innerSection = section.TrimEnd(',');
            innerSection.Trim();
            innerSection = innerSection.Substring(17).Trim(); //this will fail dramatically whenever the form has a Section that is named not in the format SECTION123
            if (!innerSection.StartsWith("{"))
            {
                throw new FileNotFoundException("no file found that supported the format. Be mindful of whitespaces in the json format. Each Section object must have a name SECTION followed closely by 3 digits. This label has to be followed by ' : {'. this label line should have a length of 19 signs");
            }
            var sectionObject = JsonConvert.DeserializeObject<Section>(innerSection);
            return sectionObject;
        }

        /// <summary>
        /// reads in a dummy html DummyForm.html form that is a copy of the real Formularbaukasten website. 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public override string GetForm(string username, string password)
        {
            Login(username, password);
            string filepath = System.AppDomain.CurrentDomain.BaseDirectory + "DummyForm_" + language + ".html";
            Debug.WriteLine("GetForm. Opening filepath: " + filepath);
            return readFile(filepath); //but this doesn't read all the content of the html form

        }
        /// <summary>
        /// reads a text file (in this case json file)
        /// </summary>
        /// <param name="pathFile"></param>
        /// <returns>the content of the file</returns>
        private string readFile(string pathFile)
        {
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
        /// mocks the user's login by returning a string that does not start with 'error'.
        /// The parameters of the overridden method are not used, so you can use every string sequence as username and password that you like.
        /// </summary>
        /// <param name="userName">Anything as a string. may be null as well.</param>
        /// <param name="password">Anything as a string. may be null as well.</param>
        /// <returns></returns>
        public override string Login(string userName, string password)
        {
            return "user is logged in.";
        }
        
    }
}