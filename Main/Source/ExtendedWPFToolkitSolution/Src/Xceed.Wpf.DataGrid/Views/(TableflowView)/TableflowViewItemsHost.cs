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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Controls;
using Xceed.Utils.Math;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Media.Animation;
using Xceed.Utils.Wpf;
using System.Windows.Input;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid.Views
{
  public class TableflowViewItemsHost : DataGridItemsHost, IAnimatedScrollInfo
  {
    #region Static Members

    private const int MaxDataRowFailFocusCount = 50;

    #endregion

    #region Constructors

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

    #endregion CONSTRUCTORS

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

    #endregion RealizedIndex Property
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

    #endregion ShouldDelayDataContext Property (Internal Attached)

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

    #endregion NavigateToGroup Command

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

          if( ( offset < m_verticalOffset )
            || ( offset >= m_verticalOffset + m_viewportHeight ) )
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

    #endregion AnimatedVerticalOffset Property

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

    private static void OnAnimatedVerticalOffsetChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      TableflowViewItemsHost host = ( TableflowViewItemsHost )sender;

      host.SetVerticalOffsetCore( ( double )e.NewValue );
      host.GeneratePage( false, host.AnimatedScrollInfo.ViewportHeight );
      host.InvalidateArrange();
      host.InvalidateScrollInfo();
    }

    #endregion AnimatedVerticalOffset Property

    #region AnimatedScrollInfo Property

    internal IAnimatedScrollInfo AnimatedScrollInfo
    {
      get
      {
        return ( IAnimatedScrollInfo )this;
      }
    }

    #endregion AnimatedScrollInfo Property

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

    #endregion PreviousTabNavigationMode ( private attached property )

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

    #endregion PreviousDirectionalNavigationMode ( private attached property )

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

    #endregion RowSelectorPane Property

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
      this.InvalidateAutomationPeerChildren();

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
        Row row = this.ExtractRowFromContainer( container );

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

      HeaderFooterItem headerFooterItem = container as HeaderFooterItem;

      if( headerFooterItem != null )
      {
        PassiveLayoutDecorator passiveLayoutDecorator = null;

        if( VisualTreeHelper.GetChildrenCount( headerFooterItem ) > 0 )
        {
          DependencyObject child = VisualTreeHelper.GetChild( headerFooterItem, 0 );
          passiveLayoutDecorator = child as PassiveLayoutDecorator;

          if( ( passiveLayoutDecorator == null ) && ( VisualTreeHelper.GetChildrenCount( child ) > 0 ) )
          {
            passiveLayoutDecorator = VisualTreeHelper.GetChild( child, 0 ) as PassiveLayoutDecorator;
          }

          if( passiveLayoutDecorator != null )
          {
            desiredSize = passiveLayoutDecorator.RealDesiredSize.Width;

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
        this.SetRowSelector( container, translationPoint, containerSize );
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
          maxDesiredWidth = desiredWidth;
      }

      return maxDesiredWidth;
    }

    #endregion Measure/Arrange Methods

    #region Containers Methods

    private Row ExtractRowFromContainer( UIElement container )
    {
      Row row = container as Row;

      if( row != null )
        return row;

      HeaderFooterItem headerFooterItem = container as HeaderFooterItem;

      if( headerFooterItem != null )
      {
        // Until the first measure is called, this will always return null.
        row = HeaderFooterItem.FindIDataGridItemContainerInChildren( headerFooterItem, headerFooterItem.AsVisual() ) as Row;
      }

      return row;
    }

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
      if( generator == null )
        return;

      if( Double.IsInfinity( viewportHeight ) )
        viewportHeight = this.AnimatedScrollInfo.ExtentHeight;

      // Make sure that container recycling is currently enabled on the generator.
      generator.IsRecyclingEnabled = true;

      // Calculate the start and end index of the current page.
      PageIndexes visiblePageIndexes;
      this.CalculateLayoutedIndexes( m_verticalOffset, viewportHeight, out visiblePageIndexes );

      bool pageChanged = ( m_lastVisiblePageIndexes != visiblePageIndexes );
      m_lastVisiblePageIndexes = visiblePageIndexes;

      if( measureInvalidated || pageChanged )
      {
        // Recycle and layout the containers for the current page.
        UIElement focusedElement = this.RecycleContainers( generator, visiblePageIndexes.StartIndex, visiblePageIndexes.EndIndex );

        if( m_containerHeight > 0 )
        {
          this.GenerateContainers( generator, visiblePageIndexes.StartIndex, visiblePageIndexes.EndIndex, measureInvalidated, ref focusedElement );
          this.GenerateStickyHeaders( generator, measureInvalidated, ref focusedElement );
          this.GenerateStickyFooters( generator, measureInvalidated, ref focusedElement );
        }

        if( focusedElement != null )
        {
          int index = generator.GetRealizedIndexForContainer( focusedElement );

          if( index > -1 )
          {
            if( measureInvalidated )
            {
              this.MeasureContainer( focusedElement );
            }

            m_layoutedContainers.Add( new LayoutedContainerInfo( index, focusedElement ) );
            focusedElement = null;
          }
          else
          {
            this.RecycleContainer( focusedElement );
            m_layoutedContainersToRecycle.Add( focusedElement );
          }
        }
      }

      this.PreventOpacityAnimation = false;
    }

    private UIElement RecycleContainers(
      ICustomItemContainerGenerator generator,
      int pageStartIndex,
      int pageEndIndex )
    {
      UIElement focusedElement = null;

      for( int i = 0; i < m_layoutedContainers.Count; i++ )
      {
        LayoutedContainerInfo layoutedContainerInfo = m_layoutedContainers[ i ];
        UIElement container = layoutedContainerInfo.Container;

        // We do not recycle the focused element!
        if( container.IsKeyboardFocusWithin )
        {
          focusedElement = container;
          continue;
        }

        int index = generator.GetRealizedIndexForContainer( container );

        // If the container's index is now out of view, recycle that container.
        if( ( index < pageStartIndex ) || ( index > pageEndIndex ) )
        {
          this.RecycleContainer( generator, index, container );
          m_layoutedContainersToRecycle.Add( container );
        }
      }

      m_layoutedContainers.Clear();
      return focusedElement;
    }

    private void RecycleUnusedStickyContainers(
      ICustomItemContainerGenerator generator,
      StickyContainerInfoList stickyContainers,
      StickyContainerInfoList stickyContainersToExclude,
      ref UIElement focusedElement )
    {
      for( int i = stickyContainers.Count - 1; i >= 0; i-- )
      {
        StickyContainerInfo stickyHeaderInfo = stickyContainers[ i ];
        UIElement container = stickyHeaderInfo.Container;

        if( m_layoutedContainers.ContainsContainer( container ) )
          continue;

        if( ( stickyContainersToExclude != null )
          && ( stickyContainersToExclude.ContainsContainer( container ) ) )
        {
          continue;
        }

        if( container.IsKeyboardFocusWithin )
        {
          System.Diagnostics.Debug.Assert( ( focusedElement == null ) || ( focusedElement == container ) );
          focusedElement = container;
          continue;
        }

        int index = generator.GetRealizedIndexForContainer( container );

        this.RecycleContainer( generator, index, container );
        m_layoutedContainersToRecycle.Add( container );
        stickyContainers.RemoveAt( i );
      }
    }

    private void RecycleContainer( UIElement container )
    {
      this.RecycleContainer( null, -1, container );
    }

    private void RecycleContainer( ICustomItemContainerGenerator generator, int containerIndex, UIElement container )
    {
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

#if DEBUG
      TableflowViewItemsHost.SetRealizedIndex( container, -1 );
#endif //DEBUG
      TableflowViewItemsHost.ClearIsSticky( container );
      container.ClearValue( UIElement.ClipProperty );

      this.DisableElementNavigation( container );
      this.FreeRowSelector( container );
    }

    private void GenerateContainers(
      ICustomItemContainerGenerator generator,
      int pageStartIndex,
      int pageEndIndex,
      bool measureInvalidated,
      ref UIElement focusedElement )
    {
      ScrollDirection scrollDirection = this.AnimatedScrollInfo.VerticalScrollingDirection;

      GeneratorPosition position;
      GeneratorDirection direction;

      if( ( scrollDirection == ScrollDirection.Forward )
        || ( scrollDirection == ScrollDirection.None ) )
      {
        position = generator.GeneratorPositionFromIndex( pageStartIndex );
        direction = GeneratorDirection.Forward;
      }
      else
      {
        position = generator.GeneratorPositionFromIndex( pageEndIndex );
        direction = GeneratorDirection.Backward;
      }

      using( generator.StartAt( position, direction, true ) )
      {
        int currentIndex = ( direction == GeneratorDirection.Forward )
          ? pageStartIndex
          : pageEndIndex;

        while( ( direction == GeneratorDirection.Forward )
          ? ( currentIndex <= pageEndIndex )
          : ( currentIndex >= pageStartIndex ) )
        {
          UIElement container = this.GenerateContainer( generator, currentIndex, measureInvalidated, true );

          if( container == null )
            return;

          if( ( focusedElement != null ) && ( focusedElement == container ) )
          {
            focusedElement = null;
          }

          // The container is now part of the page layout. Add it to the list.
          m_layoutedContainers.Add( new LayoutedContainerInfo( currentIndex, container ) );
          m_layoutedContainersToRecycle.Remove( container );

          currentIndex += ( direction == GeneratorDirection.Forward ) ? 1 : -1;
        }
      }

      m_layoutedContainers.Sort();
    }

    private int GetStickyHeaderCountForIndex( int desiredIndex )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return 0;

      CustomItemContainerGenerator customGenerator = dataGridContext.CustomItemContainerGenerator;

      if( customGenerator == null )
        return 0;

      return customGenerator.GetStickyHeaderCountForIndex( desiredIndex,
                                                           this.AreHeadersStickyCache,
                                                           this.AreGroupHeadersStickyCache,
                                                           this.AreParentRowsStickyCache );
    }

    private int GetStickyFooterCountForIndex( int desiredIndex )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return 0;

      CustomItemContainerGenerator customGenerator = dataGridContext.CustomItemContainerGenerator;

      if( customGenerator == null )
        return 0;

      return customGenerator.GetStickyFooterCountForIndex( desiredIndex,
                                                           this.AreFootersStickyCache,
                                                           this.AreGroupFootersStickyCache );
    }

    private void GenerateStickyHeaders( ICustomItemContainerGenerator generator, bool measureInvalidated, ref UIElement focusedElement )
    {
      CustomItemContainerGenerator customGenerator = ( CustomItemContainerGenerator )generator;

      if( !this.AreHeadersStickyCache && !this.AreGroupHeadersStickyCache && !this.AreParentRowsStickyCache )
      {
        this.RecycleUnusedStickyContainers( generator, m_stickyHeaders, null, ref focusedElement );
        m_stickyHeaders.Clear();
        return;
      }

      int numberOfContainerChecked = 0;

      StickyContainerInfoList newStickyHeaders = new StickyContainerInfoList();

      foreach( LayoutedContainerInfo layoutedContainerInfo in m_layoutedContainers )
      {
        UIElement container = layoutedContainerInfo.Container;

        // For each visible container, we must find what headers should be sticky for it.
        List<StickyContainerGenerated> stickyHeaders =
          customGenerator.GenerateStickyHeaders( container, this.AreHeadersStickyCache, this.AreGroupHeadersStickyCache, this.AreParentRowsStickyCache );

        stickyHeaders.Sort( StickyContainerGeneratedComparer.Singleton );

        // For each sticky headers returned, we must get the index of the last container
        // that could need that container to be sticky. 
        foreach( StickyContainerGenerated stickyHeaderInfo in stickyHeaders )
        {
          UIElement stickyContainer = ( UIElement )stickyHeaderInfo.StickyContainer;

          if( newStickyHeaders.ContainsContainer( stickyContainer ) )
            continue;

          int lastContainerIndex = customGenerator.GetLastHoldingContainerIndexForStickyHeader( stickyContainer );

          StickyContainerInfo stickyContainerInfo = new StickyContainerInfo( stickyContainer, stickyHeaderInfo.Index, lastContainerIndex );
          newStickyHeaders.Add( stickyContainerInfo );

          if( focusedElement == stickyContainer )
          {
            focusedElement = null;
          }

          this.HandleGeneratedStickyContainerPreparation( stickyHeaderInfo, measureInvalidated );
        }

        // We only need to find the sticky headers for one 
        // more element than what is already sticky.
        int visibleStickyHeadersCount = ( int )Math.Ceiling( this.GetStickyHeadersRegionHeight( newStickyHeaders ) / m_containerHeight );

        if( ( ++numberOfContainerChecked - visibleStickyHeadersCount ) >= 1 )
          break;
      }

      foreach( StickyContainerInfo stickyHeaderInfo in newStickyHeaders )
      {
        UIElement container = stickyHeaderInfo.Container;
        int index = m_layoutedContainers.IndexOfContainer( container );

        if( index > -1 )
        {
          m_layoutedContainers.RemoveAt( index );
        }

        m_layoutedContainersToRecycle.Remove( container );
      }

      this.RecycleUnusedStickyContainers( generator, m_stickyHeaders, newStickyHeaders, ref focusedElement );

      m_stickyHeaders.Clear();
      m_stickyHeaders.AddRange( newStickyHeaders );
      m_stickyHeaders.Sort( StickyContainerInfoComparer.Singleton );
    }

    private void GenerateStickyFooters( ICustomItemContainerGenerator generator, bool measureInvalidated, ref UIElement focusedElement )
    {
      if( !this.AreFootersStickyCache && !this.AreGroupFootersStickyCache )
      {
        this.RecycleUnusedStickyContainers( generator, m_stickyFooters, null, ref focusedElement );
        m_stickyFooters.Clear();
        return;
      }

      CustomItemContainerGenerator customGenerator = ( CustomItemContainerGenerator )generator;

      int numberOfContainerChecked = 0;

      StickyContainerInfoList newStickyFooters = new StickyContainerInfoList();

      int layoutedContainerCount = m_layoutedContainers.Count;
      if( layoutedContainerCount > 0 )
      {
        // We must not generate sticky footers if the last container is not even at the bottom of the view!
        LayoutedContainerInfo bottomMostContainerInfo = m_layoutedContainers[ layoutedContainerCount - 1 ];
        double bottomMostContainerOffset = this.GetContainerOffsetFromIndex( bottomMostContainerInfo.RealizedIndex );

        if( ( bottomMostContainerOffset + m_containerHeight ) >= m_viewportHeight )
        {
          for( int i = layoutedContainerCount - 1; i >= 0; i-- )
          {
            LayoutedContainerInfo layoutedContainerInfo = m_layoutedContainers[ i ];
            UIElement layoutedContainer = layoutedContainerInfo.Container;

            // For each visible container, we must find what footers should be sticky for it.
            List<StickyContainerGenerated> stickyFooters =
              customGenerator.GenerateStickyFooters( layoutedContainer, this.AreFootersStickyCache, this.AreGroupFootersStickyCache );

            stickyFooters.Sort( StickyContainerGeneratedReverseComparer.Singleton );

            // For each sticky headers returned, we must get the index of the last container
            // that could need that container to be sticky. 
            foreach( StickyContainerGenerated stickyFooterInfo in stickyFooters )
            {
              UIElement stickyContainer = ( UIElement )stickyFooterInfo.StickyContainer;

              if( newStickyFooters.ContainsContainer( stickyContainer ) )
                continue;

              int firstContainerIndex = customGenerator.GetFirstHoldingContainerIndexForStickyFooter( stickyContainer );

              StickyContainerInfo stickyContainerInfo = new StickyContainerInfo( stickyContainer, stickyFooterInfo.Index, firstContainerIndex );
              newStickyFooters.Add( stickyContainerInfo );

              if( focusedElement == stickyContainer )
              {
                focusedElement = null;
              }

              this.HandleGeneratedStickyContainerPreparation( stickyFooterInfo, measureInvalidated );
            }

            // We only need to find the sticky footers for one 
            // more element than what is already sticky.
            int visibleStickyFootersCount = ( int )Math.Ceiling( ( this.AnimatedScrollInfo.ViewportHeight - this.GetStickyFootersRegionHeight( newStickyFooters ) ) / m_containerHeight );

            if( ( ++numberOfContainerChecked - visibleStickyFootersCount ) >= 1 )
              break;
          }
        }
      }

      foreach( StickyContainerInfo stickyFooterInfo in newStickyFooters )
      {
        int indexOf = m_layoutedContainers.IndexOfContainer( stickyFooterInfo.Container );

        if( indexOf > -1 )
        {
          m_layoutedContainers.RemoveAt( indexOf );
        }
      }

      this.RecycleUnusedStickyContainers( generator, m_stickyFooters, newStickyFooters, ref focusedElement );

      m_stickyFooters.Clear();
      m_stickyFooters.AddRange( newStickyFooters );
      m_stickyFooters.Sort( StickyContainerInfoReverseComparer.Singleton );
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

    private UIElement GenerateContainer(
      ICustomItemContainerGenerator generator,
      int index,
      bool measureInvalidated,
      bool delayDataContext )
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

    private void HandleGeneratedContainerPreparation(
      UIElement container,
      int containerIndex,
      bool isNewlyRealized,
      bool delayDataContext )
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
      bool rowSelectorPaneVisible = this.IsRowSelectorPaneVisible();

      // Layout out of view the recycled container
      foreach( UIElement container in m_layoutedContainersToRecycle )
      {
        this.ArrangeContainer( container, TableflowViewItemsHost.OutOfViewPoint, false );
      }

      m_layoutedContainersToRecycle = new HashSet<UIElement>();

      this.LayoutStickyHeaders( rowSelectorPaneVisible );
      this.LayoutStickyFooters( rowSelectorPaneVisible );
      this.LayoutNonStickyContainers( rowSelectorPaneVisible );

      CommandManager.InvalidateRequerySuggested();

      // We must not call Mouse.Synchronize if we are currently dragging rows. 
      // Update the mouse status to make sure no container has invalid mouse over status.
      // Only do this when the mouse is over the panel, to prevent unescessary update when scrolling with thumb
      if( ( this.ParentDataGridControl.DragDataObject == null ) && ( this.IsMouseOver ) )
      {
        Mouse.Synchronize();
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

    private void ResetContainers()
    {

      // Everything will be recalculated and redrawn in the measure pass.
      this.InvalidateMeasure();
    }

    private void PrepareContainer( UIElement container )
    {
      object dataItem = container.GetValue( Xceed.Wpf.DataGrid.CustomItemContainerGenerator.DataItemPropertyProperty );

      if( dataItem != null )
      {
        // Prepare the container.
        this.ParentDataGridControl.PrepareItemContainer( container, dataItem );
      }
    }

    #endregion Containers Methods

    #region Animations Management

    private void InitializeHorizontalOffsetAnimation()
    {
      if( m_horizontalOffsetAnimation != null )
        return;

      m_horizontalOffsetAnimation = new OffsetAnimation();
    }

    private void StartHorizontalOffsetAnimation()
    {
      DataGridContext rootDataGridContext = this.CachedRootDataGridContext;

      if( rootDataGridContext == null )
      {
        this.StopHorizontalOffsetAnimation();
        return;
      }

      IAnimatedScrollInfo animatedScrollInfo = this.AnimatedScrollInfo;

      m_horizontalOffsetAnimation.Duration = TimeSpan.FromMilliseconds( TableflowView.GetScrollingAnimationDuration( rootDataGridContext ) );
      m_horizontalOffsetAnimation.From = animatedScrollInfo.OriginalHorizontalOffset;
      m_horizontalOffsetAnimation.To = animatedScrollInfo.TargetHorizontalOffset;

      this.StopHorizontalOffsetAnimation();

      m_horizontalOffsetAnimationClock = ( AnimationClock )m_horizontalOffsetAnimation.CreateClock( true );
      m_horizontalOffsetAnimationClock.Completed += this.OnHorizontalOffsetAnimationCompleted;

      this.ApplyAnimationClock( TableflowViewItemsHost.AnimatedHorizontalOffsetProperty, m_horizontalOffsetAnimationClock, HandoffBehavior.SnapshotAndReplace );
    }

    private void StopHorizontalOffsetAnimation()
    {
      if( ( m_horizontalOffsetAnimationClock != null )
        && ( m_horizontalOffsetAnimationClock.Controller != null ) )
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
      DataGridContext rootDataGridContext = this.CachedRootDataGridContext;

      if( rootDataGridContext == null )
      {
        this.StopVerticalOffsetAnimation();

        // Cannot start animation.
        return false;
      }

      double scrollingAnimationDuration = TableflowView.GetScrollingAnimationDuration( rootDataGridContext );

      if( scrollingAnimationDuration == 0 )
      {
        // No animation to start.
        return false;
      }

      IAnimatedScrollInfo animatedScrollInfo = this.AnimatedScrollInfo;

      m_verticalOffsetAnimation.Duration = TimeSpan.FromMilliseconds( scrollingAnimationDuration );
      m_verticalOffsetAnimation.From = animatedScrollInfo.OriginalVerticalOffset;
      m_verticalOffsetAnimation.To = animatedScrollInfo.TargetVerticalOffset;

      this.StopVerticalOffsetAnimation( false );

      m_verticalOffsetAnimationClock = ( AnimationClock )m_verticalOffsetAnimation.CreateClock( true );
      m_verticalOffsetAnimationClock.Completed += this.OnVerticalOffsetAnimationCompleted;

      this.ApplyAnimationClock( TableflowViewItemsHost.AnimatedVerticalOffsetProperty, m_verticalOffsetAnimationClock, HandoffBehavior.SnapshotAndReplace );

      // Animation started.
      return true;
    }

    private void StopVerticalOffsetAnimation( bool resetScrollDirection = true )
    {
      if( ( m_verticalOffsetAnimationClock != null )
        && ( m_verticalOffsetAnimationClock.Controller != null ) )
      {
        if( resetScrollDirection )
          this.AnimatedScrollInfo.VerticalScrollingDirection = ScrollDirection.None;

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

    #endregion Animations Management

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

      if( animate )
      {
        this.StartHorizontalOffsetAnimation();

        // The animated offset will take care of generating the 
        // page and invalidating the ScrollInfo.
      }
      else
      {
        this.StopHorizontalOffsetAnimation();
        this.SetHorizontalOffsetCore( offset );
        // No need to regenerate the page since only the horizontal offset have changed.
        // We must, on the other hand, relayout the containers to reflect the new offsets.
        this.LayoutContainers();
        this.InvalidateScrollInfo();

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
          this.GeneratePage( false, animatedScrollInfo.ViewportHeight );
          this.LayoutContainers();
          this.InvalidateScrollInfo();
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
      ICustomItemContainerGenerator generator = this.CustomItemContainerGenerator;
      GeneratorPosition position = generator.GeneratorPositionFromIndex( index );
      UIElement container = null;

      using( generator.StartAt( position, GeneratorDirection.Forward, true ) )
      {
        container = this.GenerateContainer( generator, index, false, false );
      }

      isDataRow = false;

      if( container != null )
      {
        isDataRow = ( container is DataRow );

        if( ( m_layoutedContainers.IndexOfContainer( container ) == -1 )
          && ( m_stickyHeaders.IndexOfContainer( container ) == -1 )
          && ( m_stickyFooters.IndexOfContainer( container ) == -1 ) )
        {
          // If the container was not already layouted, force
          // a layout of the current page so that the new container
          // is drawn at the right offset.
          m_layoutedContainers.Add( new LayoutedContainerInfo( index, container ) );
          this.LayoutContainers();
        }

        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( container );

        ColumnBase column = ( changeColumn )
          ? dataGridContext.CurrentColumn
          : null;

        // if the current column is null or is (for an unknown reason) set to current but is readOnly, 
        // try to set a new current column

        if( ( isDataRow )
          && ( ( changeColumn ) && ( ( column == null ) || ( !column.CanBeCurrentWhenReadOnly && column.ReadOnly ) ) ) )
        {
          int focusableIndex = DataGridScrollViewer.GetFirstVisibleFocusableColumnIndex( dataGridContext );

          if( focusableIndex >= 0 )
            column = dataGridContext.VisibleColumns[ focusableIndex ];
        }

        try
        {
          DataGridContext currentContext = dataGridContext.DataGridControl.CurrentContext;

          if( currentContext != null )
          {
            currentContext.EndEdit();
          }
        }
        catch( DataGridException )
        {
          return false;
        }

        if( dataGridContext.DataGridControl.SetFocusHelper( container, column, true, true ) )
        {
          generator.SetCurrentIndex( index );
          return true;
        }
      }

      return false;
    }

    #endregion Scrolling Management

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

          // We succeded in changing the current or we're already 
          // at the first index? Then nothing we can do.
          if( ( currentChanged )
            || ( desiredIndex == firstIndex ) )
          {
            break;
          }

          desiredIndex--;
        }

        // We will use MoveFocus if the focused index is currently in view.
        if( ( !currentChanged )
          && ( focusedIndex >= layoutedIndexes.StartIndex )
          && ( focusedIndex <= layoutedIndexes.EndIndex ) )
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

        // We succeded in changing the current or we're already 
        // at the first index? Then nothing we can do.
        if( ( currentChanged )
          || ( desiredIndex == minimumIndex ) )
        {
          break;
        }

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

          // We succeded in changing the current or we're already 
          // at the first index? Then nothing we can do.
          if( ( currentChanged )
            || ( desiredIndex == lastIndex ) )
          {
            break;
          }

          desiredIndex++;
        }

        // We will use MoveFocus if the focused index is currently in view.
        if( ( !currentChanged )
          && ( focusedIndex >= layoutedIndexes.StartIndex )
          && ( focusedIndex <= layoutedIndexes.EndIndex ) )
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

        // We succeded in changing the current or we're already 
        // at the first index? Then nothing we can do.
        if( ( currentChanged )
          || ( desiredIndex == maximumIndex ) )
        {
          break;
        }

        desiredIndex++;
      }

      return currentChanged;
    }

    #endregion BringIntoView Methods

    #region DataGridItemsHost Overrides

    protected override IList<UIElement> CreateChildCollection()
    {
      return new TableflowViewUIElementCollection( this );
    }

    protected override void OnItemsAdded(
      GeneratorPosition position,
      int index,
      int itemCount )
    {
      this.PreventOpacityAnimation = true;

      // We are calling InvalidateMeasure to regenerate the current 
      // page and forcing the recycling of out-of-view containers.
      this.InvalidateMeasure();

    }

    protected override void OnItemsMoved(
      GeneratorPosition position,
      int index,
      GeneratorPosition oldPosition,
      int oldIndex,
      int itemCount,
      int itemUICount,
      IList<DependencyObject> affectedContainers )
    {
      System.Diagnostics.Debug.Fail( "When is this called?" );
      this.ResetContainers();
    }

    protected override void OnItemsReplaced(
      GeneratorPosition position,
      int index,
      GeneratorPosition oldPosition,
      int oldIndex,
      int itemCount,
      int itemUICount,
      IList<DependencyObject> affectedContainers )
    {
      System.Diagnostics.Debug.Fail( "When is this called?" );
      this.ResetContainers();
    }

    protected override void OnItemsRemoved(
      GeneratorPosition position,
      int index,
      GeneratorPosition oldPosition,
      int oldIndex,
      int itemCount,
      int itemUICount,
      IList<DependencyObject> affectedContainers )
    {
      this.PreventOpacityAnimation = true;

      // We are calling InvalidateMeasure to regenerate the current 
      // page and forcing the recycling of out-of-view containers.
      this.InvalidateMeasure();

    }

    protected override void OnItemsReset()
    {
      this.ResetContainers();
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

    #endregion DataGridItemsHost Overrides

    #region PreviewKeyDown and KeyDown Handling

    protected override void HandleTabKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      DataGridControl dataGridControl = this.ParentDataGridControl;

      if( dataGridControl == null )
        return;

      DataGridContext currentDataGridContext = dataGridControl.CurrentContext;

      if( currentDataGridContext == null )
        return;

      // Only process tab in a special way when the grid has focus and is
      // being edited
      if( !dataGridControl.IsKeyboardFocusWithin || !dataGridControl.IsBeingEdited )
        return;

      DependencyObject predictedNextVisual = null;

      //If the original source is not a control (e.g. the cells panel instead of a cell), columns will be used to move focus.
      Control originalSource = e.OriginalSource as Control;
      if( originalSource != null )
      {
        if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift )
        {
          predictedNextVisual = ( e.OriginalSource as Control ).PredictFocus( FocusNavigationDirection.Left );
        }
        else
        {
          predictedNextVisual = ( e.OriginalSource as Control ).PredictFocus( FocusNavigationDirection.Right );
        }
      }

      if( predictedNextVisual != null )
      {
        Cell ownerCell = Cell.FindFromChild( predictedNextVisual );

        if( ( ownerCell != null ) && ( ownerCell.ParentColumn == dataGridControl.CurrentColumn ) )
        {
          if( object.Equals( ownerCell.ParentRow.DataContext, dataGridControl.CurrentItemInEdition ) )
            return;
        }
      }

      int visibleColumnCount = currentDataGridContext.VisibleColumns.Count;
      ReadOnlyObservableCollection<ColumnBase> visibleColumns = currentDataGridContext.VisibleColumns;

      ColumnBase currentColumn = currentDataGridContext.CurrentColumn;

      if( currentColumn == null )
      {
        int firstFocusableColumn = DataGridScrollViewer.GetFirstVisibleFocusableColumnIndex( currentDataGridContext );

        if( firstFocusableColumn < 0 )
          throw new DataGridException( "Trying to edit while no cell is focusable. " );

        try
        {
          currentDataGridContext.SetCurrentColumnAndChangeSelection( currentDataGridContext.VisibleColumns[ firstFocusableColumn ] );
        }
        catch( DataGridException )
        {
          // We swallow the exception if it occurs because of a validation error or Cell was read-only or
          // any other GridException.
        }

        e.Handled = true;
      }
      else
      {
        if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift )
        {
          int previousColumnVisiblePosition = DataGridScrollViewer.GetPreviousVisibleFocusableColumnIndex( currentDataGridContext );

          if( previousColumnVisiblePosition < visibleColumnCount )
          {
            if( previousColumnVisiblePosition >= 0 )
            {
              try
              {
                currentDataGridContext.SetCurrentColumnAndChangeSelection( visibleColumns[ previousColumnVisiblePosition ] );
              }
              catch( DataGridException )
              {
                // We swallow the exception if it occurs because of a validation error or Cell was read-only or
                // any other GridException.
              }
            }
            else
            {
              previousColumnVisiblePosition = DataGridScrollViewer.GetFirstVisibleFocusableColumnIndex( currentDataGridContext );

              if( previousColumnVisiblePosition < 0 )
                throw new DataGridException( "Trying to edit while no cell is focusable. " );

              try
              {
                // Wrap around to the last column
                currentDataGridContext.SetCurrentColumnAndChangeSelection( visibleColumns[ previousColumnVisiblePosition ] );
              }
              catch( DataGridException )
              {
                // We swallow the exception if it occurs because of a validation error or Cell was read-only or
                // any other GridException.
              }
            }
            e.Handled = true;
          }
        }
        else
        {
          int nextColumnVisiblePosition = DataGridScrollViewer.GetNextVisibleFocusableColumnIndex( currentDataGridContext );

          // If previous Column VisiblePosition is greater than 0 and
          // less than the total VisibleColumn count, affect it
          if( nextColumnVisiblePosition >= 0 )
          {
            if( nextColumnVisiblePosition < visibleColumnCount )
            {
              try
              {
                currentDataGridContext.SetCurrentColumnAndChangeSelection( visibleColumns[ nextColumnVisiblePosition ] );
              }
              catch( DataGridException )
              {
                // We swallow the exception if it occurs because of a validation error or Cell was read-only or
                // any other GridException.
              }
            }
            else
            {
              nextColumnVisiblePosition = DataGridScrollViewer.GetFirstVisibleFocusableColumnIndex( currentDataGridContext );

              if( nextColumnVisiblePosition < 0 )
                throw new DataGridException( "Trying to edit while no cell is focusable. " );

              try
              {
                currentDataGridContext.SetCurrentColumnAndChangeSelection( visibleColumns[ nextColumnVisiblePosition ] );
              }
              catch( DataGridException )
              {
                // We swallow the exception if it occurs because of a validation error or Cell was read-only or
                // any other GridException.
              }
            }

            e.Handled = true;
          }
        }
      }
    }

    protected override void HandleLeftKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      e.Handled = this.MoveFocusLeft();
    }

    protected override void HandleRightKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      e.Handled = this.MoveFocusRight();
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
        // Simply scroll to top and set first index as current
        this.AnimatedScrollInfo.ScrollOwner.ScrollToTop();
        this.SetCurrent( 0, changeCurrentColumn );
        return;
      }

      UIElement focusedContainer = DataGridItemsHost.GetItemsHostContainerFromElement( this, Keyboard.FocusedElement as DependencyObject );

      if( ( focusedContainer == null ) ||
        ( navigationBehavior == NavigationBehavior.None ) )
      {
        // We just need to scroll one page up.
        this.AnimatedScrollInfo.ScrollOwner.PageUp();
        return;
      }

      int generatorItemCount = this.CustomItemContainerGenerator.ItemCount;
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

        // SetCurrent on the index or down to a focusable index
        while( ( !dataGridControl.HasValidationError )
            && ( !isDataRow )
            && ( desiredPageUpIndex < generatorItemCount ) )
        {
          if( this.SetCurrent( desiredPageUpIndex, changeCurrentColumn, out isDataRow ) )
            return;

          desiredPageUpIndex++;
        }

        if( ( dataGridControl.HasValidationError ) || ( isDataRow ) )
          return;

        // No container were focused while processing indexes from focused to 
        // initialDesiredIndex, try SetCurrent on indexes lower than
        // the initial up to 0
        desiredPageUpIndex = initialDesiredIndex - 1;
        isDataRow = false;

        while( ( !dataGridControl.HasValidationError )
            && ( !isDataRow )
            && ( desiredPageUpIndex > 0 ) )
        {
          if( this.SetCurrent( desiredPageUpIndex, changeCurrentColumn, out isDataRow ) )
            return;

          desiredPageUpIndex--;
        }
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

      // CTRL + PageDown
      if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
      {
        // Simply scroll to bottom and set last index as current
        this.AnimatedScrollInfo.ScrollOwner.ScrollToBottom();
        this.SetCurrent( this.CustomItemContainerGenerator.ItemCount - 1, changeCurrentColumn );
        return;
      }

      UIElement focusedContainer = DataGridItemsHost.GetItemsHostContainerFromElement( this, Keyboard.FocusedElement as DependencyObject );

      // No focused container or no navigation allowed
      if( ( focusedContainer == null ) ||
        ( navigationBehavior == NavigationBehavior.None ) )
      {
        // We just need to scroll one page down.
        this.AnimatedScrollInfo.ScrollOwner.PageDown();
        return;
      }

      int generatorItemCount = this.CustomItemContainerGenerator.ItemCount;
      int focusedContainerRealizedIndex = this.CustomItemContainerGenerator.GetRealizedIndexForContainer( focusedContainer );

      int desiredPageDownIndex = Math.Min( generatorItemCount - 1,
                                       focusedContainerRealizedIndex + m_pageVisibleContainerCount - 1 );

      int initialDesiredIndex = desiredPageDownIndex;

      int maxIndex = generatorItemCount - 1;

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

        // SetCurrent on the index or up to focusedContainerRealizedIndex to a focusable index 

        while( ( !dataGridControl.HasValidationError )
            && ( !isDataRow )
            && ( desiredPageDownIndex > focusedContainerRealizedIndex ) )
        {
          if( this.SetCurrent( desiredPageDownIndex, changeCurrentColumn, out isDataRow ) )
            return;

          desiredPageDownIndex--;
        }

        if( ( dataGridControl.HasValidationError ) || ( isDataRow ) )
          return;

        //Debug.Assert( false, "When this will occur???" );
        desiredPageDownIndex = initialDesiredIndex + 1;
        isDataRow = false;

        // No container were focused while processing indexes from focused to 
        // initialDesiredIndex, try SetCurrent on indexes higher than
        // the initial down to maxIndex
        while( ( !dataGridControl.HasValidationError )
            && ( !isDataRow )
            && ( desiredPageDownIndex < maxIndex ) )
        {
          if( this.SetCurrent( Math.Min( maxIndex, desiredPageDownIndex ), changeCurrentColumn, out isDataRow ) )
            return;

          desiredPageDownIndex++;
        }
      }
    }

    protected override void HandleHomeKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      DataGridControl dataGridControl = this.ParentDataGridControl;
      IAnimatedScrollInfo scrollInfo = this.AnimatedScrollInfo;
      ICustomItemContainerGenerator generator = this.CustomItemContainerGenerator;

      NavigationBehavior navigationBehavior = dataGridControl.NavigationBehavior;
      bool changeCurrentColumn = ( navigationBehavior == NavigationBehavior.CellOnly );

      if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
      {
        if( navigationBehavior != NavigationBehavior.None )
        {
          int currentIndex = generator.GetCurrentIndex();
          int itemCount = generator.ItemCount;

          // If the current index is different than the CurrentIndex
          // force the first focusable from index 0 to become Current
          if( ( currentIndex != 0 ) && ( itemCount > 0 ) )
          {
            // We have to find the first index that can get keyboard focus
            // from 0 to currentIndex
            this.FocusIndexOrNextFocusable( 0, currentIndex, changeCurrentColumn );
          }

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

    private void ResetHorizontalOffset()
    {
      ScrollViewer parentScrollViewer = this.m_scrollOwner;

      if( parentScrollViewer != null )
        parentScrollViewer.ScrollToHorizontalOffset( 0d );
    }

    protected override void HandleEndKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
      {
        DataGridControl dataGridControl = this.ParentDataGridControl;
        IAnimatedScrollInfo scrollInfo = this.AnimatedScrollInfo;
        ICustomItemContainerGenerator generator = this.CustomItemContainerGenerator;

        FlowDirection flowDirection = dataGridControl.FlowDirection;
        NavigationBehavior navigationBehavior = dataGridControl.NavigationBehavior;
        bool changeCurrentColumn = ( navigationBehavior == NavigationBehavior.CellOnly );

        if( navigationBehavior != NavigationBehavior.None )
        {
          int currentIndex = generator.GetCurrentIndex();
          int itemCount = generator.ItemCount;
          int lastItemIndex = itemCount - 1;

          // If the current index is different than the CurrentIndex
          // force the last focusable from index itemCount to become Current
          if( ( currentIndex != lastItemIndex ) && ( itemCount > 0 ) )
          {
            // We have to find the last index that can get keyboard focus
            // from lastItemIndex to currentIndex
            this.FocusIndexOrPreviousFocusable( lastItemIndex, currentIndex, changeCurrentColumn );
          }

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

    private bool MoveToNextColumn()
    {
      DataGridControl dataGridControl = this.ParentDataGridControl;

      if( dataGridControl == null )
        return false;

      DataGridContext currentDataGridContext = dataGridControl.CurrentContext;

      if( currentDataGridContext == null )
        return false;

      ReadOnlyColumnCollection visibleColumnsCollection = ( ReadOnlyColumnCollection )currentDataGridContext.VisibleColumns;
      int visibleColumnsCollectionCount = visibleColumnsCollection.Count;
      ColumnBase currentColumn = currentDataGridContext.CurrentColumn;

      if( currentColumn == null )
      {
        if( visibleColumnsCollectionCount > 0 )
        {
          this.MoveToFirstVisibleColumn();
          return true;
        }
      }
      Row currentRow = currentDataGridContext.CurrentRow;

      int currentColumnIndex = visibleColumnsCollection.IndexOf( currentColumn );

      int nextFocusableVisibleColumn = 0;

      bool focusableColumnFound = false;

      if( currentRow != null )
      {
        for( nextFocusableVisibleColumn = currentColumnIndex + 1; nextFocusableVisibleColumn < visibleColumnsCollectionCount; nextFocusableVisibleColumn++ )
        {
          if( currentRow.Cells[ visibleColumnsCollection[ nextFocusableVisibleColumn ] ].GetCalculatedCanBeCurrent() )
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
              currentDataGridContext.VisibleColumns[ nextFocusableVisibleColumn ] );
          }
          catch( DataGridException )
          {
            // We swallow the exception if it occurs because of a validation error or Cell was read-only or
            // any other GridException.
          }

          return true;
        }
        // If the column is other than last Column or than the last current column
        else
        {
          this.MoveToLastVisibleColumn();
        }
      }
      return false;
    }

    private bool MoveToPreviousColumn()
    {
      DataGridControl dataGridControl = this.ParentDataGridControl;

      if( dataGridControl == null )
        return false;

      DataGridContext currentDataGridContext = dataGridControl.CurrentContext;

      if( currentDataGridContext == null )
        return false;

      ReadOnlyColumnCollection visibleColumnsCollection = ( ReadOnlyColumnCollection )currentDataGridContext.VisibleColumns;
      int visibleColumnsCollectionCount = visibleColumnsCollection.Count;

      ColumnBase currentColumn = currentDataGridContext.CurrentColumn;

      if( currentColumn == null )
      {
        if( visibleColumnsCollectionCount > 0 )
        {
          this.MoveToFirstVisibleColumn();
          return true;
        }
      }
      else
      {
        Row currentRow = currentDataGridContext.CurrentRow;

        int currentColumnIndex = visibleColumnsCollection.IndexOf( currentColumn );
        bool focusableColumnFound = false;

        // If the column is other than first Column
        if( currentColumnIndex > 0 )
        {
          int previousFocusableVisibleColumn = 0;
          if( currentRow != null )
          {
            for( previousFocusableVisibleColumn = currentColumnIndex - 1; previousFocusableVisibleColumn >= 0; previousFocusableVisibleColumn-- )
            {
              if( currentRow.Cells[ visibleColumnsCollection[ previousFocusableVisibleColumn ] ].GetCalculatedCanBeCurrent() )
              {
                focusableColumnFound = true;
                break;
              }
            }
          }

          if( focusableColumnFound )
          {
            try
            {
              currentDataGridContext.SetCurrentColumnAndChangeSelection(
                 currentDataGridContext.VisibleColumns[ previousFocusableVisibleColumn ] );
            }
            catch( DataGridException )
            {
              // We swallow the exception if it occurs because of a validation error or Cell was read-only or
              // any other GridException.
            }

            return true;
          }
          else
          {
            this.MoveToFirstVisibleColumn();
          }
        }
      }
      return false;
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

    #endregion

    #region Scroll Easing Management






























    #endregion Scroll Easing Management

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

    #endregion IAnimatedScrollInfo Members

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

      IAnimatedScrollInfo animatedScrollInfo = this.AnimatedScrollInfo;
      double itemOffset = ( m_delayedBringIntoViewIndex * m_containerHeight );

      bool areHeadersSticky = this.AreHeadersStickyCache || this.AreParentRowsStickyCache || this.AreGroupHeadersStickyCache;
      bool areFootersSticky = this.AreFootersStickyCache || this.AreGroupFootersStickyCache;

      int stickyHeadersCount = ( areHeadersSticky )
        ? this.GetStickyHeaderCountForIndex( m_delayedBringIntoViewIndex )
        : 0;

      double stickyHeadersHeight = stickyHeadersCount * m_containerHeight;
      double topThreshold = m_verticalOffset + stickyHeadersHeight;

      bool handled = false;

      if( itemOffset < topThreshold )
      {
        this.ScrollToVerticalOffset( itemOffset - stickyHeadersHeight );
        handled = true;
      }

      if( !handled )
      {
        int stickyFootersCount = ( areFootersSticky )
          ? this.GetStickyFooterCountForIndex( m_delayedBringIntoViewIndex )
          : 0;

        double stickyFootersHeight = stickyFootersCount * m_containerHeight;
        double bottomThreshold = ( m_verticalOffset + m_viewportHeight ) - stickyHeadersHeight;

        if( itemOffset + m_containerHeight > bottomThreshold )
        {
          this.ScrollToVerticalOffset( itemOffset - ( m_viewportHeight - stickyFootersHeight - m_containerHeight ) );
          handled = true;
        }
      }

    }


    #endregion ICustomVirtualizingPanel Members

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

    #endregion IScrollInfo Members

    #region ScrollTip Helper Methods

    internal bool IsDataItemHiddenBySticky( int index )
    {
      if( !m_layoutedContainers.ContainsRealizedIndex( index ) )
        return false;

      double indexOffset = this.GetContainerOffsetFromIndex( index );
      double stickyHeadersRegionHeight = this.GetStickyHeadersRegionHeight();
      double stickyFootersRegionHeight = this.GetStickyFootersRegionHeight();

      return
        ( ( indexOffset - m_verticalOffset + m_containerHeight ) <= stickyHeadersRegionHeight ) ||
        ( ( indexOffset - m_verticalOffset ) >= stickyFootersRegionHeight );
    }

    #endregion ScrollTip Helper Methods

    #region PRIVATE METHODS

    private static void MoveFocusForwardExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      e.Handled = ( ( TableflowViewItemsHost )sender ).MoveFocusRight();
    }

    private static void MoveFocusForwardCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = true;
      e.Handled = true;
    }

    private static void MoveFocusBackExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      e.Handled = ( ( TableflowViewItemsHost )sender ).MoveFocusLeft();
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

    private bool MoveFocusLeft()
    {
      DataGridControl dataGridControl = this.ParentDataGridControl;

      if( dataGridControl == null )
        return false;

      bool dataGridControlFocusWithin = dataGridControl.IsKeyboardFocusWithin;

      // Process Left key even if NavigationBehavior is RowOnly
      // when the grid is being edited 
      bool dataGridControlIsBeingEdited =
        dataGridControlFocusWithin && dataGridControl.IsBeingEdited;

      FlowDirection flowDirection = dataGridControl.FlowDirection;
      NavigationBehavior navigationBehavior = dataGridControl.NavigationBehavior;

      if( ( navigationBehavior == NavigationBehavior.CellOnly )
        || ( ( navigationBehavior == NavigationBehavior.RowOnly ) && dataGridControlIsBeingEdited )
        || ( ( navigationBehavior == NavigationBehavior.RowOrCell )
              && dataGridControlFocusWithin
              && dataGridControl.CurrentContext.CurrentColumn != null ) )
      {
        if( flowDirection == FlowDirection.LeftToRight )
        {
          return this.MoveToPreviousColumn();
        }
        else
        {
          return this.MoveToNextColumn();
        }
      }

      return false;
    }

    private bool MoveFocusRight()
    {
      DataGridControl dataGridControl = this.ParentDataGridControl;

      if( dataGridControl == null )
        return false;

      bool dataGridControlFocusWithin = dataGridControl.IsKeyboardFocusWithin;

      // Process Right key even if NavigationBehavior is RowOnly
      // when the grid is being edited 
      bool dataGridControlIsBeingEdited =
        dataGridControlFocusWithin && dataGridControl.IsBeingEdited;

      FlowDirection flowDirection = dataGridControl.FlowDirection;
      NavigationBehavior navigationBehavior = dataGridControl.NavigationBehavior;

      if( ( navigationBehavior == NavigationBehavior.CellOnly )
        || ( ( navigationBehavior == NavigationBehavior.RowOnly ) && dataGridControlIsBeingEdited )
        || ( ( navigationBehavior == NavigationBehavior.RowOrCell )
              && dataGridControlFocusWithin
              && dataGridControl.CurrentContext.CurrentColumn != null ) )
      {
        if( flowDirection == FlowDirection.LeftToRight )
        {
          return this.MoveToNextColumn();
        }
        else
        {
          return this.MoveToPreviousColumn();
        }
      }

      return false;
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

    #endregion PRIVATE METHODS

    #region CONSTANTS

    private static readonly Point OutOfViewPoint = new Point( -999999, -999999 );
    private static readonly Point EmptyPoint = new Point( 0, 0 );

    #endregion CONSTANTS

    #region PRIVATE FIELDS



    private LayoutedContainerInfoList m_layoutedContainers = new LayoutedContainerInfoList();
    private HashSet<UIElement> m_layoutedContainersToRecycle = new HashSet<UIElement>();

    private StickyContainerInfoList m_stickyHeaders = new StickyContainerInfoList();
    private StickyContainerInfoList m_stickyFooters = new StickyContainerInfoList();

    private int m_pageVisibleContainerCount;

    private OffsetAnimation m_horizontalOffsetAnimation;
    private AnimationClock m_horizontalOffsetAnimationClock;

    private OffsetAnimation m_verticalOffsetAnimation;
    private AnimationClock m_verticalOffsetAnimationClock;

    private PageIndexes m_lastVisiblePageIndexes = PageIndexes.Empty;

    private Dictionary<string, double> m_cachedContainerDesiredWidth = new Dictionary<string, double>();
    private Dictionary<string, double> m_cachedContainerRealDesiredWidth = new Dictionary<string, double>();

    private List<string> m_autoWidthCalculatedDataGridContextList = new List<string>();

    private int m_delayedBringIntoViewIndex = -1;

    private Size m_lastMeasureAvailableSize = Size.Empty;
    private Size m_lastArrangeFinalSize = Size.Empty;

    private double m_containerHeight;

    private BitVector32 m_flags = new BitVector32();

    #endregion PRIVATE FIELDS

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

    #endregion TableflowItemsHostFlags Private Enum

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

    #endregion PageIndexes Private Class
  }
}
