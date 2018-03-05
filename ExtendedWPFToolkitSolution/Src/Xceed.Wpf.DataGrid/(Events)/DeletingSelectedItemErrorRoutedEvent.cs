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
  public delegate void DeletingSelectedItemErrorRoutedEventHandler( object sender, DeletingSelectedItemErrorRoutedEventArgs e );

  public class DeletingSelectedItemErrorRoutedEventArgs : RoutedEventArgs
  {
    #region CONSTRUCTORS

    public DeletingSelectedItemErrorRoutedEventArgs( object item, Exception exception )
      : base()
    {
      m_item = item;
      m_exception = exception;
    }

    public DeletingSelectedItemErrorRoutedEventArgs( object item, Exception exception, RoutedEvent routedEvent )
      : base( routedEvent )
    {
      m_item = item;
      m_exception = exception;
    }

    public DeletingSelectedItemErrorRoutedEventArgs( object item, Exception exception, RoutedEvent routedEvent, object source )
      : base( routedEvent, source )
    {
      m_item = item;
      m_exception = exception;
    }

    #endregion CONSTRUCTORS

    #region Item Property

    public object Item
    {
      get
      {
        return m_item;
      }
    }

    #endregion Item Property

    #region Exception Property

    public Exception Exception
    {
      get
      {
        return m_exception;
      }
    }

    #endregion Exception Property

    #region Action Property

    public DeletingSelectedItemErrorAction Action
    {
      get
      {
        return m_action;
      }
      set
      {
        m_action = value;
      }
    }

    #endregion Action Property

    #region PRIVATE FIELDS

    private object m_item;
    private Exception m_exception;
    private DeletingSelectedItemErrorAction m_action = DeletingSelectedItemErrorAction.Skip;

    #endregion PRIVATE FIELDS
  }
}
