using ListenToMe.Common;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.Media.SpeechRecognition;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Popups;
using System.Linq;
using ListenToMe.Model;
using ClassLibrary.model;
using System.Collections;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using ListenToMe.View;
using Windows.UI.Xaml.Data;
using System.Globalization;
using System.Reflection;

namespace ListenToMe
{
    /// <summary>
    /// contains the navigation buttons of the app as well as the speech input field
    /// </summary>
    public partial class MainPage : Page
    {
        /// <summary>
        /// Navigationhelper is administrating the history of the pages in the frame of the programm
        /// </summary>
        NavigationHelper navigationHelper;
        /// <summary>
        /// navigationService is controlling the rootframe changes whenever navigation occurs
        /// </summary>
        NavigationService navigationService;
        /// <summary>
        /// a variable that is true when the user is speaking to the speech input field and false if he is typing into the speech input field
        /// </summary>
        bool listening = false;
        /// <summary>
        /// a variable that is true when speech synthesis is generating output
        /// </summary>
        bool DisableSpeaking = false;
        /// <summary>
        /// rootframe navigation helper is helping the rootframe navigation with keyboard events
        /// </summary>
        private RootFrameNavigationHelper myFrameHelper;
        /// <summary>
        /// evaluates speech input if the user is continually speaking. triggered by the user saying start listening. If the command stop listening is said, it will stop continous speech recognition
        /// </summary>
        private SpeechRecognizer speechRecognizerContinuous;
        /// <summary>
        /// loads localized string ressources from Strings/ directory
        /// </summary>
        private ResourceLoader loader;
        /// <summary>
        /// counts the user's inputs for helping the app at guessing which field is filled out next.
        /// </summary>
        private int countUserInputs;
        /// <summary>
        /// evaluates speech input if the user is only speaking a defined interval (e.g while speech input button is pressed)
        /// </summary>
        private SpeechRecognizer speechRecognizer;
        /// <summary>
        /// this event is for c# speech recognition. if it is set() then the speech recognition is stopping
        /// </summary>
        private ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        /// <summary>
        /// variable that counts the sections to know which section has to be loades into the frame
        /// </summary>
        private int pagesCount;
        /// <summary>
        /// contains the TextBoxes and other controls of the page
        /// </summary>
        private StackPanel currentPanel;
       
        private StackPanel[] AllPanels;
        /// <summary>
        /// index of the last TextBox to which text was send with <see cref="SendMessage(string, bool)"/>
        /// </summary>
        private int indexOfCurrentElement = 0;
        /// <summary>
        /// storing wether the grid is visible for grid navigation
        /// </summary>
        public bool showGrid;
        
        /// <summary>
        /// flag for preventing the <see cref="Clicked(string, string)" method from being fired when the buttons are initialized/>
        /// </summary>
        private bool IsInitializing = true;
        private DisambiguateView dialog;
        /// <summary>
        /// used for text to speech synthesis
        /// </summary>
        private SpeechSynthesizer speechSyntesizer;

        public MainPage()
        {
            this.InitializeComponent();
            IsInitializing = true;
            App.Bot = new Bot();
            loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            pagesCount = 1;
            showGrid = false;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += NavigationHelper_LoadState;
            this.navigationHelper.SaveState += NavigationHelper_SaveState;
            
            SetWhereAmI(indexOfCurrentElement);
            mainFrame.Name = "mainFrame";
            myFrameHelper = new RootFrameNavigationHelper(mainFrame);
            navigationService = new NavigationService(mainFrame);
            App.hashTable = new System.Collections.Hashtable();
            InitFrame();
        }


        /// <summary>
        /// fills the section field in formstore with information from the web form
        /// </summary>
        private async void InitFrame()
        {
            ProgressRing.IsActive = true;
            try
            {
                await App.formstore.Fill_sections();//binding xml statt html?
            }
            catch (System.ServiceModel.CommunicationException e)
            {
                Debug.WriteLine("WCF Service is unavailable. Please rebuilt the service, then refresh Service Reference UWP App project.");
                Debug.WriteLine(e.Message);
                throw;
            }
            AllPanels = new StackPanel[App.formstore.Sections.Count];
            await FillFrameAsync();
            ProgressRing.IsActive = false;
        }

