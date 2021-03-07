using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

namespace Laba2.Runner.Utils
{
    public sealed class RelayCommand : ICommand
    {
        #region  Declarations

        private readonly Func<Boolean> _canExecuteMethod;
        private readonly Action _executeMethod;

        #endregion

        #region  Events

        /// <summary>
        ///     Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecuteMethod != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }

            remove
            {
                if (_canExecuteMethod != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        /// <summary>
        ///     Handles the RaiseEvent event of the CanExecuteChanged control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        public void CanExecuteChanged_RaiseEvent(Object sender, EventArgs e)
        {
            if (_canExecuteMethod != null)
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }

        #endregion

        #region  Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="executeMethod">The execute method.</param>
        public RelayCommand(Action executeMethod)
            : this(executeMethod, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="executeMethod">The execute method.</param>
        /// <param name="canExecuteMethod">The can execute method.</param>
        public RelayCommand(Action executeMethod, Func<Boolean> canExecuteMethod)
        {
            _executeMethod = executeMethod ?? throw new ArgumentNullException("executeMethod", "Delegate commands can not be null");
            _canExecuteMethod = canExecuteMethod;
        }

        #endregion

        #region  Methods

        /// <summary>
        ///     Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        /// <returns>
        ///     true if this command can be executed; otherwise, false.
        /// </returns>
        [DebuggerStepThrough]
        public Boolean CanExecute(Object parameter)
        {
            return _canExecuteMethod == null || _canExecuteMethod();
        }

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        [DebuggerStepThrough]
        public void Execute(Object parameter)
        {
            if (_executeMethod == null)
            {
                return;
            }
            _executeMethod();
        }

        #endregion
    }

    //
    //=====================================================================================================
    //=====================================================================================================
    //=====================================================================================================
    //

    /// <summary>
    ///     Represents RelayCommand that uses the WPF CommandManager
    /// </summary>
    /// <typeparam name="T">Parameter type</typeparam>
    public sealed class RelayCommand<T> : ICommand
    {
        #region  Declarations

        private readonly Predicate<T> _canExecuteMethod;
        private readonly Action<T> _executeMethod;

        #endregion

        #region  Events

        /// <summary>
        ///     Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecuteMethod != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }

            remove
            {
                if (_canExecuteMethod != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        /// <summary>
        ///     Handles the RaiseEvent event of the CanExecuteChanged control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        public void CanExecuteChanged_RaiseEvent(Object sender, EventArgs e)
        {
            if (_canExecuteMethod != null)
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }

        #endregion

        #region  Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand{T}" /> class.
        /// </summary>
        /// <param name="executeMethod">The execute method.</param>
        public RelayCommand(Action<T> executeMethod)
            : this(executeMethod, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand{T}" /> class.
        /// </summary>
        /// <param name="executeMethod">The execute method.</param>
        /// <param name="canExecuteMethod">The can execute method.</param>
        public RelayCommand(Action<T> executeMethod, Predicate<T> canExecuteMethod)
        {
            _executeMethod = executeMethod ?? throw new ArgumentNullException("executeMethod", "Delegate commands can not be null");
            _canExecuteMethod = canExecuteMethod;
        }

        #endregion

        #region  Methods

        /// <summary>
        ///     Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        /// <returns>
        ///     true if this command can be executed; otherwise, false.
        /// </returns>
        [DebuggerStepThrough]
        public Boolean CanExecute(Object parameter)
        {
            return _canExecuteMethod == null || _canExecuteMethod((T)parameter);
        }

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        [DebuggerStepThrough]
        public void Execute(Object parameter)
        {
            _executeMethod((T)parameter);
        }

        #endregion
    }
}
