using ListenToMe.Model;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ListenToMe
{

    /// <summary>
    /// establishes connection to LUIS API. It extracts the intent that it recognises behind the user input. For example,
    /// a user input like "what is the stock price of dax?" will return JsonObject:
    /// <code>
    /// Response{
 /// "query": "what about dax",
  ///"topScoringIntent": {
   /// "intent": "StockPrice2",
   /// "score": 0.3969668
  ///},
  ///"intents": [
   /// {
   ///   "intent": "StockPrice2",
   ///   "score": 0.3969668
   /// },
    ///{
      ///"intent": "None",
      ///"score": 0.372112036
    ///},
 /// ],
  ///"entities": [
    ///{
      ///"entity": "dax",
      ///"type": "StockSymbol"
   /// }
  ///]
///}
///</code>
///The HttpClientConnection is from an old state of the App. The App uses now a DirectLine-Connection via http. Therefore it contains
///methods and fields that support DirectLine as well. With the DirectLine Connection it is not only possible to reach the luis model
///but also the bot. This enables to create a more sophisticated bot e.g. containing FormFlow and ccommunicate with it.
    /// </summary>
        public sealed class Bot
        {

        //reference: https://github.com/Psychlist1972/InternetOfStrangerThings
        
        
        /// <summary>
        /// The fromUser tag is a parameter when initializing a directLine call. It is a string that helps identifying the sender.
        /// there are no name restrictions on that. Could be also 'Alice' or 'Bob'
        /// </summary>
        private static readonly string fromUser = App.UserName;

        /// <summary>
        /// The method is calling the LUIS-Model via HttpClient 
        /// ToDo find out how to model this to DirectLine as well.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public IAsyncOperation<Rootobject> SendMessageAndGetIntentFromBot(string message)
            {
            
                return Task.Run<Rootobject>(async () =>
                {

                    string intent = "none";
                    Rootobject myObject = null;
                    try
                    {
                        myObject= await Proxy.GetJSON(message);//toDo return the rootobject (because it also has discovered entities)
                        var topscoringIntent = myObject.topScoringIntent;
                        try{
                            intent = topscoringIntent.intent;
                        }catch(System.NullReferenceException )
                            {
                            Debug.WriteLine("No topScoringIntent discovered by Bot.cs.SendMessageAndGetIntentFromBot()");
                            }
                        Debug.WriteLine("topScoringIntent" + intent);

                    }
                    catch (Exception e)
                    {
                        // no op
                        Debug.WriteLine(e.Message);
                    }
                    Debug.WriteLine("Bot is returning "+myObject.ToString());
                    return myObject;
                }).AsAsyncOperation();
            }
        
    }
}

