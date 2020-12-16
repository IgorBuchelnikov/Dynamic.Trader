using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Trader.Domain.Infrastucture
{
   public abstract class AbstractNotifyPropertyChanged : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// If the value has changed, sets referenced backing field and raise notify property changed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="backingField">The backing field.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void SetAndRaise<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = null)
        {
            backingField = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
