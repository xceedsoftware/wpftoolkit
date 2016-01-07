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
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Xceed.Utils.Collections;
using Xceed.Utils.Wpf;
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
    #region Static Fields

    private const int GlobalItemsResetThreshold = 10000;

    #endregion

    internal static CustomItemContainerGenerator CreateGenerator( DataGridControl dataGridControl, CollectionView collectionView, DataGridContext dataGridContext )
    {
      if( dataGridControl == null )
        throw new ArgumentNullException( "dataGridControl" );

      if( collectionView == null )
        DataGridException.ThrowSystemException( "collectionView", typeof( ArgumentNullException ), dataGridControl.Name );

      if( dataGridContext == null )
        DataGridException.ThrowSystemException( "dataGridContext", typeof( ArgumentNullException ), dataGridControl.Name );

      var log = new Log();
      log.StartUp();

      var handleGlobalItemsResetFlag = new InheritAutoResetFlag();
      var deferDetailsRemapFlag = new LeveledAutoResetFlag();
      var generator = new CustomItemContainerGenerator( dataGridControl, collectionView, dataGridContext, log, handleGlobalItemsResetFlag, deferDetailsRemapFlag );

      dataGridContext.SetGenerator( generator );

      return generator;
    }

    private static CustomItemContainerGenerator CreateGenerator( DataGridControl dataGridControl, CollectionView collectionView, DataGridContext dataGridContext, CustomItemContainerGenerator masterGenerator )
    {
      if( dataGridControl == null )
        throw new ArgumentNullException( "dataGridControl" );

      if( collectionView == null )
        DataGridException.ThrowSystemException( "collectionView", typeof( ArgumentNullException ), dataGridControl.Name );

      if( dataGridContext == null )
        DataGridException.ThrowSystemException( "dataGridContext", typeof( ArgumentNullException ), dataGridControl.Name );

      if( masterGenerator == null )
        DataGridException.ThrowSystemException( "masterGenerator", typeof( ArgumentNullException ), dataGridControl.Name );

      var log = masterGenerator.m_log;
      var handleGlobalItemsResetFlag = new InheritAutoResetFlag( masterGenerator.m_handleGlobalItemsReset );
      var deferDetailsRemapFlag = masterGenerator.m_deferDetailsRemap.GetChild();
      var generator = new CustomItemContainerGenerator( dataGridControl, collectionView, dataGridContext, log, handleGlobalItemsResetFlag, deferDetailsRemapFlag );

      dataGridContext.SetGenerator( generator );

      return generator;
    }

    private CustomItemContainerGenerator(
      DataGridControl dataGridControl,
      CollectionView collectionView,
      DataGridContext dataGridContext,
      Log log,
      InheritAutoResetFlag handleGlobalItemResetFlag,
      LeveledAutoResetFlag deferDetailsRemapFlag )
    {
      Debug.Assert( dataGridControl != null );
      Debug.Assert( collectionView != null );
      Debug.Assert( dataGridContext != null );
      Debug.Assert( log != null );
      Debug.Assert( handleGlobalItemResetFlag != null );
      Debug.Assert( deferDetailsRemapFlag != null );

      m_dataGridControl = dataGridControl;
      m_dataGridContext = dataGridContext;
      m_collectionView = collectionView;
      m_log = log;
      m_handleGlobalItemsReset = handleGlobalItemResetFlag;
      m_deferDetailsRemap = deferDetailsRemapFlag;

      //initialize explicitly (set a local value) to the DataItem attached property (for nested DataGridControls)
      CustomItemContainerGenerator.SetDataItemProperty( m_dataGridControl, EmptyDataItemDataProvider.Instance );

      this.RegisterEvents();

      m_nodeFactory = new GeneratorNodeFactory( this.OnGeneratorNodeItemsCollectionChanged,
                                                this.OnGeneratorNodeGroupsCollectionChanged,
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
      return m_ensureNodeTreeCreated.Set();
    }

    private readonly AutoResetFlag m_ensureNodeTreeCreated = AutoResetFlagFactory.Create();

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
      m_log.Assert( this, ( m_containersRemovedDeferCount != 0 ) || ( m_deferredContainersRemoved.Count == 0 ), "( m_containersRemovedDeferCount != 0 ) || ( m_deferredContainersRemoved.Count == 0 )" );

      return m_handleGlobalItemsReset.SetLocal();
    }

    private readonly InheritAutoResetFlag m_handleGlobalItemsReset;

    #endregion

    #region DetailsChanged Event

    public event EventHandler DetailsChanged;

    #endregion

    #region ItemsChanged Internal Event

    internal event CustomGeneratorChangedEventHandler ItemsChanged;

    private void SendRemoveEvent( GeneratorPosition remPos, int oldIndex, int remCount, int generatedRemCount, IList<DependencyObject> removedContainers )
    {
      var handler = this.ItemsChanged;
      if( handler == null )
        return;

      using( this.DeferDetailsRemap() )
      {
        handler.Invoke( this, new CustomGeneratorChangedEventArgs( NotifyCollectionChangedAction.Remove, remPos, oldIndex, remPos, oldIndex, remCount, generatedRemCount, removedContainers ) );
      }
    }

    private void SendAddEvent( GeneratorPosition genPos, int index, int addCount )
    {
      var handler = this.ItemsChanged;
      if( handler == null )
        return;

      using( this.DeferDetailsRemap() )
      {
        handler.Invoke( this, new CustomGeneratorChangedEventArgs( NotifyCollectionChangedAction.Add, genPos, index, addCount, 0 ) );
      }
    }

    private void SendResetEvent()
    {
      var handler = this.ItemsChanged;
      if( handler == null )
        return;

      using( this.DeferDetailsRemap() )
      {
        handler.Invoke( this, new CustomGeneratorChangedEventArgs( NotifyCollectionChangedAction.Reset, new GeneratorPosition(), 0, 0, 0 ) );
      }
    }

    #endregion

    #region ContainersRemoved Internal Event

    internal event ContainersRemovedEventHandler ContainersRemoved;

    private void NotifyContainersRemoved( IList<DependencyObject> removedContainers )
    {
      if( m_containersRemovedDeferCount > 0 )
      {
        m_deferredContainersRemoved.AddRange( removedContainers );
      }
      else if( removedContainers.Count > 0 )
      {
        this.NotifyContainersRemoved( new ContainersRemovedEventArgs( removedContainers ) );
      }
    }

    private void NotifyContainersRemoved( ContainersRemovedEventArgs e )
    {
      if( !this.IsRecyclingEnabled )
        return;

      var handler = this.ContainersRemoved;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    #region Logging Methods

    [Conditional( "CUSTOMLOG" )]
    internal void ResetLog( string gridUniqueName )
    {
      m_log.StartUp( gridUniqueName );
    }

    [Conditional( "LOG" )]
    private void LogState()
    {
      StringBuilder sb = new StringBuilder( 1024 );
      this.LogState( sb );

      m_log.WriteLine( this, sb.ToString() );
    }

    [Conditional( "LOG" )]
    private void LogState( StringBuilder sb )
    {
      var parent = m_dataGridContext.ParentDataGridContext;
      if( parent != null )
      {
        parent.CustomItemContainerGenerator.LogState( sb );
      }

      sb.Append( "Generator state : " );
      if( parent == null )
      {
        sb.AppendLine( "MASTER" );
      }
      else
      {
        sb.AppendLine( CustomItemContainerGenerator.FormatObjectId( "DETAIL - D", m_dataGridContext.ParentItem ) );
      }

      sb.AppendLine( string.Empty );
      sb.AppendLine( " m_genPosToContainer :" );
      for( int i = 0; i < m_genPosToContainer.Count; i++ )
      {
        object value = m_genPosToContainer[ i ];

        if( value == null )
        {
          sb.AppendLine( "  [ " + i.ToString() + " ] - null" );
        }
        else
        {
          sb.AppendLine( "  [ " + i.ToString() + " ] - " + value.GetHashCode() );
        }
      }

      sb.AppendLine( " m_genPosToIndex :" );
      for( int i = 0; i < m_genPosToIndex.Count; i++ )
      {
        object value = m_genPosToIndex[ i ];

        if( value == null )
        {
          sb.AppendLine( "  [ " + i.ToString() + " ] - null" );
        }
        else
        {
          sb.AppendLine( "  [ " + i.ToString() + " ] - " + value.ToString() );
        }
      }

      sb.AppendLine( " m_genPosToItem :" );
      for( int i = 0; i < m_genPosToItem.Count; i++ )
      {
        object value = m_genPosToItem[ i ];

        if( value == null )
        {
          sb.AppendLine( "  [ " + i.ToString() + " ] - null" );
        }
        else
        {
          sb.AppendLine( "  [ " + i.ToString() + " ] - D" + value.GetHashCode() );
        }
      }

      sb.AppendLine( " m_genPosToNode :" );
      for( int i = 0; i < m_genPosToNode.Count; i++ )
      {
        object value = m_genPosToNode[ i ];

        if( value == null )
        {
          sb.AppendLine( "  [ " + i.ToString() + " ] - null" );
        }
        else
        {
          string prefix;

          if( value is DetailGeneratorNode )
          {
            prefix = "DN";
          }
          else if( value is ItemsGeneratorNode )
          {
            prefix = "IN";
          }
          else if( value is CollectionGeneratorNode )
          {
            prefix = "CN";
          }
          else
          {
            prefix = "?N";
          }

          int itemCount = ( value is GeneratorNode ) ? ( ( GeneratorNode )value ).ItemCount : 0;

          sb.AppendLine( "  [ " + i.ToString() + " ] - " + prefix + value.GetHashCode() + " - ItemCount : " + itemCount );
        }
      }

      sb.AppendLine( " m_generatorDirection :" + m_generatorDirection.ToString() );
      sb.AppendLine( CustomItemContainerGenerator.FormatObjectId( " m_generatorCurrentDetail : DN", m_generatorCurrentDetail ) );
      sb.AppendLine( " m_generatorCurrentDetailIndex : " + m_generatorCurrentDetailIndex.ToString() );
      sb.AppendLine( " m_generatorCurrentDetailNodeIndex : " + m_generatorCurrentDetailNodeIndex.ToString() );
      sb.AppendLine( " m_generatorCurrentGlobalIndex : " + m_generatorCurrentGlobalIndex.ToString() );
      sb.AppendLine( " m_generatorCurrentOffset : " + m_generatorCurrentOffset.ToString() );
      sb.AppendLine( " m_generatorStatus : " + m_generatorStatus.ToString() );
      sb.AppendLine( " m_genPosToIndexUpdateInhibitCount : " + m_genPosToIndexUpdateInhibitCount.ToString() );
      sb.AppendLine( " IsEnsuringNodeTreeCreated : " + this.IsEnsuringNodeTreeCreated.ToString() );
      sb.AppendLine( " IsHandlingGlobalItemsReset : " + this.IsHandlingGlobalItemsReset.ToString() );
      sb.AppendLine( " IsDetailsRemapDeferred : " + this.IsDetailsRemapDeferred.ToString() );

      int newItemCount = m_cachedItemCount;
      if( m_lastValidItemCountGeneration != m_currentGeneratorContentGeneration )
      {
        if( m_startNode != null )
        {
          int chainLength;
          GeneratorNodeHelper.EvaluateChain( m_startNode, out newItemCount, out chainLength );
        }
        else
        {
          newItemCount = 0;
        }
      }

      sb.Append( " m_cachedItemCount : " + m_cachedItemCount.ToString() );
      if( newItemCount != m_cachedItemCount )
      {
        sb.Append( " (needs refresh) - ItemCount : " + newItemCount.ToString() );
      }
      sb.AppendLine( string.Empty );

      sb.AppendLine( string.Empty );
    }

    [Conditional( "LOG" )]
    private void ValidateIndexOrder( string message )
    {
      int count = m_genPosToIndex.Count;
      int previousIndex = -1;

      for( int i = 0; i < count; i++ )
      {
        int tempIndex = m_genPosToIndex[ i ];

        if( tempIndex <= previousIndex )
        {
          this.LogState();

          if( string.IsNullOrEmpty( message ) )
          {
            m_log.Fail( this, "### none sequential index detected." );
          }
          else
          {
            m_log.Fail( this, "### none sequential index detected. " + message );
          }

          break;
        }

        previousIndex = tempIndex;
      }
    }

    [Conditional( "DEBUG" ), Conditional( "LOG" )]
    private void CheckMasterItem( object dataItem )
    {
      if( m_masterToDetails.ContainsKey( dataItem ) )
        return;

      m_log.Fail( this, "m_masterToDetails do not contains the item from m_floatingDetails." );
    }

    private static string FormatObjectId( string message, object item )
    {
      string id = ( item == null ) ? "null" : item.GetHashCode().ToString();

      if( string.IsNullOrEmpty( message ) )
        return id;

      return message + id;
    }

    private readonly Log m_log;

    #endregion

    public static T FindContainerFromChild<T>( DataGridControl dataGridControl, DependencyObject element ) where T : DependencyObject, IDataGridItemContainer
    {
      if( ( dataGridControl == null ) || ( element == null ) )
        return null;

      T container = null;

      while( element != null )
      {
        var currentDataGridContext = DataGridControl.GetDataGridContext( element );
        var currentDataGridControl = ( currentDataGridContext == null ) ? null : currentDataGridContext.DataGridControl;

        // We are only interested in containers that are part of the same grid.
        if( currentDataGridControl == dataGridControl )
        {
          container = element as T;

          // We have found the container;
          if( container != null )
            break;
        }

        // Since the element is the grid itself, we already know we will not find a suitable container beyond this point.
        if( element == dataGridControl )
          break;

        element = TreeHelper.GetParent( element );
      }

      return container;
    }

    public static FrameworkElement FindContainerFromChildOrRowSelectorOrSelf( DataGridControl dataGridControl, DependencyObject originalChildOrSelf )
    {
      if( ( dataGridControl == null ) || ( originalChildOrSelf == null ) )
        return null;

      DependencyObject childOrSelf = originalChildOrSelf;
      IDataGridItemContainer container = null;

      while( childOrSelf != null )
      {
        var currentDataGridContext = DataGridControl.GetDataGridContext( childOrSelf );
        var currentDataGridControl = ( currentDataGridContext == null ) ? null : currentDataGridContext.DataGridControl;

        // We are only interested in containers that are part of the same grid.
        if( currentDataGridControl == dataGridControl )
        {
          container = childOrSelf as IDataGridItemContainer;

          // We have found the container;
          if( container != null )
            break;
        }

        // Since the element is the grid itself, we already know we will not find a suitable container beyond this point.
        if( childOrSelf == dataGridControl )
          break;

        childOrSelf = TreeHelper.GetParent( childOrSelf );
      }

      // RowSelector are handled differently.
      if( container == null )
      {
        var target = DataGridControl.GetContainer( originalChildOrSelf );

        if( TreeHelper.IsDescendantOf( target, dataGridControl ) )
        {
          container = target as IDataGridItemContainer;
        }
      }

      dataGridControl.CustomItemContainerGenerator.m_log.Assert( ( container == null ) || ( container is FrameworkElement ),
                                                                "( container == null ) || ( container is FrameworkElement )" );

      return container as FrameworkElement;
    }

    public void ResetGeneratorContent()
    {
      this.CleanupGenerator();
      this.EnsureNodeTreeCreated();
    }

    public DependencyObject ContainerFromIndex( int itemIndex )
    {
      if( this.IsHandlingGlobalItemsReset )
        return null;

      DependencyObject retval = null;

      this.EnsureNodeTreeCreated();

      //retrieve the genenerator index for the index specified
      int genPos = m_genPosToIndex.IndexOf( itemIndex );

      //if the generator index is -1, then the item is not realized
      if( genPos != -1 )
      {
        DetailGeneratorNode detailNode = m_genPosToNode[ genPos ] as DetailGeneratorNode;

        if( detailNode == null )
        {
          //remap the generator index on the container list.
          retval = m_genPosToContainer[ genPos ];
        }
      }

      return retval;
    }

    internal DependencyObject ContainerFromItem( object item )
    {
      if( this.IsHandlingGlobalItemsReset )
        return null;

      DependencyObject retval = null;

      //retrieve the genenerator index for the item specified
      int genPos = this.FindFirstGeneratedIndexForLocalItem( item );

      //if the generator index is -1, then the item is not realized
      if( genPos != -1 )
      {
        retval = m_genPosToContainer[ genPos ];
      }

      return retval;
    }

    internal List<object> GetRealizedDataItemsForGroup( GroupGeneratorNode group )
    {
      int count = m_genPosToNode.Count;
      List<object> items = new List<object>( count );

      for( int i = 0; i < count; i++ )
      {
        ItemsGeneratorNode node = m_genPosToNode[ i ] as ItemsGeneratorNode;

        if( ( node != null ) && ( node.Parent == group ) )
        {
          items.Add( m_genPosToItem[ i ] );
        }
      }

      return items;
    }

    internal List<object> GetRealizedDataItems()
    {
      int count = m_genPosToNode.Count;
      List<object> items = new List<object>( count );

      for( int i = 0; i < count; i++ )
      {
        ItemsGeneratorNode node = m_genPosToNode[ i ] as ItemsGeneratorNode;

        if( node != null )
        {
          items.Add( m_genPosToItem[ i ] );
        }
      }

      return items;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic" )]
    internal object ItemFromContainer( DependencyObject container )
    {
      if( this.IsHandlingGlobalItemsReset )
        return null;

      var dataItemStore = CustomItemContainerGenerator.GetDataItemProperty( container );
      if( ( dataItemStore == null ) || dataItemStore.IsEmpty )
        return null;

      var dataItem = dataItemStore.Data;
      var genPosIndex = m_genPosToContainer.IndexOf( container );

      if( genPosIndex != -1 )
      {
        DetailGeneratorNode detailNode = m_genPosToNode[ genPosIndex ] as DetailGeneratorNode;
        if( detailNode != null )
          return null;
      }

      return dataItem;
    }

    public int IndexFromItem( object item )
    {
      int retval = -1;

      if( item != null )
      {
        this.EnsureNodeTreeCreated();

        if( m_startNode != null )
        {
          //if item is generated
          int generatedIndex = this.FindFirstGeneratedIndexForLocalItem( item );
          if( generatedIndex != -1 )
          {
            retval = m_genPosToIndex[ generatedIndex ];
          }
          //if the item is not generated
          else
          {
            GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
            //Note: Under that specific case, I do not want to search through collapsed group nodes... Effectivelly, an item "below" a collapsed group node 
            // Have no index as per the "item to index" interface of the generator. 
            retval = nodeHelper.FindItem( item );
          }
        }
      }

      return retval;
    }

    public object ItemFromIndex( int index )
    {
      object retval = null;

      this.EnsureNodeTreeCreated();

      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );

      retval = nodeHelper.FindIndex( index );

      return retval;
    }

    public int GetGroupIndex( Group group )
    {
      if( group == null )
        DataGridException.ThrowSystemException( "group", typeof( ArgumentNullException ), m_dataGridControl.Name );

      this.EnsureNodeTreeCreated();

      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );

      if( nodeHelper.FindGroup( group.CollectionViewGroup ) )
        return nodeHelper.Index;

      //the item was not found in this generator's content... check all the detail generators
      foreach( var detailNode in this.GetDetailGeneratorNodes() )
      {
        int groupIndex = detailNode.DetailGenerator.GetGroupIndex( group );

        if( groupIndex > -1 )
          return groupIndex + this.FindGlobalIndexForDetailNode( detailNode );
      }

      return -1;
    }

    public Group GetGroupFromItem( object item )
    {
      Group retval = null;

      GeneratorNode nodeForItem = null;

      this.EnsureNodeTreeCreated();

      //item might be in the "generated" list... much quicker to find-out if it is!
      int itemGenPosIndex = this.FindFirstGeneratedIndexForLocalItem( item );
      if( itemGenPosIndex != -1 )
      {
        //item is generated...
        nodeForItem = m_genPosToNode[ itemGenPosIndex ];
      }
      else
      {
        //only try to find the item is the generator has some content...
        if( m_startNode != null )
        {
          GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
          if( nodeHelper.Contains( item ) ) //NOTE: this will only return items directly contained in this generator (not from details )
          {
            //if the nodeHelper was able to locate the content, use the nodeHelper's CurrentNode as the node for the item.
            nodeForItem = nodeHelper.CurrentNode;
          }
          else
          {
            DataGridException.ThrowSystemException( "An attempt was made to retrieve the group of an item that does not belong to the generator.",
                                                    typeof( InvalidOperationException ), m_dataGridControl.Name );
          }
        }
      }

      if( nodeForItem != null )
      {
        GroupGeneratorNode parentGroup = nodeForItem.Parent as GroupGeneratorNode;
        if( parentGroup != null )
        {
          retval = parentGroup.UIGroup;
        }
      }

      return retval;
    }

    public Group GetGroupFromCollectionViewGroup( CollectionViewGroup collectionViewGroup )
    {
      return this.GetGroupFromCollectionViewGroup( null, collectionViewGroup );
    }

    public Group GetGroupFromCollectionViewGroup( Group parentUIGroup, CollectionViewGroup collectionViewGroup )
    {
      this.EnsureNodeTreeCreated();
      GroupGeneratorNode groupGeneratorNode = null;

      if( m_groupNodeMappingCache.TryGetValue( collectionViewGroup, out groupGeneratorNode ) )
        return groupGeneratorNode.UIGroup;

      return null;
    }

    public CollectionViewGroup GetParentGroupFromItem( object item, bool recurseDetails )
    {
      if( item == null )
        DataGridException.ThrowSystemException( "item", typeof( ArgumentNullException ), m_dataGridControl.Name );

      CollectionViewGroup collectionViewGroup = null;

      if( !this.TryGetParentGroupFromItem( item, recurseDetails, out collectionViewGroup ) )
        DataGridException.ThrowSystemException( "An attempt was made to retrieve the parent group of an item that does not belong to the generator.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      return collectionViewGroup;
    }

    public bool TryGetParentGroupFromItem( object item, bool recurseDetails, out CollectionViewGroup collectionViewGroup )
    {
      collectionViewGroup = null;

      if( item == null )
        return false;

      this.EnsureNodeTreeCreated();

      if( m_startNode == null )
        throw new DataGridInternalException( "Start node is null for the CollectionViewGroup.", m_dataGridControl );

      //Invoke the helper that will check for the parent group within the local generator.
      if( this.TryGetParentGroupFromItemHelper( item, out collectionViewGroup ) )
      {
        //if the item was found within the local generator, then return.
        return true;
      }
      //If the item was not in the local generator, continue with method.

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
      CollectionViewGroup groupItem = item as CollectionViewGroup;
      if( groupItem != null )
      {
        Group group = this.GetGroupFromCollectionViewGroup( groupItem );
        if( group != null )
        {
          GroupGeneratorNode groupGeneratorNode = group.GeneratorNode;
          if( groupGeneratorNode.Parent == null )
          {
            //no parent for speficied item.
            return true;
          }
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
      int itemGenPosIndex = this.FindFirstGeneratedIndexForLocalItem( item );
      if( itemGenPosIndex != -1 )
      {
        //item was generated and was not from a DetailGeneratorNode
        if( m_genPosToNode[ itemGenPosIndex ].Parent == null )
        {
          //no parent for speficied item.
          return true;
        }
        collectionViewGroup = ( ( GroupGeneratorNode )m_genPosToNode[ itemGenPosIndex ].Parent ).CollectionViewGroup;
        return true;
      }

      //-----------------------------------------------
      //3 - Third check is to check of the item is a GroupHeaderFooterItem
      //-----------------------------------------------
      if( item.GetType() == typeof( GroupHeaderFooterItem ) )
      {
        GroupHeaderFooterItem groupHeaderFooterItem = ( GroupHeaderFooterItem )item;
        CollectionViewGroup parentGroup = groupHeaderFooterItem.Group;

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
      GeneratorNodeHelper finalNodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
      if( finalNodeHelper.AbsoluteFindItem( item ) )
      {
        //item was not generated but was part of this generator
        if( finalNodeHelper.CurrentNode.Parent == null )
        {
          //no parent for speficied item.
          return true;
        }
        collectionViewGroup = ( ( GroupGeneratorNode )finalNodeHelper.CurrentNode.Parent ).CollectionViewGroup;
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool ExpandGroup( CollectionViewGroup group )
    {
      bool groupExpanded = this.ExpandGroupCore( group, false );

      if( !groupExpanded )
        DataGridException.ThrowSystemException( "An attempt was made to expand a group that does not exist.", typeof( InvalidOperationException ), m_dataGridControl.Name );

      return groupExpanded;
    }

    internal bool ExpandGroupCore( CollectionViewGroup group, bool recurseDetails )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "An attempt was made to expand a group while the generator is busy generating items.",
                                                 typeof( InvalidOperationException ), m_dataGridControl.Name );

      if( group == null )
        DataGridException.ThrowSystemException( "group", typeof( ArgumentNullException ), m_dataGridControl.Name );

      this.EnsureNodeTreeCreated();

      if( m_firstItem == null )
        throw new DataGridInternalException( "No GeneratorNode found for the group.", m_dataGridControl );

      Group uiGroup = this.GetGroupFromCollectionViewGroup( group );
      if( uiGroup != null )
      {
        uiGroup.GeneratorNode.IsExpanded = true;
        return true;
      }

      if( recurseDetails )
      {
        //the item was not found in this generator's content... check all the detail generators
        foreach( var generator in this.GetDetailGenerators() )
        {
          // If the item is not found in the detail generator, it will return false;
          // The "public" function call will throw if item is never found in itself or its details.
          if( generator.ExpandGroupCore( group, recurseDetails ) )
            return true;
        }
      }

      return false;
    }

    public bool CollapseGroup( CollectionViewGroup group )
    {
      bool groupCollapsed = this.CollapseGroupCore( group, false );

      if( !groupCollapsed )
        DataGridException.ThrowSystemException( "An attempt was made to collapse a group that does not exist.", typeof( InvalidOperationException ), m_dataGridControl.Name );

      return groupCollapsed;
    }

    internal bool CollapseGroupCore( CollectionViewGroup group, bool recurseDetails )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "An attempt was made to collapse a group while the generator is busy generating items.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      if( group == null )
        DataGridException.ThrowSystemException( "group", typeof( ArgumentNullException ), m_dataGridControl.Name );

      this.EnsureNodeTreeCreated();

      if( m_firstItem == null )
        throw new DataGridInternalException( "No GeneratorNode found for the group.", m_dataGridControl );

      Group uiGroup = this.GetGroupFromCollectionViewGroup( group );
      if( uiGroup != null )
      {
        uiGroup.GeneratorNode.IsExpanded = false;
        return true;
      }

      if( recurseDetails )
      {
        //the item was not found in this generator's content... check all the detail generators
        foreach( var generator in this.GetDetailGenerators() )
        {
          // If the item is not found in the detail generator, it will return false;
          // The "public" function call will throw if item is never found in itself or its details.
          if( generator.CollapseGroupCore( group, recurseDetails ) )
            return true;
        }
      }

      return false;
    }

    public bool ToggleGroupExpansion( CollectionViewGroup group )
    {
      bool groupToggled = this.ToggleGroupExpansionCore( group, false );

      if( !groupToggled )
        DataGridException.ThrowSystemException( "An attempt was made to toggle a group that does not exist.", typeof( InvalidOperationException ), m_dataGridControl.Name );

      return groupToggled;
    }

    internal bool ToggleGroupExpansionCore( CollectionViewGroup group, bool recurseDetails )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "An attempt was made to a toggle a group's expansion while the generator is busy generating items.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      if( group == null )
        DataGridException.ThrowSystemException( "group", typeof( ArgumentNullException ), m_dataGridControl.Name );

      this.EnsureNodeTreeCreated();

      if( m_firstItem == null )
        throw new DataGridInternalException( "No GeneratorNode found for the group.", m_dataGridControl );

      Group uiGroup = this.GetGroupFromCollectionViewGroup( group );
      if( uiGroup != null )
      {
        GroupGeneratorNode groupNode = uiGroup.GeneratorNode;
        groupNode.IsExpanded = !groupNode.IsExpanded;
        return true;
      }

      if( recurseDetails )
      {
        //the item was not found in this generator's content... check all the detail generators
        foreach( var generator in this.GetDetailGenerators() )
        {
          try
          {
            return generator.ToggleGroupExpansionCore( group, recurseDetails );
          }
          catch( InvalidOperationException )
          {
            //if the item is not found in the detail generator, it will throw 
            //an invalid operation exception. If it doesn't throw, then it is 
            //safe to return the return value of the function.

            //otherwise, suppress this exception as ultimately, the "root" function call will 
            //throw if item is never found in itself or its details.
          }
        }
      }

      return false;
    }

    public bool IsGroupExpanded( CollectionViewGroup group )
    {
      return this.IsGroupExpandedCore( group, false );
    }

    internal bool IsGroupExpandedCore( CollectionViewGroup group, bool recurseDetails )
    {
      if( group == null )
      {
        DataGridException.ThrowSystemException( "group", typeof( ArgumentNullException ), m_dataGridControl.Name );
        //Simply there to remove compile error.
        return false;
      }

      this.EnsureNodeTreeCreated();

      if( m_firstItem == null )
        throw new DataGridInternalException( "No GeneratorNode found for the group.", m_dataGridControl );

      Group uiGroup = this.GetGroupFromCollectionViewGroup( group );
      if( uiGroup != null )
      {
        return uiGroup.GeneratorNode.IsExpanded;
      }

      if( recurseDetails )
      {
        //the group was not found in this generator, check in all the child generators for the group.
        foreach( var generator in this.GetDetailGenerators() )
        {
          try
          {
            return generator.IsGroupExpandedCore( group, recurseDetails );
          }
          catch( InvalidOperationException )
          {
            //if the item is not found in the detail generator, it will throw 
            //an invalid operation exception. If it doesn't throw, then it is 
            //safe to return the return value of the function.

            //otherwise, suppress this exception as ultimately, the "root" function call will 
            //throw if item is never found in itself or its details.
          }
        }
      }

      DataGridException.ThrowSystemException( "An attempt was made to consult the expansion state of a group that does not exist.",
                                              typeof( InvalidOperationException ), m_dataGridControl.Name );

      //Simply there to remove compile error.
      return false;
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
      if( dataItem == null )
        DataGridException.ThrowSystemException( "dataItem", typeof( ArgumentNullException ), m_dataGridControl.Name );

      return m_masterToDetails.ContainsKey( dataItem );
    }

    public void RemoveAllAndNotify()
    {
      if( m_startNode == null )
        return; //nothing to do, the generator is already in a "clean" state.

      //this message is issued by the DataGridContext when the VisibleColumns collection changes (re-ordering or changes to its content).
      IList<DependencyObject> removedContainers = new List<DependencyObject>();
      this.RemoveGeneratedItems( int.MinValue, int.MaxValue, removedContainers );

      this.SendResetEvent();

      removedContainers.Clear();
      removedContainers = null;

      removedContainers = m_dataGridContext.RecyclingManager.Clear();

      if( removedContainers.Count > 0 )
      {
        this.NotifyContainersRemoved( removedContainers );
      }
    }

    internal int FindIndexForItem( object item, DataGridContext dataGridContext )
    {
      int retval = -1;
      bool recurse = false;

      this.EnsureNodeTreeCreated();

      // If the seeked DataGridContext is this generator's context, the item should be here.
      // When the seeked DataGridContext is null, search for the first item match no matter the generator's context.
      if( ( m_dataGridContext == dataGridContext ) || ( dataGridContext == null ) )
      {
        retval = this.IndexFromItem( item );

        // If the item wasn't found in this generator's context and that we don't have a precise target context, recurse.
        if( ( retval == -1 ) && ( dataGridContext == null ) )
          recurse = true;
      }
      else
      {
        // The seeked DataGridContext is not this generator's context, recurse.
        recurse = true;
      }

      if( recurse )
      {
        foreach( KeyValuePair<object, List<DetailGeneratorNode>> masterToDetails in m_masterToDetails )
        {
          int detailsTotalCount = 0;
          foreach( DetailGeneratorNode detailNode in masterToDetails.Value )
          {
            int itemIndex = detailNode.DetailGenerator.FindIndexForItem( item, dataGridContext );
            if( itemIndex != -1 )
            {
              int masterIndex = this.IndexFromItem( masterToDetails.Key );

              m_log.Assert( this, masterIndex != -1, "masterIndex != -1" );

              retval = masterIndex + 1 + detailsTotalCount + itemIndex;
              break;
            }

            detailsTotalCount += detailNode.ItemCount;
          }
        }
      }

      return retval;
    }

    public DependencyObject GetRealizedContainerForIndex( int index )
    {
      //If the node tree is not created, then there can be no containers for the index.
      if( m_startNode == null )
        return null;

      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );

      if( !nodeHelper.FindNodeForIndex( index ) )
        DataGridException.ThrowSystemException( "The specified index does not correspond to an item in the generator.",
                                                typeof( ArgumentException ), m_dataGridControl.Name, "index" );

      int indexIndex = m_genPosToIndex.IndexOf( index );
      if( indexIndex == -1 )
        return null;

      return m_genPosToContainer[ indexIndex ];
    }

    public int GetRealizedIndexForContainer( DependencyObject container )
    {
      int containerIndex = m_genPosToContainer.IndexOf( container );
      if( containerIndex == -1 )
        return -1;

      return m_genPosToIndex[ containerIndex ];
    }

    internal List<int> GetMasterIndexexWithExpandedDetails()
    {
      List<int> masterIndexes = new List<int>();

      foreach( object dataItem in m_masterToDetails.Keys )
      {
        int itemIndex = m_collectionView.IndexOf( dataItem );

        m_log.Assert( this, itemIndex != -1, "itemIndex != -1" );

        masterIndexes.Add( itemIndex );
      }

      masterIndexes.Sort();

      return masterIndexes;
    }

    internal IEnumerable<DataGridContext> GetChildContextsForMasterItem( object item )
    {
      if( item != null )
      {
        List<DetailGeneratorNode> detailNodes = null;

        if( m_masterToDetails.TryGetValue( item, out detailNodes ) )
        {
          foreach( DetailGeneratorNode detailNode in detailNodes )
          {
            yield return detailNode.DetailContext;
          }
        }
      }
    }

    public DataGridContext GetChildContext( object parentItem, string relationName )
    {
      if( parentItem == null )
        DataGridException.ThrowSystemException( "parentItem", typeof( ArgumentNullException ), m_dataGridControl.Name );

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
          {
            return detailNode.DetailContext;
          }
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

      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
      return nodeHelper.Contains( item );
    }

    public object[] GetNamesTreeFromGroup( CollectionViewGroup group )
    {
      this.EnsureNodeTreeCreated();

      Group uiGroup = this.GetGroupFromCollectionViewGroup( group );
      if( uiGroup != null )
      {
        return uiGroup.GeneratorNode.NamesTree;
      }

      return null;
    }

    public CollectionViewGroup GetGroupFromNamesTree( object[] namesTree )
    {
      this.EnsureNodeTreeCreated();

      NamesTreeGroupFinderVisitor visitor = new NamesTreeGroupFinderVisitor( namesTree );

      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
      bool visitWasStopped;
      nodeHelper.ProcessVisit( m_dataGridContext, 0, int.MaxValue, visitor, DataGridContextVisitorType.Groups, false, out visitWasStopped );

      return visitor.Group;
    }

    #region Sticky Headers Methods

    public List<StickyContainerGenerated> GenerateStickyHeaders(
      DependencyObject container,
      bool areHeadersSticky,
      bool areGroupHeadersSticky,
      bool areParentRowsSticky )
    {
      List<StickyContainerGenerated> generatedStickyContainers = new List<StickyContainerGenerated>();

      GeneratorNode containerNode;
      int containerRealizedIndex;
      object containerDataItem;

      if( this.FindGeneratorListMappingInformationForContainer( container,
                                                                out containerNode,
                                                                out containerRealizedIndex,
                                                                out containerDataItem ) )
      {
        GeneratorNodeHelper nodeHelper = null;

        DetailGeneratorNode detailNode = containerNode as DetailGeneratorNode;

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
          if( index > -1 )
          {
            int sourceDataIndex = ( int )m_genPosToContainer[ index ].GetValue( DataGridVirtualizingPanel.ItemIndexProperty );
            containerNode = m_genPosToNode[ index ];
            containerRealizedIndex = m_genPosToIndex[ index ];

            CollectionGeneratorNode collectionNode = containerNode as CollectionGeneratorNode;
            if( collectionNode != null )
            {
              nodeHelper = new GeneratorNodeHelper(
                containerNode, containerRealizedIndex - collectionNode.IndexOf( containerDataItem ), sourceDataIndex );
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
            throw new DataGridInternalException( "The index of a sticky header container is out of bound.", m_dataGridControl );

          generatedStickyContainers.AddRange(
            this.GenerateStickyHeadersForDetail( container,
                                                 detailNode,
                                                 areHeadersSticky,
                                                 areGroupHeadersSticky,
                                                 areParentRowsSticky ) );
        }
        else
        {
          CollectionGeneratorNode collectionNode = containerNode as CollectionGeneratorNode;
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
        HeadersFootersGeneratorNode headersNode = nodeHelper.CurrentNode as HeadersFootersGeneratorNode;

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
        HeadersFootersGeneratorNode topMostHeaderNode = this.GetTopMostHeaderNode( nodeHelper );
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
      HeadersFootersGeneratorNode parentNode = null;

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
      List<StickyContainerGenerated> generatedStickyContainers =
        detailNode.DetailGenerator.GenerateStickyHeaders( container, areHeadersSticky, areGroupHeadersSticky, areParentRowsSticky );

      int detailIndex = this.FindGlobalIndexForDetailNode( detailNode );

      int count = generatedStickyContainers.Count;
      for( int i = 0; i < count; i++ )
      {
        StickyContainerGenerated stickyContainer = generatedStickyContainers[ i ];
        int detailItemIndex = stickyContainer.Index + detailIndex;

        //if the container was just realized, ensure to add it to the lists maintaining the generated items.
        if( stickyContainer.IsNewlyRealized )
        {
          int insertionIndex = this.FindInsertionPoint( detailItemIndex );

          m_genPosToIndex.Insert( insertionIndex, detailItemIndex );
          m_genPosToItem.Insert( insertionIndex, CustomItemContainerGenerator.GetDataItemProperty( stickyContainer.StickyContainer ).Data );
          m_genPosToContainer.Insert( insertionIndex, stickyContainer.StickyContainer );
          m_genPosToNode.Insert( insertionIndex, detailNode );
        }

        generatedStickyContainers[ i ] = new StickyContainerGenerated(
          stickyContainer.StickyContainer,
          detailItemIndex,
          stickyContainer.IsNewlyRealized );
      }

      return generatedStickyContainers;
    }

    private StickyContainerGenerated GenerateStickyParentRow( int itemNodeIndex )
    {
      ICustomItemContainerGenerator generator = ( ICustomItemContainerGenerator )this;
      GeneratorPosition position = generator.GeneratorPositionFromIndex( itemNodeIndex );

      using( generator.StartAt( position, GeneratorDirection.Forward, true ) )
      {
        bool isNewlyRealized;
        DependencyObject container = generator.GenerateNext( out isNewlyRealized );

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
      List<StickyContainerGenerated> generatedStickyContainers = new List<StickyContainerGenerated>();

      // The container is already part of the header.
      ICustomItemContainerGenerator generator = ( ICustomItemContainerGenerator )this;
      GeneratorPosition position = generator.GeneratorPositionFromIndex( headerNodeIndex );

      // In that case, the potential sticky containers are the requested one and up.
      using( generator.StartAt( position, GeneratorDirection.Forward, true ) )
      {
        for( int i = 0; i < headerNode.ItemCount; i++ )
        {
          // If the realized index represent the index of one of the HeaderNode
          // item, do not process
          if( ( isRealizedIndexPartOfHeaderNode )
              && ( headerNodeIndex + i > realizedIndex ) )
          {
            break;
          }

          object item = headerNode.GetAt( i );

          GroupHeaderFooterItem? groupHeaderFooterItem = null;

          if( item is GroupHeaderFooterItem )
            groupHeaderFooterItem = ( GroupHeaderFooterItem )item;

          if( ( groupHeaderFooterItem != null )
            && ( !( groupHeaderFooterItem.Value.Group.IsBottomLevel ) || !this.IsGroupExpanded( groupHeaderFooterItem.Value.Group ) ) )
          {
            this.Skip();
            continue;
          }

          bool isNewlyRealized;
          DependencyObject stickyContainer = generator.GenerateNext( out isNewlyRealized );

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
      int index = m_genPosToContainer.IndexOf( container );

      if( index == -1 )
      {
        containerNode = null;
        containerRealizedIndex = -1;
        containerDataItem = null;

        return false;
      }

      containerNode = m_genPosToNode[ index ];
      containerRealizedIndex = m_genPosToIndex[ index ];
      containerDataItem = m_genPosToItem[ index ];

      return true;
    }

    public int GetLastHoldingContainerIndexForStickyHeader( DependencyObject stickyHeader )
    {
      return this.GetLastHoldingContainerIndexForStickyHeaderRecurse( stickyHeader, this.ItemCount );
    }

    private int GetLastHoldingContainerIndexForStickyHeaderRecurse( DependencyObject stickyHeader, int parentCount )
    {
      int lastContainerIndex = 0;

      int containerRealizedIndex;
      GeneratorNode containerNode;
      object containerDataItem;

      if( this.FindGeneratorListMappingInformationForContainer( stickyHeader, out containerNode, out containerRealizedIndex, out containerDataItem ) )
      {
        DetailGeneratorNode detailNode = containerNode as DetailGeneratorNode;

        if( detailNode != null )
        {
          lastContainerIndex =
            detailNode.DetailGenerator.GetLastHoldingContainerIndexForStickyHeaderRecurse( stickyHeader, detailNode.ItemCount ) +
            this.FindGlobalIndexForDetailNode( detailNode );
        }
        else
        {
          ItemsGeneratorNode itemsNode = containerNode as ItemsGeneratorNode;

          if( itemsNode != null )
          {
            // This means that the sticky container is a MasterRow for a detail.
            if( this.AreDetailsExpanded( containerDataItem ) )
            {
              List<DetailGeneratorNode> detailNodesForDataItem = m_masterToDetails[ containerDataItem ];

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
            GeneratorNodeHelper nodeHelper = null;

            // This means that the sticky container is a Header.
            CollectionGeneratorNode collectionNode = containerNode as CollectionGeneratorNode;
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
      int firstContainerIndex = 0;

      int containerRealizedIndex;
      GeneratorNode containerNode;
      object containerDataItem;

      if( this.FindGeneratorListMappingInformationForContainer( stickyFooter, out containerNode, out containerRealizedIndex, out containerDataItem ) )
      {
        DetailGeneratorNode detailNode = containerNode as DetailGeneratorNode;

        if( detailNode != null )
        {
          int detailIndex = this.FindGlobalIndexForDetailNode( detailNode );
          firstContainerIndex = detailNode.DetailGenerator.GetFirstHoldingContainerIndexForStickyFooter( stickyFooter ) + detailIndex;
        }
        else
        {
          GeneratorNodeHelper nodeHelper = null;

          CollectionGeneratorNode collectionNode = containerNode as CollectionGeneratorNode;
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
      List<StickyContainerGenerated> generatedStickyContainers = new List<StickyContainerGenerated>();

      GeneratorNode containerNode;
      int containerRealizedIndex;
      object containerDataItem;


      if( this.FindGeneratorListMappingInformationForContainer( container,
                                                                out containerNode,
                                                                out containerRealizedIndex,
                                                                out containerDataItem ) )
      {
        GeneratorNodeHelper nodeHelper = null;
        DetailGeneratorNode detailNode = containerNode as DetailGeneratorNode;

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
          if( index > -1 )
          {
            int sourceDataIndex = ( int )m_genPosToContainer[ index ].GetValue( DataGridVirtualizingPanel.ItemIndexProperty );
            containerNode = m_genPosToNode[ index ];
            containerRealizedIndex = m_genPosToIndex[ index ];

            CollectionGeneratorNode collectionNode = containerNode as CollectionGeneratorNode;
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
            throw new DataGridInternalException( "The index of a sticky footer container is out of bound.", m_dataGridControl );

          generatedStickyContainers.AddRange(
            this.GenerateStickyFootersForDetail( container, detailNode, areFootersSticky, areGroupFootersSticky ) );
        }
        else
        {
          CollectionGeneratorNode collectionNode = containerNode as CollectionGeneratorNode;
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
        HeadersFootersGeneratorNode footersNode = nodeHelper.CurrentNode as HeadersFootersGeneratorNode;

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
        HeadersFootersGeneratorNode bottomFootersNode = this.GetDetailFootersNode( nodeHelper );

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
      List<StickyContainerGenerated> generatedStickyContainers =
        detailNode.DetailGenerator.GenerateStickyFooters( container, areFootersSticky, areGroupFootersSticky );

      int detailIndex = this.FindGlobalIndexForDetailNode( detailNode );

      int count = generatedStickyContainers.Count;
      for( int i = 0; i < count; i++ )
      {
        StickyContainerGenerated stickyContainer = generatedStickyContainers[ i ];
        int detailItemIndex = stickyContainer.Index + detailIndex;

        //if the container was just realized, ensure to add it to the lists maintaining the generated items.
        if( stickyContainer.IsNewlyRealized )
        {
          int insertionIndex = this.FindInsertionPoint( detailItemIndex );

          m_genPosToIndex.Insert( insertionIndex, detailItemIndex );
          m_genPosToItem.Insert( insertionIndex, CustomItemContainerGenerator.GetDataItemProperty( stickyContainer.StickyContainer ).Data );
          m_genPosToContainer.Insert( insertionIndex, stickyContainer.StickyContainer );
          m_genPosToNode.Insert( insertionIndex, detailNode );
        }

        generatedStickyContainers[ i ] = new StickyContainerGenerated(
          stickyContainer.StickyContainer,
          detailItemIndex,
          stickyContainer.IsNewlyRealized );
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
      List<StickyContainerGenerated> generatedStickyContainers = new List<StickyContainerGenerated>();

      int footersNodeItemCount = footerNode.ItemCount;

      // The container is already part of the footer.
      ICustomItemContainerGenerator generator = ( ICustomItemContainerGenerator )this;
      GeneratorPosition position = generator.GeneratorPositionFromIndex( footerNodeIndex + footersNodeItemCount - 1 );

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

          object item = footerNode.GetAt( i );

          GroupHeaderFooterItem? groupHeaderFooterItem = null;

          if( item is GroupHeaderFooterItem )
            groupHeaderFooterItem = ( GroupHeaderFooterItem )item;

          if( ( groupHeaderFooterItem != null )
            && ( !( groupHeaderFooterItem.Value.Group.IsBottomLevel ) || !this.IsGroupExpanded( groupHeaderFooterItem.Value.Group ) ) )
          {
            this.Skip();
            continue;
          }

          bool isNewlyRealized;
          DependencyObject stickyContainer = generator.GenerateNext( out isNewlyRealized );

          generatedStickyContainers.Add( new StickyContainerGenerated( stickyContainer, footerNodeIndex + i, isNewlyRealized ) );
        }
      }

      return generatedStickyContainers;
    }

    private HeadersFootersGeneratorNode GetDetailFootersNode( GeneratorNodeHelper nodeHelper )
    {
      HeadersFootersGeneratorNode parentNode = null;

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
      int stickyHeadersCount = 0;

      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );

      if( !nodeHelper.FindNodeForIndex( index ) )
      {
        // Unable to find the node, no sticky header for this node
        return 0;
      }

      GeneratorNode indexNode = nodeHelper.CurrentNode;

      ItemsGeneratorNode itemsGeneratorNode = indexNode as ItemsGeneratorNode;

      // We found an ItemsGeneratorNode
      if( itemsGeneratorNode != null )
      {
        GeneratorNode innerNode = itemsGeneratorNode.GetDetailNodeForIndex( index - nodeHelper.Index );

        DetailGeneratorNode detailNode = innerNode as DetailGeneratorNode;

        // Only do a special case if the index represent
        // a DetailNode
        if( detailNode != null )
        {
          if( ( areParentRowsSticky )
            && ( this.AreDetailsExpanded( detailNode.DetailContext.ParentItem ) ) )
          {
            stickyHeadersCount++;
          }

          int detailFirstRealizedIndex = this.FindGlobalIndexForDetailNode( detailNode );
          int correctedIndex = index - detailFirstRealizedIndex;

          m_log.Assert( this, correctedIndex >= 0, "correctedIndex >= 0 .. 1" );

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
      HeadersFootersGeneratorNode headersNode = nodeHelper.CurrentNode as HeadersFootersGeneratorNode;

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
      HeadersFootersGeneratorNode topMostHeaderNode = this.GetTopMostHeaderNode( nodeHelper );
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
      int stickyHeaderCount = 0;

      for( int i = 0; i < headerNode.ItemCount; i++ )
      {
        if( ( isRealizedIndexPartOfHeaderNode )
            && ( headerNodeIndex + i >= realizedIndex ) )
        {
          break;
        }

        object item = headerNode.GetAt( i );

        GroupHeaderFooterItem? groupHeaderFooterItem = null;

        if( item is GroupHeaderFooterItem )
          groupHeaderFooterItem = ( GroupHeaderFooterItem )item;

        if( ( groupHeaderFooterItem != null )
          && ( !( groupHeaderFooterItem.Value.Group.IsBottomLevel ) || !this.IsGroupExpanded( groupHeaderFooterItem.Value.Group ) ) )
        {
          continue;
        }

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
      int stickyFooterCount = 0;

      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );

      if( !nodeHelper.FindNodeForIndex( index ) )
      {
        // Unable to find the node, no sticky header for this node
        return 0;
      }

      GeneratorNode indexNode = nodeHelper.CurrentNode;

      ItemsGeneratorNode itemsGeneratorNode = indexNode as ItemsGeneratorNode;

      // We found an ItemsGeneratorNode
      if( itemsGeneratorNode != null )
      {
        GeneratorNode innerNode = itemsGeneratorNode.GetDetailNodeForIndex( index - nodeHelper.Index );

        DetailGeneratorNode detailNode = innerNode as DetailGeneratorNode;

        if( detailNode != null )
        {
          int detailFirstRealizedIndex = this.FindGlobalIndexForDetailNode( detailNode );
          int correctedIndex = index - detailFirstRealizedIndex;

          m_log.Assert( this, correctedIndex >= 0, "correctedIndex >= 0 .. 2" );

          stickyFooterCount +=
            detailNode.DetailGenerator.GetStickyFooterCountForIndex( correctedIndex,
                                                                     areFootersSticky,
                                                                     areGroupFootersSticky );
        }
      }

      // We want to find the HeaderFooterGeneratorNode for the container 
      // node. This is to find the footers for the container.
      nodeHelper.MoveToEnd();
      HeadersFootersGeneratorNode footersNode = nodeHelper.CurrentNode as HeadersFootersGeneratorNode;

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
      HeadersFootersGeneratorNode bottomFootersNode = this.GetDetailFootersNode( nodeHelper );

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
      int stickyFooterCount = 0;

      int footersNodeItemCount = footerNode.ItemCount;
      for( int i = footersNodeItemCount - 1; i >= 0; i-- )
      {
        if( ( isRealizedIndexPartOfHeaderNode )
            && ( footerNodeIndex + i < realizedIndex ) )
        {
          continue;
        }

        object item = footerNode.GetAt( i );

        GroupHeaderFooterItem? groupHeaderFooterItem = null;

        if( item is GroupHeaderFooterItem )
          groupHeaderFooterItem = ( GroupHeaderFooterItem )item;

        if( ( groupHeaderFooterItem != null )
          && ( !( groupHeaderFooterItem.Value.Group.IsBottomLevel ) || !this.IsGroupExpanded( groupHeaderFooterItem.Value.Group ) ) )
        {
          continue;
        }

        stickyFooterCount++;
      }

      return stickyFooterCount;
    }

    #endregion

    #region IItemContainerGenerator Members

    DependencyObject IItemContainerGenerator.GenerateNext( out bool isNewlyRealized )
    {
      if( this.Status != GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "The Generator is not active: StartAt() was not called prior calling GenerateNext() or the returned IDisposable was already disposed of.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      DependencyObject container = null;
      isNewlyRealized = false;

      if( m_generatorNodeHelper == null ) //the m_generatorNodeHelper will be turned to null when we reach the end of the list ot items.
        return null;

      m_log.Assert( this, !( m_generatorNodeHelper.CurrentNode is GroupGeneratorNode ), "Algorithm should not allow the Generator's m_generatorNodeHelper to be on a GroupGeneratorNode" );

      GeneratorNode node = m_generatorNodeHelper.CurrentNode;

      if( node == null )
        throw new DataGridInternalException( "CurrentNode is null.", m_dataGridControl );

      if( !( node is CollectionGeneratorNode ) )
        throw new DataGridInternalException( "CurrentNode is not a CollectionGeneratorNode.", m_dataGridControl );

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
        Debug.Indent();
        int insertionIndex = this.FindInsertionPoint( m_generatorCurrentGlobalIndex );
        Debug.Unindent();

        m_genPosToIndex.Insert( insertionIndex, m_generatorCurrentGlobalIndex );
        m_genPosToItem.Insert( insertionIndex, CustomItemContainerGenerator.GetDataItemProperty( container ).Data );
        m_genPosToContainer.Insert( insertionIndex, container );
        m_genPosToNode.Insert( insertionIndex, node );

        this.ValidateIndexOrder( "genpos" + insertionIndex.ToString() + ", globalIndex" + m_generatorCurrentGlobalIndex.ToString() + ")" );
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

    DependencyObject IItemContainerGenerator.GenerateNext()
    {
      IItemContainerGenerator generatorInterface = this;
      bool flag;
      return generatorInterface.GenerateNext( out flag );
    }

    public GeneratorPosition GeneratorPositionFromIndex( int itemIndex )
    {
      this.EnsureNodeTreeCreated();

      int genPosIndex = m_genPosToIndex.IndexOf( itemIndex );
      //if the Index maps to a Generated item, then return the GeneratorPosition (easy)
      if( genPosIndex != -1 )
      {
        return new GeneratorPosition( genPosIndex, 0 );
      }
      else
      {
        //If not generated
        int storedIndex = -1;
        int offset = itemIndex + 1;

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
    }

    private GeneratorPosition GetSafeGeneratorPositionFromIndex( int itemIndex )
    {
      using( this.DeferDetailsRemap() )
      {
        return this.GeneratorPositionFromIndex( itemIndex );
      }
    }

    ItemContainerGenerator IItemContainerGenerator.GetItemContainerGeneratorForPanel( Panel panel )
    {
      return ( ( IItemContainerGenerator )( ( ItemsControl )m_dataGridControl ).ItemContainerGenerator ).GetItemContainerGeneratorForPanel( panel );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <returns>The index returned is the GlobalRealizedIndex</returns>
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
        DataGridException.ThrowSystemException( "The Generator is not active: StartAt() was not called prior calling PrepareItemContainer() or the returned IDisposable was already disposed of.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

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
        DataGridException.ThrowSystemException( "Cannot perform this operation while the generator is busy generating items",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      if( position.Offset != 0 )
        DataGridException.ThrowSystemException( "The GeneratorPosition to remove cannot map to a non-realized item.", typeof( InvalidOperationException ), m_dataGridControl.Name );

      if( position.Index == -1 )
        DataGridException.ThrowSystemException( "he GeneratorPosition to remove cannot map to a non-realized item.", typeof( InvalidOperationException ), m_dataGridControl.Name );

      //if the index passed is within the array!
      if( position.Index < m_genPosToIndex.Count )
      {
        //remove the items requested.
        for( int i = 0; i < count; i++ )
        {
          if( this.RemoveGeneratedItem( position.Index, null ) == 0 )
            //This case deserves a more solid approach... We do not want to allow removal from the user panel of an item that somehow is not present.
            throw new DataGridInternalException( "Trying to remove an item at an out-of-bound GenratorPosition index.", m_dataGridControl );
        }

        //No need to update the Generator current generation, since the content of the generator items did not change (only the list of realized items).
        //m_currentGeneratorContentGeneration++;
      }
      else
      {
        throw new DataGridInternalException( "Trying to remove an item at an out-of-bound GenratorPosition index", m_dataGridControl );
      }

    }

    void IItemContainerGenerator.RemoveAll()
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "Cannot perform this operation while the generator is busy generating items",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      using( this.SetIsHandlingGlobalItemsResetLocally() )
      {
        //Call to remove all shall not request container recycling panels to remove their containers. Therefore, I am not collecting the remove containers.
        this.RemoveAllGeneratedItems();
      }

      //No need to update the Generator current generation, since the content of the generator items did not change (only the list of realized items).
    }

    IDisposable IItemContainerGenerator.StartAt( GeneratorPosition position, GeneratorDirection direction, bool allowStartAtRealizedItem )
    {
      this.SetIsInUse();

      if( this.Status == GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "Cannot perform this operation while the generator is busy generating items",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

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
            //      If it does that correctly, then this function is OK.
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
        //if the container recylcing is turned OFF from ON
        if( value == ( bool )m_flags[ ( int )CustomItemContainerGeneratorFlags.RecyclingEnabled ] )
          return;

        if( !value )
        {
          //clear the container queues
          m_dataGridContext.RecyclingManager.Clear();
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

      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );

      //First, locate the item within the Generator.
      object newCurrentItem = nodeHelper.FindIndex( newCurrentIndex );

      if( newCurrentItem == null )
        DataGridException.ThrowSystemException( "An attempt was made to access an item at an index that does not correspond to an item.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      //Then, if the item is within an ItemsNode, check if it belongs to a detail
      ItemsGeneratorNode itemsNode = nodeHelper.CurrentNode as ItemsGeneratorNode;
      if( itemsNode != null )
      {
        int masterIndex;
        int detailIndex;
        int detailNodeIndex;

        DetailGeneratorNode detailNode = itemsNode.GetDetailNodeForIndex( newCurrentIndex - nodeHelper.Index, out masterIndex, out detailIndex, out detailNodeIndex );
        //If it belongs to a detail
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
      int index = -1;
      if( m_dataGridControl.CurrentContext != null )
      {
        index = this.FindIndexForItem( m_dataGridControl.CurrentContext.InternalCurrentItem, m_dataGridContext );
      }

      return index;
    }

    void ICustomItemContainerGenerator.RestoreFocus( DependencyObject container )
    {
      if( !m_genPosToContainer.Contains( container ) )
        DataGridException.ThrowSystemException( "The specified container is not part of the generator's content.",
                                                typeof( ArgumentException ), m_dataGridControl.Name, "container" );

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( container );
      ColumnBase column = ( dataGridContext == null ) ? null : dataGridContext.CurrentColumn;
      m_dataGridControl.SetFocusHelper( container as UIElement, column, false, true );
    }

    #endregion

    private DependencyObject GenerateNextLocalContainer( out bool isNewlyRealized )
    {
      DependencyObject container = null;
      object dataItem;
      CollectionGeneratorNode node = m_generatorNodeHelper.CurrentNode as CollectionGeneratorNode;

      m_log.Assert( this, node != null, "CustomItemContainerGenerator.GenerateNextLocalContainer: node is null." );

      ItemsGeneratorNode itemsNode = node as ItemsGeneratorNode;

      //if the index exists in the list, then the item is already realized.
      int genPosIndex = m_genPosToIndex.IndexOf( m_generatorCurrentGlobalIndex );

      if( genPosIndex != -1 )
      {
        //retrieve the container for the item that is already stored in the data structure
        container = m_genPosToContainer[ genPosIndex ];
        dataItem = m_genPosToItem[ genPosIndex ];
        GeneratorNode tempNode = m_genPosToNode[ genPosIndex ];
        node = tempNode as CollectionGeneratorNode;

        if( tempNode == null )
        {
          this.LogState();
          m_log.Fail( this, "CustomItemContainerGenerator.GenerateNextLocalContainer: ### cached Node is null ( at " + genPosIndex.ToString() + " )" );

          throw new DataGridInternalException( "CustomItemContainerGenerator.GenerateNextLocalContainer: cached Node is null", m_dataGridControl );
        }
        else if( node != m_generatorNodeHelper.CurrentNode )
        {
          this.LogState();
          m_log.Fail( this, "CustomItemContainerGenerator.GenerateNextLocalContainer: # testNode is not the current one. ( " + m_genPosToNode[ genPosIndex ].GetType().ToString() + " ) ( at " + genPosIndex.ToString() + " )" );

          throw new DataGridInternalException( "CustomItemContainerGenerator.GenerateNextLocalContainer: Node is not the current one.", m_dataGridControl );
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

        //this section is used to ensure that the Item Index is properly attached to all containers
        DataGridVirtualizingPanel.SetItemIndex( container, itemIndex );

        SelectionRange currentSelection = new SelectionRange( itemIndex );

        DataRow rowToSelect = container as DataRow;

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
        GroupConfiguration groupConfig = null;
        GroupGeneratorNode parentNode = node.Parent as GroupGeneratorNode; //implicit rule: a parent node is always a GroupGeneratorNode
        if( parentNode != null )
        {
          groupConfig = parentNode.GroupConfiguration;
        }

        if( groupConfig != null )
        {
          DataGridControl.SetContainerGroupConfiguration( container, groupConfig );
        }
      }
      else
      {
        this.LogState();
        m_log.Fail( this, "CustomItemContainerGenerator.GenerateNextLocalContainer: node is null ( at " + genPosIndex.ToString() + " )" );
      }

      return container;
    }

    private void UpdateDataVirtualizationLockForItemsNode( ItemsGeneratorNode itemsNode, object dataItem, bool applyLock )
    {
      IList items = itemsNode.Items;

      ItemCollection itemCollection = items as ItemCollection;

      if( itemCollection != null )
      {
        DataGridVirtualizingCollectionViewBase dataGridVirtualizingCollectionViewBase =
          itemCollection.SourceCollection as DataGridVirtualizingCollectionViewBase;

        if( dataGridVirtualizingCollectionViewBase != null )
        {
          int index = itemsNode.Items.IndexOf( dataItem );
          if( applyLock )
          {
            dataGridVirtualizingCollectionViewBase.RootGroup.LockGlobalIndex( index );
          }
          else
          {
            dataGridVirtualizingCollectionViewBase.RootGroup.UnlockGlobalIndex( index );
          }
        }
      }
      else
      {
        VirtualList virtualItemList = items as VirtualList;

        if( virtualItemList != null )
        {
          int index = itemsNode.Items.IndexOf( dataItem );

          if( applyLock )
          {
            virtualItemList.LockPageForLocalIndex( index );
          }
          else
          {
            virtualItemList.UnlockPageForLocalIndex( index );
          }
        }
      }

    }

    private void MoveGeneratorForward()
    {
      ItemsGeneratorNode currentMasterNode = m_generatorNodeHelper.CurrentNode as ItemsGeneratorNode;

      //if I was generating from a detail generator
      if( m_generatorCurrentDetail != null )
      {
        m_log.Assert( this, currentMasterNode != null, "CustomItemContainerGenerator.MoveGeneratorForward: currentMasterNode is not null." );
        m_log.Assert( this, currentMasterNode.Details != null, "CustomItemContainerGenerator.MoveGeneratorForward: currentMasterNode.Details is not null." );

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

          m_log.Assert( this, detailsforMasterNode != null, "CustomItemContainerGenerator.MoveGeneratorForward: detailsforMasterNode is not null." );

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
          bool foundDetail = false;

          for( int i = 0; i < detailsForNode.Count; i++ )
          {
            DetailGeneratorNode detailNode = detailsForNode[ i ];

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
      ItemsGeneratorNode currentMasterNode = m_generatorNodeHelper.CurrentNode as ItemsGeneratorNode;

      //if I was generating from a detail generator
      if( m_generatorCurrentDetail != null )
      {
        m_log.Assert( this, currentMasterNode != null, "CustomItemContainerGenerator.MoveGeneratorBackward: currentMasterNode is not null." );
        m_log.Assert( this, currentMasterNode.Details != null, "CustomItemContainerGenerator.MoveGeneratorBackward: currentMasterNode.Details is not null." );

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

            m_log.Assert( this, detailsforMasterNode != null, "CustomItemContainerGenerator.MoveGeneratorBackward: detailsforMasterNode is not null." );

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

      if( ( node != null ) && ( node.Details != null )
        && ( node.Details.TryGetValue( m_generatorCurrentOffset, out detailsForNode ) ) )
      {
        for( int i = detailsForNode.Count - 1; i >= 0; i-- )
        {
          DetailGeneratorNode detailNode = detailsForNode[ i ];

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
        DataGridException.ThrowSystemException( "An attempt was made to call the CustomItemContainerGenerator.ShouldDrawBottomLine method while containers are not being generated.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      return ( m_generatorCurrentGlobalIndex == ( this.ItemCount - 1 ) );
    }

    private bool ItemHasExpandedDetails()
    {
      if( m_generatorStatus != GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "An attempt was made to call the CustomItemContainerGenerator.ItemHasExpandedDetails method while containers are not being generated.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      ItemsGeneratorNode itemsNode = m_generatorNodeHelper.CurrentNode as ItemsGeneratorNode;
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
      m_log.Assert( this, !this.IsHandlingGlobalItemsResetLocally, "Generator is already processing a HandleGlobalItemReset or CleanupGenerator" );

      if( this.IsHandlingGlobalItemsResetLocally )
        return;

      using( m_log.BeginBlock( this, null ) )
      using( this.SetIsHandlingGlobalItemsResetLocally() )
      {
        this.RemoveAllGeneratedItems();

        IList<DependencyObject> removedContainers = null;

        var recyclingManager = m_dataGridContext.RecyclingManager;
        recyclingManager.RemoveRef( this );
        if( recyclingManager.RefCount == 0 )
        {
          removedContainers = m_dataGridContext.RecyclingManager.Clear();
        }

        if( !isNestedCall )
        {
          this.SendResetEvent();
        }

        //This call needs not to be defered or moved... effectivelly, the reset has already occured in that case...
        if( ( removedContainers != null ) && ( removedContainers.Count > 0 ) )
        {
          this.NotifyContainersRemoved( removedContainers );
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
      }
    }

    private void CleanupDetailRelations()
    {
      using( m_log.BeginBlock( this, null ) )
      {
        var detailNodes = this.GetDetailGeneratorNodes().ToList();

        m_masterToDetails.Clear();

        foreach( var detailNode in detailNodes )
        {
          this.ClearDetailGeneratorNode( detailNode );
        }

        m_floatingDetails.Clear();
      }
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
      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Remove:
          {
            break;
          }

        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Add:
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Reset:
        default:
          {
            using( m_log.BeginBlock( this, "DetailConfiguration Changed" ) )
            {
              // That ensure the RemapFloatingDetails() got called and the loop on m_masterToDetails.Keys will not contain invalid item.
              this.EnsureNodeTreeCreated();
            }

            break;
          }
      }
    }

    private void OnCollectionViewGroupsPropertyChanged()
    {
      if( m_groupsCollection == m_collectionView.Groups )
        return;

      this.HandleGlobalItemsReset();
    }

    private void OnItemsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      // Avoid re-entrance when processing a global reset
      if( this.IsHandlingGlobalItemsResetLocally )
        return;

      if( this.Status == GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "Cannot perform this operation while the generator is busy generating items.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Add:
          //do not handle the Items.CollectionChanged if groups are present or if there is already a items node.
          // (because the OnGeneratorNodeItemsCollectionChanged will handle things)
          if( m_collectionView.GroupDescriptions.Count == 0 )
          {
            if( m_firstItem == null )
            {

              int addCount = e.NewItems.Count;
              GeneratorPosition genPos = new GeneratorPosition( -1, 1 );

              this.IncrementCurrentGenerationCount();

              this.SendAddEvent( genPos, 0, addCount );
            }
          }
          break;

        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Remove:
        case NotifyCollectionChangedAction.Replace:
          //I voluntarilly do not handle these particular cases because the ItemsNode
          //handler will cover it. (OnGeneratorNodeItemsCollectionChanged)
          break;

        case NotifyCollectionChangedAction.Reset:
          if( ( m_groupsCollection == null ) || ( m_collectionView.GroupDescriptions.Count == 0 ) )
          {
            this.HandleGlobalItemsReset();
          }

          break;
      }
    }

    private void OnGroupsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      // Avoid re-entrance when processing a global reset
      if( this.IsHandlingGlobalItemsResetLocally )
        return;

      if( this.Status == GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "Cannot perform this operation while the generator is busy generating items.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      //this fonction is only used to process the content of the DataGridControl.Items.Groups collection...
      // for the CollectionChanged event of branch groups ( IsBottomLevel = false ), refer to the 
      // OnBranchGroupsChanged fonction

      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Add:
          int addCount = e.NewItems.Count;

          GeneratorPosition genPos = new GeneratorPosition( -1, 1 ); //this would map to the first item in the list if not generated.

          int addIndex = -1;

          //if the first item is empty, do not do anything, the structure will be generated when the generator is started!
          if( m_firstItem != null )
          {
            //The only moment where the m_firstItem is null is typically when a reset occured...
            //other moments is when there are 0 items (in which case, the.

            GeneratorNode addNode = this.HandleSameLevelGroupAddition( m_firstItem, out addCount, e );

            this.IncrementCurrentGenerationCount();

            GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( addNode, 0, 0 );//index not important, will reserve find it.
            nodeHelper.ReverseCalculateIndex();
            addIndex = nodeHelper.Index;

            genPos = this.GeneratorPositionFromIndex( addIndex );
          }

          this.SendAddEvent( genPos, addIndex, addCount );

          break;

        case NotifyCollectionChangedAction.Move:
          if( m_firstItem != null )
          {
            if( !( m_firstItem is GroupGeneratorNode ) )
              throw new DataGridInternalException( "Trying to move a GeneratorNode that is not a GroupGeneratorNode.", m_dataGridControl );

            m_log.Assert( this, e.OldStartingIndex != e.NewStartingIndex, "An attempt was made to move a group to the same location." );

            this.HandleSameLevelGroupMove( m_firstItem, e );
          }
          break;

        case NotifyCollectionChangedAction.Remove:
          if( m_firstItem != null )
          {
            if( !( m_firstItem is GroupGeneratorNode ) )
              throw new DataGridInternalException( "Trying to remove a GeneratorNode that is not a GroupGeneratorNode.", m_dataGridControl );

            int remCount;
            int generatedRemCount;
            int removeIndex;
            List<DependencyObject> removedContainers = new List<DependencyObject>();
            GeneratorPosition remPos = this.HandleSameLevelGroupRemove( m_firstItem, out remCount, out generatedRemCount, out removeIndex, e, removedContainers );

            //there is no need to check if the parent node is expanded or not... since the first level of group cannot be collapsed.

            this.IncrementCurrentGenerationCount();

            this.SendRemoveEvent( remPos, removeIndex, remCount, generatedRemCount, removedContainers );
          }

          break;

        case NotifyCollectionChangedAction.Replace:
          DataGridException.ThrowSystemException( "Replace not supported at the moment on groups.", typeof( NotSupportedException ), m_dataGridControl.Name );
          break;

        case NotifyCollectionChangedAction.Reset:
          if( m_firstItem != null )
          {
            if( !( m_firstItem is GroupGeneratorNode ) )
              throw new DataGridInternalException( "Trying to reset a GeneratorNode that is not a GroupGeneratorNode.", m_dataGridControl );

            this.HandleSameLevelGroupReset( m_firstItem );
          }
          else
          {
            this.HandleGlobalItemsReset();
          }
          break;
      }

    }

    private void OnGeneratorNodeItemsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      // Avoid re-entrance when processing a global reset
      if( this.IsHandlingGlobalItemsResetLocally )
        return;

      if( this.Status == GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "Cannot perform this operation while the generator is busy generating items.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      var node = sender as GeneratorNode;
      if( node == null )
        return;

      var itemsNode = sender as ItemsGeneratorNode;
      var headersNode = sender as HeadersFootersGeneratorNode;

      e = e.GetRangeActionOrSelf();

      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Add:
          this.HandleItemAddition( node, e );
          break;

        case NotifyCollectionChangedAction.Remove:
          if( itemsNode != null )
          {
            this.HandleItemRemoveMoveReplace( itemsNode, e );
          }
          else if( headersNode != null )
          {
            this.HandleHeaderFooterRemove( headersNode, e );
          }
          else
          {
            throw new DataGridInternalException( "Cannot remove the GeneratorNode as it is not of a valid type.", m_dataGridControl );
          }
          break;

        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Replace:
          if( itemsNode != null )
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
            }
            else
            {
              //any other case, normal handling
              this.HandleItemRemoveMoveReplace( itemsNode, e );
            }
          }
          else if( headersNode != null )
          {
            this.HandleHeaderFooterReplace( headersNode, e );
          }
          else
          {
            throw new DataGridInternalException( "Cannot move or replace the GeneratorNode as it is not of a valid type.", m_dataGridControl );
          }
          break;

        case NotifyCollectionChangedAction.Reset:
          this.HandleItemReset( node );
          break;
      }
    }

    private void OnGeneratorNodeExpansionStateChanged( object sender, ExpansionStateChangedEventArgs e )
    {
      //throw an error is the Generator is actually busy generating!
      if( this.Status == GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "Cannot perform this operation while the generator is busy generating items.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      GeneratorNode node = sender as GeneratorNode;

      m_log.Assert( this, node != null, "node != null" );

      if( node == null )
        return;

      GroupGeneratorNode changedNode = node.Parent as GroupGeneratorNode;

      m_log.Assert( this, changedNode != null, "changedNode != null" ); //should never be null, as the "node" is supposed to be the child node of this one.

      //Determine if the changedNode is "below" a collapsed group (because if so, I don't need any sort of notification or removal ).
      GroupGeneratorNode parentGroupNode = changedNode.Parent as GroupGeneratorNode;
      if( ( parentGroupNode != null ) && ( !parentGroupNode.IsComputedExpanded ) )
        return;

      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( node, 0, 0 );
      nodeHelper.ReverseCalculateIndex();

      //if the node was "Collapsed"
      if( !e.NewExpansionState )
      {
        int removeCount = e.Count;
        int startIndex = nodeHelper.Index + e.IndexOffset;

        GeneratorPosition removeGenPos = this.GeneratorPositionFromIndex( startIndex );

        List<DependencyObject> removedContainers = new List<DependencyObject>();
        //remove the Generated items between the appropriate indexes
        int removeUICount = this.RemoveGeneratedItems( startIndex, startIndex + removeCount - 1, removedContainers );

        if( removeCount > 0 )
        {
          //send the event so the panel can remove the group elements
          this.SendRemoveEvent( removeGenPos, startIndex, removeCount, removeUICount, removedContainers );
        }
      }
      //if the node was "Expanded" 
      else
      {
        int addCount = e.Count;
        int startIndex = nodeHelper.Index + e.IndexOffset;
        GeneratorPosition addGenPos = this.GeneratorPositionFromIndex( startIndex );

        if( addCount > 0 )
        {
          //send the event so the panel can add the group elements
          this.SendAddEvent( addGenPos, startIndex, addCount );
        }
      }
    }

    private void OnGroupGeneratorNodeIsExpandedChanging( object sender, EventArgs e )
    {
      //throw an error is the Generator is actually busy generating!
      if( this.Status == GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "Cannot perform this operation while the generator is busy generating items.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      m_log.Assert( this, m_currentGenPosToIndexInhibiterDisposable == null, "m_currentGenPosToIndexInhibiterDisposable == null" );

      m_currentGenPosToIndexInhibiterDisposable = this.InhibitParentGenPosToIndexUpdate();

      GroupGeneratorNode groupGeneratorNode = sender as GroupGeneratorNode;
      if( ( m_dataGridContext != null )
        && ( !m_dataGridContext.IsDeferRestoringState )
        && ( !m_dataGridContext.IsRestoringState )
        && ( m_dataGridControl != null )
        && ( groupGeneratorNode != null )
        && ( groupGeneratorNode.IsExpanded ) )
      {
        TableflowViewItemsHost tableflowItemsHost = m_dataGridControl.ItemsHost as TableflowViewItemsHost;
        if( tableflowItemsHost != null )
        {
          tableflowItemsHost.OnGroupCollapsing( groupGeneratorNode.UIGroup );
        }
      }
    }

    private void OnGroupGeneratorNodeIsExpandedChanged( object sender, EventArgs e )
    {
      //throw an error is the Generator is actually busy generating!
      if( this.Status == GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "Cannot perform this operation while the generator is busy generating items.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      GroupGeneratorNode node = sender as GroupGeneratorNode;
      if( node != null )
      {
        this.IncrementCurrentGenerationCount();
      }

      if( m_currentGenPosToIndexInhibiterDisposable != null )
      {
        m_currentGenPosToIndexInhibiterDisposable.Dispose();
        m_currentGenPosToIndexInhibiterDisposable = null;
      }
    }

    private void OnGeneratorNodeGroupsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "Cannot perform this operation while the generator is busy generating items.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      GroupGeneratorNode node = sender as GroupGeneratorNode;

      if( node != null )
      {
        switch( e.Action )
        {
          case NotifyCollectionChangedAction.Add:
            {
              int addCount;
              GeneratorNode addNode = this.HandleParentGroupAddition( node, out addCount, e );

              if( node.IsComputedExpanded )
              {
                GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( addNode, 0, 0 );//index not important, will reserve find it.
                nodeHelper.ReverseCalculateIndex();

                this.IncrementCurrentGenerationCount();

                GeneratorPosition genPos = this.GeneratorPositionFromIndex( nodeHelper.Index );

                this.SendAddEvent( genPos, nodeHelper.Index, addCount );
              }
            }
            break;

          case NotifyCollectionChangedAction.Move:
            {
              if( node.Child == null )
                throw new DataGridInternalException( "An attempt was made to move a group with a null child GeneratorNode.", m_dataGridControl );

              m_log.Assert( this, e.OldStartingIndex != e.NewStartingIndex, "An attempt was made to move a group to the same location." );

              this.HandleSameLevelGroupMove( node.Child, e );
            }
            break;

          case NotifyCollectionChangedAction.Remove:
            {
              if( node.Child == null )
                throw new DataGridInternalException( "An attempt was made to remove a group with a null child GeneratorNode.", m_dataGridControl );

              int remCount;
              int generatedRemCount;
              int removeIndex;
              List<DependencyObject> removedContainers = new List<DependencyObject>();
              GeneratorPosition remPos = this.HandleParentGroupRemove( node, out remCount, out generatedRemCount, out removeIndex, e, removedContainers );

              if( node.IsComputedExpanded )
              {
                this.IncrementCurrentGenerationCount();

                this.SendRemoveEvent( remPos, removeIndex, remCount, generatedRemCount, removedContainers );
              }
            }
            break;

          case NotifyCollectionChangedAction.Replace:
            DataGridException.ThrowSystemException( "Replace not supported at the moment on groups.", typeof( NotSupportedException ), m_dataGridControl.Name );
            break;

          case NotifyCollectionChangedAction.Reset:
            {
              if( node.Child == null )
                throw new DataGridInternalException( "An attempt was made to reset a group with a null child GeneratorNode.", m_dataGridControl );

              this.HandleSameLevelGroupReset( node.Child );
            }
            break;
        }
      }
    }

    private void OnViewThemeChanged( object sender, EventArgs e )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "Cannot perform this operation while the generator is busy generating items.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

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

    private void ResetNodeList()
    {

      this.UpdateHeaders( m_dataGridContext.Headers );

      int addCount;
      this.SetupInitialItemsNodes( out addCount );

      this.UpdateFooters( m_dataGridContext.Footers );

      //Add self to the RecyclingManager ref count manager.
      //This is to ensure that the list of container availlable for recycling will be preserved taking this generator into consideration
      m_dataGridContext.RecyclingManager.AddRef( this );
    }

    private int ClearItems()
    {
      int retval = 0;

      //if the first item is not null, that means that we have items within the grid.
      if( m_firstItem != null )
      {
        GeneratorNode previous = m_firstItem.Previous;

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

        retval = removeCount;
      }

      return retval;
    }

    private int RemoveGeneratedItem( int index, IList<DependencyObject> removedContainers )
    {
      //basic error handling
      if( ( index < 0 ) || ( index >= m_genPosToIndex.Count ) )
        return 0;

      object item = m_genPosToItem[ index ];
      DependencyObject container = m_genPosToContainer[ index ];
      GeneratorNode node = m_genPosToNode[ index ];

      m_log.Assert( this, ( m_genPosToContainer.Count != 0 )
                     && ( m_genPosToIndex.Count != 0 )
                     && ( m_genPosToItem.Count != 0 )
                     && ( m_genPosToNode.Count != 0 ), "Internal Generator's lists are empty, cannot continue to process RemoveGeneratedItems" );

      //remove the item from the 4 lists... (same as doing a "remove")
      this.GenPosArraysRemoveAt( index );

      if( removedContainers != null )
      {
        removedContainers.Add( container );
      }

      DetailGeneratorNode detailNode = node as DetailGeneratorNode;

      //to ensure this is only done once, check if the node associated with the container is a Detail or not
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
      int index = m_genPosToContainer.IndexOf( container );

      if( index == -1 )
        return 0;

      return this.RemoveGeneratedItem( index, null );
    }

    private int RemoveGeneratedItems( int startIndex, int endIndex, IList<DependencyObject> removedContainers )
    {
      if( startIndex > endIndex )
        return 0;

      var count = m_genPosToIndex.Count;
      if( ( count <= 0 ) || ( m_genPosToIndex[ 0 ] > endIndex ) || ( m_genPosToIndex[ count - 1 ] < startIndex ) )
        return 0;

      var removed = 0;

      //cycle through the list of generated items, and see if any items are generated between the indexes in the removed range.
      //start from the end so that removing an item will not cause the indexes to shift.
      for( var i = count - 1; i >= 0; i-- )
      {
        m_log.Assert( this, ( m_genPosToContainer.Count != 0 )
                         && ( m_genPosToIndex.Count != 0 )
                         && ( m_genPosToItem.Count != 0 )
                         && ( m_genPosToNode.Count != 0 ), "Internal Generator's lists are empty, cannot continue to process RemoveGeneratedItems" );

        var index = m_genPosToIndex[ i ];

        //if the item is within the range removed
        if( ( index >= startIndex ) && ( index <= endIndex ) )
        {
          //this will ensure to recurse the call to the appropriate Detail Generator for clearing of the container.
          //otherwise, it will only remove it from the list of container generated in the current generator instance.
          this.RemoveGeneratedItem( i, removedContainers );

          removed++;
        }
      }

      return removed;
    }

    private int RemoveGeneratedItems( GeneratorNode referenceNode, IList<DependencyObject> removedContainers )
    {
      int genCountRemoved = 0;

      List<GeneratorNode> toRemove = new List<GeneratorNode>();
      toRemove.Add( referenceNode );

      ItemsGeneratorNode itemsNode = referenceNode as ItemsGeneratorNode;
      if( itemsNode != null )
      {
        if( itemsNode.Details != null )
        {
          //cycle through all the details currently mapped to the reference node
          foreach( KeyValuePair<int, List<DetailGeneratorNode>> detailsForNode in itemsNode.Details )
          {
            foreach( DetailGeneratorNode detailNode in detailsForNode.Value )
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
        GeneratorNode node = m_genPosToNode[ i ];

        //if the item is within the range removed
        if( toRemove.Contains( node ) )
        {
          //this will ensure to recurse the call to the appropriate Detail Generator for clearing of the container.
          //otherwise, it will only remove it from the list of container generated in the current generator instance.
          this.RemoveGeneratedItem( i, removedContainers );

          //increment realized item count removed
          genCountRemoved++;
        }
      }

      return genCountRemoved;
    }

    private void RemoveAllGeneratedItems()
    {
      m_log.Assert( this.IsHandlingGlobalItemsReset, "IsHandlingGlobalItemsReset is not set." );

      using( m_log.BeginBlock( this, null ) )
      {
        //Call RemoveAllGeneratedItems() on all the detail generators
        foreach( var generator in this.GetDetailGenerators() )
        {
          generator.RemoveAllGeneratedItems();
        }

        //then we can clean the list of items of items generated held by this generator
        int genCountRemoved = m_genPosToNode.Count;

        //start from the end so that removing an item will not cause the indexes to shift
        for( int i = genCountRemoved - 1; i >= 0; i-- )
        {
          GeneratorNode node = m_genPosToNode[ i ];
          DependencyObject container = m_genPosToContainer[ i ];

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
    }

    private bool RemoveGeneratedItems( HeadersFootersGeneratorNode referenceNode, object referenceItem, IList<DependencyObject> removedContainers )
    {

      bool retval = false;

      //cycle through the list of generated items, and see if any items are generated between the indexes in the removed range.
      //start from the end so that removing an item will not cause the indexes to shift
      for( int i = m_genPosToNode.Count - 1; i >= 0; i-- )
      {
        GeneratorNode node = m_genPosToNode[ i ];

        //if the item is within the range removed
        if( node == referenceNode )
        {
          object item = m_genPosToItem[ i ];

          if( item.Equals( referenceItem ) )
          {
            DependencyObject container = m_genPosToContainer[ i ];
            removedContainers.Add( container );

            this.RemoveContainer( container, referenceItem );

            retval = true;

            this.GenPosArraysRemoveAt( i );

            break;
          }
        }
      }

      return retval;
    }

    private void GenPosArraysRemoveAt( int index )
    {
      //remove the item from the 4 lists... (same as doing a "remove")
      ItemsGeneratorNode itemsNode = m_genPosToNode[ index ] as ItemsGeneratorNode;

      // Unlock DataVirtualization hold on item's page.
      if( itemsNode != null )
        this.UpdateDataVirtualizationLockForItemsNode( itemsNode, m_genPosToItem[ index ], false );

      m_genPosToContainer.RemoveAt( index );
      m_genPosToIndex.RemoveAt( index );
      m_genPosToItem.RemoveAt( index );
      m_genPosToNode.RemoveAt( index );
    }

    private void UpdateHeaders( IList headers )
    {

      //if there was no header prior this udpate.
      if( m_firstHeader == null )
      {
        //create the node(s) that would contain the headers
        m_firstHeader = this.CreateHeaders( headers );

        int count;
        int chainLength;
        GeneratorNodeHelper.EvaluateChain( m_firstHeader, out count, out chainLength );

        //if there was items present in the linked list.
        if( m_startNode != null )
        {
          //headers are automatically inserted at the beginning
          GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
          nodeHelper.InsertBefore( m_firstHeader );
        }
        else
        {
          m_log.WriteLine( this, "UpdateHeaders - new tree" );
        }

        //set the start node as the first header node.
        m_startNode = m_firstHeader;
      }

      //If the m_firstHeader is not NULL, then there is nothing to do, since the Header node contains an  observable collection
      //which we monitor otherwise.
    }

    private void UpdateFooters( IList footers )
    {

      //if there was no header prior this udpate.
      if( m_firstFooter == null )
      {
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

            m_log.WriteLine( this, "UpdateFooters - new tree" );
          }
        }
      }

      //If the m_firstHeader is not NULL, then there is nothing to do, since the Header node contains an  observable collection
      //which we monitor otherwise.
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
      GeneratorNode newItemNode = null;

      addCount = 0;

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
            GeneratorNode originalPrevious = m_firstFooter.Previous;
            GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_firstFooter, 0, 0 ); //do not care about index!

            nodeHelper.InsertBefore( m_firstItem );

            if( originalPrevious == null ) //that means that the first footer is the first item
            {
              m_startNode = newItemNode;
            }
          }
          else if( m_firstHeader != null ) //if there is no footer but some headers, add it at the end.
          {
            GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_firstHeader, 0, 0 ); //do not care about index!

            nodeHelper.MoveToEnd();
            nodeHelper.InsertAfter( m_firstItem );
          }
          else
          {
            //this case should not be possible: no header, no footers but there is a startNode
            throw new DataGridInternalException( "No start node found for the current GeneratorNode.", m_dataGridControl );
          }
        }
        else
        {
          m_startNode = m_firstItem;

          m_log.WriteLine( this, "SetupInitialItemsNodes - new tree" );
        }
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

      return this.NodeFactory.CreateItemsGeneratorNode( list, null, null, null, this );
    }

    private GeneratorNode CreateGroupListFromCollection( IList collection, GeneratorNode parentNode )
    {
      if( collection.Count == 0 )
        return null;

      GeneratorNode rootNode = null;
      GeneratorNode previousNode = null;
      GroupGeneratorNode actualNode = null;

      GeneratorNode childNode = null;
      int level = ( parentNode == null ) ? 0 : parentNode.Level + 1;
      GroupConfiguration groupConfig;
      bool initiallyExpanded;

      ObservableCollection<GroupDescription> groupDescriptions = DataGridContext.GetGroupDescriptionsHelper( m_collectionView );
      GroupConfigurationSelector groupConfigurationSelector = m_dataGridContext.GroupConfigurationSelector;
      LateGroupLevelDescription lateGroupLevelDescription = this.CreateOrGetLateGroupLevelDescription( level, groupDescriptions );

      foreach( CollectionViewGroup group in collection )
      {
        groupConfig = GroupConfiguration.GetGroupConfiguration( m_dataGridContext, groupDescriptions, groupConfigurationSelector, level, group );

        m_log.Assert( this, groupConfig != null, "groupConfig != null" );

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

        GeneratorNodeHelper childNodeHelper = new GeneratorNodeHelper( childNode, 0, 0 ); //do not care about index.
        childNodeHelper.MoveToEnd(); //extensibility, just in case SetupGroupHeaders() ever return a node list.

        IList<object> subItems = group.GetItems();

        //if the node newly created is not the bottom level
        if( !group.IsBottomLevel )
        {
          if( ( subItems != null ) && ( subItems.Count > 0 ) )
          {
            GeneratorNode subGroupsNode = this.CreateGroupListFromCollection( subItems as IList, actualNode );
            if( subGroupsNode != null )
            {
              childNodeHelper.InsertAfter( subGroupsNode );
            }
          }
        }
        else
        {
          //this is the bottom level, create an Items node
          GeneratorNode itemsNode = this.NodeFactory.CreateItemsGeneratorNode( subItems as IList, actualNode, null, null, this );
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

    private GeneratorPosition HandleParentGroupRemove( GeneratorNode parent, out int countRemoved, out int genCountRemoved, out int removeIndex, NotifyCollectionChangedEventArgs e, IList<DependencyObject> removedContainers )
    {
      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( parent, 0, 0 ); //do not care about index (for now).

      // start by moving to the first child... of the node (GroupHeaders node, most probably).
      // false parameter is to prevent skipping over a collapsed node (item count 0 )
      if( !nodeHelper.MoveToChild( false ) )
        //could not advance to the child item so there is no items to be removed...
        throw new DataGridInternalException( "No child item in the group to remove.", m_dataGridControl );

      return this.HandleSameLevelGroupRemove( nodeHelper.CurrentNode, out countRemoved, out genCountRemoved, out removeIndex, e, removedContainers );
    }

    private GeneratorPosition HandleSameLevelGroupRemove( GeneratorNode firstChild, out int countRemoved, out int genCountRemoved, out int removeIndex, NotifyCollectionChangedEventArgs e, IList<DependencyObject> removedContainers )
    {
      GeneratorPosition retval;

      countRemoved = 0;
      genCountRemoved = 0;

      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( firstChild, 0, 0 );
      nodeHelper.ReverseCalculateIndex();

      //Advance to the first "Group" node (skip the GroupHeaders)
      while( !( nodeHelper.CurrentNode is GroupGeneratorNode ) )
      {
        if( !nodeHelper.MoveToNext() )
          throw new DataGridInternalException( "Unable to move to next GeneratorNode.", m_dataGridControl );
      }

      //then move up to the removal start point.
      if( !nodeHelper.MoveToNextBy( e.OldStartingIndex ) )
        throw new DataGridInternalException( "Unable to move to the requested generator index.", m_dataGridControl );

      GroupGeneratorNode startNode = nodeHelper.CurrentNode as GroupGeneratorNode;
      removeIndex = -1;

      //Only fetch the index if the group itself is not "collapsed" or under a collapsed group already
      if( ( startNode.IsExpanded == startNode.IsComputedExpanded ) && ( startNode.ItemCount > 0 ) )
      {
        removeIndex = nodeHelper.Index;
        retval = this.GeneratorPositionFromIndex( removeIndex );
      }
      else
      {
        retval = new GeneratorPosition( -1, 1 );
      }

      //retrieve the generator position for the first item to remove.

      this.ProcessGroupRemoval( startNode, e.OldItems.Count, true, out countRemoved );

      //Clean the chain "isolated" previously
      this.NodeFactory.CleanGeneratorNodeTree( startNode );

      if( removeIndex != -1 )
      {
        //remove the appropriate 
        genCountRemoved = this.RemoveGeneratedItems( removeIndex, removeIndex + countRemoved - 1, removedContainers );
      }

      return retval;
    }

    private GroupGeneratorNode ProcessGroupRemoval( GeneratorNode startNode, int removeCount, bool updateGroupNodeMappingCache, out int countRemoved )
    {
      m_log.Assert( this, removeCount != 0, "remove count cannot be 0" );

      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( startNode, 0, 0 );//index not important.
      GroupGeneratorNode parentGroup = startNode.Parent as GroupGeneratorNode;
      int i = 0;

      countRemoved = 0;

      do
      {
        GroupGeneratorNode group = nodeHelper.CurrentNode as GroupGeneratorNode;

        if( updateGroupNodeMappingCache )
        {
          m_groupNodeMappingCache.Remove( group.CollectionViewGroup );
        }

        m_log.Assert( this, group != null, "node to be removed must be a GroupGeneratorNode" );

        //add the total number of child to the count of items removed.
        countRemoved += group.ItemCount;

        i++;

        if( i < removeCount )
        {
          if( !nodeHelper.MoveToNext() )
            throw new DataGridInternalException( "Could not move to the last node to be removed.", m_dataGridControl );
        }
      }
      while( i < removeCount );

      //disconnect the node chain to be removed from the linked list.
      GeneratorNode previous = startNode.Previous;
      GeneratorNode next = nodeHelper.CurrentNode.Next;

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
      }

      m_log.Assert( this, nodeHelper.CurrentNode is GroupGeneratorNode, "last node is not a GroupGeneratorNode" );

      return ( GroupGeneratorNode )nodeHelper.CurrentNode;
    }

    private void HandleSameLevelGroupReset( GeneratorNode node )
    {
      Debug.Assert( node != null );

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

      var groupsAdded = default( ICollection<CollectionViewGroup> );
      var groupsRemoved = default( ICollection<CollectionViewGroup> );
      var groupsMoved = default( ICollection<CollectionViewGroup> );

      this.GetGroupChanges( oldGroups, newGroups, out groupsAdded, out groupsRemoved, out groupsMoved );

      if( groupsAdded.Count + groupsRemoved.Count + groupsMoved.Count > CustomItemContainerGenerator.GlobalItemsResetThreshold )
      {
        this.HandleGlobalItemsReset();
      }
      else
      {
        this.ApplyGroupChanges( node, oldGroups, newGroups, groupsAdded, groupsRemoved, groupsMoved );
      }
    }

    private void GetGroupChanges(
      IList<CollectionViewGroup> oldGroups,
      IList<CollectionViewGroup> newGroups,
      out ICollection<CollectionViewGroup> groupsAdded,
      out ICollection<CollectionViewGroup> groupsRemoved,
      out ICollection<CollectionViewGroup> groupsMoved )
    {
      Debug.Assert( oldGroups != null );
      Debug.Assert( newGroups != null );

      groupsAdded = new HashSet<CollectionViewGroup>();
      groupsRemoved = new HashSet<CollectionViewGroup>();
      groupsMoved = new HashSet<CollectionViewGroup>();

      var oldGroupsPositions = new Dictionary<CollectionViewGroup, int>();

      for( var i = 0; i < oldGroups.Count; i++ )
      {
        oldGroupsPositions.Add( oldGroups[ i ], i );
      }

      var sequence = new List<int>( newGroups.Count );
      var isAlive = new BitArray( oldGroups.Count, false );

      foreach( var group in newGroups )
      {
        int index;

        if( oldGroupsPositions.TryGetValue( group, out index ) )
        {
          isAlive[ index ] = true;
          sequence.Add( index );
        }
        else
        {
          Debug.Assert( !m_groupNodeMappingCache.ContainsKey( group ), "How come there is a node for the group." );

          groupsAdded.Add( group );
        }
      }

      var hasNotMoved = new BitArray( oldGroups.Count, false );

      // The subsequence contains the position of the old groups that are in the new groups and that have not moved.
      foreach( var index in LongestIncreasingSubsequence.Find( sequence ) )
      {
        hasNotMoved[ index ] = true;
      }

      for( var i = 0; i < oldGroups.Count; i++ )
      {
        if( isAlive[ i ] )
        {
          if( !hasNotMoved[ i ] )
          {
            groupsMoved.Add( oldGroups[ i ] );
          }
        }
        else
        {
          groupsRemoved.Add( oldGroups[ i ] );
        }
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
              addNodeHelper.InsertAfter( groupNode );
            }
            else
            {
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
                  addNodeHelper.InsertBefore( groupNode );
                }
                else if( m_firstHeader != null )
                {
                  var addNodeHelper = new GeneratorNodeHelper( m_firstHeader, 0, 0 );
                  addNodeHelper.MoveToEnd();
                  addNodeHelper.InsertAfter( groupNode );
                }
                else
                {
                  throw new DataGridInternalException( "No start node found for the current GeneratorNode.", m_dataGridControl );
                }
              }
              else
              {
                m_startNode = groupNode;
              }
            }
            else
            {
              Debug.Assert( parentNode.Child != null );

              var addNodeHelper = new GeneratorNodeHelper( parentNode.Child, 0, 0 );
              addNodeHelper.MoveToEnd();
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
          addNodeHelper.InsertAfter( groupNode );
        }
      }

      this.IncrementCurrentGenerationCount();
      this.SendResetEvent();
    }

    private void HandleSameLevelGroupMove( GeneratorNode node, NotifyCollectionChangedEventArgs e )
    {
      GroupGeneratorNode parentGroup = node.Parent as GroupGeneratorNode;

      //Start a NodeHelper on the first child of the node where the move occured.
      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( node, 0, 0 );
      nodeHelper.ReverseCalculateIndex(); //determine index of the node.

      //Advance to the first "Group" node (skip the GroupHEaders)
      while( !( nodeHelper.CurrentNode is GroupGeneratorNode ) )
      {
        if( !nodeHelper.MoveToNext() )
          throw new DataGridInternalException( "Unable to move to next GeneratorNode.", m_dataGridControl );
      }

      //then move up to the removal start point.
      if( !nodeHelper.MoveToNextBy( e.OldStartingIndex ) )
        throw new DataGridInternalException( "Unable to move to the requested generator index.", m_dataGridControl );

      //remember the current node as the start point of the move (will be used when "extracting the chain")
      GeneratorNode startNode = nodeHelper.CurrentNode;
      //also remember the index of the node, to calculate range of elements to remove (containers )
      int startIndex = nodeHelper.Index;

      //then, cumulate the total number of items in the groups concerned
      int totalCountRemoved = 0;

      node = this.ProcessGroupRemoval( startNode, e.OldItems.Count, false, out totalCountRemoved );

      //send a message to the panel to remove the visual elements concerned 
      GeneratorPosition removeGenPos = this.GeneratorPositionFromIndex( startIndex );

      List<DependencyObject> removedContainers = new List<DependencyObject>();
      int genCountRemoved = this.RemoveGeneratedItems( startIndex, startIndex + totalCountRemoved - 1, removedContainers );

      this.SendRemoveEvent( removeGenPos, startIndex, totalCountRemoved, genCountRemoved, removedContainers );

      //reset the node parameter for the "re-addition"
      node = ( parentGroup != null ) ? parentGroup.Child : m_firstItem;

      if( node == null )
        throw new DataGridInternalException( "No node found to move.", m_dataGridControl );

      //Once the chain was pulled out, re-insert it at the appropriate location.
      nodeHelper = new GeneratorNodeHelper( node, 0, 0 ); //do not care about the index for what I need

      //Advance to the first "Group" node (skip the GroupHEaders)
      while( !( nodeHelper.CurrentNode is GroupGeneratorNode ) )
      {
        if( !nodeHelper.MoveToNext() )
          throw new DataGridInternalException( "Unable to move to next GeneratorNode.", m_dataGridControl );
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
        }

        //reinsert the chain at the specified location.
        nodeHelper.InsertBefore( startNode );
      }
      else
      {
        nodeHelper.InsertAfter( startNode );
      }

      //and finally, call to increment the generation count for the generator content
      this.IncrementCurrentGenerationCount();
    }

    private GeneratorNode HandleSameLevelGroupAddition( GeneratorNode firstChild, out int countAdded, NotifyCollectionChangedEventArgs e )
    {
      m_log.Assert( this, ( ( firstChild.Parent == null ) || ( firstChild.Parent is GroupGeneratorNode ) ), "parent of the node should be a GroupGeneratorNode" );

      GeneratorNode newNodeChain = this.CreateGroupListFromCollection( e.NewItems, firstChild.Parent );

      countAdded = 0;
      if( newNodeChain != null )
      {
        int chainLength;
        GeneratorNodeHelper.EvaluateChain( newNodeChain, out countAdded, out chainLength );

        GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( firstChild, 0, 0 ); //do not care about index.

        //Advance to the first "Group" node (skip the GroupHEaders)
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

        bool insertAfter = false;
        //If there is 0 group in the parent group, then this loop will exit without executing the control block once...
        for( int i = 0; i < e.NewStartingIndex; i++ )
        {
          if( !nodeHelper.MoveToNext() )
          {
            insertAfter = true;
          }
        }

        //if we are inserting past the end of the linked list level.
        if( insertAfter )
        {
          nodeHelper.InsertAfter( newNodeChain );
        }
        else
        {
          //we are inserting in the middle of the list
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
        }

      }

      return newNodeChain;
    }

    private GeneratorNode HandleParentGroupAddition( GeneratorNode parent, out int countAdded, NotifyCollectionChangedEventArgs e )
    {
      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( parent, 0, 0 ); //do not care about index (for now).

      //start by moving to the first child... of the node (GroupHeaders node, most probably).
      if( !nodeHelper.MoveToChild( false ) ) 
        throw new DataGridInternalException( "Could not move to the child node to be removed.", m_dataGridControl );

      return this.HandleSameLevelGroupAddition( nodeHelper.CurrentNode, out countAdded, e );
    }

    private void HandleItemAddition( GeneratorNode node, NotifyCollectionChangedEventArgs e )
    {
      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( node, 0, 0 ); //index not important for now.

      node.AdjustItemCount( e.NewItems.Count );

      ItemsGeneratorNode itemsNode = node as ItemsGeneratorNode;
      if( itemsNode != null )
      {
        itemsNode.AdjustLeafCount( e.NewItems.Count );
        this.OffsetDetails( itemsNode, e.NewStartingIndex, e.NewItems.Count );
      }

      //if the node is totally expanded
      if( node.IsComputedExpanded )
      {
        nodeHelper.ReverseCalculateIndex();

        //invalidate the indexes
        this.IncrementCurrentGenerationCount();

        int startIndex = nodeHelper.Index + e.NewStartingIndex;
        GeneratorPosition addGenPos = this.GeneratorPositionFromIndex( startIndex );

        //and send notification message
        this.SendAddEvent( addGenPos, startIndex, e.NewItems.Count );
      }
      else
      {
        //An item can be added in the source without the need to regenerate the containers, but still provoke a change to the realized containers' corresponding source index.
        this.UpdateContainersIndex();
      }
    }

    private void HandleItemRemoveMoveReplace( ItemsGeneratorNode node, NotifyCollectionChangedEventArgs e )
    {
      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( node, 0, 0 ); //index not important for now.
      nodeHelper.ReverseCalculateIndex();

      node.AdjustItemCount( -e.OldItems.Count );
      node.AdjustLeafCount( -e.OldItems.Count );

      int nodeStartIndex = e.OldStartingIndex;
      int nodeEndIndex = nodeStartIndex + e.OldItems.Count - 1;
      int detailCountToRemove = CustomItemContainerGenerator.ComputeDetailsCount( node, nodeStartIndex, nodeEndIndex );
      int detailCountBeforeRemovedItems = 0;
      if( nodeStartIndex > 0 )
      {
        detailCountBeforeRemovedItems = CustomItemContainerGenerator.ComputeDetailsCount( node, 0, nodeStartIndex - 1 );
      }

      int startIndex = nodeHelper.Index + e.OldStartingIndex + detailCountBeforeRemovedItems;
      int endIndex = startIndex + detailCountToRemove + e.OldItems.Count - 1;

      int removeCount = e.OldItems.Count + detailCountToRemove;
      int replaceCount = ( e.Action == NotifyCollectionChangedAction.Replace ) ? e.NewItems.Count : 0;

      //Remove the details from the ItemsGeneratorNode and re-index the other details appropriatly.
      this.RemoveDetails( node, nodeStartIndex, nodeEndIndex, replaceCount );

      GeneratorPosition removeGenPos = this.GetSafeGeneratorPositionFromIndex( startIndex );

      //Try to remap the old item for detail remapping (will do nothing if item has no details )
      foreach( object oldItem in e.OldItems )
      {
        this.QueueDetailItemForRemapping( oldItem );
      }

      bool needsUpdateToIndexes = true;
      //if the node is totally expanded
      if( node.IsComputedExpanded )
      {
        needsUpdateToIndexes = false;
        List<DependencyObject> removedContainers = new List<DependencyObject>();
        int genRemCount = this.RemoveGeneratedItems( startIndex, endIndex, removedContainers );

        this.IncrementCurrentGenerationCount();

        this.SendRemoveEvent( removeGenPos, startIndex, removeCount, genRemCount, removedContainers );
      }

      //then, based on the action that was performed (move, replace or remove)
      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Move:
          this.OffsetDetails( node, e.NewStartingIndex, e.NewItems.Count );
          this.HandleItemMoveRemoveReplaceHelper( node, e, nodeHelper.Index );
          break;

        case NotifyCollectionChangedAction.Replace:
          this.HandleItemMoveRemoveReplaceHelper( node, e, nodeHelper.Index );
          break;

        case NotifyCollectionChangedAction.Remove:
          if( needsUpdateToIndexes )
          {
            //An item can be removed from the source without the need to regenerate the containers,
            //but still provoke a change to the realized containers' corresponding source index.
            this.UpdateContainersIndex();
          }
          break;

        case NotifyCollectionChangedAction.Add:
        case NotifyCollectionChangedAction.Reset:
          DataGridException.ThrowSystemException( "Add or Reset not supported at the moment on groups.", typeof( NotSupportedException ), m_dataGridControl.Name );
          break;
      }

    }

    private void HandleItemMoveRemoveReplaceHelper( ItemsGeneratorNode node, NotifyCollectionChangedEventArgs e, int nodeIndex )
    {
      node.AdjustItemCount( e.NewItems.Count );
      node.AdjustLeafCount( e.NewItems.Count );

      this.IncrementCurrentGenerationCount();

      //this is used to notify any Master Generator that it must update its content's status (UpdateGenPosToIndexList)
      this.SendAddEvent( new GeneratorPosition( -1, 1 ), nodeIndex + e.NewStartingIndex, e.NewItems.Count );
    }

    private void UpdateContainersIndex()
    {
      DataGridCollectionView collectionView = m_collectionView.SourceCollection as DataGridCollectionView;
      GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
      int currentIndex = -1;
      int offset = 0;
      int sourceItemIndex;

      foreach( object item in m_genPosToItem )
      {
        currentIndex++;

        //If the item is not found, it means it belongs to a detail grid, no need to process it.
        if( nodeHelper.FindItem( item ) == -1 )
          continue;

        //If we're not on a ItemsGeneratorNode, it means we are not on a DataRow, so no need to process it (all ItemIndex == -1 in this case).
        ItemsGeneratorNode itemsNode = nodeHelper.CurrentNode as ItemsGeneratorNode;
        if( itemsNode == null )
        {
          offset = 0;
          continue;
        }

        //If we have a DataGridCollectionView, let's get the sorted index from it in case there are details expanded.
        if( collectionView != null )
        {
          sourceItemIndex = collectionView.GetGlobalSortedIndexFromDataItem( item );
        }
        //If not a DGCV, it means there are no details, so this will work fine.
        else
        {
          if( offset == 0 )
          {
            //This will make sure we get the right index in case the group is patially scrolled.
            offset = itemsNode.IndexOf( item );
          }
          sourceItemIndex = nodeHelper.SourceDataIndex + offset++;
        }
        DataGridVirtualizingPanel.SetItemIndex( m_genPosToContainer[ currentIndex ], sourceItemIndex );
      }
    }

    private void OffsetDetails( ItemsGeneratorNode node, int startIndex, int addOffset )
    {
      if( ( node.Details == null ) || ( node.Details.Count == 0 ) )
        return;

      int detailsCount = node.Details.Count;
      //first, create an array that will contain the keys for all the expanded details from the ItemsGeneratorNode
      int[] keys = new int[ detailsCount ];
      node.Details.Keys.CopyTo( keys, 0 );

      //sort the array, this will prevent any operation that will duplicate keys in the dictionary.
      Array.Sort<int>( keys );

      //loop from the end of the sorted array to the beginning. to ensuyre
      for( int i = detailsCount - 1; i >= 0; i-- )
      {
        int key = keys[ i ];

        //only process the key if it is in the processed range
        if( key >= startIndex )
        {
          List<DetailGeneratorNode> details;
          if( node.Details.TryGetValue( key, out details ) )
          {
            m_log.WriteLine( this, "details.Offset for Add - IN" + node.GetHashCode().ToString() + "- Di" + key.ToString() + " - new Di" + ( key + addOffset ).ToString() );

            node.Details.Remove( key );
            node.Details.Add( key + addOffset, details );
          }
          else
          {
            //Key not found in the dictionary, something wrong is going on.
            throw new DataGridInternalException( "Key not found in Details dictionary.", m_dataGridControl );
          }
        }
      }
    }

    private void RemoveDetails( ItemsGeneratorNode node, int nodeStartIndex, int nodeEndIndex, int replaceCount )
    {
      if( ( node.Details == null ) || ( node.Details.Count == 0 ) )
        return;

      int removeCount = nodeEndIndex - nodeStartIndex + 1 - replaceCount;
      //Note: If a replace was invoked, replace count will be greater than 0, and will be used to properly re-offset the 
      //details beyond the initial remove range.

      //Note2: for the case of a move or remove, the replace count must remain 0, so that the other details are correctly offseted.

      //first, create an array that will contain the keys for all the expanded details from the ItemsGeneratorNode
      int[] keys = new int[ node.Details.Count ];
      node.Details.Keys.CopyTo( keys, 0 );

      //sort the array, this will prevent any operation that will duplicate keys in the dictionary.
      Array.Sort<int>( keys );

      //cycle through all of the old items
      int countDetailsRemoved = 0;
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
            foreach( DetailGeneratorNode detailNode in details )
            {
              countDetailsRemoved += detailNode.ItemCount;
            }
            details.Clear(); //note: detail generators will be "closed" by another section of code (Remap floating details).

            m_log.WriteLine( this, "details.Remove - IN" + node.GetHashCode().ToString() + " - Di" + key.ToString() );

            node.Details.Remove( key );

            if( node.Details.Count == 0 )
            {
              node.Details = null;
            }
          }
          else
          {
            //Key not found in the dictionary, something wrong is going on.
            throw new DataGridInternalException( "Key not found in Details dictionary within the remove range.", m_dataGridControl );
          }
        }
        //If the key is above the remove range, re-key it appropriatly.
        else if( key > nodeEndIndex )
        {
          List<DetailGeneratorNode> details;
          if( node.Details.TryGetValue( key, out details ) )
          {
            m_log.WriteLine( this, "details.offset for remove - IN" + node.GetHashCode().ToString() + "- Di" + key.ToString() + " - new Di" + ( key - removeCount ).ToString() );

            node.Details.Remove( key );
            node.Details.Add( key - removeCount, details );
          }
          else
          {
            //Key not found in the dictionary, something wrong is going on.
            throw new DataGridInternalException( "Key not found in Details dictionary above the remove range.", m_dataGridControl );
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
            foreach( DetailGeneratorNode detailNode in details )
            {
              detailCount += detailNode.ItemCount;
            }
          }
        }
      }

      return detailCount;
    }

    private void HandleItemReset( GeneratorNode node )
    {
      //these 4 variables will hold the content for the Removal and the re-addition of the items.
      int countGeneratedRemoved;
      GeneratorPosition itemGenPos;

      List<DependencyObject> removedContainers = new List<DependencyObject>();

      //by definition, items from a node are contiguous...
      itemGenPos = this.FindFirstRealizedItemsForNode( node );
      int removeIndex = ( itemGenPos.Offset == 0 ) ? m_genPosToIndex[ itemGenPos.Index ] : -1;
      countGeneratedRemoved = this.RemoveGeneratedItems( node, removedContainers );

      // ensure that the mapping of the details to that node are removed!
      ItemsGeneratorNode itemsNode = node as ItemsGeneratorNode;
      if( ( itemsNode != null ) && ( itemsNode.Details != null ) )
      {
        int detailCount = 0;
        foreach( List<DetailGeneratorNode> detailList in itemsNode.Details.Values )
        {
          foreach( DetailGeneratorNode detailNode in detailList )
          {
            detailCount += detailNode.ItemCount;
          }
        }

        m_log.WriteLine( this, "details.Clear - IN" + itemsNode.GetHashCode().ToString() );

        itemsNode.Details.Clear();
        itemsNode.Details = null;

        node.AdjustItemCount( -detailCount );
      }

      //send the removal notification to the panel...
      this.IncrementCurrentGenerationCount();

      if( countGeneratedRemoved > 0 )
      {
        this.SendRemoveEvent( itemGenPos, removeIndex, 0, countGeneratedRemoved, removedContainers );
      }
    }

    private void HandleHeaderFooterRemove( HeadersFootersGeneratorNode node, NotifyCollectionChangedEventArgs e )
    {
      bool itemsRemoved = false;

      node.AdjustItemCount( -e.OldItems.Count );

      //if the node is totally expanded
      if( node.IsComputedExpanded )
      {
        GroupGeneratorNode parentGroup = node.Parent as GroupGeneratorNode;

        int removeGenPosIndex;
        int removeIndex;

        foreach( object item in e.OldItems )
        {
          object realItem = ( parentGroup != null )
                              ? new GroupHeaderFooterItem( parentGroup.CollectionViewGroup, item )
                              : item;


          removeGenPosIndex = m_genPosToItem.IndexOf( realItem );
          GeneratorPosition removeGenPos;
          // If the value is -1, it means the Header/Footer was not realized when the remove occured.
          if( removeGenPosIndex != -1 )
          {
            removeIndex = m_genPosToIndex[ removeGenPosIndex ];
            removeGenPos = new GeneratorPosition( removeGenPosIndex, 0 );
          }
          else
          {
            //Since there is no way to get the item's index from the list of generated items, then
            //compute it based on the node's index and the event args parameters.

            GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( node, 0, 0 );
            nodeHelper.ReverseCalculateIndex();

            removeIndex = nodeHelper.Index + e.OldStartingIndex;

            removeGenPos = this.GeneratorPositionFromIndex( removeIndex );
          }

          List<DependencyObject> removedContainers = new List<DependencyObject>();
          this.RemoveGeneratedItems( node, realItem, removedContainers );

          this.SendRemoveEvent( removeGenPos, removeIndex, 1, removedContainers.Count, removedContainers );
          itemsRemoved = true;
        }

        this.IncrementCurrentGenerationCount( itemsRemoved );
      }
    }

    private void HandleHeaderFooterReplace( HeadersFootersGeneratorNode node, NotifyCollectionChangedEventArgs e )
    {
      //add immediately the number of new items to the "parent" node since the function call right below will remove the number of old items.
      node.AdjustItemCount( e.NewItems.Count );

      this.HandleHeaderFooterRemove( node, e );
    }

    private GeneratorPosition FindFirstRealizedItemsForNode( GeneratorNode referenceNode )
    {
      GeneratorPosition retval = new GeneratorPosition( -1, 1 );
      int genPosCounter = -1;

      List<GeneratorNode> nodesAccepted = new List<GeneratorNode>();
      nodesAccepted.Add( referenceNode );

      //if there are details for the reference node
      ItemsGeneratorNode itemsNode = referenceNode as ItemsGeneratorNode;
      if( ( itemsNode != null ) && ( itemsNode.Details != null ) )
      {
        foreach( List<DetailGeneratorNode> details in itemsNode.Details.Values )
        {
          foreach( DetailGeneratorNode detailNode in details )
          {
            nodesAccepted.Add( detailNode );
          }
        }
      }

      for( int i = 0; i < m_genPosToNode.Count; i++ )
      {
        GeneratorNode node = m_genPosToNode[ i ];

        //For master/detail, I have to decouple the index to genPos relationship (because of detail rows, possibly messing up with generated items.
        genPosCounter++;

        //if the node currently observed is my reference node, get it's computed generator position
        if( nodesAccepted.Contains( node ) )
        {
          retval = new GeneratorPosition( genPosCounter, 0 );
          break;
        }
      }

      return retval;
    }

    private void HandleGlobalItemsReset()
    {
      m_log.Assert( this, !this.IsHandlingGlobalItemsResetLocally, "Generator is already processing a HandleGlobalItemReset or CleanupGenerator" );

      if( this.IsHandlingGlobalItemsResetLocally )
        return;

      if( m_startNode == null )
      {
        m_log.WriteLine( this, "No starting node in HandleGlobalItemsReset." );
        return;
      }

      using( m_log.BeginBlock( this, null ) )
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

    internal void EnsureNodeTreeCreated()
    {
      //This is done to ensure that any public interface function called get an "up to date" generator content when being accessed.

      // No reentrency is allowed for this method
      // to avoid any problem.
      if( this.IsEnsuringNodeTreeCreated )
        return;

      using( this.SetIsEnsuringNodeTreeCreated() )
      {
        if( m_startNode == null )
        {
          m_log.WriteLine( this, "EnsureNodeTreeCreated - new tree" );

          int addCount;

          this.UpdateHeaders( m_dataGridContext.Headers );
          this.SetupInitialItemsNodes( out addCount );
          this.UpdateFooters( m_dataGridContext.Footers );

          //This is to ensure that the RecyclingManager will consider this generator with regards to RefCount
          //(refcount is used to determine when the list of container from the RecyclingManager needs to be removed)
          m_dataGridContext.RecyclingManager.AddRef( this );

          this.IncrementCurrentGenerationCount();
        }
        else if( m_firstItem == null )
        {
          this.HandleItemsRecreation();
        }

        m_dataGridControl.RestoreDataGridContextState( m_dataGridContext );
      }
    }

    private void HandleItemsRecreation()
    {
      //Then, since its a Reset, recreates the Items nodes.
      int resetAddCount;
      GeneratorNode addNode = this.SetupInitialItemsNodes( out resetAddCount );

      if( addNode != null )
      {
        this.IncrementCurrentGenerationCount();
      }
    }

    private void UpdateGenPosToIndexList()
    {
      this.ValidateIndexOrder( "Before" );

      //after the modification to have the item count stored "locally" in the DetailGeneratorNodes,
      //it becomes important to have the nodes updated when the layout of the items changes.
      foreach( var detailNode in this.GetDetailGeneratorNodes() )
      {
        detailNode.UpdateItemCount();
      }

      if( m_startNode != null )
      {
        GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 ); //index is 0, since i'm beginning at the start 
        //(and I want to reindex the whole list)

        DetailGeneratorNode cachedDetailNode = null;
        int cachedDetailNodeIndex = -1;
        int previousDetailIndex = -1;

        //loop through the realized items. By design they are present sequentially in the linked list, so I do not need to reset the GeneratorNodeHelper
        for( int i = 0; i < m_genPosToItem.Count; i++ )
        {
          object item = m_genPosToItem[ i ];

          DetailGeneratorNode detailNode = m_genPosToNode[ i ] as DetailGeneratorNode;
          if( detailNode == null )
          {
            //find the Item 
            int tmpIndex = nodeHelper.FindItem( item );
            if( tmpIndex != -1 )
            {
              //set the Index (new) for the item
              ItemsGeneratorNode itemsNode = nodeHelper.CurrentNode as ItemsGeneratorNode;
              if( itemsNode != null )
              {
                tmpIndex = nodeHelper.Index + itemsNode.IndexOf( item );
              }

              m_genPosToIndex[ i ] = tmpIndex;
            }
            else
            {
              m_log.WriteLine( this, CustomItemContainerGenerator.FormatObjectId( "# UpdateGenPosToIndexList - item not found D", item ) );

              //a possible fix for this is to set the index of the "not found" element to the same index as previous item in the generated list...
              m_genPosToIndex[ i ] = ( i > 0 ) ? m_genPosToIndex[ i - 1 ] : 0;

              //item is not in the linked list... there is a problem, throw.
              //throw new DataGridInternalException();

            }
          }
          else //else is detailNode != null
          {
            if( cachedDetailNode != detailNode )
            {
              cachedDetailNodeIndex = this.FindGlobalIndexForDetailNode( detailNode );
              cachedDetailNode = detailNode;
              previousDetailIndex = -1;
            }
            int detailIndex = detailNode.DetailGenerator.IndexFromRealizedItem( item, previousDetailIndex + 1, out previousDetailIndex );
            m_genPosToIndex[ i ] = cachedDetailNodeIndex + detailIndex;
          }
        } //end for()
      }

      this.ValidateIndexOrder( "After" );
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
          throw new DataGridInternalException( "Master item index is out of bound.", m_dataGridControl );

        return offset + detailIndex + parentItemIndex + 1;
      }

      return -1;
    }

    private void StartGenerator( GeneratorPosition startPos, GeneratorDirection direction )
    {
      if( this.Status == GeneratorStatus.GeneratingContainers )
        DataGridException.ThrowSystemException( "Cannot perform this operation while the generator is busy generating items.",
                                                typeof( InvalidOperationException ), m_dataGridControl.Name );

      //set the GeneratorStatus to "Generating"
      m_generatorStatus = GeneratorStatus.GeneratingContainers;

      //Initialize the Direction
      m_generatorDirection = direction;

      //retrieve the Index for the GeneratorPosition retrieved
      m_generatorCurrentGlobalIndex = this.IndexFromGeneratorPosition( startPos );

      int itemCount = this.ItemCount;
      if( ( m_generatorCurrentGlobalIndex < 0 ) || ( ( itemCount > 0 ) && ( m_generatorCurrentGlobalIndex >= itemCount ) ) )
        DataGridException.ThrowSystemException( "The specified start position is outside the range of the Generator content.",
                                                typeof( ArgumentOutOfRangeException ), m_dataGridControl.Name, "startPos" );

      if( itemCount == 0 )
        return;

      //and create a node helper that will assist us during the Generation process...
      m_generatorNodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 ); // start index is always 0

      //position the GeneratorNodeHelper to the appropriate node
      if( !m_generatorNodeHelper.FindNodeForIndex( m_generatorCurrentGlobalIndex ) ) //find index?!?!
        //there was a problem moving the Node helper... 
        throw new DataGridInternalException( "Unable to move to the required generator node by using the current index.", m_dataGridControl );

      //Calculate the offset
      m_generatorCurrentOffset = m_generatorCurrentGlobalIndex - m_generatorNodeHelper.Index;

      ItemsGeneratorNode itemsNode = m_generatorNodeHelper.CurrentNode as ItemsGeneratorNode;
      if( itemsNode != null )
      {
        m_generatorCurrentDetail = itemsNode.GetDetailNodeForIndex( m_generatorCurrentOffset, out m_generatorCurrentOffset, out m_generatorCurrentDetailIndex, out m_generatorCurrentDetailNodeIndex );
      }

      if( m_generatorCurrentDetail != null )
      {
        // No detail generator should be started already.
        if( m_generatorCurrentDetailDisposable != null )
          throw new DataGridInternalException( "The detail generator is already started.", m_dataGridControl );

        m_generatorCurrentDetailDisposable = ( ( IItemContainerGenerator )m_generatorCurrentDetail.DetailGenerator ).StartAt( m_generatorCurrentDetail.DetailGenerator.GeneratorPositionFromIndex( m_generatorCurrentDetailIndex ), direction, true );
      }
    }

    private void StopGenerator()
    {
      if( this.Status != GeneratorStatus.GeneratingContainers )
        throw new DataGridInternalException( "Cannot perform this operation while the generator is busy generating items.", m_dataGridControl );

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
      GeneratorPosition retval = position;

      int index = this.IndexFromGeneratorPosition( position ) + 1;

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
      int index = this.IndexFromGeneratorPosition( position ) - 1;

      while( index >= 0 )
      {
        if( !m_genPosToIndex.Contains( index ) )
        {
          break;
        }

        index--;
      }

      GeneratorPosition retval = this.GeneratorPositionFromIndex( index );

      return retval;
    }

    private void RemoveContainer( DependencyObject container, object dataItem )
    {
      m_dataGridControl.ClearItemContainer( container, dataItem );

      if( this.IsRecyclingEnabled )
      {
        this.EnqueueContainer( container, dataItem );

        var dataItemStore = container.ReadLocalValue( CustomItemContainerGenerator.DataItemPropertyProperty ) as DataItemDataProviderBase;
        if( dataItemStore != null )
        {
          dataItemStore.ClearDataItem();
        }

        if( GroupLevelIndicatorPane.GetGroupLevel( container ) == -1 )
        {
          container.ClearValue( GroupLevelIndicatorPane.GroupLevelProperty );
        }

        if( container is HeaderFooterItem )
        {
          container.ClearValue( DataGridControl.StatContextPropertyKey );
        }
      }
    }

    private DependencyObject CreateContainerForItem( object dataItem, GeneratorNode node )
    {
      DependencyObject container = null;

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
        throw new DataGridInternalException( "Cannot create container for the GeneratorNode, as it is not of a valid type.", m_dataGridControl );
      }

      if( container == null )
        throw new DataGridInternalException( "A container could not be created or recycled for the GeneratorNode.", m_dataGridControl );

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
      GroupGeneratorNode parentGroup = node.Parent as GroupGeneratorNode;
      DataGridCollectionViewGroup collectionViewGroup = null;

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

    private bool IsItemItsOwnContainer( object dataItem )
    {
      return m_dataGridControl.IsItemItsOwnContainer( dataItem );
    }

    private DependencyObject CreateHeaderFooterContainer( object dataItem )
    {
      if( this.IsRecyclingEnabled )
      {
        DependencyObject recycledContainer = null;
        recycledContainer = this.DequeueHeaderFooterContainer( dataItem );
        if( recycledContainer != null )
          return recycledContainer;
      }

      //If the container cannot be recycled, then create a new one.
      object realDataItem = dataItem;
      if( dataItem.GetType() == typeof( GroupHeaderFooterItem ) )
      {
        realDataItem = ( ( GroupHeaderFooterItem )dataItem ).Template;
      }

      DataTemplate template = realDataItem as DataTemplate;
      if( template == null )
      {
        GroupHeaderFooterItemTemplate vwc = realDataItem as GroupHeaderFooterItemTemplate;
        if( vwc != null )
        {
          vwc.Seal();
          template = vwc.Template;
        }

        if( template == null )
          throw new DataGridInternalException( "No template found for the creation of a header or footer container.", m_dataGridControl );
      }

      HeaderFooterItem newItem = new HeaderFooterItem();

      BindingOperations.SetBinding( newItem, HeaderFooterItem.ContentProperty, m_headerFooterDataContextBinding );
      newItem.ContentTemplate = template;

      return newItem;
    }

    private int FindInsertionPoint( int itemIndex )
    {
      int i = 0;

      int collectionCount = m_genPosToIndex.Count;
      for( i = 0; i < collectionCount; i++ )
      {
        //if the item is larger in index, then I want to insert before it!
        if( m_genPosToIndex[ i ] > itemIndex )
        {
          break;
        }

      }

      return i;
    }

    private void QueueDetailItemForRemapping( object item )
    {
      if( item == null )
        return;

      if( ( !m_floatingDetails.Contains( item ) ) && ( m_masterToDetails.ContainsKey( item ) ) )
      {
        m_floatingDetails.Add( item );
      }
    }

    private DetailGeneratorNode CreateDetailGeneratorNode( object dataItem, DataGridCollectionViewBase collectionView, DetailConfiguration detailConfiguration )
    {
      var detailDataGridContext = new DataGridContext( m_dataGridContext, m_dataGridControl, dataItem, collectionView, detailConfiguration );
      detailDataGridContext.SetAssociatedAutomationPeer();

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
      using( m_log.BeginBlock( this, CustomItemContainerGenerator.FormatObjectId( "ClearDetailGeneratorNode - DN", detailNode ) ) )
      {
        m_dataGridControl.SelectionChangerManager.Begin();

        try
        {
          m_dataGridControl.SelectionChangerManager.UnselectAllItems( detailNode.DetailContext );
          m_dataGridControl.SelectionChangerManager.UnselectAllCells( detailNode.DetailContext );
        }
        finally
        {
          m_dataGridControl.SelectionChangerManager.End( false, false, false );
        }

        m_dataGridControl.SaveDataGridContextState( detailNode.DetailContext, true, int.MaxValue );

        CustomItemContainerGenerator detailGenerator = detailNode.DetailGenerator;

        detailNode.CleanGeneratorNode();

        DetailsChangedEventManager.RemoveListener( detailGenerator, this );
        detailGenerator.ItemsChanged -= new CustomGeneratorChangedEventHandler( this.HandleDetailGeneratorContentChanged );
        detailGenerator.ContainersRemoved -= new ContainersRemovedEventHandler( this.OnDetailContainersRemoved );
        detailGenerator.SetGenPosToIndexUpdateInhibiter( null );
        detailGenerator.UnregisterEvents();
        detailGenerator.ClearEvents();
      }
    }

    private void RegisterEvents()
    {
      Debug.Assert( m_dataGridControl != null );
      Debug.Assert( m_collectionView != null );
      Debug.Assert( ( m_dataGridContext != null ) && ( m_dataGridContext.DetailConfigurations != null ) );

      CollectionChangedEventManager.AddListener( m_collectionView, this );
      GroupConfigurationSelectorChangedEventManager.AddListener( m_dataGridContext, this );
      CollectionChangedEventManager.AddListener( m_dataGridContext.DetailConfigurations, this );
      PropertyChangedEventManager.AddListener( m_collectionView, this, "Groups" );

      // The top most generator must register to additional events.
      if( m_dataGridContext.SourceDetailConfiguration == null )
      {
        ItemsSourceChangeCompletedEventManager.AddListener( m_dataGridControl, this );
        ViewChangedEventManager.AddListener( m_dataGridControl, this );
        ThemeChangedEventManager.AddListener( m_dataGridControl, this );
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
      PropertyChangedEventManager.RemoveListener( m_collectionView, this, "Groups" );

      // The top most generator must unregister from additional events.
      if( m_dataGridContext.SourceDetailConfiguration == null )
      {
        ItemsSourceChangeCompletedEventManager.RemoveListener( m_dataGridControl, this );
        ViewChangedEventManager.RemoveListener( m_dataGridControl, this );
        ThemeChangedEventManager.RemoveListener( m_dataGridControl, this );
      }
    }

    private void ClearEvents()
    {
      this.ContainersRemoved = null;
      this.DetailsChanged = null;
      this.ItemsChanged = null;
      this.PropertyChanged = null;
    }

    private void HandleDetailGeneratorContentChanged( object sender, CustomGeneratorChangedEventArgs e )
    {
      m_log.Assert( this, !this.IsHandlingGlobalItemsResetLocally, "Generator is already processing a HandleGlobalItemReset or CleanupGenerator" );

      if( this.IsHandlingGlobalItemsResetLocally )
        return;

      m_log.Assert( this, m_startNode != null, "m_startNode != null" );

      CustomItemContainerGenerator detailGenerator = sender as CustomItemContainerGenerator;

      m_log.Assert( this, detailGenerator != null, "detailGenerator != null" );

      object masterItem;
      DetailGeneratorNode detailNode = this.FindDetailGeneratorNodeForGenerator( detailGenerator, out masterItem );

      m_log.Assert( this, masterItem != null, "masterItem != null" );
      m_log.Assert( this, detailNode != null, "detailNode != null" );

      if( ( detailNode != null ) && ( masterItem != null ) )
      {
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
            throw new DataGridInternalException( "CustomItemContainerGenerator never notifies a Replace action.", m_dataGridControl );

          case NotifyCollectionChangedAction.Reset:
            this.HandleDetailReset( masterItem, detailNode );
            break;

          default:
            break;
        }
      }
    }

    private void HandleDetailAddition( object masterItem, DetailGeneratorNode detailNode, CustomGeneratorChangedEventArgs e )
    {
      if( m_floatingDetails.Contains( masterItem ) )
        return;

      var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
      var masterIndex = nodeHelper.FindItem( masterItem );

      // The master item has been found.
      if( masterIndex != -1 )
      {
        int globalIndex = -1;
        var convertedGeneratorPosition = this.ConvertDetailGeneratorPosition( e.Position, masterItem, detailNode, out globalIndex );
        var masterNode = ( ItemsGeneratorNode )nodeHelper.CurrentNode;

        masterNode.AdjustItemCount( e.ItemCount );
        detailNode.UpdateItemCount();

        this.IncrementCurrentGenerationCount();
        this.SendAddEvent( convertedGeneratorPosition, masterIndex + 1 + e.Index, e.ItemCount );
      }
      // The master item could be located inside a collapsed group.
      else
      {
        //in that case, I need to determine the appropriate masterNode another way
        nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
        if( !nodeHelper.Contains( masterItem ) )
          DataGridException.ThrowSystemException( "An attempt was made to add a detail for an item that does not belong to the generator.",
                                                  typeof( InvalidOperationException ), m_dataGridControl.Name );

        var masterNode = ( ItemsGeneratorNode )nodeHelper.CurrentNode;

        masterNode.AdjustItemCount( e.ItemCount );
        detailNode.UpdateItemCount();

        this.IncrementCurrentGenerationCount();
      }
    }

    private void HandleDetailMoveRemove( object masterItem, DetailGeneratorNode detailNode, CustomGeneratorChangedEventArgs e )
    {
      if( m_floatingDetails.Contains( masterItem ) )
        return;

      var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
      var masterIndex = nodeHelper.FindItem( masterItem );

      // The master item has been found.
      if( masterIndex != -1 )
      {
        int globalIndex = -1;
        var convertedGeneratorPosition = this.ConvertDetailGeneratorPosition( e.OldPosition, masterItem, detailNode, out globalIndex );

        this.RemoveDetailContainers( convertedGeneratorPosition, e.ItemUICount );

        if( e.Action == NotifyCollectionChangedAction.Remove )
        {
          var masterNode = ( ItemsGeneratorNode )nodeHelper.CurrentNode;

          masterNode.AdjustItemCount( -e.ItemCount );
          detailNode.UpdateItemCount();
        }

        this.IncrementCurrentGenerationCount();
        this.SendRemoveEvent( convertedGeneratorPosition, globalIndex, e.ItemCount, e.ItemUICount, e.RemovedContainers );
      }
      // The master item could be located inside a collapsed group.
      else
      {
        //in that case, I need to determine the appropriate masterNode another way
        nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
        if( !nodeHelper.Contains( masterItem ) )
          DataGridException.ThrowSystemException( "An attempt was made to move or remove a detail for an item that does not belong to the generator.",
                                                  typeof( InvalidOperationException ), m_dataGridControl.Name );

        if( e.Action == NotifyCollectionChangedAction.Remove )
        {
          var masterNode = ( ItemsGeneratorNode )nodeHelper.CurrentNode;

          masterNode.AdjustItemCount( -e.ItemCount );
          detailNode.UpdateItemCount();
        }

        this.IncrementCurrentGenerationCount();
      }
    }

    private void HandleDetailReset( object masterItem, DetailGeneratorNode detailNode )
    {
      if( m_floatingDetails.Contains( masterItem ) )
        return;

      var nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
      var masterIndex = nodeHelper.FindItem( masterItem );
      var masterItemFound = ( masterIndex != -1 );

      // The master item could be located inside a collapsed group.
      if( !masterItemFound )
      {
        nodeHelper = new GeneratorNodeHelper( m_startNode, 0, 0 );
        if( !nodeHelper.Contains( masterItem ) )
          DataGridException.ThrowSystemException( "An attempt was made to reset a detail for an item that does not belong to the generator.",
                                                  typeof( InvalidOperationException ), m_dataGridControl.Name );
      }

      var masterNode = ( ItemsGeneratorNode )nodeHelper.CurrentNode;

      List<DetailGeneratorNode> detailsForMaster;
      if( ( masterNode.Details == null ) || !masterNode.Details.TryGetValue( masterNode.Items.IndexOf( masterItem ), out detailsForMaster ) )
        throw new DataGridInternalException( "An attempt was made to reset a detail that does not belong to the generator.", m_dataGridControl );

      //start index will be ignored later on if the masterIndex is -1!!
      int startIndex = nodeHelper.Index + masterNode.IndexOf( masterItem ) + 1; //details start a master index + 1

      //this is required to ensure that if the details that resets is not the first one, the index is calculated appropriatly.
      foreach( DetailGeneratorNode node in detailsForMaster )
      {
        if( node == detailNode )
          break;

        startIndex += node.ItemCount;
      }

      var removePosition = new GeneratorPosition( -1, 1 );
      int removedCount = 0;
      var removedContainers = new List<DependencyObject>();
      var oldDetailCount = detailNode.ItemCount;
      var hasOldDetailItems = ( oldDetailCount > 0 );

      if( hasOldDetailItems )
      {
        // The information about the removal is only required when the master item is not collapsed.
        if( masterItemFound )
        {
          removePosition = this.GetSafeGeneratorPositionFromIndex( startIndex );
          removedCount = this.RemoveGeneratedItems( startIndex, startIndex + oldDetailCount - 1, removedContainers );
        }

        masterNode.AdjustItemCount( -oldDetailCount );
      }

      detailNode.UpdateItemCount();

      GeneratorPosition addPosition = new GeneratorPosition( -1, 1 );
      int newDetailCount = detailNode.ItemCount;
      bool hasNewDetailItems = ( newDetailCount > 0 );

      if( hasNewDetailItems )
      {
        // The information about the addition is only required when the master item is not collapsed.
        if( masterItemFound )
        {
          addPosition = this.GetSafeGeneratorPositionFromIndex( startIndex );
        }

        masterNode.AdjustItemCount( newDetailCount );
      }

      this.IncrementCurrentGenerationCount();

      // The events must be raised only when the master item is not collapsed.
      if( masterItemFound )
      {
        if( hasOldDetailItems && hasNewDetailItems )
        {
          this.SendResetEvent();
        }
        else if( hasOldDetailItems )
        {
          this.SendRemoveEvent( removePosition, masterIndex + 1, oldDetailCount, removedCount, removedContainers );
        }
        else if( hasNewDetailItems )
        {
          this.SendAddEvent( addPosition, masterIndex + 1, newDetailCount );
        }
      }
    }

    private void RemoveDetailContainers( GeneratorPosition convertedGeneratorPosition, int removeCount )
    {
      int removeGenPosIndex = convertedGeneratorPosition.Index;
      if( convertedGeneratorPosition.Offset > 0 )
      {
        Debug.Assert( convertedGeneratorPosition.Offset == 1 );
        removeGenPosIndex++;
      }

      for( int i = 0; i < removeCount; i++ )
      {
        this.GenPosArraysRemoveAt( removeGenPosIndex );
      }
    }

    private GeneratorPosition ConvertDetailGeneratorPosition( GeneratorPosition referencePosition, object masterItem, DetailGeneratorNode detailNode, out int globalIndex )
    {
      //If the requested generator position map past at least one generated item from the detail generator, then the job is easy...
      if( referencePosition.Index >= 0 )
      {
        int generatorIndex = this.FindGeneratorIndexForNode( detailNode, referencePosition.Index );

        // Ensure to return the globalIndex as -1
        // if the generator index is not found for
        // a DetailNode. This can occur if a Detail
        // is filtered out via AutoFiltering.
        globalIndex = ( generatorIndex > -1 ) ? m_genPosToIndex[ generatorIndex ] : -1;

        return new GeneratorPosition( generatorIndex, referencePosition.Offset );
      }
      else
      {
        //This means the GeneratorPosition returned by the DetailGenerator is "before" any generated item from the detail generator.
        //I need more complex detection of the GeneratorPosition.

        // First - Get the Index of the MasterItem
        int masterIndex = this.IndexFromItem( masterItem );

        //Second - Get the DetailGenerator's Index for the DetailGenerator's GenPos
        int detailGeneratorIndex = detailNode.DetailGenerator.IndexFromGeneratorPosition( referencePosition );

        globalIndex = masterIndex + detailGeneratorIndex + 1;

        // Finally - Have the Master Generator compute the GeneratorPosition from the sum of both
        return this.GeneratorPositionFromIndex( globalIndex );
      }
    }

    private int FindGeneratorIndexForNode( DetailGeneratorNode referenceNode, int offset )
    {
      int offsetCounter = -1;

      for( int i = 0; i < m_genPosToNode.Count; i++ )
      {
        GeneratorNode node = m_genPosToNode[ i ];
        if( node == referenceNode )
        {
          offsetCounter++;
          if( offsetCounter == offset )
            return i;
        }
      }

      return -1;
    }

    private DetailGeneratorNode FindDetailGeneratorNodeForGenerator( CustomItemContainerGenerator detailGenerator, out object masterItem )
    {
      foreach( KeyValuePair<object, List<DetailGeneratorNode>> detailsForItem in m_masterToDetails )
      {
        foreach( DetailGeneratorNode detailNode in detailsForItem.Value )
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
      {
        return m_genPosToIndexInhibiter.InhibitGenPosToIndexUpdates();
      }
      return null;
    }

    private int FindFirstGeneratedIndexForLocalItem( object item )
    {
      //this function will find an item in the m_genPosToItem, but will filter out those that belongs to details.
      //This is to avoid the problem caused by Detail items that belongs also to a master.
      int retval = -1;

      int runningIndex = 0;
      int itemCount = m_genPosToItem.Count;

      while( runningIndex < itemCount )
      {
        retval = m_genPosToItem.IndexOf( item, runningIndex );

        //No item found past the current runningIndex
        if( retval == -1 )
          break;

        //check if the item belongs to a Detail or not.
        DetailGeneratorNode detailNode = m_genPosToNode[ retval ] as DetailGeneratorNode;
        if( detailNode == null )
          break; //the item is not a detail and therefore qualifies as a return value.

        //Item is from a detail
        runningIndex = retval + 1;
      }

      return retval;
    }

    private void EnqueueContainer( DependencyObject container, object item )
    {
      RecyclingManager manager = m_dataGridContext.RecyclingManager;

      m_log.Assert( this, manager != null, "manager != null" );

      if( container is HeaderFooterItem )
      {
        if( item is GroupHeaderFooterItem )
        {
          //If the group is not in the CollectionView anymore (e.g. all its rows have been deleted), do not recycle it.
          CollectionViewGroup viewGroup = ( ( GroupHeaderFooterItem )item ).Group;
          if( viewGroup == null )
            return;

          //The tree is already correctly created since we are recycling containers!
          Group group;

          using( this.SetIsEnsuringNodeTreeCreated() )
          {
            group = this.GetGroupFromCollectionViewGroup( viewGroup );
          }

          if( group == null )
            return;

          string groupBy = group.GroupBy;
          if( string.IsNullOrEmpty( groupBy ) )
            return;

          manager.EnqueueGroupHeaderFooterContainer( groupBy, item, container );
        }
        else
        {
          manager.EnqueueHeaderFooterContainer( item, container );
        }
      }
      else
      {
        manager.EnqueueItemContainer( item, container );
      }
    }

    private DependencyObject DequeueItemContainer( object item )
    {
      RecyclingManager manager = m_dataGridContext.RecyclingManager;

      m_log.Assert( this, manager != null, "manager != null" );

      return manager.DequeueItemContainer( item );
    }

    private DependencyObject DequeueHeaderFooterContainer( object item )
    {
      RecyclingManager manager = m_dataGridContext.RecyclingManager;

      m_log.Assert( this, manager != null, "manager != null" );

      if( item is GroupHeaderFooterItem )
      {
        CollectionViewGroup viewGroup = ( ( GroupHeaderFooterItem )item ).Group;
        if( viewGroup == null )
          return null;

        //The tree is already correctly created since we are recycling containers!
        Group group;

        using( this.SetIsEnsuringNodeTreeCreated() )
        {
          group = this.GetGroupFromCollectionViewGroup( viewGroup );
        }

        if( group == null )
          return null;

        string groupBy = group.GroupBy;
        if( string.IsNullOrEmpty( groupBy ) )
          return null;

        return manager.DequeueGroupHeaderFooterContainer( groupBy, item );
      }
      else
      {
        return manager.DequeueHeaderFooterContainer( item );
      }
    }

    private void OnDetailContainersRemoved( object sender, ContainersRemovedEventArgs e )
    {
      if( m_containersRemovedDeferCount > 0 )
      {
        m_deferredContainersRemoved.AddRange( e.RemovedContainers );
      }
      else
      {
        this.NotifyContainersRemoved( e );
      }
    }

    private IDisposable DeferContainersRemovedNotification()
    {
      return new DeferContainersRemovedDisposable( this );
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

    // Data Members

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

    private List<DependencyObject> m_deferredContainersRemoved = new List<DependencyObject>();
    private int m_containersRemovedDeferCount = 0;

    private Binding m_headerFooterDataContextBinding;

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        var nccArgs = ( NotifyCollectionChangedEventArgs )e;

        if( sender == m_collectionView )
        {
          this.OnItemsChanged( sender, nccArgs.GetRangeActionOrSelf() );
          return true;
        }
        else if( sender == m_groupsCollection )
        {
          this.OnGroupsChanged( sender, nccArgs );
          return true;
        }
        else if( sender == m_dataGridContext.DetailConfigurations )
        {
          this.OnDetailConfigurationsChanged( sender, nccArgs );
          return true;
        }
      }
      else if( managerType == typeof( PropertyChangedEventManager ) )
      {
        var pcArgs = ( PropertyChangedEventArgs )e;

        if( sender == m_collectionView )
        {
          this.OnCollectionViewGroupsPropertyChanged();
          return true;
        }
        else
        {
          //this is only registered on the DataGridContext, this has the effect of forwarding property changes for all properties of the DataGridContext
          this.OnNotifyPropertyChanged( pcArgs );
          return true;
        }
      }
      else if( ( managerType == typeof( ViewChangedEventManager ) ) || ( managerType == typeof( ThemeChangedEventManager ) ) )
      {
        this.OnViewThemeChanged( sender, e );
        return true;
      }
      else if( managerType == typeof( DetailsChangedEventManager ) )
      {
        if( this.DetailsChanged != null )
        {
          this.DetailsChanged( sender, e );
        }

        return true;
      }
      else if( managerType == typeof( ItemsSourceChangeCompletedEventManager ) )
      {
        this.OnItemsSourceChanged( sender, e );
        return true;
      }
      else if( managerType == typeof( GroupConfigurationSelectorChangedEventManager ) )
      {
        this.OnGroupConfigurationSelectorChanged();
        return true;
      }

      return false;
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnNotifyPropertyChanged( PropertyChangedEventArgs e )
    {
      if( this.PropertyChanged != null )
      {
        this.PropertyChanged( this, e );
      }
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
          return m_flag.IsSet;
        }
      }

      public override IDisposable Set()
      {
        return m_flag.Set();
      }

      internal LeveledAutoResetFlag GetChild()
      {
        int index = m_index + 1;
        if( index < m_levels.Count )
        {
          var flag = ( LeveledAutoResetFlag )m_levels[ index ].Target;
          if( flag != null )
            return flag;
        }

        return new LeveledAutoResetFlag( m_levels, index );
      }

      private readonly IList<WeakReference> m_levels;
      private readonly AutoResetFlag m_flag;
      private readonly int m_index;
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
        var generator = m_generator;

        // Prevent this method from being called more than once.
        if( Interlocked.CompareExchange<CustomItemContainerGenerator>( ref m_generator, null, generator ) == null )
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
      public GenPosToIndexInhibitionDisposable( CustomItemContainerGenerator generator )
      {
        if( generator == null )
          throw new ArgumentNullException( "generator" );

        m_generator = generator;

        m_generator.m_genPosToIndexUpdateInhibitCount++;

        if( m_generator.m_genPosToIndexInhibiter != null )
        {
          m_nestedDisposable = m_generator.m_genPosToIndexInhibiter.InhibitGenPosToIndexUpdates();
        }
      }

      #region IDisposable Members

      public void Dispose()
      {
        m_generator.m_genPosToIndexUpdateInhibitCount--;

        if( ( m_generator.m_genPosToIndexUpdateInhibitCount == 0 ) && ( m_generator.GenPosToIndexNeedsUpdate ) )
        {
          m_generator.IncrementCurrentGenerationCount();
        }

        if( m_nestedDisposable != null )
        {
          m_nestedDisposable.Dispose();
          m_nestedDisposable = null;
        }
      }

      #endregion

      private CustomItemContainerGenerator m_generator; // = null
      private IDisposable m_nestedDisposable; // = null
    }

    #endregion

    #region DeferContainersRemovedDisposable Private Class

    private sealed class DeferContainersRemovedDisposable : IDisposable
    {
      public DeferContainersRemovedDisposable( CustomItemContainerGenerator generator )
      {
        if( generator == null )
          throw new ArgumentNullException( "generator" );

        m_generator = generator;

        m_generator.m_log.Assert( this, ( m_generator.m_containersRemovedDeferCount != 0 ) || ( m_generator.m_deferredContainersRemoved.Count == 0 ), "( m_generator.m_containersRemovedDeferCount != 0 ) || ( m_generator.m_deferredContainersRemoved.Count == 0 )" );

        m_generator.m_containersRemovedDeferCount++;
      }

      #region IDisposable Members

      public void Dispose()
      {
        m_generator.m_containersRemovedDeferCount--;

        if( ( m_generator.m_containersRemovedDeferCount == 0 ) && ( m_generator.m_deferredContainersRemoved.Count > 0 ) )
        {
          m_generator.NotifyContainersRemoved( new ContainersRemovedEventArgs( m_generator.m_deferredContainersRemoved ) );
          m_generator.m_deferredContainersRemoved.Clear();
        }
      }

      #endregion

      private CustomItemContainerGenerator m_generator; // = null
    }

    #endregion

    #region CustomItemContainerGeneratorFlags Private Enum

    [Flags]
    private enum CustomItemContainerGeneratorFlags
    {
      RecyclingEnabled = 1,
      InUse = 2,
      GenPosToIndexNeedsUpdate = 4,
    }

    #endregion
  }
}
