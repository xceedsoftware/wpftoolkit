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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid
{
  public class ObservableColumnCollection : ObservableCollection<ColumnBase>
  {
    public ColumnBase this[ string fieldName ]
    {
      get
      {
        ColumnBase column = null;

        m_fieldNameToColumns.TryGetValue( fieldName, out column );

        return column;
      }
    }

    #region ProcessingVisibleColumnsUpdate Property

    internal AutoResetFlag ProcessingVisibleColumnsUpdate
    {
      get
      {
        return m_processingVisibleColumnsUpdate;
      }
    }

    private readonly AutoResetFlag m_processingVisibleColumnsUpdate = AutoResetFlagFactory.Create();

    #endregion

    #region ProcessingCollectionChangedUpdate Property

    internal bool ProcessingCollectionChangedUpdate
    {
      get;
      set;
    }

    #endregion

    protected override void RemoveItem( int index )
    {
      ColumnBase column = this[ index ];

      if( column != null )
      {
        this.UnregisterColumnChangedEvent( column );

        m_fieldNameToColumns.Remove( column.FieldName );
      }

      base.RemoveItem( index );
    }

    protected override void InsertItem( int index, ColumnBase item )
    {
      if( item != null )
      {
        if( ( item != null ) && ( string.IsNullOrEmpty( item.FieldName ) == true ) )
          throw new ArgumentException( "A column must have a fieldname.", "item" );

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
      }

      m_fieldNameToColumns.Clear();

      base.ClearItems();
    }

    protected override void SetItem( int index, ColumnBase item )
    {
      if( ( item != null ) && ( string.IsNullOrEmpty( item.FieldName ) == true ) )
        throw new ArgumentException( "A column must have a fieldname.", "item" );

      if( m_fieldNameToColumns.ContainsKey( item.FieldName ) )
        throw new DataGridException( "A column with same field name already exists in collection." );

      ColumnBase column = this[ index ];

      if( ( column != null ) && ( column != item ) )
      {
        this.UnregisterColumnChangedEvent( column );

        m_fieldNameToColumns.Remove( column.FieldName );
      }

      if( ( item != null ) && ( column != item ) )
      {
        m_fieldNameToColumns.Add( item.FieldName, item );

        this.RegisterColumnChangedEvent( item );
      }

      base.SetItem( index, item );
    }

    protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      if( ( m_columnAdditionMessagesDeferedCount > 0 ) && ( e.Action == NotifyCollectionChangedAction.Add ) && ( e.NewStartingIndex == ( this.Count - e.NewItems.Count ) ) )
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

    internal IDisposable DeferColumnAdditionMessages()
    {
      return new DeferedColumnAdditionMessageDisposable( this );
    }

    internal event EventHandler ColumnVisibilityChanged;

    private void RegisterColumnChangedEvent( ColumnBase column )
    {
      column.VisiblePositionChanged += new EventHandler( this.OnColumnVisibilityChanged );
      column.VisibleChanged += new EventHandler( this.OnColumnVisibilityChanged );
    }

    private void UnregisterColumnChangedEvent( ColumnBase column )
    {
      column.VisiblePositionChanged -= new EventHandler( this.OnColumnVisibilityChanged );
      column.VisibleChanged -= new EventHandler( this.OnColumnVisibilityChanged );
    }

    private void OnColumnVisibilityChanged( object sender, EventArgs e )
    {
      if( this.ColumnVisibilityChanged != null )
      {
        this.ColumnVisibilityChanged( this, new ColumnCollection.WrappedEventEventArgs( sender, e ) );
      }
    }

    private int m_columnAdditionMessagesDeferedCount = 0;
    private int m_deferedColumnAdditionMessageStartIndex = -1;
    private List<ColumnBase> m_deferedColumns = new List<ColumnBase>();
    private Dictionary<string, ColumnBase> m_fieldNameToColumns = new Dictionary<string, ColumnBase>(); // To optimize indexing speed for FieldName

    private sealed class DeferedColumnAdditionMessageDisposable : IDisposable
    {
      internal DeferedColumnAdditionMessageDisposable( ObservableColumnCollection columns )
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

      private ObservableColumnCollection m_columns; // = null
    }
  }
}
