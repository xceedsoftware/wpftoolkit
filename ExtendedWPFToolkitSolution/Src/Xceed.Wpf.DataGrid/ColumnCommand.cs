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
using System.Diagnostics;
using System.Windows.Input;

namespace Xceed.Wpf.DataGrid
{
  internal abstract class ColumnCommand : ICommand
  {
    #region Validation Methods

    protected static void ThrowIfNull( object value, string paramName )
    {
      if( value == null )
        throw new ArgumentNullException( paramName );
    }

    #endregion

    protected abstract bool CanExecuteImpl( object parameter );
    protected abstract void ExecuteImpl( object parameter );

    #region ICommand Members

    event EventHandler ICommand.CanExecuteChanged
    {
      add
      {
      }
      remove
      {
      }
    }

    bool ICommand.CanExecute( object parameter )
    {
      return this.CanExecuteImpl( parameter );
    }

    void ICommand.Execute( object parameter )
    {
      this.ExecuteImpl( parameter );
    }

    #endregion
  }
}
