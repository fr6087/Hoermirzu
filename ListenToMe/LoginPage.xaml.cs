using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace ListenToMe
{
    /// <summary>
    /// retrieves and stores login Information in a password vault
    /// toDo: use this for directLine secret as well
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        /// <summary>
        /// names the resourcename for the passwordvault. 
        /// </summary>
       // private string resourceName = "ListenToMe";

        /// <summary>
        /// retreives the credentials from the list and populates the GUI with them, sets the cursor
        /// </summary>
        public LoginPage()
        {
            this.InitializeComponent();

            UserName.Text = "fgeiss";
            UserName.SelectionStart = UserName.Text.Length; // add some logic if length is 0
            UserName.SelectionLength = 0;
            
            if (App.Vault == null)
                App.Vault = new PasswordVault();

            try
            {
                var readonlylist = App.Vault.FindAllByUserName(UserName.Text);
                var credential = readonlylist[0];//take the first entry from the credentiallist

                credential.RetrievePassword();
                Password.Password = credential.Password;
                Password.Focus(FocusState.Programmatic);
            }
            catch (COMException)
            {
                Debug.WriteLine("First Login on this device");
            }
            
            //for debugging ignore the login
            //switchToNextPage();
        }

        /// <summary>
        /// calls some valigation rules based on length that are similar to the buisiness logic behind the webform
        /// </summary>
        /// <param name="sender">the button</param>
        /// <param name="e">event information</param>
        private async void LoginButton_ClickAsync(object sender, RoutedEventArgs e)
        {

            //password Credentials
            if (String.IsNullOrWhiteSpace(UserName.Text))
            {
                WelcomeLabel.Text = "Bitte geben Sie einen Benutzernamen ein";
            }
            else if (String.IsNullOrWhiteSpace(Password.Password) | Password.Password.Length < 8)
            {
                WelcomeLabel.Text = "Bitte geben Sie ein Passwort aus mindestens 8 Zeichen ein";
            }
            else
            {
                bool credentialsAreValid = false;
                string formOrErrorMessage = "";
                //try to login to the form with the user credentials
                try
                {
                    //toDo: Wert weiterverwenden
                    formOrErrorMessage = await App.client.LoginAsync(UserName.Text, Password.Password);

                    if (!formOrErrorMessage.StartsWith("error."))
                        credentialsAreValid = true;
                }
                catch (Exception)
                {
                    WelcomeLabel.Text = "Kundenportal nicht erreichbar um Deine Anmeldedaten zu verifizieren.";
                }


                //now, that the server validated the user credentials as valid, store them in passwordvault for later usage, if they aren't stored there already
                if (credentialsAreValid)
                {
                    
                    try
                    {
                        App.Vault.FindAllByResource("ListenToMe");
                    }
                    catch (COMException) //if no list was found for ListenToMe
                    {
                        //create entry in vault for this user
                        AddUserCredential(UserName.Text, Password.Password);
                    }
                    //retrieve credential from vault by userName

                    var readonlylist = App.Vault.FindAllByUserName(UserName.Text);
                    var credential = readonlylist[0];//take the first entry from the credentiallist
                    Debug.WriteLine("found so credentials for" + credential.UserName + " user:" + readonlylist.Count);
                    App.formstore.Sections = await App.client.GetAllElementsAsync(credential.UserName,credential.Password);
                    
                    //Debug.WriteLine(App.SectionsList.ElementAt(App.SectionsList.IndexOf(App.SectionsList.First()) + 1));
                    //fill section dynamically to panel
                    SwitchToNextPage();
                }
                else//if credentials aren't valid
                {
                    WelcomeLabel.Text = formOrErrorMessage;
                }
               
            }
                
        }

        /// <summary>
        /// navigates the frame if logged in sucessfully
        /// </summary>
        private void SwitchToNextPage()
        {
            
            App.NavigationService.Navigate(typeof(MainPage));
        }

        /// <summary>
        /// adds a user credential to the password vault
        /// </summary>
        /// <param name="username">the username of the credential</param>
        /// <param name="password">the password of the credential</param>
        private static void AddUserCredential(string username, string password)
        {
            if(App.Vault!=null)
            App.Vault = new Windows.Security.Credentials.PasswordVault();
            
                //only if there isn't already an entry add a new entry to the vault
                App.Vault.Add(new Windows.Security.Credentials.PasswordCredential(
               "ListenToMe", username, password));
           
        }
    }
}