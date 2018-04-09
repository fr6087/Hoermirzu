using ClassLibrary.model;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace FBKConnectorService
{
    /// <summary>
    /// provides an interface that lists all methods of Service1
    /// </summary>
    [ServiceContract]
    public interface IService1
    {
        /// <summary>
        /// for login
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [OperationContract]
        string Login(string username, string password);

        /// <summary>
        /// for retrieving a html document as string
        /// </summary>
        /// <param name="_username"></param>
        /// <param name="_password"></param>
        /// <returns></returns>
        [OperationContract]
        string GetForm(string _username, string _password);

        /// <summary>
        /// for listing input fields present in the html form
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        [OperationContract]
        List<string> GetInputs(string username, string password);

        /// <summary>
        /// for listing headings in the html form
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="xpath">the path with which to search for html nodes</param>
        /// <returns></returns>
        [OperationContract]
        List<string> GetSpecificElements(string username, string password, string xpath);

        /// <summary>
        /// retrieves all elements in the html form
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [OperationContract]
        List<Section> GetAllElements(string username, string password);

        /// <summary>
        /// reads a file with the filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        [OperationContract]
        string ReadJasonFromFile(string filename);

        /// <summary>
        /// outputs a string separated in lines that math the list_id.
        /// </summary>
        /// <param name="list_id"></param>
        /// <returns></returns>
        [OperationContract]
        string[] GetCSVDropdownString(string list_id, string filter =null);

        /// <summary>
        /// outputs a json string
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [OperationContract]
        string SerializeToJson(string username, string password);

        /// <summary>
        /// converts a list object to json string
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        [OperationContract]
        bool UploadSection(Section section);

        /// <summary>
        /// sends a request message to the bot and retrieves an answer.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [OperationContract]
        Task<string> TalkToTheBotAsync(string message);

        /// <summary>
        /// creates a string suited for the json that a Microsoft Bot can build FormFlow dialogs from
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="sectionindex"></param>
        /// <returns></returns>
        [OperationContract]
        string JsonForBot(string username, string password, short sectionindex);

        }
}