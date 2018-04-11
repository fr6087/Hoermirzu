using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassLibrary.model;

namespace FBKWebService
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Klassennamen "Service1" sowohl im Code als auch in der SVC- und der Konfigurationsdatei ändern.
    // HINWEIS: Wählen Sie zum Starten des WCF-Testclients zum Testen dieses Diensts Service1.svc oder Service1.svc.cs im Projektmappen-Explorer aus, und starten Sie das Debuggen.
    public class Service1 : IService1
    {
        Factory Factory = new Factory(true, "en");

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
            List<Section> myList = Factory.GetImplementation().GetAllElements(username, password);
            return myList;
        }
        

        public string GetForm(string username, string password)
        {
            return Factory.GetImplementation().GetForm(username,password);
        }

        public string Login(string username, string password)
        {
            return Factory.GetImplementation().Login(username, password);
        }

        public string JsonForBot(string username, string password, short sectionindex)
        {
            return Factory.GetImplementation().JsonForBot(username, password, sectionindex);
        }

        public Task<string> TalkToTheBotAsync(string message)
        {
            Task<string> BotsAnswer= Factory.GetImplementation().TalkToTheBotAsync(message);
            return BotsAnswer;
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
            List<string> Elements = Factory.GetImplementation().GetHeadings(username, password, xpath);
            return Elements;
        }
        /// <summary>
        /// outputs csv lines for dropdown information
        /// </summary>
        /// <param name="list_id">the id of the list to access</param>
        /// <param name="filter">if the dropdownlines should contain this string, exclude matching lines with it from the return</param>
        /// <returns></returns>
        public string[] GetCSVDropdownString(string list_id, string filter)
        {
            string[] lines = Factory.GetImplementation().GetCSVDropdownString(list_id, filter);
            return lines;
        }
    }
}
