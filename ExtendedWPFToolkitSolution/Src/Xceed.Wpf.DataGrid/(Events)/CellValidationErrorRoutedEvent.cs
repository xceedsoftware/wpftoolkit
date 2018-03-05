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
  public delegate void CellValidationErrorRoutedEventHandler( object sender, CellValidationErrorRoutedEventArgs e );

  public class CellValidationErrorRoutedEventArgs : RoutedEventArgs
  {
    public CellValidationErrorRoutedEventArgs( CellValidationError cellValidationError )
      : base()
    {
      if( cellValidationError == null )
        throw new ArgumentNullException( "cellValidationError" );

      m_cellValidationError = cellValidationError;
    }

    public CellValidationErrorRoutedEventArgs( RoutedEvent routedEvent, CellValidationError cellValidationError )
      : base( routedEvent )
    {
      if( cellValidationError == null )
        throw new ArgumentNullException( "cellValidationError" );

      m_cellValidationError = cellValidationError;
    }

    public CellValidationErrorRoutedEventArgs( RoutedEvent routedEvent, object source, CellValidationError cellValidationError )
      : base( routedEvent, source )
    {
      if( cellValidationError == null )
        throw new ArgumentNullException( "cellValidationError" );

      m_cellValidationError = cellValidationError;
    }

    #region CellValidationError PROPERTY

    public CellValidationError CellValidationError
    {
      get
      {
        return m_cellValidationError;
      }
    }

    private CellValidationError m_cellValidationError;

    #endregion CellValidationError PROPERTY
  }
}
