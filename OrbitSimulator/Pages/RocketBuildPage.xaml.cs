using System;
using System.Collections.Generic;
using OrbitSimulator.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OrbitSimulator.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RocketBuildPage : ContentPage
    { //REVIEW Add way to change target height?
        RocketBuildViewModel viewModel;
        int stageNumber = 0;

        public RocketBuildPage(RocketBuildViewModel buildViewModel)
        {
            InitializeComponent();
            viewModel = buildViewModel;
            this.BindingContext = viewModel;
            AddStage();
        }

        void AddStage()
        {
            stageNumber++;
            stck_Stages.Children.Add(
                new StageBuildView(viewModel, stageNumber)
                );
        }

        /// <summary>
        /// Generates a list of <see cref="DataService.Stage"/> objects from the inputted data
        /// </summary>
        List<DataService.Stage> GetStagesData()
        {
            List<DataService.Stage> stages = new List<DataService.Stage>(stck_Stages.Children.Count);
            foreach (StageBuildView stageBuildView in stck_Stages.Children)
            { //Iterate through all the stages
                var stageData = stageBuildView.GetStageData(); //Retrieve the data from the control
                if (stageData is null) //There was an error
                {
                    return null;
                }
                else //Set the name of the stage
                {
                    if (stageData.IsCustom && string.IsNullOrEmpty(stageData.Name)) //It is possible that the user chooses a stage that has been created previously
                    {
                        stageData.Name = $"{input_RocketName.Input} Stage {stages.Count + 1}"; //+1 because the stage hasn't been added yet
                    }
                    stages.Add(stageData); //Add to the list
                }
            }
            return stages;
        }

        private async void btn_AddStage_Clicked(object sender, EventArgs e)
        {
            AddStage();
            await scrollView_Main.ScrollToAsync(btn_AddStage, ScrollToPosition.End, true); //Scroll to end
        }

        private async void btn_Start_Clicked(object sender, EventArgs args)
        {
            //Input validation
            if (!input_RocketName.IsInputValid)
            { //Not null or empty
                await DisplayAlert("Invalid Input", "Please specify a name for the rocket", "OK");
                return;
            }

            List<DataService.Stage> stages = GetStagesData();
            if (stages is null)
            { //There was an error
                await DisplayAlert("Invalid Input", "Please check the provided input", "OK");
                return;
            }
            viewModel.SetRocketStages(stages); //Set the list of stages
            try
            {
                await viewModel.SaveRocketAsync(input_RocketName.Input); //Save the rocket to the database
                var simViewModel = await viewModel.GetSimulationViewModelAsync(stagesSet: true); //The stages of the rockets have been set
                if (simViewModel is null)
                { //Could not load data correctly
                    await DisplayAlert("Error", "There was an error loading the data", "OK");
                }
                else
                { //Succeeded
                    await Navigation.PushAsync(new FlightPage(simViewModel));
                }
            }
            catch (DataService.NameNotUniqueException e)
            {
                await DisplayAlert("Invalid Name", $"A rocket with the name {e.Name} already exists", "OK");
            }
            catch (DataService.DataServiceException)
            { //Some other error occurred when saving the rocket
                await DisplayAlert("Error", "There was an error saving the rocket. Please try again", "OK");
            }
        }
    }
}