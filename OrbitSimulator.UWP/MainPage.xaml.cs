namespace OrbitSimulator.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new OrbitSimulator.App());
        }
    }
}
