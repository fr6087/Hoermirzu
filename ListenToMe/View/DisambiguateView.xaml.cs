
using Windows.UI.Xaml.Controls;


namespace ListenToMe.View
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class DisambiguateView : ContentDialog
    {
        public DisambiguateView()
        {
            this.InitializeComponent();
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
           

            this.CloseButtonText = loader.GetString("Close");
        }
    }
}
