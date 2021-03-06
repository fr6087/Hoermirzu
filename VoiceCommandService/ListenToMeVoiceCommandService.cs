﻿
using ClassLibrary.model;
using ListenToMe.VoiceCommands.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources.Core;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;

namespace ListenToMe.VoiceCommands
{
    /// <summary>
    /// is implementing a background service for the App ListentApp. The background service is activated in the app's
    /// VoiceCommands.xml in Information command. As it is, it does not work as it should, so Cortana can not execute the command information. There are
    /// exceptions thrown
    /// toDo Debug
    /// </summary>
    public sealed class VoiceCommandService : IBackgroundTask
    {
        /// <summary>
        /// the service connection is maintained for the lifetime of a cortana session, once a voice command
        /// has been triggered via Cortana.
        /// </summary>
        VoiceCommandServiceConnection voiceServiceConnection;

        /// <summary>
        /// Lifetime of the background service is controlled via the BackgroundTaskDeferral object, including
        /// registering for cancellation events, signalling end of execution, etc. Cortana may terminate the 
        /// background service task if it loses focus, or the background task takes too long to provide.
        /// 
        /// Background tasks can run for a maximum of 30 seconds.
        /// </summary>
        BackgroundTaskDeferral serviceDeferral;

        /// <summary>
        /// ResourceMap containing localized strings for display in Cortana.
        /// </summary>
        ResourceMap cortanaResourceMap;

        /// <summary>
        /// The context for localized strings.
        /// </summary>
        ResourceContext cortanaContext;

        Service1Client client = new Service1Client();

        /// <summary>
        /// Get globalization-aware date formats.
        /// </summary>
        DateTimeFormatInfo dateFormatInfo;
        /// <summary>
        /// The background task entrypoint. 
        /// 
        /// Background tasks must respond to activation by Cortana within 0.5 seconds, and must 
        /// report progress to Cortana every 5 seconds (unless Cortana is waiting for user
        /// input). There is no execution time limit on the background task managed by Cortana,
        /// but developers should use plmdebug (https://msdn.microsoft.com/library/windows/hardware/jj680085%28v=vs.85%29.aspx)
        /// on the Cortana app package in order to prevent Cortana timing out the task during
        /// debugging.
        /// 
        /// The Cortana UI is dismissed if Cortana loses focus. 
        /// The background task is also dismissed even if being debugged. 
        /// Use of Remote Debugging is recommended in order to debug background task behaviors. 
        /// Open the project properties for the app package (not the background task project), 
        /// and enable Debug -> "Do not launch, but debug my code when it starts". 
        /// Alternatively, add a long initial progress screen, and attach to the background task process while it executes.
        /// </summary>
        /// <param name="taskInstance">Connection to the hosting background service process.</param>
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            serviceDeferral = taskInstance.GetDeferral();
            /*MessageDialog message = new MessageDialog("running VoiceCommandService");
            await message.ShowAsync();*/

            // Register to receive an event if Cortana dismisses the background task. This will
            // occur if the task takes too long to respond, or if Cortana's UI is dismissed.
            // Any pending operations should be cancelled or waited on to clean up where possible.
            taskInstance.Canceled += OnTaskCanceled;

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            Debug.Write("We have some TriggerDetails named " + triggerDetails.Name);

            // Load localized resources for strings sent to Cortana to be displayed to the user.
            cortanaResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");

            // Select the system language, which is what Cortana should be running as.
            cortanaContext = ResourceContext.GetForViewIndependentUse();

            // Get the currently used system date format
            dateFormatInfo = CultureInfo.CurrentCulture.DateTimeFormat;

