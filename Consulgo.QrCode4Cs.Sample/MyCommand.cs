using System;

namespace Consulgo.QrCode4Cs.Sample
{
    public class MyCommand : System.Windows.Input.ICommand
    {
        private readonly Action<object> _action;
        private readonly Func<object, bool> _canExecute;

        public MyCommand(Action<object> action, Func<object, bool> canExecute)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            _action = action;

            if (canExecute != null)
            {
                _canExecute = canExecute;
            }
        }

        public MyCommand(Action action)
            : this(action, null)
        {
        }

        public MyCommand(Action action, Func<bool> canExecute)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            _action = (obj) => action();

            if (canExecute != null)
            {
                _canExecute = (obj) => canExecute();
            }
        }


        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
            {
                return _canExecute(parameter);
            }

            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}
