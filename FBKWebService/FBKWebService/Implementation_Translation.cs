
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using ClassLibrary.model;
using HtmlAgilityPack;


namespace FBKWebService
{
    /// <summary>
    /// calls microsoft text translation service in between form delivery
    /// </summary>
    public class Implementation_Translation : Implementation_Real
    {
        /// <summary>
        /// The endpoint address of the google text translation
        /// </summary>
        private static string host = "https://api.microsofttranslator.com";
        /// <summary>
        /// The web service address of the google text translation
        /// </summary>
        private static string path = "/V2/Http.svc/Translate";
        /// <summary>
        /// A client that helps connecting to the google text translation
        /// </summary>
        private HttpClient client = new HttpClient();
        /// <summary>
        /// The password of the google text translation
        /// </summary>
        private static string key = System.Configuration.ConfigurationManager.AppSettings["MSTextTranslator"];
        /// <summary>
        /// The target language in which the form should be translated to
        /// </summary>
        private string language;

        public Implementation_Translation(string lang)
        {
            language = lang;
        }
        public Implementation_Translation()
        {
            language = "de";
        }
        /// <summary>
        /// overrides the parent class' function by a call to the online form that translates
        /// the contents of the HTML tags to the language specified in the property language.
        /// </summary>
        /// <param name="username">the username to login into the form</param>
        /// <param name="password">the password to login into the form</param>
        /// <returns></returns>
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
                            element.Text = Translate(textNode.InnerText.Replace("\r\n", ""), language).Result;
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
                                element.Text = Translate(textNode.InnerText.Replace("\r\n", ""), language).Result;
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
                        element.Text = this.CleanStringFromHTML(node.InnerText);
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
                                    element.Text = Translate(element.Text.Replace(childElem.Text, ""), language).Result;//delete the childnode's text from the label node's text

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
                                    childElem.Placeholder = Translate(placeholder, language).Result;
                                }
                                childElem.Maxlength = childNode.GetAttributeValue("maxlength", null);
                                childElem.Required = childNode.GetAttributeValue("ng-required", null);//toDo:look if that is right buisiness rule. have all not required fields "" and ng-required attribute?
                                childElem.Disabled = childNode.GetAttributeValue("disabled", null);
                                childElem.Pattern = base.CleanStringFromHTML(childNode.GetAttributeValue("ng-pattern", null));//childNode.GetAttributeValue("ng-pattern", null);

                                childElem.Comment = childNode.GetAttributeValue("uib-datepicker-popup", "not-datepicker");//if the subelem. of the label is no datepicker, it's a textfield
                                if (childElem.Comment.Equals("not-datepicker"))
                                {//set the comment of a text field to the tooltip or if no tooltip attribute present, to text field
                                    childElem.Comment = childNode.GetAttributeValue("uib-tooltip", "text field");
                                    if (!childElem.Comment.Equals("text field"))
                                    {
                                        childElem.Placeholder = Translate(childElem.Comment, language).Result;
                                        childElem.Comment = "text field";
                                    }
                                }
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
                        element.Text = Translate(node.InnerText.Replace("\r\n", "").Replace("\n", "").Trim().Replace("   ", ""), language).Result;

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
            client.Dispose();
            return returnList;
        }
        /// <summary>
        /// translates the input toTranslate to the language specified in the string language
        /// reference: https://www.codeproject.com/Articles/308809/WPF-Language-Translator 
        /// </summary>
        /// <param name="toTranslate">e.g. "Beneath this mask there is an idea..."</param>
        /// <param name="language">e.g. zh-CHS for simplified chinese</param>
        /// <returns></returns>
        private async Task<string> Translate(string toTranslate, string language)
        {
            if (string.IsNullOrWhiteSpace(toTranslate))
                return null;
            if (language.StartsWith("de"))
                return toTranslate;

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

            string uri = host + path + "?from=de&to=" + language + "&text=" + System.Net.WebUtility.UrlEncode(toTranslate);
            HttpResponseMessage response = await client.GetAsync(uri);

            string result = await response.Content.ReadAsStringAsync();
            var content = XElement.Parse(result).Value;

            Debug.WriteLine("Translate: " + content);
            return content;
        }
        /// <summary>
        /// overrides inherited method from parent class by adding to the HTML-cleansing
        /// a translation service.
        /// </summary>
        /// <param name="toClean"></param>
        /// <returns></returns>
        new string CleanStringFromHTML(string toClean)
        {
            if (string.IsNullOrWhiteSpace(toClean))
                return null;
            toClean = HttpUtility.HtmlDecode(toClean).Replace("amp;", "&");
            toClean = toClean.Replace("\r\n", "").Replace("  ", "").Trim();
            toClean = Translate(toClean, language).Result;

            return toClean;
        }
    }
}