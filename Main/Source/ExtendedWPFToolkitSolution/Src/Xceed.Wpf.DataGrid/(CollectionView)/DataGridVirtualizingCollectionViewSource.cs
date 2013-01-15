/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  public class DataGridVirtualizingCollectionViewSource : DataGridVirtualizingCollectionViewSourceBase
  {
    #region CONSTRUCTORS

    static DataGridVirtualizingCollectionViewSource()
    {
      CollectionViewSource.CollectionViewTypeProperty.OverrideMetadata(
        typeof( DataGridVirtualizingCollectionViewSource ),
        new FrameworkPropertyMetadata( typeof( DataGridVirtualizingCollectionView ) ) );
    }

    public DataGridVirtualizingCollectionViewSource()
      : base()
    {
    }

    #endregion CONSTRUCTORS

    #region DATA VIRTUALIZATION EVENTS

    public event EventHandler<QueryItemCountEventArgs> QueryItemCount;

    internal void OnQueryItemCount( QueryItemCountEventArgs e )
    {
      if( this.QueryItemCount != null )
        this.QueryItemCount( this, e );
    }

    public event EventHandler<QueryGroupsEventArgs> QueryGroups;

    internal void OnQueryGroups( QueryGroupsEventArgs e )
    {
      if( this.QueryGroups != null )
        this.QueryGroups( this, e );
    }

    public event EventHandler<QueryItemsEventArgs> AbortQueryItems;

    internal void OnAbortQueryItems( QueryItemsEventArgs e )
    {
      if( this.AbortQueryItems != null )
        this.AbortQueryItems( this, e );
    }

    public event EventHandler<QueryItemsEventArgs> QueryItems;

    internal void OnQueryItems( QueryItemsEventArgs e )
    {
      if( this.QueryItems != null )
        this.QueryItems( this, e );
    }

    internal event EventHandler<QueryAutoFilterDistinctValuesEventArgs> QueryAutoFilterDistinctValues;

    internal void OnQueryAutoFilterDistinctValues( QueryAutoFilterDistinctValuesEventArgs e )
    {
      if( this.QueryAutoFilterDistinctValues != null )
        this.QueryAutoFilterDistinctValues( this, e );
    }

    #endregion DATA VIRTUALIZATION EVENTS


    internal override DataGridCollectionViewBaseDataProvider CreateDataProvider()
    {
      return new DataGridVirtualizingCollectionViewDataProvider( this );
    }

  }
}
