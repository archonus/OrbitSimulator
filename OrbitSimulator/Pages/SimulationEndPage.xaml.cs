using System.Text;
using OrbitSimulator.Core;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OrbitSimulator.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SimulationEndPage : ContentPage
    {
        public SimulationEndPage()
        {
            InitializeComponent();
        }
        public SimulationEndPage(SimulationViewModel viewModel) : this()
        {
            lbl_Main.Text = viewModel.IsCrashed //Message depends on whether the rocket has crashed
                ? "Rocket Crashed"
                : GetMessage(viewModel); //If the rocket has not crashed, message is more complex
        }

        /// <summary>
        /// Generate message to be displayed from the state of the simulation
        /// </summary>
        /// <param name="viewModel">The view-model which has the state of the simulation</param>
        /// <returns>The message</returns>
        private string GetMessage(SimulationViewModel viewModel)
        {
            StringBuilder message = new StringBuilder(); //For creating the message
            if (viewModel.Height >= PhysicsUtils.TroposphereEdge)
            {
                message.AppendLine("Reached upper atmosphere");
            }
            if (viewModel.Height >= PhysicsUtils.AtmosphereEdge)
            {
                message.AppendLine("Reached outer space");
            }
            if (viewModel.Fuel == 0)
            {
                message.AppendLine("Full burn reached");
            }
            if (viewModel.Height < viewModel.TargetHeight)
            { //Did not reach target height
                message.AppendLine($"Failed to reach target height of {viewModel.TargetHeight} km");
                message.AppendLine($"Current speed is {viewModel.Speed:F0} km/h");
            }
            else
            { //Reached the target height
                message.AppendLine($"Reached the target height of {viewModel.TargetHeight} km \n");
                var requiredSpeed = PhysicsUtils.GetRequiredOrbitalSpeed(viewModel.TargetHeight);
                var escapeVelocity = PhysicsUtils.GetEscapeVelocity(viewModel.TargetHeight);
                var effectiveSpeed = viewModel.EffectiveOrbitalSpeed;
                message.AppendLine($"Effective velocity of {effectiveSpeed:F0} km/h was achieved");
                if (effectiveSpeed < requiredSpeed)
                { //Too slow
                    message.AppendLine($"Speed of {requiredSpeed:F0} km/h required to remain in orbit.");
                }
                else if (effectiveSpeed < escapeVelocity)
                { //Will stay in orbit
                    message.AppendLine($"Success! Satellite will remain in orbit");
                }
                else
                { //Too fast
                    message.AppendLine($"Satellite's speed is greater than the escape velocity of {escapeVelocity:F0} km/h and has escaped orbit.");
                }
            }
            return message.ToString(); //Convert the StringBuilder object to string
        }

        private async void btn_Restart_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}