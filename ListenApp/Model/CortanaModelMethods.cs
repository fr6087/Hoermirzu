using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;

namespace ListenToMe
{
    /// <summary>
    /// Contains methods that update the cortana voice command definition file to make it more dynamic.
    /// Note: This doesn't work as intendet, because phrase lists are only allowing one word entries and the form headings are mostly more than one word long.
    /// </summary>
    class CortanaModelMethods : IModelMethods
    {
        /// <summary>
        /// stores the headings that the wcf service finds on the form
        /// </summary>
        internal ObservableCollection<string> headings= new ObservableCollection<string>();
        /// <summary>
        /// stores the input field labels that the wcf service finds on the form
        /// </summary>
        internal ObservableCollection<string> inputs = new ObservableCollection<string>();
        
        /// <summary>
        /// reference TripViewModel.UpdateDestinationPhraseList in AdventureWorks project
        /// called when App Activates VoiceCommands, this method fills the collections of tags from the html Form and updates
        /// the VoiceCommand-xml File
        /// </summary>
        public async Task<List<String>> UpdatePhraseList(String phraselistName)
        {
            Debug.WriteLine("calling UpdatePraseList at CortanaModelMethods");
            try
            {
                // Update the destination phrase list, so that Cortana voice commands can use destinations added by users.
                // When saving a trip, the UI navigates automatically back to this page, so the phrase list will be
                // updated automatically.
                VoiceCommandDefinition commandDefinitions;

                string countryCode = CultureInfo.CurrentCulture.Name.ToLower();
                if (countryCode.Length == 0)
                {
                    countryCode = "de-de";//en-us
                }
                else
                {
                    Debug.WriteLine("COUNTRYCODE: " + countryCode);
                }
                Debug.WriteLine(VoiceCommandDefinitionManager.InstalledCommandDefinitions.Count());

                var dic = VoiceCommandDefinitionManager.InstalledCommandDefinitions;
                /*foreach(var keyValPair in dic)
                {
                    Debug.WriteLine(keyValPair.Key + keyValPair.Value);
                }*/
                if (VoiceCommandDefinitionManager.InstalledCommandDefinitions.TryGetValue("ListenToMeCommandSet_"+countryCode, out commandDefinitions))
                {
                    
                    ObservableCollection<String> observable = new ObservableCollection<string>();
                    if (phraselistName.Equals("Page"))
                    {
                        headings = await App.client.GetSpecificElementsAsync(App.UserName, App.UserPassword, "//*[self::h3]");
                        foreach (String item in headings)
                        {
                            string[] Words = item.Split(' ');
                            //sorts a string array by longest word first with O(n)
                            var Collection = Words.Aggregate(string.Empty, (seed, f) => f.Length > seed.Length ? f : seed);
                            observable.Add(Collection);

                        }
                        observable = headings;
                    }   
                    else if (phraselistName.Equals("Field"))
                    {
                        //toDo: gibt es eine englische Variante dieses Formulars? ->ja, aber ungenießbar.
                        observable = new ObservableCollection<string>(App.hashTable.Keys.Cast<string>());
                    }
                    List<string> items = observable.ToList();
                    //toDo: extract this from client
                    foreach (String item in items)
                    {
                        Debug.WriteLine("--praselist: "+item);

                    }
                    await commandDefinitions.SetPhraseListAsync(phraselistName, items);
                    return items;

                }
                return null;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Updating Phrase list for VCDs: " + ex.ToString());
                return null;
            }
        }


    }
}
