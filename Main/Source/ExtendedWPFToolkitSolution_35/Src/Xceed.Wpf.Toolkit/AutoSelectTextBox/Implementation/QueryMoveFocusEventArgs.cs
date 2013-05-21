/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;
using System.Windows.Input;

namespace Xceed.Wpf.Toolkit
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1003:UseGenericEventHandlerInstances" )]
  public delegate void QueryMoveFocusEventHandler( object sender, QueryMoveFocusEventArgs e );

  public class QueryMoveFocusEventArgs : RoutedEventArgs
  {
    //default CTOR private to prevent its usage.
    private QueryMoveFocusEventArgs()
    {
    }

    //internal to prevent anybody from building this type of event.
    internal QueryMoveFocusEventArgs( FocusNavigationDirection direction, bool reachedMaxLength )
      : base( AutoSelectTextBox.QueryMoveFocusEvent )
    {
      m_navigationDirection = direction;
      m_reachedMaxLength = reachedMaxLength;
    }

    public FocusNavigationDirection FocusNavigationDirection
    {
      get
      {
        return m_navigationDirection;
      }
    }

    public bool ReachedMaxLength
    {
      get
      {
        return m_reachedMaxLength;
      }
    }

    public bool CanMoveFocus
    {
      get
      {
        return m_canMove;
      }
      set
      {
        m_canMove = value;
      }
    }

    private FocusNavigationDirection m_navigationDirection;
    private bool m_reachedMaxLength;
    private bool m_canMove = true; //defaults to true... if nobody does nothing, then its capable of moving focus.

  }
}
