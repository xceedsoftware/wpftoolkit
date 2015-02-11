/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Threading;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class EmptyDataItemDataProvider : DataItemDataProviderBase
  {
    #region Constructor

    private EmptyDataItemDataProvider()
    {
    }

    #endregion

    #region Instance Static Property

    internal static EmptyDataItemDataProvider Instance
    {
      get
      {
        if( m_instance == null )
        {
          Interlocked.CompareExchange<EmptyDataItemDataProvider>( ref EmptyDataItemDataProvider.m_instance, new EmptyDataItemDataProvider(), null );
        }

        return m_instance;
      }
    }

    private static EmptyDataItemDataProvider m_instance;

    #endregion

    #region IsEmpty Property

    public override bool IsEmpty
    {
      get
      {
        return true;
      }
    }

    #endregion

    public override void SetDataItem( object dataItem )
    {
    }

    public override void ClearDataItem()
    {
    }
  }
}
