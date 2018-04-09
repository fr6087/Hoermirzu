namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using LuisBot.Dialogs;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using LuisBot.Resource;
    using System.Diagnostics;

    /*
* for new languages such as Polish:
type windows key and search visual studio comand prompt. navigate to the Resource folder with cd command
execute resgen.exe tool in visual command prompt using the syntax resgen inputFilename [outputFilename] /str:language[,namespace,[classname[,filename]]] [/publicClass] 
assembly name is assembly LuisBot.dll and namespace for rview tool Method to build Form: ESF2CompanyDetailsForm.BuildForm();

resgen Resource1.resx LuisBot.Resource.Resource1.resources /r:../bin/LuisBot.dll /str:cs,LuisBot.Resource,Resource1

see also http://www.c-sharpcorner.com/uploadfile/yougerthen/handle-resource-files-use-resgen-exe-to-generate-resources-files-part-v/
*/
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string LuisOption = "2";
        private const string UploadOption = "3";
        private const string FormOption = "1";
        private string PersonInfo="";

        private Func<IDialog<JObject>> makeJsonRootDialog;
        

        public RootDialog(Func<IDialog<JObject>> makeJsonRootDialog)
        {
            this.makeJsonRootDialog = makeJsonRootDialog;
        }

        public async Task StartAsync(IDialogContext context)
        {
            Resource1.Culture = Thread.CurrentThread.CurrentUICulture;
            context.Wait(this.MessageReceivedAsync);
            
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;
            
            if (activity.Entities != null)
            {
                var userInfo = activity.Entities.FirstOrDefault(e => e.Type.Equals("UserInfo"));
                if (userInfo != null)
                {
                    var email = userInfo.Properties.Value<string>("email");
                    //var name = userInfo.Properties.Value<JProperty>("name")["GivenName"].Value<string>();
                    /* structure of the json we want to access in the following code
                     
        {
          "type": "UserInfo",
          "email": "friederike.geissler@somewhere.de",
          "name": {
            "GivenName": "Friederike",
            "FamilyName": "Geissler"
          }
        },

                     */
                    if (!string.IsNullOrEmpty(email))
                    {
                        PersonInfo = Resource1.hello + ", " + email;
                        

                    }
                }
                this.ShowOptions(context);
            } 
            
        }
        

        /// <summary>
        /// displays options about which child dialog to pick. similar to a menu in an html5 website. 
        /// </summary>
        /// <param name="context"></param>
        private void ShowOptions(IDialogContext context)
        {
            string retry = Resource1.DialogRetry;
            
            List<string> options = new List<string>() { FormOption, LuisOption, UploadOption
                  };
            List<string> descriptions= new List<string>() {
                Resource1.OptionForm, Resource1.OptionLuis, Resource1.OptionUpload };
            PromptDialog.Choice(context, this.OnOptionSelected, options, PersonInfo+ Resource1.AskSelectDialog, retry, 3, descriptions: descriptions);
        }

        /// <summary>
        /// defines the actions after a child dialog has been picked. delegates task to child dialog.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case LuisOption:
                        context.Call(new BasicLuisDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case UploadOption:
                        context.Call(new UploadDialog(), this.ResumeAfterOptionDialog);//, new Activity(ActivityTypes.Message, "upload"));
                        break;

                    case FormOption:
                        await context.Forward(makeJsonRootDialog(), this.ResumeAfterOptionDialog, new Activity(ActivityTypes.Message,"form"));
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.SayAsync(text: $"" + Resource1.TooManyAttempts, speak: $"" + Resource1.TooManyAttempts);

                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterSupportDialog(IDialogContext context, IAwaitable<object> result)
        {
            var ticketNumber = await result;
            string answer = $"" + Resource1.AfterSupport;
            var message = new Activity(answer);
            message.Speak = answer;
            await context.PostAsync(message);
        }

        /// <summary>
        /// defines what happens after returning sucessfully from one of the child dialogs (luis, form or upload)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
                //toDo do something with the JSONObject returned by FormDialog
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Resource1.Exception+$"{ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }
        
    }
}