using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OrbitSimulator.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
#if DEBUG
            var merlin9 = new DataService.Engine
            {
                Name = "Merlin9",
                Mass = 4230,
                SpecificImpulse = 311,
                MassFlowRate = 4230
            };

            var merlin = new DataService.Engine
            {
                Name = "Merlin 1D Vac",
                Mass = 470,
                SpecificImpulse = 348,
                MassFlowRate = 287
            };

            var falcon9Stage1 = new DataService.Stage
            {
                Name = "Falcon 9 Stage 1",
                EngineData = merlin9,
                PropellantMass = 418_700,
                StructuralMass = 22_970
            };

            var falcon9Stage2 = new DataService.Stage
            {
                Name = "Falcon 9 Stage 2",
                EngineData = merlin,
                PropellantMass = 111_500,
                StructuralMass = 4_030
            };
            var rocketData = new DataService.Rocket();
            rocketData.SetStages(new DataService.Stage[] { falcon9Stage1, falcon9Stage2 });
            rocketData.PayloadMass = input_Payload.NumericValue.GetValueOrDefault(100);
            Button testButton = new Button() { Text = "For testing purposes only" }; //Skip the loading
            testButton.Clicked += async (sender, e) =>
            {
                var defaultTargetHeight = 300;
                var orbitHeight = input_OrbitHeight.NumericValue.GetValueOrDefault(defaultTargetHeight);
                var rocket = Factory.MultiStageRocketFactory.ConstructMultiStageRocket(rocketData);
                await Navigation.PushAsync(
                    new FlightPage(
                        new SimulationViewModel(rocket, orbitHeight)
                        )
                    );
            };
            stckLayout_Main.Children.Add(testButton);
#endif

        }

        /// <summary>
        /// Constructs the <see cref="RocketBuildViewModel"/> object to be used
        /// </summary>
        /// <returns>The object, or null if unable to initialise</returns>
        private RocketBuildViewModel GetViewModel()
        {
            var orbitHeight = input_OrbitHeight.NumericValue.GetValueOrDefault();
            var payloadMass = input_Payload.NumericValue.GetValueOrDefault();
            if (orbitHeight <= 0)
            {
                DisplayAlert("Invalid Input", "Please specify a valid target height", "OK");
                return null;
            }
            if (payloadMass <= 0)
            {
                DisplayAlert("Invalid Input", "Please specify a valid payload mass", "OK");
                return null;
            }
            var viewModel = new RocketBuildViewModel(App.DatabasePath, orbitHeight);

            return viewModel;
        }

        private async void btn_Build_Clicked(object sender, EventArgs e)
        {
            var viewModel = GetViewModel();
            if (!(viewModel is null))
            {
                await Navigation.PushAsync(new RocketBuildPage(viewModel));
            }
        }

        private async void btn_UseSaved_Clicked(object sender, EventArgs e)
        {
            var viewModel = GetViewModel();
            if (!(viewModel is null))
            {
                await Navigation.PushAsync(new RocketSelectionPage(viewModel));
            }
        }
    }
}