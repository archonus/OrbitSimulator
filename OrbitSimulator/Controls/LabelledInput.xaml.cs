using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OrbitSimulator.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LabelledInput : ContentView
    {
        double numericValue;

        /// <summary>
        /// The value displayed in the label
        /// </summary>
        public string Label
        {
            get => lbl_InputLabel.Text;
            set => lbl_InputLabel.Text = value;
        }

        /// <summary>
        /// The width of the input field - the setter sets the width request
        /// </summary>
        /// <remarks>Setting it does not necessarily mean the width is changed</remarks>
        public double InputWidth
        {
            get => entry_Input.Width;
            set
            {
                entry_Input.WidthRequest = value;
            }
        }

        /// <summary>
        /// The user input
        /// </summary>
        public string Input
        {
            get => entry_Input.Text;
            set => entry_Input.Text = value;
        }

        /// <summary>
        /// The units displayed after the textbox entry - if set to "", nothing will be displayed
        /// </summary>
        public string Units
        {
            get => lbl_Units.Text;
            set => lbl_Units.Text = value;
        }

        public bool HasUnits => string.IsNullOrEmpty(Units);

        /// <summary>
        /// Whether to check if the input is a valid numeric input
        /// </summary>
        public bool IsNumeric { get; set; }

        /// <summary>
        /// Whether the input box should alert the user if the input is invalid
        /// </summary>
        /// <remarks>By default set to true</remarks>
        public bool AlertInvalid { get; set; } = true;


        /// <summary>
        /// Whether to allow negative numbers when validating
        /// </summary>
        public bool IsNegativeAllowed { get; set; } = false;

        /// <summary>
        /// If the user input is valid for use
        /// </summary>
        /// <remarks>False if the input is empty or null and, if <see cref="IsNumeric"/> is true, whether the input can be converted to a <see cref="double"/></remarks>
        public bool IsInputValid { get; private set; }


        /// <summary>
        /// The numeric value of the input, if the input is valid and the input is set as numeric
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when setting the value to null</exception>
        /// <exception cref="InvalidOperationException">Thrown if trying to set when <see cref="IsNumeric"/> is false</exception>
        public double? NumericValue
        {
            get
            {
                if (IsNumeric && IsInputValid)
                { //Only if the value is supposed to be a number, and it is valid
                    return numericValue;
                }
                else
                { //The input cannot be converted to a number
                    return null;
                }
            }
            set
            {
                if (IsNumeric)
                {
                    numericValue = value ?? throw new ArgumentNullException($"{nameof(NumericValue)} cannot be set to null");
                    Input = numericValue.ToString(); //Change the text as well
                }
                else
                {
                    throw new InvalidOperationException("Input is designated as non-numeric");
                }
            }
        }
        public LabelledInput()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            bool valid;
            if (IsNumeric)
            { //Check whether it is valid as a number
                valid = double.TryParse(Input, out double temp); //Try to convert to a number
                valid = valid && (IsNegativeAllowed || temp >= 0); //If negative is not allowed, check that it isn't negative
                if (valid)
                { //If the number can be converted, store its value
                    numericValue = temp;
                }
            }
            else
            { //A valid string is not null or empty
                valid = !string.IsNullOrEmpty(Input);
            }
            if (AlertInvalid)
            { //The control should display the fact that the input is invalid
                entry_Input.BackgroundColor = valid ? Color.Default : Color.Red;
            }
            IsInputValid = valid; //Set the property
        }
    }
}