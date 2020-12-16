using System;
using ObservableComputations;

namespace Trader.Domain.Infrastucture
{
    public class WpfOcDispatcher : IOcDispatcher
    {
        private System.Windows.Threading.Dispatcher _dispatcher;

        public WpfOcDispatcher(System.Windows.Threading.Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        #region Implementation of IDispatcher

        public void Invoke(Action action, int priority, object parameter, object context)
        {
            if (_dispatcher.CheckAccess())
                action();
            else
                _dispatcher.Invoke(action);
        }

        #endregion
    }
}