        /// <summary>
        /// toDo implement formstore call that saves the values of the input fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }
        /// <summary>
        /// toDo implement formstore call that loads the values of the input fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }
        /// <summary>
        /// is called after the login was sucessful
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.Write("MainPage_OnNavig");
            Media.MediaEnded += Media_MediaEnded;
            await InitContinuousRecognition();
            await InitTextToSpeech();

            if (e.Parameter != null && e.Parameter is bool) //if activated with voicecommand?
            {
                string response = App.client.TalkToTheBotAsync("Moin").Result;
                await SpeakAsync(response);
                await SetListeningAsync(true);
            }
            else if (e.Parameter != null && e.Parameter is string && !string.IsNullOrWhiteSpace(e.Parameter as string))//if activated with text?
            {
                SendMessage(e.Parameter as string, true);
                await SetListeningAsync(true);
            }
            navigationHelper.OnNavigatedTo(e);
        }

        private async Task InitTextToSpeech()
        {
            speechSyntesizer = new SpeechSynthesizer();
            //see https://stackoverflow.com/questions/29163994/adding-a-new-language-to-speechsynthesizer
            using (var speaker = new SpeechSynthesizer())
            {
                var availableVoices = SpeechSynthesizer.AllVoices;
                speaker.Voice = (availableVoices.First(x => x.Gender == VoiceGender.Male && x.Language.Contains(CultureInfo.CurrentCulture.Name)));
                if (speaker.Voice == null)
                {
                    MessageDialog dialog = new MessageDialog("The text to speech language for " + CultureInfo.CurrentCulture.Name + "is not installed in your device. Recommended: Change this in speech settings.");
                    await dialog.ShowAsync();
                }
                speechSyntesizer.Voice = speaker.Voice;
            }
        }

        /// <summary>
        /// is essential in processing the recognized LUIS-entities. Sometimes LUIS entities are not recognized by the language model.
        /// </summary>
        /// <param name="intent">the intent that LUIS recognizes. Is not importent, as this will be always Field.FillIn</param>
        /// <param name="textValue">the textvalue for the input field</param>
        /// <param name="usersFieldName">the dirty text type name the user used in the query. </param>
        /// <param name="luisTypeOfTextValue">the clean text type</param>
        /// <returns>error messages</returns>
        private async Task<string> DetermineResponse(string intent, string textValue, string usersFieldName, string luisTypeOfTextValue)//second entity contains 2nd type (recognized by LUIS
        {
            Debug.WriteLine(intent + " textWert: " + textValue + "Feldname " + usersFieldName);
            var page = mainFrame.Content as DynamicPage;

            List<string> possibleMatches = new List<string>();
            
            foreach (string key in App.hashTable.Keys)
            {
                string cleanString = Regex.Replace(key, @"[^a-zA-Z0-9]", "");//hyphons are ignored. If they have to be evaluated add '/-' behind the '9'
                if (Regex.IsMatch(usersFieldName.ToLower(), cleanString.ToLower()) | Regex.IsMatch(cleanString.ToLower(), usersFieldName.ToLower()))
                {
                    Debug.WriteLine(key);

                    possibleMatches.Add(key);
                }
            }
            if (!string.IsNullOrWhiteSpace(textValue))
                textValue = FirstCharToUpper(textValue);
            if (possibleMatches.Count == 1 && !String.IsNullOrWhiteSpace(possibleMatches.ElementAt(0)))
            {
                var box = App.hashTable[possibleMatches.ElementAt(0)] as TextBox;
                indexOfCurrentElement = currentPanel.Children.IndexOf(box);
                txtElement.Text = "Element: " + indexOfCurrentElement;
                box.Text = textValue;
            }
            else if (possibleMatches.Count > 1)
            {
                dialog = new DisambiguateView();
                dialog.Title = loader.GetString("DisambiguateField");

                for (int i = 0; i < possibleMatches.Count; i++)
                {
                    Button button = new Button();
                    button.Name = textValue;
                    button.Content = possibleMatches[i];
                    button.Padding = new Thickness(5, 5, 5, 5);
                    button.Click += new RoutedEventHandler(this.DynamicButton_Click);
                    dialog.OptionButtonsPanel.Children.Add(button);
                }
                IsInitializing = false;
                await dialog.ShowAsync();
            }
            return "done";
        }

        private void Clicked(string sender, string textValue)
        {
            if (String.IsNullOrWhiteSpace(textValue))
                throw new ArgumentException("textValue is empty or not existing");
            if (IsInitializing)
                return;
            //toDo activeTextBoxCount hochsetzen
            Debug.WriteLine("clicked " + sender);
            TextBox box = App.hashTable[sender] as TextBox;
            indexOfCurrentElement = currentPanel.Children.IndexOf(box);
            
            SetWhereAmI(indexOfCurrentElement);
            box.Text = textValue;
            dialog.Hide();
        }

