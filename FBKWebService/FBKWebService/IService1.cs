using ClassLibrary.model;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace FBKWebService
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Schnittstellennamen "IService1" sowohl im Code als auch in der Konfigurationsdatei ändern.
    [ServiceContract]
    public interface IService1
    {

        /// <summary>
        /// creates a string suited for the json that a Microsoft Bot can build FormFlow dialogs from
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="sectionindex"></param>
        /// <returns></returns>
        [OperationContract]
        string JsonForBot(string username, string password, short sectionindex);
        /// <summary>
        /// sends a request message to the bot and retrieves an answer.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [OperationContract]
        Task<string> TalkToTheBotAsync(string message);
        /// <summary>
        /// for listing headings in the html form
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="xpath">the path with which to search for html nodes</param>
        /// <returns></returns>
        [OperationContract]
        List<string> GetSpecificElements(string username, string password, string xpath);

        [OperationContract]
        string GetForm(string username, string password);
        [OperationContract]
        List<Section> GetAllElements(string username, string password);
        [OperationContract]
        string Login(string username, string password);
        /// <summary>
        /// outputs a string separated in lines that math the list_id.
        /// </summary>
        /// <param name="list_id"></param>
        /// <returns></returns>
        [OperationContract]
        string[] GetCSVDropdownString(string list_id, string filter = null);
    }

    
}
