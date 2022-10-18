
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OrbitSimulator.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RocketDataView : ContentView
    {
        public RocketDataView(SimulationViewModel rocketViewModel) : this()
        {
            this.BindingContext = rocketViewModel;
        }

        public RocketDataView()
        {
            InitializeComponent();
        }
    }
}