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
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class NotifyRangeCollectionChangedEventArgs : NotifyCollectionChangedEventArgs
  {
    #region Constructor

    internal NotifyRangeCollectionChangedEventArgs( NotifyCollectionChangedEventArgs args )
      : base( NotifyCollectionChangedAction.Reset )
    {
      if( args == null )
        throw new ArgumentNullException( "args" );

      m_originalEventArgs = args;
    }

    #endregion

    #region OriginalEventArgs Property

    internal NotifyCollectionChangedEventArgs OriginalEventArgs
    {
      get
      {
        return m_originalEventArgs;
      }
    }

    private readonly NotifyCollectionChangedEventArgs m_originalEventArgs;

    #endregion
  }
}
