
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace ListenToMe.Model
{
    /// <summary>
    /// queries LUISbotAi with techniques of Collin Blake from https://www.youtube.com/watch?v=ziLkj4PmcCE
    /// </summary>
    public class Proxy
    {
        /// <summary>
        /// sends a query to the language understanding model LUIS
        /// </summary>
        /// <param name="query">the user message to be evaluated</param>
        /// <returns></returns>
        public async static Task<Rootobject> GetJSON(string query)
        {

            var http = new System.Net.Http.HttpClient();
            //toDo: Bots für unterschiedliche Sprachen referenzieren
            var response = await http.GetAsync("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/e81f058a-dd41-46b9-8ba6-ad32174c63c4?subscription-key=a5de48ea62014f2bbdc4ad05943f2081&verbose=true&timezoneOffset=0&q=" +
                query);

            var result = await response.Content.ReadAsStringAsync();
            //access denied due to invalid suscription key
            Debug.Write(result);
            var serializer = new DataContractJsonSerializer(typeof(Rootobject));

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));

            var data = (Rootobject)serializer.ReadObject(ms);
            http.Dispose();
            return data;
        }

        /// <summary>
        /// the luis api is theoretically also able to process other requests automatically. This one is for updating the entities in the luis model.
        /// But it didn't work. I think I'm misunderstanding the documentation
        /// reference: https://westus.dev.cognitive.microsoft.com/docs/services/5890b47c39e2bb17b84a55ff/operations/5890b47c39e2bb052c5b9c2f
        /// </summary>
        public async static Task UploadHeadings()
        {
            var client = new Windows.Web.Http.HttpClient();
            //api/v2.0/apps/{appId}/versions/{versionId}/customprebuiltentities -> in api
            String appId = "b0ea3d44-cf05-42df-8fae-96b45e08cef7";
            String versionId = "0.1";
            Uri uri = new Uri("https://westus.api.cognitive.microsoft.com/luis/api/v2.0/apps/" + appId + "/versions/" + versionId + "/entities");//before:customprebuiltentities
            //Service1Client form_reading_client = new Service1Client();
            Debug.WriteLine(App.UserName + " " + App.UserPassword);

            //var headings = await form_reading_client.GetInputsAsync(App.userName, App.userPassword, App.uri);
            //create Json Object
            CustomPrebuildEntity entity = new CustomPrebuildEntity();
            entity.name = "IBAN";
            //inputs.canonicalForm = "FeldName1";
            //inputs.list = headings;
            //System.ServiceModel.FaultException: "Der Server konnte die Anforderung aufgrund eines internen Fehlers nicht verarbeiten. Wenn Sie weitere Informationen zum Fehler erhalten möchten, aktivieren Sie entweder IncludeExceptionDetailInFaults (über das ServiceBehaviorAttribute oder das <serviceDebug>-Konfigurationsverhalten) für den Client, um die Ausnahmeinformationen zurück an den Server zu senden, oder aktivieren Sie die Ablaufverfolgung gemäß der Microsoft .NET Framework SDK-Dokumentation, und überprüfen Sie d
            //convert Json object to String and Send it to file
            String fieldName = JsonConvert.SerializeObject(entity, Formatting.Indented);
            await MakeRequest(appId, versionId, fieldName);

            /*fieldName = "[" + Environment.NewLine + fieldName + Environment.NewLine + "]";

            using (client)
            {
                var req = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Post, uri);
                //add Luis programmatic api key
                req.Headers.Add("Ocp-Apim-Subscription-Key", "a5de48ea62014f2bbdc4ad05943f2081");// from the azure portal forces 401- please use from luis.ai, F
                req.Headers["Accept"] = "application/json";
                req.Headers.Add("Content-Type", "application/json");
                var stringContent = new Windows.Web.Http.HttpStringContent(fieldName);
                var response = await client.PostAsync(uri, stringContent);
                Debug.WriteLine(response);//some id
            }*/

        }

        /// <summary>
        /// this method is written to send data back to the luis-model. Could be useful if the form changes dramatically.
        /// doesn't work as in the luis APÍ documented.
        /// reference: https://westus.dev.cognitive.microsoft.com/docs/services/5890b47c39e2bb17b84a55ff/operations/5890b47c39e2bb052c5b9c2f
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="versionId"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        static async Task MakeRequest(String appId, String versionId, String body)
        {
            Debug.WriteLine(body);
            var client = new System.Net.Http.HttpClient();
            //var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "a5de48ea62014f2bbdc4ad05943f2081");

            //var uri = "https://westus.api.cognitive.microsoft.com/luis/api/v2.0/apps/{appId}/versions/{versionId}/example?";
            var uri = "https://westus.api.cognitive.microsoft.com/luis/api/v2.0/apps/b0ea3d44-cf05-42df-8fae-96b45e08cef7/versions/0.1/customprebuiltentities";

            System.Net.Http.HttpResponseMessage response;

            // Request body
            byte[] byteData = Encoding.UTF8.GetBytes(body);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                Debug.WriteLine(response);
            }
            client.Dispose();

        }

    }
    /// <summary>
    /// models the entity that programmatically should be uploaded to the luis model
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class CustomPrebuildEntity
    {
        [JsonProperty]
        public String name { get; set; }
    }

    /// <summary>
    /// models the default LUIS Http-answer which consiscts in a json object providing propabilities of intens together with entity model
    /// </summary>
    [DataContract]
    public class Rootobject
    {
        [DataMember]
        public string query { get; set; }
        [DataMember]
        public Topscoringintent topScoringIntent { get; set; }
        [DataMember]
        public Intent[] intents { get; set; }
        [DataMember]
        public Entity[] entities { get; set; }
    }
    /// <summary>
    /// subclass of RootObject. Note: these classes were easily pasted in C# using the visual studio tools for converting JSON
    /// </summary>
    [DataContract]
    public class Topscoringintent
    {
        [DataMember]
        public string intent { get; set; }
        [DataMember]
        public float score { get; set; }
    }

    /// <summary>
    /// subclass of RootObject. Note: these classes were easily pasted in C# using the visual studio tools for converting JSON
    /// </summary>
    [DataContract]
    public class Intent
    {
        [DataMember]
        public string intent { get; set; }
        [DataMember]
        public float score { get; set; }
    }
    /// <summary>
    /// subclass of RootObject. Note: these classes were easily pasted in C# using the visual studio tools for converting JSON
    /// </summary>
    [DataContract]
    public class Entity
    {
        [DataMember]
        public string entity { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public int startIndex { get; set; }
        [DataMember]
        public int endIndex { get; set; }
        [DataMember]
        public float score { get; set; }
    }

}
