using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.Media.SpeechRecognition;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;
using System.Threading.Tasks;
using System.Diagnostics;
using ListenToMe.Common;
using System.Text.RegularExpressions;
using Windows.Security.Credentials;
using ListenToMe.Model;
using ListenToMe.ServiceReferenceFBK;

namespace ListenToMe
{
    /// <summary>
    /// contains application specific methods for starting the application. Initializations and state-related actions are includes.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// saves the adresses of the input fields present in the frame; this helps identifying the control in which to display the textvalue recognized by Luis
        /// </summary>
        public static System.Collections.Hashtable hashTable;
        /// <summary>
        /// Navigation service, provides a decoupled way to trigger the UI Frame
        /// to transition between views.
        /// </summary>
        public static NavigationService NavigationService { get; private set; }
        /// <summary>
        /// connects via DirectLine tho the WebApp. Communicates with the language understanding intelligence model
        /// </summary>
        public static Bot Bot;
        /// <summary>
        /// the WCF-Service client instance that is able to parse the HTML-Form
        /// </summary>
        public static Service1Client client;
        /// <summary>
        /// The password vault stores application specific usernames and passwords encrypted
        /// </summary>
        public static PasswordVault Vault { get; internal set; }

        /// <summary>
        /// The rootframenavigation helper instance enables App to register to keayboard events related to navigation
        /// </summary>
        private RootFrameNavigationHelper rootFrameNavigationHelper;

        /// <summary>
        /// the userName field is a tempory storage for the user's name
        ///
        /// </summary>
        internal static string UserName { get; private set; }
        internal static string UserPassword { get; private set; }

        /// <summary>
        /// keeps a collection <see cref="Model.FormStore.Sections"/> of all the sections of the web form. is able to store and load sections in the device storage
        /// </summary>
        public static Model.Form formstore = new Model.Form();

        /// <summary>
        /// the uri of the form to be accessed. Warning: the client is highly form-specific and is searching for specific attributes and types. Similar generated
        /// forms that contain the same attributes and html-tags will work, but all other forms will not be parsed correctly.
        /// this form ist available in Polish and English as well. But the forms of these languages still contain 50% German so I'm neglecting these other languages.
        /// </summary>
        internal static readonly string uri = "http://10.150.50.21/formularservice/formular/A_FOREX_ANTRAG_ESF/appl/d556026e-991d-11e7-9fb1-27c0f1da4ec4/?lang=de";

        /// <summary>
        /// is Debug determines the mode in which to run the application. If the above form uri is sometimes not available due to maintenace
        /// this should be set to true. If the form is available, this should be false.
        /// </summary>
        internal static bool isDebug = true;

        /// <summary>
        /// the Dictionary is filled if the Debug mode is not enabled instead of sectionsList
        /// </summary>
        //internal static Dictionary<string, Section> sections;

