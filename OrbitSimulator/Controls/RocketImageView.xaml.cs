using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OrbitSimulator.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RocketImageView : ContentView
    {
        private const int flameRotationAngle = 5;
        SimulationViewModel viewModel;
        public RocketImageView()
        {
            InitializeComponent();
        }

        public RocketImageView(SimulationViewModel viewModel) : this()
        {
            InitialisePage(viewModel);
        }

        /// <summary>
        /// Sets the view model of the page, and thus the binding context, and thus properly initialises the page
        /// </summary>
        /// <param name="rocketViewModel">The <see cref="SimulationViewModel"/> object that will be this page's view model</param>
        public void InitialisePage(SimulationViewModel viewModel)
        {
            this.viewModel = viewModel;
            //this.BindingContext = viewModel;
            LoadImages();
        }

        /// <summary>
        /// Update the display of the rocket to display its current state
        /// </summary>
        /// <remarks>To be called everytime the display needs to be updated</remarks>
        public void UpdateDisplay()
        {
            var rocketDisplayLength = grid_RocketImages.Height; //How tall the image is in device independent units
            var scaling = rocketDisplayLength / viewModel.RocketLength; //The scaling applied to convert from the actual height
            uint duration = (uint) (viewModel.Timer.TimerInterval * 1000); //Convert to milliseconds and the round to get uint
            var scaledHeight = viewModel.Height * scaling;
            if (scaledHeight > 0) //The rocket is above the ground
            {
                box_Ground.TranslateTo(0, scaledHeight, duration); //Move the ground down
            }
            grid_RocketImages.RotateTo(viewModel.Direction, duration); //Rotate the rocket to the direction of its flight
            _ = AnimateFlamesAsync(duration);

        }

        public void EjectStage()
        {
            if (grid_RocketImages.Children.Count >= 3) //There are at least two stages, not counting the final image of the flames
            {
                grid_RocketImages.Children.RemoveAt(grid_RocketImages.Children.Count - 2); //Remove the last child
                Grid.SetRow(img_Flames, grid_RocketImages.RowDefinitions.Count - 2); //Move the flames up
                grid_RocketImages.RowDefinitions.RemoveAt(grid_RocketImages.RowDefinitions.Count - 1); //Remove the final row
            }
        }

        /// <summary>
        /// Display message on the label
        /// </summary>
        /// <param name="message">The text to be displayed</param>
        /// <param name="duration">The time for which the message should be displayed - pass <see cref="double.PositiveInfinity"/> for no fading</param>
        /// <returns></returns>
        public async Task DisplayMessageAsync(string message, double duration = 15)
        {

            lbl_TextDisplay.Opacity = 1; //Make label visible
            lbl_TextDisplay.Text = message;
            if (!double.IsInfinity(duration)) //If the message is not supposed to be displayed 
            {
                uint time = (uint) (duration * 1000); //Convert to milliseconds
                await lbl_TextDisplay.FadeTo(0, time); //Fade to nothing in 5 seconds
            }

        }

        async Task AnimateFlamesAsync(uint duration)
        {
            var time = duration / 2;
            await Task.WhenAll(img_Flames.ScaleXTo(0.85, time), img_Flames.FadeTo(0.85, time));
            await Task.WhenAll(img_Flames.ScaleXTo(1, time), img_Flames.FadeTo(1, time));
        }

        void LoadImages()
        {
            string rocketTopImageId = App.GetResourceId(App.rocketTopName);
            string rocketStageImageId = App.GetResourceId(App.rocketStageName);
            string rocketFlamesImageId = App.GetResourceId(App.rocketFlamesName);
            string leftArrowImageId = App.GetResourceId(App.leftArrowName);
            string pauseImageId = App.GetResourceId(App.pauseButtonName);
            string rocketBottomId = App.GetResourceId(App.rocketBottomName);

            img_RocketTop.Source = ImageSource.FromResource(rocketTopImageId, App.AppAssembly);
            Image stageImage = new Image
            {
                Source = ImageSource.FromResource(rocketStageImageId, App.AppAssembly)
            };
            int i;
            for (i = 1; i < viewModel.NumStages - 1; i++) //Since the first stage is part of the rocket top image, start at 1
            {
                grid_RocketImages.RowDefinitions.Insert(grid_RocketImages.RowDefinitions.Count - 1, new RowDefinition());
                grid_RocketImages.Children.Add(stageImage, 0, i); //Column 0 (there is only one column), row i
            }
            Grid.SetRow(img_RocketBottom, i);
            img_RocketBottom.Source = ImageSource.FromResource(rocketBottomId, App.AppAssembly);

            Grid.SetRow(img_Flames, i + 1); //Move the image of the flames to the last row
            img_Flames.Source = ImageSource.FromResource(rocketFlamesImageId, App.AppAssembly);


            imgbtn_Left.Source = ImageSource.FromResource(leftArrowImageId, App.AppAssembly);
            imgbtn_Right.Source = ImageSource.FromResource(leftArrowImageId, App.AppAssembly);
            imgbtn_Right.RotationY = 180; //Flip the image

            img_Paused.Source = ImageSource.FromResource(pauseImageId, App.AppAssembly);
            img_Paused.IsVisible = false;
        }

        private void imgbtn_Left_Pressed(object sender, EventArgs e)
        {
            viewModel.ChangeThrustDirection(-5); //5 degrees to the left
            img_Flames.Rotation = -flameRotationAngle;
        }

        private void ThrustDirectionButton_Released(object sender, EventArgs e)
        {
            viewModel.ChangeThrustDirection(0); //Return to up the rocket
            img_Flames.Rotation = 0;
        }

        private void imgbtn_Right_Pressed(object sender, EventArgs e)
        {
            viewModel.ChangeThrustDirection(5); //5 degrees to the right
            img_Flames.Rotation = flameRotationAngle;
        }
    }
}