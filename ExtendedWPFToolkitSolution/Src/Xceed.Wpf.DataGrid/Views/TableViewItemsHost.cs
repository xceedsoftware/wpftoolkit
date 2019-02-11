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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Utils.Collections;
using Xceed.Utils.Wpf;

namespace Xceed.Wpf.DataGrid.Views
{
  public class TableViewItemsHost : DataGridItemsHost, IScrollInfo, IDeferableScrollInfoRefresh
  {
    static TableViewItemsHost()
    {
      KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(
        typeof( TableViewItemsHost ),
        new FrameworkPropertyMetadata( KeyboardNavigationMode.Local ) );

      CommandManager.RegisterClassCommandBinding( typeof( TableViewItemsHost ),
        new CommandBinding( ComponentCommands.MoveFocusForward,
          TableViewItemsHost.MoveFocusForwardExecuted, TableViewItemsHost.MoveFocusForwardCanExecute ) );

      CommandManager.RegisterClassCommandBinding( typeof( TableViewItemsHost ),
        new CommandBinding( ComponentCommands.MoveFocusBack,
          TableViewItemsHost.MoveFocusBackExecuted, TableViewItemsHost.MoveFocusBackCanExecute ) );

      CommandManager.RegisterClassCommandBinding( typeof( TableViewItemsHost ),
        new CommandBinding( ComponentCommands.MoveFocusUp,
          TableViewItemsHost.MoveFocusUpExecuted, TableViewItemsHost.MoveFocusUpCanExecute ) );

      CommandManager.RegisterClassCommandBinding( typeof( TableViewItemsHost ),
        new CommandBinding( ComponentCommands.MoveFocusDown,
          TableViewItemsHost.MoveFocusDownExecuted, TableViewItemsHost.MoveFocusDownCanExecute ) );
    }

    public TableViewItemsHost()
    {
      this.AddHandler( FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler( this.OnRequestBringIntoView ) );
    }

    #region Orientation Property

    [Obsolete( "The Orientation property is obsolete. Only a vertical orientation is supported.", false )]
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register( "Orientation", typeof( Orientation ), typeof( TableViewItemsHost ), new UIPropertyMetadata( Orientation.Vertical ) );

    [Obsolete( "The Orientation property is obsolete. Only a vertical orientation is supported.", false )]
    public Orientation Orientation
    {
      get
      {
#pragma warning disable 618
        return ( Orientation )this.GetValue( TableViewItemsHost.OrientationProperty );
#pragma warning restore 618
      }
      set
      {
#pragma warning disable 618
        this.SetValue( TableViewItemsHost.OrientationProperty, value );
#pragma warning restore 618
      }
    }

    #endregion

    #region StableScrollingEnabled Property

    [Obsolete( "The StableScrollingEnabled property is obsolete.", false )]
    public static readonly DependencyProperty StableScrollingEnabledProperty =
        DependencyProperty.Register( "StableScrollingEnabled", typeof( bool ), typeof( TableViewItemsHost ), new UIPropertyMetadata( true ) );

    [Obsolete( "The StableScrollingEnabled property is obsolete.", false )]
    public bool StableScrollingEnabled
    {
      get
      {
#pragma warning disable 618
        return ( bool )this.GetValue( TableViewItemsHost.StableScrollingEnabledProperty );
#pragma warning restore 618
      }
      set
      {
#pragma warning disable 618
        this.SetValue( TableViewItemsHost.StableScrollingEnabledProperty, value );
#pragma warning restore 618
      }
    }

    #endregion

    #region StableScrollingProportion Property

    [Obsolete( "The StableScrollingProportion property is obsolete.", false )]
    public static readonly DependencyProperty StableScrollingProportionProperty =
        DependencyProperty.Register( "StableScrollingProportion", typeof( double ), typeof( TableViewItemsHost ), new UIPropertyMetadata( 0.5d ) );

    [Obsolete( "The StableScrollingProportion property is obsolete.", false )]
    public double StableScrollingProportion
    {
      get
      {
#pragma warning disable 618
        return ( double )this.GetValue( TableViewItemsHost.StableScrollingProportionProperty );
#pragma warning restore 618
      }
      set
      {
#pragma warning disable 618
        this.SetValue( TableViewItemsHost.StableScrollingProportionProperty, value );
#pragma warning restore 618
      }
    }

    #endregion

    #region ScrollInfo Property

    internal IScrollInfo ScrollInfo
    {
      get
      {
        return ( IScrollInfo )this;
      }
    }

    #endregion

    #region PreviousTabNavigationMode ( private attached property )

    private static readonly DependencyProperty PreviousTabNavigationModeProperty = DependencyProperty.RegisterAttached(
      "PreviousTabNavigationMode",
      typeof( KeyboardNavigationMode ),
      typeof( TableViewItemsHost ),
      new FrameworkPropertyMetadata( ( KeyboardNavigationMode )KeyboardNavigationMode.None ) );

    private static KeyboardNavigationMode GetPreviousTabNavigationMode( DependencyObject d )
    {
      return ( KeyboardNavigationMode )d.GetValue( TableViewItemsHost.PreviousTabNavigationModeProperty );
    }

    private static void SetPreviousTabNavigationMode( DependencyObject d, KeyboardNavigationMode value )
    {
      d.SetValue( TableViewItemsHost.PreviousTabNavigationModeProperty, value );
    }

    #endregion

    #region PreviousDirectionalNavigationMode ( private attached property )

    private static readonly DependencyProperty PreviousDirectionalNavigationModeProperty = DependencyProperty.RegisterAttached(
      "PreviousDirectionalNavigationMode",
      typeof( KeyboardNavigationMode ),
      typeof( TableViewItemsHost ),
      new FrameworkPropertyMetadata( ( KeyboardNavigationMode )KeyboardNavigationMode.None ) );

    private static KeyboardNavigationMode GetPreviousDirectionalNavigationMode( DependencyObject d )
    {
      return ( KeyboardNavigationMode )d.GetValue( TableViewItemsHost.PreviousDirectionalNavigationModeProperty );
    }

    private static void SetPreviousDirectionalNavigationMode( DependencyObject d, KeyboardNavigationMode value )
    {
      d.SetValue( TableViewItemsHost.PreviousDirectionalNavigationModeProperty, value );
    }

    #endregion

    #region RowSelectorPane Property

    private RowSelectorPane RowSelectorPane
    {
      get
      {
        var scrollViewer = this.ScrollInfo.ScrollOwner as TableViewScrollViewer;
        if( scrollViewer == null )
          return null;

        return scrollViewer.RowSelectorPane;
      }
    }

    private bool IsRowSelectorPaneVisible()
    {
      var rowSelectorPane = this.RowSelectorPane;

      return ( rowSelectorPane != null )
          && ( rowSelectorPane.Visibility == Visibility.Visible );
    }

    #endregion

    #region Measure/Arrange Methods

    protected override Size MeasureOverride( Size availableSize )
    {
      m_cachedContainerDesiredWidth.Clear();
      m_cachedContainerRealDesiredWidth.Clear();
      m_autoWidthCalculatedDataGridContextList.Clear();
      m_lastMeasureAvailableSize = availableSize;

      var generatedPage = this.GeneratePageAndUpdateIScrollInfoValues( availableSize, true );

      return this.GetNewDesiredSize( generatedPage );
    }

    private void MeasureContainer( UIElement container )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( container );

      string dataGridContextName = this.GetDataGridContextName( container, dataGridContext );
      bool containerIsRow = this.ContainerIsRow( container );

