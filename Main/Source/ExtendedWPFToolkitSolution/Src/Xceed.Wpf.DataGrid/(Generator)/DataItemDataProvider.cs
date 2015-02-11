/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class DataItemDataProvider : DataItemDataProviderBase
  {
    #region Static Fields

    private static readonly object EmptyDataItem = new object();

    #endregion

    #region IsEmpty Property

    public override bool IsEmpty
    {
      get
      {
        return m_isEmpty;
      }
    }

    private void SetIsEmpty( bool value )
    {
      if( m_isEmpty == value )
        return;

      m_isEmpty = value;

      this.OnPropertyChanged( new PropertyChangedEventArgs( "IsEmpty" ) );
    }

    private bool m_isEmpty = true;

    #endregion

    public override void SetDataItem( object dataItem )
    {
      if( m_dataItem == dataItem )
        return;

      m_dataItem = dataItem;

      if( this.IsRefreshDeferred )
        return;

      this.Refresh();
    }

    public override void ClearDataItem()
    {
      if( m_dataItem == DataItemDataProvider.EmptyDataItem )
        return;

      m_dataItem = DataItemDataProvider.EmptyDataItem;

      if( this.IsRefreshDeferred )
        return;

      this.Refresh();
    }

    protected override void BeginQuery()
    {
      if( this.IsRefreshDeferred )
        return;

      base.BeginQuery();

      if( m_dataItem == DataItemDataProvider.EmptyDataItem )
      {
        this.OnQueryFinished( null, null, this.OnDataChanged, true );
      }
      else
      {
        this.OnQueryFinished( m_dataItem, null, this.OnDataChanged, false );
      }
    }

    private object OnDataChanged( object arg )
    {
      this.SetIsEmpty( ( bool )arg );

      return null;
    }

    #region Private Fields

    private object m_dataItem = DataItemDataProvider.EmptyDataItem;

    #endregion
  }
}
