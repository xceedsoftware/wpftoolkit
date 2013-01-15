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
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  public class DataGridItemPropertyCollection : ObservableCollection<DataGridItemPropertyBase>
  {
    internal DataGridItemPropertyCollection()
      : base()
    {
    }

    public DataGridItemPropertyBase this[ string name ]
    {
      get
      {
        int index = this.IndexOf( name );

        if( index == -1 )
          return null;

        return this.Items[ index ];
      }
    }

    public int IndexOf( string name )
    {
      IList<DataGridItemPropertyBase> items = this.Items;
      int count = items.Count;

      for( int i = 0; i < count; i++ )
      {
        if( string.Equals( items[ i ].Name, name ) )
          return i;
      }

      return -1;
    }

    public event EventHandler FilterCriterionChanged;

    protected override void ClearItems()
    {
      DataGridItemPropertyBase[] oldItems = new DataGridItemPropertyBase[ this.Count ];
      this.CopyTo( oldItems, 0 );

      m_unboundItemPropertyCount = 0;
      base.ClearItems();

      for( int i = 0; i < oldItems.Length; i++ )
      {
        DataGridItemPropertyBase oldItem = oldItems[ i ];
        oldItem.FilterCriterionChanged -= new EventHandler( DataGridItemProperty_FilterCriterionChanged );
        oldItem.ValueChanged -= new EventHandler<DataGridItemPropertyBase.ValueChangedEventArgs>( DataGridItemProperty_ValueChanged );
      }
    }

    protected override void RemoveItem( int index )
    {
      DataGridItemPropertyBase oldItem = null;

      if( index < this.Count )
      {
        oldItem = this[ index ];
      }

      base.RemoveItem( index );

      if( oldItem != null )
      {
        if( oldItem is DataGridUnboundItemProperty )
        {
          m_unboundItemPropertyCount--;
        }

        oldItem.FilterCriterionChanged -= new EventHandler( DataGridItemProperty_FilterCriterionChanged );
        oldItem.ValueChanged -= new EventHandler<DataGridItemPropertyBase.ValueChangedEventArgs>( DataGridItemProperty_ValueChanged );
      }
    }

    protected override void SetItem( int index, DataGridItemPropertyBase item )
    {
      if( string.IsNullOrEmpty( item.Name ) )
        throw new InvalidOperationException( "An attempt was made to add an item that does not have a name." );

      DataGridItemPropertyBase oldItem = null;

      if( index < this.Count )
      {
        oldItem = this[ index ];
      }

      if( item == oldItem )
      {
        base.SetItem( index, item );
        return;
      }

      item.ValueChanged += new EventHandler<DataGridItemPropertyBase.ValueChangedEventArgs>( DataGridItemProperty_ValueChanged );
      item.SetUnspecifiedPropertiesValues( this );

      try
      {
        base.SetItem( index, item );
      }
      catch
      {
        item.ValueChanged -= new EventHandler<DataGridItemPropertyBase.ValueChangedEventArgs>( DataGridItemProperty_ValueChanged );
        throw;
      }

      if( item is DataGridUnboundItemProperty )
      {
        m_unboundItemPropertyCount++;
      }

      if( oldItem != null )
      {
        if( oldItem is DataGridUnboundItemProperty )
        {
          m_unboundItemPropertyCount--;
        }

        oldItem.FilterCriterionChanged -= new EventHandler( DataGridItemProperty_FilterCriterionChanged );
        oldItem.ValueChanged -= new EventHandler<DataGridItemPropertyBase.ValueChangedEventArgs>( DataGridItemProperty_ValueChanged );
      }

      item.FilterCriterionChanged += new EventHandler( DataGridItemProperty_FilterCriterionChanged );
      item.Initialized = true;
    }

    protected override void InsertItem( int index, DataGridItemPropertyBase item )
    {
      string itemName = item.Name;

      if( string.IsNullOrEmpty( itemName ) )
        throw new InvalidOperationException( "An attempt was made to add an item that does not have a name." );

      int count = this.Count;
      for( int i = 0; i < count; i++ )
      {
        if( String.Equals( itemName, this[ i ].Name ) )
          throw new InvalidOperationException( "An attempt was made to add an item that has the same name (" + itemName + ") as another item currently contained in the collection." );
      }

      item.ValueChanged += new EventHandler<DataGridItemPropertyBase.ValueChangedEventArgs>( DataGridItemProperty_ValueChanged );
      item.SetUnspecifiedPropertiesValues( this );
      item.Initialized = true;

      if( item is DataGridUnboundItemProperty )
      {
        m_unboundItemPropertyCount++;
      }

      try
      {
        base.InsertItem( index, item );
      }
      catch
      {
        item.ValueChanged -= new EventHandler<DataGridItemPropertyBase.ValueChangedEventArgs>( DataGridItemProperty_ValueChanged );
        item.Initialized = false;

        if( item is DataGridUnboundItemProperty )
        {
          m_unboundItemPropertyCount--;
        }

        throw;
      }

      item.FilterCriterionChanged += new EventHandler( DataGridItemProperty_FilterCriterionChanged );
    }

    internal List<DataGridItemPropertyBase> DefaultItemProperties
    {
      get
      {
        return m_defaultItemProperties;
      }
      set
      {
        if( m_defaultItemProperties == null )
        {
          m_defaultItemProperties = value;

          //if the default item properties were just set...
          //then go through all pre-existing DataGridItemProperty and try to give them the PropertyDescriptor, ValuePath or Title for matching default item property
          if( m_itemType != null )
          {
            //but only do it if the ItemType is set as well
            foreach( DataGridItemPropertyBase itemProperty in this )
            {
              itemProperty.SetUnspecifiedPropertiesValues( this );
            }
          }
        }
      }
    }

    internal Type ItemType
    {
      get
      {
        return m_itemType;
      }
      set
      {
        if( m_itemType == null )
        {
          m_itemType = value;

          //if the item type were just set...
          //then go through all pre-existing DataGridItemProperty and try to give then the PropertyDescriptor, ValuePath or Title for matching default item property
          if( m_defaultItemProperties != null )
          {
            //but only do it if the DefaultItemProperties is set as well
            foreach( DataGridItemPropertyBase itemProperty in this )
            {
              itemProperty.SetUnspecifiedPropertiesValues( this );
            }
          }
        }
      }
    }

    internal void UnregisterDataGridItemPropertiesEvents()
    {
      int count = this.Count;

      for( int i = 0; i < count; i++ )
      {
        DataGridItemPropertyBase item = this[ i ];

        item.FilterCriterionChanged -= this.DataGridItemProperty_FilterCriterionChanged;
        item.ValueChanged -= this.DataGridItemProperty_ValueChanged;
      }
    }

    internal DataGridItemPropertyBase FindDefaultItemProperty( string name )
    {
      if( m_defaultItemProperties != null )
      {
        foreach( DataGridItemPropertyBase itemProperty in m_defaultItemProperties )
        {
          if( string.Equals( itemProperty.Name, name ) )
            return itemProperty;
        }
      }

      return null;
    }

    internal void RefreshUnboundItemProperty( object component )
    {
      if( m_unboundItemPropertyCount > 0 )
      {
        if( this.IsUnboundItemPropertyInRefresh( component ) )
          return;

        if( this.IsUnboundItemPropertyChangedSuspended( component ) )
          return;

        this.AddUnboundItemPropertyInRefresh( component );
        UnboundDataItem unboundDataItem;
        UnboundDataItem.GetUnboundDataItemNode( component, out unboundDataItem );

        try
        {
          foreach( DataGridItemPropertyBase itemProperty in this )
          {
            DataGridUnboundItemProperty unboundItemProperty = itemProperty as DataGridUnboundItemProperty;

            if( unboundItemProperty != null )
            {
              unboundItemProperty.Refresh( unboundDataItem );
            }
          }
        }
        finally
        {
          this.RemoveUnboundItemPropertyInRefresh( component );
        }
      }
    }

    internal void SuspendUnboundItemPropertyChanged( object component )
    {
      if( m_unboundItemPropertyChangedSuspended == null )
        m_unboundItemPropertyChangedSuspended = new Hashtable( 1 );

      m_unboundItemPropertyChangedSuspended[ component ] = DBNull.Value;
    }

    internal void ResumeUnboundItemPropertyChanged( object component )
    {
      if( m_unboundItemPropertyChangedSuspended == null )
      {
        Debug.Assert( false, "Not suppose to resume when nothing is in the UnboundItemPropertyChangedSuspended list." );
        return;
      }

      m_unboundItemPropertyChangedSuspended.Remove( component );

      if( m_unboundItemPropertyChangedSuspended.Count == 0 )
        m_unboundItemPropertyChangedSuspended = null;

      this.RefreshUnboundItemProperty( component );
    }

    private bool IsUnboundItemPropertyChangedSuspended( object component )
    {
      if( m_unboundItemPropertyChangedSuspended == null )
        return false;

      return m_unboundItemPropertyChangedSuspended.Contains( component );
    }

    private void DataGridItemProperty_FilterCriterionChanged( object sender, EventArgs e )
    {
      if( this.FilterCriterionChanged != null )
        this.FilterCriterionChanged( this, EventArgs.Empty );
    }

    private void DataGridItemProperty_ValueChanged( object sender, DataGridItemPropertyBase.ValueChangedEventArgs e )
    {
      this.RefreshUnboundItemProperty( e.Component );
    }

    private bool IsUnboundItemPropertyInRefresh( object component )
    {
      if( m_unboundItemPropertyInRefresh == null )
        return false;

      return m_unboundItemPropertyInRefresh.Contains( component );
    }

    private void AddUnboundItemPropertyInRefresh( object component )
    {
      if( m_unboundItemPropertyInRefresh == null )
        m_unboundItemPropertyInRefresh = new Hashtable( 1 );

      object count = m_unboundItemPropertyInRefresh[ component ];

      if( count == null )
      {
        m_unboundItemPropertyInRefresh[ component ] = 1;
      }
      else
      {
        m_unboundItemPropertyInRefresh[ component ] = ( ( int )count ) + 1;
      }
    }

    private void RemoveUnboundItemPropertyInRefresh( object component )
    {
      if( m_unboundItemPropertyInRefresh == null )
      {
        Debug.Assert( false, "Not suppose to remove when nothing is in the UnboundItemPropertyInRefresh list." );
        return;
      }

      object count = m_unboundItemPropertyInRefresh[ component ];

      if( count != null )
      {
        int intCount = ( int )count;

        if( intCount <= 1 )
        {
          if( m_unboundItemPropertyInRefresh.Count == 1 )
          {
            m_unboundItemPropertyInRefresh = null;
          }
          else
          {
            m_unboundItemPropertyInRefresh.Remove( component );
          }
        }
        else
        {
          m_unboundItemPropertyInRefresh[ component ] = intCount - 1;
        }
      }
    }

    private List<DataGridItemPropertyBase> m_defaultItemProperties;
    private Type m_itemType;
    private int m_unboundItemPropertyCount;
    private Hashtable m_unboundItemPropertyInRefresh;
    private Hashtable m_unboundItemPropertyChangedSuspended;
  }
}
