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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Xceed.Utils.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal abstract class DataGridDetailDescription : DependencyObject, IWeakEventListener
  {
    protected DataGridDetailDescription()
    {
      m_itemProperties = new DataGridItemPropertyCollection();
      m_detailDescriptions = new DataGridDetailDescriptionCollection();
      m_defaultPropertyDescriptions = new PropertyDescriptionRouteDictionary();

      m_groupDescriptions = new GroupDescriptionCollection();
      m_sortDescriptions = new DataGridSortDescriptionCollection();    

      this.AutoCreateDetailDescriptions = true;
      this.AutoCreateItemProperties = true;
      this.DefaultCalculateDistinctValues = true;

      CollectionChangedEventManager.AddListener( m_itemProperties, this );
      InitializeItemPropertyEventManager.AddListener( m_itemProperties, this );
      CollectionChangedEventManager.AddListener( m_detailDescriptions, this );
    }

    #region RelationName Public Property

    public string RelationName
    {
      get
      {
        return m_relationName;
      }
      set
      {
        if( this.InternalIsSealed == true )
          throw new InvalidOperationException( "An attempt was made to change the RelationName property after the DataGridDetailDescription has been sealed." );

        m_relationName = value;
      }
    }

    private string m_relationName;

    #endregion

    #region Title Public Property

    public object Title
    {
      get
      {
        return m_title;
      }
      set
      {
        m_title = value;
      }
    }

    private object m_title;

    #endregion

    #region TitleTemplate Public Property

    public DataTemplate TitleTemplate
    {
      get
      {
        return m_titleTemplate;
      }
      set
      {
        m_titleTemplate = value;
      }
    }

    private DataTemplate m_titleTemplate;

    #endregion

    #region AutoCreateItemProperties Public Property

    public bool AutoCreateItemProperties
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateItemProperties ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateItemProperties ] = value;
      }
    }

    #endregion

    #region AutoCreateDetailDescriptions Public Property

    public bool AutoCreateDetailDescriptions
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateDetailDescriptions ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateDetailDescriptions ] = value;
      }
    }

    #endregion

    #region AutoCreateForeignKeyDescriptions Public Property

    public bool AutoCreateForeignKeyDescriptions
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateForeignKeyDescriptions ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateForeignKeyDescriptions ] = value;
      }
    }

    #endregion

    #region DistinctValuesConstraint Public Property

    public DistinctValuesConstraint DistinctValuesConstraint
    {
      get
      {
        return m_distinctValuesConstraint;
      }
      set
      {
        m_distinctValuesConstraint = value;
      }
    }

    private DistinctValuesConstraint m_distinctValuesConstraint = DistinctValuesConstraint.All;

    #endregion

    #region ItemProperties Public Property

    public DataGridItemPropertyCollection ItemProperties
    {
      get
      {
        return m_itemProperties;
      }
    }

    private readonly DataGridItemPropertyCollection m_itemProperties;

    #endregion

    #region DefaultCalculateDistinctValues Public Property

    public bool DefaultCalculateDistinctValues
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.DefaultCalculateDistinctValues ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.DefaultCalculateDistinctValues ] = value;
      }
    }

    #endregion

    #region DetailDescriptions Public Property

    public DataGridDetailDescriptionCollection DetailDescriptions
    {
      get
      {
        return m_detailDescriptions;
      }
    }

    private readonly DataGridDetailDescriptionCollection m_detailDescriptions;

    #endregion

    #region GroupDescriptions Public Property

    public ObservableCollection<GroupDescription> GroupDescriptions
    {
      get
      {
        return m_groupDescriptions;
      }
    }

    private readonly GroupDescriptionCollection m_groupDescriptions;

    #endregion

    #region SortDescriptions Public Propertiy

    public SortDescriptionCollection SortDescriptions
    {
      get
      {
        return m_sortDescriptions;
      }
    }

    private readonly DataGridSortDescriptionCollection m_sortDescriptions;

    #endregion

    #region AutoCreateItemPropertiesCompleted Internal Property

    internal bool AutoCreateItemPropertiesCompleted
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateItemPropertiesCompleted ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateItemPropertiesCompleted ] = value;
      }
    }

    #endregion

    #region AutoCreateDetailDescriptionsCompleted Internal Property

    internal bool AutoCreateDetailDescriptionsCompleted
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateDetailDescriptionsCompleted ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.AutoCreateDetailDescriptionsCompleted ] = value;
      }
    }

    #endregion

    #region DefaultItemPropertiesInitialized Internal Property

    internal bool DefaultItemPropertiesInitialized
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.DefaultItemPropertiesInitialized ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.DefaultItemPropertiesInitialized ] = value;
      }
    }

    #endregion

    #region DefaultPropertyDescriptionsCreated Internal Property

    internal bool DefaultPropertyDescriptionsCreated
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.DefaultPropertyDescriptionsCreated ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.DefaultPropertyDescriptionsCreated ] = value;
      }
    }

    #endregion

    #region DefaultPropertyDescriptions Internal Property

    internal PropertyDescriptionRouteDictionary DefaultPropertyDescriptions
    {
      get
      {
        return m_defaultPropertyDescriptions;
      }
    }

    private readonly PropertyDescriptionRouteDictionary m_defaultPropertyDescriptions;

    #endregion

    #region IsAutoCreated Internal Property

    internal bool IsAutoCreated
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.IsAutoCreated ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.IsAutoCreated ] = value;
      }
    }

    #endregion

    #region InternalIsSealed Internal Property

    internal bool InternalIsSealed
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.IsSealed ];
      }
      private set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.IsSealed ] = value;
      }
    }

    #endregion

    #region IsInitialized Internal Property

    internal bool IsInitialized
    {
      get
      {
        return m_flags[ ( int )DataGridDetailDescriptionFlags.IsInitialized ];
      }
      set
      {
        m_flags[ ( int )DataGridDetailDescriptionFlags.IsInitialized ] = value;
      }
    }

    #endregion

    #region ItemType Internal Property

    internal Type ItemType
    {
      get
      {
        return m_itemType;
      }
      set
      {
        if( value == m_itemType )
          return;

        m_itemType = value;
      }
    }

    private Type m_itemType; //null

    #endregion

    #region DataGridSortDescriptions Internal Property

    internal DataGridSortDescriptionCollection DataGridSortDescriptions
    {
      get
      {
        return m_sortDescriptions;
      }
    }

    #endregion

    protected internal virtual void Initialize( DataGridCollectionViewBase parentCollectionView )
    {
    }

    protected internal abstract IEnumerable GetDetailsForParentItem( DataGridCollectionViewBase parentCollectionView, object parentItem );

    internal void Seal()
    {
      this.InternalIsSealed = true;
    }

    internal void InternalInitialize( DataGridCollectionViewBase parentCollectionView )
    {
      if( string.IsNullOrEmpty( this.RelationName ) )
        throw new InvalidOperationException( "An attempt was made to initialize a detail description that does not have a relation name." );

      this.Initialize( parentCollectionView );
    }

    private void OnItemPropertiesCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      var removedItems = default( IEnumerable<DataGridItemPropertyBase> );

      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Replace:
          removedItems = e.OldItems.Cast<DataGridItemPropertyBase>();
          break;

        case NotifyCollectionChangedAction.Remove:
          removedItems = e.OldItems.Cast<DataGridItemPropertyBase>();
          break;

        case NotifyCollectionChangedAction.Reset:
          throw new NotSupportedException();
      }
    }

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( ( managerType == null ) || ( sender == null ) || ( e == null ) )
        return false;

      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        var eventArgs = ( NotifyCollectionChangedEventArgs )e;

        if( m_itemProperties == sender )
        {
          this.OnItemPropertiesCollectionChanged( sender, eventArgs );
        }
        else if( m_detailDescriptions == sender )
        {
        }
      }
      else if( managerType == typeof( InitializeItemPropertyEventManager ) )
      {
        var eventArgs = ( InitializeItemPropertyEventArgs )e;

        if( m_itemProperties == sender )
        {
          var itemProperty = eventArgs.ItemProperty;
          var itemPropertyRoute = DataGridItemPropertyRoute.Create( itemProperty );

          ItemsSourceHelper.SetPropertyDescriptionsFromItemProperty( m_defaultPropertyDescriptions, null, null, m_itemType, itemPropertyRoute );
          ItemsSourceHelper.InitializePropertyDescriptions( m_defaultPropertyDescriptions, itemPropertyRoute, m_itemType, this.DefaultPropertyDescriptionsCreated );
        }
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    private BitVector32 m_flags = new BitVector32();

    [Flags]
    private enum DataGridDetailDescriptionFlags
    {
      IsSealed = 1 << 0,
      DefaultCalculateDistinctValues = 1 << 1,
      IsInitialized = 1 << 2,
      AutoCreateItemProperties = 1 << 3,
      AutoCreateDetailDescriptions = 1 << 4,
      AutoCreateForeignKeyDescriptions = 1 << 5,
      IsAutoCreated = 1 << 6,
      AutoCreateItemPropertiesCompleted = 1 << 7,
      AutoCreateDetailDescriptionsCompleted = 1 << 8,
      DefaultItemPropertiesInitialized = 1 << 9,
      DefaultPropertyDescriptionsCreated = 1 << 10,
    }
  }
}
