using System.Windows.Media.Imaging;

namespace CandyLauncher.Abstraction.Action
{
    public class ErrorAction : ModifiableActionBase
    {
        public ErrorAction(BitmapImage icon, string title, string subtitle)
        {
            IsExecutable = false;
            Icon = icon;
            Title = title;
            Subtitle = subtitle;
        }
    }
}
