using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OrbitSimulator.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FlightPage : ContentPage
    {

        SimulationViewModel viewModel;

        #region Constructors
        public FlightPage()
        {
            InitializeComponent();
        }

        public FlightPage(SimulationViewModel rocketViewModel) : this()
        {
            SetViewModel(rocketViewModel);
        }
        #endregion

        /// <summary>
        /// Sets the view model of the page, and thus the binding context, and thus properly initialises the page
        /// </summary>
        /// <param name="rocketViewModel">The <see cref="SimulationViewModel"/> object that will be this page's view model</param>
        public void SetViewModel(SimulationViewModel rocketViewModel)
        {
            viewModel = rocketViewModel;
            this.BindingContext = viewModel;
            viewModel.SimulationOver += viewModel_SimulationOver; //Set the event handler for the SimulationOver event
            viewModel.RocketStateChanged += viewModel_RocketStateChanged; //Set the event handler for the RocketStateChanged event

            try
            { //Load the images for the control, and sets the view model
                rocketImageView.InitialisePage(viewModel);
            }
            catch (FileNotFoundException)
            { //The resources failed to load
                DisplayAlert("Error", "There was an error accessing resources", "OK");
                Navigation.PopAsync();
            }
            timeRatioControl.BindingContext = viewModel.Timer; //Set the binding context of the timeRatioControl
        }

        private async void viewModel_RocketStateChanged(object sender, RocketStateChangedArgs eventArgs)
        {
            //Check the eventArgs object's fields
            if (!string.IsNullOrEmpty(eventArgs.Message))
            { //There's a message to display
                _ = rocketImageView.DisplayMessageAsync(eventArgs.Message);
            }
            if (eventArgs.StageEjected)
            { //If the rocket has ejected a stage, change display accordingly
                rocketImageView.EjectStage();
            }
            if (eventArgs.FullBurnReached)
            { //When there is no more fuel left in the current stage
                viewModel.StopSimulation();
                if (eventArgs.CanEject)
                { //The rocket can eject the current stage
                    var ejectStage = await DisplayAlert("Full Burn Reached", "There is no more fuel left in the current stage. Do you wish to eject?", "Yes", "No");
                    if (ejectStage)
                    { //The user wishes to eject
                        viewModel.EjectStage();
                    }
                    viewModel.StartSimulation();
                }
                else
                { //Final stage of the rocket, so cannot do anything
                    await DisplayAlert("Full Burn Reached", "There is no more fuel left", "OK");
                }

            }
            rocketImageView.UpdateDisplay(); //Update the rocket display
        }

        private async void viewModel_SimulationOver(object sender, EventArgs e)
        {
            await EndSimulation();
        }

        private async void btn_End_Clicked(object sender, EventArgs e)
        {
            viewModel.StopSimulation(); //Pause
            var response = await DisplayAlert("Warning", "All progress will be lost - are you sure you wish to exit?", "Exit", "Cancel");
            if (response)
            { //User wishes to continue
                await EndSimulation(); //End it
            }
            else
            { //Restart simulation
                viewModel.StartSimulation();
            }
        }

        /// <summary>
        /// Ends the simulation, and navigates to the SimulationEndPage
        /// </summary>
        private async Task EndSimulation()
        {
            viewModel.StopSimulation(); //Pause it to stop timer continuing
            btn_PlayPause.IsEnabled = false; //Disable the play button
            await Navigation.PushAsync(new SimulationEndPage(viewModel)); //Navigate to the final page
            Navigation.RemovePage(this); //To prevent the simulation from being returned to using the back button, since it would then be in invalid state
        }

        protected override void OnDisappearing()
        {
            viewModel.StopSimulation(); //To ensure that the timer is not left running when the page is navigated away from
            //Unsubscribe from the events to aid garbage collector
            viewModel.RocketStateChanged -= viewModel_RocketStateChanged;
            viewModel.SimulationOver -= viewModel_SimulationOver;
        }
    }
}