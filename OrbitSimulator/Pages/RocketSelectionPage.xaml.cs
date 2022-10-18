using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OrbitSimulator.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RocketSelectionPage : ContentPage
    {
        RocketBuildViewModel viewModel;

        public RocketSelectionPage(RocketBuildViewModel buildViewModel)
        {
            InitializeComponent();
            viewModel = buildViewModel;
            this.BindingContext = viewModel;
        }

        private async void btn_Start_Clicked(object sender, EventArgs e)
        {
            if (picker_SelectedRocket.SelectedIndex < 0)
            { //No item chosen
                await DisplayAlert("Invalid Operation", "Please choose a rocket", "OK");
                return;
            }
            var simViewModel = await viewModel.GetSimulationViewModelAsync(); //Create the simulation view model
            if (simViewModel is null)
            { //There was an error
                await DisplayAlert("Error", "There was an error loading the data. Please try again", "OK");
            }
            else
            { //Navigate to the flight page
                await Navigation.PushAsync(new FlightPage(simViewModel));
            }
        }
    }
}