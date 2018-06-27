/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Windows.Input;

namespace Xceed.Wpf.AvalonDock.Commands
{
  internal class RelayCommand : ICommand
  {
    #region Fields

    private readonly Action<object> _execute;
    private readonly Predicate<object> _canExecute;

    #endregion // Fields

    #region Constructors

    public RelayCommand( Action<object> execute )
        : this( execute, null )
    {
    }

    public RelayCommand( Action<object> execute, Predicate<object> canExecute )
    {
      if( execute == null )
        throw new ArgumentNullException( "execute" );

      _execute = execute;
      _canExecute = canExecute;
    }
    #endregion // Constructors

    #region ICommand Members

    public bool CanExecute( object parameter )
    {
      return _canExecute == null ? true : _canExecute( parameter );
    }

    public event EventHandler CanExecuteChanged
    {
      add
      {
        CommandManager.RequerySuggested += value;
      }
      remove
      {
        CommandManager.RequerySuggested -= value;
      }
    }

    public void Execute( object parameter )
    {
      _execute( parameter );
    }

    #endregion // ICommand Members
  }
}
