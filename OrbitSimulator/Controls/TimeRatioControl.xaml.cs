
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OrbitSimulator.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimeRatioControl : ContentView
    {
        public TimeRatioControl()
        {
            InitializeComponent();
            LoadImages();
        }

        private void LoadImages()
        {
            string resourceId = App.GetResourceId(App.fastForwardsName);
            var imageSource = ImageSource.FromResource(resourceId, App.AppAssembly);
            btn_SpeedUp.Source = imageSource;
            btn_SlowDown.Source = imageSource;
            btn_SlowDown.RotationY = 180; //Flip 180 across y-axis

        }
    }
}