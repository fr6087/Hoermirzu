using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using ClassLibrary.model;
using System.Diagnostics;

namespace FBKConnectorService
{
    /// <summary>
    /// polls the online form Formularbaukasten in intervals via its property WebsiteCrawler. Once the WebsiteCrawler notices, that a specific attribute
    /// is present in the html code of that form, it assumes that the javascript functions in the form have finished their work. Then, this class has methods
    /// for retrieving all controls on that page via XPATH- filter and binding them to Portable Librara Classes' Section and Element.
    /// \note the xpath-expression is very closely dependant on the html structure. It will find controls only of specific type. For more detail see GetAllElements()
    /// </summary>
    public class Implementation_Real : ServiceClass
    {
        /// <summary>
        /// the headless browser used to connect to the form.
        /// </summary>
        private WebsiteCrawler wc = new WebsiteCrawler();

        private string language;

        public Implementation_Real(string lang)
        {
            language = lang;
        }

        public Implementation_Real()
        {
            language = "de";
        }

        /// <summary>
        /// calls the website Fromularbaukasten, retrieves the HTML-Form as a HTML-Document and translates the nodes found by the xpath expression
        /// to Elements grouped in Sections.
        /// </summary>
        /// <param name="username">Valid username for the Formularbaukasten form</param>
        /// <param name="password">Valid password for the Formularbaukasten form</param>
        /// <returns>List of Sections. In the case of the ESF_2 form usaually 19 Sections long.</returns>
        public override List<Section> GetAllElements(string username, string password)
        {
            List<Section> returnList = new List<Section>();
            string form = "";

            form = GetForm(username, password);
            Debug.WriteLine("called GetAllelements");
            if (form.StartsWith("error"))
            {
                return returnList;
            }


            bool DontAddToOutPut = false;
            Section section = null;
            List<Element> inputsAndHeadings = new List<Element>(); //this has to be casted to Array before adding it to the section

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(form);
            //span-Elements are for the form number nodes
            //h3-h5 are headings
            //input type radio is radiobutton
            //div elements contain sections
            //label nodes that do not contain text for a radiobutton. Some may have no text at all.
            string buttonfilter = "self::inn-button";
            string labelfilter = "self::label[not(contains(@ng-bind, '::text.label'))]";//all labelnodes that have no attribute called ng-bind with value xyz, i.e.  all labels except radio button labels
            var xpath = "//*[span[@class='antragsNummer'] | self::h3 | self::h4 | self::h5 | " + labelfilter + " | " + buttonfilter + " | self::inn-radiobutton |self::inn-checkbox   | self::inn-section]";
            var nodes = doc.DocumentNode.SelectNodes(xpath);
            foreach (var node in nodes)
            {
                Element element = new Element();
                element.Type = node.OriginalName;
                Debug.WriteLine(element.Type);

                if (element.Type.Equals("inn-radiobutton"))
                {
                    //text = node.GetAttributeValue("inn-radio-value", null);
                    HtmlNode textNode = node.SelectSingleNode("//label[@ng-bind='::text.label']");
                    if (textNode != null)
                    {

                        if (!string.IsNullOrWhiteSpace(textNode.InnerText.Replace("\r\n", "")))
                            element.Text = textNode.InnerText.Replace("\r\n", "");
                        textNode.Remove();//remove the text node from the document after evaluation
                    }
                    element.Group_name = node.GetAttributeValue("inn-radio-group", null);
                }
                else if (element.Type.Equals("inn-checkbox"))
                {
                    var parentnode = node.ParentNode;
                    element.Text = "check";
                    //check whether the div is contains a checkbox
                    var checkboxNode = node.SelectSingleNode("//input[@type='checkbox']");
                    if (checkboxNode != null)
                    {
                        HtmlNode textNode = checkboxNode.NextSibling;
                        if (textNode != null)
                        {
                            if (!string.IsNullOrWhiteSpace(textNode.InnerText.Replace("\r\n", "")))
                                element.Text = textNode.InnerText.Replace("\r\n", "");
                            textNode.Remove();//remove the text node from the document after evaluation
                        }
                    }
                }
                else if (element.Type.Equals("inn-button"))//this is buggy code
                {
                    element.Text = CleanStringFromHTML(node.InnerText);
                }
                else if (element.Type.StartsWith("h") && element.Type.Length < 3) //headings
                {
                    //for headings, take content as text
                    element.Text = CleanStringFromHTML(node.InnerText);
                    if (element.Text == null)
                    {
                        DontAddToOutPut = true;
                    }
                    else
                    {
                        element.Text = element.Text.Replace("\r\n", "").Trim().Replace("   ", "");
                        CutLeadingNumberFromHeading(element); //this is for aestetics
                        //set the size for the heading
                        if (element.Type.Length < 3 && element.Type[1] > '0' && element.Type[1] < '6')
                        {
                            element.Size = element.Type[1].ToString(); //extract numbers from h3...h5 headings
                            element.Type = "heading";
                        }
                    }


                }
                else if (element.Type.Equals("label") && node.HasChildNodes) //maybe a codelist or a inputfield
                {
                    element.Comment = null; //delete comment that was set at beginning of the method
                    //wenn das label kindknoten hat, dann ist der text aus dem ersten kindkoten ablesbar, der das attribut ng-binding hat
                    HtmlNode labelNode = node.SelectSingleNode("//span[@ng-bind='::text.label']");
                    if (labelNode == null)
                    {
                        DontAddToOutPut = true; //don't add that node to output list
                    }
                    else if (string.IsNullOrWhiteSpace(labelNode.InnerText))
                    {
                        DontAddToOutPut = true; //don't add that node to output list

                    }
                    else
                    {
                        element.Text = CleanStringFromHTML(node.InnerText);
                        HtmlNode childNode = SelectChildNode(node);//look what kind of child node there might be, eg. input or inn-codelist
                        if (childNode != null)
                        {
                            Element childElem = new Element();
                            childElem.Type = childNode.OriginalName;
                            childElem.Text = CleanStringFromHTML(childNode.InnerText);


                            //wenn das label einen Kindknoten vom typ liste hat
                            if (childNode.OriginalName.Equals("inn-codelist-base"))
                            {
                                if (childElem.Text != null)
                                    element.Text = element.Text.Replace(childElem.Text, "");//delete the childnode's text from the label node's text

                                childElem.Type = "dropdown";
                                var tooltipNode = node.SelectSingleNode("self::div[@uib-tooltip]");
                                childElem.List_id = childNode.GetAttributeValue("inn-list-id", null);
                                childElem.Required = childNode.GetAttributeValue("inn-required", null);
                                if (tooltipNode != null)
                                    childElem.Comment = tooltipNode.GetAttributeValue("uib-tooltip", null);
                            }
                            else if (childNode.OriginalName.Equals("input"))
                            {//e.g. with datepicker
                                childElem.Type = childNode.OriginalName;
                                string placeholder = childNode.GetAttributeValue("placeholder", null);
                                if (!string.IsNullOrWhiteSpace(placeholder))
                                {
                                    childElem.Placeholder = placeholder;
                                }
                                childElem.Maxlength = childNode.GetAttributeValue("maxlength", null);
                                childElem.Required = childNode.GetAttributeValue("ng-required", null);//toDo:look if that is right buisiness rule. have all not required fields "" and ng-required attribute?
                                childElem.Disabled = childNode.GetAttributeValue("disabled", null);
                                childElem.Pattern = CleanStringFromHTML(childNode.GetAttributeValue("ng-pattern", null));//childNode.GetAttributeValue("ng-pattern", null);

                                childElem.Comment = childNode.GetAttributeValue("uib-datepicker-popup", "not-datepicker");//if the subelem. of the label is no datepicker, it's a textfield
                                if (childElem.Comment.Equals("not-datepicker"))//set the comment of a text field to the tooltip or if no tooltip attribute present, to text field
                                    childElem.Comment = childNode.GetAttributeValue("uib-tooltip", "text field");
                            }
                            Element[] subElems = new Element[1];
                            subElems[0] = childElem;
                            element.Subelems = subElems;
                        }
                    }
                }
                else if (node.OriginalName.Equals("inn-section"))//if you reach a tag that signals the start of a new section
                {
                    //don't set the element text
                    DontAddToOutPut = true;
                    section = new Section();

                    //now let's find the user-readable heading to this heading-anchor
                    section.InputsAndHeadings = inputsAndHeadings.ToArray();
                    inputsAndHeadings = new List<Element>(); //create new élement list
                    returnList.Add(section);

                }
                else if (node.OriginalName.Equals("div"))//nodes containing the form number, also checkboxes
                {
                    if (!string.IsNullOrWhiteSpace(node.InnerText))
                        element.Text = node.InnerText.Replace("\r\n", "").Replace("\n", "").Trim().Replace("   ", "");


                }

                //if the current element wasn't a section tag, add the element to the inputsAndHeadings
                if (!DontAddToOutPut)//!string.IsNullOrWhiteSpace(element.Text)&&
                {
                    if (element.Text != null)//don't allow radio button labels without text
                        inputsAndHeadings.Add(element);
                }
                else if (DontAddToOutPut) //if you had a startsaction node, do not add the node to list, but reset the flag for sections
                {
                    DontAddToOutPut = false;
                }
                //this means, that a heading without leading digits is not recognized as a heading
            }
            return returnList;
        }
        /// <summary>
        /// helps GetAllElements to cut the numbers from the headings.
        /// </summary>
        /// <param name="element">The element whose text property has to be split</param>
        protected void CutLeadingNumberFromHeading(Element element)
        {
            //now, split the content into heading without ordinal number e.g. Registereintragungen and the ordinal number e.g. 1.1 instead of "1.1. Registereintragungen"
            string finalValue = "";

            Regex regex = new Regex("^(\\d+\\.)((.)*)"); //^search Matches at the beginning [d]of type digit that have a following dot after (.) no or * many signs into two groups ()()

            if (regex.IsMatch(element.Text))
            {
                MatchCollection matches = regex.Matches(element.Text);
                string number = "";
                foreach (Match match in matches)
                {
                    //Debug.WriteLine("Number:        {0}", match.Groups[1].Value);
                    number = match.Groups[1].Value;
                    //Debug.WriteLine("heading: {0}", match.Groups[2].Value);
                }

                finalValue = element.Text.Substring(number.Length).Trim();
                element.Text = HttpUtility.HtmlDecode(finalValue);
            }
        }

