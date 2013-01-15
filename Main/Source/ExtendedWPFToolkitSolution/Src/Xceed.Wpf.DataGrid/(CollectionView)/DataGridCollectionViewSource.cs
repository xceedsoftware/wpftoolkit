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
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Collections;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;
using Xceed.Wpf.DataGrid.Stats;

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
      m_statFunctions = new StatFunctionCollection();
      m_statFunctions.CollectionChanged += new NotifyCollectionChangedEventHandler( this.ForwardedCollection_CollectionChanged );
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

    #region StatFunctions Property

    internal ObservableCollection<Stats.StatFunction> StatFunctions
    {
      get
      {
        return m_statFunctions;
      }
    }

    private ObservableCollection<Stats.StatFunction> m_statFunctions;

    #endregion StatFunctions Property

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

      DataGridCollectionView dataGridCollectionView = currentView as DataGridCollectionView;

      dataGridCollectionView.StatFunctions.Clear();
      int count = m_statFunctions.Count;

      for( int i = 0; i < count; i++ )
      {
        dataGridCollectionView.StatFunctions.Add( m_statFunctions[ i ] );
      }

      dataGridCollectionView.UpdateChangedPropertyStatsOnly = this.UpdateChangedPropertyStatsOnly;
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
