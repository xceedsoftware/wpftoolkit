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
using System.ComponentModel;
using System.Diagnostics;
using Xceed.Wpf.DataGrid.Utils;

namespace Xceed.Wpf.DataGrid
{
  public class ColumnCollection : ObservableColumnCollection
  {
    #region Static Fields

    internal static readonly string MainColumnPropertyName = PropertyHelper.GetPropertyName( ( ColumnCollection c ) => c.MainColumn );

    #endregion


    internal ColumnCollection( DataGridControl dataGridControl, DetailConfiguration parentDetailConfiguration )
    {
      m_dataGridControl = dataGridControl;
      m_parentDetailConfiguration = parentDetailConfiguration;
    }

    #region MainColumn Property

    public virtual ColumnBase MainColumn
    {
      get
      {
        return m_mainColumn;
      }
      set
      {
        if( value == m_mainColumn )
          return;

        if( ( value != null ) && !this.Contains( value ) )
          throw new ArgumentException( "The column was not found in the collection." );

        using( this.DeferColumnsUpdate() )
        {
          var oldMainColumn = m_mainColumn;
          m_mainColumn = value;

          if( m_mainColumn != null )
          {
            m_mainColumn.RaiseIsMainColumnChanged();
          }

          if( oldMainColumn != null )
          {
            oldMainColumn.RaiseIsMainColumnChanged();
          }
        }

        this.OnPropertyChanged( new PropertyChangedEventArgs( ColumnCollection.MainColumnPropertyName ) );
      }
    }

    private ColumnBase m_mainColumn;

    #endregion

    #region DataGridControl Internal Property

    internal DataGridControl DataGridControl
    {
      get
      {
        return m_dataGridControl;
      }
      set
      {
        if( value == m_dataGridControl )
          return;

        m_dataGridControl = value;

        foreach( var column in this )
        {
          column.NotifyDataGridControlChanged();
        }
      }
    }

    private DataGridControl m_dataGridControl;

    #endregion

    #region ParentDetailConfiguration Internal Property

    internal DetailConfiguration ParentDetailConfiguration
    {
      get
      {
        return m_parentDetailConfiguration;
      }
      set
      {
        m_parentDetailConfiguration = value;
      }
    }

    private DetailConfiguration m_parentDetailConfiguration;

    #endregion

    #region RealizedContainersRequested Internal Event

    internal event RealizedContainersRequestedEventHandler RealizedContainersRequested;

    private void OnRealizedContainersRequested( RealizedContainersRequestedEventArgs e )
    {
      var handler = this.RealizedContainersRequested;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    #region DistinctValuesRequested Internal Event

    internal event DistinctValuesRequestedEventHandler DistinctValuesRequested;

    private void OnDistinctValuesRequested( DistinctValuesRequestedEventArgs e )
    {
      var handler = this.DistinctValuesRequested;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    #region ActualWidthChanged Internal Event

    internal event ColumnActualWidthChangedEventHandler ActualWidthChanged;

    private void OnActualWidthChanged( ColumnActualWidthChangedEventArgs e )
    {
      var handler = this.ActualWidthChanged;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    protected override void OnItemAdding( ColumnBase item )
    {
      base.OnItemAdding( item );

      Debug.Assert( item != null );

      if( item.IsMainColumn )
      {
        m_nextMainColumn = item;
      }

      item.AttachToContainingCollection( this );

      item.PropertyChanged += new PropertyChangedEventHandler( this.OnColumnPropertyChanged );
      item.RealizedContainersRequested += new RealizedContainersRequestedEventHandler( this.OnColumnRealizedContainersRequested );
      item.ActualWidthChanged += new ColumnActualWidthChangedEventHandler( this.OnColumnActualWidthChanged );
      item.DistinctValuesRequested += new DistinctValuesRequestedEventHandler( this.OnColumnDistinctValuesRequested );
    }

    protected override void OnItemAdded( ColumnBase item )
    {
      base.OnItemAdded( item );

      this.UpdateMainColumn();
    }

    protected override void OnItemRemoving( ColumnBase item )
    {
      Debug.Assert( item != null );

      item.PropertyChanged -= new PropertyChangedEventHandler( this.OnColumnPropertyChanged );
      item.RealizedContainersRequested -= new RealizedContainersRequestedEventHandler( this.OnColumnRealizedContainersRequested );
      item.ActualWidthChanged -= new ColumnActualWidthChangedEventHandler( this.OnColumnActualWidthChanged );
      item.DistinctValuesRequested -= new DistinctValuesRequestedEventHandler( this.OnColumnDistinctValuesRequested );

      item.DetachFromContainingCollection();

      base.OnItemRemoving( item );
    }

    protected override void OnItemRemoved( ColumnBase item )
    {
      base.OnItemRemoved( item );

      this.UpdateMainColumn();
    }

    private void UpdateMainColumn()
    {
      switch( this.Count )
      {
        case 0:
          this.MainColumn = null;
          break;

        case 1:
          this.MainColumn = this[ 0 ];
          break;

        default:
          {
            var column = m_nextMainColumn ?? m_mainColumn;

            m_nextMainColumn = null;

            if( ( column != null ) && this.Contains( column ) )
            {
              this.MainColumn = column;
            }
            else
            {
              this.MainColumn = null;
            }
          }
          break;
      }
    }

    private IDisposable DeferColumnsUpdate()
    {
      if( m_parentDetailConfiguration != null )
        return m_parentDetailConfiguration.DeferColumnsUpdate();

      if( m_dataGridControl != null )
        return m_dataGridControl.DeferColumnsUpdate();

      return new EmptyDisposable();
    }

    private void OnColumnPropertyChanged( object sender, PropertyChangedEventArgs e )
    {
    }

    private void OnColumnRealizedContainersRequested( object sender, RealizedContainersRequestedEventArgs e )
    {
      this.OnRealizedContainersRequested( e );
    }

    private void OnColumnDistinctValuesRequested( object sender, DistinctValuesRequestedEventArgs e )
    {
      this.OnDistinctValuesRequested( e );
    }

    private void OnColumnActualWidthChanged( object sender, ColumnActualWidthChangedEventArgs e )
    {
      this.OnActualWidthChanged( e );
    }

    private ColumnBase m_nextMainColumn; //null

    #region EmptyDisposable Private Class

    private sealed class EmptyDisposable : IDisposable
    {
      void IDisposable.Dispose()
      {
      }
    }

    #endregion
  }
}
