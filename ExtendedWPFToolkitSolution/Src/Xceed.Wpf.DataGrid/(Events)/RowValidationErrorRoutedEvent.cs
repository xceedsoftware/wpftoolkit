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
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  public delegate void RowValidationErrorRoutedEventHandler( object sender, RowValidationErrorRoutedEventArgs e );

  public class RowValidationErrorRoutedEventArgs : RoutedEventArgs
  {
    public RowValidationErrorRoutedEventArgs( RowValidationError rowValidationError )
      : base()
    {
      if( rowValidationError == null )
        throw new ArgumentNullException( "rowValidationError" );

      m_rowValidationError = rowValidationError;
    }

    public RowValidationErrorRoutedEventArgs( RoutedEvent routedEvent, RowValidationError rowValidationError )
      : base( routedEvent )
    {
      if( rowValidationError == null )
        throw new ArgumentNullException( "rowValidationError" );

      m_rowValidationError = rowValidationError;
    }

    public RowValidationErrorRoutedEventArgs( RoutedEvent routedEvent, object source, RowValidationError rowValidationError )
      : base( routedEvent, source )
    {
      if( rowValidationError == null )
        throw new ArgumentNullException( "rowValidationError" );

      m_rowValidationError = rowValidationError;
    }

    #region ValidationError PROPERTY

    public RowValidationError RowValidationError
    {
      get
      {
        return m_rowValidationError;
      }
    }

    private RowValidationError m_rowValidationError;

    #endregion ValidationError PROPERTY
  }
}
