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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.DataGrid.Utils;

namespace Xceed.Wpf.DataGrid
{
  public sealed class Group : IGroupLevelDescription, INotifyPropertyChanged, IWeakEventListener
  {
    #region Static Fields

    internal static readonly string IsExpandedPropertyName = PropertyHelper.GetPropertyName( ( Group g ) => g.IsExpanded );
    internal static readonly string ItemCountPropertyName = PropertyHelper.GetPropertyName( ( Group g ) => g.ItemCount );
    internal static readonly string GroupByPropertyName = PropertyHelper.GetPropertyName( ( Group g ) => g.GroupBy );
    internal static readonly string TitlePropertyName = PropertyHelper.GetPropertyName( ( Group g ) => g.Title );
    internal static readonly string TitleTemplatePropertyName = PropertyHelper.GetPropertyName( ( Group g ) => g.TitleTemplate );
    internal static readonly string TitleTemplateSelectorPropertyName = PropertyHelper.GetPropertyName( ( Group g ) => g.TitleTemplateSelector );
    internal static readonly string ValueTemplatePropertyName = PropertyHelper.GetPropertyName( ( Group g ) => g.ValueTemplate );
    internal static readonly string ValueTemplateSelectorPropertyName = PropertyHelper.GetPropertyName( ( Group g ) => g.ValueTemplateSelector );
    internal static readonly string ValueStringFormatPropertyName = PropertyHelper.GetPropertyName( ( Group g ) => g.ValueStringFormat );
    internal static readonly string ValueStringFormatCulturePropertyName = PropertyHelper.GetPropertyName( ( Group g ) => g.ValueStringFormatCulture );

    #endregion

    internal Group(
      GroupGeneratorNode node,
      CollectionViewGroup group,
      LateGroupLevelDescription groupLevelDescription,
      DataGridContext dataGridContext )
    {
      if( node == null )
        throw new ArgumentNullException( "node" );

      if( group == null )
        throw new ArgumentNullException( "group" );

      if( groupLevelDescription == null )
        throw new ArgumentNullException( "groupLevelDescription" );

      if( dataGridContext == null )
        throw new ArgumentNullException( "dataGridContext" );

      m_collectionViewGroup = group;

      // Initialization is done through setters to register for events.
      this.DataGridContext = dataGridContext;
      this.GeneratorNode = node;
      this.GroupLevelDescription = groupLevelDescription;
    }

    #region IsExpanded Property

    public bool IsExpanded
    {
      get
      {
        if( m_generatorNode != null )
          return m_generatorNode.IsExpanded;

        return false;
      }
      set
      {
        if( m_generatorNode == null )
          return;

        m_generatorNode.IsExpanded = value;
      }
    }

    internal bool IsComputedExpanded
    {
      get
      {
        if( m_generatorNode != null )
          return m_generatorNode.IsComputedExpanded;

        return false;
      }
    }

    #endregion

    #region Level Property

    public int Level
    {
      get
      {
        if( m_generatorNode != null )
          return m_generatorNode.Level;

        return -1;
      }
    }

    #endregion

    #region ItemCount Property

    public int ItemCount
    {
      get
      {
        if( m_generatorNode != null )
          return m_generatorNode.TotalLeafCount;

        return 0;
      }
    }

    #endregion

    #region ParentGroups Property

    public ReadOnlyCollection<Group> ParentGroups
    {
      get
      {
        if( m_generatorNode == null )
          return ( new List<Group>( 0 ) ).AsReadOnly();

        var list = new List<Group>();
        var nodeHelper = new GeneratorNodeHelper( m_generatorNode, 0, 0 ); //index is not important

        while( nodeHelper.MoveToParent() )
        {
          GroupGeneratorNode parentGroup = nodeHelper.CurrentNode as GroupGeneratorNode;
          if( parentGroup == null )
            break;

          list.Insert( 0, parentGroup.UIGroup );
        }

        return list.AsReadOnly();
      }
    }

    #endregion

    #region SiblingGroups Property

    public ReadOnlyCollection<Group> SiblingGroups
    {
      get
      {
        if( m_generatorNode == null )
          return null;

        var headersFootersGeneratorNode = HeadersFootersGeneratorNode.GetSameLevelFirstHeaderNode( m_generatorNode );
        if( headersFootersGeneratorNode == null )
          return null;

        var currentGeneratorContentGeneration = this.DataGridContext.CustomItemContainerGenerator.CurrentGeneratorContentGeneration;
        return headersFootersGeneratorNode.GetImmediateUIGroups( currentGeneratorContentGeneration );
      }
    }

    #endregion

    #region Value Property

    public object Value
    {
      get
      {
        if( m_collectionViewGroup != null )
          return m_collectionViewGroup.Name;

        return null;
      }
    }

    #endregion

    #region ValueStringFormat Property

    public string ValueStringFormat
    {
      get
      {
        var description = this.GroupLevelDescription;
        if( description != null )
          return description.ValueStringFormat;

        return null;
      }
    }

