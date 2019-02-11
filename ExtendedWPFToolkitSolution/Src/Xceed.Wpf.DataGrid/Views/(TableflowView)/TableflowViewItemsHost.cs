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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Xceed.Wpf.DataGrid.Views
{
  public class TableflowViewItemsHost : DataGridItemsHost, IAnimatedScrollInfo, IDeferableScrollInfoRefresh
  {
    static TableflowViewItemsHost()
    {
      KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(
        typeof( TableflowViewItemsHost ),
        new FrameworkPropertyMetadata( KeyboardNavigationMode.Local ) );

      TableflowView.AreHeadersStickyProperty.OverrideMetadata(
        typeof( DataGridContext ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( TableflowViewItemsHost.OnViewPropertiesChanged ) ) );

      TableflowView.AreFootersStickyProperty.OverrideMetadata(
        typeof( DataGridContext ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( TableflowViewItemsHost.OnViewPropertiesChanged ) ) );

      TableflowView.AreGroupHeadersStickyProperty.OverrideMetadata(
        typeof( DataGridContext ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( TableflowViewItemsHost.OnViewPropertiesChanged ) ) );

      TableflowView.AreGroupFootersStickyProperty.OverrideMetadata(
        typeof( DataGridContext ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( TableflowViewItemsHost.OnViewPropertiesChanged ) ) );

      TableflowView.AreParentRowsStickyProperty.OverrideMetadata(
        typeof( DataGridContext ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( TableflowViewItemsHost.OnViewPropertiesChanged ) ) );

      TableflowView.ContainerHeightProperty.OverrideMetadata(
        typeof( DataGridContext ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( TableflowViewItemsHost.OnViewPropertiesChanged ) ) );

      TableflowView.AreGroupsFlattenedProperty.OverrideMetadata(
        typeof( DataGridContext ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( TableflowViewItemsHost.OnAreGroupsFlattenedChanged ) ) );

      TableflowViewItemsHost.IsStickyProperty = TableflowViewItemsHost.IsStickyPropertyKey.DependencyProperty;

      CommandManager.RegisterClassCommandBinding( typeof( TableflowViewItemsHost ),
        new CommandBinding( ComponentCommands.MoveFocusForward,
          TableflowViewItemsHost.MoveFocusForwardExecuted, TableflowViewItemsHost.MoveFocusForwardCanExecute ) );

      CommandManager.RegisterClassCommandBinding( typeof( TableflowViewItemsHost ),
        new CommandBinding( ComponentCommands.MoveFocusBack,
          TableflowViewItemsHost.MoveFocusBackExecuted, TableflowViewItemsHost.MoveFocusBackCanExecute ) );

      CommandManager.RegisterClassCommandBinding( typeof( TableflowViewItemsHost ),
        new CommandBinding( ComponentCommands.MoveFocusUp,
          TableflowViewItemsHost.MoveFocusUpExecuted, TableflowViewItemsHost.MoveFocusUpCanExecute ) );

      CommandManager.RegisterClassCommandBinding( typeof( TableflowViewItemsHost ),
        new CommandBinding( ComponentCommands.MoveFocusDown,
          TableflowViewItemsHost.MoveFocusDownExecuted, TableflowViewItemsHost.MoveFocusDownCanExecute ) );
    }

    public TableflowViewItemsHost()
    {
      this.AddHandler( FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler( this.OnRequestBringIntoView ) );

      this.CommandBindings.Add( new CommandBinding(
        GroupNavigationButton.NavigateToGroup, this.OnNavigateToGroupExecuted, this.OnNavigateToGroupCanExecute ) );

      this.InitializeHorizontalOffsetAnimation();
      this.InitializeVerticalOffsetAnimation();

      this.Unloaded += new RoutedEventHandler( this.TableflowViewItemsHost_Unloaded );
    }

    private void TableflowViewItemsHost_Unloaded( object sender, RoutedEventArgs e )
    {
      this.StopHorizontalOffsetAnimation();
      this.StopVerticalOffsetAnimation();
    }

    private static void OnAreGroupsFlattenedChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridContext dataGridContext = sender as DataGridContext;

      // This property is not intended to be update live in an application.
      // We need to force a Reset on the CustomItemContainerGenerator to clear
      // all containers in order to get the right indentation.
      // Only reset when the change occurs on a the master DataGridContext
      // since the property is a ViewOnly property.
      if( ( dataGridContext != null ) && ( dataGridContext.ParentDataGridContext == null ) )
      {
        CustomItemContainerGenerator customItemContainerGenerator = dataGridContext.CustomItemContainerGenerator;

        if( customItemContainerGenerator != null )
        {
          dataGridContext.CustomItemContainerGenerator.ResetGeneratorContent();
        }
      }
    }

    private static void OnViewPropertiesChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridContext dataGridContext = sender as DataGridContext;

      if( ( dataGridContext == null )
        || ( dataGridContext.DataGridControl == null )
        || ( dataGridContext.DataGridControl.ItemsHost == null ) )
      {
        return;
      }

      // Nothing to do for detail DataGridContext.
      if( dataGridContext.ParentDataGridContext == null )
      {
        dataGridContext.DataGridControl.ItemsHost.InvalidateMeasure();
      }
    }

#if DEBUG
    #region RealizedIndex Property

    public static readonly DependencyProperty RealizedIndexProperty = DependencyProperty.RegisterAttached(
      "RealizedIndex",
      typeof( int ),
      typeof( TableflowViewItemsHost ) );

    public int RealizedIndex
    {
      get
      {
        return TableflowViewItemsHost.GetRealizedIndex( this );
      }
      set
      {
        TableflowViewItemsHost.SetRealizedIndex( this, value );
      }
    }

    public static int GetRealizedIndex( DependencyObject obj )
    {
      return ( int )obj.GetValue( TableflowViewItemsHost.RealizedIndexProperty );
    }

    public static void SetRealizedIndex( DependencyObject obj, int value )
    {
      obj.SetValue( TableflowViewItemsHost.RealizedIndexProperty, value );
    }

    #endregion