        /// <summary>
        /// cleans a text from Htlm decoded values. I found that HttpUtility.Decode() is not working on this correctly, when there are two &amp; 's behind each other as in the patterns sometimes the caase
        /// reference: http://weblogs.sqlteam.com/mladenp/archive/2008/10/21/Different-ways-how-to-escape-an-XML-string-in-C.aspx
        /// </summary>
        /// <param name="toClean"></param>
        /// <returns>a string that is HTML decoded and does not contain any newline characters or whitespace at the beginning or eind oth th text.</returns>
        protected string CleanStringFromHTML(string toClean)
        {
            if (string.IsNullOrWhiteSpace(toClean))
                return null;
            toClean = HttpUtility.HtmlDecode(toClean).Replace("amp;", "&");
            return toClean.Replace("\r\n", "").Replace("  ", "").Trim();

        }

        /// <summary>
        /// selects a specific child node from the dropdown list node.
        /// </summary>
        /// <param name="node">The parent node</param>
        /// <returns>a html node matching present in the parent node that maches one of the xpath expressions below.</returns>
        protected HtmlNode SelectChildNode(HtmlNode node)
        {
            HtmlNode returnVal = null;
            returnVal = node.SelectSingleNode(".//inn-codelist-base");//codelist label
            if (returnVal == null)
                returnVal = node.SelectSingleNode(".//input");
            return returnVal;
        }

