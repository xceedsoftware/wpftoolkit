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
  public class TableViewItemsHost : DataGridItemsHost, IScrollInfo, IDeferableScrollInfoRefresh
  {
    #region Static Members

    private const int MaxDataRowFailFocusCount = 50;

    #endregion

    #region Constructors

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

    #endregion CONSTRUCTORS

    #region Orientation Property

    [Obsolete( "The Orientation property is obsolete. Only a vertical orientation is supported.", false )]
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register( "Orientation", typeof( Orientation ), typeof( TableViewItemsHost ), new UIPropertyMetadata( Orientation.Vertical ) );

    [Obsolete( "The Orientation property is obsolete. Only a vertical orientation is supported.", false )]
    public Orientation Orientation
    {
      get
      {
        return ( Orientation )this.GetValue( TableViewItemsHost.OrientationProperty );
      }
      set
      {
        this.SetValue( TableViewItemsHost.OrientationProperty, value );
      }
    }

    #endregion Orientation Property

    #region StableScrollingEnabled Property

    [Obsolete( "The StableScrollingEnabled property is obsolete.", false )]
    public static readonly DependencyProperty StableScrollingEnabledProperty =
        DependencyProperty.Register( "StableScrollingEnabled", typeof( bool ), typeof( TableViewItemsHost ), new UIPropertyMetadata( true ) );

    [Obsolete( "The StableScrollingEnabled property is obsolete.", false )]
    public bool StableScrollingEnabled
    {
      get
      {
        return ( bool )this.GetValue( TableViewItemsHost.StableScrollingEnabledProperty );
      }
      set
      {
        this.SetValue( TableViewItemsHost.StableScrollingEnabledProperty, value );
      }
    }

    #endregion StableScrollingEnabled Property

    #region StableScrollingProportion Property

    [Obsolete( "The StableScrollingProportion property is obsolete.", false )]
    public static readonly DependencyProperty StableScrollingProportionProperty =
        DependencyProperty.Register( "StableScrollingProportion", typeof( double ), typeof( TableViewItemsHost ), new UIPropertyMetadata( 0.5d ) );

    [Obsolete( "The StableScrollingProportion property is obsolete.", false )]
    public double StableScrollingProportion
    {
      get
      {
        return ( double )this.GetValue( TableViewItemsHost.StableScrollingProportionProperty );
      }
      set
      {
        this.SetValue( TableViewItemsHost.StableScrollingProportionProperty, value );
      }
    }

    #endregion StableScrollingProportion Property

    #region ScrollInfo Property

    internal IScrollInfo ScrollInfo
    {
      get
      {
        return ( IScrollInfo )this;
      }
    }

    #endregion ScrollInfo Property

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

    #endregion PreviousTabNavigationMode ( private attached property )

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

    #endregion PreviousDirectionalNavigationMode ( private attached property )

    #region RowSelectorPane Property

    private RowSelectorPane RowSelectorPane
    {
      get
      {
        TableViewScrollViewer tableViewScrollViewer = this.ScrollInfo.ScrollOwner as TableViewScrollViewer;
        return ( tableViewScrollViewer == null ) ? null : tableViewScrollViewer.RowSelectorPane;
      }
    }

    #endregion RowSelectorPane Property

    #region Measure/Arrange Methods

    protected override Size MeasureOverride( Size availableSize )
    {
      this.InvalidateAutomationPeerChildren();

      m_cachedContainerDesiredWidth.Clear();
      m_cachedContainerRealDesiredWidth.Clear();
      m_autoWidthCalculatedDataGridContextList.Clear();
      m_lastMeasureAvailableSize = availableSize;

      double availableHeight = availableSize.Height;
      double viewportHeight = availableHeight;

      this.GeneratePageAndUpdateIScrollInfoValues( availableSize, true, ref viewportHeight );

      return this.GetNewDesiredSize( viewportHeight );
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

      this.LayoutContainers();

      m_lastLayoutedPage = m_lastGeneratedPage;
      m_indexToBringIntoView = -1;

      return finalSize;
    }

    private void ArrangeContainer( UIElement container, Point translationPoint, bool rowSelectorPaneVisible )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( container );
      string dataGridContextName = this.GetDataGridContextName( container, dataGridContext );

      Size containerSize = new Size(
        this.GetContainerWidth( dataGridContextName ),
        container.DesiredSize.Height );

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
      DataGridScrollViewer dgScrollViewer = this.ScrollInfo.ScrollOwner as DataGridScrollViewer;

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

    private Size GetNewDesiredSize( double viewportHeight )
    {
      double availableHeight = m_lastMeasureAvailableSize.Height;

      if( m_lastGeneratedPage.Length < this.ScrollInfo.ExtentHeight )
        return new Size( m_viewportWidth, availableHeight );

      return new Size( m_viewportWidth, ( double.IsInfinity( availableHeight ) ? viewportHeight : Math.Min( availableHeight, viewportHeight ) ) );
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

    private void GeneratePage(
      double availableHeight,
      bool measureInvalidated,
      ref PageIndexes generatedPage,
      out double containersHeight )
    {
      containersHeight = 0d;
      ICustomItemContainerGenerator generator = this.CustomItemContainerGenerator;

      // The generator can be null if we're in design mode.
      if( generator == null )
        return;

      // Make sure that container recycling is currently enabled on the generator.
      generator.IsRecyclingEnabled = true;

      bool pageChanged =
        ( generatedPage.StartIndex != m_lastGeneratedPage.StartIndex )
        || ( m_lastGeneratedPageViewPortHeight != availableHeight );

      if( ( pageChanged ) || ( measureInvalidated ) )
      {
        UIElement focusedContainer = DataGridItemsHost.GetItemsHostContainerFromElement( this, Keyboard.FocusedElement as DependencyObject );
        HashSet<UIElement> layoutedContainersToRecycle = new HashSet<UIElement>();
        int newPageLengthApproximation = Math.Max( 1, m_lastGeneratedPage.Length );

        foreach( LayoutedContainerInfo containerInfo in m_layoutedContainers )
        {
          int realizedIndex = containerInfo.RealizedIndex;
          UIElement container = containerInfo.Container;
          bool waitForRecycle = ( ( generatedPage.StartIndex != -1 ) && ( realizedIndex >= generatedPage.StartIndex ) && ( realizedIndex <= generatedPage.StartIndex + newPageLengthApproximation ) )
                             || ( ( generatedPage.EndIndex != -1 ) && ( realizedIndex >= generatedPage.EndIndex - newPageLengthApproximation ) && ( realizedIndex <= generatedPage.EndIndex ) );

          // Mark the container has a candidate for recycling.
          if( ( waitForRecycle ) || ( container == focusedContainer ) )
          {
            layoutedContainersToRecycle.Add( container );
          }
          // The element will probably not be on the generated page.  Recycle its container immediatly
          // to minimize the number of new containers created.
          else
          {
            this.TrySafeRecycleContainer( generator, realizedIndex, container );

            m_layoutedContainersToRecycle.Add( container );
          }
        }

        m_layoutedContainers.Clear();

        this.GenerateContainers( generator, availableHeight, layoutedContainersToRecycle, measureInvalidated, ref generatedPage, out containersHeight );

        // We do not recycle the focused element!
        if( layoutedContainersToRecycle.Contains( focusedContainer ) )
        {
          layoutedContainersToRecycle.Remove( focusedContainer );
          m_layoutedContainersToRecycle.Remove( focusedContainer );
          m_layoutedContainers.Add( new LayoutedContainerInfo( generator.GetRealizedIndexForContainer( focusedContainer ), focusedContainer ) );
        }

        // Recycle the containers for the current page.
        this.RecycleContainers( layoutedContainersToRecycle, generator );

        foreach( UIElement container in layoutedContainersToRecycle )
        {
          m_layoutedContainersToRecycle.Add( container );
        }

        m_lastGeneratedPage = generatedPage;
        m_lastGeneratedPageViewPortHeight = availableHeight;
        m_lastGeneratedPageContainersHeight = containersHeight;
      }
      else
      {
        generatedPage = m_lastGeneratedPage;
        containersHeight = m_lastGeneratedPageContainersHeight;
      }
    }

    private void RecycleContainers(
      HashSet<UIElement> layoutedContainersToRecycle,
      ICustomItemContainerGenerator generator )
    {
      foreach( UIElement containerToRecycle in layoutedContainersToRecycle )
      {
        this.RecycleContainer( generator, generator.GetRealizedIndexForContainer( containerToRecycle ), containerToRecycle );
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

      this.DisableElementNavigation( container );
      this.FreeRowSelector( container );
    }

    private void TrySafeRecycleContainer( ICustomItemContainerGenerator generator, int containerIndex, UIElement container )
    {
      int newIndex = -1;

      if( ( generator != null ) && ( containerIndex != -1 ) )
      {
        GeneratorPosition position = generator.GeneratorPositionFromIndex( containerIndex );
        if( ( position.Index != -1 ) && ( position.Offset == 0 ) )
        {
          newIndex = containerIndex;
        }
      }

      this.RecycleContainer( generator, newIndex, container );
    }

    private void GenerateContainers(
      ICustomItemContainerGenerator generator,
      double pageHeight,
      HashSet<UIElement> layoutedContainersToRecycle,
      bool measureInvalidated,
      ref PageIndexes pageIndexes,
      out double containersHeight )
    {
      int currentIndex = pageIndexes.StartIndex;
      GeneratorDirection direction;

      if( currentIndex == -1 )
      {
        currentIndex = pageIndexes.EndIndex;
        Debug.Assert( currentIndex != -1 );
        direction = GeneratorDirection.Backward;
      }
      else
      {
        direction = GeneratorDirection.Forward;
      }

      int startIndex = currentIndex;
      int endIndex = currentIndex;
      containersHeight = 0d;
      GeneratorPosition position;

      position = generator.GeneratorPositionFromIndex( currentIndex );
      int itemCount = generator.ItemCount;

      using( generator.StartAt( position, direction, true ) )
      {
        while( ( ( direction == GeneratorDirection.Forward )
          ? ( currentIndex < itemCount ) : ( currentIndex >= 0 ) ) && ( pageHeight > 0 ) )
        {
          UIElement container = this.GenerateContainer( generator, currentIndex, measureInvalidated );

          if( container == null )
            break;

          double containerHeight = container.DesiredSize.Height;

          m_layoutedContainers.Add( new LayoutedContainerInfo( currentIndex, container ) );
          layoutedContainersToRecycle.Remove( container );
          m_layoutedContainersToRecycle.Remove( container );

          if( ( direction == GeneratorDirection.Backward ) && ( ( pageHeight - containerHeight ) < 0 ) )
          {
            // We do not want to recycle the container since it will cause a re-invalidation of the measure and 
            // may cause an infinit loop.  This case has been observed with a MaxHeight set on the DataGridControl.
            break;
          }

          pageHeight -= containerHeight;
          containersHeight += containerHeight;
          endIndex = currentIndex;
          currentIndex += ( direction == GeneratorDirection.Forward ) ? 1 : -1;
        }
      }

      if( pageHeight > 0 )
      {
        if( direction == GeneratorDirection.Forward )
        {
          direction = GeneratorDirection.Backward;
        }
        else
        {
          direction = GeneratorDirection.Forward;
        }

        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

        if( ( direction == GeneratorDirection.Forward ) || ( ( dataGridContext == null ) || ( TableView.GetAutoFillLastPage( dataGridContext ) ) ) )
        {
          currentIndex = ( direction == GeneratorDirection.Forward ) ? startIndex + 1 : startIndex - 1;

          if( ( direction == GeneratorDirection.Forward ) ? ( currentIndex < itemCount ) : ( currentIndex >= 0 ) )
          {
            position = generator.GeneratorPositionFromIndex( currentIndex );

            using( generator.StartAt( position, direction, true ) )
            {
              // If we still have more space, try to get more container to fill up the page.
              while( ( ( direction == GeneratorDirection.Forward ) ?
                ( currentIndex < itemCount ) : ( currentIndex >= 0 ) ) && ( pageHeight > 0 ) )
              {
                UIElement container = this.GenerateContainer( generator, currentIndex, measureInvalidated );

                if( container == null )
                  break;

                double containerHeight = container.DesiredSize.Height;
                pageHeight -= containerHeight;

                m_layoutedContainers.Add( new LayoutedContainerInfo( currentIndex, container ) );
                layoutedContainersToRecycle.Remove( container );
                m_layoutedContainersToRecycle.Remove( container );

                if( ( direction == GeneratorDirection.Backward ) && ( pageHeight < 0 ) )
                {
                  // We do not want to recycle the container since it will cause a re-invalidation of the measure and 
                  // may cause an infinit loop.  This case has been observed with a MaxHeight set on the DataGridControl.
                  break;
                }

                containersHeight += containerHeight;
                startIndex = currentIndex;
                currentIndex += ( direction == GeneratorDirection.Forward ) ? 1 : -1;
              }
            }
          }
        }
      }

      m_layoutedContainers.Sort();

      if( startIndex > endIndex )
      {
        pageIndexes = new PageIndexes( endIndex, startIndex );
      }
      else
      {
        pageIndexes = new PageIndexes( startIndex, endIndex );
      }
    }

    private UIElement GenerateContainer( ICustomItemContainerGenerator generator, int index, bool measureInvalidated )
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
          this.PrepareContainer( container );
        }

        if( ( isNewlyRealized ) || ( measureInvalidated ) )
        {
          this.MeasureContainer( container );
        }
      }

      return container;
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
      this.LayoutContainers( rowSelectorPaneVisible );
      CommandManager.InvalidateRequerySuggested();

      // We must not call Mouse.Synchronize if we are currently dragging rows. 
      // Update the mouse status to make sure no container has invalid mouse over status.
      // Only do this when the mouse is over the panel, to prevent unescessary update when scrolling with thumb
      if( ( this.ParentDataGridControl.DragDataObject == null ) && ( this.IsMouseOver ) )
      {
        Mouse.Synchronize();
      }
    }

    private void LayoutContainers( bool rowSelectorPaneVisible )
    {
      int count = m_layoutedContainers.Count;
      double currentOffset = 0;

      // Layout out of view the recycled containers.
      foreach( UIElement container in m_layoutedContainersToRecycle )
      {
        this.ArrangeContainer( container, TableViewItemsHost.OutOfViewPoint, false );
      }

      m_layoutedContainersToRecycle.Clear();
      m_layoutedContainersToRecycle.TrimExcess();

      for( int i = 0; i < count; i++ )
      {
        LayoutedContainerInfo layoutedContainerInfo = m_layoutedContainers[ i ];
        UIElement container = layoutedContainerInfo.Container;
        Point translationPoint;

        if( ( layoutedContainerInfo.RealizedIndex < m_lastGeneratedPage.StartIndex )
          || ( layoutedContainerInfo.RealizedIndex > m_lastGeneratedPage.EndIndex ) )
        {
          translationPoint = new Point( -m_horizontalOffset, TableViewItemsHost.OutOfViewPoint.Y );
          this.ArrangeContainer( container, translationPoint, rowSelectorPaneVisible );
        }
        else
        {
          translationPoint = new Point( -m_horizontalOffset, currentOffset );
          this.ArrangeContainer( container, translationPoint, rowSelectorPaneVisible );
          currentOffset += container.RenderSize.Height;
        }
      }
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
      object dataItem = container.GetValue( Xceed.Wpf.DataGrid.CustomItemContainerGenerator.DataItemPropertyProperty );

      if( dataItem != null )
      {
        // Prepare the container.
        this.ParentDataGridControl.PrepareItemContainer( container, dataItem );
      }
    }

    #endregion Containers Methods

    #region Scrolling Management

    internal void InvalidateScrollInfo()
    {
      ScrollViewer scrollOwner = this.ScrollInfo.ScrollOwner;

      if( scrollOwner != null )
      {
        scrollOwner.InvalidateScrollInfo();
      }
    }

    private void GeneratePageAndUpdateIScrollInfoValues()
    {
      Size availableSize = m_lastMeasureAvailableSize;
      double viewportHeight = availableSize.Height;

      this.GeneratePageAndUpdateIScrollInfoValues( availableSize, false, ref viewportHeight );

      Size newDesiredSize = this.GetNewDesiredSize( viewportHeight );
      if( newDesiredSize != this.DesiredSize )
      {
        this.InvalidateMeasure();
      }
      else
      {
        this.InvalidateArrange();
      }
    }

    private void GeneratePageAndUpdateIScrollInfoValues(
      Size availableSize,
      bool measureInvalidated,
      ref double viewportHeight )
    {
      PageIndexes generatedPage;

      // We must ensure the VerticalOffset is valid according
      // to the actual viewport height in case the VerticalOffset
      // is greater than the new viewportHeight.
      IScrollInfo scrollInfo = this as IScrollInfo;

      double maxOffset = Math.Max( 0d, scrollInfo.ExtentHeight - 1 );
      double offset = Math.Max( Math.Min( m_verticalOffset, maxOffset ), 0d );

      if( offset != m_verticalOffset )
        this.SetVerticalOffsetCore( offset );

      int verticalOffset = ( int )m_verticalOffset;

      if( m_indexToBringIntoView != -1 )
      {
        int intOffset = Math.Min( m_indexToBringIntoView, ( int )maxOffset );

        if( intOffset < m_lastLayoutedPage.StartIndex )
        {
          generatedPage = new PageIndexes( intOffset, -1 );
        }
        else
        {
          generatedPage = new PageIndexes( -1, intOffset );
        }
      }
      else
      {
        generatedPage = new PageIndexes( verticalOffset, -1 );
      }

      // CALCULATE THE VIEWPORT HEIGHT AND GENERATE CONTAINERS
      this.GeneratePage( availableSize.Height, measureInvalidated, ref generatedPage, out viewportHeight );

      // CALCULATE THE EXTENT WIDTH
      m_extentWidth = Math.Max( this.GetMaxDesiredWidth(), this.GetSynchronizedExtentWidth() );

      // CALCULATE THE VIEWPORT WIDTH
      m_viewportWidth = Double.IsInfinity( availableSize.Width )
        ? m_extentWidth : Math.Min( m_extentWidth, availableSize.Width );

      this.SetVerticalOffsetCore( generatedPage.StartIndex );
      this.InvalidateScrollInfo();
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
      ICustomItemContainerGenerator generator = this.CustomItemContainerGenerator;
      GeneratorPosition position = generator.GeneratorPositionFromIndex( index );
      UIElement container = null;

      using( generator.StartAt( position, GeneratorDirection.Forward, true ) )
      {
        container = this.GenerateContainer( generator, index, false );
      }

      isDataRow = false;

      if( container != null )
      {
        isDataRow = ( container is DataRow );

        if( m_layoutedContainers.IndexOfContainer( container ) == -1 )
        {
          m_layoutedContainers.Add( new LayoutedContainerInfo( index, container ) );
        }

        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( container );
        ColumnBase column = ( changeColumn ) ? dataGridContext.CurrentColumn : null;

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

          // We succeded in changing the current or we're already 
          // at the first index? Then nothing we can do.
          if( ( currentChanged )
            || ( desiredIndex == firstIndex ) )
          {
            break;
          }

          desiredIndex--;
        }

        LayoutedContainerInfo layoutedContainer = m_layoutedContainers[ focusedContainer ];

        // We will use MoveFocus if the focused index is currently in view.
        if( ( !currentChanged )
          && ( focusedIndex >= m_lastGeneratedPage.StartIndex )
          && ( focusedIndex <= m_lastGeneratedPage.EndIndex ) )
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

            if( cannotFocusCount > TableViewItemsHost.MaxDataRowFailFocusCount )
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

          // We succeded in changing the current or we're already 
          // at the first index? Then nothing we can do.
          if( ( currentChanged )
            || ( desiredIndex == lastIndex ) )
          {
            break;
          }

          desiredIndex++;
        }

        LayoutedContainerInfo layoutedContainer = m_layoutedContainers[ focusedContainer ];

        // We will use MoveFocus if the focused index is currently in view.
        if( ( !currentChanged )
          && ( focusedIndex >= m_lastGeneratedPage.StartIndex )
          && ( focusedIndex <= m_lastGeneratedPage.EndIndex ) )
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

            if( cannotFocusCount > TableViewItemsHost.MaxDataRowFailFocusCount )
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

    protected override void OnItemsAdded(
      GeneratorPosition position,
      int index,
      int itemCount )
    {
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

      // Everything will be recalculated and redrawn in the measure pass.
      this.InvalidateMeasure();
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

      // Everything will be recalculated and redrawn in the measure pass.
      this.InvalidateMeasure();
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
      }
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
        this.ScrollInfo.ScrollOwner.ScrollToTop();
        this.SetCurrent( 0, changeCurrentColumn );
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
        double containersHeight;
        PageIndexes generatedPage = new PageIndexes( -1, focusedContainerRealizedIndex );
        this.GeneratePage( this.RenderSize.Height, false, ref generatedPage, out containersHeight );
        this.SetVerticalOffsetCore( generatedPage.StartIndex );
        int initialDesiredIndex = generatedPage.StartIndex;

        if( focusedContainerRealizedIndex != initialDesiredIndex )
        {
          int desiredPageUpIndex = initialDesiredIndex;

          bool isDataRow = false;

          // SetCurrent on the index or down to a focusable index
          while( ( !dataGridControl.HasValidationError )
              && ( !isDataRow )
              && ( desiredPageUpIndex < focusedContainerRealizedIndex ) )
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
        this.ScrollInfo.ScrollOwner.ScrollToBottom();
        this.SetCurrent( this.CustomItemContainerGenerator.ItemCount - 1, changeCurrentColumn );
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

      int generatorItemCount = this.CustomItemContainerGenerator.ItemCount;
      int focusedContainerRealizedIndex = this.CustomItemContainerGenerator.GetRealizedIndexForContainer( focusedContainer );
      int maxIndex = generatorItemCount - 1;

      if( focusedContainerRealizedIndex == maxIndex )
      {
        this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Down ) );
      }
      else
      {
        this.InvalidateMeasure();
        double containersHeight;
        PageIndexes generatedPage = new PageIndexes( focusedContainerRealizedIndex, -1 );
        this.GeneratePage( this.RenderSize.Height, false, ref generatedPage, out containersHeight );
        this.SetVerticalOffsetCore( generatedPage.StartIndex );
        int initialDesiredIndex;

        // Last row not totally visible, take the one before the last
        if( ( containersHeight > this.RenderSize.Height ) && ( generatedPage.Length > 1 ) )
        {
          initialDesiredIndex = generatedPage.EndIndex - 1;
        }
        else
        {
          initialDesiredIndex = generatedPage.EndIndex;
        }

        if( focusedContainerRealizedIndex != initialDesiredIndex )
        {
          int desiredPageDownIndex = initialDesiredIndex;

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
              && ( desiredPageDownIndex <= maxIndex ) )
          {
            if( this.SetCurrent( desiredPageDownIndex, changeCurrentColumn, out isDataRow ) )
              return;

            desiredPageDownIndex++;
          }
        }
      }
    }

    protected override void HandleHomeKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      DataGridControl dataGridControl = this.ParentDataGridControl;
      IScrollInfo scrollInfo = this.ScrollInfo;
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
        IScrollInfo scrollInfo = this.ScrollInfo;
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

    #region ICustomVirtualizingPanel Members

    protected override void BringIntoViewCore( int index )
    {
      if( ( index >= m_lastGeneratedPage.StartIndex ) && ( index < m_lastGeneratedPage.EndIndex ) )
        return;

      this.InvalidateMeasure();
      m_indexToBringIntoView = index;
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

      DataGridScrollViewer scrollViewer = this.ScrollInfo.ScrollOwner as DataGridScrollViewer;
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

      DataGridScrollViewer scrollViewer = this.ScrollInfo.ScrollOwner as DataGridScrollViewer;
      this.ScrollToVerticalOffset( offset );
    }

    double IScrollInfo.ViewportHeight
    {
      get
      {
        if( m_lastGeneratedPageContainersHeight > m_lastGeneratedPageViewPortHeight )
        {
          return Math.Max( 1, m_lastGeneratedPage.Length - 1 );
        }
        else
        {
          return Math.Max( 1, m_lastGeneratedPage.Length );
        }
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
      int newOffset = m_lastGeneratedPage.EndIndex;

      // We only have one row and it is bigger than the view port.
      if( ( m_lastGeneratedPage.Length == 1 ) && ( m_lastGeneratedPageContainersHeight > m_lastGeneratedPageViewPortHeight ) )
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
      m_indexToBringIntoView = m_lastGeneratedPage.StartIndex;

      // We only have one row and it is bigger than the view port.
      if( ( m_lastGeneratedPage.Length == 1 ) && ( m_lastGeneratedPageContainersHeight > m_lastGeneratedPageViewPortHeight ) )
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

    #endregion IScrollInfo Members

    #region IDeferableScrollInfoRefresh Members

    IDisposable IDeferableScrollInfoRefresh.DeferScrollInfoRefresh( Orientation orientation )
    {
      if( orientation == Orientation.Vertical )
      {
        return new LayoutSuspendedHelper( this, orientation );
      }

      return null;
    }

    #endregion IDeferableScrollInfoRefresh Members

    #region Internal Methods

    internal static bool ComputedCanScrollHorizontally(
      FrameworkElement target,
      DataGridItemsHost itemsHost )
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

    internal static double GetVisibleDimensionForRequestBringIntoViewTarget(
      double targetDimension,
      double targetToItemsHostOffset,
      double viewportDimension )
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

    internal static bool TargetRequiresBringIntoView(
      double targetToItemsHostPosition,
      double targetDesiredSizeDimension,
      double itemsHostRenderSizeDimension )
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
      Point targetToParentContainer = TableViewItemsHost.EmptyPoint;
      FrameworkElement targetParentContainer = DataGridItemsHost.GetItemsHostContainerFromElement( itemsHost, target ) as FrameworkElement;

      if( ( targetParentContainer != null ) && ( target != targetParentContainer ) )
      {
        targetToParentContainer = target.TranslatePoint( TableViewItemsHost.EmptyPoint,
                                                         targetParentContainer );
      }

      Point targetToItemsHostPosition = target.TranslatePoint( TableViewItemsHost.EmptyPoint, itemsHost );
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

    #endregion Internal Methods

    #region PRIVATE METHODS

    private static void MoveFocusForwardExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      e.Handled = ( ( TableViewItemsHost )sender ).MoveFocusRight();
    }

    private static void MoveFocusForwardCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = true;
      e.Handled = true;
    }

    private static void MoveFocusBackExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      e.Handled = ( ( TableViewItemsHost )sender ).MoveFocusLeft();
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
      if( m_layoutSuspended )
      {
        m_layoutInvalidatedDuringSuspend = true;
        return;
      }

      this.GeneratePageAndUpdateIScrollInfoValues();
    }

    #endregion PRIVATE METHODS

    #region CONSTANTS

    private static readonly Point OutOfViewPoint = new Point( -999999, -999999 );
    private static readonly Point EmptyPoint = new Point( 0, 0 );

    #endregion CONSTANTS

    #region PRIVATE FIELDS



    private LayoutedContainerInfoList m_layoutedContainers = new LayoutedContainerInfoList();
    private HashSet<UIElement> m_layoutedContainersToRecycle = new HashSet<UIElement>();

    private PageIndexes m_lastGeneratedPage = PageIndexes.Empty;
    private PageIndexes m_lastLayoutedPage = PageIndexes.Empty;
    private double m_lastGeneratedPageContainersHeight;
    private double m_lastGeneratedPageViewPortHeight;

    private Dictionary<string, double> m_cachedContainerDesiredWidth = new Dictionary<string, double>();
    private Dictionary<string, double> m_cachedContainerRealDesiredWidth = new Dictionary<string, double>();

    private HashSet<string> m_autoWidthCalculatedDataGridContextList = new HashSet<string>();

    private Size m_lastMeasureAvailableSize = Size.Empty;
    private Size m_lastArrangeFinalSize = Size.Empty;
    private int m_indexToBringIntoView = -1;

    private bool m_layoutSuspended;
    private bool m_layoutInvalidatedDuringSuspend;

    #endregion PRIVATE FIELDS

    #region PageIndexes Private Struct

    private struct PageIndexes
    {
      public PageIndexes( int startIndex, int endIndex )
      {
        if( ( startIndex > endIndex ) && ( endIndex != -1 ) && ( startIndex != -1 ) )
          throw new ArgumentException( "startIndex must be less than or equal to endIndex.", "startIndex" );

        m_startIndex = startIndex;
        m_endIndex = endIndex;
      }

      public static readonly PageIndexes Empty = new PageIndexes( -1, -1 );

      private int m_startIndex;
      public int StartIndex
      {
        get
        {
          return m_startIndex;
        }

        set
        {
          m_startIndex = value;
        }
      }

      private int m_endIndex;
      public int EndIndex
      {
        get
        {
          return m_endIndex;
        }

        set
        {
          m_endIndex = value;
        }
      }

      public int Length
      {
        get
        {
          if( this.EndIndex == -1 )
            return 0;

          return this.EndIndex - this.StartIndex + 1;
        }
      }

      public override bool Equals( object obj )
      {
        if( !( obj is PageIndexes ) )
          return false;

        return ( this == ( PageIndexes )obj );
      }

      public override int GetHashCode()
      {
        return ( this.StartIndex ^ this.EndIndex );
      }

      public static bool operator ==( PageIndexes objA, PageIndexes objB )
      {
        return ( objA.StartIndex == objB.StartIndex )
          && ( objA.EndIndex == objB.EndIndex );
      }

      public static bool operator !=( PageIndexes objA, PageIndexes objB )
      {
        return !( objA == objB );
      }
    }

    #endregion PageIndexes Private Struct

    #region LayoutSuspendedHelper Private Class

    private sealed class LayoutSuspendedHelper : IDisposable
    {
      public LayoutSuspendedHelper( TableViewItemsHost panel, Orientation orientation )
      {
        if( panel == null )
          throw new ArgumentNullException( "panel" );

        m_panel = panel;
        m_panel.m_layoutSuspended = true;
      }

      public void Dispose()
      {
        m_panel.m_layoutSuspended = false;

        if( m_panel.m_layoutInvalidatedDuringSuspend )
        {
          m_panel.InvalidateMeasure();
        }
      }

      private TableViewItemsHost m_panel; // = null
    }

    #endregion LayoutSuspendedHelper Private Class
  }
}
