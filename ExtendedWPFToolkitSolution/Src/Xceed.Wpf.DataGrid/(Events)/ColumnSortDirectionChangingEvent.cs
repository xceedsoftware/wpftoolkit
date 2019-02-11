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
  public delegate void ColumnSortDirectionChangingEventHandler( object sender, ColumnSortDirectionChangingEventArgs e );

  public class ColumnSortDirectionChangingEventArgs : RoutedEventArgs
  {
    public ColumnSortDirectionChangingEventArgs( RoutedEvent routedEvent, ColumnBase column, DataGridContext dataGridContext, SortDirection currentSortDirection, SortDirection nextSortDirection )
      : base( routedEvent, column )
    {
      if( column == null )
        throw new ArgumentNullException( "column" );

      if( dataGridContext == null )
        throw new ArgumentNullException( "dataGridContext" );

      m_dataGridContext = dataGridContext;
      m_current = currentSortDirection;
      m_next = nextSortDirection;
    }

    #region Column Property

    public ColumnBase Column
    {
      get
      {
        return ( ColumnBase )this.OriginalSource;
      }
    }

    #endregion

    #region DataGridContext Property

    public DataGridContext DataGridContext
    {
      get
      {
        return m_dataGridContext;
      }
    }

    private DataGridContext m_dataGridContext;

    #endregion

    #region CurrentSortDirection Property

    public SortDirection CurrentSortDirection
    {
      get
      {
        return m_current;
      }
    }

    private SortDirection m_current;

    #endregion

    #region NextSortDirection Property

    public SortDirection NextSortDirection
    {
      get
      {
        return m_next;
      }
      set
      {
        m_next = value;
      }
    }

    private SortDirection m_next;

    #endregion
  }
}
