using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AintBnB.App.Helpers
{
    internal class Rl : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting;

        public Rl(Func<Task> execute) : this(execute, () => true)
        {
        }

        public Rl(Func<Task> execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return !(_isExecuting && _canExecute());
        }

        public event EventHandler CanExecuteChanged;

        public async void Execute(object parameter)
        {
            _isExecuting = true;
            OnCanExecuteChanged();
            try
            {
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                OnCanExecuteChanged();
            }
        }

        protected virtual void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
        }
    }
}
