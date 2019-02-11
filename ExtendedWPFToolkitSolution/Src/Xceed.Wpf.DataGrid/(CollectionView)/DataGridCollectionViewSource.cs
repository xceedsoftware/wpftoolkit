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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  public sealed class DataGridCollectionViewSource : DataGridCollectionViewSourceBase
  {
    #region CONSTRUCTORS

    static DataGridCollectionViewSource()
    {
      CollectionViewSource.CollectionViewTypeProperty.OverrideMetadata(
        typeof( DataGridCollectionViewSource ),
        new FrameworkPropertyMetadata( typeof( DataGridCollectionView ) ) );
    }

    public DataGridCollectionViewSource()
      : base()
    {
    }

    #endregion CONSTRUCTORS

    #region CollectionViewType Property

    protected override void OnCollectionViewTypeChanged(
      Type oldCollectionViewType,
      Type newCollectionViewType )
    {
      if( newCollectionViewType != typeof( DataGridCollectionView ) )
        throw new InvalidOperationException( "An attempt was made to use a view other than DataGridCollectionView." );

      base.OnCollectionViewTypeChanged( oldCollectionViewType, newCollectionViewType );
    }

    #endregion CollectionViewType Property

    #region UpdateChangedPropertyStatsOnly Property

    internal bool UpdateChangedPropertyStatsOnly
    {
      get;
      set;
    }

    #endregion

    #region SourceItems Property

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The SourceItems property is obsolete and should no longer be used. Unbound data should be wrapped in a collection and assigned to the Source property instead.", true )]
    public ObservableCollection<object> SourceItems
    {
      get
      {
        throw new NotSupportedException( "The SourceItems property is obsolete and should no longer be used. Unbound data should be wrapped in a collection and assigned to the Source property instead." );
      }
    }

    #endregion SourceItems Property

    #region INTERNAL METHODS

    internal override DataGridCollectionViewBaseDataProvider CreateDataProvider()
    {
      return new DataGridCollectionViewDataProvider( this );
    }

    internal override void ApplyExtraPropertiesToView( DataGridCollectionViewBase currentView )
    {
      base.ApplyExtraPropertiesToView( currentView );     
    }

    #endregion INTERNAL METHODS

    #region PRIVATE METHODS

    private static void OnDataGridCollectionViewSourceDependencyPropertyChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      DataGridCollectionViewSource source = o as DataGridCollectionViewSource;

      if( source == null )
        return;

      source.AdviseForwardedPropertyChanged();
    }

    #endregion PRIVATE METHODS
  }
}
