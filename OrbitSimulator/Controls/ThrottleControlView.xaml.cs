
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OrbitSimulator.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ThrottleControlView : ContentView
    {
        public double ThrottleValue => slider_Throttle.Value; //Just in case it needs to be acccessed from code
        public ThrottleControlView()
        {
            InitializeComponent();
        }

        public ThrottleControlView(SimulationViewModel viewModel) : this()
        {
            this.BindingContext = viewModel;
        }
    }
}