            // This should match the uap:AppService and VoiceCommandService references from the 
            // package manifest and VCD files, respectively. Make sure we've been launched by
            // a Cortana Voice Command.
            if (triggerDetails != null && triggerDetails.Name == "VoiceCommandService")
            {
                try
                {
                    voiceServiceConnection =
                        VoiceCommandServiceConnection.FromAppServiceTriggerDetails(
                            triggerDetails);

                    voiceServiceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;

                    // GetVoiceCommandAsync establishes initial connection to Cortana, and must be called prior to any 
                    // messages sent to Cortana. Attempting to use ReportSuccessAsync, ReportProgressAsync, etc
                    // prior to calling this will produce undefined behavior.
                    VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();
                    Debug.Write("received voicecommand" + voiceCommand.Properties.ToString());
                    // Depending on the operation (defined in AdventureWorks:AdventureWorksCommands.xml)
                    // perform the appropriate command.
                    switch (voiceCommand.CommandName)
                    {
                        case "Shutdown":
                            //var destination = voiceCommand.Properties["destination"][0];
                            //await SendCompletionMessageForDestinationAsync(destination);
                            break;
                        case "Edit":
                            Debug.WriteLine("called Edit.");
                            //var cancelDestination = voiceCommand.Properties["destination"][0];
                            //await SendCompletionMessageForCancellationAsync(cancelDestination);
                            break;
                        case "Page":
                            //var pageName = voiceCommand.Properties["destination"][0];
                            //await SendCompletionMessageForCancellationAsync(pageName);
                            break;
                        case "Upload":
                            //var uploadFile = voiceCommand.Properties["destination"][0];
                            //await SendCompletionMessageForCancellationAsync(uploadFile);
                            break;
                        case "Information":
                            Debug.WriteLine("Jay!!!!!!!!!!!!!!!!!!!!!!!!!!Reached VCD-switch");
                            String uploadFile = "someText";
                            try
                            {
                                uploadFile = voiceCommand.Properties["Field"][0];
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e.Message);
                            }
                            await SendCompletionMessageForGeneralInfoAsync(uploadFile);
                            break;
                        default:
                            // As with app activation VCDs, we need to handle the possibility that
                            // an app update may remove a voice command that is still registered.
                            // This can happen if the user hasn't run an app since an update.
                            LaunchAppInForegroundAsync();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Handling Voice Command failed " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// loads a section from form store 
        /// ToDo implement data storage
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private async Task SendCompletionMessageForGeneralInfoAsync(string fieldName = null)
        {
            // If this operation is expected to take longer than 0.5 seconds, the task must
            // provide a progress response to Cortana prior to starting the operation, and
            // provide updates at most every 5 seconds.
            string loadingPageToEdit = "";
            Debug.WriteLine("Jay!!!!!!!!!!!!!!!!!!!!!!!!Reached VCD-switch");
            loadingPageToEdit = string.Format(
                       cortanaResourceMap.GetValue("LoadingFieldToEdit", cortanaContext).ValueAsString,
                       fieldName);//vormals LoadingTripToDestination
            Debug.WriteLine("dfkskkdkkd" +loadingPageToEdit);
            if (String.IsNullOrWhiteSpace(loadingPageToEdit))
            {
                loadingPageToEdit = "put here some general info if no field was specified";
            }
            await ShowProgressScreenAsync(loadingPageToEdit);
            ClassLibrary.model.Form store = new ClassLibrary.model.Form();
            


            // Look for the specified section. The destination *should* be pulled from the grammar we
            // provided, and the subsequently updated phrase list, so it should be a 1:1 match, including case.
            // However, we might have multiple trips to the destination. For now, we just pick the first.
            
            store.Sections = await client.GetAllElementsAsync("FakeUserName","FakePassword");
           
            
            IEnumerable<ClassLibrary.model.Section> sections = store.Sections.Where(f => f.InputsAndHeadings.Any(e=>e.Text.Equals(fieldName))); //toDo:repair this
            Debug.WriteLine("sections count: "+sections.Count<Section>());
            var userMessage = new VoiceCommandUserMessage();
            var fieldsContentTiles = new List<VoiceCommandContentTile>();
            if (sections.Count() == 0)
            {
                // In this scenario, perhaps someone has modified data on your service outside of your 
                // control. If you're accessing a remote service, having a background task that
                // periodically refreshes the phrase list so it's likely to be in sync is ideal.
                // This is unlikely to occur for this sample app, however.
                string foundNoPageToEdit = string.Format(
                       cortanaResourceMap.GetValue("FoundNoPageToEdit", cortanaContext).ValueAsString,//vormals FoundNoTripToDestination
                       fieldName);
                userMessage.DisplayMessage = foundNoPageToEdit;
                userMessage.SpokenMessage = foundNoPageToEdit;
            }
            else
            {
                // Set a title message for the page.
                string message = "";
                if (sections.Count() > 1)
                {
                    message = cortanaResourceMap.GetValue("PluralFields", cortanaContext).ValueAsString;
                }
                else
                {
                    message = cortanaResourceMap.GetValue("SingularUpcomingField", cortanaContext).ValueAsString;
                }
                userMessage.DisplayMessage = message;
                userMessage.SpokenMessage = message;

                // file in tiles for each destination, to display information about the trips without
                // launching the app.
                foreach (ClassLibrary.model.Section section in sections)
                {
                    int i = 1;

                    var destinationTile = new VoiceCommandContentTile();
                    IEnumerable<ClassLibrary.model.Element> elements = section.InputsAndHeadings.Where(e => e.Text.Equals(fieldName)); //toDo:repair this

                    foreach (Element field in elements)
                    {

                        int j = 1;
                        // To handle UI scaling, Cortana automatically looks up files with FileName.scale-<n>.ext formats based on the requested filename.
                        // See the VoiceCommandService\Images folder for an example.
                        destinationTile.ContentTileType = VoiceCommandContentTileType.TitleWith68x68IconAndText;
                        destinationTile.Image = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///ListenToMe.VoiceCommands/Images/Triangle.png"));

                        destinationTile.AppLaunchArgument = section.InputsAndHeadings[4].Text;
                        //toDo: find out wht the app is doing over here; why it does find Startdate
                        destinationTile.Title = section.InputsAndHeadings.First().Text;

                        if (field.Text != null)
                        {
                            destinationTile.TextLine1 = "in dem Feld steht gerade"+field.Subelems.First().Text;//.Value.ToString(dateFormatInfo.LongDatePattern);
                        }
                        else
                        {
                            destinationTile.TextLine1 = section.InputsAndHeadings[4].Text + " Field " + j; 
                        }
                        fieldsContentTiles.Add(destinationTile);
                        j++;
                    }
                    i++;
                }
            }

            var response = VoiceCommandResponse.CreateResponse(userMessage, fieldsContentTiles);

            if (sections.Count() > 0)
            {
                response.AppLaunchArgument = fieldName;
            }

            await voiceServiceConnection.ReportSuccessAsync(response);
        }

        /*
    /// <summary>
    /// search for and find single page in cortana
    /// </summary>
    /// <param name="uploadFile"></param>
    /// <returns></returns>
    private async Task SendCompletionMessageForCancellationAsync(string destination)
    {
        // If this operation is expected to take longer than 0.5 seconds, the task must
        // provide a progress response to Cortana prior to starting the operation, and
        // provide updates at most every 5 seconds.
        string loadingPageToDestination = string.Format(
                   cortanaResourceMap.GetValue("LoadingTripToDestination", cortanaContext).ValueAsString,
                   destination);
        await ShowProgressScreenAsync(loadingPageToDestination);
        //ListenToMe.Model.PageStore store = new ListenToMe.Model.PageStore();
        //await store.LoadPages();

        // Look for the specified trip. The destination *should* be pulled from the grammar we
        // provided, and the subsequently updated phrase list, so it should be a 1:1 match, including case.
        // However, we might have multiple trips to the destination. For now, we just pick the first.
        //IEnumerable<ListenToMe.Model.Page> pages = store.Pages.Where(p => p.destination == destination);

        var userMessage = new VoiceCommandUserMessage();
        var destinationsContentTiles = new List<VoiceCommandContentTile>();
        if (pages.Count() == 0)
        {
            // In this scenario, perhaps someone has modified data on your service outside of your 
            // control. If you're accessing a remote service, having a background task that
            // periodically refreshes the phrase list so it's likely to be in sync is ideal.
            // This is unlikely to occur for this sample app, however.
            string foundNoTripToDestination = string.Format(
                   cortanaResourceMap.GetValue("FoundNoPageToDestination", cortanaContext).ValueAsString,
                   destination);
            userMessage.DisplayMessage = foundNoTripToDestination;
            userMessage.SpokenMessage = foundNoTripToDestination;
        }
        else
        {
            // Set a title message for the page.
            string message = "";
            if (pages.Count() > 1)
            {
                message = cortanaResourceMap.GetValue("PluralUpcomingPages", cortanaContext).ValueAsString;
            }
            else
            {
                message = cortanaResourceMap.GetValue("SingularUpcomingPage", cortanaContext).ValueAsString;
            }
            userMessage.DisplayMessage = message;
            userMessage.SpokenMessage = message;

            // file in tiles for each destination, to display information about the trips without
            // launching the app.
            foreach (ListenToMe.Model.Page page in pages)
            {
                int i = 1;

                var destinationTile = new VoiceCommandContentTile();

                // To handle UI scaling, Cortana automatically looks up files with FileName.scale-<n>.ext formats based on the requested filename.
                // See the VoiceCommandService\Images folder for an example.
                destinationTile.ContentTileType = VoiceCommandContentTileType.TitleWith68x68IconAndText;
                //destinationTile.Image = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///AdventureWorks.VoiceCommands/Images/GreyTile.png"));

                destinationTile.AppLaunchArgument = page.destination;
                destinationTile.Title = page.destination;
                /*if (page.StartDate != null) find out about motive here
                {
                    destinationTile.TextLine1 = page.StartDate.Value.ToString(dateFormatInfo.LongDatePattern);
                }
                else
                {
                destinationTile.TextLine1 = page.destination + " " + i;
                //}

                destinationsContentTiles.Add(destinationTile);
                i++;
            }
        }

        var response = VoiceCommandResponse.CreateResponse(userMessage, destinationsContentTiles);

        if (pages.Count() > 0)
        {
            response.AppLaunchArgument = destination;
        }

        await voiceServiceConnection.ReportSuccessAsync(response);
    }
*/
        /// <summary>
        /// creates a progeress screen in the cortana canvas
        /// </summary>
        /// <param name="myMessage"></param>
        /// <returns></returns>
        private async Task ShowProgressScreenAsync(string myMessage)
        {
            Debug.WriteLine(myMessage);
            var userProgressMessage = new VoiceCommandUserMessage();
            userProgressMessage.DisplayMessage = userProgressMessage.SpokenMessage = myMessage;
            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userProgressMessage);
            await voiceServiceConnection.ReportProgressAsync(response);
        }

        /// <summary>
        /// if app is suspended, removes network connections
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }
        /// <summary>
        /// launches the App ListenToMe in forground
        /// </summary>
        private async void LaunchAppInForegroundAsync()
        {
            var userMessage = new VoiceCommandUserMessage();
            //this is NullReferenceException "Object reference not set to an instance of an object."
            userMessage.SpokenMessage = cortanaResourceMap.GetValue("LaunchingListenToMe", cortanaContext).ValueAsString;

            var response = VoiceCommandResponse.CreateResponse(userMessage);

            response.AppLaunchArgument = "";

            await voiceServiceConnection.RequestAppLaunchAsync(response);
        }
        /// <summary>
        /// called if the exceution of <see cref="Run(IBackgroundTaskInstance)"/> is canceled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reason"></param>
        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            System.Diagnostics.Debug.WriteLine("Task cancelled, clean up");
            if (this.serviceDeferral != null)
            {
                //Complete the service deferral
                this.serviceDeferral.Complete();
            }
        }
    }
}





