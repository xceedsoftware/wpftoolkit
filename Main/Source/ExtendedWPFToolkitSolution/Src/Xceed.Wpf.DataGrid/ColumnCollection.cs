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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  public class ColumnCollection : ObservableCollection<ColumnBase>
  {

    internal ColumnCollection( DataGridControl dataGridControl, DetailConfiguration parentDetailConfiguration )
      : base()
    {
      this.DataGridControl = dataGridControl;
      this.ParentDetailConfiguration = parentDetailConfiguration;
    }

    public ColumnBase this[ string fieldName ]
    {
      get
      {
        ColumnBase column = null;

        m_fieldNameToColumns.TryGetValue( fieldName, out column );

        return column;
      }
    }

    // PUBLIC PROPERTIES

    #region MainColumn Property

    public ColumnBase MainColumn
    {
      get
      {
        return m_mainColumn;
      }
      set
      {
        if( m_mainColumn == value )
          return;

        m_mainColumn = value;
        this.OnPropertyChanged( new PropertyChangedEventArgs( "MainColumn" ) );
      }
    }

    private ColumnBase m_mainColumn;

    #endregion

    // INTERNAL PROPERTIES

    #region FieldNameToColumnDictionary Property

    internal Dictionary<string,ColumnBase> FieldNameToColumnDictionary
    {
      get
      {
        return m_fieldNameToColumns;
      }
    }

    #endregion FieldNameToColumnDictionary Property

    #region DataGridControl Property

    internal DataGridControl DataGridControl
    {
      get
      {
        return m_dataGridControl;
      }

      set
      {
        m_dataGridControl = value;

        foreach( ColumnBase column in this )
        {
          column.NotifyDataGridControlChanged();
        }
      }
    }

    private DataGridControl m_dataGridControl;

    #endregion DataGridControl Property

    #region ParentDetailConfiguration Property

    internal DetailConfiguration ParentDetailConfiguration
    {
      get;
      set;
    }

    #endregion ParentDetailConfiguration Property

    #region ProcessingVisibleColumnsUpdate Property

    internal bool ProcessingVisibleColumnsUpdate
    {
      get
      {
        return m_processingVisibleColumnsUpdate;
      }
      set
      {
        m_processingVisibleColumnsUpdate = value;
      }
    }

    private bool m_processingVisibleColumnsUpdate; // = false

    #endregion

    #region VisibleColumnsUpdateDelayed Property

    internal bool VisibleColumnsUpdateDelayed
    {
      get
      {
        return m_visibleColumnsUpdateDelayed;
      }
      set
      {
        m_visibleColumnsUpdateDelayed = value;
      }
    }

    private bool m_visibleColumnsUpdateDelayed; // = false

    #endregion

    #region Column Event Handlers

    private void RegisterColumnChangedEvent( ColumnBase column )
    {
      column.TitleChanged += new EventHandler( this.OnColumnValueChanged );
      column.VisiblePositionChanged += new EventHandler( this.OnColumnValueChanged );
      column.VisiblePositionChanged += new EventHandler( this.OnColumnVisibilityChanged );
      column.VisibleChanged += new EventHandler( this.OnColumnVisibilityChanged );
    }

    private void UnregisterColumnChangedEvent( ColumnBase column )
    {
      column.TitleChanged -= new EventHandler( this.OnColumnValueChanged );
      column.VisiblePositionChanged -= new EventHandler( this.OnColumnValueChanged );
      column.VisiblePositionChanged -= new EventHandler( this.OnColumnVisibilityChanged );
      column.VisibleChanged -= new EventHandler( this.OnColumnVisibilityChanged );
    }

    private void OnColumnVisibilityChanged( object sender, EventArgs e )
    {
      if( this.ColumnVisibilityChanged != null )
        this.ColumnVisibilityChanged( this, new WrappedEventEventArgs( sender, e ) );
    }

    internal event EventHandler ColumnVisibilityChanged;


    private void OnColumnValueChanged( object sender, EventArgs e )
    {
    }

    #endregion Column Event Handlers

    // PROTECTED METHODS

    protected override void RemoveItem( int index )
    {
      ColumnBase column = this[ index ];

      if( column != null )
      {
        m_fieldNameToColumns.Remove( column.FieldName );

        this.UnregisterColumnChangedEvent( column );

        column.DetachFromContainingCollection();
      }

      base.RemoveItem( index );
    }

    protected override void InsertItem( int index, ColumnBase item )
    {
      if( item != null )
      {
        if( ( string.IsNullOrEmpty( item.FieldName ) == true ) && ( !DesignerProperties.GetIsInDesignMode( item ) ) )
        {
          throw new ArgumentException( "A column must have a fieldname.", "item" );
        }

        item.AttachToContainingCollection( this );

        if( m_fieldNameToColumns.ContainsKey( item.FieldName ) )
          throw new DataGridException( "A column with same field name already exists in collection." );

        m_fieldNameToColumns.Add( item.FieldName, item );

        this.RegisterColumnChangedEvent( item );
      }

      base.InsertItem( index, item );
    }

    protected override void ClearItems()
    {
      foreach( ColumnBase column in this )
      {
        this.UnregisterColumnChangedEvent( column );

        column.DetachFromContainingCollection();
      }

      m_fieldNameToColumns.Clear();

      base.ClearItems();
    }

    protected override void SetItem( int index, ColumnBase item )
    {
      if( ( item != null ) && ( string.IsNullOrEmpty( item.FieldName ) == true ) )
      {
        throw new ArgumentException( "A column must have a fieldname.", "item" );
      }

      ColumnBase column = this[ index ];

      if( ( column != null ) && ( column != item ) )
      {
        m_fieldNameToColumns.Remove( column.FieldName );

        this.UnregisterColumnChangedEvent( column );

        column.DetachFromContainingCollection();
      }

      if( ( item != null ) && ( column != item ) )
      {
        item.AttachToContainingCollection( this );

        this.RegisterColumnChangedEvent( item );

        m_fieldNameToColumns.Add( item.FieldName, item );
      }

      base.SetItem( index, item );
    }

    protected override void OnCollectionChanged( System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
    {
      if( ( m_columnAdditionMessagesDeferedCount > 0 )
        && ( e.Action == NotifyCollectionChangedAction.Add ) && ( e.NewStartingIndex == ( this.Count - e.NewItems.Count ) ) )
      {
        if( m_deferedColumnAdditionMessageStartIndex == -1 )
        {
          m_deferedColumnAdditionMessageStartIndex = e.NewStartingIndex;
        }

        foreach( object item in e.NewItems )
        {
          m_deferedColumns.Add( ( ColumnBase )item );
        }
      }
      else
      {
        base.OnCollectionChanged( e );
      }
    }

    // INTERNAL METHODS

    internal IDisposable DeferColumnAdditionMessages()
    {
      return new DeferedColumnAdditionMessageDisposable( this );
    }

    internal void NotifyVisibleColumnsUpdating()
    {
      if( this.VisibleColumnsUpdating != null )
      {
        this.VisibleColumnsUpdating( this, EventArgs.Empty );
      }
    }

    internal event EventHandler VisibleColumnsUpdating;

    internal void NotifyVisibleColumnsUpdated()
    {
      if( this.VisibleColumnsUpdated != null )
      {
        this.VisibleColumnsUpdated( this, EventArgs.Empty );
      }
    }

    internal event EventHandler VisibleColumnsUpdated;

    internal void OnRealizedContainersRequested( object sender, RealizedContainersRequestedEventArgs e )
    {
      if( this.RealizedContainersRequested != null )
      {
        this.RealizedContainersRequested( this, e );
      }
    }

    internal event RealizedContainersRequestedEventHandler RealizedContainersRequested;

    internal void OnDistinctValuesRequested( object sender, DistinctValuesRequestedEventArgs e )
    {
      if( this.DistinctValuesRequested != null )
      {
        this.DistinctValuesRequested( this, e );
      }
    }

    internal event DistinctValuesRequestedEventHandler DistinctValuesRequested;

    internal void OnActualWidthChanged( object sender, ColumnActualWidthChangedEventArgs e )
    {
      if( this.ActualWidthChanged != null )
      {
        this.ActualWidthChanged( this, e );
      }
    }

    internal event ColumnActualWidthChangedHandler ActualWidthChanged;

    private int m_columnAdditionMessagesDeferedCount = 0;
    private int m_deferedColumnAdditionMessageStartIndex = -1;
    private List<ColumnBase> m_deferedColumns = new List<ColumnBase>();
    private Dictionary<string, ColumnBase> m_fieldNameToColumns = new Dictionary<string, ColumnBase>(); // To optimize indexing speed for FieldName

    private sealed class DeferedColumnAdditionMessageDisposable : IDisposable
    {
      public DeferedColumnAdditionMessageDisposable( ColumnCollection columns )
      {
        if( columns == null )
          throw new ArgumentNullException( "columns" );

        m_columns = columns;

        m_columns.m_columnAdditionMessagesDeferedCount++;
      }

      #region IDisposable Members

      public void Dispose()
      {
        m_columns.m_columnAdditionMessagesDeferedCount--;

        if( m_columns.m_columnAdditionMessagesDeferedCount == 0 )
        {
          if( m_columns.m_deferedColumns.Count > 0 )
          {
            m_columns.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, m_columns.m_deferedColumns, m_columns.m_deferedColumnAdditionMessageStartIndex ) );
          }

          m_columns.m_deferedColumnAdditionMessageStartIndex = -1;
          m_columns.m_deferedColumns.Clear();
        }
      }

      #endregion

      private ColumnCollection m_columns; // = null
    }

    internal class WrappedEventEventArgs : EventArgs
    {
      public WrappedEventEventArgs( object sender, EventArgs eventArgs )
      {
        if( sender == null )
          throw new ArgumentNullException( "sender" );

        if( eventArgs == null )
          throw new ArgumentNullException( "eventArgs" );

        this.WrappedSender = sender;
        this.WrappedEventArgs = eventArgs;
      }

      public object WrappedSender { get; private set; }
      public object WrappedEventArgs { get; private set; }
    }
  }
}
