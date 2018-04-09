using ClassLibrary.model;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace FBKConnectorService
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Schnittstellennamen "IService1" sowohl im Code als auch in der Konfigurationsdatei ändern.
    /// <summary>
    /// contains operations of the Service1 client for parsing the html-webform. The methods that are abstract in this class have diffrent implementations in the
    /// child classes Implementation_Real and Implementation_Mock. All other functions are the same for these two modi and thus implemented here.
    /// </summary>
    public abstract class ServiceClass
    {
        /// <summary>
        /// connects to the bot using DirectLine
        /// </summary>
        private CortanaDirectLineClient botClient = new CortanaDirectLineClient();

        /// <summary>
        /// uploads a section to the Formmularbaukasten
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public abstract bool UploadSection(Section section);

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
                    
                    return ReadLinesContainingString(list_id, file, filter).Replace("\r\n","\n").Split('\n');
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
