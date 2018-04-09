using ClassLibrary.model;
using ListenToMe.View;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ListenToMe.ViewModel
{
    /// <summary>
    /// View Model controlling the behavior of a List view of Sections 
    /// </summary>
    public class SectionListViewModel : ViewModelBase
    {
        private ObservableCollection<Section> _Sections;
        private Section selectedSection;
        private Model.Form store;

        /// <summary>
        /// Construct the Section view, passing in the persistent Section store. Sets up
        /// a command to handle invoking the Add button.
        /// </summary>
        /// <param name="store"></param>
        public SectionListViewModel(Model.Form store)
        {
            this.store = store;
            _Sections = store.Sections;
            
        }

        /// <summary>
        /// The list of Sections to display on the UI.
        /// </summary>
        public ObservableCollection<Section> Sections
        {
            get
            {
                return _Sections;
            }
            private set
            {
                _Sections = value;
                NotifyPropertyChanged("Sections");
            }
        }

        /// <summary>
        /// A two-way binding that keeps reference to the currently selected Section on
        /// the UI.
        /// </summary>
        public Section SelectedSection
        {
            get
            {
                return selectedSection;
            }
            set
            {
                selectedSection = value;
                NotifyPropertyChanged("SelectedSection");
            }
        }
        

        /// <summary>
        /// Reload the Section store from data files.
        /// </summary>
        internal void LoadSections()
        {
            Debug.WriteLine("called LoadSections");
            //await store.LoadSections();
        }


        /// <summary>
        /// The implementation of the command to execute when the Add button is pressed.
        /// </summary>
        private void AddSection()
        {
            App.NavigationService.Navigate<SectionDetails>();
        }

        /// <summary>
        /// Handles a user selecting a Section on the UI by navigating to the SectionDetails view
        /// for that Section.
        /// </summary>
        internal void SelectionChanged()
        {
            if (SelectedSection != null)
            {
                App.NavigationService.Navigate<SectionDetails>(SelectedSection);
            }
        }
    }
}