    #endregion

    #region ValueStringFormatCulture Property

    public CultureInfo ValueStringFormatCulture
    {
      get
      {
        var description = this.GroupLevelDescription;
        if( description != null )
          return description.ValueStringFormatCulture;

        return null;
      }
    }

    #endregion

    #region ValueTemplate Property

    public DataTemplate ValueTemplate
    {
      get
      {
        var description = this.GroupLevelDescription;
        if( description != null )
          return description.ValueTemplate;

        return null;
      }
    }

    #endregion

    #region ValueTemplateSelector Property

    public DataTemplateSelector ValueTemplateSelector
    {
      get
      {
        var description = this.GroupLevelDescription;
        if( description != null )
          return description.ValueTemplateSelector;

        return null;
      }
    }

    #endregion

    #region IsBottomLevel Property

    public bool IsBottomLevel
    {
      get
      {
        if( m_collectionViewGroup != null )
          return m_collectionViewGroup.IsBottomLevel;

        return true;
      }
    }

    #endregion

    #region Items Property [Obsoleted]

    [Obsolete( "The Items property is obsolete and has been replaced by the GroupExtensions.GetItems extensibility method.", false )]
    public ReadOnlyObservableCollection<object> Items
    {
      get
      {
        IList<object> items = this.GetItems();

        if( items is ReadOnlyObservableCollection<object> )
          return ( ReadOnlyObservableCollection<object> )items;

        return null;
      }
    }

    #endregion

    #region Title Property

    public object Title
    {
      get
      {
        var description = this.GroupLevelDescription;
        if( description != null )
          return description.Title;

        return null;
      }
    }

    #endregion

    #region TitleTemplate Property

    public DataTemplate TitleTemplate
    {
      get
      {
        var description = this.GroupLevelDescription;
        if( description != null )
          return description.TitleTemplate;

        return null;
      }
    }

    #endregion

    #region TitleTemplateSelector Property

    public DataTemplateSelector TitleTemplateSelector
    {
      get
      {
        var description = this.GroupLevelDescription;
        if( description != null )
          return description.TitleTemplateSelector;

        return null;
      }
    }

    #endregion

    #region StatContext Property

    public object StatContext
    {
      get
      {
        return m_collectionViewGroup as DataGridCollectionViewGroup;
      }
    }

    #endregion

    #region GroupConfiguration Property

    public GroupConfiguration GroupConfiguration
    {
      get
      {
        if( m_generatorNode != null )
          return m_generatorNode.GroupConfiguration;

        return null;
      }
    }

    #endregion

    #region GeneratorNode Internal Property

    internal GroupGeneratorNode GeneratorNode
    {
      get
      {
        return m_generatorNode;
      }
      private set
      {
        if( value == m_generatorNode )
          return;

        if( m_generatorNode != null )
        {
          this.UnregisterItemCountEvent( m_generatorNode );
          this.UnregisterIsExpandedEvent( m_generatorNode );
        }

        m_generatorNode = value;

        if( m_generatorNode != null )
        {
          this.RegisterItemCountEvent( m_generatorNode, false );
          this.RegisterIsExpandedEvent( m_generatorNode, false );
        }
      }
    }

    private GroupGeneratorNode m_generatorNode; // = null

    #endregion

    #region GroupBy Internal Property

    internal string GroupBy
    {
      get
      {
        var description = this.GroupLevelDescription;
        if( description != null )
          return description.FieldName;

        return string.Empty;
      }
    }

    #endregion

    #region DataGridContext Internal Property

    internal DataGridContext DataGridContext
    {
      get
      {
        return m_dataGridContext;
      }
      private set
      {
        m_dataGridContext = value;
      }
    }

    private DataGridContext m_dataGridContext;

    #endregion

    #region CollectionViewGroup Internal Property

    internal CollectionViewGroup CollectionViewGroup
    {
      get
      {
        return m_collectionViewGroup;
      }
    }

    private CollectionViewGroup m_collectionViewGroup;

    #endregion

    #region GroupDescription Private Property

    private GroupDescription GroupDescription
    {
      get
      {
        return m_groupDescription;
      }
    }

    private GroupDescription m_groupDescription;

    #endregion

    #region GroupLevelDescription Private Property

    private LateGroupLevelDescription GroupLevelDescription
    {
      get
      {
        return m_groupLevelDescription;
      }
      set
      {
        if( value == m_groupLevelDescription )
          return;

        if( m_groupLevelDescription != null )
        {
          this.UnregisterGroupLevelDescriptionEvent( m_groupLevelDescription );
        }

        m_groupLevelDescription = value;

        if( m_groupLevelDescription != null )
        {
          this.RegisterGroupLevelDescriptionEvent( m_groupLevelDescription, false );
        }

        this.OnGroupLevelDescriptionPropertyChanged();
      }
    }

    private LateGroupLevelDescription m_groupLevelDescription;

    #endregion

    #region SyncRoot Private Property

