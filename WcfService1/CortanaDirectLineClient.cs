
using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Threading.Tasks;
using Microsoft.Rest;
using System.Configuration;
using System.Diagnostics;

namespace WcfService1
{
    /// <summary>
    /// creates DirectLine connection to the bot
    /// </summary>
    public class CortanaDirectLineClient
    {
        /// <summary>
        /// client
        /// </summary>
        private DirectLineClient _directLine;
        /// <summary>
        /// client's conversation id
        /// </summary>
        private string _conversationId;
        /// <summary>
        /// the key to the bot's DirectLine channel
        /// </summary>
        private string _directLineSecret = ConfigurationManager.AppSettings["DirectLineSecret"];
        /// <summary>
        /// the bot's microsoft id
        /// </summary>
        private string botId = ConfigurationManager.AppSettings["BotId"];
        private readonly string username = ConfigurationManager.AppSettings["userName"];



        //these are methods that work except for herocards reference Microsoft Botbuidler-Samples on Github
        /// <summary>
        /// opens a DirectLine channel to the bot
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            _directLine = new DirectLineClient(_directLineSecret);

            HttpOperationResponse<Conversation> conversation = await _directLine.Conversations.StartConversationWithHttpMessagesAsync();// NewConversationWithHttpMessagesAsync();
            _conversationId = conversation.Body.ConversationId;

            System.Diagnostics.Debug.WriteLine("Bot connection set up.");
        }

        /// <summary>
        /// retrieves a response from the bot
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetResponse()
        {
            try
            {
                if (this._conversationId == null)
                {
                    await this.ConnectAsync();
                }

                var httpMessages = await _directLine.Conversations.GetActivitiesWithHttpMessagesAsync(_conversationId);
                var messages = httpMessages.Body.Activities;//Activities;

                // our bot only returns a single message, so we won't loop through
                // First message is the question, second message is the response
                if (messages?.Count > 1)
                {
                    // select latest message -- the response
                    var text = messages[messages.Count - 1].Text;
                    System.Diagnostics.Debug.WriteLine("Response from bot was: " + text);

                    return text;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Response from bot was empty.");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                throw;
            }

        }


        /// <summary>
        /// sends a message to the bot and retrieves da response for it
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<string> TalkToTheBotAsync(string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Sending bot message");

                //Message msg = new Message();//type was in original Message
                Activity userMessage = new Activity
                {
                    From = new ChannelAccount(username),
                    Text = message,
                    Type = ActivityTypes.Message
                };

                if (this._conversationId == null)
                {
                    await this.ConnectAsync();
                }

                System.Diagnostics.Debug.WriteLine("Posting " + _conversationId);
                try
                {
                    await _directLine.Conversations.PostActivityAsync(_conversationId, userMessage);
                }
                catch (Microsoft.Rest.HttpOperationException e)
                {
                    Debug.WriteLine(e.Source + e.Message);
                    Debug.WriteLine("");
                    Debug.WriteLine("");
                }


                System.Diagnostics.Debug.WriteLine("Post complete");

                return await GetResponse();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                throw;
            }
        }


    }
}
