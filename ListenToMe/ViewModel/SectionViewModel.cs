using ClassLibrary.model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ListenToMe.ViewModel
{
    /// <summary>
    /// View Model associated with SectionDetails.xaml. Provides access to an individual Section,
    /// and Commands for saving new, updating existing, and deleting Sections.
    /// Is able to both create brand new Sections, and edit existing Sections, hiding buttons that
    /// should not be accessible in some cases.
    /// </summary>
    public class SectionViewModel : ViewModelBase
    {
        private Section section;
        private Model.Form store;
        private bool showDestinationValidation = false;
        private string destinationValidationError;
        private bool showDelete = false;

        /// <summary>
        /// The Section this view model represents.
        /// </summary>
        public Section Section
        {
            get
            {
                return section;
            }
            set
            {
                section = value;
                NotifyPropertyChanged("Section");
            }
        }

        /// <summary>
        /// We require that a destination be set to a non-empty string. If the user
        /// attempts to save without one, this will be set to an explanatory validation
        /// prompt to be rendered in the UI.
        /// </summary>
        public String DestinationValidationError
        {
            get
            {
                return destinationValidationError;
            }

            private set
            {
                destinationValidationError = value;
                NotifyPropertyChanged("DestinationValidationError");
            }
        }

        /// <summary>
        /// Controls whether the TextBlock that contains the destination validation string
        /// is visible. Combined with the VisibilityConverter, can show or collapse elements.
        /// </summary>
        public bool ShowDestinationValidation
        {
            get
            {
                return showDestinationValidation;
            }
            private set
            {
                showDestinationValidation = value;
                NotifyPropertyChanged("ShowDestinationValidation");
            }
        }

        /// <summary>
        /// Controls whether the Button that deletes Sections is shown. If creating a new Section,
        /// this is false. Otherwise, true.
        /// </summary>
        public bool ShowDelete
        {
            get
            {
                return showDelete;
            }
            private set
            {
                showDelete = value;
                NotifyPropertyChanged("ShowDelete");
            }
        }
        /*
        /// <summary>
        /// Bound to the save button, provides a command to invoke when pressed.
        /// </summary>
        public ICommand SaveSectionCommand
        {
            get
            {
                return saveSectionCommand;
            }
        }
        */
        /// <summary>
        /// Load a Section fomr the store, and set up the view per a normal ShowSection command.
        /// </summary>
        /// <param name="destination"></param>
        internal async void LoadSectionFromStore(string destination)
        {
            //4th heading in section contains the section name 
            Section t = store.Sections.Where(p => p.InputsAndHeadings[4].Text == destination).FirstOrDefault();
            if (t != null)
            {
                this.ShowSection(t);
            }
            else
            {
                // Redirect back to the main page, and pass along an error message to show
                await Window.Current.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    App.NavigationService.Navigate<View.SectionListView>(
                    string.Format(
                        "Sorry, couldn't find a Section with Destination {0}",
                        destination));
                });
            }
        }

        /*
        /// <summary>
        /// Bound to the Delete button, provides a command to invoke when pressed.
        /// </summary>
        public ICommand DeleteSectionCommand
        {
            get
            {
                return deleteSectionCommand;
            }
        }
        */
        /// <summary>
        /// Construct Section ViewModel, providing the store to persist Sections. 
        /// Creates the RelayCommands to be bound to various buttons in the UI.
        /// </summary>
        /// <param name="store">The persistent store</param>
        public SectionViewModel(Model.Form store)
        {
            Section = new Section();
           // saveSectionCommand = new RelayCommand(new Action(SaveSection));
           // deleteSectionCommand = new RelayCommand(new Action(DeleteSection));
            this.store = store;
        }

        /// <summary>
        /// removes a Section from the store, and returns to the Section list.
        /// </summary>
        private async void DeleteSection()
        {
            //await store.DeleteSection(Section);
            await UpdateDestinationPhraseList();
            App.NavigationService.GoBack();
        }

        /// <summary>
        /// Whenever a Section is modified, we trigger an update of the voice command Phrase list. This allows
        /// voice commands such as "Adventure Works Show Section to {destination} to be up to date with saved
        /// Sections.
        /// </summary>
        public async Task UpdateDestinationPhraseList()
        {
            try
            {
                // Update the destination phrase list, so that Cortana voice commands can use destinations added by users.
                // When saving a Section, the UI navigates automatically back to this page, so the phrase list will be
                // updated automatically.
                Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinition commandDefinitions;

                string countryCode = CultureInfo.CurrentCulture.Name.ToLower();
                if (countryCode.Length == 0)
                {
                    countryCode = "en-us";
                }

                if (Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstalledCommandDefinitions.TryGetValue("AdventureWorksCommandSet_" + countryCode, out commandDefinitions))
                {
                    List<string> destinations = new List<string>();
                    foreach (Section t in store.Sections)
                    {
                        //4th heading in section contains section heading
                        string longest = GetLongestWordInSection(t.InputsAndHeadings[4].Text);
                        destinations.Add(longest);
                    }

                    await commandDefinitions.SetPhraseListAsync("destination", destinations);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Updating Phrase list for VCDs: " + ex.ToString());
            }
        }
        /// <summary>
        /// Since PhaseList my only be updated with one word, filter the longest word from the herding
        /// </summary>
        /// <param name="t">the Section whose </param>
        /// <returns></returns>
        private string GetLongestWordInSection(string heading)
        {
            var punctuation = heading.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = heading.Split().Select(x => x.Trim(punctuation));
            return words.OrderByDescending(s => s.Length).First();

        }


        /// <summary>
        /// Performs validation on the destination to ensure it's not empty, then 
        /// saves a Section to the store. If the destination isn't valid, shows a validation
        /// error.
        /// </summary>
        private async void SaveSection()
        {
            ShowDestinationValidation = false;
            bool valid = true;

            if (String.IsNullOrEmpty(Section.InputsAndHeadings[4].Text))
            {
                valid = false;
                ShowDestinationValidation = true;
                DestinationValidationError = "Section heading cannot be blank";
            }
            else
            {
                Section.InputsAndHeadings[4].Text = Section.InputsAndHeadings[4].Text.Trim();
            }

            if (valid)
            {
                //await store.SaveSection(this.Section);
                await App.client.UploadSectionAsync(this.Section);
                await UpdateDestinationPhraseList();
                App.NavigationService.GoBack();
            }
        }

        /// <summary>
        /// Sets up the view model to show an existing Section.
        /// </summary>
        /// <param name="Section"></param>
        internal void ShowSection(Section section)
        {
            showDelete = true;
            Section = section;
        }

        /// <summary>
        /// Sets up the view model to create a new Section.
        /// </summary>
        internal void NewSection()
        {
            showDelete = false;
            Section = new Section();
        }
    }
}