#endif //DEBUG

    #region ShouldDelayDataContext Property (Internal Attached)

    internal static readonly DependencyProperty ShouldDelayDataContextProperty = DependencyProperty.RegisterAttached(
      "ShouldDelayDataContext",
      typeof( bool ),
      typeof( TableflowViewItemsHost ) );

    internal static bool GetShouldDelayDataContext( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableflowViewItemsHost.ShouldDelayDataContextProperty );
    }

    internal static void SetShouldDelayDataContext( DependencyObject obj, bool value )
    {
      obj.SetValue( TableflowViewItemsHost.ShouldDelayDataContextProperty, value );
    }

    #endregion

    #region NavigateToGroup Command

    private void OnNavigateToGroupCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = true;
      e.Handled = true;
    }

    private void OnNavigateToGroupExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      Group group = e.Parameter as Group;

      if( group != null )
      {
        double offset;
        double stickyHeadersRegionHeight;
        int groupContainerIndex;

        if( this.GetGroupOffset( group, out offset, out stickyHeadersRegionHeight, out groupContainerIndex ) )
        {
          this.ScrollToVerticalOffset( offset - stickyHeadersRegionHeight );
          e.Handled = true;
        }
      }
    }

    private bool GetGroupOffset( Group group, out double offset, out double stickyHeadersRegionHeight, out int groupContainerIndex )
    {
      groupContainerIndex = ( ( CustomItemContainerGenerator )this.CustomItemContainerGenerator ).GetGroupIndex( group );

      if( groupContainerIndex > -1 )
      {
        stickyHeadersRegionHeight = this.GetStickyHeaderCountForIndex( groupContainerIndex ) * m_containerHeight;
        offset = this.GetContainerOffsetFromIndex( groupContainerIndex );

        return true;
      }

      offset = 0d;
      stickyHeadersRegionHeight = 0d;

      return false;
    }

    #endregion

    internal void OnGroupCollapsing( Group group )
    {
      double groupOffset;
      double stickyHeadersRegionHeight;
      int groupContainerIndex;

      if( this.GetGroupOffset( group, out groupOffset, out stickyHeadersRegionHeight, out groupContainerIndex ) )
      {
        if( m_stickyHeaders.ContainsRealizedIndex( groupContainerIndex )
          || m_stickyFooters.ContainsRealizedIndex( groupContainerIndex )
          || m_layoutedContainers.ContainsRealizedIndex( groupContainerIndex ) )
        {
          double offset = groupOffset - stickyHeadersRegionHeight;

          if( ( offset < m_verticalOffset ) || ( offset >= m_verticalOffset + m_viewportHeight ) )
          {
            this.PreventOpacityAnimation = true;
            this.ScrollToVerticalOffset( offset, false, ScrollDirection.None );
          }
        }
      }
    }

    #region AnimatedHorizontalOffset Property

    private static readonly DependencyProperty AnimatedHorizontalOffsetProperty = DependencyProperty.Register(
      "AnimatedHorizontalOffset",
      typeof( double ),
      typeof( TableflowViewItemsHost ),
      new FrameworkPropertyMetadata( new PropertyChangedCallback( TableflowViewItemsHost.OnAnimatedHorizontalOffsetChanged ) ) );

    private double AnimatedHorizontalOffset
    {
      get
      {
        return ( double )this.GetValue( TableflowViewItemsHost.AnimatedHorizontalOffsetProperty );
      }
      set
      {
        this.SetValue( TableflowViewItemsHost.AnimatedHorizontalOffsetProperty, value );
      }
    }

    private static void OnAnimatedHorizontalOffsetChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      TableflowViewItemsHost host = ( TableflowViewItemsHost )sender;

      host.SetHorizontalOffsetCore( ( double )e.NewValue );
      host.InvalidateArrange();
      host.InvalidateScrollInfo();
    }

    #endregion

    #region AnimatedVerticalOffset Property

    private static readonly DependencyProperty AnimatedVerticalOffsetProperty = DependencyProperty.Register(
      "AnimatedVerticalOffset",
      typeof( double ),
      typeof( TableflowViewItemsHost ),
      new FrameworkPropertyMetadata( new PropertyChangedCallback( TableflowViewItemsHost.OnAnimatedVerticalOffsetChanged ) ) );

    private double AnimatedVerticalOffset
    {
      get
      {
        return ( double )this.GetValue( TableflowViewItemsHost.AnimatedVerticalOffsetProperty );
      }
      set
      {
        this.SetValue( TableflowViewItemsHost.AnimatedVerticalOffsetProperty, value );
      }
    }

    private void OnAnimatedVerticalOffsetChanged( double oldValue, double newValue )
    {
      this.SetVerticalOffsetCore( newValue );
      this.InvalidateLayoutFromScrollingHelper();
    }

    private static void OnAnimatedVerticalOffsetChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = ( TableflowViewItemsHost )sender;

      // Do not generate a page while the changed is due to the unloading 
      // of the grid. See TableflowViewItemsHost.OnParentDataGridControlChanged.
      if( ( self == null ) || ( DataGridControl.GetDataGridContext( self ) == null ) )
        return;

      self.OnAnimatedVerticalOffsetChanged( ( double )e.OldValue, ( double )e.NewValue );
    }

    #endregion

    #region AnimatedScrollInfo Property

    internal IAnimatedScrollInfo AnimatedScrollInfo
    {
      get
      {
        return ( IAnimatedScrollInfo )this;
      }
    }

    #endregion

    #region PreviousTabNavigationMode ( private attached property )

    private static readonly DependencyProperty PreviousTabNavigationModeProperty = DependencyProperty.RegisterAttached(
      "PreviousTabNavigationMode",
      typeof( KeyboardNavigationMode ),
      typeof( TableflowViewItemsHost ),
      new FrameworkPropertyMetadata( ( KeyboardNavigationMode )KeyboardNavigationMode.None ) );

    private static KeyboardNavigationMode GetPreviousTabNavigationMode( DependencyObject d )
    {
      return ( KeyboardNavigationMode )d.GetValue( TableflowViewItemsHost.PreviousTabNavigationModeProperty );
    }

    private static void SetPreviousTabNavigationMode( DependencyObject d, KeyboardNavigationMode value )
    {
      d.SetValue( TableflowViewItemsHost.PreviousTabNavigationModeProperty, value );
    }

    #endregion

    #region PreviousDirectionalNavigationMode ( private attached property )

    private static readonly DependencyProperty PreviousDirectionalNavigationModeProperty = DependencyProperty.RegisterAttached(
      "PreviousDirectionalNavigationMode",
      typeof( KeyboardNavigationMode ),
      typeof( TableflowViewItemsHost ),
      new FrameworkPropertyMetadata( ( KeyboardNavigationMode )KeyboardNavigationMode.None ) );

    private static KeyboardNavigationMode GetPreviousDirectionalNavigationMode( DependencyObject d )
    {
      return ( KeyboardNavigationMode )d.GetValue( TableflowViewItemsHost.PreviousDirectionalNavigationModeProperty );
    }

    private static void SetPreviousDirectionalNavigationMode( DependencyObject d, KeyboardNavigationMode value )
    {
      d.SetValue( TableflowViewItemsHost.PreviousDirectionalNavigationModeProperty, value );
    }

    #endregion

    #region RowSelectorPane Property

    private RowSelectorPane RowSelectorPane
    {
      get
      {
        TableViewScrollViewer tableViewScrollViewer = this.AnimatedScrollInfo.ScrollOwner as TableViewScrollViewer;

        return ( tableViewScrollViewer == null )
          ? null
          : tableViewScrollViewer.RowSelectorPane;
      }
    }

    #endregion

    #region IsSticky Internal Attached Property

    private static readonly DependencyPropertyKey IsStickyPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "IsSticky", typeof( bool ), typeof( TableflowViewItemsHost ),
      new FrameworkPropertyMetadata( false, FrameworkPropertyMetadataOptions.Inherits ) );

    internal static readonly DependencyProperty IsStickyProperty;

    internal static bool GetIsSticky( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableflowViewItemsHost.IsStickyProperty );
    }

    internal static void SetIsSticky( DependencyObject obj, bool value )
    {
      obj.SetValue( TableflowViewItemsHost.IsStickyPropertyKey, value );
    }

    internal static void ClearIsSticky( DependencyObject obj )
    {
      obj.ClearValue( TableflowViewItemsHost.IsStickyPropertyKey );
    }

    #endregion

    #region AreHeadersStickyCache Private Property

    private bool AreHeadersStickyCache
    {
      get
      {
        return m_flags[ ( int )TableflowItemsHostFlags.AreHeadersSticky ];
      }
      set
      {
        m_flags[ ( int )TableflowItemsHostFlags.AreHeadersSticky ] = value;
      }
    }

    #endregion

    #region AreFootersStickyCache Private Property

    private bool AreFootersStickyCache
    {
      get
      {
        return m_flags[ ( int )TableflowItemsHostFlags.AreFootersSticky ];
      }
      set
      {
        m_flags[ ( int )TableflowItemsHostFlags.AreFootersSticky ] = value;
      }
    }

    #endregion

    #region AreGroupHeadersStickyCache Private Property

    private bool AreGroupHeadersStickyCache
    {
      get
      {
        return m_flags[ ( int )TableflowItemsHostFlags.AreGroupHeadersSticky ];
      }
      set
      {
        m_flags[ ( int )TableflowItemsHostFlags.AreGroupHeadersSticky ] = value;
      }
    }

    #endregion

    #region AreGroupFootersStickyCache Private Property

    private bool AreGroupFootersStickyCache
    {
      get
      {
        return m_flags[ ( int )TableflowItemsHostFlags.AreGroupFootersSticky ];
      }
      set
      {
        m_flags[ ( int )TableflowItemsHostFlags.AreGroupFootersSticky ] = value;
      }
    }

    #endregion

    #region AreParentRowsStickyCache Private Property

    private bool AreParentRowsStickyCache
    {
      get
      {
        return m_flags[ ( int )TableflowItemsHostFlags.AreParentRowSticky ];
      }
      set
      {
        m_flags[ ( int )TableflowItemsHostFlags.AreParentRowSticky ] = value;
      }
    }

    #endregion

    #region PreventOpacityAnimation Private Property

    private bool PreventOpacityAnimation
    {
      get
      {
        return m_flags[ ( int )TableflowItemsHostFlags.PreventOpacityAnimation ];
      }
      set
      {
        m_flags[ ( int )TableflowItemsHostFlags.PreventOpacityAnimation ] = value;
      }
    }

    #endregion

    #region IsDeferredLoadingEnabledCache Private Property

    private bool IsDeferredLoadingEnabledCache
    {
      get
      {
        return m_flags[ ( int )TableflowItemsHostFlags.IsDeferredLoadingEnabled ];
      }
      set
      {
        m_flags[ ( int )TableflowItemsHostFlags.IsDeferredLoadingEnabled ] = value;
      }
    }

    #endregion

    #region Measure/Arrange Methods

    protected override Size MeasureOverride( Size availableSize )
    {
      m_cachedContainerDesiredWidth.Clear();
      m_cachedContainerRealDesiredWidth.Clear();
      m_autoWidthCalculatedDataGridContextList.Clear();

      m_lastMeasureAvailableSize = availableSize;

      this.CacheViewProperties();

      // CALCULATE THE VIEWPORT HEIGHT
      m_viewportHeight = Double.IsInfinity( availableSize.Height )
        ? this.AnimatedScrollInfo.ExtentHeight
        : Math.Min( availableSize.Height, this.AnimatedScrollInfo.ExtentHeight );

      this.CalculatePageItemCount( m_viewportHeight );
      this.EnsureVerticalOffsetValid( m_viewportHeight );
      this.GeneratePage( true, m_viewportHeight );

      double synchronizedExtentWidth = 0d;
      DataGridScrollViewer scrollViewer = this.AnimatedScrollInfo.ScrollOwner as DataGridScrollViewer;

      if( scrollViewer != null )
      {
        synchronizedExtentWidth = scrollViewer.SynchronizedScrollViewersWidth;
      }

      // CALCULATE THE EXTENT WIDTH
      m_extentWidth = Math.Max( this.GetMaxDesiredWidth(), synchronizedExtentWidth );

      // CALCULATE THE VIEWPORT WIDTH
      m_viewportWidth = Double.IsInfinity( availableSize.Width )
        ? m_extentWidth : Math.Min( availableSize.Width, m_extentWidth );

      this.InvalidateScrollInfo();

      if( m_delayedBringIntoViewIndex != -1 )
      {
        this.DelayedBringIntoView();
      }

      return new Size( m_viewportWidth, m_viewportHeight );
    }

    private void CacheViewProperties()
    {
      DataGridContext dataGridContext = this.CachedRootDataGridContext;

      m_containerHeight = TableflowView.GetContainerHeight( dataGridContext );

      this.AreHeadersStickyCache = TableflowView.GetAreHeadersSticky( dataGridContext );
      this.AreFootersStickyCache = TableflowView.GetAreFootersSticky( dataGridContext );
      this.AreGroupHeadersStickyCache = TableflowView.GetAreGroupHeadersSticky( dataGridContext );
      this.AreGroupFootersStickyCache = TableflowView.GetAreGroupFootersSticky( dataGridContext );
      this.AreParentRowsStickyCache = TableflowView.GetAreParentRowsSticky( dataGridContext );
      this.IsDeferredLoadingEnabledCache = TableflowView.GetIsDeferredLoadingEnabled( dataGridContext );
    }

    private void MeasureContainer( UIElement container )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( container );

      string dataGridContextName = this.GetDataGridContextName( container, dataGridContext );
      bool containerIsRow = this.ContainerIsRow( container );

      if( !m_autoWidthCalculatedDataGridContextList.Contains( dataGridContextName )
        && containerIsRow )
      {
        dataGridContext.ColumnStretchingManager.ColumnStretchingCalculated = false;

        // Calling Measure with the Viewport's width will have the effect of 
        // distributing the extra space (see FixedCellPanel's MeasureOverride). 
        // Eventually, the FixedCellPanel will receive an adjusted viewport 
        // width (where GroupLevelIndicator's width et al will be substracted).
        container.Measure( new Size( m_lastMeasureAvailableSize.Width, m_containerHeight ) );

        if( dataGridContext.ColumnStretchingManager.ColumnStretchingCalculated )
        {
          // We only calculate once per detail.
          m_autoWidthCalculatedDataGridContextList.Add( dataGridContextName );
        }
      }
      else
      {
        // We invalidate the CellsHostPanel of the row and all is parent to ensure the
        // size is correctly evaluated when we have some auto stretching column.
        container.InvalidateMeasure();

        Row row = Row.FromContainer( container );
        if( row != null )
        {
          UIElement itemToInvalidate = row.CellsHostPanel;

          while( ( itemToInvalidate != null ) && ( itemToInvalidate != container ) )
          {
            itemToInvalidate.InvalidateMeasure();
            itemToInvalidate = VisualTreeHelper.GetParent( itemToInvalidate ) as UIElement;
          }
        }
      }

      // We always measure the container with infinity so that we'll arrange each container,
      // for a DataGridContext, with the maximum needed for that context.
      container.Measure( new Size( double.PositiveInfinity, m_containerHeight ) );

      double desiredSize = container.DesiredSize.Width;
      double cachedSize;

      if( m_cachedContainerDesiredWidth.TryGetValue( dataGridContextName, out cachedSize ) )
      {
        // Keep the largest size!
        if( cachedSize < desiredSize )
        {
          m_cachedContainerDesiredWidth[ dataGridContextName ] = desiredSize;
        }
      }
      else
      {
        // Cache the size for the context.
        m_cachedContainerDesiredWidth.Add( dataGridContextName, desiredSize );
      }

      PassiveLayoutDecorator decorator = this.GetPassiveLayoutDecorator( container as HeaderFooterItem );
      if( decorator != null )
      {
        desiredSize = decorator.RealDesiredSize.Width;

        if( m_cachedContainerRealDesiredWidth.TryGetValue( dataGridContextName, out cachedSize ) )
        {
          // Keep the largest size!
          if( cachedSize < desiredSize )
          {
            m_cachedContainerRealDesiredWidth[ dataGridContextName ] = desiredSize;
          }
        }
        else
        {
          // Cache the size for the context.
          m_cachedContainerRealDesiredWidth.Add( dataGridContextName, desiredSize );
        }
      }
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
      // Never call InvalidateScrollInfo() in there, that can cause infinit invalidation loop.
      m_lastArrangeFinalSize = finalSize;

      this.CalculatePageItemCount( finalSize.Height );
      this.LayoutContainers();

      m_delayedBringIntoViewIndex = -1;

      return finalSize;
    }

    private PassiveLayoutDecorator GetPassiveLayoutDecorator( HeaderFooterItem item )
    {
      if( ( item == null ) || ( VisualTreeHelper.GetChildrenCount( item ) == 0 ) )
        return null;

      var child = VisualTreeHelper.GetChild( item, 0 );
      if( child == null )
        return null;

      var decorator = child as PassiveLayoutDecorator;
      if( ( decorator == null ) && ( VisualTreeHelper.GetChildrenCount( child ) > 0 ) )
      {
        decorator = VisualTreeHelper.GetChild( child, 0 ) as PassiveLayoutDecorator;
      }

      return decorator;
    }

    private void ArrangeContainer( UIElement container, Point translationPoint, bool rowSelectorPaneVisible )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( container );
      string dataGridContextName = this.GetDataGridContextName( container, dataGridContext );

      Size containerSize = new Size(
        this.GetContainerWidth( dataGridContextName ),
        m_containerHeight );

      container.Arrange( new Rect( translationPoint, containerSize ) );
      this.SetCompensationOffset( dataGridContext, container, containerSize.Width );

      if( rowSelectorPaneVisible )
      {
        this.SetRowSelector( container, translationPoint, containerSize );
      }
    }

    private double GetContainerWidth( string dataGridContextName )
    {
      double synchronizedWidth = this.GetSynchronizedExtentWidth();
      double desiredWidth = 0;

      bool sizeCached = m_cachedContainerDesiredWidth.TryGetValue( dataGridContextName, out desiredWidth );

      if( !sizeCached )
      {
        desiredWidth = synchronizedWidth;
      }

      if( string.IsNullOrEmpty( dataGridContextName ) ) // master level 
      {
        //for the master level, I want to consider the Synchronized Extent size as well.
        desiredWidth = Math.Max( synchronizedWidth, desiredWidth );
      }

      if( desiredWidth == 0 )
      {
        m_cachedContainerRealDesiredWidth.TryGetValue( dataGridContextName, out desiredWidth );
      }

      return desiredWidth;
    }

    private double GetSynchronizedExtentWidth()
    {
      DataGridScrollViewer dgScrollViewer = this.AnimatedScrollInfo.ScrollOwner as DataGridScrollViewer;

      if( dgScrollViewer != null )
        return dgScrollViewer.SynchronizedScrollViewersWidth;

      return 0;
    }

    private void SetCompensationOffset( DataGridContext dataGridContext, UIElement container, double desiredWidth )
    {
      double compensationOffset = Math.Max(
        0,
        ( ( m_horizontalOffset + Math.Min( m_viewportWidth, desiredWidth ) ) - desiredWidth ) );

      TableView.SetCompensationOffset( container, compensationOffset );

      // Affect the CompensationOffset on the DataGridContext for the TableViewColumnVirtualizationManager
      // to bind to this value
      TableView.SetCompensationOffset( dataGridContext, compensationOffset );
    }

    private void SetRowSelector( UIElement container, Point translationPoint, Size size )
    {
      RowSelectorPane rowSelectorPane = this.RowSelectorPane;

      if( rowSelectorPane == null )
        return;

      rowSelectorPane.SetRowSelectorPosition(
        container,
        new Rect( translationPoint, size ),
        this );
    }

    private void FreeRowSelector( UIElement container )
    {
      RowSelectorPane rowSelectorPane = this.RowSelectorPane;

      if( rowSelectorPane == null )
        return;

      rowSelectorPane.FreeRowSelector( container );
    }

    private double GetMaxDesiredWidth()
    {
      double maxDesiredWidth = 0d;

      foreach( var item in m_cachedContainerDesiredWidth )
      {
        double desiredWidth = item.Value;

        if( desiredWidth == 0 )
        {
          m_cachedContainerRealDesiredWidth.TryGetValue( item.Key, out desiredWidth );
        }

        if( desiredWidth > maxDesiredWidth )
        {
          maxDesiredWidth = desiredWidth;
        }
      }

      return maxDesiredWidth;
    }

    #endregion

    #region Containers Methods

    private bool ContainerIsRow( UIElement container )
    {
      if( container is Row )
        return true;

      HeaderFooterItem headerFooterItem = container as HeaderFooterItem;

      if( headerFooterItem != null )
        return typeof( Row ).IsAssignableFrom( headerFooterItem.VisualRootElementType );

      return false;
    }

    private string GetDataGridContextName( UIElement container )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( container );

      return this.GetDataGridContextName( container, dataGridContext );
    }

    private string GetDataGridContextName( UIElement container, DataGridContext dataGridContext )
    {
      return ( dataGridContext.SourceDetailConfiguration != null )
        ? dataGridContext.SourceDetailConfiguration.RelationName
        : string.Empty;
    }

    private void CalculatePageItemCount( double height )
    {
      m_pageVisibleContainerCount = Double.IsInfinity( height )
        ? this.CustomItemContainerGenerator.ItemCount
        : ( int )Math.Ceiling( height / m_containerHeight );
    }

    private int CalculateFlooredPageItemCount( double height )
    {
      return Double.IsInfinity( height )
        ? this.CustomItemContainerGenerator.ItemCount
        : ( int )Math.Floor( height / m_containerHeight );
    }

    private void CalculateLayoutedIndexes( double verticalOffset, double viewportHeight, out PageIndexes layoutedIndexes )
    {
      layoutedIndexes = new PageIndexes();

      int itemCount = this.CustomItemContainerGenerator.ItemCount;
      if( itemCount > 0 )
      {
        layoutedIndexes.StartIndex = Math.Min(
          this.GetContainerIndexFromOffset( verticalOffset, false ),
          itemCount - 1 );

        layoutedIndexes.EndIndex = Math.Min(
          this.GetContainerIndexFromOffset( verticalOffset + viewportHeight - m_containerHeight, true ),
          itemCount - 1 );

        layoutedIndexes.EndIndex = Math.Max(
          layoutedIndexes.StartIndex,
          layoutedIndexes.EndIndex );
      }
    }

    private PageIndexes CalculateVisibleIndexesForDesiredFirstVisibleIndex( int desiredFirstVisibleIndex )
    {
      PageIndexes layoutedIndexes = new PageIndexes();

      // Get the sticky header count for the desiredFirstVisibleIndex
      // and set the StartIndex
      int stickyHeaderCountForFirstVisibleIndex = this.GetStickyHeaderCountForIndex( desiredFirstVisibleIndex );
      layoutedIndexes.StartIndex = desiredFirstVisibleIndex - stickyHeaderCountForFirstVisibleIndex;

      // Get the index of the maximal visible index according
      // to the number of container the viewport can display
      // and the number of sticky header the desiredFirstVisibleIndex
      // must display
      int maxLastVisibleIndex = layoutedIndexes.StartIndex
        + this.CalculateFlooredPageItemCount( this.AnimatedScrollInfo.ViewportHeight );

      layoutedIndexes.EndIndex = maxLastVisibleIndex;

      // Process indexes from top to bottom up to an index for which
      // the number of required sticky footers will force a visible index
      // larger than the maximal acceptable last visible index
      for( int i = desiredFirstVisibleIndex + 1; i <= maxLastVisibleIndex; i++ )
      {
        int stickyFooterCountForIndex = this.GetStickyFooterCountForIndex( i );

        // The index of the container + its sticky footer count
        // is greater than the last acceptable visible index, we ensure
        // the previous container as the maximal one to set as current
        if( maxLastVisibleIndex < ( i + stickyFooterCountForIndex ) )
        {
          layoutedIndexes.EndIndex = i - 1;
          break;
        }
      }

      return layoutedIndexes;
    }

    private PageIndexes CalculateVisibleIndexesForDesiredLastVisibleIndex( int desiredLastVisibleIndex )
    {
      PageIndexes layoutedIndexes = new PageIndexes();

      int stickyFooterCountForLastVisibleIndex = this.GetStickyFooterCountForIndex( desiredLastVisibleIndex );
      layoutedIndexes.EndIndex = desiredLastVisibleIndex + stickyFooterCountForLastVisibleIndex;

      int minVisibleIndex = layoutedIndexes.EndIndex
        - this.CalculateFlooredPageItemCount( this.AnimatedScrollInfo.ViewportHeight );

      layoutedIndexes.StartIndex = minVisibleIndex;

      // Process indexes from bottom to top up to an index for which
      // the number of required sticky headers will force a visible index
      // larger than the maximal acceptable last visible index
      for( int i = desiredLastVisibleIndex - 1; i >= minVisibleIndex; i-- )
      {
        int stickyHeaderCountForIndex = this.GetStickyHeaderCountForIndex( i );

        // If the index of the container - its sticky header count
        // is less than the minimal acceptable visible index, we ensure
        // the next container index as the minimal one to set as current
        if( minVisibleIndex > ( i - stickyHeaderCountForIndex ) )
        {
          layoutedIndexes.StartIndex = i + 1;
          break;
        }
      }

      return layoutedIndexes;
    }

    private void GeneratePage( bool measureInvalidated, double viewportHeight )
    {
      ICustomItemContainerGenerator generator = this.CustomItemContainerGenerator;

      // The generator can be null if we're in design mode.
      if( generator != null )
      {
        if( Double.IsInfinity( viewportHeight ) )
        {
          viewportHeight = this.AnimatedScrollInfo.ExtentHeight;
        }

        // Make sure that container recycling is currently enabled on the generator.
        generator.IsRecyclingEnabled = true;

        // Calculate the start and end index of the current page.
        PageIndexes visiblePageIndexes;
        this.CalculateLayoutedIndexes( m_verticalOffset, viewportHeight, out visiblePageIndexes );

        bool pageChanged = ( m_lastVisiblePageIndexes != visiblePageIndexes );

        if( measureInvalidated || pageChanged )
        {
          ICollection<UIElement> nonRecyclableContainers;

          if( m_containerHeight > 0d )
          {
            // Recycle the containers we know will not be in the current page.  GenerateContainers will recycle
            // the remaining containers that were uncertain.
            nonRecyclableContainers = this.RecycleSafeContainers( generator, visiblePageIndexes.StartIndex, visiblePageIndexes.EndIndex );

            this.GenerateContainers( generator, visiblePageIndexes.StartIndex, visiblePageIndexes.EndIndex, measureInvalidated, nonRecyclableContainers );
            this.GenerateStickyHeaders( generator, measureInvalidated, nonRecyclableContainers );
            this.GenerateStickyFooters( generator, measureInvalidated, nonRecyclableContainers );
          }
          else
          {
            nonRecyclableContainers = this.RecycleAllContainers( generator );
            Debug.Assert( m_layoutedContainers.Count == 0 );
          }

          // Keep alive containers that cannot be recycled (i.e. focused element).
          this.KeepNonRecyclableContainers( generator, measureInvalidated, nonRecyclableContainers );
        }

        m_lastVisiblePageIndexes = visiblePageIndexes;

        this.PreventOpacityAnimation = false;
      }

    }

    private ICollection<UIElement> RecycleContainers( ICustomItemContainerGenerator generator, int pageStartIndex, int pageEndIndex, bool clearContainer = false )
    {
      var nonRecyclableContainers = new HashSet<UIElement>( m_layoutedContainers.Select( entry => entry.Container ).Where( c => !this.CanRecycleContainer( c ) ) );

      for( int i = m_layoutedContainers.Count - 1; i >= 0; i-- )
      {
        var container = m_layoutedContainers[ i ].Container;

        var index = generator.GetRealizedIndexForContainer( container );

        // If the container's index is now out of view, recycle that container.
        if( ( index < pageStartIndex ) || ( index > pageEndIndex ) )
        {
          if( !nonRecyclableContainers.Contains( container ) )
          {
            this.RecycleContainer( generator, index, container, clearContainer );
          }

          m_layoutedContainers.RemoveAt( i );
        }
      }

      return nonRecyclableContainers;
    }

    private ICollection<UIElement> RecycleAllContainers( ICustomItemContainerGenerator generator )
    {
      return this.RecycleContainers( generator, -1, -1, true );
    }

    private ICollection<UIElement> RecycleSafeContainers( ICustomItemContainerGenerator generator, int pageStartIndex, int pageEndIndex )
    {
      if( m_lastVisiblePageIndexes == PageIndexes.Empty )
        return this.RecycleContainers( generator, pageStartIndex, pageEndIndex );

      if( m_lastVisiblePageIndexes.StartIndex == pageStartIndex )
        return this.RecycleContainers( generator,
                                       pageStartIndex,
                                       Math.Max( pageEndIndex, m_lastVisiblePageIndexes.EndIndex ) );

      if( m_lastVisiblePageIndexes.EndIndex == pageEndIndex )
        return this.RecycleContainers( generator,
                                       Math.Min( pageStartIndex, m_lastVisiblePageIndexes.StartIndex ),
                                       pageEndIndex );

      int itemCount = generator.ItemCount;
      int threshold = Math.Max( 0, ( m_lastVisiblePageIndexes.EndIndex - m_lastVisiblePageIndexes.StartIndex ) - ( pageEndIndex - pageStartIndex ) );

      int endIndex = Math.Min( pageEndIndex + threshold, itemCount - 1 );
      if( endIndex < 0 )
        return this.RecycleAllContainers( generator );

      int startIndex = Math.Max( endIndex - threshold - ( pageEndIndex - pageStartIndex ), 0 );

      return this.RecycleContainers( generator, startIndex, endIndex );
    }

    private void RecycleUnusedStickyContainers(
      ICustomItemContainerGenerator generator,
      StickyContainerInfoList stickyContainers,
      StickyContainerInfoList stickyContainersToExclude,
      ICollection<UIElement> nonRecyclableContainers )
    {
      for( int i = stickyContainers.Count - 1; i >= 0; i-- )
      {
        StickyContainerInfo stickyHeaderInfo = stickyContainers[ i ];
        UIElement container = stickyHeaderInfo.Container;

        if( m_layoutedContainers.ContainsContainer( container ) )
          continue;

        if( ( stickyContainersToExclude != null ) && ( stickyContainersToExclude.ContainsContainer( container ) ) )
          continue;

        if( ( nonRecyclableContainers != null ) && nonRecyclableContainers.Contains( container ) )
          continue;

        var index = generator.GetRealizedIndexForContainer( container );

        this.RecycleContainer( generator, index, container );
        stickyContainers.RemoveAt( i );
      }
    }

    private void RecycleContainer( UIElement container )
    {
      this.RecycleContainer( null, -1, container );
    }

    private void RecycleContainer( ICustomItemContainerGenerator generator, int containerIndex, UIElement container, bool clearContainer = true )
    {
      if( !clearContainer )
      {
        var dataGridItemContainer = container as IDataGridItemContainer;
        if( dataGridItemContainer != null )
        {
          dataGridItemContainer.IsRecyclingCandidate = true;
        }
      }
      else
      {
        this.ClearContainer( container );
      }

      if( ( generator != null ) && ( containerIndex != -1 ) )
      {
        try
        {
          GeneratorPosition position = generator.GeneratorPositionFromIndex( containerIndex );
          generator.Remove( position, 1 );
        }
        catch
        {
          Debug.Fail( "Unable to remove container for containerIndex " + containerIndex );
        }
      }
    }

    private void CleanRecyclingCandidates()
    {
      var generator = this.CustomItemContainerGenerator as CustomItemContainerGenerator;
      generator.CleanRecyclingCandidates();
    }

    private void GenerateContainers( ICustomItemContainerGenerator generator, int pageStartIndex, int pageEndIndex, bool measureInvalidated, ICollection<UIElement> nonRecyclableContainers )
    {
      HashSet<UIElement> unusedLayoutedContainers = new HashSet<UIElement>( m_layoutedContainers.Select( item => item.Container ) );
      m_layoutedContainers.Clear();

      ScrollDirection scrollDirection = this.AnimatedScrollInfo.VerticalScrollingDirection;

      GeneratorPosition position;
      GeneratorDirection direction;
      int currentIndex;
      int step;

      if( ( scrollDirection == ScrollDirection.Forward )
        || ( scrollDirection == ScrollDirection.None ) )
      {
        position = generator.GeneratorPositionFromIndex( pageStartIndex );
        direction = GeneratorDirection.Forward;
        currentIndex = pageStartIndex;
        step = 1;
      }
      else
      {
        position = generator.GeneratorPositionFromIndex( pageEndIndex );
        direction = GeneratorDirection.Backward;
        currentIndex = pageEndIndex;
        step = -1;
      }

      using( generator.StartAt( position, direction, true ) )
      {
        while( ( currentIndex >= pageStartIndex ) && ( currentIndex <= pageEndIndex ) )
        {
          var container = this.GenerateContainer( generator, currentIndex, measureInvalidated, true );
          if( container == null )
            break;

          // The container is now part of the page layout. Add it to the list.
          m_layoutedContainers.Add( new LayoutedContainerInfo( currentIndex, container ) );
          unusedLayoutedContainers.Remove( container );
          nonRecyclableContainers.Remove( container );

          currentIndex += step;
        }
      }

      // A ScrollViewer may measure forever if the containers that are "underneath" the horizontal
      // scrollbar are the ones that required the scrollbar in the first place.
      if( m_lastVisiblePageIndexes != PageIndexes.Empty )
      {
        int remainingItemCount = ( m_lastVisiblePageIndexes.EndIndex - m_lastVisiblePageIndexes.StartIndex )
                               - ( pageEndIndex - pageStartIndex );

        if( ( remainingItemCount > 0 ) && ( this.GetMaxDesiredWidth() < this.AnimatedScrollInfo.ExtentWidth ) )
        {
          int itemCount = generator.ItemCount;

          if( ( currentIndex >= 0 ) && ( currentIndex < itemCount ) )
          {
            position = generator.GeneratorPositionFromIndex( currentIndex );

            using( generator.StartAt( position, direction, true ) )
            {
              while( ( remainingItemCount > 0 ) && ( currentIndex >= 0 ) && ( currentIndex < itemCount ) )
              {
                var container = this.GenerateContainer( generator, currentIndex, measureInvalidated, true );
                if( container == null )
                  break;

                // The container is now part of the page layout. Add it to the list.
                m_layoutedContainers.Add( new LayoutedContainerInfo( currentIndex, container ) );
                unusedLayoutedContainers.Remove( container );
                nonRecyclableContainers.Remove( container );

                currentIndex += step;
                remainingItemCount--;
              }
            }
          }
        }
      }

      m_layoutedContainers.Sort();

      // Recycle the containers that have not been reused in the current page.
      foreach( var container in unusedLayoutedContainers )
      {
        if( nonRecyclableContainers.Contains( container ) )
          continue;

        var index = generator.GetRealizedIndexForContainer( container );

        this.RecycleContainer( generator, index, container );
      }

      this.CleanRecyclingCandidates();
    }

    private void KeepNonRecyclableContainers( ICustomItemContainerGenerator generator, bool measureInvalidated, ICollection<UIElement> nonRecyclableContainers )
    {
      if( ( nonRecyclableContainers == null ) || ( nonRecyclableContainers.Count == 0 ) )
        return;

      foreach( var container in nonRecyclableContainers )
      {
        var index = generator.GetRealizedIndexForContainer( container );
        if( index >= 0 )
        {
          if( measureInvalidated )
          {
            this.MeasureContainer( container );
          }

          m_layoutedContainers.Add( new LayoutedContainerInfo( index, container ) );
        }
        else
        {
          this.RecycleContainer( container );
        }
      }
    }

    private int GetStickyHeaderCountForIndex( int desiredIndex )
    {
      var dataGridContext = DataGridControl.GetDataGridContext( this );
      if( dataGridContext == null )
        return 0;

      var customGenerator = dataGridContext.CustomItemContainerGenerator;
      if( customGenerator == null )
        return 0;

      return customGenerator.GetStickyHeaderCountForIndex( desiredIndex,
                                                           this.AreHeadersStickyCache,
                                                           this.AreGroupHeadersStickyCache,
                                                           this.AreParentRowsStickyCache );
    }

    private int GetStickyFooterCountForIndex( int desiredIndex )
    {
      var dataGridContext = DataGridControl.GetDataGridContext( this );
      if( dataGridContext == null )
        return 0;

      var customGenerator = dataGridContext.CustomItemContainerGenerator;
      if( customGenerator == null )
        return 0;

      return customGenerator.GetStickyFooterCountForIndex( desiredIndex,
                                                           this.AreFootersStickyCache,
                                                           this.AreGroupFootersStickyCache );
    }

    private void GenerateStickyHeaders( ICustomItemContainerGenerator generator, bool measureInvalidated, ICollection<UIElement> nonRecyclableContainers )
    {
      var customGenerator = ( CustomItemContainerGenerator )generator;
      var newStickyHeaders = new StickyContainerInfoList();

      if( this.AreHeadersStickyCache || this.AreGroupHeadersStickyCache || this.AreParentRowsStickyCache )
      {
        var numberOfContainerChecked = 0;

        foreach( var layoutedContainerInfo in m_layoutedContainers )
        {
          var container = layoutedContainerInfo.Container;

          // For each visible container, we must find what headers should be sticky for it.
          var stickyHeaders = customGenerator.GenerateStickyHeaders( container, this.AreHeadersStickyCache, this.AreGroupHeadersStickyCache, this.AreParentRowsStickyCache );

          stickyHeaders.Sort( StickyContainerGeneratedComparer.Singleton );

          // For each sticky headers returned, we must get the index of the last container
          // that could need that container to be sticky. 
          foreach( var stickyHeaderInfo in stickyHeaders )
          {
            var stickyContainer = ( UIElement )stickyHeaderInfo.StickyContainer;
            if( newStickyHeaders.ContainsContainer( stickyContainer ) )
              continue;

            var lastContainerIndex = customGenerator.GetLastHoldingContainerIndexForStickyHeader( stickyContainer );

            var stickyContainerInfo = new StickyContainerInfo( stickyContainer, stickyHeaderInfo.Index, lastContainerIndex );
            newStickyHeaders.Add( stickyContainerInfo );
            nonRecyclableContainers.Remove( stickyContainer );

            this.HandleGeneratedStickyContainerPreparation( stickyHeaderInfo, measureInvalidated );
          }

          // We only need to find the sticky headers for one 
          // more element than what is already sticky.
          var visibleStickyHeadersCount = ( int )Math.Ceiling( this.GetStickyHeadersRegionHeight( newStickyHeaders ) / m_containerHeight );

          if( ( ++numberOfContainerChecked - visibleStickyHeadersCount ) >= 1 )
            break;
        }

        foreach( var stickyHeaderInfo in newStickyHeaders )
        {
          var index = m_layoutedContainers.IndexOfContainer( stickyHeaderInfo.Container );
          if( index < 0 )
            continue;

          m_layoutedContainers.RemoveAt( index );
        }

        foreach( var stickyHeaderInfo in m_stickyHeaders )
        {
          var container = stickyHeaderInfo.Container;

          if( !newStickyHeaders.ContainsContainer( container ) && !nonRecyclableContainers.Contains( container ) && !this.CanRecycleContainer( container ) )
          {
            nonRecyclableContainers.Add( container );
          }
        }
      }

      this.RecycleUnusedStickyContainers( generator, m_stickyHeaders, newStickyHeaders, nonRecyclableContainers );

      m_stickyHeaders.Clear();

      if( newStickyHeaders.Count > 0 )
      {
        m_stickyHeaders.AddRange( newStickyHeaders );
        m_stickyHeaders.Sort( StickyContainerInfoComparer.Singleton );
      }
    }

    private void GenerateStickyFooters( ICustomItemContainerGenerator generator, bool measureInvalidated, ICollection<UIElement> nonRecyclableContainers )
    {
      var newStickyFooters = new StickyContainerInfoList();

      if( this.AreFootersStickyCache || this.AreGroupFootersStickyCache )
      {
        var customGenerator = ( CustomItemContainerGenerator )generator;
        var numberOfContainerChecked = 0;
        var layoutedContainerCount = m_layoutedContainers.Count;

        if( layoutedContainerCount > 0 )
        {
          // We must not generate sticky footers if the last container is not even at the bottom of the view!
          var bottomMostContainerInfo = m_layoutedContainers[ layoutedContainerCount - 1 ];
          var bottomMostContainerOffset = this.GetContainerOffsetFromIndex( bottomMostContainerInfo.RealizedIndex );

          if( ( bottomMostContainerOffset + m_containerHeight ) >= m_viewportHeight )
          {
            for( var i = layoutedContainerCount - 1; i >= 0; i-- )
            {
              var layoutedContainerInfo = m_layoutedContainers[ i ];
              var layoutedContainer = layoutedContainerInfo.Container;

              // For each visible container, we must find what footers should be sticky for it.
              var stickyFooters = customGenerator.GenerateStickyFooters( layoutedContainer, this.AreFootersStickyCache, this.AreGroupFootersStickyCache );

              stickyFooters.Sort( StickyContainerGeneratedReverseComparer.Singleton );

              // For each sticky headers returned, we must get the index of the last container
              // that could need that container to be sticky. 
              foreach( var stickyFooterInfo in stickyFooters )
              {
                var stickyContainer = ( UIElement )stickyFooterInfo.StickyContainer;
                if( newStickyFooters.ContainsContainer( stickyContainer ) )
                  continue;

                var firstContainerIndex = customGenerator.GetFirstHoldingContainerIndexForStickyFooter( stickyContainer );

                var stickyContainerInfo = new StickyContainerInfo( stickyContainer, stickyFooterInfo.Index, firstContainerIndex );
                newStickyFooters.Add( stickyContainerInfo );
                nonRecyclableContainers.Remove( stickyContainer );

                this.HandleGeneratedStickyContainerPreparation( stickyFooterInfo, measureInvalidated );
              }

              // We only need to find the sticky footers for one 
              // more element than what is already sticky.
              var visibleStickyFootersCount = ( int )Math.Ceiling( ( this.AnimatedScrollInfo.ViewportHeight - this.GetStickyFootersRegionHeight( newStickyFooters ) ) / m_containerHeight );

              if( ( ++numberOfContainerChecked - visibleStickyFootersCount ) >= 1 )
                break;
            }
          }
        }

        foreach( var stickyFooterInfo in newStickyFooters )
        {
          var index = m_layoutedContainers.IndexOfContainer( stickyFooterInfo.Container );
          if( index < 0 )
            continue;

          m_layoutedContainers.RemoveAt( index );
        }

        foreach( var stickyFooterInfo in m_stickyFooters )
        {
          var container = stickyFooterInfo.Container;

          if( !newStickyFooters.ContainsContainer( container ) && !nonRecyclableContainers.Contains( container ) && !this.CanRecycleContainer( container ) )
          {
            nonRecyclableContainers.Add( container );
          }
        }
      }

      this.RecycleUnusedStickyContainers( generator, m_stickyFooters, newStickyFooters, nonRecyclableContainers );

      m_stickyFooters.Clear();

      if( newStickyFooters.Count > 0 )
      {
        m_stickyFooters.AddRange( newStickyFooters );
        m_stickyFooters.Sort( StickyContainerInfoReverseComparer.Singleton );
      }
    }

    private void HandleGeneratedStickyContainerPreparation( StickyContainerGenerated stickyContainerInfo, bool measureInvalidated )
    {
      UIElement container = ( UIElement )stickyContainerInfo.StickyContainer;
      bool isNewlyRealized = stickyContainerInfo.IsNewlyRealized;

      if( isNewlyRealized )
      {
        if( !this.Children.Contains( container ) )
        {
          this.Children.Add( container );
        }

        this.EnableElementNavigation( container );
        KeyboardNavigation.SetTabIndex( container, stickyContainerInfo.Index );
      }

      TableflowViewItemsHost.SetIsSticky( container, true );

      // This will prepare, if needed, the container.
      this.HandleGeneratedContainerPreparation( container, stickyContainerInfo.Index, isNewlyRealized, true );

      // Measure, if needed, the container.
      if( isNewlyRealized || measureInvalidated )
      {
        this.MeasureContainer( container );
      }
    }

    private UIElement GenerateContainer( ICustomItemContainerGenerator generator, int index, bool measureInvalidated, bool delayDataContext )
    {
      bool isNewlyRealized;
      UIElement container = ( UIElement )generator.GenerateNext( out isNewlyRealized );

      if( container != null )
      {
        if( isNewlyRealized )
        {
          if( !this.Children.Contains( container ) )
          {
            this.Children.Add( container );
          }

          this.EnableElementNavigation( container );
          KeyboardNavigation.SetTabIndex( container, index );
        }

        this.HandleGeneratedContainerPreparation( container, index, isNewlyRealized, delayDataContext );

        if( ( isNewlyRealized ) || ( measureInvalidated ) )
        {
          this.MeasureContainer( container );
        }
      }

      return container;
    }

    private void HandleGeneratedContainerPreparation( UIElement container, int containerIndex, bool isNewlyRealized, bool delayDataContext )
    {
#if DEBUG
      TableflowViewItemsHost.SetRealizedIndex( container, containerIndex );
#endif //DEBUG

      try
      {
        TableflowViewItemsHost.SetShouldDelayDataContext(
          container, ( !this.PreventOpacityAnimation && this.IsDeferredLoadingEnabledCache && delayDataContext ) );

        if( isNewlyRealized )
        {
          // We must prepare the container if the container preparation
          // should not be delayed or if we've been ask to prepare it (from 
          // a keyboard navigation for example)
          this.PrepareContainer( container );
        }

        // We must set those properties AFTER calling PrepareContainer since PrepareContainer
        // will set the ItemContainerStyle which could affect the Height properties. In our case,
        // we want to override those properties so that our layout is ok.
        container.SetValue( FrameworkElement.MinHeightProperty, m_containerHeight );
        container.SetValue( FrameworkElement.MaxHeightProperty, m_containerHeight );
        container.SetValue( FrameworkElement.HeightProperty, m_containerHeight );

        // In the case of HeaderFooterItems, we also want to set the Height properties of the inner 
        // container if that container is a Row or a descendant.
        HeaderFooterItem headerFooterItem = container as HeaderFooterItem;

        if( ( headerFooterItem != null )
          && ( headerFooterItem.Container != null )
          && ( this.ContainerIsRow( container ) ) )
        {
          headerFooterItem.Container.SetValue( FrameworkElement.MinHeightProperty, m_containerHeight );
          headerFooterItem.Container.SetValue( FrameworkElement.MaxHeightProperty, m_containerHeight );
          headerFooterItem.Container.SetValue( FrameworkElement.HeightProperty, m_containerHeight );
        }
      }
      finally
      {
        container.ClearValue( TableflowViewItemsHost.ShouldDelayDataContextProperty );
      }
    }

    private bool IsRowSelectorPaneVisible()
    {
      DataGridControl parentDataGridControl = this.ParentDataGridControl;

      if( parentDataGridControl != null )
      {
        TableViewScrollViewer scrollViewer = parentDataGridControl.ScrollViewer as TableViewScrollViewer;
        RowSelectorPane rowSelectorPane = ( scrollViewer != null ) ? scrollViewer.RowSelectorPane : null;

        if( rowSelectorPane != null )
          return ( rowSelectorPane.Visibility == Visibility.Visible );
      }

      return false;
    }

    private void LayoutContainers()
    {
      var rowSelectorPaneVisible = this.IsRowSelectorPaneVisible();

      this.LayoutStickyHeaders( rowSelectorPaneVisible );
      this.LayoutStickyFooters( rowSelectorPaneVisible );
      this.LayoutNonStickyContainers( rowSelectorPaneVisible );

      var layoutedContainers = new HashSet<UIElement>();

      foreach( var container in m_layoutedContainers.Select( item => item.Container ) )
      {
        layoutedContainers.Add( container );
      }

      foreach( var container in m_stickyHeaders.Select( item => item.Container ) )
      {
        layoutedContainers.Add( container );
      }

      foreach( var container in m_stickyFooters.Select( item => item.Container ) )
      {
        layoutedContainers.Add( container );
      }

      // Layout out of view the recycled container
      foreach( var container in this.Children )
      {
        if( layoutedContainers.Contains( container ) )
          continue;

        this.ArrangeContainer( container, TableflowViewItemsHost.OutOfViewPoint, false );
      }

      CommandManager.InvalidateRequerySuggested();

      // The call to Mouse.Synchronize must not start dragging rows.
      // Update the mouse status to make sure no container has invalid mouse over status.
      // Only do this when the mouse is over the panel, to prevent unescessary update when scrolling with thumb.
      if( this.IsMouseOver )
      {
        var dataGridControl = this.ParentDataGridControl;
        if( dataGridControl != null )
        {
          using( dataGridControl.InhibitDrag() )
          {
            Mouse.Synchronize();
          }
        }
      }
    }

    private void LayoutNonStickyContainers( bool rowSelectorPaneVisible )
    {
      int count = m_layoutedContainers.Count;
      double clippedHeaderOffset = this.GetStickyHeadersRegionHeight();
      double clippedFooterOffset = this.GetStickyFootersRegionHeight();

      for( int i = 0; i < count; i++ )
      {
        LayoutedContainerInfo layoutedContainerInfo = m_layoutedContainers[ i ];
        int realizedIndex = layoutedContainerInfo.RealizedIndex;
        UIElement container = layoutedContainerInfo.Container;

        double desiredOffset = this.GetContainerOffsetFromIndex( realizedIndex ) - m_verticalOffset;

        if( desiredOffset < clippedHeaderOffset )
        {
          double shownRectHeight = Math.Max( m_containerHeight - ( clippedHeaderOffset - desiredOffset ), 0 );

          Rect shownRect = new Rect(
            new Point( 0, m_containerHeight - shownRectHeight ),
            new Size( this.AnimatedScrollInfo.ExtentWidth, shownRectHeight ) );

          container.SetValue( UIElement.ClipProperty, new RectangleGeometry( shownRect ) );
        }
        else if( desiredOffset + m_containerHeight > clippedFooterOffset )
        {
          double shownRectHeight = Math.Min( clippedFooterOffset - desiredOffset, m_containerHeight );
          shownRectHeight = Math.Max( shownRectHeight, 0 );

          Rect shownRect = new Rect(
            new Point(),
            new Size( this.AnimatedScrollInfo.ExtentWidth, shownRectHeight ) );

          container.SetValue( UIElement.ClipProperty, new RectangleGeometry( shownRect ) );
        }
        else
        {
          container.ClearValue( UIElement.ClipProperty );
        }

        Point translationPoint = new Point( -m_horizontalOffset, desiredOffset );

        this.ArrangeContainer( container, translationPoint, rowSelectorPaneVisible );
      }
    }

    private void LayoutStickyHeaders( bool rowSelectorPaneVisible )
    {
      int count = m_stickyHeaders.Count;
      double lastStickyHeaderOffset = -m_containerHeight;
      double clippedOffset = lastStickyHeaderOffset + m_containerHeight;

      for( int i = 0; i < count; i++ )
      {
        StickyContainerInfo stickyContainerInfo = m_stickyHeaders[ i ];
        UIElement container = stickyContainerInfo.Container;

        double desiredOffset = this.ComputeStickyHeaderDesiredOffset( stickyContainerInfo, lastStickyHeaderOffset );
        lastStickyHeaderOffset = Math.Max( desiredOffset, lastStickyHeaderOffset );

        if( desiredOffset < clippedOffset )
        {
          double shownRectHeight = Math.Max( m_containerHeight - ( clippedOffset - desiredOffset ), 0 );

          Rect shownRect = new Rect(
            new Point( 0, m_containerHeight - shownRectHeight ),
            new Size( this.AnimatedScrollInfo.ExtentWidth, shownRectHeight ) );

          container.SetValue( UIElement.ClipProperty, new RectangleGeometry( shownRect ) );
        }
        else
        {
          container.ClearValue( UIElement.ClipProperty );
        }

        clippedOffset = lastStickyHeaderOffset + m_containerHeight;

        Point translationPoint = new Point(
          -m_horizontalOffset,
          desiredOffset );

        this.ArrangeContainer( container, translationPoint, rowSelectorPaneVisible );
      }
    }

    internal double GetStickyHeadersRegionHeight()
    {
      return this.GetStickyHeadersRegionHeight( m_stickyHeaders );
    }

    internal double GetStickyHeadersRegionHeight( StickyContainerInfoList stickyHeaders )
    {
      int count = stickyHeaders.Count;
      double lastStickyHeaderOffset = -m_containerHeight;

      for( int i = 0; i < count; i++ )
      {
        double desiredOffset = this.ComputeStickyHeaderDesiredOffset( stickyHeaders[ i ], lastStickyHeaderOffset );
        lastStickyHeaderOffset = Math.Max( desiredOffset, lastStickyHeaderOffset );
      }

      return lastStickyHeaderOffset + m_containerHeight;
    }

    private double ComputeStickyHeaderDesiredOffset( StickyContainerInfo stickyHeaderInfo, double lastStickyHeaderOffset )
    {
      double lastHoldingContainerOffset = this.GetContainerOffsetFromIndex( stickyHeaderInfo.HoldingContainerIndex ) - m_verticalOffset;
      double desiredStickyHeaderOffset = lastStickyHeaderOffset + m_containerHeight;
      double realHeaderOffset = this.GetContainerOffsetFromIndex( stickyHeaderInfo.ContainerIndex ) - m_verticalOffset;

      desiredStickyHeaderOffset = Math.Min( lastHoldingContainerOffset, desiredStickyHeaderOffset );
      desiredStickyHeaderOffset = Math.Max( realHeaderOffset, desiredStickyHeaderOffset );

      return desiredStickyHeaderOffset;
    }

    private void LayoutStickyFooters( bool rowSelectorPaneVisible )
    {
      int count = m_stickyFooters.Count;
      double lastStickyFooterOffset = m_viewportHeight;
      double clippedHeaderOffset = this.GetStickyHeadersRegionHeight();
      double clippedOffset = lastStickyFooterOffset;

      // The first one in the list is the last visible one!
      for( int i = 0; i < count; i++ )
      {
        StickyContainerInfo stickyContainerInfo = m_stickyFooters[ i ];
        UIElement container = stickyContainerInfo.Container;

        double desiredOffset = this.ComputeStickyFooterDesiredOffset( stickyContainerInfo, lastStickyFooterOffset );
        lastStickyFooterOffset = Math.Min( desiredOffset, lastStickyFooterOffset );

        if( desiredOffset < clippedHeaderOffset )
        {
          double shownRectHeight = Math.Max( m_containerHeight - ( clippedHeaderOffset - desiredOffset ), 0 );

          Rect shownRect = new Rect(
            new Point( 0, m_containerHeight - shownRectHeight ),
            new Size( this.AnimatedScrollInfo.ExtentWidth, shownRectHeight ) );

          container.SetValue( UIElement.ClipProperty, new RectangleGeometry( shownRect ) );
        }
        else if( desiredOffset + m_containerHeight > clippedOffset )
        {
          double shownRectHeight = Math.Min( clippedOffset - desiredOffset, m_containerHeight );
          shownRectHeight = Math.Max( shownRectHeight, 0 );

          Rect shownRect = new Rect(
            new Point(),
            new Size( this.AnimatedScrollInfo.ExtentWidth, shownRectHeight ) );

          container.SetValue( UIElement.ClipProperty, new RectangleGeometry( shownRect ) );
        }
        else
        {
          container.ClearValue( UIElement.ClipProperty );
        }

        clippedOffset = lastStickyFooterOffset;

        Point translationPoint = new Point( -m_horizontalOffset, desiredOffset );

        this.ArrangeContainer( container, translationPoint, rowSelectorPaneVisible );
      }
    }

    internal double GetStickyFootersRegionHeight()
    {
      return this.GetStickyFootersRegionHeight( m_stickyFooters );
    }

    internal double GetStickyFootersRegionHeight( StickyContainerInfoList stickyFooters )
    {
      int count = stickyFooters.Count;
      double lastStickyFooterOffset = m_viewportHeight;

      for( int i = 0; i < count; i++ )
      {
        double desiredOffset = this.ComputeStickyFooterDesiredOffset( stickyFooters[ i ], lastStickyFooterOffset );
        lastStickyFooterOffset = Math.Min( desiredOffset, lastStickyFooterOffset );
      }

      return lastStickyFooterOffset;
    }

    private double ComputeStickyFooterDesiredOffset( StickyContainerInfo stickyFooterInfo, double lastStickyFooterOffset )
    {
      double firstHoldingContainerOffset = this.GetContainerOffsetFromIndex( stickyFooterInfo.HoldingContainerIndex ) - m_verticalOffset;
      double desiredStickyFooterOffset = lastStickyFooterOffset - m_containerHeight;
      double realFooterOffset = this.GetContainerOffsetFromIndex( stickyFooterInfo.ContainerIndex ) - m_verticalOffset;

      desiredStickyFooterOffset = Math.Max( firstHoldingContainerOffset, desiredStickyFooterOffset );
      desiredStickyFooterOffset = Math.Min( realFooterOffset, desiredStickyFooterOffset );

      return desiredStickyFooterOffset;
    }

    private int GetContainerIndexFromOffset( double offset, bool includePartiallyVisibleContainers )
    {
      if( includePartiallyVisibleContainers )
      {
        return ( int )Math.Ceiling( offset / m_containerHeight );
      }
      else
      {
        return ( int )Math.Floor( offset / m_containerHeight );
      }
    }

    private double GetContainerOffsetFromIndex( int index )
    {
      return ( index * m_containerHeight );
    }

    private void DisableElementNavigation( UIElement child )
    {
      //get previous values and store them on the container (attached)
      TableflowViewItemsHost.SetPreviousDirectionalNavigationMode( child, KeyboardNavigation.GetDirectionalNavigation( child ) );
      TableflowViewItemsHost.SetPreviousTabNavigationMode( child, KeyboardNavigation.GetTabNavigation( child ) );

      KeyboardNavigation.SetDirectionalNavigation( child, KeyboardNavigationMode.None );
      KeyboardNavigation.SetTabNavigation( child, KeyboardNavigationMode.None );
    }

    private void EnableElementNavigation( UIElement child )
    {
      //checking only one of the 2 properties... This is because they are set together.
      if( child.ReadLocalValue( TableflowViewItemsHost.PreviousDirectionalNavigationModeProperty ) != DependencyProperty.UnsetValue )
      {
        KeyboardNavigation.SetDirectionalNavigation( child, TableflowViewItemsHost.GetPreviousDirectionalNavigationMode( child ) );
        KeyboardNavigation.SetTabNavigation( child, TableflowViewItemsHost.GetPreviousTabNavigationMode( child ) );
      }
      //If unset, then nothing to do in this method!
    }

    private void PrepareContainer( UIElement container )
    {
      container.ClearValue( UIElement.VisibilityProperty );

      var dataItemStore = Xceed.Wpf.DataGrid.CustomItemContainerGenerator.GetDataItemProperty( container );
      if( ( dataItemStore == null ) || dataItemStore.IsEmpty )
        return;

      var dataItem = dataItemStore.Data;
      if( dataItem == null )
        return;

      // Prepare the container.
      this.ParentDataGridControl.PrepareItemContainer( container, dataItem );
    }

    private void ClearContainer( UIElement container )
    {
#if DEBUG
      TableflowViewItemsHost.SetRealizedIndex( container, -1 );
#endif //DEBUG
      TableflowViewItemsHost.ClearIsSticky( container );
      container.ClearValue( UIElement.ClipProperty );

      this.DisableElementNavigation( container );
      this.FreeRowSelector( container );

      container.Visibility = Visibility.Collapsed;

      // The call to DataGridControl.ClearItemContainer will be done by the CustomItemContainerGenerator.
    }

    #endregion

    #region Animations Management

    private void InitializeHorizontalOffsetAnimation()
    {
      if( m_horizontalOffsetAnimation != null )
        return;

      m_horizontalOffsetAnimation = new OffsetAnimation();
    }

    private bool StartHorizontalOffsetAnimation()
    {
      var rootDataGridContext = this.CachedRootDataGridContext;
      if( rootDataGridContext == null )
      {
        this.StopHorizontalOffsetAnimation();
        return false;
      }

      var scrollingAnimationDuration = TableflowView.GetScrollingAnimationDuration( rootDataGridContext );
      if( scrollingAnimationDuration <= 0d )
        return false;

      var animatedScrollInfo = this.AnimatedScrollInfo;

      m_horizontalOffsetAnimation.Duration = TimeSpan.FromMilliseconds( scrollingAnimationDuration );
      m_horizontalOffsetAnimation.From = animatedScrollInfo.OriginalHorizontalOffset;
      m_horizontalOffsetAnimation.To = animatedScrollInfo.TargetHorizontalOffset;

      this.StopHorizontalOffsetAnimation();

      m_horizontalOffsetAnimationClock = ( AnimationClock )m_horizontalOffsetAnimation.CreateClock( true );
      m_horizontalOffsetAnimationClock.Completed += this.OnHorizontalOffsetAnimationCompleted;

      this.ApplyAnimationClock( TableflowViewItemsHost.AnimatedHorizontalOffsetProperty, m_horizontalOffsetAnimationClock, HandoffBehavior.SnapshotAndReplace );

      return true;
    }

    private void StopHorizontalOffsetAnimation()
    {
      if( ( m_horizontalOffsetAnimationClock != null ) && ( m_horizontalOffsetAnimationClock.Controller != null ) )
      {
        // We must call Pause instead on Stop to avoid having our
        // offset momentary set to 0. Weird...
        m_horizontalOffsetAnimationClock.Controller.Pause();
        m_horizontalOffsetAnimationClock.Completed -= this.OnHorizontalOffsetAnimationCompleted;
        m_horizontalOffsetAnimationClock = null;
      }
    }

    private void OnHorizontalOffsetAnimationCompleted( object sender, EventArgs e )
    {
      this.AnimatedScrollInfo.HorizontalScrollingDirection = ScrollDirection.None;
    }

    private void InitializeVerticalOffsetAnimation()
    {
      if( m_verticalOffsetAnimation != null )
        return;

      m_verticalOffsetAnimation = new OffsetAnimation();
    }

    private bool StartVerticalOffsetAnimation()
    {
      var rootDataGridContext = this.CachedRootDataGridContext;
      if( rootDataGridContext == null )
      {
        this.StopVerticalOffsetAnimation();
        return false;
      }

      var scrollingAnimationDuration = TableflowView.GetScrollingAnimationDuration( rootDataGridContext );
      if( scrollingAnimationDuration <= 0d )
        return false;

      var animatedScrollInfo = this.AnimatedScrollInfo;

      m_verticalOffsetAnimation.Duration = TimeSpan.FromMilliseconds( scrollingAnimationDuration );
      m_verticalOffsetAnimation.From = animatedScrollInfo.OriginalVerticalOffset;
      m_verticalOffsetAnimation.To = animatedScrollInfo.TargetVerticalOffset;

      this.StopVerticalOffsetAnimation( false );

      m_verticalOffsetAnimationClock = ( AnimationClock )m_verticalOffsetAnimation.CreateClock( true );
      m_verticalOffsetAnimationClock.Completed += this.OnVerticalOffsetAnimationCompleted;

      this.ApplyAnimationClock( TableflowViewItemsHost.AnimatedVerticalOffsetProperty, m_verticalOffsetAnimationClock, HandoffBehavior.SnapshotAndReplace );

      return true;
    }

    private void StopVerticalOffsetAnimation( bool resetScrollDirection = true )
    {
      if( ( m_verticalOffsetAnimationClock != null ) && ( m_verticalOffsetAnimationClock.Controller != null ) )
      {
        if( resetScrollDirection )
        {
          this.AnimatedScrollInfo.VerticalScrollingDirection = ScrollDirection.None;
        }

        // We must call Pause instead on Stop to avoid having our
        // offset momentary set to 0. Weird...
        m_verticalOffsetAnimationClock.Controller.Pause();
        m_verticalOffsetAnimationClock.Completed -= this.OnVerticalOffsetAnimationCompleted;
        m_verticalOffsetAnimationClock = null;

      }
    }

    private void OnVerticalOffsetAnimationCompleted( object sender, EventArgs e )
    {
      this.AnimatedScrollInfo.VerticalScrollingDirection = ScrollDirection.None;
    }

    #endregion

    #region Scrolling Management

    internal void InvalidateScrollInfo()
    {
      ScrollViewer scrollOwner = this.AnimatedScrollInfo.ScrollOwner;

      if( scrollOwner != null )
      {
        scrollOwner.InvalidateScrollInfo();
      }
    }

    private void EnsureVerticalOffsetValid( double height )
    {
      IAnimatedScrollInfo animatedScrollInfo = this.AnimatedScrollInfo;

      double maxOffset;
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( ( !Double.IsInfinity( height ) ) && ( ( dataGridContext == null ) || ( TableView.GetAutoFillLastPage( dataGridContext ) ) ) )
      {
        maxOffset = Math.Max( animatedScrollInfo.ExtentHeight - height, 0 );
      }
      else
      {
        maxOffset = Math.Max( animatedScrollInfo.ExtentHeight - m_containerHeight, 0 );
      }

      double offset = Math.Max( m_verticalOffset, 0 );
      offset = Math.Min( m_verticalOffset, maxOffset );

      if( offset != m_verticalOffset )
      {
        this.SetVerticalOffsetCore( offset );
        this.AnimatedScrollInfo.TargetVerticalOffset = offset;
      }
    }

    private void ScrollByHorizontalOffset( double offset )
    {
      this.ScrollByHorizontalOffset( offset, true );
    }

    private void ScrollByHorizontalOffset( double offset, bool animate )
    {
      this.ScrollToHorizontalOffset( this.AnimatedScrollInfo.TargetHorizontalOffset + offset, animate );
    }

    private void ScrollToHorizontalOffset( double offset )
    {
      this.ScrollToHorizontalOffset( offset, true );
    }

    private void ScrollToHorizontalOffset( double offset, bool animate )
    {
      this.ScrollToHorizontalOffset( offset, animate, null );
    }

    private void ScrollToHorizontalOffset( double offset, bool animate, ScrollDirection? scrollingDirection )
    {
      IAnimatedScrollInfo animatedScrollInfo = this.AnimatedScrollInfo;

      double maxOffset = Math.Max( animatedScrollInfo.ExtentWidth - animatedScrollInfo.ViewportWidth, 0 );
      offset = Math.Max( offset, 0 );
      offset = Math.Min( offset, maxOffset );

      if( animatedScrollInfo.TargetHorizontalOffset == offset )
        return;

      double scrollChange = ( offset - animatedScrollInfo.TargetHorizontalOffset );

      if( scrollingDirection.HasValue )
      {
        animatedScrollInfo.HorizontalScrollingDirection = scrollingDirection.Value;
      }
      else if( scrollChange > 0 )
      {
        animatedScrollInfo.HorizontalScrollingDirection = ScrollDirection.Forward;
      }
      else if( scrollChange == 0 )
      {
        animatedScrollInfo.HorizontalScrollingDirection = ScrollDirection.None;
      }
      else
      {
        animatedScrollInfo.HorizontalScrollingDirection = ScrollDirection.Backward;
      }

      animatedScrollInfo.OriginalHorizontalOffset = animatedScrollInfo.HorizontalOffset;
      animatedScrollInfo.TargetHorizontalOffset = offset;

      bool animationStarted = false;

      if( animate )
      {
        // The animated offset will take care of generating the 
        // page and invalidating the ScrollInfo.
        animationStarted = this.StartHorizontalOffsetAnimation();
      }

      if( !animationStarted )
      {
        if( this.CachedRootDataGridContext != null )
        {
          this.StopHorizontalOffsetAnimation();
          this.SetHorizontalOffsetCore( offset );
          // No need to regenerate the page since only the horizontal offset have changed.
          // We must, on the other hand, relayout the containers to reflect the new offsets.
          this.LayoutContainers();
          this.InvalidateScrollInfo();
        }

        animatedScrollInfo.HorizontalScrollingDirection = ScrollDirection.None;
      }
    }

    private void SetHorizontalOffsetCore( double offset )
    {
      if( m_horizontalOffset == offset )
        return;

      m_horizontalOffset = offset;
    }

    private void ScrollByVerticalOffset( double offset )
    {
      this.ScrollByVerticalOffset( offset, true );
    }

    private void ScrollByVerticalOffset( double offset, bool animate )
    {
      this.ScrollToVerticalOffset( this.AnimatedScrollInfo.TargetVerticalOffset + offset, animate );
    }

    private void ScrollToVerticalOffset( double offset )
    {
      this.ScrollToVerticalOffset( offset, true );
    }

    private void ScrollToVerticalOffset( double offset, bool animate )
    {
      this.ScrollToVerticalOffset( offset, animate, null );
    }

    private void ScrollToVerticalOffset( double offset, bool animate, ScrollDirection? scrollingDirection )
    {
      IAnimatedScrollInfo animatedScrollInfo = this.AnimatedScrollInfo;

      double maxOffset;
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( ( dataGridContext == null ) || ( TableView.GetAutoFillLastPage( dataGridContext ) ) )
      {
        maxOffset = Math.Max( animatedScrollInfo.ExtentHeight - animatedScrollInfo.ViewportHeight, 0 );
      }
      else
      {
        maxOffset = Math.Max( animatedScrollInfo.ExtentHeight - m_containerHeight, 0 );
      }

      offset = Math.Max( offset, 0 );
      offset = Math.Min( offset, maxOffset );

      if( animatedScrollInfo.TargetVerticalOffset == offset )
        return;

      double scrollChange = ( offset - animatedScrollInfo.TargetVerticalOffset );

      if( scrollingDirection.HasValue )
      {
        animatedScrollInfo.VerticalScrollingDirection = scrollingDirection.Value;
      }
      else if( scrollChange > 0 )
      {
        animatedScrollInfo.VerticalScrollingDirection = ScrollDirection.Forward;
      }
      else if( scrollChange == 0 )
      {
        animatedScrollInfo.VerticalScrollingDirection = ScrollDirection.None;
      }
      else
      {
        animatedScrollInfo.VerticalScrollingDirection = ScrollDirection.Backward;
      }

      animatedScrollInfo.OriginalVerticalOffset = animatedScrollInfo.VerticalOffset;
      animatedScrollInfo.TargetVerticalOffset = offset;

      bool animationStarted = false;

      if( animate )
      {
        // The animated offset will take care of generating the 
        // page and invalidating the ScrollInfo.
        animationStarted = this.StartVerticalOffsetAnimation();
      }

      if( !animationStarted )
      {
        if( this.CachedRootDataGridContext != null )
        {
          this.StopVerticalOffsetAnimation( false );
          this.SetVerticalOffsetCore( offset );
          this.InvalidateLayoutFromScrollingHelper();
        }

        animatedScrollInfo.VerticalScrollingDirection = ScrollDirection.None;
        animatedScrollInfo.HorizontalScrollingDirection = ScrollDirection.None;
      }
    }

    private void SetVerticalOffsetCore( double offset )
    {
      if( m_verticalOffset == offset )
        return;

      m_verticalOffset = offset;
    }

    private bool SetCurrent( int index, bool changeColumn )
    {
      bool isDataRow;
      return this.SetCurrent( index, changeColumn, out isDataRow );
    }

    private bool SetCurrent( int index, bool changeColumn, out bool isDataRow )
    {
      var generator = this.CustomItemContainerGenerator;
      var position = generator.GeneratorPositionFromIndex( index );
      var container = default( UIElement );

      using( generator.StartAt( position, GeneratorDirection.Forward, true ) )
      {
        container = this.GenerateContainer( generator, index, false, false );
      }

      isDataRow = false;

      if( container != null )
      {
        isDataRow = ( container is DataRow );

        if( ( m_layoutedContainers.IndexOfContainer( container ) < 0 )
          && ( m_stickyHeaders.IndexOfContainer( container ) < 0 )
          && ( m_stickyFooters.IndexOfContainer( container ) < 0 ) )
        {
          // If the container was not already layouted, force
          // a layout of the current page so that the new container
          // is drawn at the right offset.
          m_layoutedContainers.Add( new LayoutedContainerInfo( index, container ) );
          this.LayoutContainers();
        }

        var dataGridContext = DataGridControl.GetDataGridContext( container );
        var currentContext = dataGridContext.DataGridControl.CurrentContext;
        var column = default( ColumnBase );

        if( changeColumn )
        {
          column = dataGridContext.CurrentColumn;

          if( isDataRow )
          {
            if( ( currentContext != dataGridContext ) && dataGridContext.AreDetailsFlatten )
            {
              column = dataGridContext.GetMatchingColumn( currentContext, currentContext.CurrentColumn ) ?? column;
            }

            if( ( column == null ) || ( !column.CanBeCurrentWhenReadOnly && column.ReadOnly ) )
            {
              var focusableIndex = NavigationHelper.GetFirstVisibleFocusableColumnIndex( dataGridContext );
              if( focusableIndex >= 0 )
              {
                column = dataGridContext.VisibleColumns[ focusableIndex ];
              }
            }
          }
        }

        try
        {
          if( currentContext != null )
          {
            currentContext.EndEdit();
          }
        }
        catch( DataGridException )
        {
          return false;
        }

        var containerIndex = generator.GetRealizedIndexForContainer( container );
        if( containerIndex >= 0 )
        {
          if( dataGridContext.DataGridControl.SetFocusHelper( container, column, true, true ) )
          {
            generator.SetCurrentIndex( containerIndex );
            return true;
          }
        }
      }

      return false;
    }

    #endregion

    #region BringIntoView Methods

    private void OnRequestBringIntoView( object sender, RequestBringIntoViewEventArgs e )
    {
      TableViewItemsHost.ProcessRequestBringIntoView( this,
        Orientation.Vertical,
        0.5, // Default StableScrollingProportion
        e );
    }

    private bool FocusContainerOrPreviousFocusable( UIElement focusedContainer, bool changeColumn )
    {
      PageIndexes layoutedIndexes;
      this.CalculateLayoutedIndexes( m_verticalOffset, this.AnimatedScrollInfo.ViewportHeight, out layoutedIndexes );

      int firstIndex = 0;
      int focusedIndex = this.CustomItemContainerGenerator.GetRealizedIndexForContainer( focusedContainer );
      int desiredIndex = focusedIndex - 1;

      System.Diagnostics.Debug.Assert( focusedIndex > -1, "How come the realized index is -1?!?" );

      bool currentChanged = false;

      if( desiredIndex != focusedIndex )
      {
        DataGridControl dataGridControl = this.ParentDataGridControl;
        int cannotFocusCount = 0;

        // We want to find the first element that can receive focus upward.
        while( ( desiredIndex >= 0 ) && ( !currentChanged ) )
        {
          bool isDataRow;
          currentChanged = this.SetCurrent( desiredIndex, changeColumn, out isDataRow );

          if( !currentChanged )
          {
            if( dataGridControl.HasValidationError )
              return false;

            if( isDataRow )
            {
              cannotFocusCount++;

              if( cannotFocusCount > TableflowViewItemsHost.MaxDataRowFailFocusCount )
                return false;
            }
          }

          // We succeded in changing the current.
          if( currentChanged )
            break;

          // We already are at the first index and still not focused on a new container.
          if( desiredIndex == firstIndex )
          {
            //Let's keep the focus on the last focused container.
            currentChanged = this.SetCurrent( focusedIndex, changeColumn, out isDataRow );
            break;
          }

          desiredIndex--;
        }

        // We will use MoveFocus if the focused index is currently in view.
        if( ( !currentChanged ) && ( focusedIndex >= layoutedIndexes.StartIndex ) && ( focusedIndex <= layoutedIndexes.EndIndex ) )
        {
          currentChanged = this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Up ) );
        }
      }

      return currentChanged;
    }

    private bool FocusIndexOrPreviousFocusable( int desiredIndex, int minimumIndex, bool changeColumn )
    {
      bool currentChanged = false;
      DataGridControl dataGridControl = this.ParentDataGridControl;
      int cannotFocusCount = 0;

      // We want to find the first element that can receive focus upward.
      while( ( desiredIndex >= 0 ) && ( !currentChanged ) )
      {
        bool isDataRow;
        currentChanged = this.SetCurrent( desiredIndex, changeColumn, out isDataRow );

        if( !currentChanged )
        {
          if( dataGridControl.HasValidationError )
            return false;

          if( isDataRow )
          {
            cannotFocusCount++;

            if( cannotFocusCount > TableflowViewItemsHost.MaxDataRowFailFocusCount )
              return false;
          }
        }

        // We succeded in changing the current or we're already at the first index? Then nothing we can do.
        if( ( currentChanged ) || ( desiredIndex == minimumIndex ) )
          break;

        desiredIndex--;
      }

      return currentChanged;
    }

    private bool FocusContainerOrNextFocusable( UIElement focusedContainer, bool changeColumn )
    {
      PageIndexes layoutedIndexes;
      this.CalculateLayoutedIndexes( m_verticalOffset, this.AnimatedScrollInfo.ViewportHeight, out layoutedIndexes );

      int lastIndex = this.CustomItemContainerGenerator.ItemCount - 1;
      int focusedIndex = this.CustomItemContainerGenerator.GetRealizedIndexForContainer( focusedContainer );
      int desiredIndex = focusedIndex + 1;

      System.Diagnostics.Debug.Assert( focusedIndex > -1, "How come the realized index is -1?!?" );

      bool currentChanged = false;

      if( desiredIndex != focusedIndex )
      {
        DataGridControl dataGridControl = this.ParentDataGridControl;
        int cannotFocusCount = 0;

        // We want to find the first element that can receive focus downward.
        while( ( desiredIndex <= lastIndex ) && ( !currentChanged ) )
        {
          bool isDataRow;
          currentChanged = this.SetCurrent( desiredIndex, changeColumn, out isDataRow );

          if( !currentChanged )
          {
            if( dataGridControl.HasValidationError )
              return false;

            if( isDataRow )
            {
              cannotFocusCount++;

              if( cannotFocusCount > TableflowViewItemsHost.MaxDataRowFailFocusCount )
                return false;
            }
          }

          // We succeded in changing the current.
          if( currentChanged )
            break;

          // We already are at the last index and still not focused on a new container.
          if( desiredIndex == lastIndex )
          {
            //Let's keep the focus on the last focused container.
            currentChanged = this.SetCurrent( focusedIndex, changeColumn, out isDataRow );
          }

          desiredIndex++;
        }

        // We will use MoveFocus if the focused index is currently in view.
        if( ( !currentChanged ) && ( focusedIndex >= layoutedIndexes.StartIndex ) && ( focusedIndex <= layoutedIndexes.EndIndex ) )
        {
          currentChanged = this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Down ) );
        }
      }

      return currentChanged;
    }

    private bool FocusIndexOrNextFocusable( int desiredIndex, int maximumIndex, bool changeColumn )
    {
      bool currentChanged = false;
      DataGridControl dataGridControl = this.ParentDataGridControl;
      int cannotFocusCount = 0;

      // We want to find the first element that can receive focus downward.
      while( ( desiredIndex <= maximumIndex ) && ( !currentChanged ) )
      {
        bool isDataRow;
        currentChanged = this.SetCurrent( desiredIndex, changeColumn, out isDataRow );

        if( !currentChanged )
        {
          if( dataGridControl.HasValidationError )
            return false;

          if( isDataRow )
          {
            cannotFocusCount++;

            if( cannotFocusCount > TableflowViewItemsHost.MaxDataRowFailFocusCount )
              return false;
          }
        }

        // We succeded in changing the current or we're already at the first index? Then nothing we can do.
        if( ( currentChanged ) || ( desiredIndex == maximumIndex ) )
          break;

        desiredIndex++;
      }

      return currentChanged;
    }

    #endregion

    #region DataGridItemsHost Overrides

    protected override IList<UIElement> CreateChildCollection()
    {
      return new TableflowViewUIElementCollection( this );
    }

    protected override void OnItemsAdded()
    {
      this.PreventOpacityAnimation = true;

      // We are calling InvalidateMeasure to regenerate the current 
      // page and forcing the recycling of out-of-view containers.
      this.InvalidateMeasure();
    }

    protected override void OnItemsRemoved( IList<DependencyObject> containers )
    {
      this.PreventOpacityAnimation = true;

      // We are calling InvalidateMeasure to regenerate the current 
      // page and forcing the recycling of out-of-view containers.
      this.InvalidateMeasure();
    }

    protected override void OnItemsReset()
    {
      // Everything will be recalculated and redrawn in the measure pass.
      this.InvalidateMeasure();
    }

    protected override void OnContainersRemoved( IList<DependencyObject> removedContainers )
    {
      foreach( UIElement element in removedContainers )
      {
        this.RecycleContainer( element );

        // Avoid checking if Children already contains
        // the element. No exception will be thrown
        // if the item is not found. This ensure to
        // at parse all the children only 1 time as
        // worst scenario
        this.Children.Remove( element );


        int index = m_layoutedContainers.IndexOfContainer( element );
        if( index > -1 )
        {
          m_layoutedContainers.RemoveAt( index );
        }

        index = m_stickyHeaders.IndexOfContainer( element );
        if( index > -1 )
        {
          m_stickyHeaders.RemoveAt( index );
        }

        index = m_stickyFooters.IndexOfContainer( element );
        if( index > -1 )
        {
          m_stickyFooters.RemoveAt( index );
        }
      }
    }

    protected override void OnParentDataGridControlChanged( DataGridControl oldValue, DataGridControl newValue )
    {
      // Ensure to stop any animation since this ItemsHost
      // is no more hosted in a DataGridControl
      if( newValue == null )
      {
        // Stop the AnimationClock for each offset property
        this.StopVerticalOffsetAnimation();
        this.StopHorizontalOffsetAnimation();

        // Ensure to clear any animation on offset property directly
        this.ApplyAnimationClock( TableflowViewItemsHost.AnimatedVerticalOffsetProperty, null, HandoffBehavior.SnapshotAndReplace );
        this.ApplyAnimationClock( TableflowViewItemsHost.AnimatedHorizontalOffsetProperty, null, HandoffBehavior.SnapshotAndReplace );
      }

      base.OnParentDataGridControlChanged( oldValue, newValue );
    }

    protected override void OnRecyclingCandidatesCleaned( IList<DependencyObject> recyclingCandidates )
    {
      foreach( UIElement candidate in recyclingCandidates )
      {
        this.ClearContainer( candidate );
      }
    }

    #endregion

    #region PreviewKeyDown and KeyDown Handling

    protected override void HandleTabKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      var dataGridControl = this.ParentDataGridControl;
      if( dataGridControl == null )
        return;

      var dataGridContext = dataGridControl.CurrentContext;
      if( dataGridContext == null )
        return;

      e.Handled = NavigationHelper.HandleTabKey( dataGridControl, dataGridContext, e.OriginalSource as FrameworkElement, e.KeyboardDevice );
    }

    protected override void HandleLeftKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      var dataGridControl = this.ParentDataGridControl;
      if( dataGridControl == null )
        return;

      e.Handled = NavigationHelper.MoveFocusLeft( dataGridControl.CurrentContext );
    }

    protected override void HandleRightKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      var dataGridControl = this.ParentDataGridControl;
      if( dataGridControl == null )
        return;

      e.Handled = NavigationHelper.MoveFocusRight( dataGridControl.CurrentContext );
    }

    protected override void HandleUpKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      e.Handled = this.MoveFocusUp();
    }

    protected override void HandleDownKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      e.Handled = this.MoveFocusDown();
    }

    protected override void HandlePageUpKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      e.Handled = true;

      DataGridControl dataGridControl = this.ParentDataGridControl;
      NavigationBehavior navigationBehavior = dataGridControl.NavigationBehavior;
      bool changeCurrentColumn = ( navigationBehavior == NavigationBehavior.CellOnly );

      if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
      {
        this.MoveToFirstOverallUIElement( changeCurrentColumn );
        return;
      }

      UIElement focusedContainer = DataGridItemsHost.GetItemsHostContainerFromElement( this, Keyboard.FocusedElement as DependencyObject );

      if( ( focusedContainer == null ) || ( navigationBehavior == NavigationBehavior.None ) )
      {
        // We just need to scroll one page up.
        this.AnimatedScrollInfo.ScrollOwner.PageUp();
        return;
      }

      int focusedContainerRealizedIndex = this.CustomItemContainerGenerator.GetRealizedIndexForContainer( focusedContainer );
      int desiredPageUpIndex = Math.Max( 0, focusedContainerRealizedIndex - m_pageVisibleContainerCount + 1 );
      int initialDesiredIndex = desiredPageUpIndex;

      if( focusedContainerRealizedIndex == 0 )
      {
        this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Up ) );
      }
      else if( focusedContainerRealizedIndex != initialDesiredIndex )
      {
        PageIndexes pageIndexes = this.CalculateVisibleIndexesForDesiredLastVisibleIndex( focusedContainerRealizedIndex );
        desiredPageUpIndex = Math.Max( 0, pageIndexes.StartIndex );
        initialDesiredIndex = desiredPageUpIndex;
        bool isDataRow = false;

        // SetCurrent on the desired index or down until BEFORE the focused container.
        while( ( !dataGridControl.HasValidationError ) && ( !isDataRow ) && ( desiredPageUpIndex < focusedContainerRealizedIndex ) )
        {
          if( this.SetCurrent( desiredPageUpIndex, changeCurrentColumn, out isDataRow ) )
            return;

          desiredPageUpIndex++;
        }

        if( ( dataGridControl.HasValidationError ) || ( isDataRow ) )
          return;

        // No container were focused while processing indexes from focused to initialDesiredIndex, try SetCurrent on indexes lower than the initial up to 0
        desiredPageUpIndex = initialDesiredIndex - 1;
        isDataRow = false;

        while( ( !dataGridControl.HasValidationError ) && ( !isDataRow ) && ( desiredPageUpIndex > 0 ) )
        {
          if( this.SetCurrent( desiredPageUpIndex, changeCurrentColumn, out isDataRow ) )
            return;

          desiredPageUpIndex--;
        }

        // If not container was successfuly focused, set focus back to the current UIElement.
        this.SetCurrent( focusedContainerRealizedIndex, changeCurrentColumn, out isDataRow );
      }
    }

    protected override void HandlePageDownKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      e.Handled = true;

      DataGridControl dataGridControl = this.ParentDataGridControl;
      NavigationBehavior navigationBehavior = dataGridControl.NavigationBehavior;
      bool changeCurrentColumn = ( navigationBehavior == NavigationBehavior.CellOnly );

      if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
      {
        this.MoveToLastOverallUIElement( changeCurrentColumn );
        return;
      }

      UIElement focusedContainer = DataGridItemsHost.GetItemsHostContainerFromElement( this, Keyboard.FocusedElement as DependencyObject );

      // No focused container or no navigation allowed
      if( ( focusedContainer == null ) || ( navigationBehavior == NavigationBehavior.None ) )
      {
        // We just need to scroll one page down.
        this.AnimatedScrollInfo.ScrollOwner.PageDown();
        return;
      }

      int generatorItemCount = this.CustomItemContainerGenerator.ItemCount;
      int focusedContainerRealizedIndex = this.CustomItemContainerGenerator.GetRealizedIndexForContainer( focusedContainer );
      int maxIndex = generatorItemCount - 1;
      int desiredPageDownIndex = Math.Min( generatorItemCount - 1, focusedContainerRealizedIndex + m_pageVisibleContainerCount - 1 );
      int initialDesiredIndex = desiredPageDownIndex;

      if( focusedContainerRealizedIndex == maxIndex )
      {
        this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Down ) );
      }
      else if( focusedContainerRealizedIndex != initialDesiredIndex )
      {
        PageIndexes pageIndexes = this.CalculateVisibleIndexesForDesiredFirstVisibleIndex( focusedContainerRealizedIndex );
        desiredPageDownIndex = Math.Min( maxIndex, pageIndexes.EndIndex );

        initialDesiredIndex = desiredPageDownIndex;

        bool isDataRow = false;

        // SetCurrent on the desired index or up until BEFORE the focused container.
        while( ( !dataGridControl.HasValidationError ) && ( !isDataRow ) && ( desiredPageDownIndex > focusedContainerRealizedIndex ) )
        {
          if( this.SetCurrent( desiredPageDownIndex, changeCurrentColumn, out isDataRow ) )
            return;

          desiredPageDownIndex--;
        }

        if( ( dataGridControl.HasValidationError ) || ( isDataRow ) )
          return;

        desiredPageDownIndex = initialDesiredIndex + 1;
        isDataRow = false;

        // No container were focused while processing indexes from focused to initialDesiredIndex, try SetCurrent on indexes higher than the initial down to maxIndex
        while( ( !dataGridControl.HasValidationError ) && ( !isDataRow ) && ( desiredPageDownIndex <= maxIndex ) )
        {
          if( this.SetCurrent( desiredPageDownIndex, changeCurrentColumn, out isDataRow ) )
            return;

          desiredPageDownIndex++;
        }

        // If not container was successfuly focused, set focus back to the current UIElement.
        this.SetCurrent( focusedContainerRealizedIndex, changeCurrentColumn, out isDataRow );
      }
    }

    protected override void HandleHomeKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
      {
        DataGridControl dataGridControl = this.ParentDataGridControl;
        IAnimatedScrollInfo scrollInfo = this.AnimatedScrollInfo;
        NavigationBehavior navigationBehavior = dataGridControl.NavigationBehavior;
        bool changeCurrentColumn = ( navigationBehavior == NavigationBehavior.CellOnly );

        if( navigationBehavior != NavigationBehavior.None )
        {
          this.MoveToFirstOverallUIElement( changeCurrentColumn );

          if( navigationBehavior == NavigationBehavior.CellOnly )
          {
            this.MoveToFirstVisibleColumn();
          }
        }
        else
        {
          scrollInfo.ScrollOwner.ScrollToTop();
        }

        // In all cases, we must scroll to the left end of the panel.
        scrollInfo.ScrollOwner.ScrollToLeftEnd();
      }
      else
      {
        // This should be handled by a parent.
        return;
      }

      e.Handled = true;
    }

    protected override void HandleEndKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
      {
        DataGridControl dataGridControl = this.ParentDataGridControl;
        IAnimatedScrollInfo scrollInfo = this.AnimatedScrollInfo;
        NavigationBehavior navigationBehavior = dataGridControl.NavigationBehavior;
        bool changeCurrentColumn = ( navigationBehavior == NavigationBehavior.CellOnly );

        if( navigationBehavior != NavigationBehavior.None )
        {
          this.MoveToLastOverallUIElement( changeCurrentColumn );

          if( navigationBehavior == NavigationBehavior.CellOnly )
          {
            this.MoveToLastVisibleColumn();
          }
        }
        else
        {
          scrollInfo.ScrollOwner.ScrollToBottom();
        }

        // In all cases, we must scroll to the right end of the panel.
        scrollInfo.ScrollOwner.ScrollToRightEnd();
      }
      else
      {
        // This should be handled by a parent.
        return;
      }

      e.Handled = true;
    }

    private void MoveToFirstOverallUIElement( bool changeCurrentColumn )
    {
      // Simply scroll to top.
      this.AnimatedScrollInfo.ScrollOwner.ScrollToTop();

      //Try to focus on first UIElement.
      if( !this.SetCurrent( 0, changeCurrentColumn ) )
      {
        //If not focusable, find the next one downward.
        CustomItemContainerGenerator generator = this.CustomItemContainerGenerator as CustomItemContainerGenerator;
        this.FocusIndexOrNextFocusable( 1, generator.RealizedContainers.Count - 1, changeCurrentColumn );
      }
    }

    private void MoveToLastOverallUIElement( bool changeCurrentColumn )
    {
      // Simply scroll to end
      this.AnimatedScrollInfo.ScrollOwner.ScrollToBottom();

      CustomItemContainerGenerator generator = this.CustomItemContainerGenerator as CustomItemContainerGenerator;
      int lastItemIndex = generator.ItemCount - 1;

      //Try to focus on last UIElement.
      if( !this.SetCurrent( lastItemIndex, changeCurrentColumn ) )
      {
        //If not focusable, find the next one upward.
        int minimumIndex = lastItemIndex - generator.RealizedContainers.Count;
        this.FocusIndexOrPreviousFocusable( lastItemIndex, minimumIndex, changeCurrentColumn );
      }
    }

    private void MoveToFirstVisibleColumn()
    {
      DataGridControl dataGridControl = this.ParentDataGridControl;

      if( dataGridControl == null )
        return;

      DataGridContext currentDataGridContext = dataGridControl.CurrentContext;
      ReadOnlyColumnCollection visibleColumnsCollection = ( ReadOnlyColumnCollection )currentDataGridContext.VisibleColumns;

      int visibleColumnsCollectionCount = visibleColumnsCollection.Count;

      // The previous SetCurrent will only select the first row. The CurrentColumn
      // is left unchanged. We must set the current column to the first one.

      if( ( currentDataGridContext != null ) && ( visibleColumnsCollectionCount > 0 ) )
      {
        int firstVisibleFocusableColumnIndex;
        bool focusableColumnFound = false;

        Row currentRow = currentDataGridContext.CurrentRow;

        if( currentRow != null )
        {
          for( firstVisibleFocusableColumnIndex = 0; firstVisibleFocusableColumnIndex < visibleColumnsCollectionCount; firstVisibleFocusableColumnIndex++ )
          {
            if( currentRow.Cells[ visibleColumnsCollection[ firstVisibleFocusableColumnIndex ] ].GetCalculatedCanBeCurrent() )
            {
              focusableColumnFound = true;
              break;
            }
          }

          if( focusableColumnFound )
          {
            try
            {
              currentDataGridContext.SetCurrentColumnAndChangeSelection(
                currentDataGridContext.VisibleColumns[ firstVisibleFocusableColumnIndex ] );
            }
            catch( DataGridException )
            {
              // We swallow the exception if it occurs because of a validation error or Cell was read-only or
              // any other GridException.
            }
          }
        }
      }
    }

    private void MoveToLastVisibleColumn()
    {
      DataGridControl dataGridControl = this.ParentDataGridControl;

      if( dataGridControl == null )
        return;

      DataGridContext currentDataGridContext = dataGridControl.CurrentContext;
      ReadOnlyColumnCollection visibleColumnsCollection = ( ReadOnlyColumnCollection )currentDataGridContext.VisibleColumns;

      int visibleColumnsCollectionCount = visibleColumnsCollection.Count;

      // The previous SetCurrent will only select the first row. The CurrentColumn
      // is left unchanged. We must set the current column to the last one.

      if( ( visibleColumnsCollection != null ) && ( visibleColumnsCollectionCount > 0 ) )
      {
        bool focusableColumnFound = false;
        int lastVisibleFocusableColumnIndex;

        Row currentRow = currentDataGridContext.CurrentRow;

        if( currentRow != null )
        {
          for( lastVisibleFocusableColumnIndex = visibleColumnsCollectionCount - 1; lastVisibleFocusableColumnIndex >= 0; lastVisibleFocusableColumnIndex-- )
          {
            if( currentRow.Cells[ visibleColumnsCollection[ lastVisibleFocusableColumnIndex ] ].GetCalculatedCanBeCurrent() )
            {
              focusableColumnFound = true;
              break;
            }
          }

          if( focusableColumnFound )
          {
            try
            {
              currentDataGridContext.SetCurrentColumnAndChangeSelection(
                currentDataGridContext.VisibleColumns[ lastVisibleFocusableColumnIndex ] );
            }
            catch( DataGridException )
            {
              // We swallow the exception if it occurs because of a validation error or Cell was read-only or
              // any other GridException.
            }
          }
        }
      }
    }

    private void ResetHorizontalOffset()
    {
      ScrollViewer parentScrollViewer = this.m_scrollOwner;

      if( parentScrollViewer != null )
        parentScrollViewer.ScrollToHorizontalOffset( 0d );
    }

    #endregion

    #region IAnimatedScrollInfo Members

    ScrollDirection IAnimatedScrollInfo.HorizontalScrollingDirection
    {
      get;
      set;
    }

    double IAnimatedScrollInfo.OriginalHorizontalOffset
    {
      get;
      set;
    }

    double IAnimatedScrollInfo.TargetHorizontalOffset
    {
      get;
      set;
    }

    ScrollDirection IAnimatedScrollInfo.VerticalScrollingDirection
    {
      get;
      set;
    }

    double IAnimatedScrollInfo.OriginalVerticalOffset
    {
      get;
      set;
    }

    double IAnimatedScrollInfo.TargetVerticalOffset
    {
      get;
      set;
    }

    #endregion

    #region ICustomVirtualizingPanel Members

    protected override void BringIntoViewCore( int index )
    {
      m_delayedBringIntoViewIndex = index;
      this.InvalidateMeasure();
    }

    private void DelayedBringIntoView()
    {
      if( m_delayedBringIntoViewIndex == -1 )
        return;

      double itemOffset = ( m_delayedBringIntoViewIndex * m_containerHeight );

      bool areHeadersSticky = this.AreHeadersStickyCache || this.AreParentRowsStickyCache || this.AreGroupHeadersStickyCache;
      int stickyHeadersCount = ( areHeadersSticky ) ? this.GetStickyHeaderCountForIndex( m_delayedBringIntoViewIndex ) : 0;

      double stickyHeadersHeight = stickyHeadersCount * m_containerHeight;
      double topThreshold = m_verticalOffset + stickyHeadersHeight;

      if( itemOffset < topThreshold )
      {
        this.ScrollToVerticalOffset( itemOffset - stickyHeadersHeight );
        return;
      }

      bool areFootersSticky = this.AreFootersStickyCache || this.AreGroupFootersStickyCache;
      int stickyFootersCount = ( areFootersSticky ) ? this.GetStickyFooterCountForIndex( m_delayedBringIntoViewIndex ) : 0;

      double stickyFootersHeight = stickyFootersCount * m_containerHeight;
      double bottomThreshold = ( m_verticalOffset + m_viewportHeight ) - stickyHeadersHeight - stickyFootersHeight;

      if( itemOffset + m_containerHeight > bottomThreshold )
      {
        this.ScrollToVerticalOffset( itemOffset - ( m_viewportHeight - stickyFootersHeight - m_containerHeight ) );
      }

    }

    #endregion

    #region IScrollInfo Members

    bool IScrollInfo.CanHorizontallyScroll
    {
      get;
      set;
    }

    bool IScrollInfo.CanVerticallyScroll
    {
      get;
      set;
    }

    double IScrollInfo.ExtentHeight
    {
      get
      {
        return ( this.CachedRootDataGridContext != null ) ? ( this.CustomItemContainerGenerator.ItemCount * m_containerHeight ) : 0;
      }
    }

    private double m_extentWidth;
    double IScrollInfo.ExtentWidth
    {
      get
      {
        return m_extentWidth;
      }
    }

    private double m_horizontalOffset;
    double IScrollInfo.HorizontalOffset
    {
      get
      {
        return m_horizontalOffset;
      }
    }

    void IScrollInfo.SetHorizontalOffset( double offset )
    {
      if( offset == m_horizontalOffset )
        return;

      DataGridScrollViewer scrollViewer = this.AnimatedScrollInfo.ScrollOwner as DataGridScrollViewer;

      // The ScrollToHorizontalOffset method is called by the scroll viewer when the thumb is dragged and when
      // the user right-click and choose "Scroll to here", "Scroll to bottom" or "Scroll to top".
      // When this method is called, we only want to animate if the called wasn't made by dragging the thumb.
      // We also validate if our parent ScrollViewer is a DataGridScrollViewer because for now, we cannot
      // have a reference to the Thumb to see is the mouse is captured.
      bool animate =
        ( ( scrollViewer != null )
        && ( scrollViewer.HorizontalScrollBar != null )
        && ( scrollViewer.HorizontalScrollBar.Track != null )
        && ( scrollViewer.HorizontalScrollBar.Track.Thumb != null )
        && ( !scrollViewer.HorizontalScrollBar.Track.Thumb.IsMouseCaptureWithin ) );

      this.ScrollToHorizontalOffset( offset, animate );
    }

    private ScrollViewer m_scrollOwner;
    ScrollViewer IScrollInfo.ScrollOwner
    {
      get
      {
        return m_scrollOwner;
      }
      set
      {



        m_scrollOwner = value;


      }
    }

    private double m_verticalOffset;
    double IScrollInfo.VerticalOffset
    {
      get
      {
        return m_verticalOffset;
      }
    }

    void IScrollInfo.SetVerticalOffset( double offset )
    {
      if( offset == m_verticalOffset )
        return;

      DataGridScrollViewer scrollViewer = this.AnimatedScrollInfo.ScrollOwner as DataGridScrollViewer;

      // The SetVerticalOffset method is called by the scroll viewer when the thumb is dragged and when
      // the user right-click and choose "Scroll to here", "Scroll to bottom" or "Scroll to top".
      // When this method is called, we only want to animate if the called wasn't made by dragging the thumb.
      // We also validate if our parent ScrollViewer is a DataGridScrollViewer because for now, we cannot
      // have a reference to the Thumb to see is the mouse is captured.
      bool animate =
        ( ( scrollViewer != null )
        && ( scrollViewer.VerticalScrollBar != null )
        && ( scrollViewer.VerticalScrollBar.Track != null )
        && ( scrollViewer.VerticalScrollBar.Track.Thumb != null )
        && ( !scrollViewer.VerticalScrollBar.Track.Thumb.IsMouseCaptureWithin ) );

      this.ScrollToVerticalOffset( offset, animate );
    }

    private double m_viewportHeight;
    double IScrollInfo.ViewportHeight
    {
      get
      {
        return m_viewportHeight;
      }
    }

    private double m_viewportWidth;
    double IScrollInfo.ViewportWidth
    {
      get
      {
        return m_viewportWidth;
      }
    }

    void IScrollInfo.LineDown()
    {
      this.ScrollByVerticalOffset( m_containerHeight );
    }

    void IScrollInfo.LineLeft()
    {
      this.ScrollByHorizontalOffset( ScrollViewerHelper.PixelScrollingCount * -1 );
    }

    void IScrollInfo.LineRight()
    {
      this.ScrollByHorizontalOffset( ScrollViewerHelper.PixelScrollingCount );
    }

    void IScrollInfo.LineUp()
    {
      this.ScrollByVerticalOffset( m_containerHeight * -1 );
    }

    void IScrollInfo.MouseWheelDown()
    {
      this.ScrollByVerticalOffset( SystemParameters.WheelScrollLines * m_containerHeight );
    }

    void IScrollInfo.MouseWheelLeft()
    {
      this.ScrollByHorizontalOffset( SystemParameters.WheelScrollLines * ScrollViewerHelper.PixelScrollingCount * -1 );
    }

    void IScrollInfo.MouseWheelRight()
    {
      this.ScrollByHorizontalOffset( SystemParameters.WheelScrollLines * ScrollViewerHelper.PixelScrollingCount );
    }

    void IScrollInfo.MouseWheelUp()
    {
      this.ScrollByVerticalOffset( SystemParameters.WheelScrollLines * m_containerHeight * -1 );
    }

    void IScrollInfo.PageDown()
    {
      this.ScrollByVerticalOffset( this.AnimatedScrollInfo.ViewportHeight );
    }

    void IScrollInfo.PageLeft()
    {
      this.ScrollByHorizontalOffset( this.AnimatedScrollInfo.ViewportWidth * -1 );
    }

    void IScrollInfo.PageRight()
    {
      this.ScrollByHorizontalOffset( this.AnimatedScrollInfo.ViewportWidth );
    }

    void IScrollInfo.PageUp()
    {
      this.ScrollByVerticalOffset( this.AnimatedScrollInfo.ViewportHeight * -1 );
    }

    Rect IScrollInfo.MakeVisible( Visual visual, Rect rectangle )
    {
      UIElement container = DataGridItemsHost.GetItemsHostContainerFromElement( this, visual );

      if( container != null )
      {
        rectangle = visual.TransformToAncestor( container ).TransformBounds( rectangle );

        // Make sure that the item is vertically visible.
        ICustomItemContainerGenerator generator = this.CustomItemContainerGenerator;

        if( generator != null )
        {
          int itemIndex = generator.GetRealizedIndexForContainer( container );

          if( itemIndex != -1 )
          {
            // If the container is already layouted and no Clip applied, nothing to make visible
            if( !m_layoutedContainers.ContainsRealizedIndex( itemIndex )
                || ( container.Clip != null ) )
            {
              // This will make sure to scroll to the right offsets.
              ICustomVirtualizingPanel virtualizingPanel = ( ICustomVirtualizingPanel )this;
              virtualizingPanel.BringIntoView( itemIndex );
            }
          }
        }
      }

      // Make sure that the item is horizontally visible.
      IAnimatedScrollInfo animatedScrollInfo = this.AnimatedScrollInfo;

      if( rectangle.Left < animatedScrollInfo.HorizontalOffset )
      {
        this.ScrollToHorizontalOffset( rectangle.Left );
      }
      else if( rectangle.Right > animatedScrollInfo.HorizontalOffset + animatedScrollInfo.ViewportWidth )
      {
        this.ScrollToHorizontalOffset(
          Math.Min( rectangle.Left, ( rectangle.Right - animatedScrollInfo.ViewportWidth ) ) );
      }

      return rectangle;
    }

    #endregion

    #region IDeferableScrollInfoRefresh Members

    IDisposable IDeferableScrollInfoRefresh.DeferScrollInfoRefresh( Orientation orientation )
    {
      if( ( orientation == Orientation.Vertical ) && !this.IsDeferredLoadingEnabledCache )
        return new LayoutSuspendedHelper( this );

      return null;
    }

    #endregion

    private static void MoveFocusForwardExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      var itemsHost = ( TableflowViewItemsHost )sender;
      var dataGridControl = itemsHost.ParentDataGridControl;
      if( dataGridControl == null )
        return;

      e.Handled = NavigationHelper.MoveFocusRight( dataGridControl.CurrentContext );
    }

    private static void MoveFocusForwardCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = true;
      e.Handled = true;
    }

    private static void MoveFocusBackExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      var itemsHost = ( TableflowViewItemsHost )sender;
      var dataGridControl = itemsHost.ParentDataGridControl;
      if( dataGridControl == null )
        return;

      e.Handled = NavigationHelper.MoveFocusLeft( dataGridControl.CurrentContext );
    }

    private static void MoveFocusBackCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = true;
      e.Handled = true;
    }

    private static void MoveFocusUpExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      e.Handled = ( ( TableflowViewItemsHost )sender ).MoveFocusUp();
    }

    private static void MoveFocusUpCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = true;
      e.Handled = true;
    }

    private static void MoveFocusDownExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      e.Handled = ( ( TableflowViewItemsHost )sender ).MoveFocusDown();
    }

    private static void MoveFocusDownCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = true;
      e.Handled = true;
    }

    private bool MoveFocusUp()
    {
      DataGridControl dataGridControl = this.ParentDataGridControl;
      UIElement focusedContainer = DataGridItemsHost.GetItemsHostContainerFromElement( this, Keyboard.FocusedElement as DependencyObject );

      NavigationBehavior navigationBehavior = dataGridControl.NavigationBehavior;
      bool changeCurrentColumn = ( navigationBehavior == NavigationBehavior.CellOnly );

      if( ( focusedContainer == null ) ||
        ( navigationBehavior == NavigationBehavior.None ) )
      {
        // We just need to scroll one line up.
        this.AnimatedScrollInfo.ScrollOwner.LineUp();
      }
      else
      {
        this.FocusContainerOrPreviousFocusable( focusedContainer, changeCurrentColumn );
      }

      return true;
    }

    private bool MoveFocusDown()
    {
      DataGridControl dataGridControl = this.ParentDataGridControl;
      UIElement focusedContainer = DataGridItemsHost.GetItemsHostContainerFromElement( this, Keyboard.FocusedElement as DependencyObject );

      NavigationBehavior navigationBehavior = dataGridControl.NavigationBehavior;
      bool changeCurrentColumn = ( navigationBehavior == NavigationBehavior.CellOnly );

      if( ( focusedContainer == null ) ||
        ( navigationBehavior == NavigationBehavior.None ) )
      {
        // We just need to scroll one line down.
        this.AnimatedScrollInfo.ScrollOwner.LineDown();
      }
      else
      {
        this.FocusContainerOrNextFocusable( focusedContainer, changeCurrentColumn );
      }

      return true;
    }

    private void InvalidateLayoutFromScrollingHelper()
    {
      if( m_layoutSuspended.IsSet )
      {
        m_layoutInvalidatedDuringSuspend = true;
        return;
      }

      this.GeneratePage( false, this.AnimatedScrollInfo.ViewportHeight );
      this.InvalidateArrange();
      this.InvalidateScrollInfo();
    }

    private const int MaxDataRowFailFocusCount = 50;
    private static readonly Point OutOfViewPoint = new Point( -999999, -999999 );
    private static readonly Point EmptyPoint = new Point( 0, 0 );

    private readonly LayoutedContainerInfoList m_layoutedContainers = new LayoutedContainerInfoList();
    private readonly StickyContainerInfoList m_stickyHeaders = new StickyContainerInfoList();
    private readonly StickyContainerInfoList m_stickyFooters = new StickyContainerInfoList();

    private int m_pageVisibleContainerCount;

    private OffsetAnimation m_horizontalOffsetAnimation;
    private AnimationClock m_horizontalOffsetAnimationClock;

    private OffsetAnimation m_verticalOffsetAnimation;
    private AnimationClock m_verticalOffsetAnimationClock;

    private PageIndexes m_lastVisiblePageIndexes = PageIndexes.Empty;

    private readonly Dictionary<string, double> m_cachedContainerDesiredWidth = new Dictionary<string, double>();
    private readonly Dictionary<string, double> m_cachedContainerRealDesiredWidth = new Dictionary<string, double>();

    private readonly List<string> m_autoWidthCalculatedDataGridContextList = new List<string>();

    private int m_delayedBringIntoViewIndex = -1;

    private Size m_lastMeasureAvailableSize = Size.Empty;
    private Size m_lastArrangeFinalSize = Size.Empty;

    private double m_containerHeight;

    private BitVector32 m_flags = new BitVector32();

    private AutoResetFlag m_layoutSuspended = AutoResetFlagFactory.Create( false );
    private bool m_layoutInvalidatedDuringSuspend;

    #region TableflowItemsHostFlags Private Enum

    [Flags]
    private enum TableflowItemsHostFlags
    {
      AreHeadersSticky = 1,
      AreFootersSticky = 2,
      AreGroupHeadersSticky = 4,
      AreGroupFootersSticky = 8,
      AreParentRowSticky = 16,
      PreventOpacityAnimation = 32,
      IsDeferredLoadingEnabled = 64,
    }

    #endregion

    #region PageIndexes Private Class

    private class PageIndexes
    {
      public PageIndexes()
      {
      }

      public PageIndexes( int startIndex, int endIndex )
      {
        this.StartIndex = startIndex;
        this.EndIndex = endIndex;
      }

      public int StartIndex
      {
        get;
        set;
      }
      public int EndIndex
      {
        get;
        set;
      }

      private static PageIndexes _empty;
      public static PageIndexes Empty
      {
        get
        {
          if( _empty == null )
            _empty = new PageIndexes( -1, -1 );

          return _empty;
        }
      }

      public override bool Equals( object obj )
      {
        PageIndexes pageIndexes = obj as PageIndexes;

        if( pageIndexes == null )
          return false;

        return ( pageIndexes.StartIndex == this.StartIndex )
          && ( pageIndexes.EndIndex == this.EndIndex );
      }

      public override int GetHashCode()
      {
        return ( this.StartIndex ^ this.EndIndex );
      }

      public static bool operator ==( PageIndexes objA, PageIndexes objB )
      {
        return object.Equals( objA, objB );
      }

      public static bool operator !=( PageIndexes objA, PageIndexes objB )
      {
        return !object.Equals( objA, objB );
      }
    }

    #endregion

    #region LayoutSuspendedHelper Private Class

    private sealed class LayoutSuspendedHelper : IDisposable
    {
      public LayoutSuspendedHelper( TableflowViewItemsHost owner )
      {
        if( owner == null )
          throw new ArgumentNullException( "owner" );

        m_owner = owner;
        m_disposable = owner.m_layoutSuspended.Set();
      }

      void IDisposable.Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        var owner = Interlocked.Exchange( ref m_owner, null );
        if( owner == null )
          return;

        if( m_disposable != null )
        {
          m_disposable.Dispose();
          m_disposable = null;
        }

        if( !owner.m_layoutSuspended.IsSet && owner.m_layoutInvalidatedDuringSuspend )
        {
          owner.InvalidateMeasure();
        }
      }

      ~LayoutSuspendedHelper()
      {
        this.Dispose( false );
      }

      private TableflowViewItemsHost m_owner;
      private IDisposable m_disposable;
    }

    #endregion
  }
}
