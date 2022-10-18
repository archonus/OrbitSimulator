using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OrbitSimulator.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StageBuildView : ContentView
    {
        RocketBuildViewModel viewModel;
        public StageBuildView(RocketBuildViewModel buildViewModel, int stageNumber = 1) : this()
        {
            viewModel = buildViewModel;
            this.BindingContext = viewModel;
            lbl_Title.Text = $"Stage {stageNumber}";
        }

        public StageBuildView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Generates a <see cref="DataService.Stage"/> object from the data inputted.
        /// </summary>
        /// <returns>A <see cref="DataService.Stage"/> object, or null if there is an error</returns>
        public DataService.Stage GetStageData()
        {
            if (picker_Engine.SelectedItem is null)
            { //The engine has not been selected
                return null;
            }
            else if (!input_Fuel.IsInputValid)
            { //The fuel is not valid
                return null;
            }
            else if (!input_Structure.IsInputValid)
            { //The structural mass is not valid
                return null;
            }
            else
            { //Passed validation checks
                if (picker_Stage.SelectedIndex == 0)
                { //The data is custom
                    return new DataService.Stage //Construct object with the data
                    {
                        PropellantMass = input_Fuel.NumericValue.Value,
                        StructuralMass = input_Structure.NumericValue.Value,
                        EngineData = (DataService.Engine) picker_Engine.SelectedItem,
                        IsCustom = true
                    };
                }
                else
                { //The data has been loaded from database
                    var stageData = (DataService.Stage) picker_Stage.SelectedItem; //Cast the selected item
                    return stageData;
                }
            }
        }

        async Task DisplayDataAsync(DataService.Stage stageData)
        {
            input_Fuel.NumericValue = stageData.PropellantMass;
            input_Structure.NumericValue = stageData.StructuralMass;
            if (stageData.EngineData is null) //If the stageData has not been initialised with an engine object, initialise it
            { //Should not be needed
                await viewModel.InitialiseStage(stageData);
            }
            var index = picker_Engine.Items.IndexOf(stageData.EngineData.Name); //Directly setting selectedItem does not work
            picker_Engine.SelectedIndex = index;
        }

        private async void picker_Stage_SelectedIndexChanged(object sender, EventArgs e)
        {
            await DisplayDataAsync((DataService.Stage) picker_Stage.SelectedItem);
            stck_StageSelection.IsEnabled = (picker_Stage.SelectedIndex == 0); //Data can only be edited if the selected stage is custom
        }
    }
}