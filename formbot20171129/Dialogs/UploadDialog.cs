using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using System.IO;
using LuisBot.Resource;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class UploadDialog : IDialog<object>
    {

        private static string ShowInlineAttachment = Resource1.UploadDialogOptionInline;
        private static string ShowUploadedAttachment = Resource1.UploadDialogOptionService;
        private static string ShowInternetAttachment = Resource1.UploadDialogOptionWeb;

        private readonly IDictionary<string, string> options = new Dictionary<string, string>
        {
            { "1", ShowInlineAttachment },
            { "2", ShowUploadedAttachment },
            { "3", ShowInternetAttachment }
        };
        public async Task StartAsync(IDialogContext context)
        {

            var welcomeMessage = Resource1.UploadDialogWelcome;
            await context.SayAsync(text: welcomeMessage, speak: welcomeMessage);
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var message = await result;
            await this.DisplayOptionsAsync(context);

        }

        private async Task DisplayOptionsAsync(IDialogContext context)
        {
            string prompt = Resource1.UploadDialogPrompt;
            string retry = Resource1.DialogRetry;
            PromptDialog.Choice<string>(
                context,
                this.ProcessSelectedOptionAsync,
                this.options.Keys,
                prompt,
                retry,
                3,
                PromptStyle.PerLine,
                this.options.Values);
        }

        private async Task ProcessSelectedOptionAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;

            var replyMessage = context.MakeMessage();

            Attachment attachment = null;

            switch (message)
            {
                case "1":
                    var message_answer = context.MakeMessage();
                    message_answer.Text = Resource1.UploadAttachment;
                    message_answer.Speak = SSMLHelper.Speak(Resource1.UploadAttachment);
                    message_answer.InputHint = InputHints.AcceptingInput;

                    await context.PostAsync(message_answer);
                    context.Wait(this.GetAttachment);
                    break;
                case "2":
                    attachment = await GetUploadedAttachmentAsync(replyMessage.ServiceUrl, replyMessage.Conversation.Id);
                    replyMessage.Attachments = new List<Attachment> { attachment };
                    replyMessage.Speak = Resource1.UploadBot;
                    await context.PostAsync(replyMessage);

                    //await this.DisplayOptionsAsync(context);
                    //await MessageReceivedAsync(context,argument);//toDo: find out if this is correct
                    context.Done(Resource1.ReEnterRoot);
                    break;
                case "3":
                    attachment = GetInternetAttachment();
                    replyMessage.Attachments = new List<Attachment> { attachment };
                    replyMessage.Speak = Resource1.UploadInternet;
                    await context.PostAsync(replyMessage);
                    context.Done(Resource1.ReEnterRoot);
                    break;
            }
            
        }

        private async Task GetAttachment(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.Attachments != null && message.Attachments.Any())
            {
                var attachment = message.Attachments.First();

                using (HttpClient httpClient = new HttpClient())
                {
                    // Skype & MS Teams attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.

                    if ((message.ChannelId.Equals("skype", StringComparison.InvariantCultureIgnoreCase) || message.ChannelId.Equals("msteams", StringComparison.InvariantCultureIgnoreCase))
                        && new Uri(attachment.ContentUrl).Host.EndsWith("skype.com"))
                    {
                        var token = await new MicrosoftAppCredentials().GetTokenAsync();
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    }
                    var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);
                    var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;
                    await context.SayAsync(text:$"Attachment of {attachment.ContentType} type and size of {contentLenghtBytes} bytes received.", speak: $"Attachment of {attachment.ContentType} type and size of {contentLenghtBytes} bytes received.");
                }
            }
            else
            {
                await context.SayAsync(text: Resource1.UploadNoAttachment, speak: Resource1.UploadNoAttachment);
            }
            context.Wait(this.MessageReceivedAsync);
        }

    /// <summary>
    ///shows how to call upload from the server
    /// </summary>
    /// <param name="serviceUrl"></param>
    /// <param name="conversationId"></param>
    /// <returns></returns>
    private static async Task<Attachment> GetUploadedAttachmentAsync(string serviceUrl, string conversationId)
    {
        var imagePath = HttpContext.Current.Server.MapPath("~/images/big-image.png");

        using (var connector = new ConnectorClient(new Uri(serviceUrl)))
        {
            var attachments = new Attachments(connector);
            var response = await attachments.Client.Conversations.UploadAttachmentAsync(
                conversationId,
                new AttachmentData
                {
                    Name = "big-image.png",
                    OriginalBase64 = File.ReadAllBytes(imagePath),
                    Type = "image/png"
                });

            var attachmentUri = attachments.GetAttachmentUri(response.Id);

            return new Attachment
            {
                Name = "big-image.png",
                ContentType = "image/png",
                ContentUrl = attachmentUri
            };
        }
    }

    private static Attachment GetInternetAttachment()
    {
        return new Attachment
        {
            Name = "BotFrameworkOverview.png",
            ContentType = "image/png",
            ContentUrl = "https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png"
        };
    }
}
}