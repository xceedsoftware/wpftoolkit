/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid
{
  public class DataGridVirtualizingQueryableCollectionViewSource : DataGridVirtualizingCollectionViewSourceBase
  {
    static DataGridVirtualizingQueryableCollectionViewSource()
    {
      CollectionViewSource.CollectionViewTypeProperty.OverrideMetadata(
        typeof( DataGridVirtualizingQueryableCollectionViewSource ),
        new FrameworkPropertyMetadata( typeof( DataGridVirtualizingQueryableCollectionView ) ) );
    }

    #region QueryableSource Property

    public static readonly DependencyProperty QueryableSourceProperty = DependencyProperty.Register(
      "QueryableSource",
      typeof( IQueryable ),
      typeof( DataGridVirtualizingQueryableCollectionViewSource ),
      new FrameworkPropertyMetadata(
        null,
        new PropertyChangedCallback( DataGridVirtualizingQueryableCollectionViewSource.OnQueryableSourcePropertyChanged ) ) );

    public IQueryable QueryableSource
    {
      get
      {
        return ( IQueryable )GetValue( DataGridVirtualizingQueryableCollectionViewSource.QueryableSourceProperty );
      }
      set
      {
        this.SetValue( DataGridVirtualizingQueryableCollectionViewSource.QueryableSourceProperty, value );
      }
    }

    private static void OnQueryableSourcePropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var source = sender as DataGridCollectionViewSourceBase;
      if( source == null )
        return;

      source.AdviseForwardedPropertyChanged();

      var provider = source.DataSourceProvider;
      if( provider != null )
      {
        provider.DelayRefresh( source.Dispatcher, DispatcherPriority.DataBind );
      }
    }

    #endregion

    internal override DataGridCollectionViewBaseDataProvider CreateDataProvider()
    {
      return new DataGridVirtualizingQueryableCollectionViewDataProvider( this );
    }
  }
}