        /// <summary>
        /// logs in to the Formularbaukasten website, then requests the form ESF_2 in the Formularbaukasten and waits until either a timeout occurs at the
        /// websitecrawler or an attribute 'load-status' is found in the html body with value 'ready'. This attribute is signalling that the javascript functions have finished their tasks
        /// on the form.
        /// </summary>
        /// <param name="username">Valid username for the Formularbaukasten form</param>
        /// <param name="password">Valid password for the Formularbaukasten form</param>
        /// <returns>html as string representing the online form</returns>
        public override string GetForm(string username, string password)
        {
            if (!(language.Equals("de") | language.Equals("en") | language.Equals("pl")))
                language="de";
            Login(username, password);
           
            string domain = "http://10.150.50.21/formularservice/formular/A_FOREX_ANTRAG_ESF_2/appl/e220f8f6-0726-11e8-b0c6-47de0dd015a0/?lang="+language+"&backURL=aHR0cCUzQSUyRiUyRjEwLjE1MC41MC4yMSUyRmlyaiUyRnBvcnRhbCUzRk5hdmlnYXRpb25UYXJnZXQlM0RST0xFUyUzQSUyRnBvcnRhbF9jb250ZW50JTJGRVUtRExSX1JlZmFjdG9yaW5nJTJGT0FNX1BPUlRBTF9BUFBMSUNBTlRfSU5ESVZJRFVBTCUyRk9ubGluZUFwcGxpY2F0aW9uQUUlMjZhcHBsaWNhdGlvbklEJTNEODEwMDQ3OTE%3D&transactionID=ad833f54-d81d-42c0-9e17-bd8419d52ff4";
            string form = wc.GetWebContentPersist(domain, "$('body').attr('load-status') == 'ready'");
            // Debug.WriteLine(form);
            return form;
        }

        /// <summary>
        /// verifys that a user name and password are valid in the Formularbaukasten website. If they are found lacking, then the string returned by this method will start with 'error' and
        /// deliver further information on the error.
        /// </summary>
        /// <param name="username">Valid username for the Formularbaukasten form</param>
        /// <param name="password">Valid password for the Formularbaukasten form</param>
        /// <returns></returns>
        public override string Login(string username, string password)
        {
            username = HttpUtility.UrlEncode(username);
            password = HttpUtility.UrlEncode(password); //test: should encode @ to %40 -> yes, it does. 


            return wc.GetWebContentPersist("http://10.150.50.21/irj/portal?login_submit=on&login_do_redirect=1&no_cert_storing=on&j_salt=l4ZnvAPEMHwyxIeUN7JmhGQwJps%3D&j_username=" + username + "&j_password=" + password + "&uidPasswordLogon=Anmelden: undefined");
            //todo find out if valid user credentials were passed

        }
        /// <summary>
        /// not implemented yet. for posting back input field values to the Formularbaukasten webiste.
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public override bool UploadSection(Section section)
        {
            throw new NotImplementedException();
        }
    }
}