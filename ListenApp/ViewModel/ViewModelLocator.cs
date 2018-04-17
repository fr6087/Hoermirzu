using ListenToMe.Model;
using System.Collections.Generic;
using System.Diagnostics;

namespace ListenToMe.ViewModel
{
    /// <summary>
    /// ViewModelLocator ensures that viewmodels can be instantiated with a common reference to the SectionStore. 
    /// This allows for easier decoupling of the store implementation and the view models, and allows for 
    /// less viewmodel specific code in the views.
    /// </summary>
    public class ViewModelLocator
    {
        private Dictionary<string, ViewModelBase> modelSet = new Dictionary<string, ViewModelBase>();

        /// <summary>
        /// Set up all of the known view models, and instantiate the Section repository.
        /// </summary>
        public ViewModelLocator()
        {
            Form store = new Form();
            InitializeStore(store);
            modelSet.Add("SectionListViewModel", new SectionListViewModel(store));
            modelSet.Add("SectionViewModel", new SectionViewModel(store));
        }

        private void InitializeStore(Form store)
        {
            Debug.WriteLine("Init store");
            //await store.LoadSections();
        }

        /// <summary>
        /// SectionList (main page) view model. The SectionListView is databound to this property.
        /// </summary>
        public SectionListViewModel SectionListViewModel
        {
            get
            {
                return (SectionListViewModel)modelSet["SectionListViewModel"];
            }
        }

        /// <summary>
        /// Section (detail page) view model. SectionDetails page is databound to this property.
        /// </summary>
        public SectionViewModel SectionViewModel
        {
            get
            {
                return (SectionViewModel)modelSet["SectionViewModel"];
            }
        }
    }
}