        private void SetWhereAmI(int indexOfCurrentElement)
        {
            txtElement.Text = "Element: " + indexOfCurrentElement;
            txtSection.Text = "Section: " + pagesCount;
        }

        /// <summary>
        /// sets up continous speech recognition
        /// </summary>
        /// <returns></returns>
        private async Task InitContinuousRecognition()
        {
            try
            {
                if (speechRecognizerContinuous == null)
                {
                    speechRecognizerContinuous = new SpeechRecognizer();
                    speechRecognizerContinuous.Constraints.Add(
                        new SpeechRecognitionListConstraint(
                            new List<String>() { loader.GetString("StartListening") }, loader.GetString("start")));
                    SpeechRecognitionCompilationResult contCompilationResult =
                        await speechRecognizerContinuous.CompileConstraintsAsync();


                    if (contCompilationResult.Status != SpeechRecognitionResultStatus.Success)
                    {
                        throw new Exception();
                    }
                    speechRecognizerContinuous.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
                }
                await speechRecognizerContinuous.ContinuousRecognitionSession.StartAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                if (args.Result.Text == loader.GetString("StartListening"))
                {
                    await Media.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await SetListeningAsync(true);
                    });
                }
            }
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            manualResetEvent.Set();
        }

        // give event away to class that is listening for keyboard shortcuts
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GoBackAsync();
        }

        private void GoBackAsync()
        {
            if (mainFrame.CanGoBack)
            {
                mainFrame.GoBack();
            }
            pagesCount--;
            currentPanel = AllPanels[pagesCount];
            SetWhereAmI(0);
        }

        /// <summary>
        /// fill Frame creates a new Page for mainFrame and adds it to the mainFrame as Content
        /// </summary>
        /// <param name="rnd">random element signalling background Page's colour</param>
        private async Task FillFrameAsync()
        {
            Debug.WriteLine(pagesCount);
            if(pagesCount-1>=0 && pagesCount-1<App.formstore.Sections.Count &&currentPanel!=null)
                AllPanels[pagesCount-1] = currentPanel;//store the old stackpanel
            ProgressRing.IsActive = true;
            currentPanel = new StackPanel();//init new section
            TextBlock formNumber = new TextBlock();
            countUserInputs = 0;
            App.hashTable = new Hashtable();//delete old textbox keys that may exist from previous sections

            //now that app knows all possible sections
            //check if loop is correct
            if (pagesCount >= App.formstore.Sections.Count)
            {
                //toDo: change this in optionsdialog with "send"-option
                MessageDialog message = new MessageDialog("page " + pagesCount + " reached. Sections count: " + App.formstore.Sections.Count);
                await message.ShowAsync();
                pagesCount = 1;
            }
            if (App.formstore.Sections.Count == 0)
                throw new Exception("Communication with the WCF client failed. It didn't return any sections in Form.fillSections()");

            if (pagesCount >= App.formstore.Sections.Count)
                throw new Exception("too few sections. Impossible to map pages count of " + pagesCount + " to a section when section count is " + App.formstore.Sections.Count);
            
            Debug.WriteLine(pagesCount);
            Section FormSection = App.formstore.Sections.ElementAt(0);
            Section sectionObject = App.formstore.Sections.ElementAt(pagesCount);
            await CreateSection(FormSection, currentPanel);
            await CreateSection(sectionObject, currentPanel);

            navigationService.Navigate(typeof(DynamicPage), currentPanel);
            ProgressRing.IsActive = false;
        }

        /// <summary>
        /// creates a section by adding control elements to a panel
        /// </summary>
        /// <param name="section">the section object that is being translated into controls</param>
        /// <param name="myPanel">the panel on which to place the controls</param>
        private async Task CreateSection(Section section, StackPanel myPanel)
        {
            Debug.WriteLine("createSection Section");
            //add elements to panel
            for (int j = 0; j < section.InputsAndHeadings.Length; j++)
            {
                await CreateElementAsync(section.InputsAndHeadings[j], myPanel);
            }
            Debug.WriteLine("");
        }

        /// <summary>
        /// creates an element from the section object by determining what type of element it is
        /// reference: https://stackoverflow.com/questions/37297810/updatesourcetrigger-dont-work-in-wpf-customcontrols
        /// </summary>
        /// <param name="element">The element from the section</param>
        /// <param name="panel">the panel on which to add the element</param>
        /// <param name="field">as label elements have subelements, field is used for recursive calls to know the parent object</param>
        private async Task CreateElementAsync(Element element, StackPanel panel, TextBox field = null)
        {
            string elemType = element.Type;
            Debug.Write(element.Type + " | ");
            var ElementBinding = new Binding();

            if (element.Type.Equals("div"))//add label to panel for form number text
            {
                TextBlock formnumber = new TextBlock();
                formnumber.TextWrapping = TextWrapping.Wrap;
                formnumber.Text = element.Text;
                panel.Children.Add(formnumber);
            }
            else if (element.Type.Equals("inn-checkbox"))
            {
                Windows.UI.Xaml.Controls.CheckBox box = new Windows.UI.Xaml.Controls.CheckBox();
                box.IsChecked = element.IsSeclected;
                ElementBinding.Source = element;
                ElementBinding.Path = new PropertyPath("IsSelected");
                ElementBinding.Mode = BindingMode.TwoWay;
                ElementBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                box.SetBinding(CheckBox.IsCheckedProperty, ElementBinding); //toDo: test this
                panel.Children.Add(box);
                //the text to checkbox is often followed by heading toDO ameliorate that
            }
            else if (element.Type.Equals("inn-button"))//the two buttons are used in the form to add table rows, to do think about how to translate that
            {
                Button button = new Button();
                button.Content = element.Text;
                panel.Children.Add(button);
            }
            else if (element.Type.Equals("inn-radiobutton"))
            {
                RadioButton button = new RadioButton();
                button.Content = element.Text;
                button.GroupName = element.Group_name;
                button.IsChecked = element.IsSeclected;

                //set a binding between the radioButton and the model
                ElementBinding.Source = element;
                ElementBinding.Path = new PropertyPath("IsSelected");
                ElementBinding.Mode = BindingMode.TwoWay;
                ElementBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                button.SetBinding(RadioButton.IsCheckedProperty, ElementBinding);

                panel.Children.Add(button);
                App.hashTable.Add(element.Text, button);
            }
            else if (element.Type.Equals("label"))
            {
                if (element.Subelems != null)
                {
                    field = new Windows.UI.Xaml.Controls.TextBox();
                    field.Header = element.Text;
                    foreach (var el in element.Subelems)
                    {
                        await CreateElementAsync(el, panel, field);
                    }

                }
                else //probably label for radio button
                {
                    TextBlock label = new TextBlock();
                    label.TextWrapping = TextWrapping.Wrap;
                    label.Text = element.Text;
                    panel.Children.Add(label);
                }
            }
            else if (element.Type.Equals("input"))
            {
                //set placeholder attribute
                string placeholder = element.Placeholder;
                if (String.IsNullOrWhiteSpace(placeholder))
                {
                    string comment = element.Comment;
                    if (!String.IsNullOrWhiteSpace(comment) && !comment.Equals("text field"))
                    {
                        field.PlaceholderText = comment;
                    }
                }
                else
                {
                    field.PlaceholderText = placeholder;
                }
                //toDo: set validator for required 
                //set maxlength attribute
                int number;
                if (Int32.TryParse(element.Maxlength, out number))
                {
                    field.MaxLength = number;
                }
                //set enabled property
                SetEnabledAndHashForField(field, element, panel);

                //set a binding between the textfield and the model
                ElementBinding.Source = element;
                ElementBinding.Path = new PropertyPath("Text");
                ElementBinding.Mode = BindingMode.TwoWay;
                ElementBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                field.SetBinding(TextBox.TextProperty, ElementBinding);

                panel.Children.Add(field);
            }
            else if (element.Type.Equals("dropdown"))
            {
                ComboBox dropdown = new ComboBox();

                //get the strings for the dropdown list from the Service1 client
                if (!String.IsNullOrEmpty(element.List_id))
                {
                    var DropdownLines = await App.client.GetCSVDropdownStringAsync(element.List_id, " ");

                    for (int i = 1; i < DropdownLines.Count; i++)
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = DropdownLines[i];
                        dropdown.Items.Add(item);
                    }
                    dropdown.SelectedItem = element.Text;
                }

                //set the placeholder text if one exists
                if (!String.IsNullOrWhiteSpace(element.Text))
                    dropdown.PlaceholderText = element.Text;

                //set the Header of the dropdown list if one exists
                if (!String.IsNullOrWhiteSpace(field.Header as string))
                {
                    dropdown.Header = field.Header;
                    App.hashTable.Add(field.Header, dropdown);
                }

                //set a binding between the selected ComboBoxItem and the model
                ElementBinding.Source = element;
                ElementBinding.Path = new PropertyPath("Text");
                ElementBinding.Mode = BindingMode.TwoWay;
                ElementBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                dropdown.SetBinding(ComboBox.SelectedItemProperty, ElementBinding);

                panel.Children.Add(dropdown);
            }
            else if (element.Type.Equals("heading"))
            {
                TextBlock heading = new TextBlock();
                heading.TextWrapping = TextWrapping.Wrap;
                heading.Text = element.Text;
                heading.FontSize = 20 - 2 * Double.Parse(element.Size); //toDo: test. this should be for h3 14, for h4 12 and h5 10
                panel.Children.Add(heading);
            }
        }

        /// <summary>
        /// sets the enabled property of a TextBox control to disabled, if the model Element's property is set to 'disabled' string. This is the case for input
        /// elements that are only for displaying information, e.g. summing up other entries made by the user. If the TextBox stays enabled,
        /// the header will be added to the HashTable that is used by the LUIS-Model to determine to which TextBox to pass a user value to.
        /// </summary>
        /// <param name="field">The TextBox to be set</param>
        /// <param name="element">the model insatnce that saved the future property valus of the field</param>
        /// <param name="panel">the panel on which all TextBoxes lie</param>
        private void SetEnabledAndHashForField(TextBox field, Element element, StackPanel panel)
        {
            if (string.IsNullOrWhiteSpace(field.Header as string))//if textbox has no label, then it's not possible to identify her via the hashtable. don't add her to hashtable.
            {
                return;
            }

            var enabled = element.Disabled;//is "disabled" as string if not enabled

            if (string.IsNullOrWhiteSpace(enabled))
            {
                var keys = App.hashTable.Keys;
                if (App.hashTable.ContainsKey(field.Header))//then that section has equally labeled input fields -> use later disambiguateview!
                {
                    App.hashTable.Add(field.Header + panel.Children.Count.ToString(), field);
                }
                else
                {
                    App.hashTable.Add(field.Header, field);
                }
            }
            else
            {
                field.IsEnabled = false;
            }
        }
        
        /// <summary>
        /// has no speacial meaning as of yet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            StackPanel[] panels = new StackPanel[App.formstore.Sections.Count - 1];
            //this button is clicked if the user wants to upload his form
            App.formstore.SaveSectionAsync(panels);
        }

        /// <summary>
        /// loads the next section into the frame
        /// </summary>
        /// <param name="sender">the next-button</param>
        /// <param name="e">some arguments</param>
        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            await GoNextAsync();
        }

        /// <summary>
        /// helps NextButton_Click to go to the next frame's page
        /// </summary>
        /// <returns></returns>
        private async Task GoNextAsync()
        {
            ++pagesCount;
            Debug.WriteLine(pagesCount);
            if (mainFrame.CanGoForward)
            {
                mainFrame.GoForward();//use Page Stack for Page creation if Stack history is available
                currentPanel = AllPanels[pagesCount];
            }
            else
            {
                await FillFrameAsync();//if Stack history is not available, create Page from Model
            }
            SetWhereAmI(0);//set active element to zero, set pagescount
        }

        /// <summary>
        /// Button is hit for microphone activation
        /// </summary>
        /// <param name="sender">the microphone button</param>
        /// <param name="e">some eventarguments</param>
        private void DynamicButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.Xaml.Controls.Button sourceButton = sender as Windows.UI.Xaml.Controls.Button;
            string textBoxName = sourceButton.Content as string;
            if (sourceButton == null | String.IsNullOrWhiteSpace(textBoxName))
                throw new ArgumentException("dynamicButton clicked but arguments were null.");
            Clicked(textBoxName, sourceButton.Name);
        }

        /// <summary>
        /// Button is hit for microphone activation
        /// </summary>
        /// <param name="sender">the microphone button</param>
        /// <param name="e">some eventarguments</param>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await SetListeningAsync(!listening);
        }

        /// <summary>
        /// sets the flag whether speech recognition is enabled according to the parameter
        /// </summary>
        /// <param name="toListen">the parameter. If true it sets the flag and activates listenmode for false vice versa</param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task SetListeningAsync(bool toListen)
        {
            if (toListen)
            {
                listening = true;
                text.IsEnabled = false;
                symbol.Symbol = Symbol.FontColor;
                text.PlaceholderText = loader.GetString("StartListening");

                if (speechRecognizerContinuous != null)
                {
                    await speechRecognizerContinuous.ContinuousRecognitionSession.CancelAsync();
                }

                StartListenMode();
            }
            else
            {
                listening = false;
                text.IsEnabled = true;
                symbol.Symbol = Symbol.Microphone;
                ProgressRing.IsActive = false;
                text.Text = "";
                text.PlaceholderText = loader.GetString("SpeechTypeOrSayPrompt");

                if (speechRecognizerContinuous != null)
                    await speechRecognizerContinuous.ContinuousRecognitionSession.StartAsync();
            }
        }

        /// <summary>
        /// analyzes the speech input, recognized the text and calls SendMessage() to display the rcognized text
        /// </summary>
        private async void StartListenMode()
        {
            while (listening)
            {
                string spokenText = await ListenForText();
                while (string.IsNullOrWhiteSpace(spokenText) && listening)
                    spokenText = await ListenForText();

                if (spokenText.ToLower().Contains(loader.GetString("StopListening")))
                {
                    speechRecognizer.UIOptions.AudiblePrompt = loader.GetString("SpeechConfirmationStopListen");
                    speechRecognizer.UIOptions.ExampleText = loader.GetString("SpeechOptions");
                    speechRecognizer.UIOptions.ShowConfirmation = false;
                    //SpeakAsync(speechRecognizer.UIOptions.AudiblePrompt);
                    var result = await speechRecognizer.RecognizeWithUIAsync();

                    if (!string.IsNullOrWhiteSpace(result.Text) && result.Text.ToLower() == loader.GetString("yes"))
                    {
                        await SetListeningAsync(false);
                    }
                }

                if (listening)
                {
                    SendMessage(spokenText, true);
                }
            }
        }

        /// <summary>
        /// calls the LUIS API to retrieve the Bot WebApp's answer for a specific request
        /// </summary>
        /// <param name="message">the request</param>
        /// <param name="speak">flag that indicates whether the funtion is called in speech mode or text mode</param>
        private async void SendMessage(string message, bool speak = false)
        {
            countUserInputs++;
            Debug.WriteLine(countUserInputs + ". sending: " + message);

            //get direct answer from LuisModel 
            Rootobject response = await App.Bot.SendMessageAndGetIntentFromBot(message); //this works as well but directline more flexible than 

            String intent = response.topScoringIntent.intent;
            if (!intent.Equals("Field.FillIn"))
            {
                Debug.WriteLine("Entity Length is: " + response.entities.Length);
                CheckOtherIntents(intent, message, speak);
            }
            else if (intent.Equals("Field.FillIn") && response.entities.Length < 2)
            {
                MessageDialog dialog = new MessageDialog("your intent was: " + intent + " and too few entities were recognized by language model.");
                await dialog.ShowAsync();
            }
            else
            {
                String fieldname = "";
                String fieldvalue = "";
                String typeOfFieldValue = "";
                for (int i = 0; i < response.entities.Length; i++)
                {
                    if (response.entities[i].type.ToLower().Equals("feldname"))
                    {
                        fieldname = response.entities[i].entity;
                    }
                    else//wenn es kein feldname ist, ist es wahrscheinlich eine feldinhalt-Entität
                    {
                        fieldvalue = response.entities[i].entity;
                        typeOfFieldValue = response.entities[i].type;
                    }
                }

                await DetermineResponse(intent, fieldvalue, fieldname, typeOfFieldValue);
                var Sections = App.formstore.Sections;
            }

            if (speak)
            {
                Debug.WriteLine("starting to speak");
                //await SpeakAsync(response);
                Debug.WriteLine("done speaking");

            }
        }

        /// <summary>
        /// helps calling actions of intents other than Field.FillIn
        /// </summary>
        /// <param name="intent">the name of the intent</param>
        private async void CheckOtherIntents(string intent, string message, bool speak)
        {
            var controls = currentPanel.Children.AsQueryable();
            MessageDialog Toast = new MessageDialog("");
            switch (intent)
            {
                case "Utilities.Help":
                     Toast.Content = loader.GetString("HelpHint");
                    await Toast.ShowAsync();
                    break;
                case "Utilities.ShowNext":
                    await GoNextAsync();
                    break;
                case "Section.Read":
                    await ReadSection(controls);
                    break;
                case "Utilities.ShowPrevious":
                    GoBackAsync();
                    break;
                case "Utilities.Stop":
                    DisableSpeaking = true;
                    break;
                case "Field.Next":
                    JumpToNextField(intent, message, speak);
                    break;
                default:
                    Toast.Content = loader.GetString("HelpClarify");
                    await Toast.ShowAsync();
                    break;
            }
        }

        /// <summary>
        /// sets the focus to the next fields and writes message into it or delegates to the other intents
        /// </summary>
        /// <param name="intent">Field.FillIn intent. If not then CheckIntents will be called again</param>
        /// <param name="message">the message the user supposedly wants in the form field</param>
        /// <param name="speak">whether speech is activated or not</param>
        private void JumpToNextField(string intent, string message, bool speak)
        {
            if (message.StartsWith(loader.GetString("nextField")))
            {
                int firstWhitespace = message.IndexOf(' ');
                if (firstWhitespace > -1)
                    message = message.Substring(firstWhitespace).Trim();
            }

            var Element = currentPanel.Children.ElementAt(++indexOfCurrentElement);
            txtElement.Text = "Element: " + indexOfCurrentElement;
            TextBlock block = Element as TextBlock;
            if (block != null && indexOfCurrentElement < currentPanel.Children.Count)//ignore headings
            {
                Debug.WriteLine("heading");
                indexOfCurrentElement = currentPanel.Children.IndexOf(block);
                CheckOtherIntents(intent, message, speak);//recursive call to jump over headings. they can't be "filled out"
                
            }
            TextBox box = Element as TextBox;
            RadioButton rb = Element as RadioButton;
            CheckBox ch = Element as CheckBox;
            ComboBox cb = Element as ComboBox;
            if (box != null)
            {
                //set the message text into textbox
                box.Text = FirstCharToUpper(message);//should write "weiter" or "nächstes"
                indexOfCurrentElement = currentPanel.Children.IndexOf(box);
                Debug.WriteLine("textbox");
            }
            else if (rb != null)
            {
                rb.IsChecked = message.Contains("select");
                indexOfCurrentElement = currentPanel.Children.IndexOf(rb);
                Debug.WriteLine("rb");
            }
            else if (ch != null)
            {
                ch.IsChecked = message.Contains("select");
                indexOfCurrentElement = currentPanel.Children.IndexOf(ch);
                Debug.WriteLine("ch");
            }
            else if (cb != null)
            {
                int x = 0;

                if (Int32.TryParse(message, out x))
                {
                    // you know that the parsing attempt
                    // was successful
                    cb.SelectedIndex = x;
                };
                indexOfCurrentElement = currentPanel.Children.IndexOf(cb);
            }
        }
        /// <summary>
        /// reads a section to the user. reading can only be stopped, if user types "shut up" or other utterance for Utilities.Stop intent
        /// </summary>
        /// <param name="controls">the controls in the current section</param>
        /// <returns></returns>
        private async Task ReadSection(IQueryable<UIElement> controls)
        {
            DisableSpeaking = false;   
            foreach(var control in controls)
            {
                if (DisableSpeaking)
                    return;
                Type Type = control.GetType();
                string TypeName = Type.Name;
                PropertyInfo propertyInfo = Type.GetProperty("Header");
                PropertyInfo propertyInfText = Type.GetProperty("Text");//in TextBlocks the text is the description, in TextBoxes the value
                PropertyInfo propertyInfoRB = Type.GetProperty("Content");//in rby the text is in the content
                PropertyInfo propertyInfoRBVal = Type.GetProperty("IsChecked");

                var header = (propertyInfo!=null)? propertyInfo.GetValue(control, null):"";
                var textProp = (propertyInfText!=null)? propertyInfText.GetValue(control, null):"";
                var textRB = (propertyInfoRB != null) ? propertyInfoRB.GetValue(control, null) : "";
                var textRBState = (propertyInfoRBVal != null) ? propertyInfoRBVal.GetValue(control, null) : "";

                //this is maybe easier as switch case
                //speak the heading of a textbox, if conrol is textbox
                switch (TypeName)
                {
                    case "TextBox":
                        await SpeakAsync(header as string);
                        await SpeakAsync(textProp as string);
                        break;
                    case "ComboBox":
                        await SpeakAsync(header as string);
                        PropertyInfo propertyInfoDrop = Type.GetProperty("SelectedItem");
                        var item = propertyInfoDrop.GetValue(control, null) as ComboBoxItem;
                        if (item != null)
                            await SpeakAsync(item.Content as string);
                        break;
                    case "TextBlock":
                        await SpeakAsync(textProp as string);
                        break;
                    case "RadioButton":
                        await SpeakAsync(textRB as string);
                        await SpeakAsync(textRBState as string);
                        break;
                    case "CheckBox":
                        await SpeakAsync(textRBState as string);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// lets the App speak the answer to the user
        /// </summary>
        /// <param name="toSpeak">the answer to be spoken</param>
        private async Task SpeakAsync(string toSpeak)
        {
            if (string.IsNullOrWhiteSpace(toSpeak)){
                return;
            }
            text.Text = loader.GetString("Speaking");
            SpeechSynthesisStream syntStream = await speechSyntesizer.SynthesizeTextToStreamAsync(toSpeak);
            Media.SetSource(syntStream, syntStream.ContentType);

            Task t = Task.Run(() =>
            {
                manualResetEvent.Reset();
                manualResetEvent.WaitOne();
            });

            await t;
            text.Text = "";
        }
        /// <summary>
        /// handles exceptions in the speechrecognition api. Also checks whether the device the user in using has enabled microphone input
        /// </summary>
        /// <returns></returns>
        private async Task<string> ListenForText()
        {
            string result = "";
            await InitSpeech();
            try
            {
                ProgressRing.IsActive = true;
                text.Text = loader.GetString("Listening");
                SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeAsync();
                if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
                {
                    result = speechRecognitionResult.Text;
                }
            }
            catch (Exception ex)
            {
                // Define a variable that holds the error for the speech recognition privacy policy. 
                // This value maps to the SPERR_SPEECH_PRIVACY_POLICY_NOT_ACCEPTED error, 
                // as described in the Windows.Phone.Speech.Recognition error codes section later on.
                const int privacyPolicyHResult = unchecked((int)0x80045509);

                // Check whether the error is for the speech recognition privacy policy.
                if (ex.HResult == privacyPolicyHResult)
                {
                    MessageDialog dialog = new MessageDialog(loader.GetString("PermissionInvalid"));
                    await dialog.ShowAsync();
                    await SetListeningAsync(false);
                }
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                ProgressRing.IsActive = false;
                text.Text = "";
            }
            return result;
        }
        /// <summary>
        /// initialized speech recognition
        /// </summary>
        private async Task InitSpeech()
        {
            if (speechRecognizer == null)
            {
                try
                {
                    speechRecognizer = new SpeechRecognizer();

                    SpeechRecognitionCompilationResult compilationResult = await speechRecognizer.CompileConstraintsAsync();
                    speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;

                    if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
                        throw new Exception();

                    Debug.WriteLine("SpeechInit AOK");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SpeechInit Failed" + ex.Message);
                    speechRecognizer = null;
                }
            }
        }
        /// <summary>
        /// helper method for cleaning output from luis. Since LUIS answers in lowercase only,
        /// this method uppercases the first letter
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FirstCharToUpper(string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default:
                    break;
            }
            //were this not NET-Core project this can be solved more elegantly with toTitleCase(), see https://stackoverflow.com/questions/72831/how-do-i-capitalize-first-letter-of-first-name-and-last-name-in-c
            MatchCollection Matches = Regex.Matches(input, "\\b\\w");
            //Phrase = input.ToLower();
            bool beginning = true;
            foreach (Match Match in Matches)
            {
                if (beginning)
                {
                    input = input.Remove(Match.Index, 1).Insert(Match.Index, Match.Value.ToUpper());

                    beginning = false;
                }
            }
            return input;
        }

        /// <summary>
        /// handles the HypothesisGenerated event of the speech recognizer
        /// </summary>
        /// <param name="sender">the speech recognizer</param>
        /// <param name="args">arguments that may be passed</param>
        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            await text.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                text.Text = args.Hypothesis.Text;
            });
        }

        /// <summary>
        /// Method that is reacting each time a key is hit while the textinput field has focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Text_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            Windows.UI.Xaml.Controls.TextBox box = (Windows.UI.Xaml.Controls.TextBox)sender;
            if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrWhiteSpace(box.Text))
            {
                SendMessage(box.Text);
                box.Text = "";
            }
        }


    }


}
