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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Xceed.Utils.Collections;
using Xceed.Wpf.DataGrid.Diagnostics;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  internal partial class CustomItemContainerGenerator
    : IInhibitGenPosToIndexUpdating,
    ICustomItemContainerGenerator,
    IWeakEventListener,
    INotifyPropertyChanged,
    IDataGridContextVisitable
  {
    internal static CustomItemContainerGenerator CreateGenerator( DataGridControl dataGridControl, CollectionView collectionView, DataGridContext dataGridContext )
    {
      if( dataGridControl == null )
        throw new ArgumentNullException( "dataGridControl" );

      if( collectionView == null )
        throw DataGridException.Create<ArgumentNullException>( "collectionView", dataGridControl );

      if( dataGridContext == null )
        throw DataGridException.Create<ArgumentNullException>( "dataGridContext", dataGridControl );

      var recyclingPools = new CustomItemContainerGeneratorRecyclingPools();
      var ensureNodeTreeCreatedRequiredFlag = new BubbleDirtyFlag( true );
      var handleGlobalItemsResetFlag = new InheritAutoResetFlag();
      var deferDetailsRemapFlag = new LeveledAutoResetFlag();
      var generator = new CustomItemContainerGenerator( dataGridControl, collectionView, dataGridContext, recyclingPools, ensureNodeTreeCreatedRequiredFlag, handleGlobalItemsResetFlag, deferDetailsRemapFlag );

      dataGridContext.SetGenerator( generator );

      return generator;
    }

    private static CustomItemContainerGenerator CreateGenerator( DataGridControl dataGridControl, CollectionView collectionView, DataGridContext dataGridContext, CustomItemContainerGenerator masterGenerator )
    {
      if( dataGridControl == null )
        throw new ArgumentNullException( "dataGridControl" );

      if( collectionView == null )
        throw DataGridException.Create<ArgumentNullException>( "collectionView", dataGridControl );

      if( dataGridContext == null )
        throw DataGridException.Create<ArgumentNullException>( "dataGridContext", dataGridControl );

      if( masterGenerator == null )
        throw DataGridException.Create<ArgumentNullException>( "masterGenerator", dataGridControl );

      var recyclingPools = masterGenerator.m_recyclingPools;
      var ensureNodeTreeCreatedRequiredFlag = new BubbleDirtyFlag( masterGenerator.m_ensureNodeTreeCreatedRequired, true );
      var handleGlobalItemsResetFlag = new InheritAutoResetFlag( masterGenerator.m_handleGlobalItemsReset );
      var deferDetailsRemapFlag = masterGenerator.m_deferDetailsRemap.GetChild();
      var generator = new CustomItemContainerGenerator( dataGridControl, collectionView, dataGridContext, recyclingPools, ensureNodeTreeCreatedRequiredFlag, handleGlobalItemsResetFlag, deferDetailsRemapFlag );

      dataGridContext.SetGenerator( generator );

      return generator;
    }

    private CustomItemContainerGenerator(
      DataGridControl dataGridControl,
      CollectionView collectionView,
      DataGridContext dataGridContext,
      CustomItemContainerGeneratorRecyclingPools recyclingPools,
      BubbleDirtyFlag ensureNodeTreeCreatedRequiredFlag,
      InheritAutoResetFlag handleGlobalItemResetFlag,
      LeveledAutoResetFlag deferDetailsRemapFlag )
    {
      Debug.Assert( dataGridControl != null );
      Debug.Assert( collectionView != null );
      Debug.Assert( dataGridContext != null );
      Debug.Assert( recyclingPools != null );
      Debug.Assert( ensureNodeTreeCreatedRequiredFlag != null );
      Debug.Assert( handleGlobalItemResetFlag != null );
      Debug.Assert( deferDetailsRemapFlag != null );

      m_dataGridControl = dataGridControl;
      m_dataGridContext = dataGridContext;
      m_collectionView = collectionView;
      m_recyclingPools = recyclingPools;
      m_ensureNodeTreeCreatedRequired = ensureNodeTreeCreatedRequiredFlag;
      m_handleGlobalItemsReset = handleGlobalItemResetFlag;
      m_deferDetailsRemap = deferDetailsRemapFlag;

      //initialize explicitly (set a local value) to the DataItem attached property (for nested DataGridControls)
      CustomItemContainerGenerator.SetDataItemProperty( m_dataGridControl, EmptyDataItemDataProvider.Instance );

      this.RegisterEvents();

      m_nodeFactory = new GeneratorNodeFactory( this.OnGeneratorNodeItemsCollectionChanged,
                                                this.OnGeneratorNodeGroupsCollectionChanged,
                                                this.OnGeneratorNodeHeadersFootersCollectionChanged,
                                                this.OnGeneratorNodeExpansionStateChanged,
                                                this.OnGroupGeneratorNodeIsExpandedChanging,
                                                this.OnGroupGeneratorNodeIsExpandedChanged,
                                                dataGridControl );

      m_headerFooterDataContextBinding = new Binding();
      m_headerFooterDataContextBinding.Source = m_dataGridControl;
      m_headerFooterDataContextBinding.Path = new PropertyPath( DataGridControl.DataContextProperty );

      this.ResetNodeList();

      this.IncrementCurrentGenerationCount();
    }

    #region CurrentGeneratorContentGeneration Internal Property

    internal int CurrentGeneratorContentGeneration
    {
      get
      {
        return m_currentGeneratorContentGeneration;
      }
    }

    private int m_currentGeneratorContentGeneration; //0

    #endregion

    #region DataItemProperty Attached Internal Property

    internal static readonly DependencyProperty DataItemPropertyProperty = DependencyProperty.RegisterAttached(
      "DataItemProperty",
      typeof( DataItemDataProviderBase ),
      typeof( CustomItemContainerGenerator ),
      new FrameworkPropertyMetadata( EmptyDataItemDataProvider.Instance, FrameworkPropertyMetadataOptions.Inherits ) );

    internal static DataItemDataProviderBase GetDataItemProperty( DependencyObject obj )
    {
      return ( DataItemDataProviderBase )obj.GetValue( CustomItemContainerGenerator.DataItemPropertyProperty );
    }

    private static void SetDataItemProperty( DependencyObject obj, DataItemDataProviderBase value )
    {
      obj.SetValue( CustomItemContainerGenerator.DataItemPropertyProperty, value );
    }

    #endregion

    #region IsInUse Internal Property

    internal bool IsInUse
    {
      get
      {
        return m_flags[ ( int )CustomItemContainerGeneratorFlags.InUse ];
      }
      private set
      {
        m_flags[ ( int )CustomItemContainerGeneratorFlags.InUse ] = value;
      }
    }

    internal void InvalidateIsInUse()
    {
      this.IsInUse = false;
    }

    internal void SetIsInUse()
    {
      this.IsInUse = true;
    }

    #endregion

    #region ForceReset Internal Property

    internal bool ForceReset
    {
      get
      {
        return m_flags[ ( int )CustomItemContainerGeneratorFlags.ForceReset ];
      }
      set
      {
        m_flags[ ( int )CustomItemContainerGeneratorFlags.ForceReset ] = value;
      }
    }

    #endregion

    #region Header Property

    public HeadersFootersGeneratorNode Header
    {
      get
      {
        return m_firstHeader;
      }
    }

    private HeadersFootersGeneratorNode m_firstHeader;

    #endregion

    #region Footer Property

    public HeadersFootersGeneratorNode Footer
    {
      get
      {
        return m_firstFooter;
      }
    }

    private HeadersFootersGeneratorNode m_firstFooter;

    #endregion

    #region RealizedContainers Property

    public ReadOnlyCollection<DependencyObject> RealizedContainers
    {
      get
      {
        return new ReadOnlyCollection<DependencyObject>( m_genPosToContainer );
      }
    }

    #endregion

    #region RealizedItems Property

    public ReadOnlyCollection<object> RealizedItems
    {
      get
      {
        return new ReadOnlyCollection<object>( m_genPosToItem );
      }
    }

    #endregion

    #region Status Property

    public GeneratorStatus Status
    {
      get
      {
        return m_generatorStatus;
      }
    }

    private GeneratorStatus m_generatorStatus = GeneratorStatus.NotStarted;

    #endregion

    #region NodeFactory Private Property

    private GeneratorNodeFactory NodeFactory
    {
      get
      {
        return m_nodeFactory;
      }
    }

    private readonly GeneratorNodeFactory m_nodeFactory;

    #endregion

    #region IsDetailsRemapDeferred Private Property

    private bool IsDetailsRemapDeferred
    {
      get
      {
        return m_deferDetailsRemap.IsSet;
      }
    }

    private IDisposable DeferDetailsRemap()
    {
      return m_deferDetailsRemap.Set();
    }

    private readonly LeveledAutoResetFlag m_deferDetailsRemap;

    #endregion

    #region IsEnsuringNodeTreeCreated Private Property

    private bool IsEnsuringNodeTreeCreated
    {
      get
      {
        return m_ensureNodeTreeCreated.IsSet;
      }
    }

    private IDisposable SetIsEnsuringNodeTreeCreated()
    {
      m_ensureNodeTreeCreatedRequired.IsSet = false;

      return m_ensureNodeTreeCreated.Set();
    }

    private readonly AutoResetFlag m_ensureNodeTreeCreated = AutoResetFlagFactory.Create();

    #endregion

    #region IsNodeTreeValid Private Property

    private bool IsNodeTreeValid
    {
      get
      {
        return !m_ensureNodeTreeCreatedRequired.IsSet;
      }
    }

    private void InvalidateNodeTree()
    {
      if( this.IsEnsuringNodeTreeCreated )
        return;

      m_ensureNodeTreeCreatedRequired.IsSet = true;
    }

    private readonly BubbleDirtyFlag m_ensureNodeTreeCreatedRequired;

    #endregion

    #region IsHandlingGlobalItemsReset Private Property

    private bool IsHandlingGlobalItemsReset
    {
      get
      {
        return m_handleGlobalItemsReset.IsSet;
      }
    }

    private bool IsHandlingGlobalItemsResetLocally
    {
      get
      {
        return m_handleGlobalItemsReset.IsSetLocal;
      }
    }

    private IDisposable SetIsHandlingGlobalItemsResetLocally()
    {
      return m_handleGlobalItemsReset.SetLocal();
    }

    private readonly InheritAutoResetFlag m_handleGlobalItemsReset;

    #endregion

    #region IsItemsChangedInhibited Private Property

    private bool IsItemsChangedInhibited
    {
      get
      {
        return m_isItemsChangedInhibited.IsSet;
      }
    }

    private IDisposable InhibitItemsChanged()
    {
      return m_isItemsChangedInhibited.Set();
    }

    private readonly AutoResetFlag m_isItemsChangedInhibited = AutoResetFlagFactory.Create();

    #endregion

    #region DetailsChanged Event

    public event EventHandler DetailsChanged;

    #endregion

    #region ItemsChanged Internal Event

    internal event CustomGeneratorChangedEventHandler ItemsChanged;

    private void SendRemoveEvent( int count, IList<DependencyObject> containers )
    {
      var handler = this.ItemsChanged;
      if( ( handler == null ) || ( count <= 0 ) || this.IsItemsChangedInhibited )
        return;

      using( this.DeferDetailsRemap() )
      {
        handler.Invoke( this, CustomGeneratorChangedEventArgs.Remove( count, containers ) );
      }
    }

    private void SendAddEvent( int count )
    {
      var handler = this.ItemsChanged;
      if( ( handler == null ) || ( count <= 0 ) || this.IsItemsChangedInhibited )
        return;

      using( this.DeferDetailsRemap() )
      {
        handler.Invoke( this, CustomGeneratorChangedEventArgs.Add( count ) );
      }
    }

    private void SendResetEvent()
    {
      var handler = this.ItemsChanged;
      if( ( handler == null ) || this.IsItemsChangedInhibited )
        return;

      using( this.DeferDetailsRemap() )
      {
        handler.Invoke( this, CustomGeneratorChangedEventArgs.Reset() );
      }
    }

    #endregion

    #region ContainersRemoved Internal Event

    internal event ContainersRemovedEventHandler ContainersRemoved;

    private void OnContainersRemoved( IList<DependencyObject> containers )
    {
      var handler = this.ContainersRemoved;
      if( ( handler == null ) || ( containers == null ) || ( containers.Count <= 0 ) )
        return;

      handler.Invoke( this, new ContainersRemovedEventArgs( containers ) );
    }

    #endregion

    #region RecyclingCandidatesCleaned Internal Event

    internal event RecyclingCandidatesCleanedEventHandler RecyclingCandidatesCleaned;

    private void OnRecyclingCandidatesCleaned( object sender, RecyclingCandidatesCleanedEventArgs e )
    {
      if( !this.IsRecyclingEnabled )
        return;

      foreach( DependencyObject container in e.RecyclingCandidates )
      {
        this.ClearStatContext( container );
      }

      var handler = this.RecyclingCandidatesCleaned;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    public void ResetGeneratorContent()
    {
      this.CleanupGenerator();
      this.EnsureNodeTreeCreated();
    }

    public DependencyObject ContainerFromIndex( int itemIndex )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_ContainerFromIndex, DataGridTraceArgs.Index( itemIndex ) ) )
      {
        if( this.IsHandlingGlobalItemsReset )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ContainerFromIndex, DataGridTraceMessages.CannotProcessOnReset, DataGridTraceArgs.Index( itemIndex ) );
          return null;
        }

        this.EnsureNodeTreeCreated();

        var index = m_genPosToIndex.IndexOf( itemIndex );
        if( index >= 0 )
        {
          var container = m_genPosToContainer[ index ];

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ContainerFromIndex, DataGridTraceMessages.ContainerFound, DataGridTraceArgs.Container( container ), DataGridTraceArgs.GeneratorIndex( index ), DataGridTraceArgs.Index( itemIndex ) );

          var detailNode = m_genPosToNode[ index ] as DetailGeneratorNode;
          if( detailNode == null )
            return container;

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ContainerFromIndex, DataGridTraceMessages.ContainerIsInDetail, DataGridTraceArgs.Container( container ), DataGridTraceArgs.GeneratorIndex( index ), DataGridTraceArgs.Index( itemIndex ), DataGridTraceArgs.Node( detailNode ) );
        }
        else
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ContainerFromIndex, DataGridTraceMessages.ContainerNotFound, DataGridTraceArgs.Index( itemIndex ) );
        }

        return null;
      }
    }

    internal DependencyObject ContainerFromItem( object item )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_ContainerFromItem, DataGridTraceArgs.Item( item ) ) )
      {
        if( this.IsHandlingGlobalItemsReset )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ContainerFromItem, DataGridTraceMessages.CannotProcessOnReset, DataGridTraceArgs.Item( item ) );
          return null;
        }

        this.EnsureNodeTreeCreated();

        var index = this.FindFirstGeneratedIndexForLocalItem( item );
        if( index >= 0 )
        {
          var container = m_genPosToContainer[ index ];

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ContainerFromItem, DataGridTraceMessages.ContainerFound, DataGridTraceArgs.Container( container ), DataGridTraceArgs.GeneratorIndex( index ), DataGridTraceArgs.Item( item ) );
          return container;
        }

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ContainerFromItem, DataGridTraceMessages.ContainerNotFound, DataGridTraceArgs.Item( item ) );
        return null;
      }
    }

    internal List<object> GetRealizedDataItemsForGroup( GroupGeneratorNode group )
    {
      var count = m_genPosToNode.Count;
      var items = new List<object>( count );

      for( int i = 0; i < count; i++ )
      {
        var node = m_genPosToNode[ i ] as ItemsGeneratorNode;
        if( ( node != null ) && ( node.Parent == group ) )
        {
          items.Add( m_genPosToItem[ i ] );
        }
      }

      return items;
    }

    internal List<object> GetRealizedDataItems()
    {
      var count = m_genPosToNode.Count;
      var items = new List<object>( count );

      for( int i = 0; i < count; i++ )
      {
        var node = m_genPosToNode[ i ] as ItemsGeneratorNode;
        if( node != null )
        {
          items.Add( m_genPosToItem[ i ] );
        }
      }

      return items;
    }

    internal object ItemFromContainer( DependencyObject container )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_ItemFromContainer, DataGridTraceArgs.Container( container ) ) )
      {
        if( this.IsHandlingGlobalItemsReset )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ItemFromContainer, DataGridTraceMessages.CannotProcessOnReset, DataGridTraceArgs.Container( container ) );
          return null;
        }

        var dataItemStore = CustomItemContainerGenerator.GetDataItemProperty( container );
        if( ( dataItemStore == null ) || dataItemStore.IsEmpty )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ItemFromContainer, DataGridTraceMessages.ItemNotFound, DataGridTraceArgs.Container( container ) );
          return null;
        }

        var dataItem = dataItemStore.Data;
        var index = m_genPosToContainer.IndexOf( container );

        if( index >= 0 )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ItemFromContainer, DataGridTraceMessages.ContainerFound, DataGridTraceArgs.Container( container ), DataGridTraceArgs.GeneratorIndex( index ) );

          var detailNode = m_genPosToNode[ index ] as DetailGeneratorNode;
          if( detailNode != null )
          {
            this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ItemFromContainer, DataGridTraceMessages.ContainerIsInDetail, DataGridTraceArgs.Container( container ), DataGridTraceArgs.GeneratorIndex( index ), DataGridTraceArgs.Node( detailNode ) );
            return null;
          }
        }
        else
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ItemFromContainer, DataGridTraceMessages.ContainerNotFound, DataGridTraceArgs.Container( container ), DataGridTraceArgs.Item( dataItem ) );
        }

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ItemFromContainer, DataGridTraceMessages.ItemFound, DataGridTraceArgs.Item( dataItem ), DataGridTraceArgs.Container( container ) );
        return dataItem;
      }
    }

    public int IndexFromItem( object item )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_IndexFromItem, DataGridTraceArgs.Item( item ) ) )
      {
        if( item == null )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_IndexFromItem, DataGridTraceMessages.ItemNotFound, DataGridTraceArgs.Item( item ) );
          return -1;
        }

        this.EnsureNodeTreeCreated();

        if( m_startNode == null )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_IndexFromItem, DataGridTraceMessages.EmptyTree, DataGridTraceArgs.Item( item ) );
          return -1;
        }

        var index = this.FindFirstGeneratedIndexForLocalItem( item );
        if( index >= 0 )
        {
          var itemIndex = m_genPosToIndex[ index ];

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_IndexFromItem, DataGridTraceMessages.IndexFound, DataGridTraceArgs.Index( itemIndex ), DataGridTraceArgs.GeneratorIndex( index ), DataGridTraceArgs.Item( item ) );
          return itemIndex;
        }
        else
        {
          //Note: Under that specific case, I do not want to search through collapsed group nodes... Effectivelly, an item "below" a collapsed group node 
          //      Have no index as per the "item to index" interface of the generator. 
          var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
          var itemIndex = nodeHelper.FindItem( item );

          if( itemIndex >= 0 )
          {
            this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_IndexFromItem, DataGridTraceMessages.IndexFound, DataGridTraceArgs.Index( itemIndex ), DataGridTraceArgs.Node( nodeHelper.CurrentNode ), DataGridTraceArgs.Item( item ) );
            return itemIndex;
          }
        }

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_IndexFromItem, DataGridTraceMessages.ItemNotFoundOrCollapsed, DataGridTraceArgs.Item( item ) );
        return -1;
      }
    }

    public object ItemFromIndex( int index )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_ItemFromIndex, DataGridTraceArgs.Index( index ) ) )
      {
        this.EnsureNodeTreeCreated();

        if( m_startNode == null )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ItemFromIndex, DataGridTraceMessages.EmptyTree, DataGridTraceArgs.Index( index ) );
          return null;
        }

        var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
        var item = nodeHelper.FindIndex( index );

        if( item != null )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ItemFromIndex, DataGridTraceMessages.ItemFound, DataGridTraceArgs.Item( item ), DataGridTraceArgs.Node( nodeHelper.CurrentNode ), DataGridTraceArgs.Index( index ) );
        }
        else
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ItemFromIndex, DataGridTraceMessages.ItemNotFound, DataGridTraceArgs.Index( index ) );
        }

        return item;
      }
    }

    internal bool IsGroupRealized( Group group )
    {
      var groupNode = ( group != null ) ? group.GeneratorNode : null;
      if( groupNode == null )
        return false;

      foreach( var node in m_genPosToNode )
      {
        var parentNode = node;

        // Find the first GroupGeneratorNode among its ancestors.
        while( parentNode != null )
        {
          if( parentNode is GroupGeneratorNode )
            break;

          parentNode = parentNode.Parent;
        }

        // Find out if the GroupGeneratorNode found is the group or a
        // child of the target group.
        while( parentNode != null )
        {
          if( parentNode == groupNode )
            return true;

          // The current node cannot be a child of the target group if
          // the parent node is not a group.
          parentNode = parentNode.Parent as GroupGeneratorNode;
        }
      }

      return false;
    }

    public int GetGroupIndex( Group group )
    {
      if( group == null )
        throw DataGridException.Create<ArgumentNullException>( "group", m_dataGridControl );

      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_GetGroupIndex, DataGridTraceArgs.Group( group ) ) )
      {
        this.EnsureNodeTreeCreated();

        if( m_startNode == null )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GetGroupIndex, DataGridTraceMessages.EmptyTree, DataGridTraceArgs.Group( group ) );
          return -1;
        }

        var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
        if( nodeHelper.FindGroup( group.CollectionViewGroup ) )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GetGroupIndex, DataGridTraceMessages.GroupFound, DataGridTraceArgs.Index( nodeHelper.Index ), DataGridTraceArgs.Node( nodeHelper.CurrentNode ), DataGridTraceArgs.Group( group ) );
          return nodeHelper.Index;
        }

        foreach( var detailNode in this.GetDetailGeneratorNodes() )
        {
          var index = detailNode.DetailGenerator.GetGroupIndex( group );
          if( index >= 0 )
          {
            var detailIndex = this.FindGlobalIndexForDetailNode( detailNode );
            if( detailIndex >= 0 )
            {
              var groupIndex = index + detailIndex;

              this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GetGroupIndex, DataGridTraceMessages.GroupFound, DataGridTraceArgs.Index( groupIndex ), DataGridTraceArgs.Group( group ) );
              return groupIndex;
            }

            this.TraceEvent( TraceEventType.Error, DataGridTraceEventId.CustomItemContainerGenerator_GetGroupIndex, DataGridTraceMessages.DetailNodeNotFound, DataGridTraceArgs.Group( group ) );
            return -1;
          }
        }

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GetGroupIndex, DataGridTraceMessages.GroupNotFound, DataGridTraceArgs.Group( group ) );
        return -1;
      }
    }

    public Group GetGroupFromItem( object item )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_GetGroupFromItem, DataGridTraceArgs.Item( item ) ) )
      {
        this.EnsureNodeTreeCreated();

        if( m_startNode == null )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GetGroupFromItem, DataGridTraceMessages.EmptyTree, DataGridTraceArgs.Item( item ) );
          return null;
        }

        GeneratorNode node;

        var index = this.FindFirstGeneratedIndexForLocalItem( item );
        if( index >= 0 )
        {
          node = m_genPosToNode[ index ];

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GetGroupFromItem, DataGridTraceMessages.NodeFound, DataGridTraceArgs.Node( node ), DataGridTraceArgs.GeneratorIndex( index ), DataGridTraceArgs.Item( item ) );
        }
        else
        {
          var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
          if( !nodeHelper.Contains( item ) ) //NOTE: this will only return items directly contained in this generator (not from details )
            throw DataGridException.Create<InvalidOperationException>( "An attempt was made to retrieve the group of an item that does not belong to the generator.", m_dataGridControl );

          //if the nodeHelper was able to locate the content, use the nodeHelper's CurrentNode as the node for the item.
          node = nodeHelper.CurrentNode;

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GetGroupFromItem, DataGridTraceMessages.NodeFound, DataGridTraceArgs.Node( node ), DataGridTraceArgs.Item( item ) );
        }

        if( node != null )
        {
          var parentGroup = node.Parent as GroupGeneratorNode;
          if( parentGroup != null )
          {
            this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GetGroupFromItem, DataGridTraceMessages.GroupFound, DataGridTraceArgs.Group( parentGroup.UIGroup ), DataGridTraceArgs.Node( parentGroup ), DataGridTraceArgs.Item( item ) );
            return parentGroup.UIGroup;
          }
        }

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GetGroupFromItem, DataGridTraceMessages.GroupNotFound, DataGridTraceArgs.Item( item ) );
        return null;
      }
    }

    public Group GetGroupFromCollectionViewGroup( CollectionViewGroup collectionViewGroup )
    {
      this.EnsureNodeTreeCreated();

      return this.GetGroupFromCollectionViewGroupCore( collectionViewGroup );
    }

    private Group GetGroupFromCollectionViewGroupCore( CollectionViewGroup collectionViewGroup )
    {
      GroupGeneratorNode node;
      if( m_groupNodeMappingCache.TryGetValue( collectionViewGroup, out node ) )
        return node.UIGroup;

      return null;
    }

    public CollectionViewGroup GetParentGroupFromItem( object item, bool recurseDetails )
    {
      if( item == null )
        throw DataGridException.Create<ArgumentNullException>( "item", m_dataGridControl );

      CollectionViewGroup collectionViewGroup;
      if( !this.TryGetParentGroupFromItem( item, recurseDetails, out collectionViewGroup ) )
        throw DataGridException.Create<InvalidOperationException>( "An attempt was made to retrieve the parent group of an item that does not belong to the generator.", m_dataGridControl );

      return collectionViewGroup;
    }

    public bool TryGetParentGroupFromItem( object item, bool recurseDetails, out CollectionViewGroup collectionViewGroup )
    {
      collectionViewGroup = null;

      if( item == null )
        return false;

      this.EnsureNodeTreeCreated();

      if( m_startNode == null )
        throw DataGridException.Create<DataGridInternalException>( "Start node is null for the CollectionViewGroup.", m_dataGridControl );

      if( this.TryGetParentGroupFromItemHelper( item, out collectionViewGroup ) )
        return true;

      if( recurseDetails )
      {
        //the item was not found in this generator's content... check all the detail generators
        foreach( var generator in this.GetDetailGenerators() )
        {
          if( generator.TryGetParentGroupFromItem( item, recurseDetails, out collectionViewGroup ) )
            return true;
        }
      }

      return false;
    }

    private bool TryGetParentGroupFromItemHelper( object item, out CollectionViewGroup collectionViewGroup )
    {
      //This helper method is used to simplify previous code flow of the TryGetParentGroupFromItem method.
      collectionViewGroup = null;

      //-----------------------------------------------
      //1 - First check is to see of the item is a CVG.
      //-----------------------------------------------
      var groupItem = item as CollectionViewGroup;
      if( groupItem != null )
      {
        var group = this.GetGroupFromCollectionViewGroup( groupItem );
        if( group != null )
        {
          var groupGeneratorNode = group.GeneratorNode;
          if( groupGeneratorNode.Parent == null )
            return true;

          //if the nodeHelper was able to locate the content, use the nodeHelper's CurrentNode as the node for the item.
          collectionViewGroup = ( ( GroupGeneratorNode )groupGeneratorNode.Parent ).CollectionViewGroup;
          return true;
        }

        //item is a CVG, but is not present in the generator!
        return false;
      }

      //-----------------------------------------------
      //2 - Second check is to see if the item is already in the generated list
      //-----------------------------------------------

      //item might be in the "generated" list... much quicker to find-out if it is!
      //note: if the item belongs to a detail, then it will be excluded from the "fast" algo.
      var itemGenPosIndex = this.FindFirstGeneratedIndexForLocalItem( item );
      if( itemGenPosIndex != -1 )
      {
        //item was generated and was not from a DetailGeneratorNode
        if( m_genPosToNode[ itemGenPosIndex ].Parent == null )
          return true;

        collectionViewGroup = ( ( GroupGeneratorNode )m_genPosToNode[ itemGenPosIndex ].Parent ).CollectionViewGroup;
        return true;
      }

      //-----------------------------------------------
      //3 - Third check is to check of the item is a GroupHeaderFooterItem
      //-----------------------------------------------
      if( item.GetType() == typeof( GroupHeaderFooterItem ) )
      {
        var groupHeaderFooterItem = ( GroupHeaderFooterItem )item;
        var parentGroup = groupHeaderFooterItem.Group;

        if( this.GetGroupFromCollectionViewGroup( parentGroup ) != null )
        {
          //since the goal is the find the parentGroup from the item passed (which is a GroupHeader or GroupFooter), then the Group
          //is what I am looking for.
          collectionViewGroup = parentGroup;

          return true;
        }

        //Item was a GroupHeaderFooterItem but was not part of the genreator!
        return false;
      }

      //-----------------------------------------------
      //4 - Final Check
      //-----------------------------------------------

      //if the item was not generated, then try to find the item as is within the generator's content  
      var finalNodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
      if( finalNodeHelper.AbsoluteFindItem( item ) )
      {
        //item was not generated but was part of this generator
        if( finalNodeHelper.CurrentNode.Parent == null )
          return true;

        collectionViewGroup = ( ( GroupGeneratorNode )finalNodeHelper.CurrentNode.Parent ).CollectionViewGroup;
        return true;
      }

      return false;
    }

    internal bool ExpandGroup( CollectionViewGroup group )
    {
      return this.ExpandGroup( group, false );
    }

    internal bool ExpandGroup( CollectionViewGroup group, bool recurseDetails )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "An attempt was made to expand a group while the generator is busy generating items.", m_dataGridControl );

      if( group == null )
        throw DataGridException.Create<ArgumentNullException>( "group", m_dataGridControl );

      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_ExpandGroup, DataGridTraceArgs.Group( group ) ) )
      {
        this.EnsureNodeTreeCreated();

        if( m_firstItem == null )
          throw DataGridException.Create<DataGridInternalException>( "No GeneratorNode found for the group.", m_dataGridControl );

        var uiGroup = this.GetGroupFromCollectionViewGroup( group );
        if( uiGroup != null )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ExpandGroup, DataGridTraceMessages.GroupFound, DataGridTraceArgs.Group( uiGroup ), DataGridTraceArgs.Node( uiGroup.GeneratorNode ), DataGridTraceArgs.Value( uiGroup.GeneratorNode.IsExpanded ) );

          uiGroup.GeneratorNode.IsExpanded = true;
          return true;
        }

        if( recurseDetails )
        {
          foreach( var generator in this.GetDetailGenerators() )
          {
            // If the item is not found in the detail generator, it will return false;
            if( generator.ExpandGroup( group, recurseDetails ) )
              return true;
          }
        }

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ExpandGroup, DataGridTraceMessages.GroupNotFound, DataGridTraceArgs.Group( group ) );
        return false;
      }
    }

    internal bool CollapseGroup( CollectionViewGroup group )
    {
      return this.CollapseGroup( group, false );
    }

    internal bool CollapseGroup( CollectionViewGroup group, bool recurseDetails )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "An attempt was made to collapse a group while the generator is busy generating items.", m_dataGridControl );

      if( group == null )
        throw DataGridException.Create<ArgumentNullException>( "group", m_dataGridControl );

      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_CollapseGroup, DataGridTraceArgs.Group( group ) ) )
      {
        this.EnsureNodeTreeCreated();

        if( m_firstItem == null )
          throw DataGridException.Create<DataGridInternalException>( "No GeneratorNode found for the group.", m_dataGridControl );

        var uiGroup = this.GetGroupFromCollectionViewGroup( group );
        if( uiGroup != null )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_CollapseGroup, DataGridTraceMessages.GroupFound, DataGridTraceArgs.Group( uiGroup ), DataGridTraceArgs.Node( uiGroup.GeneratorNode ), DataGridTraceArgs.Value( uiGroup.GeneratorNode.IsExpanded ) );

          uiGroup.GeneratorNode.IsExpanded = false;
          return true;
        }

        if( recurseDetails )
        {
          foreach( var generator in this.GetDetailGenerators() )
          {
            // If the item is not found in the detail generator, it will return false;
            if( generator.CollapseGroup( group, recurseDetails ) )
              return true;
          }
        }

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_CollapseGroup, DataGridTraceMessages.GroupNotFound, DataGridTraceArgs.Group( group ) );
        return false;
      }
    }

    internal bool ToggleGroupExpansion( CollectionViewGroup group )
    {
      return this.ToggleGroupExpansion( group, false );
    }

    internal bool ToggleGroupExpansion( CollectionViewGroup group, bool recurseDetails )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "An attempt was made to toggle a group's expansion while the generator is busy generating items.", m_dataGridControl );

      if( group == null )
        throw DataGridException.Create<ArgumentNullException>( "group", m_dataGridControl );

      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_ToggleGroupExpansion, DataGridTraceArgs.Group( group ) ) )
      {
        this.EnsureNodeTreeCreated();

        if( m_firstItem == null )
          throw DataGridException.Create<DataGridInternalException>( "No GeneratorNode found for the group.", m_dataGridControl );

        var uiGroup = this.GetGroupFromCollectionViewGroup( group );
        if( uiGroup != null )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ToggleGroupExpansion, DataGridTraceMessages.GroupFound, DataGridTraceArgs.Group( uiGroup ), DataGridTraceArgs.Node( uiGroup.GeneratorNode ), DataGridTraceArgs.Value( uiGroup.GeneratorNode.IsExpanded ) );

          var groupNode = uiGroup.GeneratorNode;
          groupNode.IsExpanded = !groupNode.IsExpanded;
          return true;
        }

        if( recurseDetails )
        {
          foreach( var generator in this.GetDetailGenerators() )
          {
            // If the item is not found in the detail generator, it will return false;
            if( generator.ToggleGroupExpansion( group, recurseDetails ) )
              return true;
          }
        }

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ToggleGroupExpansion, DataGridTraceMessages.GroupNotFound, DataGridTraceArgs.Group( group ) );
        return false;
      }
    }

    internal bool? IsGroupExpanded( CollectionViewGroup group )
    {
      return this.IsGroupExpanded( group, false );
    }

    internal bool? IsGroupExpanded( CollectionViewGroup group, bool recurseDetails )
    {
      if( group == null )
        throw DataGridException.Create<ArgumentNullException>( "group", m_dataGridControl );

      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_IsGroupExpanded, DataGridTraceArgs.Group( group ) ) )
      {
        this.EnsureNodeTreeCreated();

        if( m_firstItem == null )
          throw DataGridException.Create<DataGridInternalException>( "No GeneratorNode found for the group.", m_dataGridControl );

        var uiGroup = this.GetGroupFromCollectionViewGroup( group );
        if( uiGroup != null )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_IsGroupExpanded, DataGridTraceMessages.GroupFound, DataGridTraceArgs.Group( uiGroup ), DataGridTraceArgs.Node( uiGroup.GeneratorNode ), DataGridTraceArgs.Value( uiGroup.GeneratorNode.IsExpanded ) );
          return uiGroup.GeneratorNode.IsExpanded;
        }

        if( recurseDetails )
        {
          //the group was not found in this generator, check in all the child generators for the group.
          foreach( var generator in this.GetDetailGenerators() )
          {
            var result = generator.IsGroupExpanded( group, recurseDetails );
            if( result.HasValue )
              return result.Value;
          }
        }

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_IsGroupExpanded, DataGridTraceMessages.GroupNotFound, DataGridTraceArgs.Group( group ) );
        return null;
      }
    }

    internal void ExpandDetails( object dataItem )
    {

    }

    internal void CollapseDetails( object dataItem )
    {

    }

    internal void ToggleDetails( object dataItem )
    {
    }

    internal bool AreDetailsExpanded( object dataItem )
    {
      return false;
    }

    public void RemoveAllAndNotify()
    {
      if( m_startNode == null )
        return;

      this.RemoveGeneratedItems( int.MinValue, int.MaxValue, null );
      this.SendResetEvent();
      this.ClearRecyclingPools();
    }

    internal int FindIndexForItem( object item, DataGridContext dataGridContext )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_FindIndexForItem, DataGridTraceArgs.Item( item ) ) )
      {
        this.EnsureNodeTreeCreated();

        // If the seeked DataGridContext is this generator's context, the item should be here.
        // When the seeked DataGridContext is null, search for the first item match no matter the generator's context.
        if( ( m_dataGridContext == dataGridContext ) || ( dataGridContext == null ) )
        {
          var itemIndex = this.IndexFromItem( item );
          if( itemIndex >= 0 )
          {
            this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_FindIndexForItem, DataGridTraceMessages.ItemFound, DataGridTraceArgs.Item( item ), DataGridTraceArgs.Index( itemIndex ) );
            return itemIndex;
          }

          if( dataGridContext != null )
          {
            this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_FindIndexForItem, DataGridTraceMessages.ItemNotFound, DataGridTraceArgs.Item( item ) );
            return -1;
          }
        }

        foreach( var masterToDetails in m_masterToDetails )
        {
          var detailsTotalCount = 0;

          foreach( var detailNode in masterToDetails.Value )
          {
            var itemIndex = detailNode.DetailGenerator.FindIndexForItem( item, dataGridContext );
            if( itemIndex >= 0 )
            {
              var masterIndex = this.IndexFromItem( masterToDetails.Key );
              if( masterIndex >= 0 )
              {
                var result = masterIndex + 1 + detailsTotalCount + itemIndex;

                this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_FindIndexForItem, DataGridTraceMessages.ItemFound, DataGridTraceArgs.Item( item ), DataGridTraceArgs.Index( result ) );
                return result;
              }

              this.TraceEvent( TraceEventType.Error, DataGridTraceEventId.CustomItemContainerGenerator_FindIndexForItem, DataGridTraceMessages.ItemNotFound, DataGridTraceArgs.Item( item ) );
              return -1;
            }

            detailsTotalCount += detailNode.ItemCount;
          }
        }

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_FindIndexForItem, DataGridTraceMessages.ItemNotFound, DataGridTraceArgs.Item( item ) );
        return -1;
      }
    }

    public DependencyObject GetRealizedContainerForIndex( int index )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_GetRealizedContainerForIndex, DataGridTraceArgs.Index( index ) ) )
      {
        this.EnsureNodeTreeCreated();

        if( m_startNode == null )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GetRealizedContainerForIndex, DataGridTraceMessages.EmptyTree, DataGridTraceArgs.Index( index ) );
          return null;
        }

        var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
        if( !nodeHelper.FindNodeForIndex( index ) )
          throw DataGridException.Create<ArgumentException>( "The specified index does not correspond to an item in the generator.", m_dataGridControl, "index" );

        var indexIndex = m_genPosToIndex.IndexOf( index );
        if( indexIndex >= 0 )
        {
          var container = m_genPosToContainer[ indexIndex ];

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GetRealizedContainerForIndex, DataGridTraceMessages.ContainerFound, DataGridTraceArgs.Container( container ), DataGridTraceArgs.GeneratorIndex( indexIndex ), DataGridTraceArgs.Index( index ) );
          return container;
        }

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GetRealizedContainerForIndex, DataGridTraceMessages.ContainerNotFound, DataGridTraceArgs.Index( index ) );
        return null;
      }
    }

    public int GetRealizedIndexForContainer( DependencyObject container )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_GetRealizedIndexForContainer, DataGridTraceArgs.Container( container ) ) )
      {
        this.EnsureNodeTreeCreated();

        var index = m_genPosToContainer.IndexOf( container );
        if( index >= 0 )
        {
          var itemIndex = m_genPosToIndex[ index ];

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GetRealizedIndexForContainer, DataGridTraceMessages.ContainerFound, DataGridTraceArgs.Index( itemIndex ), DataGridTraceArgs.GeneratorIndex( index ) );
          return itemIndex;
        }

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GetRealizedIndexForContainer, DataGridTraceMessages.ContainerNotFound, DataGridTraceArgs.Container( container ) );
        return -1;
      }
    }

    internal List<int> GetMasterIndexesWithExpandedDetails()
    {
      var masterIndexes = new List<int>();

      foreach( var dataItem in m_masterToDetails.Keys )
      {
        var itemIndex = m_collectionView.IndexOf( dataItem );
        Debug.Assert( itemIndex >= 0 );

        masterIndexes.Add( itemIndex );
      }

      masterIndexes.Sort();

      return masterIndexes;
    }

    internal IEnumerable<DataGridContext> GetChildContextsForMasterItem( object item )
    {
      if( item == null )
        yield break;

      List<DetailGeneratorNode> detailNodes;
      if( m_masterToDetails.TryGetValue( item, out detailNodes ) )
      {
        foreach( var detailNode in detailNodes )
        {
          yield return detailNode.DetailContext;
        }
      }
    }

    public DataGridContext GetChildContext( object parentItem, string relationName )
    {
      if( parentItem == null )
        throw DataGridException.Create<ArgumentNullException>( "parentItem", m_dataGridControl );

      this.EnsureNodeTreeCreated();

      //Note: This function does not validate if the parentItem is part of the generator or not, effectivelly, there is
      // no need to validate of the parentItem is part of the Generator or not, the only calling function is already doing the 
      // validation (DataGridContext.GetChildContext() ).
      List<DetailGeneratorNode> details;
      if( m_masterToDetails.TryGetValue( parentItem, out details ) )
      {
        foreach( DetailGeneratorNode detailNode in details )
        {
          //Note: DetailContext.SourceDetailConfiguration will always be non-null, since we are looking for child contexts ( only the root master context can have a null
          //SourceDetailConfiguration.
          if( detailNode.DetailContext.SourceDetailConfiguration.RelationName == relationName )
            return detailNode.DetailContext;
        }
      }

      return null;
    }

    public IEnumerable<DataGridContext> GetChildContexts()
    {
      this.EnsureNodeTreeCreated();

      return this.GetChildContextsCore();
    }

    internal IEnumerable<DataGridContext> GetChildContextsCore()
    {
      return this.GetDetailContexts();
    }

    public bool Contains( object item )
    {
      this.EnsureNodeTreeCreated();

      var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
      return nodeHelper.Contains( item );
    }

    #region Sticky Headers Methods

    public List<StickyContainerGenerated> GenerateStickyHeaders(
      DependencyObject container,
      bool areHeadersSticky,
      bool areGroupHeadersSticky,
      bool areParentRowsSticky )
    {
      var generatedStickyContainers = new List<StickyContainerGenerated>();

      GeneratorNode containerNode;
      int containerRealizedIndex;
      object containerDataItem;

      if( this.FindGeneratorListMappingInformationForContainer( container,
                                                                out containerNode,
                                                                out containerRealizedIndex,
                                                                out containerDataItem ) )
      {
        var nodeHelper = default( GeneratorNodeHelper );
        var detailNode = containerNode as DetailGeneratorNode;

        if( detailNode != null )
        {
          // Get the parent item of the detail node to be able
          // to readjust the containerRealizedIndex to the one
          // of the master item container since the detail node
          // will be processed by the DetailGenerator.
          containerDataItem = detailNode.DetailContext.ParentItem;

          // OPTIMIZATION: We will look in the m_genPos* first to avoid using
          //               FindItem for performance reason.
          var index = m_genPosToItem.IndexOf( containerDataItem );
          if( index >= 0 )
          {
            var sourceDataIndex = ( int )m_genPosToContainer[ index ].GetValue( DataGridVirtualizingPanel.ItemIndexProperty );
            containerNode = m_genPosToNode[ index ];
            containerRealizedIndex = m_genPosToIndex[ index ];

            var collectionNode = containerNode as CollectionGeneratorNode;
            if( collectionNode != null )
            {
              nodeHelper = new GeneratorNodeHelper( containerNode, containerRealizedIndex - collectionNode.IndexOf( containerDataItem ), sourceDataIndex );
            }
          }

          if( nodeHelper == null )
          {
            // We want to find the ItemsGeneratorNode for the DetailNode.
            nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
            containerRealizedIndex = nodeHelper.FindItem( containerDataItem );
            containerNode = nodeHelper.CurrentNode;
          }

          if( containerRealizedIndex == -1 )
            throw DataGridException.Create<DataGridInternalException>( "The index of a sticky header container is out of bound.", m_dataGridControl );

          generatedStickyContainers.AddRange(
            this.GenerateStickyHeadersForDetail( container,
                                                 detailNode,
                                                 areHeadersSticky,
                                                 areGroupHeadersSticky,
                                                 areParentRowsSticky ) );
        }
        else
        {
          var collectionNode = containerNode as CollectionGeneratorNode;
          if( collectionNode != null )
          {
            // We don't need to have an up to date sourceDataIndex so we pass 0
            nodeHelper = new GeneratorNodeHelper( containerNode, containerRealizedIndex - collectionNode.IndexOf( containerDataItem ), 0 );
          }

          if( nodeHelper == null )
          {
            nodeHelper = new GeneratorNodeHelper( containerNode, 0, 0 );
            nodeHelper.ReverseCalculateIndex();
          }
        }

        if( ( areParentRowsSticky )
          && ( containerDataItem != null )
          && ( this.AreDetailsExpanded( containerDataItem ) ) )
        {
          generatedStickyContainers.Add( this.GenerateStickyParentRow( containerRealizedIndex ) );
        }

        // We want to find the HeaderFooterGeneratorNode for the container 
        // node. This is to find the headers for the container.
        nodeHelper.MoveToFirst();
        var headersNode = nodeHelper.CurrentNode as HeadersFootersGeneratorNode;

        // There is no headers to generate if the item count of the node is 0.
        if( headersNode.ItemCount > 0 )
        {
          if( ( ( areHeadersSticky ) && ( headersNode.Parent == null ) )
            || ( ( areGroupHeadersSticky ) && ( headersNode.Parent is GroupGeneratorNode ) ) )
          {
            generatedStickyContainers.AddRange(
              this.GenerateStickyHeadersForNode( headersNode,
                                                 nodeHelper.Index,
                                                 containerRealizedIndex,
                                                 ( headersNode == containerNode ) ) );
          }
        }

        // We must also find the top most headers for our level of detail and, if they need to be sticky,
        // we will generate the containers and add them the to list.
        var topMostHeaderNode = this.GetTopMostHeaderNode( nodeHelper );
        if( ( areHeadersSticky )
          && ( topMostHeaderNode != null )
          && ( topMostHeaderNode != headersNode )
          && ( topMostHeaderNode.ItemCount > 0 ) )
        {
          generatedStickyContainers.AddRange(
            this.GenerateStickyHeadersForNode( topMostHeaderNode, nodeHelper.Index ) );
        }
      }

      return generatedStickyContainers;
    }

    private HeadersFootersGeneratorNode GetTopMostHeaderNode( GeneratorNodeHelper nodeHelper )
    {
      var parentNode = default( HeadersFootersGeneratorNode );

      if( nodeHelper.MoveToParent()
        && nodeHelper.MoveToFirst() )
      {
        if( ( nodeHelper.CurrentNode is HeadersFootersGeneratorNode )
          && ( nodeHelper.CurrentNode.Parent == null ) )
        {
          parentNode = nodeHelper.CurrentNode as HeadersFootersGeneratorNode;
        }
        else
        {
          parentNode = this.GetTopMostHeaderNode( nodeHelper );
        }
      }

      return parentNode;
    }

    private List<StickyContainerGenerated> GenerateStickyHeadersForDetail(
      DependencyObject container,
      DetailGeneratorNode detailNode,
      bool areHeadersSticky,
      bool areGroupHeadersSticky,
      bool areParentRowsSticky )
    {
      var generatedStickyContainers = detailNode.DetailGenerator.GenerateStickyHeaders( container, areHeadersSticky, areGroupHeadersSticky, areParentRowsSticky );
      var detailIndex = this.FindGlobalIndexForDetailNode( detailNode );
      var count = generatedStickyContainers.Count;

      for( int i = 0; i < count; i++ )
      {
        var stickyContainer = generatedStickyContainers[ i ];
        var detailItemIndex = stickyContainer.Index + detailIndex;

        //if the container was just realized, ensure to add it to the lists maintaining the generated items.
        if( stickyContainer.IsNewlyRealized )
        {
          var insertionIndex = this.FindInsertionPoint( detailItemIndex );
          var item = CustomItemContainerGenerator.GetDataItemProperty( stickyContainer.StickyContainer ).Data;

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GenerateStickyHeadersForDetail, DataGridTraceMessages.ContainerAdded, DataGridTraceArgs.Container( stickyContainer.StickyContainer ), DataGridTraceArgs.Node( detailNode ), DataGridTraceArgs.Item( item ), DataGridTraceArgs.GeneratorIndex( insertionIndex ), DataGridTraceArgs.Index( detailItemIndex ) );

          m_genPosToIndex.Insert( insertionIndex, detailItemIndex );
          m_genPosToItem.Insert( insertionIndex, item );
          m_genPosToContainer.Insert( insertionIndex, stickyContainer.StickyContainer );
          m_genPosToNode.Insert( insertionIndex, detailNode );
        }

        generatedStickyContainers[ i ] = new StickyContainerGenerated( stickyContainer.StickyContainer, detailItemIndex, stickyContainer.IsNewlyRealized );
      }

      return generatedStickyContainers;
    }

    private StickyContainerGenerated GenerateStickyParentRow( int itemNodeIndex )
    {
      var generator = ( ICustomItemContainerGenerator )this;
      var position = generator.GeneratorPositionFromIndex( itemNodeIndex );

      using( generator.StartAt( position, GeneratorDirection.Forward, true ) )
      {
        bool isNewlyRealized;
        var container = generator.GenerateNext( out isNewlyRealized );

        return new StickyContainerGenerated( container, itemNodeIndex, isNewlyRealized );
      }
    }

    private List<StickyContainerGenerated> GenerateStickyHeadersForNode(
      HeadersFootersGeneratorNode headerNode,
      int headerNodeIndex )
    {
      return this.GenerateStickyHeadersForNode( headerNode, headerNodeIndex, -1, false );
    }

    private List<StickyContainerGenerated> GenerateStickyHeadersForNode(
      HeadersFootersGeneratorNode headerNode,
      int headerNodeIndex,
      int realizedIndex,
      bool isRealizedIndexPartOfHeaderNode )
    {
      var generatedStickyContainers = new List<StickyContainerGenerated>();

      // The container is already part of the header.
      var generator = ( ICustomItemContainerGenerator )this;
      var position = generator.GeneratorPositionFromIndex( headerNodeIndex );

      // In that case, the potential sticky containers are the requested one and up.
      using( generator.StartAt( position, GeneratorDirection.Forward, true ) )
      {
        for( int i = 0; i < headerNode.ItemCount; i++ )
        {
          // If the realized index represent the index of one of the HeaderNode
          // item, do not process
          if( ( isRealizedIndexPartOfHeaderNode )
              && ( headerNodeIndex + i > realizedIndex ) )
            break;

          var item = headerNode.GetAt( i );
          var groupHeaderFooterItem = default( GroupHeaderFooterItem? );

          if( item is GroupHeaderFooterItem )
          {
            groupHeaderFooterItem = ( GroupHeaderFooterItem )item;
          }

          if( ( groupHeaderFooterItem != null )
            && ( !groupHeaderFooterItem.Value.Group.IsBottomLevel || ( this.IsGroupExpanded( groupHeaderFooterItem.Value.Group ) != true ) ) )
          {
            this.Skip();
            continue;
          }

          bool isNewlyRealized;
          var stickyContainer = generator.GenerateNext( out isNewlyRealized );

          generatedStickyContainers.Add( new StickyContainerGenerated( stickyContainer, headerNodeIndex + i, isNewlyRealized ) );
        }
      }

      return generatedStickyContainers;
    }

    private bool FindGeneratorListMappingInformationForContainer(
      DependencyObject container,
      out GeneratorNode containerNode,
      out int containerRealizedIndex,
      out object containerDataItem )
    {
      var index = m_genPosToContainer.IndexOf( container );
      if( index < 0 )
      {
        containerNode = null;
        containerRealizedIndex = -1;
        containerDataItem = null;

        return false;
      }
      else
      {
        containerNode = m_genPosToNode[ index ];
        containerRealizedIndex = m_genPosToIndex[ index ];
        containerDataItem = m_genPosToItem[ index ];

        return true;
      }
    }

    public int GetLastHoldingContainerIndexForStickyHeader( DependencyObject stickyHeader )
    {
      return this.GetLastHoldingContainerIndexForStickyHeaderRecurse( stickyHeader, this.ItemCount );
    }

    private int GetLastHoldingContainerIndexForStickyHeaderRecurse( DependencyObject stickyHeader, int parentCount )
    {
      var lastContainerIndex = 0;

      int containerRealizedIndex;
      GeneratorNode containerNode;
      object containerDataItem;

      if( this.FindGeneratorListMappingInformationForContainer( stickyHeader, out containerNode, out containerRealizedIndex, out containerDataItem ) )
      {
        var detailNode = containerNode as DetailGeneratorNode;
        if( detailNode != null )
        {
          lastContainerIndex =
            detailNode.DetailGenerator.GetLastHoldingContainerIndexForStickyHeaderRecurse( stickyHeader, detailNode.ItemCount ) +
            this.FindGlobalIndexForDetailNode( detailNode );
        }
        else
        {
          var itemsNode = containerNode as ItemsGeneratorNode;
          if( itemsNode != null )
          {
            // This means that the sticky container is a MasterRow for a detail.
            if( this.AreDetailsExpanded( containerDataItem ) )
            {
              var detailNodesForDataItem = m_masterToDetails[ containerDataItem ];

              foreach( DetailGeneratorNode detailNodeForDataItem in detailNodesForDataItem )
              {
                lastContainerIndex +=
                  detailNodeForDataItem.ItemCount +
                  this.FindGlobalIndexForDetailNode( detailNodeForDataItem ) -
                  1;
              }
            }
          }
          else
          {
            var nodeHelper = default( GeneratorNodeHelper );

            // This means that the sticky container is a Header.
            var collectionNode = containerNode as CollectionGeneratorNode;
            if( collectionNode != null )
            {
              // We don't need to have an up to date sourceDataIndex so we pass 0
              nodeHelper = new GeneratorNodeHelper( containerNode, containerRealizedIndex - collectionNode.IndexOf( containerDataItem ), 0 );
            }

            if( nodeHelper == null )
            {
              nodeHelper = new GeneratorNodeHelper( containerNode, 0, 0 );
              nodeHelper.ReverseCalculateIndex();
            }

            lastContainerIndex = nodeHelper.Index;

            if( containerNode.Parent != null )
            {
              // This means that it is a GroupHeader
              lastContainerIndex += containerNode.Parent.ItemCount - 1;
            }
            else
            {
              lastContainerIndex += Math.Max( 0, parentCount - 1 );
            }
          }
        }
      }

      return lastContainerIndex;
    }

    public int GetFirstHoldingContainerIndexForStickyFooter( DependencyObject stickyFooter )
    {
      var firstContainerIndex = 0;

      int containerRealizedIndex;
      GeneratorNode containerNode;
      object containerDataItem;

      if( this.FindGeneratorListMappingInformationForContainer( stickyFooter, out containerNode, out containerRealizedIndex, out containerDataItem ) )
      {
        var detailNode = containerNode as DetailGeneratorNode;

        if( detailNode != null )
        {
          int detailIndex = this.FindGlobalIndexForDetailNode( detailNode );
          firstContainerIndex = detailNode.DetailGenerator.GetFirstHoldingContainerIndexForStickyFooter( stickyFooter ) + detailIndex;
        }
        else
        {
          var nodeHelper = default( GeneratorNodeHelper );
          var collectionNode = containerNode as CollectionGeneratorNode;

          if( collectionNode != null )
          {
            // We don't need to have an up to date sourceDataIndex so we pass 0
            nodeHelper = new GeneratorNodeHelper( containerNode, containerRealizedIndex - collectionNode.IndexOf( containerDataItem ), 0 );
          }

          if( nodeHelper == null )
          {
            nodeHelper = new GeneratorNodeHelper( containerNode, 0, 0 );
            nodeHelper.ReverseCalculateIndex();
          }

          nodeHelper.MoveToFirst();
          nodeHelper.MoveToNext(); // We exclude headers from this calculation.
          firstContainerIndex = nodeHelper.Index;
        }
      }

      return firstContainerIndex;
    }

    #endregion Sticky Headers Methods

    #region Sticky Footers Methods

    public List<StickyContainerGenerated> GenerateStickyFooters(
      DependencyObject container,
      bool areFootersSticky,
      bool areGroupFootersSticky )
    {
      var generatedStickyContainers = new List<StickyContainerGenerated>();

      GeneratorNode containerNode;
      int containerRealizedIndex;
      object containerDataItem;


      if( this.FindGeneratorListMappingInformationForContainer( container,
                                                                out containerNode,
                                                                out containerRealizedIndex,
                                                                out containerDataItem ) )
      {
        var nodeHelper = default( GeneratorNodeHelper );
        var detailNode = containerNode as DetailGeneratorNode;

        if( detailNode != null )
        {
          // Get the parent item of the detail node to be able
          // to readjust the containerRealizedIndex to the one
          // of the master item container since the detail node
          // will be processed by the DetailGenerator.
          containerDataItem = detailNode.DetailContext.ParentItem;

          // OPTIMIZATION: We will look in the m_genPos* first to avoid using
          //               FindItem for performance reason.
          int index = m_genPosToItem.IndexOf( containerDataItem );
          if( index >= 0 )
          {
            var sourceDataIndex = ( int )m_genPosToContainer[ index ].GetValue( DataGridVirtualizingPanel.ItemIndexProperty );
            containerNode = m_genPosToNode[ index ];
            containerRealizedIndex = m_genPosToIndex[ index ];

            var collectionNode = containerNode as CollectionGeneratorNode;
            if( collectionNode != null )
            {
              nodeHelper = new GeneratorNodeHelper( containerNode, containerRealizedIndex - collectionNode.IndexOf( containerDataItem ), sourceDataIndex );
            }
          }

          if( nodeHelper == null )
          {
            // We want to find the ItemsGeneratorNode for the DetailNode.
            nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
            containerRealizedIndex = nodeHelper.FindItem( containerDataItem );
            containerNode = nodeHelper.CurrentNode;
          }

          if( containerRealizedIndex == -1 )
            throw DataGridException.Create<DataGridInternalException>( "The index of a sticky footer container is out of bound.", m_dataGridControl );

          generatedStickyContainers.AddRange(
            this.GenerateStickyFootersForDetail( container, detailNode, areFootersSticky, areGroupFootersSticky ) );
        }
        else
        {
          var collectionNode = containerNode as CollectionGeneratorNode;
          if( collectionNode != null )
          {
            // We don't need to have an up to date sourceDataIndex so we pass 0
            nodeHelper = new GeneratorNodeHelper( containerNode, containerRealizedIndex - collectionNode.IndexOf( containerDataItem ), 0 );
          }

          if( nodeHelper == null )
          {
            nodeHelper = new GeneratorNodeHelper( containerNode, 0, 0 );
            nodeHelper.ReverseCalculateIndex();
          }
        }

        bool isHeaderNode =
          ( ( nodeHelper.CurrentNode is HeadersFootersGeneratorNode ) &&
            ( nodeHelper.CurrentNode.Previous == null ) );

        // We want to find the HeaderFooterGeneratorNode for the container 
        // node. This is to find the footers for the container.
        nodeHelper.MoveToEnd();
        var footersNode = nodeHelper.CurrentNode as HeadersFootersGeneratorNode;

        if( !isHeaderNode )
        {
          // There is no footers to generate if the item count of the node is 0.
          if( footersNode.ItemCount > 0 )
          {
            if( ( ( areFootersSticky ) && ( footersNode.Parent == null ) )
              || ( ( areGroupFootersSticky ) && ( footersNode.Parent is GroupGeneratorNode ) ) )
            {
              generatedStickyContainers.AddRange(
                this.GenerateStickyFootersForNode( footersNode,
                                                   nodeHelper.Index,
                                                   containerRealizedIndex,
                                                   ( footersNode == containerNode ) ) );
            }
          }
        }

        // We must also find the bottom most footers for our level of detail and, if they need to be sticky,
        // we will generate the containers and add them the to list.
        var bottomFootersNode = this.GetDetailFootersNode( nodeHelper );

        if( ( areFootersSticky )
          && ( bottomFootersNode != null )
          && ( bottomFootersNode != footersNode )
          && ( bottomFootersNode.ItemCount > 0 ) )
        {
          generatedStickyContainers.AddRange(
            this.GenerateStickyFootersForNode( bottomFootersNode, nodeHelper.Index ) );
        }
      }

      return generatedStickyContainers;
    }

    private List<StickyContainerGenerated> GenerateStickyFootersForDetail(
      DependencyObject container,
      DetailGeneratorNode detailNode,
      bool areFootersSticky,
      bool areGroupFootersSticky )
    {
      var generatedStickyContainers = detailNode.DetailGenerator.GenerateStickyFooters( container, areFootersSticky, areGroupFootersSticky );
      var detailIndex = this.FindGlobalIndexForDetailNode( detailNode );
      var count = generatedStickyContainers.Count;

      for( int i = 0; i < count; i++ )
      {
        var stickyContainer = generatedStickyContainers[ i ];
        var detailItemIndex = stickyContainer.Index + detailIndex;

        //if the container was just realized, ensure to add it to the lists maintaining the generated items.
        if( stickyContainer.IsNewlyRealized )
        {
          var insertionIndex = this.FindInsertionPoint( detailItemIndex );
          var item = CustomItemContainerGenerator.GetDataItemProperty( stickyContainer.StickyContainer ).Data;

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GenerateStickyFootersForDetail, DataGridTraceMessages.ContainerAdded, DataGridTraceArgs.Container( stickyContainer.StickyContainer ), DataGridTraceArgs.Node( detailNode ), DataGridTraceArgs.Item( item ), DataGridTraceArgs.GeneratorIndex( insertionIndex ), DataGridTraceArgs.Index( detailItemIndex ) );

          m_genPosToIndex.Insert( insertionIndex, detailItemIndex );
          m_genPosToItem.Insert( insertionIndex, item );
          m_genPosToContainer.Insert( insertionIndex, stickyContainer.StickyContainer );
          m_genPosToNode.Insert( insertionIndex, detailNode );
        }

        generatedStickyContainers[ i ] = new StickyContainerGenerated( stickyContainer.StickyContainer, detailItemIndex, stickyContainer.IsNewlyRealized );
      }

      return generatedStickyContainers;
    }

    private List<StickyContainerGenerated> GenerateStickyFootersForNode(
      HeadersFootersGeneratorNode footerNode,
      int footerNodeIndex )
    {
      return this.GenerateStickyFootersForNode( footerNode, footerNodeIndex, -1, false );
    }

    private List<StickyContainerGenerated> GenerateStickyFootersForNode(
      HeadersFootersGeneratorNode footerNode,
      int footerNodeIndex,
      int realizedIndex,
      bool isRealizedIndexPartOfFooterNode )
    {
      var generatedStickyContainers = new List<StickyContainerGenerated>();

      int footersNodeItemCount = footerNode.ItemCount;

      // The container is already part of the footer.
      var generator = ( ICustomItemContainerGenerator )this;
      var position = generator.GeneratorPositionFromIndex( footerNodeIndex + footersNodeItemCount - 1 );

      // In that case, the potential sticky containers are the requested one and bottom.
      using( generator.StartAt( position, GeneratorDirection.Backward, true ) )
      {
        for( int i = footersNodeItemCount - 1; i >= 0; i-- )
        {
          if( ( isRealizedIndexPartOfFooterNode )
              && ( footerNodeIndex + i < realizedIndex ) )
          {
            continue;
          }

          var item = footerNode.GetAt( i );
          var groupHeaderFooterItem = default( GroupHeaderFooterItem? );

          if( item is GroupHeaderFooterItem )
          {
            groupHeaderFooterItem = ( GroupHeaderFooterItem )item;
          }

          if( ( groupHeaderFooterItem != null )
            && ( !groupHeaderFooterItem.Value.Group.IsBottomLevel || ( this.IsGroupExpanded( groupHeaderFooterItem.Value.Group ) != true ) ) )
          {
            this.Skip();
            continue;
          }

          bool isNewlyRealized;
          var stickyContainer = generator.GenerateNext( out isNewlyRealized );

          generatedStickyContainers.Add( new StickyContainerGenerated( stickyContainer, footerNodeIndex + i, isNewlyRealized ) );
        }
      }

      return generatedStickyContainers;
    }

    private HeadersFootersGeneratorNode GetDetailFootersNode( GeneratorNodeHelper nodeHelper )
    {
      var parentNode = default( HeadersFootersGeneratorNode );

      if( nodeHelper.MoveToParent()
        && nodeHelper.MoveToEnd() )
      {
        if( ( nodeHelper.CurrentNode is HeadersFootersGeneratorNode )
          && ( nodeHelper.CurrentNode.Parent == null ) )
        {
          parentNode = nodeHelper.CurrentNode as HeadersFootersGeneratorNode;
        }
        else
        {
          parentNode = this.GetDetailFootersNode( nodeHelper );
        }
      }

      return parentNode;
    }

    #endregion Sticky Footers Methods

    #region Sticky Header Count Methods

    internal int GetStickyHeaderCountForIndex(
     int index,
     bool areHeadersSticky,
     bool areGroupHeadersSticky,
     bool areParentRowsSticky )
    {
      var stickyHeadersCount = 0;
      var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );

      // Unable to find the node, no sticky header for this node
      if( !nodeHelper.FindNodeForIndex( index ) )
        return 0;

      var indexNode = nodeHelper.CurrentNode;
      var itemsGeneratorNode = indexNode as ItemsGeneratorNode;

      // We found an ItemsGeneratorNode
      if( itemsGeneratorNode != null )
      {
        var detailNode = itemsGeneratorNode.GetDetailNodeForIndex( index - nodeHelper.Index );

        // Only do a special case if the index represent
        // a DetailNode
        if( detailNode != null )
        {
          if( ( areParentRowsSticky )
            && ( this.AreDetailsExpanded( detailNode.DetailContext.ParentItem ) ) )
          {
            stickyHeadersCount++;
          }

          var detailFirstRealizedIndex = this.FindGlobalIndexForDetailNode( detailNode );
          var correctedIndex = index - detailFirstRealizedIndex;

          Debug.Assert( correctedIndex >= 0, "correctedIndex >= 0 .. 1" );

          stickyHeadersCount +=
            detailNode.DetailGenerator.GetStickyHeaderCountForIndex( correctedIndex,
                                                                     areHeadersSticky,
                                                                     areGroupHeadersSticky,
                                                                     areParentRowsSticky );
        }
      }

      // We want to find the HeaderFooterGeneratorNode for the container 
      // node. This is to find the headers for the container.
      nodeHelper.MoveToFirst();
      var headersNode = nodeHelper.CurrentNode as HeadersFootersGeneratorNode;

      // There is no headers to generate if the item count of the node is 0.
      if( headersNode.ItemCount > 0 )
      {
        if( ( ( headersNode.Parent == null ) && ( areHeadersSticky ) )
          || ( ( headersNode.Parent is GroupGeneratorNode ) && ( areGroupHeadersSticky ) ) )
        {
          // We are generating sticky headers for a container that is already 
          // part of the headers, we need to handle it differently.
          stickyHeadersCount += this.GetStickyHeaderCountForNode( headersNode,
                                                                  nodeHelper.Index,
                                                                  index,
                                                                  ( headersNode == indexNode ) );
        }
      }

      // We must also find the top most headers for our level of detail and, if they need to be sticky,
      // we will generate the containers and add them the to list.
      var topMostHeaderNode = this.GetTopMostHeaderNode( nodeHelper );
      if( ( topMostHeaderNode != null )
        && ( topMostHeaderNode != headersNode )
        && ( topMostHeaderNode.ItemCount > 0 )
        && ( areHeadersSticky ) )
      {
        stickyHeadersCount += this.GetStickyHeaderCountForNode( topMostHeaderNode, nodeHelper.Index );
      }

      return stickyHeadersCount;
    }

    private int GetStickyHeaderCountForNode( HeadersFootersGeneratorNode headerNode, int headerNodeIndex )
    {
      return this.GetStickyHeaderCountForNode( headerNode, headerNodeIndex, -1, false );
    }

    private int GetStickyHeaderCountForNode(
      HeadersFootersGeneratorNode headerNode,
      int headerNodeIndex,
      int realizedIndex,
      bool isRealizedIndexPartOfHeaderNode )
    {
      var stickyHeaderCount = 0;

      for( int i = 0; i < headerNode.ItemCount; i++ )
      {
        if( ( isRealizedIndexPartOfHeaderNode )
            && ( headerNodeIndex + i >= realizedIndex ) )
          break;

        var item = headerNode.GetAt( i );
        var groupHeaderFooterItem = default( GroupHeaderFooterItem? );

        if( item is GroupHeaderFooterItem )
        {
          groupHeaderFooterItem = ( GroupHeaderFooterItem )item;
        }

        if( ( groupHeaderFooterItem != null )
          && ( !groupHeaderFooterItem.Value.Group.IsBottomLevel || ( this.IsGroupExpanded( groupHeaderFooterItem.Value.Group ) != true ) ) )
          continue;

        stickyHeaderCount++;
      }

      return stickyHeaderCount;
    }

    #endregion

    #region Sticky Footers Count Methods

    internal int GetStickyFooterCountForIndex(
      int index,
      bool areFootersSticky,
      bool areGroupFootersSticky )
    {
      var stickyFooterCount = 0;
      var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );

      // Unable to find the node, no sticky header for this node
      if( !nodeHelper.FindNodeForIndex( index ) )
        return 0;

      var indexNode = nodeHelper.CurrentNode;
      var itemsGeneratorNode = indexNode as ItemsGeneratorNode;

      // We found an ItemsGeneratorNode
      if( itemsGeneratorNode != null )
      {
        var detailNode = itemsGeneratorNode.GetDetailNodeForIndex( index - nodeHelper.Index );
        if( detailNode != null )
        {
          var detailFirstRealizedIndex = this.FindGlobalIndexForDetailNode( detailNode );
          var correctedIndex = index - detailFirstRealizedIndex;

          Debug.Assert( correctedIndex >= 0, "correctedIndex >= 0 .. 2" );

          stickyFooterCount +=
            detailNode.DetailGenerator.GetStickyFooterCountForIndex( correctedIndex,
                                                                     areFootersSticky,
                                                                     areGroupFootersSticky );
        }
      }

      // We want to find the HeaderFooterGeneratorNode for the container 
      // node. This is to find the footers for the container.
      nodeHelper.MoveToEnd();
      var footersNode = nodeHelper.CurrentNode as HeadersFootersGeneratorNode;

      // There is no footers to generate if the item count of the node is 0.
      if( footersNode.ItemCount > 0 )
      {
        if( ( ( footersNode.Parent == null ) && ( areFootersSticky ) )
          || ( ( footersNode.Parent is GroupGeneratorNode ) && ( areGroupFootersSticky ) ) )
        {
          // We are generating sticky footers for a container that is already 
          // part of the footers, we need to handle it differently.
          stickyFooterCount += this.GetStickyFooterCountForNode( footersNode,
                                                                 nodeHelper.Index,
                                                                 index,
                                                                 ( footersNode != indexNode ) );
        }
      }

      // We must also find the bottom most footers for our level of detail and, if they need to be sticky,
      // we will generate the containers and add them the to list.
      var bottomFootersNode = this.GetDetailFootersNode( nodeHelper );

      if( ( bottomFootersNode != null )
        && ( bottomFootersNode != footersNode )
        && ( bottomFootersNode.ItemCount > 0 )
        && ( areFootersSticky ) )
      {
        stickyFooterCount += this.GetStickyFooterCountForNode( bottomFootersNode, nodeHelper.Index );
      }

      return stickyFooterCount;
    }

    private int GetStickyFooterCountForNode(
      HeadersFootersGeneratorNode footerNode,
      int footerNodeIndex )
    {
      return this.GetStickyFooterCountForNode( footerNode, footerNodeIndex, -1, false );
    }

    private int GetStickyFooterCountForNode(
      HeadersFootersGeneratorNode footerNode,
      int footerNodeIndex,
      int realizedIndex,
      bool isRealizedIndexPartOfHeaderNode )
    {
      var stickyFooterCount = 0;
      var footersNodeItemCount = footerNode.ItemCount;

      for( int i = footersNodeItemCount - 1; i >= 0; i-- )
      {
        if( ( isRealizedIndexPartOfHeaderNode )
            && ( footerNodeIndex + i < realizedIndex ) )
          continue;

        var item = footerNode.GetAt( i );
        var groupHeaderFooterItem = default( GroupHeaderFooterItem? );

        if( item is GroupHeaderFooterItem )
        {
          groupHeaderFooterItem = ( GroupHeaderFooterItem )item;
        }

        if( ( groupHeaderFooterItem != null )
          && ( !groupHeaderFooterItem.Value.Group.IsBottomLevel || ( this.IsGroupExpanded( groupHeaderFooterItem.Value.Group ) != true ) ) )
          continue;

        stickyFooterCount++;
      }

      return stickyFooterCount;
    }

    #endregion

    #region IItemContainerGenerator Members

    DependencyObject IItemContainerGenerator.GenerateNext( out bool isNewlyRealized )
    {
      if( this.Status != GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "The Generator is not active: StartAt() was not called prior calling GenerateNext() or the returned IDisposable was already disposed of.", m_dataGridControl );

      isNewlyRealized = false;

      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_IItemContainerGenerator_GenerateNext ) )
      {
        if( m_generatorNodeHelper == null ) //the m_generatorNodeHelper will be turned to null when we reach the end of the list ot items.
          return null;

        var node = m_generatorNodeHelper.CurrentNode;
        if( node == null )
          throw DataGridException.Create<DataGridInternalException>( "CurrentNode is null.", m_dataGridControl );

        if( !( node is CollectionGeneratorNode ) )
          throw DataGridException.Create<DataGridInternalException>( "CurrentNode is not a CollectionGeneratorNode.", m_dataGridControl );

        if( node is GroupGeneratorNode )
        {
          this.TraceEvent( TraceEventType.Error, DataGridTraceEventId.CustomItemContainerGenerator_IItemContainerGenerator_GenerateNext, DataGridTraceMessages.UnexpectedNode, DataGridTraceArgs.Node( node ) );
        }

        var container = default( DependencyObject );

        //if a detail generator is currently "started", then rely on it for the generation of items
        if( m_generatorCurrentDetail != null )
        {
          //if the detail generator was not yet started
          if( m_generatorCurrentDetailDisposable == null )
          {
            //start it
            m_generatorCurrentDetailDisposable = ( ( IItemContainerGenerator )m_generatorCurrentDetail.DetailGenerator ).StartAt( m_generatorCurrentDetail.DetailGenerator.GeneratorPositionFromIndex( m_generatorCurrentDetailIndex ), m_generatorDirection, true );
          }

          container = ( ( IItemContainerGenerator )m_generatorCurrentDetail.DetailGenerator ).GenerateNext( out isNewlyRealized );
          node = m_generatorCurrentDetail;
          //Detail Generator will have taken care of the "nasty" stuff ( ItemIndex, ... )
        }
        else
        {
          //otherwise, it means the item to be generated is within this generator.
          container = this.GenerateNextLocalContainer( out isNewlyRealized );

          //special case for table view grid lines
          Xceed.Wpf.DataGrid.Views.ViewBase.SetIsLastItem( container, this.ShouldDrawBottomLine() );
          DataGridControl.SetHasExpandedDetails( container, this.ItemHasExpandedDetails() );
        }

        //if the container was just realized, ensure to add it to the lists maintaining the generated items.
        if( isNewlyRealized )
        {
          var insertionIndex = this.FindInsertionPoint( m_generatorCurrentGlobalIndex );
          var item = CustomItemContainerGenerator.GetDataItemProperty( container ).Data;

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_IItemContainerGenerator_GenerateNext, DataGridTraceMessages.ContainerAdded, DataGridTraceArgs.Container( container ), DataGridTraceArgs.Node( node ), DataGridTraceArgs.Item( item ), DataGridTraceArgs.GeneratorIndex( insertionIndex ), DataGridTraceArgs.Index( m_generatorCurrentGlobalIndex ) );

          if( insertionIndex > 0 )
          {
            if( m_generatorCurrentGlobalIndex <= m_genPosToIndex[ insertionIndex - 1 ] )
              throw DataGridException.Create<DataGridInternalException>( "Realized item inserted at wrong location.", m_dataGridControl );
          }
          else if( m_genPosToIndex.Count > 0 )
          {
            if( m_generatorCurrentGlobalIndex >= m_genPosToIndex[ insertionIndex ] )
              throw DataGridException.Create<DataGridInternalException>( "Realized item inserted at wrong location.", m_dataGridControl );
          }

          m_genPosToIndex.Insert( insertionIndex, m_generatorCurrentGlobalIndex );
          m_genPosToItem.Insert( insertionIndex, item );
          m_genPosToContainer.Insert( insertionIndex, container );
          m_genPosToNode.Insert( insertionIndex, node );
        }

        if( m_generatorDirection == GeneratorDirection.Forward )
        {
          this.MoveGeneratorForward();
        }
        else
        {
          this.MoveGeneratorBackward();
        }

        return container;
      }
    }

    DependencyObject IItemContainerGenerator.GenerateNext()
    {
      bool flag;

      return ( ( IItemContainerGenerator )this ).GenerateNext( out flag );
    }

    public GeneratorPosition GeneratorPositionFromIndex( int itemIndex )
    {
      this.EnsureNodeTreeCreated();

      var genPosIndex = m_genPosToIndex.IndexOf( itemIndex );
      if( genPosIndex >= 0 )
        return new GeneratorPosition( genPosIndex, 0 );

      var storedIndex = -1;
      var offset = itemIndex + 1;

      //Find the closest (lower) realized index
      for( int i = 0; i < m_genPosToIndex.Count; i++ )
      {
        storedIndex = i;
        if( m_genPosToIndex[ i ] > itemIndex )
        {
          storedIndex = i - 1;
          break;
        }
      }

      //and from there, compute the offset.
      if( storedIndex >= 0 )
      {
        offset = itemIndex - m_genPosToIndex[ storedIndex ];
      }

      return new GeneratorPosition( storedIndex, offset );
    }

    ItemContainerGenerator IItemContainerGenerator.GetItemContainerGeneratorForPanel( Panel panel )
    {
      return ( ( IItemContainerGenerator )( ( ItemsControl )m_dataGridControl ).ItemContainerGenerator ).GetItemContainerGeneratorForPanel( panel );
    }

    public int IndexFromGeneratorPosition( GeneratorPosition position )
    {
      this.EnsureNodeTreeCreated();

      int retval = -1;

      if( ( position.Index >= 0 ) && ( position.Index < m_genPosToIndex.Count ) )
      {
        //calculate based on the generator position passed.
        retval = m_genPosToIndex[ position.Index ] + position.Offset;
      }
      else if( position.Index == -1 )
      {
        // if the Index is -1, then we ask for a non realized item, at the beginning of the range, so
        // first step, ensure the GeneratorPosition asked is valid (offset non-Zero)
        if( position.Offset > 0 )
        {
          //then return index based on the GenPos.
          retval = position.Offset - 1;
        }
      }
      //if not above 0, and not -1, then return an error (-1)...

      return retval;
    }

    void IItemContainerGenerator.PrepareItemContainer( DependencyObject container )
    {
      if( this.Status != GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "The Generator is not active: StartAt() was not called prior calling PrepareItemContainer() or the returned IDisposable was already disposed of.", m_dataGridControl );

      var dataItemStore = CustomItemContainerGenerator.GetDataItemProperty( container );
      if( ( dataItemStore == null ) || dataItemStore.IsEmpty )
        return;

      var dataItem = dataItemStore.Data;
      if( dataItem == null )
        return;

      m_dataGridControl.PrepareItemContainer( container, dataItem );
    }

    void IItemContainerGenerator.Remove( GeneratorPosition position, int count )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "Cannot perform this operation while the generator is busy generating items", m_dataGridControl );

      if( position.Offset != 0 )
        throw DataGridException.Create<ArgumentException>( "The GeneratorPosition to remove cannot map to a non-realized item.", m_dataGridControl, "position" );

      if( position.Index == -1 )
        throw DataGridException.Create<ArgumentException>( "The GeneratorPosition to remove cannot map to a non-realized item.", m_dataGridControl, "position" );

      if( count <= 0 )
        throw DataGridException.Create<ArgumentException>( "The number of item to remove must be greater than or equal to one.", m_dataGridControl, "count" );

      if( position.Index >= m_genPosToIndex.Count )
        throw DataGridException.Create<DataGridInternalException>( "Trying to remove an item at an out-of-bound GeneratorPosition index", m_dataGridControl );

      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_IItemContainerGenerator_Remove, DataGridTraceArgs.Index( position.Index ), DataGridTraceArgs.Count( count ) ) )
      {
        for( int i = 0; i < count; i++ )
        {
          if( this.RemoveGeneratedItem( position.Index, null ) == 0 )
            //This case deserves a more solid approach... We do not want to allow removal from the user panel of an item that somehow is not present.
            throw DataGridException.Create<DataGridInternalException>( "Trying to remove an item at an out-of-bound GeneratorPosition index.", m_dataGridControl );
        }
      }
    }

    void IItemContainerGenerator.RemoveAll()
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "Cannot perform this operation while the generator is busy generating items", m_dataGridControl );

      using( this.SetIsHandlingGlobalItemsResetLocally() )
      {
        //Call to remove all shall not request container recycling panels to remove their containers. Therefore, I am not collecting the remove containers.
        this.RemoveAllGeneratedItems();
      }
    }

    IDisposable IItemContainerGenerator.StartAt( GeneratorPosition position, GeneratorDirection direction, bool allowStartAtRealizedItem )
    {
      this.SetIsInUse();

      if( this.Status == GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "Cannot perform this operation while the generator is busy generating items", m_dataGridControl );

      this.EnsureNodeTreeCreated();

      if( ( !allowStartAtRealizedItem ) && ( position.Offset == 0 ) )
      {
        if( direction == GeneratorDirection.Forward )
        {
          position = this.FindNextUnrealizedGeneratorPosition( position );
        }
        else
        {
          position = this.FindPreviousUnrealizedGeneratorPosition( position );
        }
      }

      return new CustomItemContainerGeneratorDisposableDisposer( this, position, direction );
    }

    IDisposable IItemContainerGenerator.StartAt( GeneratorPosition position, GeneratorDirection direction )
    {
      return ( ( IItemContainerGenerator )this ).StartAt( position, direction, false );
    }

    #endregion

    #region ICustomItemContainerGenerator Members

    public int ItemCount
    {
      get
      {
        this.EnsureNodeTreeCreated();

        //if the value that was last cached is invalid
        if( m_lastValidItemCountGeneration != m_currentGeneratorContentGeneration )
        {
          //then re-evaluate the number of items.
          if( m_startNode != null )
          {
            // If it does that correctly, then this function is OK.
            int chainLength;
            GeneratorNodeHelper.EvaluateChain( m_startNode, out m_cachedItemCount, out chainLength );
          }
          else
          {
            m_cachedItemCount = 0;
          }

          m_lastValidItemCountGeneration = m_currentGeneratorContentGeneration;
        }

        //return the cached value.
        return m_cachedItemCount;
      }
    }

    public bool IsRecyclingEnabled
    {
      get
      {
        return m_flags[ ( int )CustomItemContainerGeneratorFlags.RecyclingEnabled ];
      }
      set
      {
        //if the container recycling is turned OFF from ON
        if( value == ( bool )m_flags[ ( int )CustomItemContainerGeneratorFlags.RecyclingEnabled ] )
          return;

        if( !value )
        {
          this.ClearRecyclingPools();
        }

        m_flags[ ( int )CustomItemContainerGeneratorFlags.RecyclingEnabled ] = value;

        foreach( var generator in this.GetDetailGenerators() )
        {
          generator.IsRecyclingEnabled = value;
        }
      }
    }

    private bool GenPosToIndexNeedsUpdate
    {
      get
      {
        return m_flags[ ( int )CustomItemContainerGeneratorFlags.GenPosToIndexNeedsUpdate ];
      }
      set
      {
        m_flags[ ( int )CustomItemContainerGeneratorFlags.GenPosToIndexNeedsUpdate ] = value;
      }
    }

    void ICustomItemContainerGenerator.SetCurrentIndex( int newCurrentIndex )
    {
      // This method enables the possibility for the "ItemsHost" panels to set the current item of the 
      // DataGridControl. This is required for scenarios such as the CardflowItemsHost.
      this.EnsureNodeTreeCreated();

      var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );

      //First, locate the item within the Generator.
      object newCurrentItem = nodeHelper.FindIndex( newCurrentIndex );

      if( newCurrentItem == null )
        throw DataGridException.Create<InvalidOperationException>( "An attempt was made to access an item at an index that does not correspond to an item.", m_dataGridControl );

      //Then, if the item is within an ItemsNode, check if it belongs to a detail
      var itemsNode = nodeHelper.CurrentNode as ItemsGeneratorNode;
      if( itemsNode != null )
      {
        int masterIndex;
        int detailIndex;
        int detailNodeIndex;

        var detailNode = itemsNode.GetDetailNodeForIndex( newCurrentIndex - nodeHelper.Index, out masterIndex, out detailIndex, out detailNodeIndex );
        if( detailNode != null )
        {
          //call recursively the SetCurrentIndex method on the detail generator to ensure that if the item
          //belongs to a sub-generator of the detail generator, then the appropriate DataGridContext will get invoked.
          ICustomItemContainerGenerator detailGenerator = detailNode.DetailGenerator;
          detailGenerator.SetCurrentIndex( detailIndex );
          return;
        }
      }

      //If the item is not within an ItemsNode or not within a detail
      //then set it current within its context.
      m_dataGridContext.SetCurrentItemCore( newCurrentItem, false, false, AutoScrollCurrentItemSourceTriggers.Navigation );
    }

    int ICustomItemContainerGenerator.GetCurrentIndex()
    {
      if( m_dataGridControl.CurrentContext != null )
        return this.FindIndexForItem( m_dataGridControl.CurrentContext.InternalCurrentItem, m_dataGridContext );

      return -1;
    }

    void ICustomItemContainerGenerator.RestoreFocus( DependencyObject container )
    {
      if( !m_genPosToContainer.Contains( container ) )
        throw DataGridException.Create<ArgumentException>( "The specified container is not part of the generator's content.", m_dataGridControl, "container" );

      var dataGridContext = DataGridControl.GetDataGridContext( container );
      var column = ( dataGridContext == null ) ? null : dataGridContext.CurrentColumn;

      m_dataGridControl.SetFocusHelper( container as UIElement, column, false, true );
    }

    #endregion

    private DependencyObject GenerateNextLocalContainer( out bool isNewlyRealized )
    {
      var container = default( DependencyObject );
      var node = m_generatorNodeHelper.CurrentNode as CollectionGeneratorNode;

      //if the index exists in the list, then the item is already realized.
      var genPosIndex = m_genPosToIndex.IndexOf( m_generatorCurrentGlobalIndex );
      var itemsNode = node as ItemsGeneratorNode;

      if( node == null )
      {
        this.TraceEvent( TraceEventType.Error, DataGridTraceEventId.CustomItemContainerGenerator_GenerateNextLocalContainer, DataGridTraceMessages.UnexpectedNode, DataGridTraceArgs.Node( node ), DataGridTraceArgs.GeneratorIndex( genPosIndex ) );
      }

      object dataItem;

      if( genPosIndex >= 0 )
      {
        //retrieve the container for the item that is already stored in the data structure
        container = m_genPosToContainer[ genPosIndex ];
        dataItem = m_genPosToItem[ genPosIndex ];

        var tempNode = m_genPosToNode[ genPosIndex ];
        node = tempNode as CollectionGeneratorNode;

        if( tempNode == null )
        {
          this.TraceEvent( TraceEventType.Critical, DataGridTraceEventId.CustomItemContainerGenerator_GenerateNextLocalContainer, DataGridTraceMessages.UnexpectedNode, DataGridTraceArgs.Node( node ), DataGridTraceArgs.GeneratorIndex( genPosIndex ), DataGridTraceArgs.Container( container ), DataGridTraceArgs.Item( dataItem ) );
          throw DataGridException.Create<DataGridInternalException>( "CustomItemContainerGenerator.GenerateNextLocalContainer: cached Node is null", m_dataGridControl );
        }
        else if( node != m_generatorNodeHelper.CurrentNode )
        {
          this.TraceEvent( TraceEventType.Critical, DataGridTraceEventId.CustomItemContainerGenerator_GenerateNextLocalContainer, DataGridTraceMessages.NodeIsNotTheCurrentNode, DataGridTraceArgs.Node( tempNode ), DataGridTraceArgs.Node( m_generatorNodeHelper.CurrentNode ), DataGridTraceArgs.GeneratorIndex( genPosIndex ), DataGridTraceArgs.Container( container ), DataGridTraceArgs.Item( dataItem ) );
          throw DataGridException.Create<DataGridInternalException>( "CustomItemContainerGenerator.GenerateNextLocalContainer: Node is not the current one.", m_dataGridControl );
        }

        isNewlyRealized = false;
      }
      else
      {
        //This means that the container is not already generated for the item...

        //First need to fetch the dataItem
        if( itemsNode != null )
        {
          dataItem = itemsNode.Items[ m_generatorCurrentOffset ]; //here I use this particular exception to prevent over-complicating the algo
          //I know it's rather dirty but!!! (i.e. calling GetAt() would return something with details computed in ).

          this.UpdateDataVirtualizationLockForItemsNode( itemsNode, dataItem, true );
        }
        else
        {
          dataItem = node.GetAt( m_generatorCurrentOffset );
          //Here I Need to call the GetAt() to make sure the collapsed GroupHeaders/Footers are correctly generated...
        }

        //create the container for the item specified according to the item's type
        container = this.CreateContainerForItem( dataItem, node );

        //flag the container as newly generated.
        isNewlyRealized = true;
      }

      if( itemsNode != null )
      {
        int itemIndex = m_generatorNodeHelper.SourceDataIndex + m_generatorCurrentOffset;

        DataGridVirtualizingPanel.SetItemIndex( container, itemIndex );

        //In order to make sure the container is properly selected, a new selection range must be initialized with the updated item index.
        var currentSelection = new SelectionRange( itemIndex );
        var rowToSelect = container as DataRow;

        if( rowToSelect != null )
        {
          if( m_dataGridContext.SelectedItemsStore.Contains( currentSelection ) )
          {
            rowToSelect.SetIsSelected( true );
          }
          else
          {
            rowToSelect.SetIsSelected( false );
          }
        }
      }
      else
      {
        DataGridVirtualizingPanel.SetItemIndex( container, -1 );
      }

      if( node != null )
      {
        //determine the appropriate GroupConfiguration ( based on parent node ) and set it on the container.
        var groupConfig = default( GroupConfiguration );
        var parentNode = node.Parent as GroupGeneratorNode; //implicit rule: a parent node is always a GroupGeneratorNode

        if( parentNode != null )
        {
          groupConfig = parentNode.GroupConfiguration;
        }

        if( groupConfig != null )
        {
          DataGridControl.SetContainerGroupConfiguration( container, groupConfig );
        }
      }

      this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GenerateNextLocalContainer, DataGridTraceMessages.ContainerGenerated, DataGridTraceArgs.Container( container ), DataGridTraceArgs.Node( node ), DataGridTraceArgs.GeneratorIndex( genPosIndex ), DataGridTraceArgs.Value( isNewlyRealized ) );

      return container;
    }

    private bool IsDataVirtualized( ItemsGeneratorNode node )
    {
      DataGridVirtualizingCollectionViewBase collectionView;
      VirtualList items;

      return this.TryGetVirtualizedCollection( node, out collectionView, out items );
    }

    private bool TryGetVirtualizedCollection( ItemsGeneratorNode node, out DataGridVirtualizingCollectionViewBase collectionView, out VirtualList items )
    {
      if( node == null )
      {
        collectionView = default( DataGridVirtualizingCollectionViewBase );
        items = default( VirtualList );
      }
      else
      {
        var itemCollection = node.Items as ItemCollection;
        if( itemCollection != null )
        {
          collectionView = itemCollection.SourceCollection as DataGridVirtualizingCollectionViewBase;
          items = default( VirtualList );
        }
        else
        {
          collectionView = default( DataGridVirtualizingCollectionViewBase );
          items = node.Items as VirtualList;
        }
      }

      return ( collectionView != null )
          || ( items != null );
    }

    private void UpdateDataVirtualizationLockForItemsNode( ItemsGeneratorNode node, object dataItem, bool applyLock )
    {
      DataGridVirtualizingCollectionViewBase collectionView;
      VirtualList items;

      if( !this.TryGetVirtualizedCollection( node, out collectionView, out items ) )
        return;

      if( collectionView != null )
      {
        var index = node.Items.IndexOf( dataItem );
        if( applyLock )
        {
          collectionView.RootGroup.LockGlobalIndex( index );
        }
        else
        {
          collectionView.RootGroup.UnlockGlobalIndex( index );
        }
      }
      else if( items != null )
      {
        var index = node.Items.IndexOf( dataItem );
        if( applyLock )
        {
          items.LockPageForLocalIndex( index );
        }
        else
        {
          items.UnlockPageForLocalIndex( index );
        }
      }
    }

    private void MoveGeneratorForward()
    {
      var currentMasterNode = m_generatorNodeHelper.CurrentNode as ItemsGeneratorNode;

      //if I was generating from a detail generator
      if( m_generatorCurrentDetail != null )
      {
        if( ( currentMasterNode == null ) || ( currentMasterNode.Details == null ) )
        {
          this.TraceEvent( TraceEventType.Critical, DataGridTraceEventId.CustomItemContainerGenerator_MoveGeneratorForward, DataGridTraceMessages.UnexpectedNode, DataGridTraceArgs.Node( currentMasterNode ) );
        }

        //incremment the running counter for the item index generated in the detail generator
        m_generatorCurrentDetailIndex++;

        //if that was the last item from the detail generator
        if( m_generatorCurrentDetailIndex >= m_generatorCurrentDetail.ItemCount )
        {
          //stop the detail generator
          m_generatorCurrentDetailDisposable.Dispose();
          m_generatorCurrentDetailDisposable = null;
          m_generatorCurrentDetailIndex = -1;

          //increment detail node index
          m_generatorCurrentDetailNodeIndex++;

          List<DetailGeneratorNode> detailsforMasterNode;
          currentMasterNode.Details.TryGetValue( m_generatorCurrentOffset, out detailsforMasterNode );

          if( detailsforMasterNode == null )
          {
            this.TraceEvent( TraceEventType.Critical, DataGridTraceEventId.CustomItemContainerGenerator_MoveGeneratorForward, DataGridTraceMessages.DetailExpected, DataGridTraceArgs.Node( currentMasterNode ) );
          }

          while( ( m_generatorCurrentDetailNodeIndex < detailsforMasterNode.Count ) && ( m_generatorCurrentDetailIndex == -1 ) )
          {
            //try to use it
            m_generatorCurrentDetail = detailsforMasterNode[ m_generatorCurrentDetailNodeIndex ];

            if( m_generatorCurrentDetail.ItemCount > 0 )
            {
              m_generatorCurrentDetailIndex = 0;
            }
            else
            {
              //increment detail node index
              m_generatorCurrentDetailNodeIndex++;
            }
          }

          if( m_generatorCurrentDetailIndex == -1 )
          {
            //if there are no other details for this master item, then move the master item counter forward
            m_generatorCurrentDetail = null;
            m_generatorCurrentOffset++;
          }
        }
      }
      else
      {
        //If I was not generating from a detail generator

        //Check if there is availlable details for the current item
        List<DetailGeneratorNode> detailsForNode;

        if( ( currentMasterNode != null ) && ( currentMasterNode.Details != null )
          && ( currentMasterNode.Details.TryGetValue( m_generatorCurrentOffset, out detailsForNode ) ) )
        {
          var foundDetail = false;

          for( int i = 0; i < detailsForNode.Count; i++ )
          {
            var detailNode = detailsForNode[ i ];

            if( detailNode.ItemCount > 0 )
            {
              //There are details for the master item
              m_generatorCurrentDetailIndex = 0;
              m_generatorCurrentDetailNodeIndex = i;
              m_generatorCurrentDetail = detailNode;
              foundDetail = true;
              break;
            }
          }

          if( !foundDetail )
          {
            //there are no items in the details
            m_generatorCurrentOffset++; //move to the next index in the local generator...
          }
        }
        else
        {
          //there are no details OR the node is not an ItemsGeneratorNode (cannot have details)
          m_generatorCurrentOffset++; //move to the next index...
        }
      }

      //if the current offset is after the last "item" of the node (can be more than 1 item, for multi item nodes)
      if( m_generatorCurrentOffset >= ( ( currentMasterNode != null ) ? currentMasterNode.Items.Count : m_generatorNodeHelper.CurrentNode.ItemCount ) )
      {
        //if we are at the last (and possible only) item in the node, move to the next node.
        if( !m_generatorNodeHelper.MoveForward() )
        {
          //if we failed to move the the next node, then we want to make sure the generator will not be able to generate
          //any more containers.
          m_generatorNodeHelper = null;
        }

        //reset the offset, since we moved the the node helper.
        m_generatorCurrentOffset = 0;
      }

      m_generatorCurrentGlobalIndex++;
    }

    private void MoveGeneratorBackward()
    {
      var currentMasterNode = m_generatorNodeHelper.CurrentNode as ItemsGeneratorNode;

      //if I was generating from a detail generator
      if( m_generatorCurrentDetail != null )
      {
        if( ( currentMasterNode == null ) || ( currentMasterNode.Details == null ) )
        {
          this.TraceEvent( TraceEventType.Critical, DataGridTraceEventId.CustomItemContainerGenerator_MoveGeneratorBackward, DataGridTraceMessages.UnexpectedNode, DataGridTraceArgs.Node( currentMasterNode ) );
        }

        //decrement the running counter for the item index generated in the detail generator
        m_generatorCurrentDetailIndex--;

        //if that was the first item from the detail generator
        if( m_generatorCurrentDetailIndex < 0 )
        {
          //stop the detail generator
          m_generatorCurrentDetailDisposable.Dispose();
          m_generatorCurrentDetailDisposable = null;
          m_generatorCurrentDetailIndex = -1;

          //decrement detail node index
          m_generatorCurrentDetailNodeIndex--;

          if( m_generatorCurrentDetailNodeIndex >= 0 )
          {
            List<DetailGeneratorNode> detailsforMasterNode;
            currentMasterNode.Details.TryGetValue( m_generatorCurrentOffset, out detailsforMasterNode );

            if( detailsforMasterNode == null )
            {
              this.TraceEvent( TraceEventType.Critical, DataGridTraceEventId.CustomItemContainerGenerator_MoveGeneratorBackward, DataGridTraceMessages.DetailExpected, DataGridTraceArgs.Node( currentMasterNode ) );
            }

            while( ( m_generatorCurrentDetailNodeIndex >= 0 ) && ( m_generatorCurrentDetailIndex == -1 ) )
            {
              //try to use it
              m_generatorCurrentDetail = detailsforMasterNode[ m_generatorCurrentDetailNodeIndex ];

              if( m_generatorCurrentDetail.ItemCount > 0 )
              {
                m_generatorCurrentDetailIndex = m_generatorCurrentDetail.ItemCount - 1;
              }
              else
              {
                //decrement detail node index
                m_generatorCurrentDetailNodeIndex--;
              }
            }
          }

          if( m_generatorCurrentDetailIndex == -1 )
          {
            //if there are no other details for this master item, then 
            //don't do anything, the current m_generatorCurrentOffset will
            //be generated
            m_generatorCurrentDetail = null;
          }
        }
      }
      else
      {
        //If I was not generating from a detail generator

        //move the current offset to the previous element
        m_generatorCurrentOffset--;

        //do not try open details if the offset falls below 0
        if( m_generatorCurrentOffset >= 0 )
        {
          //Check if there is availlable details for the current item
          this.SetupLastDetailForNode( currentMasterNode );
        }
      }

      //if the current offset is below the last "item" of the node (can be more than 1 item, for multi item nodes)
      if( m_generatorCurrentOffset < 0 )
      {
        //if we are at the last (and possible only) item in the node, move to the next node.
        if( !m_generatorNodeHelper.MoveBackward() )
        {
          //if we failed to move the the next node, then we want to make sure the generator will not be able to generate
          //any more containers.
          m_generatorNodeHelper = null;
        }
        else
        {
          currentMasterNode = m_generatorNodeHelper.CurrentNode as ItemsGeneratorNode;

          //reset the offset, since we moved the the node helper.
          m_generatorCurrentOffset = ( ( currentMasterNode != null ) ? currentMasterNode.Items.Count : m_generatorNodeHelper.CurrentNode.ItemCount ) - 1;

          this.SetupLastDetailForNode( currentMasterNode );
        }
      }

      m_generatorCurrentGlobalIndex--;
    }

    private void SetupLastDetailForNode( ItemsGeneratorNode node )
    {
      List<DetailGeneratorNode> detailsForNode;

      if( ( node != null ) && ( node.Details != null ) && ( node.Details.TryGetValue( m_generatorCurrentOffset, out detailsForNode ) ) )
      {
        for( int i = detailsForNode.Count - 1; i >= 0; i-- )
        {
          var detailNode = detailsForNode[ i ];
          if( detailNode.ItemCount > 0 )
          {
            //There are details for the master item
            m_generatorCurrentDetailNodeIndex = i;
            m_generatorCurrentDetail = detailNode;
            m_generatorCurrentDetailIndex = detailNode.ItemCount - 1;
            break;
          }
        }
      }
    }

    private bool ShouldDrawBottomLine()
    {
      if( m_generatorStatus != GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "An attempt was made to call the CustomItemContainerGenerator.ShouldDrawBottomLine method while containers are not being generated.", m_dataGridControl );

      return ( m_generatorCurrentGlobalIndex == ( this.ItemCount - 1 ) );
    }

    private bool ItemHasExpandedDetails()
    {
      if( m_generatorStatus != GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "An attempt was made to call the CustomItemContainerGenerator.ItemHasExpandedDetails method while containers are not being generated.", m_dataGridControl );

      var itemsNode = m_generatorNodeHelper.CurrentNode as ItemsGeneratorNode;
      if( ( itemsNode == null ) || ( itemsNode.Details == null ) )
        return false;

      return itemsNode.Details.ContainsKey( m_generatorCurrentOffset );
    }

    internal void CleanupGenerator()
    {
      this.CleanupGenerator( false );
    }

    internal void CleanupGenerator( bool isNestedCall )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_CleanupGenerator ) )
      {
        this.ForceReset = false;

        if( this.IsHandlingGlobalItemsResetLocally )
        {
          this.TraceEvent( TraceEventType.Warning, DataGridTraceEventId.CustomItemContainerGenerator_CleanupGenerator, DataGridTraceMessages.CannotProcessOnReset );
          return;
        }

        using( this.SetIsHandlingGlobalItemsResetLocally() )
        {
          this.RemoveAllGeneratedItems();

          if( !isNestedCall )
          {
            this.SendResetEvent();
            this.ClearRecyclingPools();
          }

          this.ClearLateGroupLevelDescriptions();

          if( m_startNode != null )
          {
            //Note: this does not disconnects the "general" master/detail relationship established between an Item and its details,
            //within the Generator itself, however, it does break the link between the GeneratorNode where the details are mapped.
            this.NodeFactory.CleanGeneratorNodeTree( m_startNode );
          }

          m_groupNodeMappingCache.Clear();
          m_firstHeader = null;
          m_firstFooter = null;
          m_firstItem = null;
          m_startNode = null;

          //This absolutelly needs to be done after the node list is Cleaned!
          this.CleanupDetailRelations();
          this.InvalidateNodeTree();
        }
      }
    }

    private void CleanupDetailRelations()
    {
      var detailNodes = this.GetDetailGeneratorNodes().ToList();

      m_masterToDetails.Clear();

      foreach( var detailNode in detailNodes )
      {
        this.ClearDetailGeneratorNode( detailNode );
      }

      m_floatingDetails.Clear();
    }

    private void IncrementCurrentGenerationCount( bool updateGenPosList )
    {
      unchecked
      {
        m_currentGeneratorContentGeneration++;
      }

      if( updateGenPosList )
      {
        if( m_genPosToIndexUpdateInhibitCount == 0 )
        {
          this.UpdateGenPosToIndexList();
          this.GenPosToIndexNeedsUpdate = false;
        }
        else
        {
          this.GenPosToIndexNeedsUpdate = true;
        }
      }
    }

    private void IncrementCurrentGenerationCount()
    {
      this.IncrementCurrentGenerationCount( true );
    }

    private void OnItemsSourceChanged( object sender, EventArgs e )
    {
      // Avoid re-entrance when processing a global reset
      if( this.IsHandlingGlobalItemsReset )
        return;

      this.CleanupGenerator();
    }

    private void OnDetailConfigurationsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_OnDetailConfigurationsChanged ) )
      {
        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_OnDetailConfigurationsChanged, DataGridTraceArgs.Action( e.Action ) );

        switch( e.Action )
        {
          case NotifyCollectionChangedAction.Remove:
            {
              using( m_recyclingPools.DeferContainersRemoved() )
              {
                foreach( DetailConfiguration detailConfiguration in e.OldItems )
                {
                  this.CloseDetails( detailConfiguration );

                  m_recyclingPools.Clear( detailConfiguration );
                }
              }
              break;
            }

          case NotifyCollectionChangedAction.Move:
          case NotifyCollectionChangedAction.Add:
          case NotifyCollectionChangedAction.Replace:
          case NotifyCollectionChangedAction.Reset:
          default:
            {
              // That ensure the RemapFloatingDetails() got called and the loop on m_masterToDetails.Keys will not contain invalid item.
              this.EnsureNodeTreeCreated();

              foreach( object item in m_masterToDetails.Keys.ToList() )
              {
                this.CloseDetailsForItem( item, null );
              }
              break;
            }
        }
      }
    }

    private void OnCollectionViewPropertyChanged( PropertyChangedEventArgs e )
    {
      var propertyName = e.PropertyName;

      if( string.IsNullOrEmpty( propertyName ) || ( propertyName == DataGridCollectionViewBase.GroupsPropertyName ) )
      {
        this.OnCollectionViewGroupsPropertyChanged();
      }

      if( string.IsNullOrEmpty( propertyName ) || ( propertyName == DataGridCollectionViewBase.RootGroupPropertyName ) )
      {
        this.OnCollectionViewRootGroupChanged();
      }
    }

    private void OnCollectionViewGroupsPropertyChanged()
    {
      if( m_groupsCollection == m_collectionView.Groups )
        return;

      this.HandleGlobalItemsReset();
    }

    private void OnCollectionViewRootGroupChanged()
    {
      for( int i = 0; i < m_genPosToNode.Count; i++ )
      {
        if( !( m_genPosToNode[ i ] is HeadersFootersGeneratorNode ) )
          continue;

        this.SetStatContext( m_genPosToContainer[ i ], m_genPosToNode[ i ] );
      }
    }

    private void OnItemsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_OnItemsChanged ) )
      {
        if( this.IsHandlingGlobalItemsResetLocally )
        {
          this.TraceEvent( TraceEventType.Warning, DataGridTraceEventId.CustomItemContainerGenerator_OnItemsChanged, DataGridTraceMessages.CannotProcessOnReset, DataGridTraceArgs.Action( e.Action ) );
          return;
        }

        if( this.Status == GeneratorStatus.GeneratingContainers )
          throw DataGridException.Create<InvalidOperationException>( "Cannot perform this operation while the generator is busy generating items.", m_dataGridControl );

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_OnItemsChanged, DataGridTraceArgs.Action( e.Action ) );

        if( this.ForceReset )
        {
          this.HandleGlobalItemsReset();
        }
        else
        {
          if( e.Action == NotifyCollectionChangedAction.Reset )
          {
            if( ( m_groupsCollection == null ) != ( m_collectionView.GroupDescriptions.Count == 0 ) )
            {
              this.HandleGlobalItemsReset();
            }
          }
        }
      }
    }

    private void OnGroupsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_OnGroupsChanged, DataGridTraceArgs.Group( sender ) ) )
      {
        if( this.IsHandlingGlobalItemsResetLocally )
        {
          this.TraceEvent( TraceEventType.Warning, DataGridTraceEventId.CustomItemContainerGenerator_OnGroupsChanged, DataGridTraceMessages.CannotProcessOnReset, DataGridTraceArgs.Group( sender ), DataGridTraceArgs.Action( e.Action ) );
          return;
        }

        if( this.Status == GeneratorStatus.GeneratingContainers )
          throw DataGridException.Create<InvalidOperationException>( "Cannot perform this operation while the generator is busy generating items.", m_dataGridControl );

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_OnGroupsChanged, DataGridTraceArgs.Group( sender ), DataGridTraceArgs.Action( e.Action ) );

        switch( e.Action )
        {
          case NotifyCollectionChangedAction.Add:
            {
              var count = e.NewItems.Count;

              //if the first item is empty, do not do anything, the structure will be generated when the generator is started!
              if( m_firstItem != null )
              {
                //The only moment where the m_firstItem is null is typically when a reset occured...
                //other moments is when there are 0 items.
                this.HandleSameLevelGroupAddition( m_firstItem, out count, e );
                this.IncrementCurrentGenerationCount();
              }

              this.SendAddEvent( count );
            }
            break;

          case NotifyCollectionChangedAction.Move:
            {
              if( m_firstItem != null )
              {
                if( !( m_firstItem is GroupGeneratorNode ) )
                  throw DataGridException.Create<DataGridInternalException>( "Trying to move a GeneratorNode that is not a GroupGeneratorNode.", m_dataGridControl );

                Debug.Assert( e.OldStartingIndex != e.NewStartingIndex, "An attempt was made to move a group to the same location." );

                this.HandleSameLevelGroupMove( m_firstItem, e );
              }
            }
            break;

          case NotifyCollectionChangedAction.Remove:
            {
              if( m_firstItem != null )
              {
                if( !( m_firstItem is GroupGeneratorNode ) )
                  throw DataGridException.Create<DataGridInternalException>( "Trying to remove a GeneratorNode that is not a GroupGeneratorNode.", m_dataGridControl );

                var count = default( int );
                var containers = new List<DependencyObject>();

                this.HandleSameLevelGroupRemove( m_firstItem, out count, e, containers );
                this.IncrementCurrentGenerationCount();
                this.SendRemoveEvent( count, containers );
              }
            }
            break;

          case NotifyCollectionChangedAction.Replace:
            throw DataGridException.Create<NotSupportedException>( "Replace not supported at the moment on groups.", m_dataGridControl );

          case NotifyCollectionChangedAction.Reset:
            {
              if( m_firstItem != null )
              {
                if( !( m_firstItem is GroupGeneratorNode ) )
                  throw DataGridException.Create<DataGridInternalException>( "Trying to reset a GeneratorNode that is not a GroupGeneratorNode.", m_dataGridControl );

                this.HandleSameLevelGroupReset( m_firstItem );
              }
              else
              {
                this.HandleGlobalItemsReset();
              }
            }
            break;
        }
      }
    }

    private void OnGeneratorNodeItemsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_OnGeneratorNodeItemsCollectionChanged, DataGridTraceArgs.Node( sender ), DataGridTraceArgs.Action( e.Action ) ) )
      {
        if( this.IsHandlingGlobalItemsResetLocally )
        {
          this.TraceEvent( TraceEventType.Warning, DataGridTraceEventId.CustomItemContainerGenerator_OnGeneratorNodeItemsCollectionChanged, DataGridTraceMessages.CannotProcessOnReset, DataGridTraceArgs.Node( sender ), DataGridTraceArgs.Action( e.Action ) );
          return;
        }

        if( this.Status == GeneratorStatus.GeneratingContainers )
          throw DataGridException.Create<InvalidOperationException>( "Cannot perform this operation while the generator is busy generating items.", m_dataGridControl );

        var node = ( ItemsGeneratorNode )sender;
        var updateContainersIndex = !node.IsComputedExpanded;

        e = e.GetRangeActionOrSelf();

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_OnGeneratorNodeItemsCollectionChanged, DataGridTraceArgs.Node( sender ), DataGridTraceArgs.Action( e.Action ) );

        switch( e.Action )
        {
          case NotifyCollectionChangedAction.Add:
            {
              this.HandleItemAddition( node, e );
            }
            break;

          case NotifyCollectionChangedAction.Remove:
            {
              this.HandleItemRemoveMoveReplace( node, e );
            }
            break;

          case NotifyCollectionChangedAction.Move:
          case NotifyCollectionChangedAction.Replace:
            {
              //detect the case where the replace is targeted at a single item, replaced with himself.
              //This particular case is there to handle the particularities of the DGCV with regards to
              //IBindingList.ListChanged.ChangeType == ItemChanged.
              if( ( e.Action == NotifyCollectionChangedAction.Replace ) &&
                  ( e.OldItems.Count == 1 ) && ( e.NewItems.Count == 1 ) &&
                  ( e.NewItems[ 0 ] == e.OldItems[ 0 ] ) )
              {
                // Getting a replace with the same instance should just do nothing.
                //
                // Note : We know that will prevent a row from refreshing correctly if the item 
                // are not implementing a mechanic of notification ( like INotifyPropertyChanged ).

                updateContainersIndex = false;
              }
              else
              {
                //any other case, normal handling
                this.HandleItemRemoveMoveReplace( node, e );
              }
            }
            break;

          case NotifyCollectionChangedAction.Reset:
            {
              this.HandleItemReset( node );
            }
            break;
        }

        if( updateContainersIndex )
        {
          this.UpdateContainersIndex();
        }
      }
    }

    private void OnGeneratorNodeExpansionStateChanged( object sender, ExpansionStateChangedEventArgs e )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "Cannot perform this operation while the generator is busy generating items.", m_dataGridControl );

      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_OnGeneratorNodeExpansionStateChanged, DataGridTraceArgs.Node( sender ) ) )
      {

        var node = sender as GeneratorNode;
        Debug.Assert( node != null );

        var changedNode = node.Parent as GroupGeneratorNode;
        if( changedNode == null )
        {
          this.TraceEvent( TraceEventType.Error, DataGridTraceEventId.CustomItemContainerGenerator_OnGeneratorNodeExpansionStateChanged, DataGridTraceMessages.UnexpectedNode, DataGridTraceArgs.Node( node.Parent ) );
        }

        //Determine if the changedNode is "below" a collapsed group (because if so, I don't need any sort of notification or removal ).
        var parentGroupNode = changedNode.Parent as GroupGeneratorNode;
        if( ( parentGroupNode != null ) && ( !parentGroupNode.IsComputedExpanded ) )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_OnGeneratorNodeExpansionStateChanged, DataGridTraceMessages.NodeIsCollapsed, DataGridTraceArgs.Node( node ) );
          return;
        }

        var nodeHelper = new GeneratorNodeHelper( node, 0, 0 );
        nodeHelper.ReverseCalculateIndex();

        //if the node was "Collapsed"
        if( !e.NewExpansionState )
        {
          var startIndex = nodeHelper.Index + e.IndexOffset;
          var containers = new List<DependencyObject>();

          this.RemoveGeneratedItems( startIndex, startIndex + e.Count - 1, containers );
          this.SendRemoveEvent( e.Count, containers );
        }
        //if the node was "Expanded" 
        else
        {
          this.SendAddEvent( e.Count );
        }
      }
    }

    private void OnGroupGeneratorNodeIsExpandedChanging( object sender, EventArgs e )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "Cannot perform this operation while the generator is busy generating items.", m_dataGridControl );

      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_OnGroupGeneratorNodeIsExpandedChanging, DataGridTraceArgs.Node( sender ) ) )
      {
        if( m_currentGenPosToIndexInhibiterDisposable != null )
        {
          this.TraceEvent( TraceEventType.Error, DataGridTraceEventId.CustomItemContainerGenerator_OnGroupGeneratorNodeIsExpandedChanging, DataGridTraceMessages.InhibiterAlreadySet );
        }

        m_currentGenPosToIndexInhibiterDisposable = this.InhibitParentGenPosToIndexUpdate();

        var groupGeneratorNode = sender as GroupGeneratorNode;

        if( ( m_dataGridContext != null )
          && ( !m_dataGridContext.IsDeferRestoringState )
          && ( !m_dataGridContext.IsRestoringState )
          && ( m_dataGridControl != null )
          && ( groupGeneratorNode != null )
          && ( groupGeneratorNode.IsExpanded ) )
        {
          var tableflowItemsHost = m_dataGridControl.ItemsHost as TableflowViewItemsHost;
          if( tableflowItemsHost != null )
          {
            tableflowItemsHost.OnGroupCollapsing( groupGeneratorNode.UIGroup );
          }
        }
      }
    }

    private void OnGroupGeneratorNodeIsExpandedChanged( object sender, EventArgs e )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "Cannot perform this operation while the generator is busy generating items.", m_dataGridControl );

      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_OnGroupGeneratorNodeIsExpandedChanged, DataGridTraceArgs.Node( sender ) ) )
      {
        var node = sender as GroupGeneratorNode;
        if( node != null )
        {
          this.IncrementCurrentGenerationCount();
        }
        else
        {
          this.TraceEvent( TraceEventType.Error, DataGridTraceEventId.CustomItemContainerGenerator_OnGroupGeneratorNodeIsExpandedChanged, DataGridTraceMessages.UnexpectedNode, DataGridTraceArgs.Node( sender ) );
        }

        if( m_currentGenPosToIndexInhibiterDisposable != null )
        {
          m_currentGenPosToIndexInhibiterDisposable.Dispose();
          m_currentGenPosToIndexInhibiterDisposable = null;
        }
      }
    }

    private void OnGeneratorNodeGroupsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "Cannot perform this operation while the generator is busy generating items.", m_dataGridControl );

      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_OnGeneratorNodeGroupsCollectionChanged, DataGridTraceArgs.Node( sender ), DataGridTraceArgs.Action( e.Action ) ) )
      {
        var node = sender as GroupGeneratorNode;
        if( node == null )
        {
          this.TraceEvent( TraceEventType.Error, DataGridTraceEventId.CustomItemContainerGenerator_OnGeneratorNodeGroupsCollectionChanged, DataGridTraceMessages.UnexpectedNode, DataGridTraceArgs.Node( sender ), DataGridTraceArgs.Action( e.Action ) );
          return;
        }

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_OnGeneratorNodeGroupsCollectionChanged, DataGridTraceArgs.Node( sender ), DataGridTraceArgs.Action( e.Action ) );

        switch( e.Action )
        {
          case NotifyCollectionChangedAction.Add:
            {
              var count = default( int );
              this.HandleParentGroupAddition( node, out count, e );

              if( node.IsComputedExpanded )
              {
                this.IncrementCurrentGenerationCount();
                this.SendAddEvent( count );
              }
              else
              {
                this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_OnGeneratorNodeGroupsCollectionChanged, DataGridTraceMessages.NodeIsCollapsed, DataGridTraceArgs.Node( node ) );
              }
            }
            break;

          case NotifyCollectionChangedAction.Move:
            {
              if( node.Child == null )
                throw DataGridException.Create<DataGridInternalException>( "An attempt was made to move a group with a null child GeneratorNode.", m_dataGridControl );

              Debug.Assert( e.OldStartingIndex != e.NewStartingIndex, "An attempt was made to move a group to the same location." );

              this.HandleSameLevelGroupMove( node.Child, e );
            }
            break;

          case NotifyCollectionChangedAction.Remove:
            {
              if( node.Child == null )
                throw DataGridException.Create<DataGridInternalException>( "An attempt was made to remove a group with a null child GeneratorNode.", m_dataGridControl );

              var count = default( int );
              var containers = new List<DependencyObject>();

              this.HandleParentGroupRemove( node, out count, e, containers );

              if( node.IsComputedExpanded )
              {
                this.IncrementCurrentGenerationCount();
                this.SendRemoveEvent( count, containers );
              }
              else
              {
                this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_OnGeneratorNodeGroupsCollectionChanged, DataGridTraceMessages.NodeIsCollapsed, DataGridTraceArgs.Node( node ) );
              }
            }
            break;

          case NotifyCollectionChangedAction.Replace:
            throw DataGridException.Create<NotSupportedException>( "Replace not supported at the moment on groups.", m_dataGridControl );

          case NotifyCollectionChangedAction.Reset:
            {
              if( node.Child == null )
                throw DataGridException.Create<DataGridInternalException>( "An attempt was made to reset a group with a null child GeneratorNode.", m_dataGridControl );

              this.HandleSameLevelGroupReset( node.Child );
            }
            break;
        }
      }
    }

    private void OnGeneratorNodeHeadersFootersCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_OnGeneratorNodeHeadersFootersCollectionChanged ) )
      {
        if( this.IsHandlingGlobalItemsResetLocally )
        {
          this.TraceEvent( TraceEventType.Warning, DataGridTraceEventId.CustomItemContainerGenerator_OnGeneratorNodeHeadersFootersCollectionChanged, DataGridTraceMessages.CannotProcessOnReset );
          return;
        }

        if( this.Status == GeneratorStatus.GeneratingContainers )
          throw DataGridException.Create<InvalidOperationException>( "Cannot perform this operation while the generator is busy generating items.", m_dataGridControl );

        var nodes = ( IEnumerable<HeadersFootersGeneratorNode> )sender;
        Debug.Assert( nodes != null );
        Debug.Assert( nodes.Any() );

        e = e.GetRangeActionOrSelf();

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_OnGeneratorNodeHeadersFootersCollectionChanged, DataGridTraceArgs.Action( e.Action ) );

        switch( e.Action )
        {
          case NotifyCollectionChangedAction.Add:
            this.HandleHeadersFootersAddition( nodes, e );
            break;

          case NotifyCollectionChangedAction.Remove:
          case NotifyCollectionChangedAction.Move:
          case NotifyCollectionChangedAction.Replace:
            this.HandleHeadersFootersRemoveMoveReplace( nodes, e );
            break;

          case NotifyCollectionChangedAction.Reset:
            this.HandleGlobalItemsReset();
            break;
        }
      }
    }

    private void OnViewThemeChanged( object sender, EventArgs e )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "Cannot perform this operation while the generator is busy generating items.", m_dataGridControl );

      if( m_startNode != null )
      {
        this.CleanupGenerator();
      }

      this.IncrementCurrentGenerationCount();
    }

    private void OnGroupConfigurationSelectorChanged()
    {
      this.CleanupGenerator();
    }

    private void OnRecyclingPoolsContainersRemoved( object sender, ContainersRemovedEventArgs e )
    {
      this.OnContainersRemoved( e.RemovedContainers );
    }

    private void ResetNodeList()
    {
      this.UpdateHeaders( m_dataGridContext.Headers );

      int addCount;
      this.SetupInitialItemsNodes( out addCount );

      this.UpdateFooters( m_dataGridContext.Footers );
    }

    private int ClearItems()
    {
      //if the first item is not null, that means that we have items within the grid.
      if( m_firstItem == null )
        return 0;

      var previous = m_firstItem.Previous;

      //there are footers items
      if( m_firstFooter != null )
      {
        //there is a previous item (headers present)
        if( previous != null )
        {
          m_firstFooter.Previous.Next = null; //clear the next pointer from the last item
          m_firstFooter.Previous = previous; //set the last header as the previous from the first footer
          previous.Next = m_firstFooter; //set the first footer as the next from the last header
        }
        else
        {
          //there is no header present
          m_firstFooter.Previous.Next = null; //set the last item next to null (protect again recursive clearing)
          m_firstFooter.Previous = null; //make the first footer have not previous

          m_startNode = m_firstFooter;
        }
      }
      else
      {
        //There is no footers after the items.
        if( previous != null )
        {
          //this means we have some headers before the items.
          previous.Next = null;
        }
        else
        {
          //this means we have no headers before the items and no footers after.
          m_startNode = null;
        }
      }

      int removeCount;
      int chainLength;
      GeneratorNodeHelper.EvaluateChain( m_firstItem, out removeCount, out chainLength );

      this.ClearLateGroupLevelDescriptions();
      m_groupNodeMappingCache.Clear();

      this.NodeFactory.CleanGeneratorNodeTree( m_firstItem );
      m_firstItem = null;

      this.InvalidateNodeTree();

      return removeCount;
    }

    private void ClearRecyclingPools()
    {
      // Only the top most generator may clear the recycling pools.
      if( m_dataGridContext.SourceDetailConfiguration != null )
        return;

      m_recyclingPools.Clear();
    }

    private int RemoveGeneratedItem( int index, IList<DependencyObject> removedContainers )
    {
      if( ( index < 0 ) || ( index >= m_genPosToIndex.Count ) )
        return 0;

      Debug.Assert( ( m_genPosToContainer.Count == m_genPosToIndex.Count )
                 && ( m_genPosToIndex.Count == m_genPosToItem.Count )
                 && ( m_genPosToItem.Count == m_genPosToNode.Count ) );

      var item = m_genPosToItem[ index ];
      var container = m_genPosToContainer[ index ];
      var node = m_genPosToNode[ index ];

      //remove the item from the 4 lists... (same as doing a "remove")
      this.GenPosArraysRemoveAt( index );

      if( removedContainers != null )
      {
        removedContainers.Insert( 0, container );
      }

      //to ensure this is only done once, check if the node associated with the container is a Detail or not
      var detailNode = node as DetailGeneratorNode;
      if( detailNode == null )
      {
        //Node for item is NOT a detail, can safelly remove the container locally (int this generator instance).
        this.RemoveContainer( container, item );

        return 1;
      }
      else
      {
        //node is from a detail relationship, calling a more appropriate detail generator for removal
        return detailNode.DetailGenerator.RemoveGeneratedItem( container );
      }
    }

    private int RemoveGeneratedItem( DependencyObject container )
    {
      var index = m_genPosToContainer.IndexOf( container );
      if( index < 0 )
        return 0;

      return this.RemoveGeneratedItem( index, null );
    }

    private void RemoveGeneratedItems( int startIndex, int endIndex, IList<DependencyObject> removedContainers )
    {
      if( startIndex > endIndex )
        return;

      var count = m_genPosToIndex.Count;
      if( ( count <= 0 ) || ( m_genPosToIndex[ 0 ] > endIndex ) || ( m_genPosToIndex[ count - 1 ] < startIndex ) )
        return;

      Debug.Assert( ( m_genPosToContainer.Count == m_genPosToIndex.Count )
                 && ( m_genPosToIndex.Count == m_genPosToItem.Count )
                 && ( m_genPosToItem.Count == m_genPosToNode.Count ) );

      //cycle through the list of generated items, and see if any items are generated between the indexes in the removed range.
      //start from the end so that removing an item will not cause the indexes to shift.
      for( var i = count - 1; i >= 0; i-- )
      {
        var index = m_genPosToIndex[ i ];

        //if the item is within the range removed
        if( ( index >= startIndex ) && ( index <= endIndex ) )
        {
          //this will ensure to recurse the call to the appropriate Detail Generator for clearing of the container.
          //otherwise, it will only remove it from the list of container generated in the current generator instance.
          this.RemoveGeneratedItem( i, removedContainers );
        }
      }
    }

    private void RemoveGeneratedItems( GeneratorNode referenceNode, IList<DependencyObject> removedContainers )
    {
      var toRemove = new List<GeneratorNode>();

      toRemove.Add( referenceNode );

      var itemsNode = referenceNode as ItemsGeneratorNode;
      if( itemsNode != null )
      {
        if( itemsNode.Details != null )
        {
          //cycle through all the details currently mapped to the reference node
          foreach( var detailsForNode in itemsNode.Details )
          {
            foreach( var detailNode in detailsForNode.Value )
            {
              toRemove.Add( detailNode );
            }
          }
        }
      }

      //cycle through the list of generated items, and see if any items are from the reference node passed.
      //start from the end so that removing an item will not cause the indexes to shift
      for( int i = m_genPosToNode.Count - 1; i >= 0; i-- )
      {
        var node = m_genPosToNode[ i ];

        //if the item is within the range removed
        if( toRemove.Contains( node ) )
        {
          //this will ensure to recurse the call to the appropriate Detail Generator for clearing of the container.
          //otherwise, it will only remove it from the list of container generated in the current generator instance.
          this.RemoveGeneratedItem( i, removedContainers );
        }
      }
    }

    private void RemoveAllGeneratedItems()
    {
      Debug.Assert( this.IsHandlingGlobalItemsReset, "A flag is not set." );

      //Call RemoveAllGeneratedItems() on all the detail generators
      foreach( var generator in this.GetDetailGenerators() )
      {
        generator.RemoveAllGeneratedItems();
      }

      //then we can clean the list of items of items generated held by this generator
      var genCountRemoved = m_genPosToNode.Count;

      //start from the end so that removing an item will not cause the indexes to shift
      for( int i = genCountRemoved - 1; i >= 0; i-- )
      {
        var node = m_genPosToNode[ i ];
        var container = m_genPosToContainer[ i ];

        //if item is NOT a detail
        if( !( node is DetailGeneratorNode ) )
        {
          // Clear it.
          this.RemoveContainer( container, m_genPosToItem[ i ] );
        }

        //Note: there is no need to call "RemoveContainer" on the "detail" items... since RemoveAllGeneratedItems() was called 
        //      on all detail generators already.
        this.GenPosArraysRemoveAt( i );
      }
    }

    private bool RemoveGeneratedItems( HeadersFootersGeneratorNode referenceNode, object referenceItem, IList<DependencyObject> removedContainers )
    {

      //cycle through the list of generated items, and see if any items are generated between the indexes in the removed range.
      //start from the end so that removing an item will not cause the indexes to shift
      for( int i = m_genPosToNode.Count - 1; i >= 0; i-- )
      {
        var node = m_genPosToNode[ i ];

        //if the item is within the range removed
        if( node == referenceNode )
        {
          var item = m_genPosToItem[ i ];

          if( item.Equals( referenceItem ) )
          {
            var container = m_genPosToContainer[ i ];

            if( removedContainers != null )
            {
              removedContainers.Add( container );
            }

            this.RemoveContainer( container, referenceItem );
            this.GenPosArraysRemoveAt( i );

            return true;
          }
        }
      }

      return false;
    }

    private void RemoveContainer( DependencyObject container, object dataItem )
    {
      m_dataGridControl.ClearItemContainer( container, dataItem );

      var isContainerRecycled = false;

      if( this.IsRecyclingEnabled )
      {
        isContainerRecycled = this.EnqueueContainer( container, dataItem );

        var dataItemStore = container.ReadLocalValue( CustomItemContainerGenerator.DataItemPropertyProperty ) as DataItemDataProviderBase;
        if( dataItemStore != null )
        {
          dataItemStore.ClearDataItem();
        }
      }

      //If recycling is not enabled, or if the container could not be enqueued, make sure it is removed from the DataGridItemsHost's child collection.
      if( !isContainerRecycled )
      {
        Debug.Assert( container != null );
        this.OnContainersRemoved( new DependencyObject[] { container } );
      }
    }

    private void RemoveDetailContainers( IEnumerable<DependencyObject> containers )
    {
      if( containers == null )
        return;

      var toRemove = new HashSet<DependencyObject>( containers );
      if( toRemove.Count <= 0 )
        return;

      for( int i = m_genPosToContainer.Count - 1; i >= 0; i-- )
      {
        if( !toRemove.Contains( m_genPosToContainer[ i ] ) )
          continue;

        this.GenPosArraysRemoveAt( i );
      }
    }

    private void GenPosArraysRemoveAt( int index )
    {
      //remove the item from the 4 lists... (same as doing a "remove")
      var itemsNode = m_genPosToNode[ index ] as ItemsGeneratorNode;

      // Unlock DataVirtualization hold on item's page.
      if( itemsNode != null )
      {
        this.UpdateDataVirtualizationLockForItemsNode( itemsNode, m_genPosToItem[ index ], false );
      }

      this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_GenPosArraysRemoveAt, DataGridTraceMessages.ContainerRemoved, DataGridTraceArgs.Container( m_genPosToContainer[ index ] ), DataGridTraceArgs.Node( m_genPosToNode[ index ] ), DataGridTraceArgs.Item( m_genPosToItem[ index ] ), DataGridTraceArgs.GeneratorIndex( index ), DataGridTraceArgs.Index( m_genPosToIndex[ index ] ) );

      m_genPosToContainer.RemoveAt( index );
      m_genPosToIndex.RemoveAt( index );
      m_genPosToItem.RemoveAt( index );
      m_genPosToNode.RemoveAt( index );
    }

    private void UpdateHeaders( IList headers )
    {
      //If the m_firstHeader is not NULL, then there is nothing to do, since the Header node contains an  observable collection
      //which we monitor otherwise.
      if( m_firstHeader != null )
        return;

      //create the node(s) that would contain the headers
      m_firstHeader = this.CreateHeaders( headers );

      int count;
      int chainLength;
      GeneratorNodeHelper.EvaluateChain( m_firstHeader, out count, out chainLength );

      //if there was items present in the linked list.
      if( m_startNode != null )
      {
        //headers are automatically inserted at the beginning
        var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
        nodeHelper.InsertBefore( m_firstHeader );
      }
      else
      {
        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_UpdateHeaders, DataGridTraceMessages.NewTreeCreated, DataGridTraceArgs.Node( m_firstHeader ) );
      }

      //set the start node as the first header node.
      m_startNode = m_firstHeader;

      this.InvalidateNodeTree();
    }

    private void UpdateFooters( IList footers )
    {
      //If the m_firstHeader is not NULL, then there is nothing to do, since the Header node contains an  observable collection
      //which we monitor otherwise.
      if( m_firstFooter != null )
        return;

      //create the node(s) that would contain the footers
      m_firstFooter = this.CreateFooters( footers );

      //if there are no footers, then the m_firstFooter will remain null
      if( m_firstFooter != null )
      {
        int count;
        int chainLength;
        GeneratorNodeHelper.EvaluateChain( m_firstFooter, out count, out chainLength );

        //if there was items present in the linked list.
        if( m_startNode != null )
        {
          //since we called ClearFooters earlier, I can just go at the end of the list of items
          GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );

          nodeHelper.MoveToEnd();
          nodeHelper.InsertAfter( m_firstFooter );
        }
        else
        {
          m_startNode = m_firstFooter;

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_UpdateFooters, DataGridTraceMessages.NewStartNode, DataGridTraceArgs.Node( m_startNode ) );
        }
      }

      this.InvalidateNodeTree();
    }

    private HeadersFootersGeneratorNode CreateHeaders( IList headers )
    {
      return this.NodeFactory.CreateHeadersFootersGeneratorNode( headers, null, null, null );
    }

    private HeadersFootersGeneratorNode CreateFooters( IList footers )
    {
      return this.NodeFactory.CreateHeadersFootersGeneratorNode( footers, null, null, null );
    }

    private GeneratorNode SetupInitialItemsNodes( out int addCount )
    {
      addCount = 0;

      var newItemNode = default( GeneratorNode );

      this.RefreshGroupsCollection();

      //this function should only be called when the m_firstItem is null.
      if( m_groupsCollection != null )
      {
        newItemNode = this.CreateGroupListFromCollection( m_groupsCollection, null );
      }
      else
      {
        newItemNode = this.CreateStandaloneItemsNode();
      }

      //if the node is non-null, then that's because there was items in the collection
      if( newItemNode != null )
      {
        m_firstItem = newItemNode;

        int chainLength;
        GeneratorNodeHelper.EvaluateChain( m_firstItem, out addCount, out chainLength );

        //find the appropriate point to inject the Items nodes...
        if( m_startNode != null )
        {
          //if there is a footer node, then the insertion point is just before the footer node (if there is anything before!)
          if( m_firstFooter != null )
          {
            var originalPrevious = m_firstFooter.Previous;
            var nodeHelper = new GeneratorNodeHelper( m_firstFooter, 0, 0 ); //do not care about index!

            nodeHelper.InsertBefore( m_firstItem );

            if( originalPrevious == null ) //that means that the first footer is the first item
            {
              m_startNode = newItemNode;
            }
          }
          else if( m_firstHeader != null ) //if there is no footer but some headers, add it at the end.
          {
            var nodeHelper = new GeneratorNodeHelper( m_firstHeader, 0, 0 ); //do not care about index!

            nodeHelper.MoveToEnd();
            nodeHelper.InsertAfter( m_firstItem );
          }
          else
          {
            //this case should not be possible: no header, no footers but there is a startNode
            throw DataGridException.Create<DataGridInternalException>( "No start node found for the current GeneratorNode.", m_dataGridControl );
          }
        }
        else
        {
          m_startNode = m_firstItem;

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_SetupInitialItemsNodes, DataGridTraceMessages.NewStartNode, DataGridTraceArgs.Node( m_startNode ) );
        }

        this.InvalidateNodeTree();
      }

      return newItemNode;
    }

    private void RefreshGroupsCollection()
    {
      var groupsCollection = m_collectionView.Groups;
      if( m_groupsCollection == groupsCollection )
        return;

      if( m_groupsCollection != null )
      {
        CollectionChangedEventManager.RemoveListener( m_groupsCollection, this );
      }

      m_groupsCollection = groupsCollection;

      if( m_groupsCollection != null )
      {
        CollectionChangedEventManager.AddListener( m_groupsCollection, this );
      }
    }

    private GeneratorNode CreateStandaloneItemsNode()
    {
      var list = CustomItemContainerGenerator.GetList( m_collectionView );

      return this.NodeFactory.CreateItemsGeneratorNode( list, null, null, null );
    }

    private GeneratorNode CreateGroupListFromCollection( IList collection, GeneratorNode parentNode )
    {
      if( collection.Count == 0 )
        return null;

      var rootNode = default( GeneratorNode );
      var previousNode = default( GeneratorNode );
      var actualNode = default( GroupGeneratorNode );
      var childNode = default( GeneratorNode );
      var level = ( parentNode == null ) ? 0 : parentNode.Level + 1;

      GroupConfiguration groupConfig;
      bool initiallyExpanded;

      var groupDescriptions = DataGridContext.GetGroupDescriptionsHelper( m_collectionView );
      var groupConfigurationSelector = m_dataGridContext.GroupConfigurationSelector;
      var lateGroupLevelDescription = this.CreateOrGetLateGroupLevelDescription( level, groupDescriptions );

      foreach( CollectionViewGroup group in collection )
      {
        groupConfig = GroupConfiguration.GetGroupConfiguration( m_dataGridContext, groupDescriptions, groupConfigurationSelector, level, group );
        Debug.Assert( groupConfig != null );

        if( groupConfig.UseDefaultHeadersFooters )
        {
          groupConfig.AddDefaultHeadersFooters();
        }

        initiallyExpanded = groupConfig.InitiallyExpanded;

        actualNode = ( GroupGeneratorNode )this.NodeFactory.CreateGroupGeneratorNode( group, parentNode, previousNode, null, groupConfig );
        actualNode.UIGroup = new Group( actualNode, group, lateGroupLevelDescription, m_dataGridContext );

        m_groupNodeMappingCache.Add( group, actualNode );

        if( rootNode == null )
        {
          rootNode = actualNode;
        }

        previousNode = actualNode;

        //Independently if the Group is the bottom level or not, we need to setup GroupHeaders
        childNode = this.SetupGroupHeaders( groupConfig, actualNode );
        actualNode.Child = childNode;

        var childNodeHelper = new GeneratorNodeHelper( childNode, 0, 0 ); //do not care about index.
        childNodeHelper.MoveToEnd(); //extensibility, just in case SetupGroupHeaders() ever return a node list.

        var subItems = group.GetItems();

        //if the node newly created is not the bottom level
        if( !group.IsBottomLevel )
        {
          if( ( subItems != null ) && ( subItems.Count > 0 ) )
          {
            var subGroupsNode = this.CreateGroupListFromCollection( subItems as IList, actualNode );
            if( subGroupsNode != null )
            {
              childNodeHelper.InsertAfter( subGroupsNode );
            }
          }
        }
        else
        {
          //this is the bottom level, create an Items node
          var itemsNode = this.NodeFactory.CreateItemsGeneratorNode( subItems as IList, actualNode, null, null );
          if( itemsNode != null )
          {
            childNodeHelper.InsertAfter( itemsNode );
          }
        }

        childNodeHelper.InsertAfter( this.SetupGroupFooters( groupConfig, actualNode ) );
      }

      return rootNode;
    }

    private LateGroupLevelDescription CreateOrGetLateGroupLevelDescription( int level, IList<GroupDescription> groupDescriptions )
    {
      Debug.Assert( groupDescriptions != null );

      if( ( m_groupLevelDescriptionCache == null ) || ( level >= m_groupLevelDescriptionCache.Length ) )
      {
        var newSize = Math.Max( level + 1, groupDescriptions.Count );
        Array.Resize<LateGroupLevelDescription>( ref m_groupLevelDescriptionCache, newSize );
      }

      var item = m_groupLevelDescriptionCache[ level ];
      if( item == null )
      {
        var groupDescription = groupDescriptions[ level ];
        Debug.Assert( groupDescription != null );

        item = new LateGroupLevelDescription( groupDescription, m_dataGridContext.GroupLevelDescriptions );
        m_groupLevelDescriptionCache[ level ] = item;
      }

      return item;
    }

    private void ClearLateGroupLevelDescriptions()
    {
      if( m_groupLevelDescriptionCache == null )
        return;

      for( int i = 0; i < m_groupLevelDescriptionCache.Length; i++ )
      {
        var item = m_groupLevelDescriptionCache[ i ];
        if( item == null )
          continue;

        item.Clear();
      }

      m_groupLevelDescriptionCache = null;
    }

    private GeneratorNode SetupGroupFooters( GroupConfiguration groupConfig, GeneratorNode actualNode )
    {
      if( groupConfig == null )
        return new GeneratorNode( actualNode );

      return this.NodeFactory.CreateHeadersFootersGeneratorNode( groupConfig.Footers, actualNode, null, null );
    }

    private GeneratorNode SetupGroupHeaders( GroupConfiguration groupConfig, GeneratorNode actualNode )
    {
      if( groupConfig == null )
        return new GeneratorNode( actualNode );

      return this.NodeFactory.CreateHeadersFootersGeneratorNode( groupConfig.Headers, actualNode, null, null );
    }

    private void HandleParentGroupRemove( GeneratorNode parent, out int countRemoved, NotifyCollectionChangedEventArgs e, IList<DependencyObject> removedContainers )
    {
      var nodeHelper = new GeneratorNodeHelper( parent, 0, 0 ); //do not care about index (for now).

      // start by moving to the first child... of the node (GroupHeaders node, most probably).
      // false parameter is to prevent skipping over a collapsed node (item count 0 )
      if( !nodeHelper.MoveToChild( false ) )
        //could not advance to the child item so there is no items to be removed...
        throw DataGridException.Create<DataGridInternalException>( "No child item in the group to remove.", m_dataGridControl );

      this.HandleSameLevelGroupRemove( nodeHelper.CurrentNode, out countRemoved, e, removedContainers );
    }

    private void HandleSameLevelGroupRemove( GeneratorNode firstChild, out int countRemoved, NotifyCollectionChangedEventArgs e, IList<DependencyObject> removedContainers )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_HandleSameLevelGroupRemove ) )
      {
        countRemoved = 0;

        var nodeHelper = new GeneratorNodeHelper( firstChild, 0, 0 );
        nodeHelper.ReverseCalculateIndex();

        //Advance to the first "Group" node (skip the GroupHeaders)
        while( !( nodeHelper.CurrentNode is GroupGeneratorNode ) )
        {
          if( !nodeHelper.MoveToNext() )
            throw DataGridException.Create<DataGridInternalException>( "Unable to move to next GeneratorNode.", m_dataGridControl );
        }

        //then move up to the removal start point.
        if( !nodeHelper.MoveToNextBy( e.OldStartingIndex ) )
          throw DataGridException.Create<DataGridInternalException>( "Unable to move to the requested generator index.", m_dataGridControl );

        var startNode = nodeHelper.CurrentNode as GroupGeneratorNode;
        var removeIndex = -1;

        //Only fetch the index if the group itself is not "collapsed" or under a collapsed group already
        if( ( startNode.IsExpanded == startNode.IsComputedExpanded ) && ( startNode.ItemCount > 0 ) )
        {
          removeIndex = nodeHelper.Index;
        }

        //retrieve the generator position for the first item to remove.
        this.ProcessGroupRemoval( startNode, e.OldItems.Count, true, out countRemoved );

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_HandleSameLevelGroupRemove, DataGridTraceMessages.GroupNodeRemoved, DataGridTraceArgs.Node( startNode ), DataGridTraceArgs.ItemCount( countRemoved ) );

        //Clean the chain "isolated" previously
        this.NodeFactory.CleanGeneratorNodeTree( startNode );

        if( removeIndex >= 0 )
        {
          this.RemoveGeneratedItems( removeIndex, removeIndex + countRemoved - 1, removedContainers );
        }
      }
    }

    private GroupGeneratorNode ProcessGroupRemoval( GeneratorNode startNode, int removeCount, bool updateGroupNodeMappingCache, out int countRemoved )
    {
      countRemoved = 0;

      var nodeHelper = new GeneratorNodeHelper( startNode, 0, 0 ); //index not important.
      var parentGroup = startNode.Parent as GroupGeneratorNode;
      var i = 0;

      do
      {
        var group = nodeHelper.CurrentNode as GroupGeneratorNode;
        if( group == null )
        {
          this.TraceEvent( TraceEventType.Warning, DataGridTraceEventId.CustomItemContainerGenerator_ProcessGroupRemoval, DataGridTraceMessages.UnexpectedNode, DataGridTraceArgs.Node( nodeHelper.CurrentNode ) );
        }
        else
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ProcessGroupRemoval, DataGridTraceMessages.GroupNodeRemoved, DataGridTraceArgs.Node( group ), DataGridTraceArgs.ItemCount( group.ItemCount ) );
        }

        if( updateGroupNodeMappingCache )
        {
          m_groupNodeMappingCache.Remove( group.CollectionViewGroup );
        }

        //add the total number of child to the count of items removed.
        countRemoved += group.ItemCount;

        i++;

        if( i < removeCount )
        {
          if( !nodeHelper.MoveToNext() )
            throw DataGridException.Create<DataGridInternalException>( "Could not move to the last node to be removed.", m_dataGridControl );
        }
      }
      while( i < removeCount );

      //disconnect the node chain to be removed from the linked list.
      var previous = startNode.Previous;
      var next = nodeHelper.CurrentNode.Next;

      if( next != null )
      {
        next.Previous = previous;
      }

      if( previous != null )
      {
        previous.Next = next;
      }

      //if the first node removed was the first child of its parent
      if( ( parentGroup != null ) && ( parentGroup.Child == startNode ) )
      {
        //set the next in line after the chain to be removed as the first child
        parentGroup.Child = next;
      }

      //break the link between the chain and its same-level siblings.
      nodeHelper.CurrentNode.Next = null;
      startNode.Previous = null;

      //Here, I need a special handling case... If I remove the first group node, I need to set a new firstItem
      if( startNode == m_firstItem )
      {
        if( next != m_firstFooter )
        {
          m_firstItem = next;
        }
        else
        {
          m_firstItem = null;
        }

        if( m_startNode == startNode )
        {
          m_startNode = next;
        }

        this.InvalidateNodeTree();
      }

      if( !( nodeHelper.CurrentNode is GroupGeneratorNode ) )
      {
        this.TraceEvent( TraceEventType.Warning, DataGridTraceEventId.CustomItemContainerGenerator_ProcessGroupRemoval, DataGridTraceMessages.UnexpectedNode, DataGridTraceArgs.LastNode( nodeHelper.CurrentNode ) );
      }

      return ( GroupGeneratorNode )nodeHelper.CurrentNode;
    }

    private void HandleSameLevelGroupReset( GeneratorNode node )
    {
      Debug.Assert( node != null );

      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_HandleSameLevelGroupReset ) )
      {
        var parentNode = node.Parent as GroupGeneratorNode;
        var oldGroups = new List<CollectionViewGroup>();
        var nodeHelper = new GeneratorNodeHelper( node, 0, 0 );

        do
        {
          var groupNode = nodeHelper.CurrentNode as GroupGeneratorNode;
          if( groupNode != null )
          {
            Debug.Assert( groupNode.CollectionViewGroup != null );
            oldGroups.Add( groupNode.CollectionViewGroup );
          }
        }
        while( nodeHelper.MoveToNext() );

        var newGroups = ( parentNode != null )
                          ? parentNode.CollectionViewGroup.GetItems().Cast<CollectionViewGroup>().ToList()
                          : m_groupsCollection.Cast<CollectionViewGroup>().ToList();

        // If nothing has changed, avoid the heavy process of finding out which groups have been added,
        // removed and moved.
        if( newGroups.Count == oldGroups.Count )
        {
          if( newGroups.SequenceEqual( oldGroups ) )
            return;
        }

        var groupsAdded = new HashSet<CollectionViewGroup>();
        var groupsRemoved = new HashSet<CollectionViewGroup>();
        var groupsMoved = new HashSet<CollectionViewGroup>();

        CustomItemContainerGenerator.FindChanges( oldGroups, newGroups, groupsAdded, groupsRemoved, groupsMoved );
        this.ApplyGroupChanges( node, oldGroups, newGroups, groupsAdded, groupsRemoved, groupsMoved );
      }
    }

    private void ApplyGroupChanges(
      GeneratorNode node,
      IList<CollectionViewGroup> oldGroups,
      IList<CollectionViewGroup> newGroups,
      ICollection<CollectionViewGroup> groupsAdded,
      ICollection<CollectionViewGroup> groupsRemoved,
      ICollection<CollectionViewGroup> groupsMoved )
    {
      Debug.Assert( node != null );

      var parentNode = node.Parent as GroupGeneratorNode;
      var insertAfter = !( node is GroupGeneratorNode );

      if( ( groupsRemoved.Count > 0 ) || ( groupsMoved.Count > 0 ) )
      {
        var removeNodeHelper = new GeneratorNodeHelper( m_groupNodeMappingCache[ oldGroups.Last() ], 0, 0 );
        removeNodeHelper.ReverseCalculateIndex();

        for( int i = oldGroups.Count - 1; i >= 0; i-- )
        {
          var group = oldGroups[ i ];
          var removed = groupsRemoved.Contains( group );
          var moved = ( !removed && groupsMoved.Contains( group ) );

          if( removed || moved )
          {
            var groupNode = removeNodeHelper.CurrentNode;
            Debug.Assert( ( groupNode != null ) && ( groupNode == m_groupNodeMappingCache[ group ] ) );

            if( groupNode == node )
            {
              Debug.Assert( !( groupNode.Previous is GroupGeneratorNode ), "How come the node was not the first GroupGeneratorNode." );

              node = groupNode.Next;
              insertAfter = false;
            }

            var startIndex = removeNodeHelper.Index;
            int count;

            removeNodeHelper.MoveToPrevious();

            if( removed )
            {
              this.ProcessGroupRemoval( groupNode, 1, true, out count );
              this.NodeFactory.CleanGeneratorNodeTree( groupNode );
            }
            else
            {
              this.ProcessGroupRemoval( groupNode, 1, false, out count );
            }

            if( count > 0 )
            {
              this.RemoveGeneratedItems( startIndex, startIndex + count - 1, null );
            }
          }
          else
          {
            removeNodeHelper.MoveToPrevious();
          }
        }
      }

      if( ( groupsAdded.Count > 0 ) || ( groupsMoved.Count > 0 ) )
      {
        var firstGroup = newGroups[ 0 ];
        var added = groupsAdded.Contains( firstGroup );
        var moved = ( !added && groupsMoved.Contains( firstGroup ) );

        if( added || moved )
        {
          var groupNode = default( GeneratorNode );

          if( added )
          {
            groupNode = this.CreateGroupListFromCollection( new object[] { firstGroup }, parentNode );
          }
          else
          {
            groupNode = m_groupNodeMappingCache[ firstGroup ];
          }

          Debug.Assert( groupNode != null );

          if( node != null )
          {
            var addNodeHelper = new GeneratorNodeHelper( node, 0, 0 );

            if( insertAfter )
            {
              this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ApplyGroupChanges, DataGridTraceMessages.GroupNodeAdded, DataGridTraceArgs.Node( groupNode ), DataGridTraceArgs.PreviousNode( addNodeHelper.CurrentNode ) );
              addNodeHelper.InsertAfter( groupNode );
            }
            else
            {
              this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ApplyGroupChanges, DataGridTraceMessages.GroupNodeAdded, DataGridTraceArgs.Node( groupNode ), DataGridTraceArgs.NextNode( addNodeHelper.CurrentNode ) );
              addNodeHelper.InsertBefore( groupNode );
              insertAfter = true;
            }

            if( parentNode == null )
            {
              if( m_startNode == m_firstItem )
              {
                m_startNode = groupNode;
              }

              m_firstItem = groupNode;

              this.InvalidateNodeTree();
            }
          }
          else
          {
            if( parentNode == null )
            {
              m_firstItem = groupNode;

              if( m_startNode != null )
              {
                if( m_firstFooter != null )
                {
                  var addNodeHelper = new GeneratorNodeHelper( m_firstFooter, 0, 0 );

                  this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ApplyGroupChanges, DataGridTraceMessages.GroupNodeAdded, DataGridTraceArgs.Node( groupNode ), DataGridTraceArgs.NextNode( addNodeHelper.CurrentNode ) );
                  addNodeHelper.InsertBefore( groupNode );
                }
                else if( m_firstHeader != null )
                {
                  var addNodeHelper = new GeneratorNodeHelper( m_firstHeader, 0, 0 );
                  addNodeHelper.MoveToEnd();

                  this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ApplyGroupChanges, DataGridTraceMessages.GroupNodeAdded, DataGridTraceArgs.Node( groupNode ), DataGridTraceArgs.PreviousNode( addNodeHelper.CurrentNode ) );
                  addNodeHelper.InsertAfter( groupNode );
                }
                else
                {
                  throw DataGridException.Create<DataGridInternalException>( "No start node found for the current GeneratorNode.", m_dataGridControl );
                }
              }
              else
              {
                m_startNode = groupNode;
              }

              this.InvalidateNodeTree();
            }
            else
            {
              Debug.Assert( parentNode.Child != null );

              var addNodeHelper = new GeneratorNodeHelper( parentNode.Child, 0, 0 );
              addNodeHelper.MoveToEnd();

              this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ApplyGroupChanges, DataGridTraceMessages.GroupNodeAdded, DataGridTraceArgs.Node( groupNode ), DataGridTraceArgs.PreviousNode( addNodeHelper.CurrentNode ) );
              addNodeHelper.InsertAfter( groupNode );
            }
          }

          node = groupNode;
        }

        for( var i = 1; i < newGroups.Count; i++ )
        {
          var group = newGroups[ i ];

          added = groupsAdded.Contains( group );
          moved = ( !added && groupsMoved.Contains( group ) );

          if( !added && !moved )
            continue;

          var groupNode = default( GeneratorNode );

          if( added )
          {
            groupNode = this.CreateGroupListFromCollection( new object[] { group }, parentNode );
          }
          else
          {
            groupNode = m_groupNodeMappingCache[ group ];
          }

          Debug.Assert( groupNode != null );

          var addNodeHelper = new GeneratorNodeHelper( m_groupNodeMappingCache[ newGroups[ i - 1 ] ], 0, 0 );

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_ApplyGroupChanges, DataGridTraceMessages.GroupNodeAdded, DataGridTraceArgs.Node( groupNode ), DataGridTraceArgs.PreviousNode( addNodeHelper.CurrentNode ) );
          addNodeHelper.InsertAfter( groupNode );
        }
      }

      this.IncrementCurrentGenerationCount();
      this.SendResetEvent();
    }

    private void HandleSameLevelGroupMove( GeneratorNode node, NotifyCollectionChangedEventArgs e )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_HandleSameLevelGroupMove ) )
      {
        var parentGroup = node.Parent as GroupGeneratorNode;

        //Start a NodeHelper on the first child of the node where the move occured.
        var nodeHelper = new GeneratorNodeHelper( node, 0, 0 );
        nodeHelper.ReverseCalculateIndex(); //determine index of the node.

        //Advance to the first "Group" node (skip the GroupHEaders)
        while( !( nodeHelper.CurrentNode is GroupGeneratorNode ) )
        {
          if( !nodeHelper.MoveToNext() )
            throw DataGridException.Create<DataGridInternalException>( "Unable to move to next GeneratorNode.", m_dataGridControl );
        }

        //then move up to the removal start point.
        if( !nodeHelper.MoveToNextBy( e.OldStartingIndex ) )
          throw DataGridException.Create<DataGridInternalException>( "Unable to move to the requested generator index.", m_dataGridControl );

        //remember the current node as the start point of the move (will be used when "extracting the chain")
        var startNode = nodeHelper.CurrentNode;
        //also remember the index of the node, to calculate range of elements to remove (containers )
        var startIndex = nodeHelper.Index;

        //then, cumulate the total number of items in the groups concerned
        var totalCountRemoved = 0;

        node = this.ProcessGroupRemoval( startNode, e.OldItems.Count, false, out totalCountRemoved );

        //send a message to the panel to remove the visual elements concerned 
        var containers = new List<DependencyObject>();

        this.RemoveGeneratedItems( startIndex, startIndex + totalCountRemoved - 1, containers );
        this.SendRemoveEvent( totalCountRemoved, containers );

        //reset the node parameter for the "re-addition"
        node = ( parentGroup != null ) ? parentGroup.Child : m_firstItem;

        if( node == null )
          throw DataGridException.Create<DataGridInternalException>( "No node found to move.", m_dataGridControl );

        //Once the chain was pulled out, re-insert it at the appropriate location.
        nodeHelper = new GeneratorNodeHelper( node, 0, 0 ); //do not care about the index for what I need

        //Advance to the first "Group" node (skip the GroupHEaders)
        while( !( nodeHelper.CurrentNode is GroupGeneratorNode ) )
        {
          if( !nodeHelper.MoveToNext() )
            throw DataGridException.Create<DataGridInternalException>( "Unable to move to next GeneratorNode.", m_dataGridControl );
        }

        bool insertBefore = nodeHelper.MoveToNextBy( e.NewStartingIndex );

        if( insertBefore )
        {
          if( nodeHelper.CurrentNode == m_firstItem )
          {
            if( m_startNode == m_firstItem )
            {
              m_startNode = startNode;
            }

            m_firstItem = startNode;

            this.InvalidateNodeTree();
          }

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_HandleSameLevelGroupMove, DataGridTraceMessages.GroupNodeAdded, DataGridTraceArgs.Node( startNode ), DataGridTraceArgs.NextNode( nodeHelper.CurrentNode ), DataGridTraceArgs.ItemCount( totalCountRemoved ) );
          nodeHelper.InsertBefore( startNode );
        }
        else
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_HandleSameLevelGroupMove, DataGridTraceMessages.GroupNodeAdded, DataGridTraceArgs.Node( startNode ), DataGridTraceArgs.PreviousNode( nodeHelper.CurrentNode ), DataGridTraceArgs.ItemCount( totalCountRemoved ) );
          nodeHelper.InsertAfter( startNode );
        }

        //and finally, call to increment the generation count for the generator content
        this.IncrementCurrentGenerationCount();
      }
    }

    private void HandleSameLevelGroupAddition( GeneratorNode firstChild, out int countAdded, NotifyCollectionChangedEventArgs e )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_HandleSameLevelGroupAddition ) )
      {
        if( ( firstChild.Parent != null ) && !( firstChild.Parent is GroupGeneratorNode ) )
        {
          this.TraceEvent( TraceEventType.Error, DataGridTraceEventId.CustomItemContainerGenerator_HandleSameLevelGroupAddition, DataGridTraceMessages.UnexpectedNode, DataGridTraceArgs.Node( firstChild.Parent ) );
        }

        var newNodeChain = this.CreateGroupListFromCollection( e.NewItems, firstChild.Parent );

        countAdded = 0;
        if( newNodeChain != null )
        {
          int chainLength;
          GeneratorNodeHelper.EvaluateChain( newNodeChain, out countAdded, out chainLength );

          var nodeHelper = default( GeneratorNodeHelper );
          var insertAfter = false;

          // Instead of counting groups to insert the group chain, we may locate right away the GroupGeneratorNode that
          // preceed the insertion point.  This will give us a performance boost when there are lots of groups.
          if( e.NewStartingIndex > 0 )
          {
            IList groups;

            if( firstChild.Parent != null )
            {
              var parentGroup = ( ( GroupGeneratorNode )firstChild.Parent ).CollectionViewGroup;

              groups = ( parentGroup != null ) ? parentGroup.Items : null;
            }
            else
            {
              groups = m_groupsCollection;
            }

            if( ( groups != null ) && ( groups.Count > 0 ) )
            {
              Debug.Assert( e.NewStartingIndex <= groups.Count );

              var previousGroup = ( CollectionViewGroup )groups[ e.NewStartingIndex - 1 ];
              var previousGroupNode = default( GroupGeneratorNode );

              if( m_groupNodeMappingCache.TryGetValue( previousGroup, out previousGroupNode ) )
              {
                nodeHelper = new GeneratorNodeHelper( previousGroupNode, 0, 0 ); //do not care about index.
                insertAfter = true;
              }
            }
          }

          // We could not find the insertion point efficiently, use a fallback strategy.
          if( nodeHelper == null )
          {
            nodeHelper = new GeneratorNodeHelper( firstChild, 0, 0 ); //do not care about index.

            while( !( nodeHelper.CurrentNode is GroupGeneratorNode ) )
            {
              if( !nodeHelper.MoveToNext() )
              {
                //if there are no items and no groups in the Group Node, then we will never find a GroupGeneratorNode...
                //However, the structure of the group/headers/footers/group headers/groups footers makes it so 
                //that inserting before the last item (footer/group footer) will place the group at the appropriate location.
                break;
              }
            }

            //If there is 0 group in the parent group, then this loop will exit without executing the control block once...
            for( int i = 0; i < e.NewStartingIndex; i++ )
            {
              if( !nodeHelper.MoveToNext() )
              {
                insertAfter = true;
              }
            }
          }

          Debug.Assert( nodeHelper != null );

          //if we are inserting past the end of the linked list level.
          if( insertAfter )
          {
            this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_HandleSameLevelGroupAddition, DataGridTraceMessages.GroupNodeAdded, DataGridTraceArgs.Node( newNodeChain ), DataGridTraceArgs.PreviousNode( nodeHelper.CurrentNode ), DataGridTraceArgs.ItemCount( countAdded ) );
            nodeHelper.InsertAfter( newNodeChain );
          }
          else
          {
            this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_HandleSameLevelGroupAddition, DataGridTraceMessages.GroupNodeAdded, DataGridTraceArgs.Node( newNodeChain ), DataGridTraceArgs.NextNode( nodeHelper.CurrentNode ), DataGridTraceArgs.ItemCount( countAdded ) );
            nodeHelper.InsertBefore( newNodeChain );
          }


          //If the insertion point is the beginning, check that the node pointers are updated properly
          if( ( e.NewStartingIndex == 0 ) && ( ( firstChild.Parent == null ) ) )
          {
            if( m_startNode == m_firstItem )
            {
              m_startNode = newNodeChain;
            }

            m_firstItem = newNodeChain;

            this.InvalidateNodeTree();
          }
        }
      }
    }

    private void HandleParentGroupAddition( GeneratorNode parent, out int countAdded, NotifyCollectionChangedEventArgs e )
    {
      var nodeHelper = new GeneratorNodeHelper( parent, 0, 0 ); //do not care about index (for now).

      //start by moving to the first child... of the node (GroupHeaders node, most probably).
      if( !nodeHelper.MoveToChild( false ) ) 
        throw DataGridException.Create<DataGridInternalException>( "Could not move to the child node to be removed.", m_dataGridControl );

      this.HandleSameLevelGroupAddition( nodeHelper.CurrentNode, out countAdded, e );
    }

    private void HandleItemAddition( ItemsGeneratorNode node, NotifyCollectionChangedEventArgs e )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_HandleItemAddition, DataGridTraceArgs.Node( node ) ) )
      {
        var nodeHelper = new GeneratorNodeHelper( node, 0, 0 ); //index not important for now.
        var itemCount = e.NewItems.Count;

        node.AdjustItemCount( itemCount );
        node.AdjustLeafCount( itemCount );
        this.OffsetDetails( node, e.NewStartingIndex, itemCount );

        if( node.IsComputedExpanded )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_HandleItemAddition, DataGridTraceMessages.ItemAdded, DataGridTraceArgs.ItemCount( itemCount ) );

          this.IncrementCurrentGenerationCount();
          this.SendAddEvent( itemCount );
        }
        else
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_HandleItemAddition, DataGridTraceMessages.CollapsedItemAdded, DataGridTraceArgs.ItemCount( itemCount ) );
        }
      }
    }

    private void HandleItemRemoveMoveReplace( ItemsGeneratorNode node, NotifyCollectionChangedEventArgs e )
    {
      if( e.Action == NotifyCollectionChangedAction.Remove )
      {
        node.AdjustItemCount( -e.OldItems.Count );
        node.AdjustLeafCount( -e.OldItems.Count );
      }

      var nodeStartIndex = e.OldStartingIndex;
      var nodeEndIndex = nodeStartIndex + e.OldItems.Count - 1;
      var detailCountToRemove = CustomItemContainerGenerator.ComputeDetailsCount( node, nodeStartIndex, nodeEndIndex );
      var detailCountBeforeRemovedItems = 0;

      if( nodeStartIndex > 0 )
      {
        detailCountBeforeRemovedItems = CustomItemContainerGenerator.ComputeDetailsCount( node, 0, nodeStartIndex - 1 );
      }

      var removeCount = e.OldItems.Count + detailCountToRemove;
      var replaceCount = ( e.Action == NotifyCollectionChangedAction.Replace ) ? e.NewItems.Count : 0;
      var startIndex = -1;
      var endIndex = -1;

      // We must memorize the range of items that we are going to remove before starting any action.
      if( node.IsComputedExpanded )
      {
        var nodeHelper = new GeneratorNodeHelper( node, 0, 0 ); //index not important for now.
        nodeHelper.ReverseCalculateIndex();

        startIndex = nodeHelper.Index + e.OldStartingIndex + detailCountBeforeRemovedItems;
        endIndex = startIndex + detailCountToRemove + e.OldItems.Count - 1;
      }

      //Remove the details from the ItemsGeneratorNode and re-index the other details appropriatly.
      this.RemoveDetails( node, nodeStartIndex, nodeEndIndex, replaceCount );

      //Try to remap the old item for detail remapping (will do nothing if item has no details )
      foreach( var oldItem in e.OldItems )
      {
        this.QueueDetailItemForRemapping( oldItem );
      }

      var raiseEvent = false;
      var removedContainers = default( List<DependencyObject> );

      // Remove the matching realized containers.
      if( node.IsComputedExpanded )
      {
        raiseEvent = true;
        removedContainers = new List<DependencyObject>();

        Debug.Assert( ( startIndex >= 0 ) && ( endIndex >= 0 ) );

        this.RemoveGeneratedItems( startIndex, endIndex, removedContainers );
      }

      if( e.Action == NotifyCollectionChangedAction.Move )
      {
        this.OffsetDetails( node, e.NewStartingIndex, e.NewItems.Count );
      }

      if( raiseEvent )
      {
        Debug.Assert( removedContainers != null );

        this.IncrementCurrentGenerationCount();
        this.SendRemoveEvent( removeCount, removedContainers );

        switch( e.Action )
        {
          case NotifyCollectionChangedAction.Move:
          case NotifyCollectionChangedAction.Replace:
            {
              this.SendAddEvent( e.NewItems.Count );
            }
            break;

          case NotifyCollectionChangedAction.Remove:
            // There is nothing to do.
            break;

          case NotifyCollectionChangedAction.Add:
          case NotifyCollectionChangedAction.Reset:
            throw DataGridException.Create<NotSupportedException>( "Add or Reset not supported at the moment on groups.", m_dataGridControl );
        }
      }
    }

    private void HandleHeadersFootersAddition( IEnumerable<HeadersFootersGeneratorNode> nodes, NotifyCollectionChangedEventArgs e )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_HandleHeadersFootersAddition ) )
      {
        var itemCount = e.NewItems.Count;
        var notify = false;

        foreach( var node in nodes )
        {
          node.AdjustItemCount( itemCount );
          notify = notify || node.IsComputedExpanded;
        }

        if( notify )
        {
          this.IncrementCurrentGenerationCount();

          this.SendResetEvent();
        }
        else
        {
          //There is no need to regenerate the containers.  However, we still need to update the realized containers source index.
          this.UpdateContainersIndex();
        }
      }
    }

    private void HandleHeadersFootersRemoveMoveReplace( IEnumerable<HeadersFootersGeneratorNode> nodes, NotifyCollectionChangedEventArgs e )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_HandleHeadersFootersRemoveMoveReplace ) )
      {
        Debug.Assert( e.OldItems != null );

        var itemCount = ( ( e.NewItems != null ) ? e.NewItems.Count : 0 ) - e.OldItems.Count;
        var notify = false;

        foreach( var node in nodes )
        {
          node.AdjustItemCount( itemCount );

          if( node.IsComputedExpanded )
          {
            notify = true;

            var parentGroup = node.Parent as GroupGeneratorNode;

            foreach( var item in e.OldItems )
            {
              var targetItem = ( parentGroup != null ) ? new GroupHeaderFooterItem( parentGroup.CollectionViewGroup, item ) : item;

              this.RemoveGeneratedItems( node, targetItem, null );
            }
          }
        }

        if( notify )
        {
          this.IncrementCurrentGenerationCount();

          this.SendResetEvent();
        }
        else
        {
          //There is no need to regenerate the containers.  However, we still need to update the realized containers source index.
          this.UpdateContainersIndex();
        }
      }
    }

    private void UpdateContainersIndex()
    {
      if( ( m_startNode == null ) || ( m_genPosToItem.Count <= 0 ) )
        return;

      var collectionView = m_collectionView.SourceCollection as DataGridCollectionView;
      if( collectionView != null )
      {
        for( int i = 0; i < m_genPosToItem.Count; i++ )
        {
          // If we're not on an ItemsGeneratorNode, it means we are not on a DataRow, so no need to process it (all ItemIndex == -1 in this case).
          if( !( m_genPosToNode[ i ] is ItemsGeneratorNode ) )
            continue;

          // There is no need to find the node or the item within the node since it should match the CollectionView.
          var sourceIndex = collectionView.GetGlobalSortedIndexFromDataItem( m_genPosToItem[ i ] );

          DataGridVirtualizingPanel.SetItemIndex( m_genPosToContainer[ i ], sourceIndex );
        }
      }
      else
      {
        var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );

        for( int i = 0; i < m_genPosToItem.Count; i++ )
        {
          // If we're not on an ItemsGeneratorNode, it means we are not on a DataRow, so no need to process it (all ItemIndex == -1 in this case).
          var node = m_genPosToNode[ i ] as ItemsGeneratorNode;
          if( node == null )
            continue;

          int offset = -1;

          if( nodeHelper.FindNode( node ) && ( nodeHelper.FindItem( m_genPosToItem[ i ] ) >= 0 ) )
          {
            offset = node.IndexOf( m_genPosToItem[ i ] );
          }

          // If not a DGCV, it means there are no details, so this will work fine.
          var sourceIndex = ( offset >= 0 )
                              ? nodeHelper.SourceDataIndex + offset
                              : -1;

          DataGridVirtualizingPanel.SetItemIndex( m_genPosToContainer[ i ], sourceIndex );
        }
      }
    }

    private void OffsetDetails( ItemsGeneratorNode node, int startIndex, int addOffset )
    {
      if( ( node.Details == null ) || ( node.Details.Count == 0 ) )
        return;

      var detailsCount = node.Details.Count;
      var keys = new int[ detailsCount ];
      node.Details.Keys.CopyTo( keys, 0 );

      Array.Sort<int>( keys );

      for( int i = detailsCount - 1; i >= 0; i-- )
      {
        var key = keys[ i ];
        if( key >= startIndex )
        {
          List<DetailGeneratorNode> details;
          if( !node.Details.TryGetValue( key, out details ) )
            throw DataGridException.Create<DataGridInternalException>( "Key not found in Details dictionary.", m_dataGridControl );

          node.Details.Remove( key );
          node.Details.Add( key + addOffset, details );
        }
      }
    }

    private void RemoveDetails( ItemsGeneratorNode node, int nodeStartIndex, int nodeEndIndex, int replaceCount )
    {
      if( ( node.Details == null ) || ( node.Details.Count == 0 ) )
        return;

      var removeCount = nodeEndIndex - nodeStartIndex + 1 - replaceCount;
      //Note: If a replace was invoked, replace count will be greater than 0, and will be used to properly re-offset the 
      //details beyond the initial remove range.

      //Note2: for the case of a move or remove, the replace count must remain 0, so that the other details are correctly offseted.

      var keys = new int[ node.Details.Count ];
      node.Details.Keys.CopyTo( keys, 0 );

      Array.Sort<int>( keys );

      var countDetailsRemoved = 0;

      foreach( int key in keys )
      {
        //if the key is below the remove range, do not do anything with the dictionary entry

        //If the key match the remove range, remove the dictionary entry ( clear and queue remap )
        if( ( key >= nodeStartIndex ) && ( key <= nodeEndIndex ) )
        {
          List<DetailGeneratorNode> details;
          if( node.Details.TryGetValue( key, out details ) )
          {
            //sum them
            foreach( var detailNode in details )
            {
              countDetailsRemoved += detailNode.ItemCount;
            }
            details.Clear(); //note: detail generators will be "closed" by another section of code (Remap floating details).

            node.Details.Remove( key );

            if( node.Details.Count == 0 )
            {
              node.Details = null;
            }
          }
          else
          {
            //Key not found in the dictionary, something wrong is going on.
            throw DataGridException.Create<DataGridInternalException>( "Key not found in Details dictionary within the remove range.", m_dataGridControl );
          }
        }
        //If the key is above the remove range, re-key it appropriatly.
        else if( key > nodeEndIndex )
        {
          List<DetailGeneratorNode> details;
          if( node.Details.TryGetValue( key, out details ) )
          {
            node.Details.Remove( key );
            node.Details.Add( key - removeCount, details );
          }
          else
          {
            //Key not found in the dictionary, something wrong is going on.
            throw DataGridException.Create<DataGridInternalException>( "Key not found in Details dictionary above the remove range.", m_dataGridControl );
          }
        }
      }

      //if some details have been "disconnected"
      if( countDetailsRemoved > 0 )
      {
        node.AdjustItemCount( -countDetailsRemoved );
      }
    }

    private static int ComputeDetailsCount( ItemsGeneratorNode node, int startIndex, int endIndex )
    {
      int detailCount = 0;

      if( node.Details != null )
      {
        //add the detail grids for the items into the calculation of what to remove.
        for( int i = startIndex; i <= endIndex; i++ )
        {
          List<DetailGeneratorNode> details;
          if( node.Details.TryGetValue( i, out details ) )
          {
            foreach( var detailNode in details )
            {
              detailCount += detailNode.ItemCount;
            }
          }
        }
      }

      return detailCount;
    }

    private void HandleItemReset( ItemsGeneratorNode node )
    {
      Debug.Assert( node != null );

      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_HandleItemReset ) )
      {
        var oldItems = new List<object>();
        var newItems = new IListWrapper( node.Items );
        var oldItemsFound = new HashSet<object>();

        for( int i = 0; i < m_genPosToNode.Count; i++ )
        {
          var currentNode = m_genPosToNode[ i ];
          var currentItem = default( object );

          if( currentNode == node )
          {
            currentItem = m_genPosToItem[ i ];
          }
          // Since some details of an unrealized item may be realized, we need to consider the unrealized
          // item as if it was realized or the details containers may not be properly ordered in the
          // internal data structure.
          else if( currentNode is DetailGeneratorNode )
          {
            currentItem = ( ( DetailGeneratorNode )currentNode ).DetailContext.ParentItem;
          }
          else
          {
            continue;
          }

          Debug.Assert( currentItem != null );
          if( !oldItemsFound.Add( currentItem ) )
            continue;

          oldItems.Add( currentItem );
        }

        ICollection<object> itemsRemoved;
        ICollection<object> itemsMoved;

        if( this.IsDataVirtualized( node ) )
        {
          // Since the items collection is doning data virtualization, we do not want to enumerate items.
          // Instead, we will simply remove the containers that were linked to old items to requery the items
          // on demand.
          itemsRemoved = new HashSet<object>( oldItems );
          itemsMoved = new HashSet<object>();
        }
        else
        {
          itemsRemoved = new HashSet<object>();
          itemsMoved = new HashSet<object>();

          // Since the old items collection contains only items that are realized, we must be aware
          // that the collections for the items moved or removed are for realized items only.  Other
          // items may have moved or were removed and we are not aware of it.
          CustomItemContainerGenerator.FindChanges( oldItems, newItems, null, itemsRemoved, itemsMoved );
        }

        this.ApplyItemChanges( node, newItems, itemsRemoved, itemsMoved );
      }
    }

    private void HandleGlobalItemsReset()
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_HandleGlobalItemsReset ) )
      {
        this.ForceReset = false;

        if( this.IsHandlingGlobalItemsResetLocally )
        {
          this.TraceEvent( TraceEventType.Warning, DataGridTraceEventId.CustomItemContainerGenerator_HandleGlobalItemsReset, DataGridTraceMessages.CannotProcessOnReset );
          return;
        }

        if( m_startNode == null )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_HandleGlobalItemsReset, DataGridTraceMessages.EmptyTree );
          return;
        }

        using( this.SetIsHandlingGlobalItemsResetLocally() )
        {
          this.RemoveAllGeneratedItems();

          //No need to clean any more Master/Detail stuff, effectivelly, the call to ClearItems() below will clean the nodes themselves...
          //generator will then be able to remap.

          //if there are items to start with!!!
          if( m_firstItem != null )
          {
            m_dataGridControl.SaveDataGridContextState( m_dataGridContext, true, int.MaxValue );

            //requeue all opened details for remapping!
            foreach( object item in m_masterToDetails.Keys )
            {
              this.QueueDetailItemForRemapping( item );
            }

            //then clear the items nodes
            this.ClearItems();
            this.RefreshGroupsCollection();

            //increment the generation count... to ensure that further calls to the index based functions are not messed up.
            this.IncrementCurrentGenerationCount();

            //Note: There is no need to Clear the recyclingManager since this only represents a reset of the Items (not the headers and footers nodes)
            //and that the list of containers to be recycled is not "invalidated"

          }

          this.SendResetEvent();
        }
      }
    }

    private void ApplyItemChanges(
      ItemsGeneratorNode node,
      IList<object> newItems,
      ICollection<object> itemsRemoved,
      ICollection<object> itemsMoved )
    {
      Debug.Assert( node != null );
      Debug.Assert( newItems != null );
      Debug.Assert( itemsRemoved != null );
      Debug.Assert( itemsMoved != null );

      // IMPORTANT: The items removed and moved collection contains realized items that have moved
      //            or have been removed.  It is possible that some of the items that were not
      //            realized have moved or have been removed and we are unaware of it.  We must
      //            consider this possibility in the following algorithm.

      var detailEntries = node.Details;
      var detailNodes = new HashSet<GeneratorNode>();
      var detailNodesByItem = new Dictionary<object, List<DetailGeneratorNode>>();
      var isDataVirtualized = this.IsDataVirtualized( node );

      // The first thing to do is to remove all details that are linked to items that are not in the final result set.
      if( detailEntries != null )
      {
        var itemCount = 0;

        foreach( var detailEntry in detailEntries.ToList() )
        {
          var i = 0;
          var detailList = detailEntry.Value;

          while( i < detailList.Count )
          {
            var detailNode = detailList[ i ];
            var masterItem = detailNode.DetailContext.ParentItem;

            // Since the items removed collection is for realized item only, we must take a look
            // at the final result set in case the item was unrealized and removed.
            if( isDataVirtualized || itemsRemoved.Contains( masterItem ) || !newItems.Contains( masterItem ) )
            {
              itemCount += detailNode.ItemCount;
              detailList.RemoveAt( i );

              // Put the detail aside and close it properly later.
              this.RemoveGeneratedItems( detailNode, null );
              this.QueueDetailItemForRemapping( masterItem );
            }
            else
            {
              // Keep a list of active detail nodes and their order.  We will need those later in the algorithm.
              detailNodes.Add( detailNode );

              var detailNodesList = default( List<DetailGeneratorNode> );
              if( !detailNodesByItem.TryGetValue( masterItem, out detailNodesList ) )
              {
                detailNodesList = new List<DetailGeneratorNode>( 1 );
                detailNodesByItem.Add( masterItem, detailNodesList );
              }

              Debug.Assert( detailNodesList != null );
              detailNodesList.Add( detailNode );

              i++;
            }
          }

          if( detailList.Count <= 0 )
          {
            detailEntries.Remove( detailEntry.Key );
          }
        }

        if( detailEntries.Count <= 0 )
        {
          node.Details = null;
        }

        node.AdjustItemCount( -itemCount );
      }

      // The next thing to do is to remove the containers for the realized items that were removed.
      if( itemsRemoved.Count > 0 )
      {
        for( int i = m_genPosToItem.Count - 1; i >= 0; i-- )
        {
          if( !itemsRemoved.Contains( m_genPosToItem[ i ] ) )
            continue;

          this.RemoveGeneratedItem( i, null );
        }
      }

      // Now that everything that needed to be removed was removed, we will repair the collection
      // that is used as a link between items and detail nodes.
      if( detailNodes.Count > 0 )
      {
        Debug.Assert( detailEntries != null );
        Debug.Assert( detailEntries == node.Details );
        Debug.Assert( !isDataVirtualized );

        detailEntries.Clear();

        for( int i = 0; i < newItems.Count; i++ )
        {
          var masterItem = newItems[ i ];
          var detailNodesList = default( List<DetailGeneratorNode> );

          if( detailNodesByItem.TryGetValue( masterItem, out detailNodesList ) )
          {
            detailNodesByItem.Remove( masterItem );
            detailEntries.Add( i, detailNodesList );
          }
        }

        Debug.Assert( ( detailNodesByItem.Count == 0 ), "The detail nodes should have been reinserted properly." );
      }
      else
      {
        Debug.Assert( ( detailEntries == null ) || ( detailEntries.Count == 0 ) );
        Debug.Assert( node.Details == null );
      }

      // The next thing to do is to reorder the containers within the internal data structure so it match
      // the ordering of the realized items that have moved.
      if( itemsMoved.Count > 0 )
      {
        Debug.Assert( !isDataVirtualized );

        var insertionIndex = default( int? );
        var itemGenPosEntries = new Dictionary<object, DependencyObject>();
        var detailGenPosEntries = new Dictionary<GeneratorNode, IList<Tuple<object, DependencyObject>>>();

        // Remove the entries within the data structure that are linked to the current node or one of its detail.
        {
          var i = 0;

          while( i < m_genPosToNode.Count )
          {
            var currentNode = m_genPosToNode[ i ];

            // We have found a container that is linked to one of the data item.
            if( currentNode == node )
            {
              itemGenPosEntries.Add( m_genPosToItem[ i ], m_genPosToContainer[ i ] );
            }
            // We have found a container that is part of a detail that is linked to one of the data item.
            else if( detailNodes.Contains( currentNode ) )
            {
              var detailInfos = default( IList<Tuple<object, DependencyObject>> );

              if( !detailGenPosEntries.TryGetValue( currentNode, out detailInfos ) )
              {
                detailInfos = new List<Tuple<object, DependencyObject>>();
                detailGenPosEntries.Add( currentNode, detailInfos );
              }

              Debug.Assert( detailInfos != null );
              detailInfos.Add( new Tuple<object, DependencyObject>( m_genPosToItem[ i ], m_genPosToContainer[ i ] ) );
            }
            // The container as nothing to do with the node we are looking for or one of its detail.
            else
            {
              i++;
              continue;
            }

            // Remove the entries from the internal data structure.
            this.GenPosArraysRemoveAt( i );

            // Keep a reference on the first location a removal occurred.
            if( !insertionIndex.HasValue )
            {
              insertionIndex = i;
            }
          }
        }

        // We must reinsert all entries that have been removed in the last step in appropriate order.
        if( insertionIndex.HasValue )
        {
          var index = insertionIndex.Value;

          for( int i = 0; i < newItems.Count; i++ )
          {
            var currentItem = newItems[ i ];
            var currentContainer = default( DependencyObject );

            if( itemGenPosEntries.TryGetValue( currentItem, out currentContainer ) )
            {
              itemGenPosEntries.Remove( currentItem );

              // The value inserted as the index is not important and will be updated at the last step of the algorithmn.
              m_genPosToContainer.Insert( index, currentContainer );
              m_genPosToIndex.Insert( index, int.MinValue );
              m_genPosToItem.Insert( index, currentItem );
              m_genPosToNode.Insert( index, node );

              index++;
            }

            if( ( detailEntries != null ) && ( detailEntries.Count > 0 ) )
            {
              var detailNodesList = default( List<DetailGeneratorNode> );

              if( detailEntries.TryGetValue( i, out detailNodesList ) && ( detailNodesList != null ) )
              {
                foreach( var detailNode in detailNodesList )
                {
                  var detailInfos = default( IList<Tuple<object, DependencyObject>> );
                  if( !detailGenPosEntries.TryGetValue( detailNode, out detailInfos ) )
                    continue;

                  detailGenPosEntries.Remove( detailNode );

                  foreach( var detailInfo in detailInfos )
                  {
                    // The value inserted as the index is not important and will be updated at the last step of the algorithmn.
                    m_genPosToContainer.Insert( index, detailInfo.Item2 );
                    m_genPosToIndex.Insert( index, int.MinValue );
                    m_genPosToItem.Insert( index, detailInfo.Item1 );
                    m_genPosToNode.Insert( index, detailNode );

                    index++;
                  }
                }
              }
            }
          }
        }

        Debug.Assert( ( itemGenPosEntries.Count == 0 ), "The containers should have been reinserted properly." );
        Debug.Assert( ( detailGenPosEntries.Count == 0 ), "The detail containers should have been reinserted properly." );
      }

      // The last thing to do is to update the indexes and raise a notification.
      this.IncrementCurrentGenerationCount();
      this.SendResetEvent();
    }

    private void ApplyDetailChanges(
      DetailGeneratorNode node,
      IList<DependencyObject> newContainers,
      ICollection<DependencyObject> containersRemoved,
      ICollection<DependencyObject> containersMoved )
    {
      Debug.Assert( node != null );
      Debug.Assert( newContainers != null );
      Debug.Assert( containersRemoved != null );
      Debug.Assert( containersMoved != null );

      // The first thing to do is to remove the containers for the realized items that were removed.
      if( containersRemoved.Count > 0 )
      {
        for( int i = m_genPosToContainer.Count - 1; i >= 0; i-- )
        {
          if( !containersRemoved.Contains( m_genPosToContainer[ i ] ) )
            continue;

          this.RemoveGeneratedItem( i, null );
        }
      }

      // The next thing to do is to reorder the containers within the internal data structure so it match
      // the ordering of the realized containers that have moved.
      if( containersMoved.Count > 0 )
      {
        var insertionIndex = default( int? );
        var entries = new Dictionary<DependencyObject, object>();

        // Remove the entries within the data structure that are linked to the detail node.
        {
          var i = 0;

          while( i < m_genPosToNode.Count )
          {
            var currentNode = m_genPosToNode[ i ];

            // We have found a container that is linked to the desired node.
            if( currentNode == node )
            {
              entries.Add( m_genPosToContainer[ i ], m_genPosToItem[ i ] );

              // Remove the entries from the internal data structure.
              this.GenPosArraysRemoveAt( i );

              // Keep a reference on the first location a removal occurred.
              if( !insertionIndex.HasValue )
              {
                insertionIndex = i;
              }
            }
            // The container as nothing to do with the node we are looking.
            else
            {
              i++;
            }
          }
        }

        // We must reinsert all entries that have been removed in the last step in appropriate order.
        if( insertionIndex.HasValue )
        {
          var index = insertionIndex.Value;

          for( int i = 0; i < newContainers.Count; i++ )
          {
            var currentContainer = newContainers[ i ];
            var currentItem = default( object );

            if( entries.TryGetValue( currentContainer, out currentItem ) )
            {
              entries.Remove( currentContainer );

              // The value inserted as the index is not important and will be updated at the last step of the algorithmn.
              m_genPosToContainer.Insert( index, currentContainer );
              m_genPosToIndex.Insert( index, int.MinValue );
              m_genPosToItem.Insert( index, currentItem );
              m_genPosToNode.Insert( index, node );

              index++;
            }
          }
        }

        Debug.Assert( ( entries.Count == 0 ), "The containers should have been reinserted properly." );
      }

      // The last thing to do is to update the indexes and raise a notification.
      this.IncrementCurrentGenerationCount();
      this.SendResetEvent();
    }

    internal void EnsureNodeTreeCreated()
    {
      //This is done to ensure that any public interface function called get an "up to date" generator content when being accessed.

      // No reentrency is allowed for this method
      // to avoid any problem.
      if( this.IsNodeTreeValid || this.IsEnsuringNodeTreeCreated )
        return;

      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_EnsureNodeTreeCreated ) )
      using( this.SetIsEnsuringNodeTreeCreated() )
      {
        if( m_startNode == null )
        {
          int addCount;

          this.UpdateHeaders( m_dataGridContext.Headers );
          this.SetupInitialItemsNodes( out addCount );
          this.UpdateFooters( m_dataGridContext.Footers );

          this.IncrementCurrentGenerationCount();
        }
        else if( m_firstItem == null )
        {
          this.HandleItemsRecreation();
        }

        this.EnsureDetailsNodeTreeCreated();
        this.RemapFloatingDetails();

        m_dataGridControl.RestoreDataGridContextState( m_dataGridContext );
      }

      // The current method will need to be called again.  The data source is most probably not set yet.
      if( ( m_startNode == null ) || ( m_firstItem == null ) || ( m_floatingDetails.Count != 0 ) )
      {
        this.InvalidateNodeTree();
      }
    }

    private void EnsureDetailsNodeTreeCreated()
    {
      if( m_masterToDetails.Count == 0 )
        return;

      Debug.Assert( this.IsEnsuringNodeTreeCreated, "The method EnsureDetailsNodeTreeCreated should have been called from EnsureNodeTreeCreated." );

      foreach( var generator in this.GetDetailGenerators().ToList() )
      {
        generator.EnsureNodeTreeCreated();
      }
    }

    private void HandleItemsRecreation()
    {
      //Then, since its a Reset, recreates the Items nodes.
      int resetAddCount;

      var addNode = this.SetupInitialItemsNodes( out resetAddCount );
      if( addNode != null )
      {
        this.IncrementCurrentGenerationCount();
      }
    }

    private void UpdateGenPosToIndexList()
    {
      if( m_startNode == null )
        return;

      //after the modification to have the item count stored "locally" in the DetailGeneratorNodes,
      //it becomes important to have the nodes updated when the layout of the items changes.
      foreach( var detailNode in this.GetDetailGeneratorNodes() )
      {
        detailNode.UpdateItemCount();
      }

      var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
      var cachedDetailNode = default( DetailGeneratorNode );
      var cachedDetailNodeIndex = -1;
      var previousDetailIndex = -1;

      //loop through the realized items. By design they are present sequentially in the linked list, so I do not need to reset the GeneratorNodeHelper
      for( int i = 0; i < m_genPosToItem.Count; i++ )
      {
        var item = m_genPosToItem[ i ];
        var node = m_genPosToNode[ i ];
        var detailNode = node as DetailGeneratorNode;

        int index;

        if( detailNode == null )
        {
          if( !nodeHelper.FindNode( node ) )
            throw DataGridException.Create<DataGridInternalException>( "Realized item not found.", m_dataGridControl );

          index = nodeHelper.FindItem( item );
          if( index < 0 )
            throw DataGridException.Create<DataGridInternalException>( "Realized item not found.", m_dataGridControl );

          var itemsNode = nodeHelper.CurrentNode as ItemsGeneratorNode;
          if( itemsNode != null )
          {
            index = nodeHelper.Index + itemsNode.IndexOf( item );
          }
        }
        else
        {
          if( cachedDetailNode != detailNode )
          {
            cachedDetailNodeIndex = this.FindGlobalIndexForDetailNode( detailNode );
            cachedDetailNode = detailNode;
            previousDetailIndex = -1;
          }

          var detailIndex = detailNode.DetailGenerator.IndexFromRealizedItem( item, previousDetailIndex + 1, out previousDetailIndex );
          if( detailIndex < 0 )
            throw DataGridException.Create<DataGridInternalException>( "Realized item not found at detail level.", m_dataGridControl );

          index = cachedDetailNodeIndex + detailIndex;
        }

        if( ( i > 0 ) && ( index <= m_genPosToIndex[ i - 1 ] ) )
          throw DataGridException.Create<DataGridInternalException>( "Realized item re-indexed at wrong location.", m_dataGridControl );

        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_UpdateGenPosToIndexList, DataGridTraceMessages.IndexUpdated, DataGridTraceArgs.Container( m_genPosToContainer[ i ] ), DataGridTraceArgs.Node( m_genPosToNode[ i ] ), DataGridTraceArgs.Item( m_genPosToItem[ i ] ), DataGridTraceArgs.GeneratorIndex( i ), DataGridTraceArgs.From( m_genPosToIndex[ i ] ), DataGridTraceArgs.To( index ) );

        m_genPosToIndex[ i ] = index;
      }
    }

    private int IndexFromRealizedItem( object referenceItem, int startIndex, out int foundIndex )
    {
      int retval = -1;
      foundIndex = startIndex;

      for( int i = startIndex; i < m_genPosToIndex.Count; i++ )
      {
        object item = m_genPosToItem[ i ];

        if( item == referenceItem )
        {
          foundIndex = i;
          retval = m_genPosToIndex[ i ];
          break;
        }
      }

      return retval;
    }

    private int FindGlobalIndexForDetailNode( DetailGeneratorNode detailNode )
    {
      foreach( var masterItemToDetails in m_masterToDetails )
      {
        var detailsNodes = masterItemToDetails.Value;
        var detailIndex = detailsNodes.IndexOf( detailNode );
        if( detailIndex < 0 )
          continue;

        var parentItem = masterItemToDetails.Key;
        var offset = detailsNodes.Take( detailIndex ).Sum( item => item.ItemCount - 1 );

        var index = m_genPosToItem.IndexOf( parentItem );

        if( index >= 0 )
        {
          if( m_genPosToNode[ index ] is ItemsGeneratorNode )
            return offset + detailIndex + m_genPosToIndex[ index ] + 1;
        }

        // Look for parent item index the old way.
        var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );

        var parentItemIndex = nodeHelper.FindItem( parentItem );
        if( parentItemIndex < 0 )
          throw DataGridException.Create<DataGridInternalException>( "Master item index is out of bound.", m_dataGridControl );

        return offset + detailIndex + parentItemIndex + 1;
      }

      return -1;
    }

    private void StartGenerator( GeneratorPosition startPos, GeneratorDirection direction )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<InvalidOperationException>( "Cannot perform this operation while the generator is busy generating items.", m_dataGridControl );

      m_generatorStatus = GeneratorStatus.GeneratingContainers;
      m_generatorDirection = direction;
      m_generatorCurrentGlobalIndex = this.IndexFromGeneratorPosition( startPos );

      var itemCount = this.ItemCount;
      if( ( m_generatorCurrentGlobalIndex < 0 ) || ( m_generatorCurrentGlobalIndex >= itemCount ) )
        return;

      //and create a node helper that will assist us during the Generation process...
      m_generatorNodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 ); // start index is always 0

      //position the GeneratorNodeHelper to the appropriate node
      if( !m_generatorNodeHelper.FindNodeForIndex( m_generatorCurrentGlobalIndex ) ) //find index?!?!
        //there was a problem moving the Node helper... 
        throw DataGridException.Create<DataGridInternalException>( "Unable to move to the required generator node by using the current index.", m_dataGridControl );

      //Calculate the offset
      m_generatorCurrentOffset = m_generatorCurrentGlobalIndex - m_generatorNodeHelper.Index;

      var itemsNode = m_generatorNodeHelper.CurrentNode as ItemsGeneratorNode;
      if( itemsNode != null )
      {
        m_generatorCurrentDetail = itemsNode.GetDetailNodeForIndex( m_generatorCurrentOffset, out m_generatorCurrentOffset, out m_generatorCurrentDetailIndex, out m_generatorCurrentDetailNodeIndex );
      }

      if( m_generatorCurrentDetail != null )
      {
        // No detail generator should be started already.
        if( m_generatorCurrentDetailDisposable != null )
          throw DataGridException.Create<DataGridInternalException>( "The detail generator is already started.", m_dataGridControl );

        m_generatorCurrentDetailDisposable = ( ( IItemContainerGenerator )m_generatorCurrentDetail.DetailGenerator ).StartAt( m_generatorCurrentDetail.DetailGenerator.GeneratorPositionFromIndex( m_generatorCurrentDetailIndex ), direction, true );
      }
    }

    private void StopGenerator()
    {
      if( this.Status != GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<DataGridInternalException>( "Cannot perform this operation while the generator is busy generating items.", m_dataGridControl );

      m_generatorNodeHelper = null;
      m_generatorCurrentOffset = -1;
      m_generatorCurrentGlobalIndex = -1;

      m_generatorCurrentDetailIndex = -1;
      m_generatorCurrentDetailNodeIndex = -1;

      m_generatorCurrentDetail = null;

      if( m_generatorCurrentDetailDisposable != null )
      {
        try
        {
          m_generatorCurrentDetailDisposable.Dispose();
          m_generatorCurrentDetailDisposable = null;
        }
        catch( Exception e )
        {
          m_generatorStatus = GeneratorStatus.Error;

          throw new DataGridInternalException( "The generator failed to stop.", e, m_dataGridControl );
        }
      }

      m_generatorStatus = GeneratorStatus.ContainersGenerated;
    }

    private GeneratorPosition FindNextUnrealizedGeneratorPosition( GeneratorPosition position )
    {
      var retval = position;
      var index = this.IndexFromGeneratorPosition( position ) + 1;

      while( index < this.ItemCount )
      {
        if( m_genPosToIndex.Contains( index ) )
        {
          retval.Index++;
        }
        else
        {
          retval.Offset++;
          break;
        }

        index++;
      }

      return retval;
    }

    private GeneratorPosition FindPreviousUnrealizedGeneratorPosition( GeneratorPosition position )
    {
      var index = this.IndexFromGeneratorPosition( position ) - 1;
      while( index >= 0 )
      {
        if( !m_genPosToIndex.Contains( index ) )
          break;

        index--;
      }

      return this.GeneratorPositionFromIndex( index );
    }

    private DependencyObject CreateContainerForItem( object dataItem, GeneratorNode node )
    {
      var container = default( DependencyObject );

      if( node is HeadersFootersGeneratorNode )
      {
        container = this.CreateHeaderFooterContainer( dataItem );

        if( node.Parent == null )
        {
          GroupLevelIndicatorPane.SetGroupLevel( container, -1 );
        }
        else
        {
          GroupLevelIndicatorPane.SetGroupLevel( container, node.Level );
        }

        this.SetStatContext( container, node );
      }
      else if( node is ItemsGeneratorNode )
      {
        //ensure that item is not its own container...
        if( !this.IsItemItsOwnContainer( dataItem ) )
        {
          container = this.CreateNextItemContainer( dataItem );
        }
        else
        {
          container = dataItem as DependencyObject;
        }

        GroupLevelIndicatorPane.SetGroupLevel( container, node.Level );
      }
      else
      {
        throw DataGridException.Create<DataGridInternalException>( "Cannot create container for the GeneratorNode, as it is not of a valid type.", m_dataGridControl );
      }

      if( container == null )
        throw DataGridException.Create<DataGridInternalException>( "A container could not be created or recycled for the GeneratorNode.", m_dataGridControl );

      var dataItemStore = container.ReadLocalValue( CustomItemContainerGenerator.DataItemPropertyProperty ) as DataItemDataProviderBase;
      if( dataItemStore != null )
      {
        dataItemStore.SetDataItem( dataItem );
      }
      else
      {
        dataItemStore = new DataItemDataProvider();
        dataItemStore.SetDataItem( dataItem );

        CustomItemContainerGenerator.SetDataItemProperty( container, dataItemStore );
      }

      DataGridControl.SetDataGridContext( container, m_dataGridContext );

      return container;
    }

    private void SetStatContext( DependencyObject container, GeneratorNode node )
    {
      var parentGroup = node.Parent as GroupGeneratorNode;
      var collectionViewGroup = default( DataGridCollectionViewGroup );

      if( parentGroup != null )
      {
        collectionViewGroup = parentGroup.CollectionViewGroup as DataGridCollectionViewGroup;
      }
      else if( m_dataGridContext != null )
      {
        var collectionView = m_dataGridContext.ItemsSourceCollection as DataGridCollectionViewBase;
        if( collectionView != null )
        {
          collectionViewGroup = collectionView.RootGroup as DataGridCollectionViewGroup;
        }
      }

      if( collectionViewGroup != null )
      {
        container.SetValue( DataGridControl.StatContextPropertyKey, collectionViewGroup );
      }
    }

    private void ClearStatContext( DependencyObject container )
    {
      if( GroupLevelIndicatorPane.GetGroupLevel( container ) == -1 )
      {
        container.ClearValue( GroupLevelIndicatorPane.GroupLevelProperty );
      }

      if( container is HeaderFooterItem )
      {
        container.ClearValue( DataGridControl.StatContextPropertyKey );
      }
    }

    private bool IsItemItsOwnContainer( object dataItem )
    {
      return m_dataGridControl.IsItemItsOwnContainer( dataItem );
    }

    private DependencyObject CreateHeaderFooterContainer( object dataItem )
    {
      if( this.IsRecyclingEnabled )
      {
        var recycledContainer = this.DequeueHeaderFooterContainer( dataItem );
        if( recycledContainer != null )
          return recycledContainer;
      }

      //If the container cannot be recycled, then create a new one.
      var realDataItem = dataItem;
      if( dataItem.GetType() == typeof( GroupHeaderFooterItem ) )
      {
        realDataItem = ( ( GroupHeaderFooterItem )dataItem ).Template;
      }

      var template = realDataItem as DataTemplate;
      if( template == null )
      {
        var vwc = realDataItem as GroupHeaderFooterItemTemplate;
        if( vwc != null )
        {
          vwc.Seal();
          template = vwc.Template;
        }

        if( template == null )
          throw DataGridException.Create<DataGridInternalException>( "No template found for the creation of a header or footer container.", m_dataGridControl );
      }

      var newItem = new HeaderFooterItem();

      BindingOperations.SetBinding( newItem, HeaderFooterItem.ContentProperty, m_headerFooterDataContextBinding );
      newItem.ContentTemplate = template;

      return newItem;
    }

    private int FindInsertionPoint( int itemIndex )
    {
      var count = m_genPosToIndex.Count;

      for( var i = 0; i < count; i++ )
      {
        //if the item is larger in index, then I want to insert before it!
        if( m_genPosToIndex[ i ] > itemIndex )
          return i;
      }

      return count;
    }

    private void RemapFloatingDetails()
    {
      if( m_floatingDetails.Count == 0 )
        return;

      if( this.IsDetailsRemapDeferred )
      {
        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_RemapFloatingDetails, DataGridTraceMessages.CannotRemapDetails );
        return;
      }

      Debug.Assert( m_startNode != null, "Generator structure should already be created." );

      while( m_floatingDetails.Count > 0 )
      {
        var closeDetail = true; //using this approach to have a default behavior or closing the details, if something inconsistent occur (see Debug Asserts).
        var itemToCheck = m_floatingDetails[ 0 ];
        m_floatingDetails.RemoveAt( 0 );

        var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
        if( nodeHelper.AbsoluteFindItem( itemToCheck ) )
        {
          var itemsNode = nodeHelper.CurrentNode as ItemsGeneratorNode;
          if( itemsNode != null )
          {
            var insertionIndex = itemsNode.Items.IndexOf( itemToCheck );
            if( insertionIndex < 0 )
            {
              this.TraceEvent( TraceEventType.Critical, DataGridTraceEventId.CustomItemContainerGenerator_RemapFloatingDetails, DataGridTraceMessages.ItemNotBelongingToNode, DataGridTraceArgs.Item( itemToCheck ), DataGridTraceArgs.Node( itemsNode ) );
            }

            if( itemsNode.Details == null )
            {
              itemsNode.Details = new SortedDictionary<int, List<DetailGeneratorNode>>();
            }

            try
            {
              this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_RemapFloatingDetails, DataGridTraceMessages.RemapDetailNodes, DataGridTraceArgs.Item( itemToCheck ) );

              var oldItemCount = this.ItemCount;

              itemsNode.Details.Add( insertionIndex, new List<DetailGeneratorNode>( m_masterToDetails[ itemToCheck ] ) );

              var addCount = 0;
              foreach( var detailNode in m_masterToDetails[ itemToCheck ] )
              {
                // Refresh the item count in case it changed while the detail was floating.
                detailNode.UpdateItemCount();

                addCount += detailNode.ItemCount;
              }
              itemsNode.AdjustItemCount( addCount );

              this.IncrementCurrentGenerationCount();

              // The ItemsChanged event should not be raised if the new items are hidden under a collapsed group, or else the master node's ItemCount will become unbalanced.
              var newItemCount = this.ItemCount;
              if( oldItemCount != newItemCount )
              {
                //ensure that no add event is sent when the generator is currently processing a Reset.
                if( !this.IsHandlingGlobalItemsReset )
                {
                  this.SendAddEvent( addCount );
                }
              }

              closeDetail = false;
            }
            catch( Exception e )
            {
              //both the "Add" and the " [] " could throw if the key is already present or if its not...
              throw new DataGridInternalException( e.Message, e, m_dataGridControl );
            }
          }
          else
          {
            this.TraceEvent( TraceEventType.Warning, DataGridTraceEventId.CustomItemContainerGenerator_RemapFloatingDetails, DataGridTraceMessages.UnexpectedNode, DataGridTraceArgs.Node( nodeHelper.CurrentNode ) );
          }
        }

        if( closeDetail )
        {
          //The item was not located in the generator, then it's time to "close" the detail node/generator
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_RemapFloatingDetails, DataGridTraceMessages.CollapsingDetail, DataGridTraceArgs.Item( itemToCheck ) );
          this.CloseDetailsForItem( itemToCheck, null );
        }
      }
    }

    private void QueueDetailItemForRemapping( object item )
    {
      if( item == null )
        return;

      if( !m_masterToDetails.ContainsKey( item ) || m_floatingDetails.Contains( item ) )
        return;

      m_floatingDetails.Add( item );

      this.InvalidateNodeTree();
    }

    private void CloseDetails( DetailConfiguration detailConfiguration )
    {
      var details = new KeyValuePair<object, List<DetailGeneratorNode>>[ m_masterToDetails.Count ];
      ( ( ICollection<KeyValuePair<object, List<DetailGeneratorNode>>> )m_masterToDetails ).CopyTo( details, 0 );

      foreach( var detail in details )
      {
        this.CloseDetailsForItem( detail.Key, detailConfiguration );
      }
    }

    private int CloseDetailsForItem( object dataItem, DetailConfiguration detailConfiguration )
    {
      if( m_generatorStatus == GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<DataGridInternalException>( "Cannot perform this operation while the generator is busy generating items", m_dataGridControl );

      if( dataItem is EmptyDataItem )
      {
        this.TraceEvent( TraceEventType.Error, DataGridTraceEventId.CustomItemContainerGenerator_CloseDetailsForItem, DataGridTraceMessages.CannotCollapseDetail, DataGridTraceArgs.Item( dataItem ) );
        return -1;
      }

      if( ( dataItem == null ) || ( !m_masterToDetails.ContainsKey( dataItem ) ) )
        return -1; //the item is no longer present in the opened details list... or item is invalid (null)

      var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
      var globalItemIndex = nodeHelper.FindItem( dataItem );
      var dataItemFound = ( globalItemIndex >= 0 );
      //Note: this function will only return index for items directly contained in this generator ( no details ).

      ItemsGeneratorNode masterNode;

      if( dataItemFound )
      {
        masterNode = nodeHelper.CurrentNode as ItemsGeneratorNode;
      }
      else
      {
        nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
        masterNode = ( nodeHelper.Contains( dataItem ) ) ? nodeHelper.CurrentNode as ItemsGeneratorNode : null;
      }

      List<DetailGeneratorNode> oldDetails;
      if( !m_masterToDetails.TryGetValue( dataItem, out oldDetails ) )
      {
        this.TraceEvent( TraceEventType.Critical, DataGridTraceEventId.CustomItemContainerGenerator_CloseDetailsForItem, DataGridTraceMessages.DetailNotFound, DataGridTraceArgs.Item( dataItem ) );
        throw DataGridException.Create<DataGridInternalException>( "Detail not found", m_dataGridControl );
      }

      var count = 0;
      var containers = new List<DependencyObject>();

      for( int i = oldDetails.Count - 1; i >= 0; i-- )
      {
        var detailNode = oldDetails[ i ];

        if( ( detailConfiguration == null ) || ( detailConfiguration == detailNode.DetailContext.SourceDetailConfiguration ) )
        {
          count += detailNode.ItemCount;

          this.RemoveGeneratedItems( detailNode, containers );
          this.ClearDetailGeneratorNode( detailNode );

          oldDetails.RemoveAt( i );
        }
      }

      if( oldDetails.Count == 0 )
      {
        m_masterToDetails.Remove( dataItem );
        m_floatingDetails.Remove( dataItem );
      }

      if( ( masterNode != null ) && ( masterNode.Details != null ) )
      {
        var indexOfItem = masterNode.Items.IndexOf( dataItem );

        if( !masterNode.Details.TryGetValue( indexOfItem, out oldDetails ) )
        {
          this.TraceEvent( TraceEventType.Critical, DataGridTraceEventId.CustomItemContainerGenerator_CloseDetailsForItem, DataGridTraceMessages.DetailNotFound, DataGridTraceArgs.Item( dataItem ) );
          throw DataGridException.Create<DataGridInternalException>( "Detail not found .. 2", m_dataGridControl );
        }

        for( int i = oldDetails.Count - 1; i >= 0; i-- )
        {
          var detailNode = oldDetails[ i ];

          if( ( detailConfiguration == null ) || ( detailConfiguration == detailNode.DetailContext.SourceDetailConfiguration ) )
          {
            oldDetails.RemoveAt( i );
          }
        }

        if( oldDetails.Count == 0 )
        {
          masterNode.Details.Remove( indexOfItem );
        }

        masterNode.AdjustItemCount( -count );

        if( masterNode.Details.Count == 0 )
        {
          masterNode.Details = null;
        }
      }

      this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_CloseDetailsForItem, DataGridTraceMessages.DetailCollapsed, DataGridTraceArgs.Item( dataItem ) );

      if( dataItemFound )
      {
        this.IncrementCurrentGenerationCount();
        this.SendRemoveEvent( count, containers );
      }

      return globalItemIndex;
    }

    private DetailGeneratorNode CreateDetailGeneratorNode( object dataItem, DataGridCollectionViewBase collectionView, DetailConfiguration detailConfiguration )
    {
      var detailDataGridContext = new DataGridContext( m_dataGridContext, m_dataGridControl, dataItem, collectionView, detailConfiguration );
      var detailGenerator = CustomItemContainerGenerator.CreateGenerator( m_dataGridControl, collectionView, detailDataGridContext, this );
      detailGenerator.SetGenPosToIndexUpdateInhibiter( this );
      detailGenerator.IsRecyclingEnabled = this.IsRecyclingEnabled;

      if( m_dataGridControl.AreDetailsFlatten )
      {
        var updateColumnSortCommand = detailDataGridContext.UpdateColumnSortCommand;
        if( updateColumnSortCommand.CanExecute() )
        {
          updateColumnSortCommand.Execute();
        }
      }

      DetailsChangedEventManager.AddListener( detailGenerator, this );

      var detailNode = new DetailGeneratorNode( detailDataGridContext, detailGenerator );

      //register to the ItemsChanged only after the columns were updated (to avoid a reset being received without having completed the initialization ).
      //Also wait after the creation of the DetailGeneratorNode, since messages could be issued before initialization completes.
      detailGenerator.ItemsChanged += new CustomGeneratorChangedEventHandler( this.HandleDetailGeneratorContentChanged );
      detailGenerator.ContainersRemoved += new ContainersRemovedEventHandler( this.OnDetailContainersRemoved );

      return detailNode;
    }

    private void ClearDetailGeneratorNode( DetailGeneratorNode detailNode )
    {
      m_dataGridControl.SelectionChangerManager.Begin();

      try
      {
        m_dataGridControl.SelectionChangerManager.UnselectAllItems( detailNode.DetailContext );
        m_dataGridControl.SelectionChangerManager.UnselectAllCells( detailNode.DetailContext );
      }
      finally
      {
        m_dataGridControl.SelectionChangerManager.End( false, false );
      }

      m_dataGridControl.SaveDataGridContextState( detailNode.DetailContext, true, int.MaxValue );

      var detailGenerator = detailNode.DetailGenerator;

      detailNode.CleanGeneratorNode();

      DetailsChangedEventManager.RemoveListener( detailGenerator, this );
      detailGenerator.ItemsChanged -= new CustomGeneratorChangedEventHandler( this.HandleDetailGeneratorContentChanged );
      detailGenerator.ContainersRemoved -= new ContainersRemovedEventHandler( this.OnDetailContainersRemoved );
      detailGenerator.SetGenPosToIndexUpdateInhibiter( null );
      detailGenerator.UnregisterEvents();
      detailGenerator.ClearEvents();
    }

    private void RegisterEvents()
    {
      Debug.Assert( m_dataGridControl != null );
      Debug.Assert( m_collectionView != null );
      Debug.Assert( m_dataGridContext != null );

      CollectionChangedEventManager.AddListener( m_collectionView, this );
      GroupConfigurationSelectorChangedEventManager.AddListener( m_dataGridContext, this );
      PropertyChangedEventManager.AddListener( m_collectionView, this, string.Empty );

      // The top most generator must register to additional events.
      if( m_dataGridContext.SourceDetailConfiguration == null )
      {
        ItemsSourceChangeCompletedEventManager.AddListener( m_dataGridControl, this );
        ViewChangedEventManager.AddListener( m_dataGridControl, this );
        ThemeChangedEventManager.AddListener( m_dataGridControl, this );

        m_recyclingPools.ContainersRemoved += new ContainersRemovedEventHandler( this.OnRecyclingPoolsContainersRemoved );
        m_recyclingPools.RecyclingCandidatesCleaned += new RecyclingCandidatesCleanedEventHandler( this.OnRecyclingCandidatesCleaned );
      }
    }

    private void UnregisterEvents()
    {
      Debug.Assert( m_dataGridControl != null );
      Debug.Assert( m_collectionView != null );
      Debug.Assert( ( m_dataGridContext != null ) && ( m_dataGridContext.DetailConfigurations != null ) );

      CollectionChangedEventManager.RemoveListener( m_collectionView, this );
      GroupConfigurationSelectorChangedEventManager.RemoveListener( m_dataGridContext, this );
      CollectionChangedEventManager.RemoveListener( m_dataGridContext.DetailConfigurations, this );
      PropertyChangedEventManager.RemoveListener( m_collectionView, this, string.Empty );

      // The top most generator must unregister from additional events.
      if( m_dataGridContext.SourceDetailConfiguration == null )
      {
        ItemsSourceChangeCompletedEventManager.RemoveListener( m_dataGridControl, this );
        ViewChangedEventManager.RemoveListener( m_dataGridControl, this );
        ThemeChangedEventManager.RemoveListener( m_dataGridControl, this );

        m_recyclingPools.ContainersRemoved -= new ContainersRemovedEventHandler( this.OnRecyclingPoolsContainersRemoved );
        m_recyclingPools.RecyclingCandidatesCleaned -= new RecyclingCandidatesCleanedEventHandler( this.OnRecyclingCandidatesCleaned );
      }
    }

    private void ClearEvents()
    {
      this.ContainersRemoved = null;
      this.DetailsChanged = null;
      this.ItemsChanged = null;
      this.PropertyChanged = null;
    }

    internal int CreateDetailsHelper( ItemsGeneratorNode masterNode, object dataItem )
    {
      var dataGridCollectionViewBase = ( m_dataGridContext != null ) ? m_dataGridContext.ItemsSourceCollection as DataGridCollectionViewBase
                                                                     : default( DataGridCollectionViewBase );

      if( dataGridCollectionViewBase == null )
      {
        this.TraceEvent( TraceEventType.Critical, DataGridTraceEventId.CustomItemContainerGenerator_CreateDetailsHelper, DataGridTraceMessages.DetailNotSupported,
                         DataGridTraceArgs.DataSource( dataGridCollectionViewBase ) );
      }

      var totalAddCount = 0;
      var newDetails = default( List<DetailGeneratorNode> );
      var detailsAlreadyExist = m_floatingDetails.Contains( dataItem );

      if( detailsAlreadyExist )
      {
        this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_CreateDetailsHelper, DataGridTraceMessages.RemapDetailNodes,
                         DataGridTraceArgs.Item( dataItem ) );

        newDetails = new List<DetailGeneratorNode>( m_masterToDetails[ dataItem ] );
        m_floatingDetails.Remove( dataItem );
      }

      var detailConfigurations = m_dataGridContext.DetailConfigurations;

      //If the master item was not found in the list of details pending requeuing, then create a new set of detail nodes
      if( newDetails == null )
      {
        newDetails = new List<DetailGeneratorNode>( detailConfigurations.Count );

        foreach( DetailConfiguration detailConfig in detailConfigurations )
        {
          if( detailConfig.IsAutoCreated )
          {
            var defaultDetailConfig = m_dataGridContext.DefaultDetailConfiguration;
            if( defaultDetailConfig == null )
            {
              defaultDetailConfig = m_dataGridContext.GetDefaultDetailConfigurationForContext();
            }

            if( defaultDetailConfig != null )
            {
              //if the default headers footers shall be used, add then to the detail config ( internally, it ensures that it is added only once ).
              if( defaultDetailConfig.UseDefaultHeadersFooters )
              {
                defaultDetailConfig.AddDefaultHeadersFooters();
                detailConfig.AddDefaultHeadersFooters();
              }
            }
            else
            {
              //if the default headers footers shall be used, add then to the detail config ( internally, it ensures that it is added only once ).
              if( detailConfig.UseDefaultHeadersFooters )
              {
                detailConfig.AddDefaultHeadersFooters();
              }
            }
          }
          else
          {
            //if the default headers footers shall be used, add then to the detail config ( internally, it ensures that it is added only once ).
            if( detailConfig.UseDefaultHeadersFooters )
            {
              detailConfig.AddDefaultHeadersFooters();
            }
          }

          // If the DetailConfiguration is not meant to be visible, then skip next section (creation of detail) and continue looping on other detail configurations
          if( !detailConfig.Visible )
            continue;

          var newDetailCollectionViewBase = default( DataGridCollectionViewBase );
          var detailDataSource = default( IEnumerable );

          // If the data source returned by the detailDescription is null ( or if there was no detail description ), then create an empty data source for the detail data.
          if( detailDataSource == null )
          {
            detailDataSource = new object[] { };
          }

          newDetailCollectionViewBase = dataGridCollectionViewBase.CreateDetailDataGridCollectionViewBase( detailDataSource, null, dataGridCollectionViewBase );

          Debug.Assert( newDetailCollectionViewBase != null );

          using( detailConfig.ColumnManager.DeferUpdate( new ColumnHierarchyManager.UpdateOptions( TableView.GetFixedColumnCount( detailConfig ), true ) ) )
          {
            if( detailConfig.AutoCreateForeignKeyConfigurations && !detailConfig.ForeignKeysUpdatedOnAutoCreate )
            {
              // Ensure to update the foreign key related properties before expanding detail
              ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurationsFromDataGridCollectionView( detailConfig.Columns,
                                                                                                       newDetailCollectionViewBase.ItemProperties,
                                                                                                       detailConfig.AutoCreateForeignKeyConfigurations );
              detailConfig.ForeignKeysUpdatedOnAutoCreate = true;
            }
          }

          var newDetailNode = this.CreateDetailGeneratorNode( dataItem, newDetailCollectionViewBase, detailConfig );
          newDetails.Add( newDetailNode );

          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_CreateDetailsHelper, DataGridTraceMessages.DetailNodeAdded,
                           DataGridTraceArgs.Node( newDetailNode ), DataGridTraceArgs.Item( dataItem ) );

          totalAddCount += newDetailNode.ItemCount;
        }
      }
      else // there was details for the master item in the list to be requeued
      {
        // count the items from the details.
        foreach( var detailNode in newDetails )
        {
          totalAddCount += detailNode.ItemCount;
        }
      }

      // Plug details in the master items node.
      var indexOfItem = masterNode.Items.IndexOf( dataItem );
      var details = masterNode.Details;

      if( details == null )
      {
        details = new SortedDictionary<int, List<DetailGeneratorNode>>();
        masterNode.Details = details;
      }

      details.Add( indexOfItem, newDetails );

      masterNode.AdjustItemCount( totalAddCount );

      if( !detailsAlreadyExist )
      {
        m_masterToDetails.Add( dataItem, new List<DetailGeneratorNode>( newDetails ) );

        for( int i = 0; i < newDetails.Count; i++ )
        {
          m_dataGridControl.RestoreDataGridContextState( newDetails[ i ].DetailContext );
        }
      }
      else
      {
        var detailsMapped = m_masterToDetails.ContainsKey( dataItem );
        Debug.Assert( detailsMapped, "Item not found on master level." );

        if( !detailsMapped )
        {
          this.TraceEvent( TraceEventType.Warning, DataGridTraceEventId.CustomItemContainerGenerator_CreateDetailsHelper, DataGridTraceMessages.RemapZombieDetail,
                           DataGridTraceArgs.Item( dataItem ) );
        }
      }

      this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_CreateDetailsHelper, DataGridTraceMessages.DetailExpanded,
                       DataGridTraceArgs.Item( dataItem ) );

      return totalAddCount;
    }

    private void CreateDetailsForItem( object dataItem )
    {
      if( m_generatorStatus == GeneratorStatus.GeneratingContainers )
        throw DataGridException.Create<DataGridInternalException>( "Cannot perform this operation while the generator is busy generating items", m_dataGridControl );

      if( ( dataItem == null ) || ( dataItem is EmptyDataItem ) )
      {
        this.TraceEvent( TraceEventType.Error, DataGridTraceEventId.CustomItemContainerGenerator_CreateDetailsForItem, DataGridTraceMessages.CannotExpandDetail, DataGridTraceArgs.Item( dataItem ) );
        return;
      }

      if( m_masterToDetails.ContainsKey( dataItem ) )
      {
        //before throwing, verify if the item is not currently pending requeue
        if( !m_floatingDetails.Contains( dataItem ) )
          throw DataGridException.Create<InvalidOperationException>( "An attempt was made to create details for an item whose details are already mapped.", m_dataGridControl );
      }

      var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
      var dataItemFound = ( nodeHelper.FindItem( dataItem ) >= 0 );

      //means either that the item is not present in the Generator or that a parent group of the item is collapsed...
      if( !dataItemFound )
      {
        //make sure it is the later, else throw an exception.
        nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
        if( !nodeHelper.Contains( dataItem ) )
          throw DataGridException.Create<InvalidOperationException>( "An attempt was made to create details for an item that does not belong to the generator.", m_dataGridControl );
      }

      var masterNode = nodeHelper.CurrentNode as ItemsGeneratorNode;
      if( masterNode == null )
        throw DataGridException.Create<InvalidOperationException>( "An attempt was made to create details for an item that does not map to an item node.", m_dataGridControl );

      try
      {
        var count = this.CreateDetailsHelper( masterNode, dataItem );

        this.IncrementCurrentGenerationCount();

        if( dataItemFound )
        {
          this.SendAddEvent( count );
        }
      }
      catch( Exception e )
      {
        throw new DataGridException( e.Message, e, m_dataGridControl );
      }
    }

    private void HandleDetailGeneratorContentChanged( object sender, CustomGeneratorChangedEventArgs e )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_OnDetailGeneratorContentChanged, DataGridTraceArgs.Node( sender ) ) )
      {
        if( this.IsHandlingGlobalItemsResetLocally )
        {
          this.TraceEvent( TraceEventType.Warning, DataGridTraceEventId.CustomItemContainerGenerator_OnDetailGeneratorContentChanged, DataGridTraceMessages.CannotProcessOnReset );
          return;
        }

        var detailGenerator = sender as CustomItemContainerGenerator;

        Debug.Assert( detailGenerator != null );
        Debug.Assert( m_startNode != null );

        object masterItem;
        var detailNode = this.FindDetailGeneratorNodeForGenerator( detailGenerator, out masterItem );

        Debug.Assert( masterItem != null );
        Debug.Assert( detailNode != null );

        if( ( detailNode != null ) && ( masterItem != null ) )
        {
          this.TraceEvent( TraceEventType.Verbose, DataGridTraceEventId.CustomItemContainerGenerator_OnDetailGeneratorContentChanged, DataGridTraceArgs.Action( e.Action ), DataGridTraceArgs.Node( detailNode ), DataGridTraceArgs.Item( masterItem ) );

          switch( e.Action )
          {
            case NotifyCollectionChangedAction.Add:
              this.HandleDetailAddition( masterItem, detailNode, e );
              break;

            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Remove:
              this.HandleDetailMoveRemove( masterItem, detailNode, e );
              break;

            case NotifyCollectionChangedAction.Replace: //CustomItemContainerGenreator never issues a Replace!
              throw DataGridException.Create<DataGridInternalException>( "CustomItemContainerGenerator never notifies a Replace action.", m_dataGridControl );

            case NotifyCollectionChangedAction.Reset:
              this.HandleDetailReset( masterItem, detailNode );
              break;

            default:
              break;
          }
        }
      }
    }

    private void HandleDetailAddition( object masterItem, DetailGeneratorNode detailNode, CustomGeneratorChangedEventArgs e )
    {
      if( m_floatingDetails.Contains( masterItem ) )
        return;

      var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );

      // The master item has been found.
      if( nodeHelper.FindItem( masterItem ) >= 0 )
      {
        var masterNode = ( ItemsGeneratorNode )nodeHelper.CurrentNode;

        masterNode.AdjustItemCount( e.Count );
        detailNode.UpdateItemCount();

        this.IncrementCurrentGenerationCount();
        this.SendAddEvent( e.Count );
      }
      // The master item could be located inside a collapsed group.
      else
      {
        //in that case, I need to determine the appropriate masterNode another way
        nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
        if( !nodeHelper.Contains( masterItem ) )
          throw DataGridException.Create<InvalidOperationException>( "An attempt was made to add a detail for an item that does not belong to the generator.", m_dataGridControl );

        var masterNode = ( ItemsGeneratorNode )nodeHelper.CurrentNode;

        masterNode.AdjustItemCount( e.Count );
        detailNode.UpdateItemCount();

        this.IncrementCurrentGenerationCount();
      }
    }

    private void HandleDetailMoveRemove( object masterItem, DetailGeneratorNode detailNode, CustomGeneratorChangedEventArgs e )
    {
      if( m_floatingDetails.Contains( masterItem ) )
        return;

      var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );

      // The master item has been found.
      if( nodeHelper.FindItem( masterItem ) >= 0 )
      {
        this.RemoveDetailContainers( e.Containers );

        if( e.Action == NotifyCollectionChangedAction.Remove )
        {
          var masterNode = ( ItemsGeneratorNode )nodeHelper.CurrentNode;

          masterNode.AdjustItemCount( -e.Count );
          detailNode.UpdateItemCount();
        }

        this.IncrementCurrentGenerationCount();
        this.SendRemoveEvent( e.Count, e.Containers );
      }
      // The master item could be located inside a collapsed group.
      else
      {
        //in that case, I need to determine the appropriate masterNode another way
        nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
        if( !nodeHelper.Contains( masterItem ) )
          throw DataGridException.Create<InvalidOperationException>( "An attempt was made to move or remove a detail for an item that does not belong to the generator.", m_dataGridControl );

        if( e.Action == NotifyCollectionChangedAction.Remove )
        {
          var masterNode = ( ItemsGeneratorNode )nodeHelper.CurrentNode;

          masterNode.AdjustItemCount( -e.Count );
          detailNode.UpdateItemCount();
        }

        this.IncrementCurrentGenerationCount();
      }
    }

    private void HandleDetailReset( object masterItem, DetailGeneratorNode detailNode )
    {
      using( this.TraceBlock( DataGridTraceEventId.CustomItemContainerGenerator_HandleDetailReset ) )
      {
        if( m_floatingDetails.Contains( masterItem ) )
        {
          this.TraceEvent( TraceEventType.Information, DataGridTraceEventId.CustomItemContainerGenerator_HandleDetailReset, DataGridTraceMessages.DetailIsFloating );
          return;
        }

        var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
        var masterItemFound = ( nodeHelper.FindItem( masterItem ) >= 0 );

        // The master item could be located inside a collapsed group.
        if( !masterItemFound )
        {
          nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
          if( !nodeHelper.Contains( masterItem ) )
          {
            this.TraceEvent( TraceEventType.Critical, DataGridTraceEventId.CustomItemContainerGenerator_HandleDetailReset, DataGridTraceMessages.ItemNotBelongingToGenerator );
            throw DataGridException.Create<InvalidOperationException>( "An attempt was made to reset a detail for an item that does not belong to the generator.", m_dataGridControl );
          }

          this.TraceEvent( TraceEventType.Information, DataGridTraceEventId.CustomItemContainerGenerator_HandleDetailReset, DataGridTraceMessages.ItemNotFoundOrCollapsed );
        }

        var masterNode = ( ItemsGeneratorNode )nodeHelper.CurrentNode;
        var oldDetailCount = detailNode.ItemCount;
        var oldContainers = m_genPosToContainer.Where( ( c, i ) => m_genPosToNode[ i ] == detailNode ).ToList();

        using( detailNode.DetailGenerator.InhibitItemsChanged() )
        {
          detailNode.UpdateItemCount();
        }

        var newDetailCount = detailNode.ItemCount;
        var newContainers = detailNode.DetailGenerator.m_genPosToContainer;

        masterNode.AdjustItemCount( newDetailCount - oldDetailCount );

        var containersRemoved = new HashSet<DependencyObject>();
        var containersMoved = new HashSet<DependencyObject>();

        CustomItemContainerGenerator.FindChanges( oldContainers, newContainers, null, containersRemoved, containersMoved );
        this.ApplyDetailChanges( detailNode, newContainers, containersRemoved, containersMoved );
      }
    }

    private DetailGeneratorNode FindDetailGeneratorNodeForGenerator( CustomItemContainerGenerator detailGenerator, out object masterItem )
    {
      foreach( var detailsForItem in m_masterToDetails )
      {
        foreach( var detailNode in detailsForItem.Value )
        {
          if( detailNode.DetailGenerator == detailGenerator )
          {
            masterItem = detailsForItem.Key;
            return detailNode;
          }
        }
      }

      masterItem = null;
      return null;
    }

    internal void SetGenPosToIndexUpdateInhibiter( IInhibitGenPosToIndexUpdating inhibiter )
    {
      m_genPosToIndexInhibiter = inhibiter;
    }

    private IDisposable InhibitParentGenPosToIndexUpdate()
    {
      if( m_genPosToIndexInhibiter != null )
        return m_genPosToIndexInhibiter.InhibitGenPosToIndexUpdates();

      return null;
    }

    private int FindFirstGeneratedIndexForLocalItem( object item )
    {
      //this function will find an item in the m_genPosToItem, but will filter out those that belongs to details.
      //This is to avoid the problem caused by Detail items that belongs also to a master.
      var retval = -1;
      var runningIndex = 0;
      var itemCount = m_genPosToItem.Count;

      while( runningIndex < itemCount )
      {
        retval = m_genPosToItem.IndexOf( item, runningIndex );

        //No item found past the current runningIndex
        if( retval == -1 )
          break;

        //check if the item belongs to a Detail or not.
        var detailNode = m_genPosToNode[ retval ] as DetailGeneratorNode;
        if( detailNode == null )
          break; //the item is not a detail and therefore qualifies as a return value.

        //Item is from a detail
        runningIndex = retval + 1;
      }

      return retval;
    }

    private bool EnqueueContainer( DependencyObject container, object item )
    {
      if( container is HeaderFooterItem )
      {
        if( item is GroupHeaderFooterItem )
        {
          //If the group does not exist anymore (i.e. all its rows have been deleted), do not recycle it.
          var groupHeaderFooterItem = ( GroupHeaderFooterItem )item;
          var collectionViewGroup = groupHeaderFooterItem.Group;
          if( collectionViewGroup == null )
            return false;

          string groupBy = null;
          if( collectionViewGroup is DataGridCollectionViewGroup )
          {
            groupBy = ( ( DataGridCollectionViewGroup )collectionViewGroup ).GroupByName;
          }

          if( string.IsNullOrEmpty( groupBy ) )
          {
            var group = this.GetGroupFromCollectionViewGroupCore( collectionViewGroup );
            if( group == null )
              return false;

            groupBy = group.GroupBy;
            if( string.IsNullOrEmpty( groupBy ) )
              return false;
          }

          m_recyclingPools.GetGroupHeaderFooterItemContainerPool( m_dataGridContext.SourceDetailConfiguration, true ).Enqueue( groupBy, groupHeaderFooterItem.Template, container );
        }
        else
        {
          m_recyclingPools.GetHeaderFooterItemContainerPool( m_dataGridContext.SourceDetailConfiguration, true ).Enqueue( item, container );
        }
      }
      else
      {
        m_recyclingPools.GetItemContainerPool( m_dataGridContext.SourceDetailConfiguration, true ).Enqueue( item, container );
      }

      return true;
    }

    private DependencyObject DequeueItemContainer( object item )
    {
      var pool = m_recyclingPools.GetItemContainerPool( m_dataGridContext.SourceDetailConfiguration );
      if( pool == null )
        return null;

      return pool.Dequeue( item );
    }

    private DependencyObject DequeueHeaderFooterContainer( object item )
    {
      if( !( item is GroupHeaderFooterItem ) )
      {
        var pool = m_recyclingPools.GetHeaderFooterItemContainerPool( m_dataGridContext.SourceDetailConfiguration );
        if( pool == null )
          return null;

        return pool.Dequeue( item );
      }
      else
      {
        var pool = m_recyclingPools.GetGroupHeaderFooterItemContainerPool( m_dataGridContext.SourceDetailConfiguration );
        if( pool == null )
          return null;

        var groupHeaderFooterItem = ( GroupHeaderFooterItem )item;
        var collectionViewGroup = groupHeaderFooterItem.Group;
        if( collectionViewGroup == null )
          return null;

        string groupBy = null;
        if( collectionViewGroup is DataGridCollectionViewGroup )
        {
          groupBy = ( ( DataGridCollectionViewGroup )collectionViewGroup ).GroupByName;
        }

        if( string.IsNullOrEmpty( groupBy ) )
        {
          var group = this.GetGroupFromCollectionViewGroupCore( collectionViewGroup );
          if( group == null )
            return null;

          groupBy = group.GroupBy;
          if( string.IsNullOrEmpty( groupBy ) )
            return null;
        }

        return pool.Dequeue( groupBy, groupHeaderFooterItem.Template );
      }
    }

    internal void CleanRecyclingCandidates()
    {
      m_recyclingPools.CleanRecyclingCandidates();
    }

    private void OnDetailContainersRemoved( object sender, ContainersRemovedEventArgs e )
    {
      Debug.Assert( !this.IsRecyclingEnabled );

      this.OnContainersRemoved( e.RemovedContainers );
    }

    private DependencyObject CreateNextItemContainer( object item )
    {
      DependencyObject container = null;

      if( this.IsRecyclingEnabled )
      {
        container = this.DequeueItemContainer( item );
      }

      if( container == null )
      {
        container = m_dataGridControl.CreateContainerForItem();
      }

      return container;
    }

    private IEnumerable<DataGridContext> GetDetailContexts()
    {
      return ( from node in this.GetDetailGeneratorNodes()
               select node.DetailContext );
    }

    private IEnumerable<CustomItemContainerGenerator> GetDetailGenerators()
    {
      return ( from node in this.GetDetailGeneratorNodes()
               select node.DetailGenerator );
    }

    private IEnumerable<DetailGeneratorNode> GetDetailGeneratorNodes()
    {
      foreach( List<DetailGeneratorNode> details in m_masterToDetails.Values )
      {
        foreach( DetailGeneratorNode detailNode in details )
        {
          yield return detailNode;
        }
      }
    }

    private static IList GetList( CollectionView collectionView )
    {
      if( collectionView == null )
        return null;

      var list = collectionView as IList;
      if( list != null )
        return list;

      return collectionView.SourceCollection as IList;
    }

    private static void FindChanges<T>(
      IList<T> source,
      IList<T> destination,
      ICollection<T> itemsAdded,
      ICollection<T> itemsRemoved,
      ICollection<T> itemsMoved )
    {
      if( source == null )
        throw new ArgumentNullException( "source" );

      if( destination == null )
        throw new ArgumentNullException( "destination" );

      // There is nothing to do since the caller is not interested by the result.
      if( ( itemsAdded == null ) && ( itemsRemoved == null ) && ( itemsMoved == null ) )
        return;

      var sourceCount = source.Count;
      var sourcePositions = new Dictionary<T, int>( sourceCount );
      var sequence = new List<int>( Math.Min( sourceCount, destination.Count ) );
      var isAlive = new BitArray( sourceCount, false );
      var hasNotMoved = default( BitArray );

      for( var i = 0; i < sourceCount; i++ )
      {
        sourcePositions.Add( source[ i ], i );
      }

      foreach( var item in destination )
      {
        int index;

        if( sourcePositions.TryGetValue( item, out index ) )
        {
          isAlive[ index ] = true;
          sequence.Add( index );
        }
        else if( itemsAdded != null )
        {
          itemsAdded.Add( item );
        }
      }

      // We may omit this part of the algorithm if the caller is not interested by the items that have moved.
      if( itemsMoved != null )
      {
        hasNotMoved = new BitArray( sourceCount, false );

        // The subsequence contains the position of the item that are in the destination collection and that have not moved.
        foreach( var index in LongestIncreasingSubsequence.Find( sequence ) )
        {
          hasNotMoved[ index ] = true;
        }
      }

      // We may omit this part of the algorithm if the caller is not interested by the items that have moved or were removed.
      if( ( itemsRemoved != null ) || ( itemsMoved != null ) )
      {
        for( var i = 0; i < sourceCount; i++ )
        {
          if( isAlive[ i ] )
          {
            // We check if the move collection is not null first because the bit array is null when the move collection is null.
            if( ( itemsMoved != null ) && !hasNotMoved[ i ] )
            {
              itemsMoved.Add( source[ i ] );
            }
          }
          else if( itemsRemoved != null )
          {
            itemsRemoved.Add( source[ i ] );
          }
        }
      }
    }

    public void Skip()
    {
      if( m_generatorDirection == GeneratorDirection.Forward )
      {
        this.MoveGeneratorForward();
      }
      else
      {
        this.MoveGeneratorBackward();
      }
    }

    #region Private Fields

    private BitVector32 m_flags = new BitVector32();

    //This list is used to map from a Realized Item generator position to an actual index. Index in list is the GenPos Index.
    private readonly List<int> m_genPosToIndex = new List<int>();
    private readonly List<object> m_genPosToItem = new List<object>();
    private readonly List<DependencyObject> m_genPosToContainer = new List<DependencyObject>();
    private readonly List<GeneratorNode> m_genPosToNode = new List<GeneratorNode>();

    private readonly Dictionary<object, List<DetailGeneratorNode>> m_masterToDetails = new Dictionary<object, List<DetailGeneratorNode>>();
    private readonly List<object> m_floatingDetails = new List<object>();

    private GeneratorNode m_startNode;
    private GeneratorNode m_firstItem;

    private ReadOnlyObservableCollection<object> m_groupsCollection;

    private readonly DataGridControl m_dataGridControl;
    private readonly DataGridContext m_dataGridContext;
    private readonly CollectionView m_collectionView;

    private readonly CustomItemContainerGeneratorRecyclingPools m_recyclingPools;

    private int m_lastValidItemCountGeneration = 0;

    private int m_cachedItemCount = 0;

    private GeneratorDirection m_generatorDirection;
    private int m_generatorCurrentOffset;
    private int m_generatorCurrentGlobalIndex = -1;
    private GeneratorNodeHelper m_generatorNodeHelper; // = null

    private readonly Dictionary<CollectionViewGroup, GroupGeneratorNode> m_groupNodeMappingCache = new Dictionary<CollectionViewGroup, GroupGeneratorNode>();
    private LateGroupLevelDescription[] m_groupLevelDescriptionCache; // = null

    private IDisposable m_generatorCurrentDetailDisposable; // = null
    private DetailGeneratorNode m_generatorCurrentDetail; // = null
    private int m_generatorCurrentDetailIndex = -1;
    private int m_generatorCurrentDetailNodeIndex = -1;

    private int m_genPosToIndexUpdateInhibitCount = 0;

    private IInhibitGenPosToIndexUpdating m_genPosToIndexInhibiter; // = null
    private IDisposable m_currentGenPosToIndexInhibiterDisposable; // = null

    private Binding m_headerFooterDataContextBinding;

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        var eventArgs = ( ( NotifyCollectionChangedEventArgs )e ).GetRangeActionOrSelf();

        if( sender == m_collectionView )
        {
          this.OnItemsChanged( sender, eventArgs );
        }
        else if( sender == m_groupsCollection )
        {
          this.OnGroupsChanged( sender, eventArgs );
        }
        else if( sender == m_dataGridContext.DetailConfigurations )
        {
          this.OnDetailConfigurationsChanged( sender, eventArgs );
        }
      }
      else if( managerType == typeof( PropertyChangedEventManager ) )
      {
        var eventArgs = ( PropertyChangedEventArgs )e;

        if( sender == m_collectionView )
        {
          this.OnCollectionViewPropertyChanged( eventArgs );
        }
        else
        {
          //this is only registered on the DataGridContext, this has the effect of forwarding property changes for all properties of the DataGridContext
          this.OnNotifyPropertyChanged( eventArgs );
        }
      }
      else if( ( managerType == typeof( ViewChangedEventManager ) ) || ( managerType == typeof( ThemeChangedEventManager ) ) )
      {
        this.OnViewThemeChanged( sender, e );
      }
      else if( managerType == typeof( DetailsChangedEventManager ) )
      {
        if( this.DetailsChanged != null )
        {
          this.DetailsChanged( sender, e );
        }
      }
      else if( managerType == typeof( ItemsSourceChangeCompletedEventManager ) )
      {
        this.OnItemsSourceChanged( sender, e );
      }
      else if( managerType == typeof( GroupConfigurationSelectorChangedEventManager ) )
      {
        this.OnGroupConfigurationSelectorChanged();
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnNotifyPropertyChanged( PropertyChangedEventArgs e )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    #region IInhibitGenPosToIndexUpdating Members

    IDisposable IInhibitGenPosToIndexUpdating.InhibitGenPosToIndexUpdates()
    {
      return new GenPosToIndexInhibitionDisposable( this );
    }

    #endregion

    #region IDataGridContextVisitable Members

    void IDataGridContextVisitable.AcceptVisitor( IDataGridContextVisitor visitor, out bool visitWasStopped )
    {
      IDataGridContextVisitable visitable = this as IDataGridContextVisitable;

      visitable.AcceptVisitor( 0, int.MaxValue, visitor, DataGridContextVisitorType.All, true, out visitWasStopped );
    }

    void IDataGridContextVisitable.AcceptVisitor( int minIndex, int maxIndex, IDataGridContextVisitor visitor, out bool visitWasStopped )
    {
      IDataGridContextVisitable visitable = this as IDataGridContextVisitable;

      visitable.AcceptVisitor( minIndex, maxIndex, visitor, DataGridContextVisitorType.All, true, out visitWasStopped );
    }

    void IDataGridContextVisitable.AcceptVisitor( int minIndex, int maxIndex, IDataGridContextVisitor visitor, DataGridContextVisitorType visitorType, out bool visitWasStopped )
    {
      IDataGridContextVisitable visitable = this as IDataGridContextVisitable;

      visitable.AcceptVisitor( minIndex, maxIndex, visitor, visitorType, true, out visitWasStopped );
    }

    void IDataGridContextVisitable.AcceptVisitor( int minIndex, int maxIndex, IDataGridContextVisitor visitor, DataGridContextVisitorType visitorType, bool visitDetails, out bool visitWasStopped )
    {
      visitWasStopped = false;

      if( m_startNode == null )
        return;

      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
      nodeHelper.ProcessVisit( m_dataGridContext, minIndex, maxIndex, visitor, visitorType, visitDetails, out visitWasStopped );
    }

    #endregion

    #region InheritAutoResetFlag Private Class

    private sealed class InheritAutoResetFlag : AutoResetFlag
    {
      internal InheritAutoResetFlag()
        : this( null )
      {
      }

      internal InheritAutoResetFlag( AutoResetFlag parent )
      {
        m_parent = parent;
        m_current = AutoResetFlagFactory.Create();
      }

      public override bool IsSet
      {
        get
        {
          return ( this.IsSetLocal )
              || ( ( m_parent != null ) && m_parent.IsSet );
        }
      }

      public bool IsSetLocal
      {
        get
        {
          return m_current.IsSet;
        }
      }

      public override IDisposable Set()
      {
        return m_current.Set();
      }

      public IDisposable SetLocal()
      {
        return this.Set();
      }

      private readonly AutoResetFlag m_parent;
      private readonly AutoResetFlag m_current;
    }

    #endregion

    #region LeveledAutoResetFlag Private Class

    private sealed class LeveledAutoResetFlag : AutoResetFlag
    {
      internal LeveledAutoResetFlag()
        : this( new List<WeakReference>( 3 ), 0 )
      {
      }

      private LeveledAutoResetFlag( IList<WeakReference> levels, int index )
      {
        Debug.Assert( levels != null );
        Debug.Assert( ( index >= 0 ) && ( index <= levels.Count ) );

        m_levels = levels;
        m_flag = AutoResetFlagFactory.Create();
        m_index = index;

        var self = new WeakReference( this );
        if( m_index == m_levels.Count )
        {
          m_levels.Add( self );
        }
        else
        {
          m_levels[ m_index ] = self;
        }
      }

      public override bool IsSet
      {
        get
        {
          if( m_flag.IsSet )
            return true;

          var parent = this.GetFlag( m_index - 1 );
          if( parent != null )
            return parent.IsSet;

          return false;
        }
      }

      public override IDisposable Set()
      {
        return m_flag.Set();
      }

      internal LeveledAutoResetFlag GetChild()
      {
        var index = m_index + 1;
        var flag = this.GetFlag( index );

        if( flag != null )
          return flag;

        return new LeveledAutoResetFlag( m_levels, index );
      }

      private LeveledAutoResetFlag GetFlag( int index )
      {
        if( ( index < 0 ) || ( index >= m_levels.Count ) )
          return null;

        return ( LeveledAutoResetFlag )m_levels[ index ].Target;
      }

      private readonly IList<WeakReference> m_levels;
      private readonly AutoResetFlag m_flag;
      private readonly int m_index;
    }

    #endregion

    #region BubbleDirtyFlag Private Class

    private sealed class BubbleDirtyFlag
    {
      internal BubbleDirtyFlag( bool isSet )
        : this( null, isSet )
      {
      }

      internal BubbleDirtyFlag( BubbleDirtyFlag parent, bool isSet )
      {
        m_parent = parent;

        this.IsSet = isSet;
      }

      internal bool IsSet
      {
        get
        {
          return m_isSet;
        }
        set
        {
          if( value == m_isSet )
            return;

          m_isSet = value;

          if( value && ( m_parent != null ) )
          {
            m_parent.IsSet = true;
          }
        }
      }

      private readonly BubbleDirtyFlag m_parent;
      private bool m_isSet;
    }

    #endregion

    #region CustomItemContainerGeneratorDisposableDisposer Private Class

    private sealed class CustomItemContainerGeneratorDisposableDisposer : IDisposable
    {
      public CustomItemContainerGeneratorDisposableDisposer( CustomItemContainerGenerator generator, GeneratorPosition startGenPos, GeneratorDirection direction )
      {
        if( generator == null )
          throw new ArgumentNullException( "generator" );

        try
        {
          generator.StartGenerator( startGenPos, direction );
        }
        catch( Exception e )
        {
          try
          {
            generator.StopGenerator();
          }
          catch
          {
            // We swallow all exceptions thrown here because the actual exception we want to raise to the user
            // is the one that has been thrown in the call to StartGenerator.
          }

          throw new DataGridInternalException( "The generator failed to start.", e, generator.m_dataGridControl );
        }

        m_generator = generator;
      }

      void IDisposable.Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        // Prevent this method from being called more than once.
        var generator = Interlocked.Exchange( ref m_generator, null );
        if( generator == null )
          return;

        generator.StopGenerator();
      }

      ~CustomItemContainerGeneratorDisposableDisposer()
      {
        this.Dispose( false );
      }

      private CustomItemContainerGenerator m_generator; //null
    }

    #endregion

    #region GenPosToIndexInhibitionDisposable Private Class

    private sealed class GenPosToIndexInhibitionDisposable : IDisposable
    {
      internal GenPosToIndexInhibitionDisposable( CustomItemContainerGenerator generator )
      {
        if( generator == null )
          throw new ArgumentNullException( "generator" );

        m_generator = generator;

        Interlocked.Increment( ref m_generator.m_genPosToIndexUpdateInhibitCount );

        if( m_generator.m_genPosToIndexInhibiter != null )
        {
          m_nestedDisposable = m_generator.m_genPosToIndexInhibiter.InhibitGenPosToIndexUpdates();
        }
      }

      private void Dispose( bool disposing )
      {
        var generator = Interlocked.Exchange( ref m_generator, null );
        if( generator == null )
          return;

        if( Interlocked.Decrement( ref generator.m_genPosToIndexUpdateInhibitCount ) == 0 )
        {
          if( generator.GenPosToIndexNeedsUpdate )
          {
            generator.IncrementCurrentGenerationCount();
          }
        }

        if( m_nestedDisposable != null )
        {
          m_nestedDisposable.Dispose();
          m_nestedDisposable = null;
        }
      }

      void IDisposable.Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      ~GenPosToIndexInhibitionDisposable()
      {
        this.Dispose( false );
      }

      private CustomItemContainerGenerator m_generator; // = null
      private IDisposable m_nestedDisposable; // = null
    }

    #endregion

    #region CustomItemContainerGeneratorFlags Private Enum

    [Flags]
    private enum CustomItemContainerGeneratorFlags
    {
      RecyclingEnabled = 1 << 0,
      InUse = 1 << 1,
      GenPosToIndexNeedsUpdate = 1 << 2,
      ForceReset = 1 << 3,
    }

    #endregion
  }
}