    private object SyncRoot
    {
      get
      {
        if( m_syncRoot == null )
        {
          Interlocked.CompareExchange( ref m_syncRoot, new object(), null );
        }

        return m_syncRoot;
      }
    }

    private object m_syncRoot;

    #endregion

    #region ItemCount Event Handling

    private void RegisterItemCountEvent( GroupGeneratorNode item, bool force )
    {
      if( item == null )
        return;

      lock( this.SyncRoot )
      {
        if( !force && ( m_propertyChanged == null ) )
          return;

        item.TotalLeafCountChanged += new EventHandler( this.OnItemCountChanged );
      }
    }

    private void UnregisterItemCountEvent( GroupGeneratorNode item )
    {
      if( item == null )
        return;

      item.TotalLeafCountChanged -= new EventHandler( this.OnItemCountChanged );
    }

    private void OnItemCountChanged( object sender, EventArgs e )
    {
      this.OnPropertyChanged( Group.ItemCountPropertyName );
    }

    #endregion

    #region IsExpanded Event Handling

    private void RegisterIsExpandedEvent( GroupGeneratorNode item, bool force )
    {
      if( item == null )
        return;

      lock( this.SyncRoot )
      {
        if( !force && ( m_propertyChanged == null ) )
          return;

        item.IsExpandedChanged += new EventHandler( this.OnIsExpandedChanged );
      }
    }

    private void UnregisterIsExpandedEvent( GroupGeneratorNode item )
    {
      if( item == null )
        return;

      item.IsExpandedChanged -= new EventHandler( this.OnIsExpandedChanged );
    }

    private void OnIsExpandedChanged( object sender, EventArgs e )
    {
      this.OnPropertyChanged( Group.IsExpandedPropertyName );
    }

    #endregion

    #region GroupLevelDescription Event Handling

    private void RegisterGroupLevelDescriptionEvent( LateGroupLevelDescription item, bool force )
    {
      if( item == null )
        return;

      lock( this.SyncRoot )
      {
        if( !force && ( m_propertyChanged == null ) )
          return;

        item.PropertyChanged += new PropertyChangedEventHandler( this.OnGroupLevelDescriptionChanged );
      }
    }

    private void UnregisterGroupLevelDescriptionEvent( LateGroupLevelDescription item )
    {
      if( item == null )
        return;

      item.PropertyChanged -= new PropertyChangedEventHandler( this.OnGroupLevelDescriptionChanged );
    }

    private void OnGroupLevelDescriptionChanged( object sender, PropertyChangedEventArgs e )
    {
      if( string.IsNullOrEmpty( e.PropertyName ) )
      {
        this.OnGroupLevelDescriptionPropertyChanged();
      }
      else
      {
        // Simply relay this property changed to this Group instance.
        this.OnPropertyChanged( e.PropertyName );
      }
    }

    private void OnGroupLevelDescriptionPropertyChanged()
    {
      this.OnPropertyChanged( Group.GroupByPropertyName );
      this.OnPropertyChanged( Group.TitlePropertyName );
      this.OnPropertyChanged( Group.TitleTemplatePropertyName );
      this.OnPropertyChanged( Group.TitleTemplateSelectorPropertyName );
      this.OnPropertyChanged( Group.ValueTemplatePropertyName );
      this.OnPropertyChanged( Group.ValueTemplateSelectorPropertyName );
      this.OnPropertyChanged( Group.ValueStringFormatPropertyName );
      this.OnPropertyChanged( Group.ValueStringFormatCulturePropertyName );
    }

    #endregion

    internal void ClearGroup()
    {
      m_groupDescription = null;
      m_collectionViewGroup = null;
      m_propertyChanged = null;

      this.DataGridContext = null;
      this.GeneratorNode = null;
      this.GroupLevelDescription = null;
    }

    #region IGroupLevelDescription Members

    string IGroupLevelDescription.FieldName
    {
      get
      {
        return this.GroupBy;
      }
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged
    {
      add
      {
        lock( this.SyncRoot )
        {
          if( m_propertyChanged == null )
          {
            this.RegisterItemCountEvent( this.GeneratorNode, true );
            this.RegisterIsExpandedEvent( this.GeneratorNode, true );
            this.RegisterGroupLevelDescriptionEvent( this.GroupLevelDescription, true );
          }

          m_propertyChanged += value;
        }
      }
      remove
      {
        lock( this.SyncRoot )
        {
          m_propertyChanged -= value;

          if( m_propertyChanged == null )
          {
            this.UnregisterItemCountEvent( this.GeneratorNode );
            this.UnregisterIsExpandedEvent( this.GeneratorNode );
            this.UnregisterGroupLevelDescriptionEvent( this.GroupLevelDescription );
          }
        }
      }
    }

    private PropertyChangedEventHandler m_propertyChanged;

    private void OnPropertyChanged( string propertyName )
    {
      var handler = m_propertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    private bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return false;
    }

    #endregion
  }
}
