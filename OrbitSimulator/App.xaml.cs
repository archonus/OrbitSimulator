using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using OrbitSimulator.Pages;
using Xamarin.Forms;

namespace OrbitSimulator
{
    public partial class App : Application
    {

#if DEBUG
        public static readonly string DatabaseFileName = "RocketsDB_DebugMode.db";
#else
        public static readonly string DatabaseFileName = "RocketsDB.db";
#endif
        /// <summary>
        /// The full path of the database file, accessed from the properties dictionary
        /// </summary>
        public static string DatabasePath
        {
            get
            {
                if (!App.Current.Properties.ContainsKey("dbPath"))
                { //The entry is not there
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DatabaseFileName); //Combine the folder path with the name
                    App.Current.Properties.Add("dbPath", path);
                    App.Current.SavePropertiesAsync();
                }
                return App.Current.Properties["dbPath"].ToString();
            }
        }

        static readonly Assembly assembly = typeof(App).Assembly;
        public static Assembly AppAssembly => assembly;

        public static readonly string rocketTopName = "rocket_top.png";
        public static readonly string rocketStageName = "rocket_stage.png";
        public static readonly string rocketFlamesName = "flames.png";
        public static readonly string leftArrowName = "left_arrow.png";
        public static readonly string pauseButtonName = "pause_button.png";
        public static readonly string rocketBottomName = "rocket_bottom.png";
        public static readonly string fastForwardsName = "fast_forward_symbol.png";

        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage());
        }

        /// <summary>
        /// Get the full name of the resource
        /// </summary>
        /// <param name="resourceName">The filename of the </param>
        /// <exception cref="FileNotFoundException"/>
        public static string GetResourceId(string resourceName)
        {
            try
            { // Select the id where the name ends with the supplied resourceName
                var id = AppAssembly.GetManifestResourceNames() //Get all the embedded resource names
                    .Where(name => name.EndsWith(resourceName)) //Select where the name ends with the resourceName
                    .Single(); //There should only be one result
                Debug.WriteLine($"Found {id}");
                return id;
            }
            catch (InvalidOperationException)
            { //The file was not found
                throw new FileNotFoundException($"Resource {resourceName} could not be found");
            }
        }

        protected override async void OnStart()
        {
            if (!File.Exists(DatabasePath)) //If the database file does not already exist, it needs to be copied there, since embedded resources cannot be written to.
            { //TODO Implement exception handling for IO errors

                string resourceId = GetResourceId("RocketsDB.db");
                using (Stream embeddedDbStream = assembly.GetManifestResourceStream(resourceId))
                {
                    using (var fileStream = new FileStream(DatabasePath, FileMode.Create)) //Create the file
                    { //Copy the data from the embedded resource to the file location
                        await embeddedDbStream.CopyToAsync(fileStream);
                    }
                }
                Debug.WriteLine($"Database file copied to {DatabasePath}");
            }
            else
            {
                Debug.WriteLine($"File found at {DatabasePath}");
            }
        }

        protected override void OnResume()
        { //REVIEW Should eventually implement some way of saving state
        }
    }
}
