﻿/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  public class DataGridVirtualizingQueryableCollectionViewSource : DataGridVirtualizingCollectionViewSourceBase
  {
    #region CONSTRUCTORS

    static DataGridVirtualizingQueryableCollectionViewSource()
    {
      CollectionViewSource.CollectionViewTypeProperty.OverrideMetadata(
        typeof( DataGridVirtualizingQueryableCollectionViewSource ),
        new FrameworkPropertyMetadata( typeof( DataGridVirtualizingQueryableCollectionView ) ) );
    }

    public DataGridVirtualizingQueryableCollectionViewSource()
      : base()
    {
    }

    #endregion CONSTRUCTORS

    #region QueryableSource

    public static readonly DependencyProperty QueryableSourceProperty = DependencyProperty.Register(
      "QueryableSource", typeof( IQueryable ), typeof( DataGridVirtualizingQueryableCollectionViewSource ),
      new FrameworkPropertyMetadata(
        null,
        new PropertyChangedCallback( DataGridVirtualizingQueryableCollectionViewSource.OnQueryableSourcePropertyChanged ) ) );

    public IQueryable QueryableSource
    {
      get { return ( IQueryable )GetValue( DataGridVirtualizingQueryableCollectionViewSource.QueryableSourceProperty ); }
      set { this.SetValue( DataGridVirtualizingQueryableCollectionViewSource.QueryableSourceProperty, value ); }
    }

    private static void OnQueryableSourcePropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridCollectionViewSourceBase source = sender as DataGridCollectionViewSourceBase;

      if( source == null )
        return;

      source.AdviseForwardedPropertyChanged();

      if( source.DataSourceProvider != null )
        source.DataSourceProvider.DelayRefresh( source.Dispatcher, System.Windows.Threading.DispatcherPriority.DataBind );
    }

    #endregion QueryableSource

    #region OVERRIDES

    internal override DataGridCollectionViewBaseDataProvider CreateDataProvider()
    {
      return new DataGridVirtualizingQueryableCollectionViewDataProvider( this );
    }

    #endregion OVERRIDES
  }
}
