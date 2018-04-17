using ClassLibrary.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace ListenToMe
{
    /// <summary>
    /// This is a page that can be dynamically filled in MainPage. It is used to display the components that the wcf service finds per section on the form
    /// </summary>
    public sealed partial class DynamicPage : Page
    {
        StackPanel myPanel { get; set; }
        public DynamicPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        void saveSection(int index)
        {
            Section section = App.formstore.Sections.ElementAt<Section>(index);
            //textbox texte an die richtigen inputsAndHeadings elemente binden
         
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(myPanel); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(myPanel, i);

                if (child is TextBox)
                {
                    Debug.WriteLine("TextBox found at " + i);
                    TextBox box = child as TextBox;
                    section.InputsAndHeadings[i].Text = box.Text;
                }
                else if (child is DatePicker)
                {
                    Debug.WriteLine("DatePicker found at "+ i);
                    DatePicker picker = child as DatePicker;
                    section.InputsAndHeadings[i].Text = picker.Date.ToString();
                }
            }
            Section[] secs = App.formstore.Sections.ToArray<Section>();
            secs[index] = section;
            //App.formstore.Sections = secs.ToList<Section>();
            //try to cast this to DatePicker
            //try to cast this to radiobutton, checkbox or dropdown menu
            //App.formstore.Sections.ElementAt<Section>(index)=section;


            //section zurückspeichern in den Formstore
        }



        /// <summary>
        /// OnNavigatedTo fills the DynamicPages's Grid element with a stackpanel that contains all fields of a section
        /// </summary>
        /// <param name="e">the stackpanel provided by NavigationService.Navigate(DynamicPage, stackpanel)-method</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            myPanel = e.Parameter as StackPanel;
            LayoutRoot.Children.Clear();
            LayoutRoot.Children.Add(myPanel);
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// called when the page is unloaded. Since there is navigation whenever the 'next' button is hit the Layout grid has to be deleted.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            LayoutRoot.Children.Clear();
            base.OnNavigatedTo(e);
        }
    }
}