      if( ( !m_autoWidthCalculatedDataGridContextList.Contains( dataGridContextName ) )
        && ( containerIsRow ) )
      {
        dataGridContext.ColumnStretchingManager.ColumnStretchingCalculated = false;

        // Calling Measure with the Viewport's width will have the effect of 
        // distributing the extra space (see FixedCellPanel's MeasureOverride). 
        // Eventually, the FixedCellPanel will receive an adjusted viewport 
        // width (where GroupLevelIndicator's width et al will be substracted).
        container.Measure( new Size( m_lastMeasureAvailableSize.Width, double.PositiveInfinity ) );

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

        var row = Row.FromContainer( container );
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
      container.Measure( new Size( double.PositiveInfinity, double.PositiveInfinity ) );

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
      HashSet<UIElement> layoutedContainers = new HashSet<UIElement>();

      if( m_lastGeneratedPage != TableViewPage.Empty )
      {
        var showRowSelector = this.IsRowSelectorPaneVisible();
        var innerPage = m_lastGeneratedPage.InnerPage;
        var startIndex = innerPage.Start;
        var endIndex = innerPage.End;
        var verticalOffset = 0d;

        foreach( var container in ( from ci in m_layoutedContainers
                                    let realizedIndex = ci.RealizedIndex
                                    where ( realizedIndex >= startIndex )
                                       && ( realizedIndex <= endIndex )
                                    select ci.Container ) )
        {
          layoutedContainers.Add( container );

          this.ArrangeContainer( container, -m_horizontalOffset, verticalOffset, showRowSelector );

          verticalOffset += container.RenderSize.Height;
        }
      }

      // Hide the containers that are not visible in the viewport.
      foreach( var container in this.Children )
      {
        if( layoutedContainers.Contains( container ) )
          continue;

        this.ArrangeContainerOutOfView( container );
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

      m_lastLayoutedPage = m_lastGeneratedPage;
      m_indexToBringIntoView = TableViewItemsHost.NullIndex;

      return finalSize;
    }

    private void ArrangeContainer( UIElement container, double horizontalOffset, double verticalOffset, bool showRowSelector )
    {
      var origin = new Point( horizontalOffset, verticalOffset );
      var dataGridContext = DataGridControl.GetDataGridContext( container );
      var dataGridContextName = this.GetDataGridContextName( container, dataGridContext );
      var containerSize = new Size( this.GetContainerWidth( dataGridContextName ), container.DesiredSize.Height );

      container.Arrange( new Rect( origin, containerSize ) );

      this.SetCompensationOffset( dataGridContext, container, containerSize.Width );

      if( showRowSelector )
      {
        this.SetRowSelector( container, origin, containerSize );
      }
    }

    private void ArrangeContainerOutOfView( UIElement container )
    {
      container.Arrange( TableViewItemsHost.OutOfViewRect );

      this.FreeRowSelector( container );
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
      var scrollViewer = this.ScrollInfo.ScrollOwner as DataGridScrollViewer;
      if( scrollViewer == null )
        return 0d;

      return scrollViewer.SynchronizedScrollViewersWidth;
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
      var rowSelectorPane = this.RowSelectorPane;
      if( rowSelectorPane == null )
        return;

      rowSelectorPane.SetRowSelectorPosition( container, new Rect( translationPoint, size ), this );
    }

    private void FreeRowSelector( UIElement container )
    {
      var rowSelectorPane = this.RowSelectorPane;
      if( rowSelectorPane == null )
        return;

      rowSelectorPane.FreeRowSelector( container );
    }

    private double GetMaxDesiredWidth()
    {
      double maxDesiredWidth = 0d;

      foreach( var item in m_cachedContainerDesiredWidth )
      {
        var desiredWidth = item.Value;
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

    private Size GetNewDesiredSize( TableViewPage pageInfo )
    {
      double desiredHeight;
      double availableHeight = m_lastMeasureAvailableSize.Height;

      if( pageInfo == TableViewPage.Empty )
      {
        desiredHeight = 0d;
      }
      // The current generated page displays only a subset of the data source. 
      else if( pageInfo.InnerPage.Length < this.ScrollInfo.ExtentHeight )
      {
        if( double.IsPositiveInfinity( availableHeight ) )
        {
          desiredHeight = pageInfo.InnerPage.Size;
        }
        else
        {
          desiredHeight = availableHeight;
        }
      }
      // The current generated page displays all containers.
      else
      {
        var pageHeight = pageInfo.OuterPage.Size;

        if( double.IsPositiveInfinity( availableHeight ) )
        {
          desiredHeight = pageHeight;
        }
        else
        {
          desiredHeight = Math.Min( availableHeight, pageHeight );
        }
      }

      return new Size( m_viewportWidth, desiredHeight );
    }

    #endregion

    #region Containers Methods

    private bool ContainerIsRow( UIElement container )
    {
      if( container is Row )
        return true;

      var headerFooterItem = container as HeaderFooterItem;
      if( headerFooterItem != null )
        return typeof( Row ).IsAssignableFrom( headerFooterItem.VisualRootElementType );

      return false;
    }

    private string GetDataGridContextName( UIElement container, DataGridContext dataGridContext )
    {
      return ( dataGridContext.SourceDetailConfiguration != null )
        ? dataGridContext.SourceDetailConfiguration.RelationName
        : string.Empty;
    }

    private TableViewPage GeneratePage( TableViewStartPageInfo pageInfo, double availableHeight, bool forceMeasure )
    {
      // The generator can be null if we're in design mode.
      var generator = this.CustomItemContainerGenerator;
      if( generator == null )
        return m_lastGeneratedPage;

      var generatePage = ( forceMeasure )
                      || ( m_lastGeneratedPage == TableViewPage.Empty )
                      || ( m_lastGeneratedPage.InnerPage.Start != pageInfo.Start )
                      || ( Math.Abs( m_lastGeneratedPage.ViewportHeight - availableHeight ) < 1d );

      if( !generatePage )
        return m_lastGeneratedPage;

      var pageGenerator = new TopBottomPageGenerator( this, pageInfo.Start, availableHeight, forceMeasure );

      return this.GeneratePage( pageGenerator );
    }

    private TableViewPage GeneratePage( TableViewEndPageInfo pageInfo, double availableHeight, bool forceMeasure )
    {
      // The generator can be null if we're in design mode.
      var generator = this.CustomItemContainerGenerator;
      if( generator == null )
        return m_lastGeneratedPage;

      var pageGenerator = new BottomTopPageGenerator( this, pageInfo.End, availableHeight, forceMeasure );

      return this.GeneratePage( pageGenerator );
    }

    private TableViewPage GeneratePage( PageGenerator pageGenerator )
    {
      var pageResult = pageGenerator.Generate( m_layoutedContainers, m_lastGeneratedPage );

      // Replace the layouted containers.
      m_layoutedContainers.Clear();
      m_layoutedContainers.AddRange( pageResult.LayoutedContainers );
      m_layoutedContainers.Sort();

      m_lastGeneratedPage = pageResult.PageInfo;

      return m_lastGeneratedPage;
    }

    private void RecycleContainer( ICustomItemContainerGenerator generator, UIElement element, int realizedIndex, bool clearContainer = true )
    {
      if( !clearContainer )
      {
        var container = element as IDataGridItemContainer;
        if( container != null )
        {
          container.IsRecyclingCandidate = true;
        }
      }
      else
      {
        this.ClearContainer( element );
      }

      if( ( generator != null ) && ( realizedIndex >= 0 ) )
      {
        try
        {
          var position = generator.GeneratorPositionFromIndex( realizedIndex );
          generator.Remove( position, 1 );
        }
        catch
        {
          Debug.Fail( "Unable to remove container for realized index " + realizedIndex );
        }
      }
    }

    private void CleanRecyclingCandidates()
    {
      var generator = this.CustomItemContainerGenerator as CustomItemContainerGenerator;
      generator.CleanRecyclingCandidates();
    }

    private UIElement GenerateContainer( ICustomItemContainerGenerator generator, int index, bool forceMeasure )
    {
      bool isNewlyRealized;
      var container = ( UIElement )generator.GenerateNext( out isNewlyRealized );
      if( container == null )
        return null;

      if( isNewlyRealized )
      {
        var collection = this.Children;
        if( !collection.Contains( container ) )
        {
          collection.Add( container );
        }

        this.EnableElementNavigation( container );
        KeyboardNavigation.SetTabIndex( container, index );
        this.PrepareContainer( container );
        this.MeasureContainer( container );
      }
      else if( forceMeasure )
      {
        this.MeasureContainer( container );
      }

      return container;
    }

    private void DisableElementNavigation( UIElement child )
    {
      //get previous values and store them on the container (attached)
      TableViewItemsHost.SetPreviousDirectionalNavigationMode( child, KeyboardNavigation.GetDirectionalNavigation( child ) );
      TableViewItemsHost.SetPreviousTabNavigationMode( child, KeyboardNavigation.GetTabNavigation( child ) );

      KeyboardNavigation.SetDirectionalNavigation( child, KeyboardNavigationMode.None );
      KeyboardNavigation.SetTabNavigation( child, KeyboardNavigationMode.None );
    }

    private void EnableElementNavigation( UIElement child )
    {
      //checking only one of the 2 properties... This is because they are set together.
      if( child.ReadLocalValue( TableViewItemsHost.PreviousDirectionalNavigationModeProperty ) != DependencyProperty.UnsetValue )
      {
        KeyboardNavigation.SetDirectionalNavigation( child, TableViewItemsHost.GetPreviousDirectionalNavigationMode( child ) );
        KeyboardNavigation.SetTabNavigation( child, TableViewItemsHost.GetPreviousTabNavigationMode( child ) );
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
      this.DisableElementNavigation( container );
      this.FreeRowSelector( container );

      container.Visibility = Visibility.Collapsed;

      // The call to DataGridControl.ClearItemContainer will be done by the CustomItemContainerGenerator.
    }

    #endregion

    #region Scrolling Management

    internal void InvalidateScrollInfo()
    {
      var scrollOwner = this.ScrollInfo.ScrollOwner;
      if( scrollOwner == null )
        return;

      scrollOwner.InvalidateScrollInfo();
    }

    private void GeneratePageAndUpdateIScrollInfoValues()
    {
      if( this.CachedRootDataGridContext != null )
      {
        var generatedPage = this.GeneratePageAndUpdateIScrollInfoValues( m_lastMeasureAvailableSize, false );
        var newDesiredSize = this.GetNewDesiredSize( generatedPage );

        if( newDesiredSize != this.DesiredSize )
        {
          this.InvalidateMeasure();
        }
        else
        {
          this.InvalidateArrange();
        }
      }
    }

    private TableViewPage GeneratePageAndUpdateIScrollInfoValues( Size availableSize, bool forceMeasure )
    {
      // We must ensure the VerticalOffset is valid according
      // to the actual viewport height in case the VerticalOffset
      // is greater than the new viewportHeight.
      var scrollInfo = this.ScrollInfo;

      double maxOffset = Math.Max( 0d, scrollInfo.ExtentHeight - 1 );
      double offset = Math.Max( Math.Min( m_verticalOffset, maxOffset ), 0d );

      if( offset != m_verticalOffset )
      {
        this.SetVerticalOffsetCore( offset );
      }

      int verticalOffset = ( int )m_verticalOffset;
      TableViewPage generatedPage;

      if( m_indexToBringIntoView >= 0 )
      {
        int intOffset = Math.Min( m_indexToBringIntoView, ( int )maxOffset );

        if( ( m_lastLayoutedPage == TableViewPage.Empty ) || ( intOffset < m_lastLayoutedPage.InnerPage.Start ) )
        {
          generatedPage = this.GeneratePage( new TableViewStartPageInfo( intOffset ), availableSize.Height, forceMeasure );
        }
        else
        {
          generatedPage = this.GeneratePage( new TableViewEndPageInfo( intOffset ), availableSize.Height, forceMeasure );
        }
      }
      else
      {
        generatedPage = this.GeneratePage( new TableViewStartPageInfo( verticalOffset ), availableSize.Height, forceMeasure );
      }

      // CALCULATE THE EXTENT WIDTH
      m_extentWidth = Math.Max( this.GetMaxDesiredWidth(), this.GetSynchronizedExtentWidth() );

      // CALCULATE THE VIEWPORT WIDTH
      m_viewportWidth = Double.IsInfinity( availableSize.Width ) ? m_extentWidth : Math.Min( m_extentWidth, availableSize.Width );

      if( generatedPage != TableViewPage.Empty )
      {
        this.SetVerticalOffsetCore( generatedPage.InnerPage.Start );
      }

      this.InvalidateScrollInfo();

      return generatedPage;
    }

    private void ScrollByHorizontalOffset( double offset )
    {
      this.ScrollToHorizontalOffset( this.ScrollInfo.HorizontalOffset + offset );
    }

    private void ScrollToHorizontalOffset( double offset )
    {
      IScrollInfo scrollInfo = this.ScrollInfo;

      double maxOffset = Math.Max( scrollInfo.ExtentWidth - scrollInfo.ViewportWidth, 0 );
      offset = Math.Max( offset, 0 );
      offset = Math.Min( offset, maxOffset );

      if( scrollInfo.HorizontalOffset == offset )
        return;

      m_horizontalOffset = offset;
      this.InvalidateArrange();
      this.InvalidateScrollInfo();
    }

    private void ScrollByVerticalOffset( double offset )
    {
      this.ScrollToVerticalOffset( this.ScrollInfo.VerticalOffset + offset );
    }

    private void ScrollToVerticalOffset( double offset )
    {
      IScrollInfo scrollInfo = this.ScrollInfo;
      double maxOffset = Math.Max( scrollInfo.ExtentHeight - 1, 0 );
      offset = Math.Max( offset, 0 );
      offset = Math.Min( offset, maxOffset );

      if( scrollInfo.VerticalOffset == offset )
        return;

      this.SetVerticalOffsetCore( offset );
      this.InvalidateLayoutFromScrollingHelper();
    }

    private void SetVerticalOffsetCore( double offset )
    {
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
        container = this.GenerateContainer( generator, index, false );
      }

      isDataRow = false;

      if( container != null )
      {
        isDataRow = ( container is DataRow );

        if( m_layoutedContainers.IndexOfContainer( container ) < 0 )
        {
          m_layoutedContainers.Add( new LayoutedContainerInfo( index, container ) );
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

              if( cannotFocusCount > TableViewItemsHost.MaxDataRowFailFocusCount )
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

        if( m_lastGeneratedPage != TableViewPage.Empty )
        {
          // We will use MoveFocus if the focused index is currently in view.
          if( ( !currentChanged ) && ( focusedIndex >= m_lastGeneratedPage.InnerPage.Start ) && ( focusedIndex <= m_lastGeneratedPage.InnerPage.End ) )
          {
            currentChanged = this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Up ) );
          }
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

            if( cannotFocusCount > TableViewItemsHost.MaxDataRowFailFocusCount )
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

              if( cannotFocusCount > TableViewItemsHost.MaxDataRowFailFocusCount )
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

        if( m_lastGeneratedPage != TableViewPage.Empty )
        {
          // We will use MoveFocus if the focused index is currently in view.
          if( ( !currentChanged ) && ( focusedIndex >= m_lastGeneratedPage.InnerPage.Start ) && ( focusedIndex <= m_lastGeneratedPage.InnerPage.End ) )
          {
            currentChanged = this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Down ) );
          }
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

            if( cannotFocusCount > TableViewItemsHost.MaxDataRowFailFocusCount )
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

    protected override void OnItemsAdded()
    {
      // We are calling InvalidateMeasure to regenerate the current 
      // page and forcing the recycling of out-of-view containers.
      this.InvalidateMeasure();
    }

    protected override void OnItemsRemoved( IList<DependencyObject> containers )
    {
      // We are calling InvalidateMeasure to regenerate the current 
      // page and forcing the recycling of out-of-view containers.
      this.InvalidateMeasure();
    }

    protected override void OnItemsReset()
    {
      // We are calling InvalidateMeasure to regenerate the current 
      // page and forcing the recycling of out-of-view containers.
      this.InvalidateMeasure();
    }

    protected override void OnContainersRemoved( IList<DependencyObject> removedContainers )
    {
      foreach( UIElement container in removedContainers )
      {
        this.ClearContainer( container );

        // Avoid checking if Children already contains the element. No exception will be thrown if the item is not found.
        // This ensures to parse all children only one time.
        this.Children.Remove( container );

        int index = m_layoutedContainers.IndexOfContainer( container );
        if( index > -1 )
        {
          m_layoutedContainers.RemoveAt( index );
        }
      }
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
        this.ScrollInfo.ScrollOwner.PageUp();
        return;
      }

      int focusedContainerRealizedIndex = this.CustomItemContainerGenerator.GetRealizedIndexForContainer( focusedContainer );

      if( focusedContainerRealizedIndex == 0 )
      {
        this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Up ) );
      }
      else
      {
        this.InvalidateMeasure();

        var generatedPage = this.GeneratePage( new TableViewEndPageInfo( focusedContainerRealizedIndex ), this.RenderSize.Height, false );

        if( generatedPage != TableViewPage.Empty )
        {
          int initialDesiredIndex = generatedPage.InnerPage.Start;
          this.SetVerticalOffsetCore( initialDesiredIndex );

          if( focusedContainerRealizedIndex != initialDesiredIndex )
          {
            int desiredPageUpIndex = initialDesiredIndex;
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

            // No container were focused while processing indexes from focused to initialDesiredIndex, try SetCurrent on indexes samller than the initial up to 0
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
        this.ScrollInfo.ScrollOwner.PageDown();
        return;
      }

      int focusedContainerRealizedIndex = this.CustomItemContainerGenerator.GetRealizedIndexForContainer( focusedContainer );
      int maxIndex = this.CustomItemContainerGenerator.ItemCount - 1;

      if( focusedContainerRealizedIndex == maxIndex )
      {
        this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Down ) );
      }
      else
      {
        this.InvalidateMeasure();

        var generatedPage = this.GeneratePage( new TableViewStartPageInfo( focusedContainerRealizedIndex ), this.RenderSize.Height, false );

        if( generatedPage != TableViewPage.Empty )
        {
          this.SetVerticalOffsetCore( generatedPage.InnerPage.Start );
          int initialDesiredIndex;

          // Last row not totally visible, take the one before the last
          if( ( generatedPage.InnerPage.Size > this.RenderSize.Height ) && ( generatedPage.InnerPage.Length > 1 ) )
          {
            initialDesiredIndex = generatedPage.InnerPage.End - 1;
          }
          else
          {
            initialDesiredIndex = generatedPage.InnerPage.End;
          }

          if( focusedContainerRealizedIndex != initialDesiredIndex )
          {
            int desiredPageDownIndex = initialDesiredIndex;
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
      }
    }

    protected override void HandleHomeKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      if( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
      {
        DataGridControl dataGridControl = this.ParentDataGridControl;
        IScrollInfo scrollInfo = this.ScrollInfo;
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
        IScrollInfo scrollInfo = this.ScrollInfo;
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
      this.ScrollInfo.ScrollOwner.ScrollToTop();

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
      this.ScrollInfo.ScrollOwner.ScrollToBottom();

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

    #endregion

    #region ICustomVirtualizingPanel Members

    protected override void BringIntoViewCore( int index )
    {
      if( !this.DelayBringIntoView )
      {
        if( m_lastGeneratedPage == TableViewPage.Empty )
          return;

        var pageInfo = m_lastGeneratedPage.InnerPage;
        if( ( index >= pageInfo.Start ) && ( index < pageInfo.End ) )
          return;
      }

      this.InvalidateMeasure();

      m_indexToBringIntoView = index;
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
        return ( this.CachedRootDataGridContext != null ) ? this.CustomItemContainerGenerator.ItemCount : 0;
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

      this.ScrollToHorizontalOffset( offset );
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

      this.ScrollToVerticalOffset( offset );
    }

    double IScrollInfo.ViewportHeight
    {
      get
      {
        if( m_lastGeneratedPage == TableViewPage.Empty )
          return 0d;

        var pageInfo = m_lastGeneratedPage.InnerPage;
        var viewportHeight = m_lastGeneratedPage.ViewportHeight;

        if( pageInfo.Size > viewportHeight )
          return Math.Max( 1, pageInfo.Length - 1 );

        return Math.Max( 1, pageInfo.Length );
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
      this.ScrollByVerticalOffset( 1 );
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
      this.ScrollByVerticalOffset( -1 );
    }

    void IScrollInfo.MouseWheelDown()
    {
      this.ScrollByVerticalOffset( SystemParameters.WheelScrollLines );
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
      this.ScrollByVerticalOffset( SystemParameters.WheelScrollLines * -1 );
    }

    void IScrollInfo.PageDown()
    {
      if( m_lastGeneratedPage == TableViewPage.Empty )
        return;

      var pageInfo = m_lastGeneratedPage.InnerPage;
      var viewportHeight = m_lastGeneratedPage.ViewportHeight;
      var newOffset = pageInfo.End;

      // We only have one row and it is bigger than the view port.
      if( ( pageInfo.Length == 1 ) && ( pageInfo.Size > viewportHeight ) )
      {
        int itemCount = ( int )( ( IScrollInfo )this ).ExtentHeight;

        if( newOffset + 1 < itemCount )
        {
          newOffset++;
        }
      }

      this.SetVerticalOffsetCore( newOffset );
      this.GeneratePageAndUpdateIScrollInfoValues();
    }

    void IScrollInfo.PageLeft()
    {
      this.ScrollByHorizontalOffset( this.ScrollInfo.ViewportWidth * -1 );
    }

    void IScrollInfo.PageRight()
    {
      this.ScrollByHorizontalOffset( this.ScrollInfo.ViewportWidth );
    }

    void IScrollInfo.PageUp()
    {
      if( m_lastGeneratedPage == TableViewPage.Empty )
        return;

      var pageInfo = m_lastGeneratedPage.InnerPage;
      var viewportHeight = m_lastGeneratedPage.ViewportHeight;

      m_indexToBringIntoView = pageInfo.Start;

      // We only have one row and it is bigger than the view port.
      if( ( pageInfo.Length == 1 ) && ( pageInfo.Size > viewportHeight ) )
      {
        if( m_indexToBringIntoView > 0 )
        {
          m_indexToBringIntoView--;
        }
      }

      this.GeneratePageAndUpdateIScrollInfoValues();
    }

    Rect IScrollInfo.MakeVisible( Visual visual, Rect rectangle )
    {
      UIElement container = DataGridItemsHost.GetItemsHostContainerFromElement( this, visual );

      if( container != null )
      {
        Rect rectangleInItemsHost = visual.TransformToAncestor( this ).TransformBounds( rectangle );
        rectangle = visual.TransformToAncestor( container ).TransformBounds( rectangle );

        if( ( rectangleInItemsHost.Bottom > this.RenderSize.Height ) || ( rectangleInItemsHost.Top < 0 ) )
        {
          // Make sure that the item is vertically visible.
          ICustomItemContainerGenerator generator = this.CustomItemContainerGenerator;

          if( generator != null )
          {
            int itemIndex = generator.GetRealizedIndexForContainer( container );

            if( itemIndex != -1 )
            {
              // This will make sure to scroll to the right offsets.
              ICustomVirtualizingPanel virtualizingPanel = ( ICustomVirtualizingPanel )this;
              virtualizingPanel.BringIntoView( itemIndex );
            }
          }
        }
      }

      // Make sure that the item is horizontally visible.
      IScrollInfo scrollInfo = this.ScrollInfo;

      if( rectangle.Left < scrollInfo.HorizontalOffset )
      {
        this.ScrollToHorizontalOffset( rectangle.Left );
      }
      else if( rectangle.Right > scrollInfo.HorizontalOffset + scrollInfo.ViewportWidth )
      {
        this.ScrollToHorizontalOffset(
          Math.Min( rectangle.Left, ( rectangle.Right - scrollInfo.ViewportWidth ) ) );
      }

      return rectangle;
    }

    #endregion

    #region IDeferableScrollInfoRefresh Members

    IDisposable IDeferableScrollInfoRefresh.DeferScrollInfoRefresh( Orientation orientation )
    {
      if( orientation == Orientation.Vertical )
        return new LayoutSuspendedHelper( this );

      return null;
    }

    #endregion

    internal static bool ComputedCanScrollHorizontally( FrameworkElement target, DataGridItemsHost itemsHost )
    {
      Debug.Assert( target != null );

      bool retval = true;

      DependencyObject visual = target;

      do
      {
        retval = TableView.GetCanScrollHorizontally( visual );

        if( retval )
        {
          visual = TreeHelper.GetParent( visual );
        }
      }
      while( ( visual != null ) && ( visual != itemsHost ) && ( retval ) );

      return retval;
    }

    internal static double GetVisibleDimensionForRequestBringIntoViewTarget( double targetDimension, double targetToItemsHostOffset, double viewportDimension )
    {
      // The items left upper corner is in the Viewport
      if( targetToItemsHostOffset >= 0 )
      {
        // The item is totally visible since its actual dimension is less than the viewport
        if( ( targetToItemsHostOffset + targetDimension ) <= viewportDimension )
        {
          // This will force the function to "cancel" the BringIntoView
          // because the width to BringIntoView will be the viewport
          return viewportDimension;
        }
        else
        {
          // Item is not totally visible, calculate how much place it occupies in the viewport.
          return viewportDimension - targetToItemsHostOffset;
        }
      }
      else
      {
        // This items left upper corner is at the left side of the viewPort
        double itemActualVisibleValue = targetDimension - targetToItemsHostOffset;

        if( itemActualVisibleValue > viewportDimension )
        {
          // Limit the value of itemActualVisibleValue to the viewport size (to prevent eventual bugs [even if nothing is visible on the horizon for this])
          itemActualVisibleValue = viewportDimension;
        }

        return itemActualVisibleValue;
      }
    }

    internal static double CorrectBringIntoViewRectFromVisibleDimensionAndAcceptableThreshold(
      double actualVisibleDimension,
      double acceptableDimensionThreshold,
      double targetToItemsHostPosition,
      double targetToTemplatedParentDimension,
      double itemsHostOffset )
    {
      // The visible dimension is greater or equal than the acceptable threshold
      if( actualVisibleDimension >= acceptableDimensionThreshold )
      {
        return itemsHostOffset - targetToTemplatedParentDimension;
      }
      else
      {
        // Calculate the starting point of the rectangle to view. 
        return Math.Max( 0, targetToItemsHostPosition * -1 );
      }
    }

    internal static bool TargetRequiresBringIntoView( double targetToItemsHostPosition, double targetDesiredSizeDimension, double itemsHostRenderSizeDimension )
    {
      //If the Offset is negative, then it's sure its not totally visible
      if( targetToItemsHostPosition < 0 )
      {
        return true;
      }
      else if( ( targetDesiredSizeDimension < itemsHostRenderSizeDimension )
               && ( ( targetToItemsHostPosition + targetDesiredSizeDimension ) > itemsHostRenderSizeDimension ) )
      {
        return true;
      }

      return false;
    }

    internal static void ProcessRequestBringIntoView(
      DataGridItemsHost itemsHost,
      Orientation orientation,
      double stableScrollingProportion,
      RequestBringIntoViewEventArgs e )
    {
      IScrollInfo scrollInfo = itemsHost as IScrollInfo;

      if( scrollInfo == null )
        return;

      // Only perform this if we are virtualizing.  
      // If not, the ScrollViewer will take care of bringing the item into view.
      if( ( scrollInfo.ScrollOwner == null ) || ( !scrollInfo.ScrollOwner.CanContentScroll ) )
        return;

      // If we are explicitly setting the focus on a cell, we don't want to bring it into view.
      // A bringIntoView would cause the HorizontalOffset to change is not a wanted behavior.
      // Therefore, flag the Request as handled and do nothing.
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( itemsHost );

      DataGridControl dataGridControl = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      if( ( dataGridControl != null ) && ( dataGridControl.SettingFocusOnCell ) )
      {
        e.Handled = true;
        return;
      }

      // Only perform this if the object is NOT a Cell and was not passed a specific rectangle.
      if( ( e.TargetObject is Cell ) || ( e.TargetRect != Rect.Empty ) )
        return;

      // Before performing calculations, ensure that the element is a FrameworkElement.
      FrameworkElement target = e.TargetObject as FrameworkElement;

      if( ( target == null ) || ( !target.IsDescendantOf( itemsHost ) ) )
        return;

      // Mark the routed event as handled, since under even the worst circumstances, 
      // a new event will be raised.
      e.Handled = true;

      // Ensure to get the offset of the target visual element
      // according to its Container when it is not directly 
      // a Container. This is to avoid horizontal scrolling when
      // the NavigationBehavior is None and that the CellsHost
      // request the focus when there are HierarchicalGroupLevelIndicators
      // and/or GroupLevelIndicators present in the Container.
      // This extra space must be taken into consideration, not
      // only the offset between the target and the ItemsHost.
      Point targetToParentContainer = TableViewItemsHost.OriginPoint;
      FrameworkElement targetParentContainer = DataGridItemsHost.GetItemsHostContainerFromElement( itemsHost, target ) as FrameworkElement;

      if( ( targetParentContainer != null ) && ( target != targetParentContainer ) )
      {
        targetToParentContainer = target.TranslatePoint( TableViewItemsHost.OriginPoint,
                                                         targetParentContainer );
      }

      Point targetToItemsHostPosition = target.TranslatePoint( TableViewItemsHost.OriginPoint, itemsHost );
      double acceptableThreshold;
      double actualVisibleValue;

      //Calculate the VisibleWidth/Height of the object within the Viewport.
      if( orientation == Orientation.Vertical )
      {
        acceptableThreshold = scrollInfo.ViewportWidth * stableScrollingProportion;

        actualVisibleValue = TableViewItemsHost.GetVisibleDimensionForRequestBringIntoViewTarget(
          target.ActualWidth,
          targetToItemsHostPosition.X,
          scrollInfo.ViewportWidth );
      }
      else
      {
        acceptableThreshold = scrollInfo.ViewportHeight * stableScrollingProportion;

        actualVisibleValue = TableViewItemsHost.GetVisibleDimensionForRequestBringIntoViewTarget(
          target.ActualHeight,
          targetToItemsHostPosition.Y,
          scrollInfo.ViewportHeight );
      }

      Rect newRect = newRect = new Rect( 0, 0, target.ActualWidth, target.ActualHeight );

      // After the visible proportion of the object is calculated, compare it with threshold
      if( actualVisibleValue < acceptableThreshold )
      {
        // The required threshold is not visible, modify the bounds of the rectangle 
        // to bring the desired threshold.
        if( orientation == Orientation.Vertical )
        {
          if( targetToItemsHostPosition.X >= 0 )
          {
            newRect.Width = Math.Min( acceptableThreshold, target.ActualWidth );
          }
          else
          {
            newRect.X = Math.Max( 0, target.ActualWidth - acceptableThreshold );
          }
        }
        else
        {
          if( targetToItemsHostPosition.Y >= 0 )
          {
            newRect.Height = Math.Min( acceptableThreshold, target.ActualHeight );
          }
          else
          {
            newRect.Y = Math.Max( 0, target.ActualHeight - acceptableThreshold );
          }
        }

        target.BringIntoView( newRect );

        return;
      }

      bool needBringIntoView = false;

      // Determine if the item is totally or partially visible on the main scrolling axis.             
      if( orientation == Orientation.Vertical )
      {
        needBringIntoView = TableViewItemsHost.TargetRequiresBringIntoView(
          targetToItemsHostPosition.Y,
          target.DesiredSize.Height,
          itemsHost.RenderSize.Height );
      }
      else
      {
        needBringIntoView = TableViewItemsHost.TargetRequiresBringIntoView(
          targetToItemsHostPosition.X,
          target.DesiredSize.Width,
          itemsHost.RenderSize.Width );
      }

      // Extra properties that must be verified to conclude
      // the container must be put into view or not
      if( itemsHost is TableflowViewItemsHost )
      {
        // TableflowViewItemsHost must take Clip into consideration
        // since this means the container is not fully visible
        // and maybe under a Sticky container
        needBringIntoView |= ( target.GetValue( UIElement.ClipProperty ) != null );

        if( !needBringIntoView )
        {
          // If the container is sticky, we must ensure all subsequent containers 
          // wil be correctly layouted and visible. e.g.: the GroupHeaderControl 
          // hides the 1st group value and the 2nd one is partially visible.
          // If the container to bring into view is the sticky GroupHeaderControl,
          // the 1st and 2nd group values must be completely visible
          needBringIntoView |= TableflowViewItemsHost.GetIsSticky( target );
        }
      }

      if( !needBringIntoView )
        return;

      // The goal is to preserve the actual opposed axis scrolling
      if( orientation == Orientation.Vertical )
      {
        //if the item to be brough into view is part of the elements in a TableView that do no scroll
        if( ( dataGridControl != null )
          && ( dataGridControl.GetView() is TableView )
          && ( !TableViewItemsHost.ComputedCanScrollHorizontally( target, itemsHost ) ) )
        {
          // Nothing to do since the item can't scroll horizontally, use 0 as newRect.X
          // and ensure to use the container as BringIntoView target since it can't 
          // scroll horizontally
          if( targetToParentContainer != null )
          {
            target = targetParentContainer;

            // Ensure that the offset to bring into view
            // is the HorizontalOffset since it does not
            // scroll Horizontally.
            newRect.X = scrollInfo.HorizontalOffset;
          }
        }
        else
        {
          newRect.X = TableViewItemsHost.CorrectBringIntoViewRectFromVisibleDimensionAndAcceptableThreshold(
            actualVisibleValue,
            acceptableThreshold,
            targetToItemsHostPosition.X,
            targetToParentContainer.X,
            scrollInfo.HorizontalOffset );
        }

        // If the rectangle of the object goes beyond the Viewport
        if( newRect.Right > scrollInfo.ViewportWidth )
        {
          // Subtract what goes beyond!
          newRect.Width = newRect.Width
            - ( newRect.Width - scrollInfo.ViewportWidth )
            - Math.Max( targetToItemsHostPosition.X, 0 );
        }
      }
      else
      {
        newRect.Y = TableViewItemsHost.CorrectBringIntoViewRectFromVisibleDimensionAndAcceptableThreshold(
          actualVisibleValue,
          acceptableThreshold,
          targetToItemsHostPosition.Y,
          targetToParentContainer.Y,
          scrollInfo.VerticalOffset );

        // If the rectangle of the object goes beyond the Viewport
        if( newRect.Bottom > scrollInfo.ViewportHeight )
        {
          // Subtract what goes beyond!
          newRect.Height = newRect.Height
            - ( newRect.Height - scrollInfo.ViewportHeight )
            - Math.Max( targetToItemsHostPosition.Y, 0 );
        }
      }

      target.BringIntoView( newRect );
    }

    private static double MakeRectVisible(
     double rectMinimum,
     double rectMaximum,
     double rectDimension,
     double itemsHostViewport,
     double itemsHostOffset,
     out bool scrollRequired )
    {
      scrollRequired = true;

      // Check if the beginning of the rectangle is before the "offset"
      if( rectMinimum < itemsHostOffset )
      {
        // Then make the origin of the rectangle visible (independent of dimension)
        return rectMinimum;
      }
      //if the bottom of the rectangle is not visible
      else if( rectMaximum > ( itemsHostOffset + itemsHostViewport ) )
      {
        // And the rectangle is greater than viewport
        if( rectDimension > itemsHostViewport )
        {
          // Make the origin of the rectangle visible
          return rectMinimum;
        }
        else
        {
          //align the bottom of the rectangle at the bottom edge of the viewport.
          return rectMaximum - itemsHostViewport;
        }
      }

      scrollRequired = false;
      return 0;
    }

    private static void MoveFocusForwardExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      var itemsHost = ( TableViewItemsHost )sender;
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
      var itemsHost = ( TableViewItemsHost )sender;
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
      e.Handled = ( ( TableViewItemsHost )sender ).MoveFocusUp();
    }

    private static void MoveFocusUpCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = true;
      e.Handled = true;
    }

    private static void MoveFocusDownExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      e.Handled = ( ( TableViewItemsHost )sender ).MoveFocusDown();
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
        this.ScrollInfo.ScrollOwner.LineUp();
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
        this.ScrollInfo.ScrollOwner.LineDown();
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

      this.GeneratePageAndUpdateIScrollInfoValues();
    }

    private const int MaxDataRowFailFocusCount = 50;
    private const int NullIndex = int.MinValue;

    private static readonly Point OriginPoint = new Point( 0d, 0d );
    private static readonly Size EmptySize = new Size( 0d, 0d );
    private static readonly Rect OutOfViewRect = new Rect( new Point( -999999d, -999999d ), TableViewItemsHost.EmptySize );

    private readonly LayoutedContainerInfoList m_layoutedContainers = new LayoutedContainerInfoList();

    private TableViewPage m_lastGeneratedPage = TableViewPage.Empty;
    private TableViewPage m_lastLayoutedPage = TableViewPage.Empty;

    private readonly Dictionary<string, double> m_cachedContainerDesiredWidth = new Dictionary<string, double>();
    private readonly Dictionary<string, double> m_cachedContainerRealDesiredWidth = new Dictionary<string, double>();

    private readonly HashSet<string> m_autoWidthCalculatedDataGridContextList = new HashSet<string>();

    private Size m_lastMeasureAvailableSize = TableViewItemsHost.EmptySize;
    private int m_indexToBringIntoView = TableViewItemsHost.NullIndex;

    private AutoResetFlag m_layoutSuspended = AutoResetFlagFactory.Create( false );
    private bool m_layoutInvalidatedDuringSuspend;

    #region LayoutSuspendedHelper Private Class

    private sealed class LayoutSuspendedHelper : IDisposable
    {
      public LayoutSuspendedHelper( TableViewItemsHost owner )
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

      private TableViewItemsHost m_owner;
      private IDisposable m_disposable;
    }

    #endregion

    #region TableViewPageResult Private Class

    private sealed class TableViewPageResult
    {
      internal TableViewPageResult( TableViewPage pageInfo, IEnumerable<LayoutedContainerInfo> layoutedContainers )
      {
        if( pageInfo == null )
          throw new ArgumentNullException( "pageInfo" );

        if( layoutedContainers == null )
          throw new ArgumentNullException( "layoutedContainers" );

        m_pageInfo = pageInfo;
        m_layoutedContainers = layoutedContainers;
      }

      internal TableViewPage PageInfo
      {
        get
        {
          return m_pageInfo;
        }
      }

      internal IEnumerable<LayoutedContainerInfo> LayoutedContainers
      {
        get
        {
          return m_layoutedContainers;
        }
      }

      private readonly TableViewPage m_pageInfo;
      private readonly IEnumerable<LayoutedContainerInfo> m_layoutedContainers;
    }

    #endregion

    #region Range Private Struct

    private struct Range
    {
      internal static readonly Range Empty = new Range( true );

      private Range( bool isEmpty )
      {
        m_start = -1;
        m_end = -1;
      }

      internal Range( int start, int end )
      {
        if( start < 0 )
          throw new ArgumentException( "The value must be greater than or equal to zero.", "start" );

        if( end < 0 )
          throw new ArgumentException( "The value must be greater than or equal to zero.", "end" );

        if( start > end )
          throw new ArgumentException( "The start value must be lesser than or equal to the end value.", "start" );

        m_start = start;
        m_end = end;
      }

      public int Start
      {
        get
        {
          if( m_start < 0 )
            return 0;

          return m_start;
        }
      }

      public int End
      {
        get
        {
          if( m_end < 0 )
            return 0;

          return m_end;
        }
      }

      public bool IsEmpty
      {
        get
        {
          return ( m_start < 0 );
        }
      }

      public int Length
      {
        get
        {
          if( this.IsEmpty )
            return 0;

          return ( m_end - m_start + 1 );
        }
      }

      public static bool operator ==( Range x, Range y )
      {
        return object.Equals( x, y );
      }

      public static bool operator !=( Range x, Range y )
      {
        return !( x == y );
      }

      public override int GetHashCode()
      {
        return this.Start ^ this.End;
      }

      public override bool Equals( object obj )
      {
        if( !( obj is Range ) )
          return false;

        var range = ( Range )obj;

        if( this.IsEmpty )
          return range.IsEmpty;

        return ( !range.IsEmpty )
            && ( range.m_start == m_start )
            && ( range.m_end == m_end );
      }

      internal Range SetStart( int value )
      {
        if( this.IsEmpty )
          throw new InvalidOperationException();

        if( value > m_end )
          return Range.Empty;

        return new Range( value, m_end );
      }

      internal Range SetEnd( int value )
      {
        if( this.IsEmpty )
          throw new InvalidOperationException();

        if( value < m_start )
          return Range.Empty;

        return new Range( m_start, value );
      }

      private readonly int m_start;
      private readonly int m_end;
    }

    #endregion

    #region PageGenerator Private Class

    private abstract class PageGenerator
    {
      protected PageGenerator( TableViewItemsHost itemsHost, int startIndex, double availableHeight, bool forceMeasure )
      {
        if( itemsHost == null )
          throw new ArgumentNullException( "itemsHost" );

        m_itemsHost = itemsHost;
        m_startIndex = startIndex;
        m_availableHeight = availableHeight;
        m_forceMeasure = forceMeasure;
      }

      protected abstract TableViewPage GenerateContainers( int startIndex, int itemCount, double availableHeight, double extendedHeight, bool fillLastPage );

      protected LayoutedContainerInfo CreateContainer( int realizedIndex )
      {
        if( ( realizedIndex < 0 ) || ( realizedIndex >= m_itemCount ) )
          return null;

        this.RecycleCandidates( realizedIndex );

        var position = m_generator.GeneratorPositionFromIndex( realizedIndex );
        using( m_generator.StartAt( position, GeneratorDirection.Forward, true ) )
        {
          var container = m_itemsHost.GenerateContainer( m_generator, realizedIndex, m_forceMeasure );
          if( container == null )
            return null;

          m_remainingViewportHeight -= container.DesiredSize.Height;
          m_nonRecyclableContainers.Remove( container );
          m_oldLayoutedContainers.Remove( container );

          var containerInfo = new LayoutedContainerInfo( realizedIndex, container );
          m_newLayoutedContainers.Add( containerInfo );

          return containerInfo;
        }
      }

      internal TableViewPageResult Generate( IEnumerable<LayoutedContainerInfo> layoutedContainers, TableViewPage lastGeneratedPage )
      {
        m_generator = m_itemsHost.CustomItemContainerGenerator;
        Debug.Assert( m_generator != null );

        m_generator.IsRecyclingEnabled = true;

        // We call ItemCount before any other call to make sure the internal state of the generator is up-to-date.
        m_itemCount = m_generator.ItemCount;

        var lastGeneratedPageSize = lastGeneratedPage.OuterPage.Length;
        m_nonRecyclableContainers = new HashSet<UIElement>();
        m_oldLayoutedContainers = new Dictionary<UIElement, int>( lastGeneratedPageSize );
        m_newLayoutedContainers = null;

        // Figure out the realized index of all currently layouted containers.
        foreach( var container in layoutedContainers.Select( item => item.Container ) )
        {
          var newRealizedIndex = m_generator.GetRealizedIndexForContainer( container );

          if( !m_itemsHost.CanRecycleContainer( container ) )
          {
            m_nonRecyclableContainers.Add( container );
            m_oldLayoutedContainers.Add( container, newRealizedIndex );
          }
          else if( newRealizedIndex >= 0 )
          {
            m_oldLayoutedContainers.Add( container, newRealizedIndex );
          }
          // The container is lost, recycle it.
          else
          {
            m_itemsHost.RecycleContainer( m_generator, container, newRealizedIndex );
          }
        }

        TableViewPageResult result;

        if( m_itemCount > 0 )
        {
          m_newLayoutedContainers = new List<LayoutedContainerInfo>( lastGeneratedPageSize );

          var extendedHeight = Math.Max( m_availableHeight, lastGeneratedPage.ViewportHeight );

          this.PrepareRecycleCandidates( extendedHeight );

          var dataGridContext = DataGridControl.GetDataGridContext( m_itemsHost );
          var fillLastPage = ( dataGridContext != null ) && TableView.GetAutoFillLastPage( dataGridContext );
          var pageInfo = this.GenerateContainers( m_startIndex, m_itemCount, m_availableHeight, extendedHeight, fillLastPage );

          // Keep active the containers that cannot be recycled by putting them in the layouted containers collection.
          foreach( var entry in m_nonRecyclableContainers )
          {
            int realizedIndex;
            if( !m_oldLayoutedContainers.TryGetValue( entry, out realizedIndex ) )
              continue;

            m_oldLayoutedContainers.Remove( entry );
            m_newLayoutedContainers.Add( new LayoutedContainerInfo( realizedIndex, entry ) );
          }

          result = new TableViewPageResult( pageInfo, m_newLayoutedContainers );
        }
        else
        {
          result = new TableViewPageResult( TableViewPage.Empty, Enumerable.Empty<LayoutedContainerInfo>() );
        }

        // Recycle the containers that have not been reused.
        foreach( var entry in m_oldLayoutedContainers )
        {
          m_itemsHost.RecycleContainer( m_generator, entry.Key, entry.Value );
        }

        m_itemsHost.CleanRecyclingCandidates();

        return result;
      }

      private void PrepareRecycleCandidates( double extendedHeight )
      {
        m_recycleCandidates = ( from entry in m_oldLayoutedContainers
                                orderby entry.Value
                                select entry.Key ).ToArray();

        var count = m_recycleCandidates.Length;

        m_recycleCandidatesRealizedIndex = new int[ count ];
        m_recycleCandidatesHeight = new DoubleFenwickTree( count );

        for( int i = 0; i < count; i++ )
        {
          var container = m_recycleCandidates[ i ];

          m_recycleCandidatesRealizedIndex[ i ] = m_oldLayoutedContainers[ container ];
          m_recycleCandidatesHeight[ i ] = container.DesiredSize.Height;
        }

        if( count > 0 )
        {
          m_lowerRecycleCandidatesRange = new Range( 0, count - 1 );
          m_upperRecycleCandidatesRange = new Range( 0, count - 1 );
        }
        else
        {
          m_lowerRecycleCandidatesRange = Range.Empty;
          m_upperRecycleCandidatesRange = Range.Empty;
        }

        m_remainingViewportHeight = extendedHeight;
      }

      private void RecycleCandidates( int realizedIndex )
      {
        if( m_lowerRecycleCandidatesRange.IsEmpty && m_upperRecycleCandidatesRange.IsEmpty )
          return;

        var pivot = Array.BinarySearch( m_recycleCandidatesRealizedIndex, realizedIndex );
        if( pivot < 0 )
        {
          pivot = ~pivot;

          if( !m_upperRecycleCandidatesRange.IsEmpty && ( m_upperRecycleCandidatesRange.Start < pivot ) )
          {
            m_upperRecycleCandidatesRange = m_upperRecycleCandidatesRange.SetStart( pivot );
          }
        }
        else
        {
          if( !m_upperRecycleCandidatesRange.IsEmpty && ( m_upperRecycleCandidatesRange.Start <= pivot ) )
          {
            m_upperRecycleCandidatesRange = m_upperRecycleCandidatesRange.SetStart( pivot + 1 );
          }
        }

        if( !m_lowerRecycleCandidatesRange.IsEmpty && ( m_lowerRecycleCandidatesRange.End >= pivot ) )
        {
          m_lowerRecycleCandidatesRange = m_lowerRecycleCandidatesRange.SetEnd( pivot - 1 );
        }

        while( !m_lowerRecycleCandidatesRange.IsEmpty )
        {
          var index = m_lowerRecycleCandidatesRange.Start;
          var container = m_recycleCandidates[ index ];

          // The container may no longer be a candidate if it has been reused already.
          if( m_oldLayoutedContainers.ContainsKey( container ) )
          {
            var distance = ( m_lowerRecycleCandidatesRange.Length > 1 )
                             ? m_recycleCandidatesHeight.GetRunningSum( index + 1, m_lowerRecycleCandidatesRange.End )
                             : 0d;

            if( distance < m_remainingViewportHeight )
              break;

            this.RecycleCandidate( container );
          }

          m_lowerRecycleCandidatesRange = m_lowerRecycleCandidatesRange.SetStart( index + 1 );
        }

        while( !m_upperRecycleCandidatesRange.IsEmpty )
        {
          var index = m_upperRecycleCandidatesRange.End;
          var container = m_recycleCandidates[ index ];

          // The container may no longer be a candidate if it has been reused already.
          if( m_oldLayoutedContainers.ContainsKey( container ) )
          {
            var distance = ( m_upperRecycleCandidatesRange.Length > 1 )
                             ? m_recycleCandidatesHeight.GetRunningSum( m_upperRecycleCandidatesRange.Start, index - 1 )
                             : 0d;

            if( distance < m_remainingViewportHeight )
              break;

            this.RecycleCandidate( container );
          }

          m_upperRecycleCandidatesRange = m_upperRecycleCandidatesRange.SetEnd( index - 1 );
        }
      }

      private void RecycleCandidate( UIElement container )
      {
        //Do not recycle the focused container
        if( m_nonRecyclableContainers.Contains( container ) )
          return;

        int realizedIndex;
        if( !m_oldLayoutedContainers.TryGetValue( container, out realizedIndex ) )
          return;

        m_oldLayoutedContainers.Remove( container );
        m_itemsHost.RecycleContainer( m_generator, container, realizedIndex, false );
      }

      private readonly TableViewItemsHost m_itemsHost;
      private readonly int m_startIndex;
      private readonly double m_availableHeight;
      private readonly bool m_forceMeasure;

      private ICustomItemContainerGenerator m_generator;
      private int m_itemCount;
      private HashSet<UIElement> m_nonRecyclableContainers;
      private Dictionary<UIElement, int> m_oldLayoutedContainers;
      private List<LayoutedContainerInfo> m_newLayoutedContainers;

      private UIElement[] m_recycleCandidates;
      private int[] m_recycleCandidatesRealizedIndex;
      private DoubleFenwickTree m_recycleCandidatesHeight;
      private Range m_lowerRecycleCandidatesRange;
      private Range m_upperRecycleCandidatesRange;
      private double m_remainingViewportHeight;
    }

    #endregion

    #region TopBottomPageGenerator Private Class

    private sealed class TopBottomPageGenerator : PageGenerator
    {
      internal TopBottomPageGenerator( TableViewItemsHost itemsHost, int startIndex, double availableHeight, bool forceMeasure )
        : base( itemsHost, startIndex, availableHeight, forceMeasure )
      {
      }

      protected override TableViewPage GenerateContainers( int startIndex, int itemCount, double availableHeight, double extendedHeight, bool fillLastPage )
      {
        var innerStartIndex = startIndex;
        var innerEndIndex = startIndex;
        var innerContainersHeight = 0d;
        var outerStartIndex = startIndex;
        var outerEndIndex = startIndex;
        var outerContainersHeight = 0d;
        var remainingInnerContainersHeight = availableHeight;
        var remainingOuterContainersHeight = extendedHeight;

        Debug.Assert( remainingInnerContainersHeight <= remainingOuterContainersHeight );

        for( int i = innerStartIndex; ( remainingOuterContainersHeight > 0d ) && ( i < itemCount ); i++ )
        {
          var containerInfo = this.CreateContainer( i );
          if( containerInfo == null )
            break;

          var containerHeight = containerInfo.Container.DesiredSize.Height;

          if( remainingInnerContainersHeight > 0d )
          {
            innerEndIndex = i;
            innerContainersHeight += containerHeight;
            remainingInnerContainersHeight -= containerHeight;
          }

          outerEndIndex = i;
          outerContainersHeight += containerHeight;
          remainingOuterContainersHeight -= containerHeight;
        }

        if( fillLastPage )
        {
          for( int i = innerStartIndex - 1; ( remainingOuterContainersHeight > 0d ) && ( i >= 0 ); i-- )
          {
            var containerInfo = this.CreateContainer( i );
            if( containerInfo == null )
              break;

            var containerHeight = containerInfo.Container.DesiredSize.Height;

            if( containerHeight <= remainingInnerContainersHeight )
            {
              innerStartIndex = i;
              innerContainersHeight += containerHeight;
              remainingInnerContainersHeight -= containerHeight;
            }

            outerStartIndex = i;
            outerContainersHeight += containerHeight;
            remainingOuterContainersHeight -= containerHeight;
          }
        }

        return new TableViewPage(
          new TableViewFullPageInfo( innerStartIndex, innerEndIndex, innerContainersHeight ),
          new TableViewFullPageInfo( outerStartIndex, outerEndIndex, outerContainersHeight ),
          availableHeight );
      }
    }

    #endregion

    #region BottomTopPageGenerator Private Class

    private sealed class BottomTopPageGenerator : PageGenerator
    {
      internal BottomTopPageGenerator( TableViewItemsHost itemsHost, int startIndex, double availableHeight, bool forceMeasure )
        : base( itemsHost, startIndex, availableHeight, forceMeasure )
      {
      }

      protected override TableViewPage GenerateContainers( int startIndex, int itemCount, double availableHeight, double extendedHeight, bool fillLastPage )
      {
        var innerStartIndex = startIndex;
        var innerEndIndex = startIndex;
        var innerContainersHeight = 0d;
        var outerStartIndex = startIndex;
        var outerEndIndex = startIndex;
        var outerContainersHeight = 0d;
        var remainingInnerContainersHeight = availableHeight;
        var remainingOuterContainersHeight = extendedHeight;

        Debug.Assert( remainingInnerContainersHeight <= remainingOuterContainersHeight );

        for( int i = innerEndIndex; ( remainingOuterContainersHeight > 0d ) && ( i >= 0 ); i-- )
        {
          var containerInfo = this.CreateContainer( i );
          if( containerInfo == null )
            break;

          var containerHeight = containerInfo.Container.DesiredSize.Height;

          outerStartIndex = i;
          outerContainersHeight += containerHeight;

          if( containerHeight > remainingInnerContainersHeight )
          {
            // The inner page must at least contain one container before we consider layouting in the other direction.
            if( innerContainersHeight != 0d )
              break;
          }

          innerStartIndex = i;
          innerContainersHeight += containerHeight;
          remainingInnerContainersHeight -= containerHeight;
          remainingOuterContainersHeight -= containerHeight;
        }

        for( int i = innerEndIndex + 1; ( remainingOuterContainersHeight > 0d ) && ( i < itemCount ); i++ )
        {
          var containerInfo = this.CreateContainer( i );
          if( containerInfo == null )
            break;

          var containerHeight = containerInfo.Container.DesiredSize.Height;

          if( remainingInnerContainersHeight > 0d )
          {
            innerEndIndex = i;
            innerContainersHeight += containerHeight;
            remainingInnerContainersHeight -= containerHeight;
          }

          outerEndIndex = i;
          outerContainersHeight += containerHeight;
          remainingOuterContainersHeight -= containerHeight;
        }

        return new TableViewPage(
          new TableViewFullPageInfo( innerStartIndex, innerEndIndex, innerContainersHeight ),
          new TableViewFullPageInfo( outerStartIndex, outerEndIndex, outerContainersHeight ),
          availableHeight );
      }
    }

    #endregion
  }
}
