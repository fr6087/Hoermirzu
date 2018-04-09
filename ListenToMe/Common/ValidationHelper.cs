using System;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ListenToMe.Common
{
    /// <summary>
    /// can help the static XAML-Pages to validate the required fields. Currently deactive.
    /// </summary>
    internal static class ValidationHelper
    {

        internal static bool MandatoryFieldsFilled(TextBox[] boxes)
        {
            foreach (TextBox b in boxes)
            {
                if (String.IsNullOrWhiteSpace(b.Text))
                {
                    b.Background = new SolidColorBrush(Colors.Red);
                    return false;
                }
            }
            return true;
        }
    }

}
