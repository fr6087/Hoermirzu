using ClassLibrary.model;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ListenToMe.Model
{
    /// <summary>
    /// able to store Sections of the form.
    /// <reference>AdventureWorks in UWP sample projects at github</reference>
    ///</summary>
    public class Form

    {

        /// <summary>
        /// flag to determine whether the page is loaded or not
        /// </summary>
        //private bool loaded;

        /// <summary>
        /// Persist the loaded fields in memory for use in other parts of the application.
        /// </summary>
        private ObservableCollection<ClassLibrary.model.Section> sections;
        /// <summary>
        /// helps storing the sections collection. 
        /// </summary>
        public ObservableCollection<ClassLibrary.model.Section> Sections { get => sections; set => sections = value; }


        public Form()
        {
           // loaded = false;
            Sections = new ObservableCollection<ClassLibrary.model.Section>();
        }



        /// <summary>
        /// calls the wcf service to dertermine the structure of the form. Then, it separates the structure into sections and binds them to a static attibute
        /// that's globally available in ListenToMe App. To which object it outputs depends on the debug mode. If debug is enabled, it will write to sectionsList.
        /// If not it will write to a Dictionary.
        /// </summary>
        public async Task Fill_sections()
        {
            string sth = App.UserPassword;
            ObservableCollection<Section> fieldsOnFormAsList = await App.client.GetAllElementsAsync("fgeiss", App.UserPassword);

            Sections = fieldsOnFormAsList;

        }

        internal void SaveSectionAsync(StackPanel[] myPanel)
        {
            /*if (sectionIndex < 0)
            {
                throw new ArgumentException("section's index may not be smaller than zero");
            }


            Element[] elements = Sections.ElementAt(sectionIndex).InputsAndHeadings;

            if (elements.Length != myPanel.Children.Count)
                Debug.WriteLine("elements has children: "+elements.Length+"panel hs children "+myPanel.Children.Count()); //but since we are adding three elements from sections.ElementAt(0) to stackpanel, this is normal

            for (int i = 0; i < elements.Length; i++)//loop through inputsAndHeadings Array
            {
                Element element = elements[i];
                int elementIndex = 0;
                if (element.Subelems != null)
                    element = element.Subelems.First();

                elementIndex = sections.ElementAt(0).Count() + i;
                if (!(elementIndex < myPanel.Children.Count))
                {
                    throw new ArgumentException("Element index "+ elementIndex+" is greater than" + myPanel.Children.Count);
                    
                }

                UIElement control = myPanel.Children.ToList<UIElement>().ElementAt(elementIndex);//ignore the 3 FormHeadings
                Element elementWithText = matchTextToControl(control, element);
                elements[i] = elementWithText;
            }
            Debug.WriteLine("saved back to Model " + elements.Length + " elements");
            Sections.ElementAt(sectionIndex).InputsAndHeadings = elements;*/

        }

        private Element MatchTextToControl(UIElement element, Element jsonElement)
        {
            UIElement control;

            control = element as TextBox;
            if (control != null)
            {
                jsonElement.Text = (element as TextBox).Text;
                return jsonElement;
            }
            control = element as RadioButton;
            if (control != null)
            {
                jsonElement.Group_name = (element as RadioButton).GroupName;
                jsonElement.IsSeclected = (element as RadioButton).IsChecked;
                return jsonElement;
            }
            control = element as DatePicker;
            if (control != null)
            {
                jsonElement.Text = (control as DatePicker).Date.ToString();
                return jsonElement;
            }
            control = element as CheckBox;
            if (control != null)
            {
                jsonElement.IsSeclected = (element as CheckBox).IsChecked;
                return jsonElement;
            }
            control = element as ComboBox;
            if (control != null)
            {
                var item = (element as ComboBox).SelectedValue as ComboBoxItem;

                jsonElement.Text = (item == null) ? jsonElement.Text : item.Content as string;
                return jsonElement;
            }
            return jsonElement;
        }
    }
}
