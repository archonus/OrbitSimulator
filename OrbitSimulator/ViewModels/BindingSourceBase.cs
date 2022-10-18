using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OrbitSimulator
{
    /// <summary>
    /// Abstract base class for classes that implement <see cref="INotifyCompletion"/>
    /// </summary>
    public abstract class BindingSourceBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets a backing field of a property, and invokes <see cref="PropertyChanged"/> with the property name provided, or the caller name if not provided
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="field">The underlying backing field of the property, passed in by reference</param>
        /// <param name="value">The value to be set</param>
        /// <param name="propertyName">The name of the property - defaults to the caller name</param>
        protected void SetAndNotifyField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            field = value;
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Method for invoking event when a property is changed
        /// </summary>
        /// <param name="propertyName">The name of the property changed - defaults to the caller name.</param>
        /// <remarks>Pass in <see cref="string.Empty"/> to notify all properties changed</remarks>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) //To get the property name that is changed
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); //When a property is changed, invoke the event if it's not null
        }

        /// <summary>
        /// Method for invoking event when a property is changed
        /// </summary>
        /// <param name="args">An already constructed <see cref="PropertyChangedEventArgs"/></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }
    }
}