        /// <summary>
        /// Initialized the Singleton application object. This is the first line of built code and the logic equivalent to main() that is WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            client = new Service1Client(new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None)
            {
                MaxReceivedMessageSize = 2147483647 //maximum possible buffer size is Int64.Value 9223372036854775807
            },
            new System.ServiceModel.EndpointAddress("http://localhost:59964/Service1.svc")

            );
            /** the MaxReceivedBufferSize is important because GetAllElements() returns a message 2^16*100 and 2^16 is the max default value 
             * reference https://stackoverflow.com/questions/16173835/wcf-error-the-server-did-not-provide-a-meaningful-reply
             */
        }

        /// <summary>
        /// Is called as the application is started normally by the user. Other enty points are used for example if the application is activated in the
        /// fore or background
        /// </summary>
        /// <param name="e">Details on application start and start arguments.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            MessageDialog message = new MessageDialog("OnLaunched was called.");
            //await message.ShowAsync();
            InitGui(e);
        }

        /// <summary>
        /// The method ActivateVoiceCommands is calling an VoiceCommands.xml-File from local storage to determine which Cortana commands are valid.
        /// It also is able to update the phrase lists page and sections. The problem with that is, though that the phrase lists are limited to one word.7
        /// For a voice command like "Open 'Angaben zum antragsstellenden Unternehmen'" this is problematic.
        /// </summary>
        private async void ActivateVoiceCommands()
        {
            try
            {
                Debug.Write("Let's search for xml in " + Package.Current.InstalledLocation.Name);
                StorageFile vcd = await Package.Current.InstalledLocation.GetFileAsync(@"VoiceCommands.xml");
                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcd);
                CortanaModelMethods meth = new CortanaModelMethods();
                List<String> headings = await meth.UpdatePhraseList("Page"); //this is supposed to load the headings from html file and add them as pages to voice command file
                List<String> inputs = await meth.UpdatePhraseList("Field"); //these UpdatePhraselist methods are not effective since prase lists support only one word items

            }
            catch (Exception ex)
            {
                Debug.Write("Failed to register custom commands because " + ex.Message);
            }
        }

        /// <summary>
        ///is called whenever an external application e.g- Cortana launches the ListenToMeApp
        /// </summary>
        protected async override void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);
            ListenToMeVoiceCommand navigationCommand = null; //vormals ?
            MessageDialog dialog = new MessageDialog("");

            // if the App was activated by a speech input
            if (e.Kind == ActivationKind.VoiceCommand)
            {
                VoiceCommandActivatedEventArgs cmdArgs = e as VoiceCommandActivatedEventArgs;

                SpeechRecognitionResult result = cmdArgs.Result;

                string commandName = result.RulePath[0];
                int len = result.RulePath.ToArray().Length;
                String rules = result.Text;
                Debug.Write("command found: " + commandName);
                dialog.Content = "You have a voice command activation " + rules;
                await dialog.ShowAsync();
                InitGui();//load default frames and page

                await PerformCommandAsync(commandName, rules);

            }
            //if the App was activated by a text input
            else if (e.Kind == ActivationKind.Protocol)
            {
                dialog.Content = "Activated by Protocol";
                await dialog.ShowAsync();

                // Extract the launch context. In this case, we're just using a field from the phrase set (passed
                // along in the background task inside Cortana), which makes no attempt to be unique. A unique id or 
                // identifier is ideal for more complex scenarios. We let the page check if the 
                // field still exists, and navigate back to the default if it doesn't.
                var commandArgs = e as ProtocolActivatedEventArgs;
                Windows.Foundation.WwwFormUrlDecoder decoder = new Windows.Foundation.WwwFormUrlDecoder(commandArgs.Uri.Query);
                var destination = decoder.GetFirstValueByName("LaunchContext");//debug this, commandargs is probably "name*" or something like that

                navigationCommand = new Model.ListenToMeVoiceCommand(
                                        "protocolLaunch",
                                        "text",
                                        "destination",
                                        destination);


                MessageDialog mymessage = new MessageDialog("Todo here some navigation by protocol ");
                await mymessage.ShowAsync();
                NavigationService.Navigate(typeof(DynamicPage), navigationCommand);
            }
            else
            {
                dialog.Content = "something else";
                await dialog.ShowAsync();
                // If we were launched via any other mechanism, fall back to the main page view.
                NavigationService.Navigate(typeof(LoginPage), navigationCommand);
            }

            // Repeat the same basic initialization as OnLaunched() above, taking into account whether
            // or not the app is already active.
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                App.NavigationService = new Common.NavigationService(rootFrame);

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            // Since we're expecting to always show a details page, navigate even if 
            // a content frame is in place (unlike OnLaunched).
            // Navigate to either the main trip list page, or if a valid voice command
            // was provided, to the details page for that trip.
            NavigationService.Navigate(typeof(MainPage), navigationCommand);
            // Ensure the current window is active
            Window.Current.Activate();

        }
        /// <summary>
        /// initGui determines whether or not there is an active Windows Frame. It creates one for the application if there wasn't one before.
        /// As soon as there exists a frame, the method can transfer launchArguments to it. This is curently not used but might be in the future
        /// interesting. The command Edit could by this concept get the launch argument "company details" and is navigating the frame to companydails page.
        /// </summary>
        /// <param name="e"></param>
        private void InitGui(LaunchActivatedEventArgs e = null)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
            // Nur sicherstellen, dass das Fenster aktiv ist.
            if (rootFrame == null)
            {
                // Frame erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
                App.NavigationService = new NavigationService(rootFrame);
                // Use the RootFrameNavigationHelper to respond to keyboard and mouse shortcuts.
                this.rootFrameNavigationHelper = new RootFrameNavigationHelper(rootFrame);

                /*
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Zustand von zuvor angehaltener Anwendung laden
                }*/

                // place frame in current window
                Window.Current.Content = rootFrame;

                // Determine if we're being activated normally, or with arguments from Cortana.
                if (string.IsNullOrEmpty(e.Arguments))
                {
                    if (Vault == null)
                        App.Vault = new PasswordVault();

                    try
                    {
                        var cred = App.Vault.Retrieve("ListenToMe", "fgeiss");
                        //if the user has already validated user credentials with the web form, skip login
                        App.UserName = "fgeiss";//not necessary
                        cred.RetrievePassword();
                        App.UserPassword = cred.Password;
                        rootFrame.Navigate(typeof(MainPage), "");
                    }
                    catch
                    {
                        // Launching normally.
                        rootFrame.Navigate(typeof(LoginPage), "");
                    }

                }
                else
                {
                    // Launching with arguments. We assume, for now, that this is likely
                    // to be in the form of "destination=<location>" from activation via Cortana.
                    if (e != null)
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
            }
            // Sicherstellen, dass das aktuelle Fenster aktiv ist
            Window.Current.Activate();
            //ActivateVoiceCommands();
        }

        /// <summary>
        /// performCommand performs the voice commmand it received from Cortana or the input text field.
        /// </summary>
        /// <param name="commandName">The name ot the command, e.g. edit. Valid names are found in ViceConnamds.xml</param>
        /// <param name="result">The speech recognition result in text form</param>
        /// <returns></returns>
        private async Task PerformCommandAsync(string commandName, String result)
        {

            Frame bigFrame = rootFrameNavigationHelper.Frame;//Window.Current.Content as Frame;
            MessageDialog message = new MessageDialog("sss" + bigFrame.Name);
            await message.ShowAsync();
            var page = (MainPage)bigFrame.Content;
            Frame subFrame = page.mainFrame;
            Window.Current.Content = bigFrame;
            MessageDialog dialog = new MessageDialog("");
            switch (commandName)
            {
                //do something
                case "Edit":
                    dialog.Content = "Edit. result.Text " + result;//result.RulePath[1];
                    Debug.WriteLine("found 2 edit command ");
                    await dialog.ShowAsync();
                    NavigationService nana = new NavigationService(subFrame);
                    var navPageType = typeof(MainPage);
                    break;
                case "Information":
                    VoiceCommandUserMessage messageM = new VoiceCommandUserMessage();
                    messageM.DisplayMessage = "You can say: 'Edit <Field xyz>' or 'Shutdown'";
                    Debug.WriteLine("found information command");
                    //Application.Current.Exit();
                    //return typeof(MainPage);
                    break;
                case "Upload":
                    dialog.Content = "upload command recognized";
                    Debug.WriteLine("found upload command");
                    await dialog.ShowAsync();
                    NavigationService.Navigate(typeof(MainPage));
                    break;
                case "Shutdown":
                    dialog.Content = "shut computer down.";
                    Debug.WriteLine("found shut down command");
                    await dialog.ShowAsync();
                    Application.Current.Exit();
                    break;
                default:
                    Debug.WriteLine("Couldn't find command name");
                    dialog.Content = "default of onActivated";
                    await dialog.ShowAsync();
                    NavigationService.Navigate(typeof(MainPage));
                    break;
            }
        }

        /// <summary>
        /// Is called when nvigation to a certain page fails
        /// </summary>
        /// <param name="sender">Frame that failed to navigate to the page</param>
        /// <param name="e">Details about navigation error</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// OnSuspending is called if the application execution is stopped. The application state is stored without knowing
        /// whether the application is terminated or will be called at a later time
        /// </summary>
        /// <param name="sender">Source of suspension.</param>
        /// <param name="e">Details on suspension.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Anwendungszustand speichern und alle Hintergrundaktivitäten beenden
            deferral.Complete();
        }

        /// <summary>
        /// Filter changed is supposed to filter every sign from input string that's not alphanumerical. Unfortunately,
        /// TextchangedEvent is only triggered between posts to the server, so this is inept try.
        /// see reference: https://msdn.microsoft.com/en-us/library/system.web.ui.webcontrols.textbox.textchanged(v=vs.110).aspx
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void Filter_Changed(object sender, TextChangedEventArgs e, String regex)
        {
            Debug.WriteLine("Text Changed ");
            var textboxSender = (TextBox)sender;
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "^(?!" + regex + ")$", "");
            textboxSender.SelectionStart = cursorPosition;

        }
    }
}

