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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Wpf.DataGrid.Views;
using Xceed.Utils.Wpf;
using System.Collections.Generic;

namespace Xceed.Wpf.DataGrid
{
  [Obsolete( "The VirtualizingStackPanel class is obsolete and has been replaced by the TableViewItemsHost class.", false )]
  public sealed class VirtualizingStackPanel : DataGridVirtualizingPanel, ICustomVirtualizingPanel, IScrollInfo, IDeferableScrollInfoRefresh
  {
    private const int CleanupThreshold = 100; //100 ms max cleanup time.

    public VirtualizingStackPanel()
    {
      this.AddHandler( FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler( OnRequestBringIntoView ) );
    }

    #region Orientation Property

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register( "Orientation", typeof( Orientation ), typeof( VirtualizingStackPanel ), new UIPropertyMetadata( Orientation.Vertical ) );

    public Orientation Orientation
    {
      get
      {
        return ( Orientation )this.GetValue( VirtualizingStackPanel.OrientationProperty );
      }
      set
      {
        this.SetValue( VirtualizingStackPanel.OrientationProperty, value );
      }
    }

    #endregion Orientation Property

    #region StableScrollingEnabled Property

    public static readonly DependencyProperty StableScrollingEnabledProperty =
        DependencyProperty.Register( "StableScrollingEnabled", typeof( bool ), typeof( VirtualizingStackPanel ), new UIPropertyMetadata( true ) );

    public bool StableScrollingEnabled
    {
      get
      {
        return ( bool )this.GetValue( VirtualizingStackPanel.StableScrollingEnabledProperty );
      }
      set
      {
        this.SetValue( VirtualizingStackPanel.StableScrollingEnabledProperty, value );
      }
    }

    #endregion StableScrollingEnabled Property

    #region StableScrollingProportion Property

    public static readonly DependencyProperty StableScrollingProportionProperty =
        DependencyProperty.Register( "StableScrollingProportion", typeof( double ), typeof( VirtualizingStackPanel ), new UIPropertyMetadata( 0.5d ) );

    public double StableScrollingProportion
    {
      get
      {
        return ( double )this.GetValue( VirtualizingStackPanel.StableScrollingProportionProperty );
      }
      set
      {
        this.SetValue( VirtualizingStackPanel.StableScrollingProportionProperty, value );
      }
    }

    #endregion StableScrollingProportion Property

    #region UsedSurface Property

    private static readonly DependencyProperty UsedSurfaceProperty =
        DependencyProperty.Register( "UsedSurface", typeof( Rect ), typeof( VirtualizingStackPanel ), new FrameworkPropertyMetadata( Rect.Empty, FrameworkPropertyMetadataOptions.AffectsRender ) );

    private Rect UsedSurface
    {
      get
      {
        return ( Rect )this.GetValue( VirtualizingStackPanel.UsedSurfaceProperty );
      }
      set
      {
        this.SetValue( VirtualizingStackPanel.UsedSurfaceProperty, value );
      }
    }

    #endregion UsedSurface Property

    #region ContentBorderBrush Property

    public static readonly DependencyProperty ContentBorderBrushProperty =
        DependencyProperty.Register( "ContentBorderBrush", typeof( Brush ), typeof( VirtualizingStackPanel ), new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.AffectsRender ) );

    public Brush ContentBorderBrush
    {
      get
      {
        return ( Brush )this.GetValue( VirtualizingStackPanel.ContentBorderBrushProperty );
      }
      set
      {
        this.SetValue( VirtualizingStackPanel.ContentBorderBrushProperty, value );
      }
    }

    #endregion ContentBorderBrush Property

    #region ContentBorderThickness Property

    public static readonly DependencyProperty ContentBorderThicknessProperty =
        DependencyProperty.Register( "ContentBorderThickness", typeof( Thickness ), typeof( VirtualizingStackPanel ), new FrameworkPropertyMetadata( DefaultThickness, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange ) );

    public Thickness ContentBorderThickness
    {
      get
      {
        return ( Thickness )this.GetValue( VirtualizingStackPanel.ContentBorderThicknessProperty );
      }
      set
      {
        this.SetValue( VirtualizingStackPanel.ContentBorderThicknessProperty, value );
      }
    }

    #endregion ContentBorderThickness Property

    protected override Size ArrangeOverride( Size finalSize )
    {
      UIElementCollection children = this.Children;
      IItemContainerGenerator generator = this.CustomItemContainerGenerator;
      Orientation orientation = this.Orientation;
      GeneratorPosition firstItemGenPos;

      if( generator == null )
      {
        // In design mode, there won't be any CustomItemContainerGenerator to work with 
        // when editing the style of VirtualizingStackPanel but, strangely, 
        // GetIsInDesignMode can return false.
        Debug.WriteLineIf( !DesignerProperties.GetIsInDesignMode( this ), "Missing generator in VirtualizingStackPanel.ArrangeOverride()" );
        firstItemGenPos = new GeneratorPosition( 0, 0 );
      }
      else
      {
        //retrieve the Generator position for the first visible item
        firstItemGenPos = generator.GeneratorPositionFromIndex( ( int )( ( orientation == Orientation.Vertical ) ? this.VerticalOffset : this.HorizontalOffset ) );
      }

      //from that generator position, the Index is the child index in the children collection.

      Point offset = new Point();
      Rect itemRect = new Rect();

      Point bottomRight = new Point();
      Point topLeft = new Point();

      double compensationOffset;
      double compensationSize;
      double compensationItemSize;

      //place the "opposed" direction's offset to the initial offset.
      if( orientation == Orientation.Vertical )
      {
        // ****************************************
        // The Starting offset is set to 0.05 so that layout is not "perfectly" aligned with start of panel.
        // This is because a perfect layout sometimes cause glitches in the keyboard navigation for items of a different layout.
        //****************************************
        if( this.VerticalOffset != 0 )
        {
          offset.Y = 0.05;
        }
        else
        {
          offset.Y = this.ContentBorderThickness.Top;
        }

        if( this.HorizontalOffset != 0 )
        {
          offset.X = -this.HorizontalOffset;
        }
        else
        {
          offset.X = this.ContentBorderThickness.Left;
        }

        //set the "starting" endpoint for the ContentBorder to the extent coordinates.
        //for this case, I cannot rely on the ExtentHeight for the bottomRight.Y
        bottomRight.X = this.ExtentWidth;

        compensationOffset = this.HorizontalOffset;
      }
      else
      {
        //****************************************
        // The Starting offset is set to 0.05 so that layout is not "perfectly" aligned with start of panel.
        // This is because a perfect layout sometimes cause glitches in the keyboard navigation for items of a different layout.
        //****************************************
        if( this.HorizontalOffset != 0 )
        {
          offset.X = 0.05;
        }
        else
        {
          offset.X = this.ContentBorderThickness.Left;
        }

        if( this.VerticalOffset != 0 )
        {
          offset.Y = -this.VerticalOffset;
        }
        else
        {
          offset.Y = this.ContentBorderThickness.Top;
        }

        //set the "starting" endpoint for the ContentBorder to the extent coordinates.
        //for this case, I cannot rely on the ExtentHeight for the bottomRight.X
        bottomRight.Y = this.ExtentHeight;

        compensationOffset = this.VerticalOffset;
      }

      bool limitInSize = false;


      //cycle through all the children, starting from that one
      for( int i = firstItemGenPos.Index; i < children.Count; i++ )
      {
        if( i >= 0 )
        {
          UIElement item = children[ i ];
          DataGridContext itemContext = DataGridControl.GetDataGridContext( item );
          string itemContextName = ( itemContext.SourceDetailConfiguration == null ) ? "" : itemContext.SourceDetailConfiguration.RelationName;

          itemRect.X = offset.X;
          itemRect.Y = offset.Y;

          if( orientation == Orientation.Vertical )
          {
            compensationItemSize = this.GetCorrectedItemWidth( itemContextName, finalSize );

            itemRect.Width = compensationItemSize;
            compensationSize = Math.Min( this.ViewportWidth, compensationItemSize );

            itemRect.Height = item.DesiredSize.Height;

            offset.Y += itemRect.Height;

            //Update the SurfaceRect with the coordinates of item just placed.
            bottomRight.Y = itemRect.Bottom;
            if( itemRect.Right > bottomRight.X )
            {
              bottomRight.X = itemRect.Right;
            }
          }
          else
          {
            itemRect.Width = item.DesiredSize.Width;

            compensationItemSize = this.GetCorrectedItemHeight( itemContextName, finalSize );

            itemRect.Height = compensationItemSize;
            compensationSize = Math.Min( this.ViewportHeight, compensationItemSize );

            itemRect.Height = Math.Max( m_extent.Height, item.DesiredSize.Height );

            offset.X += itemRect.Width;

            //Update the SurfaceRect with the coordinates of item just placed.
            bottomRight.X = itemRect.Right;
            if( itemRect.Bottom > bottomRight.Y )
            {
              bottomRight.Y = itemRect.Bottom;
            }
          }

          TableView.SetCompensationOffset( item, Math.Max( 0, ( ( compensationOffset + compensationSize ) - compensationItemSize ) ) );
          item.Arrange( itemRect );
        }
      }

      //reset the "opposed" direction's offset to the initial offset.
      if( orientation == Orientation.Vertical )
      {
        //****************************************
        // The Starting offset is set to 0.05 so that layout is not "perfectly" aligned with start of panel.
        // This is because a perfect layout sometimes cause glitches in the keyboard navigation for items of a different layout.
        //****************************************
        if( this.VerticalOffset != 0 )
        {
          offset.Y = 0.05;
        }
        else
        {
          offset.Y = this.ContentBorderThickness.Top;
        }

        if( this.HorizontalOffset != 0 )
        {
          offset.X = -this.HorizontalOffset;
        }
        else
        {
          offset.X = this.ContentBorderThickness.Left;
        }

        topLeft.X = offset.X;
      }
      else
      {
        //****************************************
        // The Starting offset is set to 0.05 so that layout is not "perfectly" aligned with start of panel.
        // This is because a perfect layout sometimes cause glitches in the keyboard navigation for items of a different layout.
        //****************************************
        if( this.HorizontalOffset != 0 )
        {
          offset.X = 0.05;
        }
        else
        {
          offset.X = this.ContentBorderThickness.Left;
        }

        if( this.VerticalOffset != 0 )
        {
          offset.Y = -this.VerticalOffset;
        }
        else
        {
          offset.Y = this.ContentBorderThickness.Top;
        }

        topLeft.Y = offset.Y;
      }

      //cycle through all the items "prior" the first visible and place them before the first one
      for( int i = firstItemGenPos.Index - 1; i >= 0; i-- )
      {
        UIElement item = children[ i ];
        DataGridContext itemContext = DataGridControl.GetDataGridContext( item );
        string itemContextName = ( itemContext.SourceDetailConfiguration == null ) ? "" : itemContext.SourceDetailConfiguration.RelationName;

        limitInSize = ( itemContext.ParentDataGridContext != null );

        if( orientation == Orientation.Vertical )
        {
          compensationItemSize = this.GetCorrectedItemWidth( itemContextName, finalSize );

          itemRect.Width = compensationItemSize;
          compensationSize = Math.Min( this.ViewportWidth, compensationItemSize );

          itemRect.Height = item.DesiredSize.Height;

          offset.Y -= itemRect.Height;

          //Update the SurfaceRect with the coordinates of item just placed.
          topLeft.Y = offset.Y;
        }
        else
        {
          itemRect.Width = item.DesiredSize.Width;

          compensationItemSize = this.GetCorrectedItemHeight( itemContextName, finalSize );

          itemRect.Height = compensationItemSize;
          compensationSize = Math.Min( this.ViewportHeight, compensationItemSize );

          offset.X -= itemRect.Width;

          //Update the SurfaceRect with the coordinates of item just placed.
          topLeft.X = itemRect.X;
        }

        itemRect.X = offset.X;
        itemRect.Y = offset.Y;

        TableView.SetCompensationOffset( item, Math.Max( 0, ( ( compensationOffset + compensationSize ) - compensationItemSize ) ) );
        item.Arrange( itemRect );

      }

      Rect surfaceRect = new Rect( topLeft, bottomRight );
      if( this.UsedSurface != surfaceRect )
      {
        this.UsedSurface = surfaceRect;
      }

      return finalSize;

    }

    private double GetSynchronizedExtentWidth()
    {
      DataGridScrollViewer dgScrollViewer = m_owner as DataGridScrollViewer;
      if( dgScrollViewer != null )
      {
        return dgScrollViewer.SynchronizedScrollViewersWidth;
      }
      return m_extent.Width;
    }

    private double GetCorrectedItemWidth( string itemContextName, Size arrangeSize )
    {
      double retval;

      double synchronizedWidth = this.GetSynchronizedExtentWidth();
      double extentWidth = Math.Max( synchronizedWidth, m_extent.Width );

      bool foundContextName = m_desiredSizeDictionary.TryGetValue( itemContextName, out retval );

      //if the itemContextName is not present in the desiredSizeDictionary
      // OR
      //if the desiredSize is real close to 0 
      if( ( foundContextName == false ) || ( Xceed.Utils.Math.DoubleUtil.AreClose( retval, 0d ) == true ) )
      {
        //I take a look at the previous sizes dictionary.
        if( m_previousSizeDictionary.TryGetValue( itemContextName, out retval ) == false )
        {
          //If the entry is not found in the previous sizes dictionary, then take the largest between the extentWidth or the arrangeSize.
          retval = Math.Max( extentWidth, arrangeSize.Width );
        }
      }
      else
      {
        //Now, if the itemContextName was found in the desiredSize dictionary AND it was larger than 0
        //Store the value in the previousSizes dictionary
        m_previousSizeDictionary[ itemContextName ] = retval;
      }

      if( string.IsNullOrEmpty( itemContextName ) == true ) // master level 
      {
        //for the master level, I want to consider the Synchronized Extent size as well.
        retval = Math.Max( synchronizedWidth, retval );        
      }

      return retval;
    }

    private double GetSynchronizedExtentHeight()
    {
      DataGridScrollViewer dgScrollViewer = m_owner as DataGridScrollViewer;
      if( dgScrollViewer != null )
      {
        return dgScrollViewer.SynchronizedScrollViewersHeight;
      }
      return m_extent.Height;
    }

    private double GetCorrectedItemHeight( string itemContextName, Size arrangeSize )
    {
      double retval;

      double synchronizedHeight = this.GetSynchronizedExtentHeight();
      double extentHeight = Math.Max( synchronizedHeight, m_extent.Height );

      bool foundContextName = m_desiredSizeDictionary.TryGetValue( itemContextName, out retval );

      //if the itemContextName is not present in the desiredSizeDictionary
      // OR
      //if the desiredSize is real close to 0 
      if( ( foundContextName == false ) || ( Xceed.Utils.Math.DoubleUtil.AreClose( retval, 0d ) == true ) )
      {
        //I take a look at the previous sizes dictionary.
        if( m_previousSizeDictionary.TryGetValue( itemContextName, out retval ) == false )
        {
          //If the entry is not found in the previous sizes dictionary, then take the largest between the extentWidth or the arrangeSize.
          retval = Math.Max( extentHeight, arrangeSize.Height );
        }
      }
      else
      {
        //Now, if the itemContextName was found in the desiredSize dictionary AND it was larger than 0
        //Store the value in the previousSizes dictionary
        m_previousSizeDictionary[ itemContextName ] = retval;
      }

      if( string.IsNullOrEmpty( itemContextName ) == true ) // master level 
      {
        //for the master level, I want to consider the Synchronized Extent size as well.
        retval = Math.Max( synchronizedHeight, retval );
      }

      return retval;
    }

    protected override Size MeasureOverride( Size availableSize )
    {
      UIElementCollection children = this.Children;
      IItemContainerGenerator generator = this.CustomItemContainerGenerator;
      List<int> calculatedAutoWidthContextLevels = new List<int>();

      if( generator == null )
      {
        // In design mode, there won't be any CustomItemContainerGenerator to work with 
        // when editing the style of VirtualizingStackPanel but, strangely, 
        // GetIsInDesignMode can return false.
        Debug.WriteLineIf( !DesignerProperties.GetIsInDesignMode( this ), "Missing generator in VirtualizingStackPanel.MeasureOverride()" );
        return new Size( 20, 20 );
      }

      //clear the dictionary that holds the "maximum desired size" for the different DataGridContexts.
      m_desiredSizeDictionary.Clear();

      bool invalidateScrollInfo = false;

      Orientation orientation = this.Orientation;

      int numberItems = this.ItemCount;

      //retrieve the index of the first item to layout
      int firstVisibleIndex = ( int )( ( orientation == Orientation.Vertical ) ? this.VerticalOffset : this.HorizontalOffset );

      // If the item offset is past the last item in the items list, then use the last item... (move the offset)
      if( firstVisibleIndex >= numberItems )
      {
        firstVisibleIndex = Math.Max( 0, numberItems - 1 );

        if( orientation == Orientation.Vertical )
        {
          m_offset.Y = firstVisibleIndex;
        }
        else
        {
          m_offset.X = firstVisibleIndex;
        }
      }

      bool newlyRealized = false;
      double actualSize = 0.0d;
      Size desiredSize = new Size( 0, 0 );
      Size restrictedMeasureSize;

      // We need to set the viewPort width while calling MeasureOverride
      // to be able to virtualize Columns
      if( orientation == Orientation.Vertical )
      {
        restrictedMeasureSize = new Size( availableSize.Width, double.PositiveInfinity );
        m_viewport.Width = availableSize.Width;
      }
      else
      {
        restrictedMeasureSize = new Size( double.PositiveInfinity, availableSize.Height );
        m_viewport.Height = availableSize.Height;
      }

      //get the generator position for the first visible index
      GeneratorPosition firstItemGeneratorPosition = generator.GeneratorPositionFromIndex( firstVisibleIndex );

      // Get index where we would insert the child for this position. If the item is realized
      // (position.Offset == 0), it's just position.Index, otherwise we have to add one to
      // insert after the corresponding child
      int childIndex = ( firstItemGeneratorPosition.Offset == 0 ) ? firstItemGeneratorPosition.Index : firstItemGeneratorPosition.Index + 1;
      int itemsCount = 0;

      //if the items collection for the ItemsControl is not empty!.
      if( numberItems > 0 )
      {
        //this flag indicates if one item was placed but was only partially visible (or not at all)
        bool busted = false;

        using( generator.StartAt( firstItemGeneratorPosition, GeneratorDirection.Forward, true ) )
        {
          bool quitLoop = false;
          DataGridContext dataGridContext;
          // The rowDesiredSizeCalculated flag is necessary for ColumnStretchMode because
          // the first row measurement will calculate and modify the columns ActualWidth.
          // Consequently, this row will have the correct DesiredSize (using the correct
          // ActualWidths). However, another Row in the same "measure pass" may not 
          // consider the new ActualWidths immediately (due to UIElement.Measure 
          // optimisation). With this flag, the first Row will be taking into account 
          // when determining the largest opposed size, but the following realized Rows
          // will be ignored. This will always work because all the rows have the same 
          // opposed size (the same width when the orientation is vertical).
          bool rowDesiredSizeCalculated = false;

          do
          {
            //If we reached the end of the possible items
            if( ( firstVisibleIndex + itemsCount ) >= numberItems )
            {
              //exit loop
              quitLoop = true;
            }
            else
            {
              UIElement item = ( UIElement )generator.GenerateNext( out newlyRealized );

              if( item == null )
              {
                quitLoop = true;
              }
              else
              {

                //if the items was just realized
                if( newlyRealized == true )
                {
                  // Figure out if we need to insert the child at the end or somewhere in the middle
                  if( childIndex >= children.Count )
                  {
                    base.AddInternalChild( item );
                  }
                  else
                  {
                    base.InsertInternalChild( childIndex, item );
                  }

                  //prepare the new item container
                  generator.PrepareItemContainer( item );
                }

                itemsCount++;
                childIndex++;
                dataGridContext = DataGridControl.GetDataGridContext( item );

                if( !calculatedAutoWidthContextLevels.Contains( dataGridContext.DetailLevel ) )
                {
                  bool isRow = item is Row;

                  if( ( !isRow ) && ( item is HeaderFooterItem ) )
                    isRow = typeof( Row ).IsAssignableFrom( ( ( HeaderFooterItem )item ).VisualRootElementType );

                  if( isRow )
                  {
                    dataGridContext.ColumnStretchingManager.ColumnStretchingCalculated = false;
                    // Calling Measure with the Viewport's width will have the effect of 
                    // distributing the extra space (see FixedCellPanel's MeasureOverride). 
                    // Eventually, the FixedCellPanel will receive an adjusted viewport 
                    // width (where GroupLevelIndicator's width et al will be substracted).
                    item.Measure( restrictedMeasureSize );

                    if( dataGridContext.ColumnStretchingManager.ColumnStretchingCalculated )
                      calculatedAutoWidthContextLevels.Add( dataGridContext.DetailLevel );

                    // We still need to measure using infinity in case that the row is alone 
                    // in its panel.
                  }
                }

                item.Measure( VirtualizingStackPanel.InfiniteSize );

                string dataGridContextName = ( dataGridContext.SourceDetailConfiguration != null ) ? dataGridContext.SourceDetailConfiguration.RelationName : string.Empty;

                if( m_desiredSizeDictionary.ContainsKey( dataGridContextName ) == false )
                {
                  m_desiredSizeDictionary.Add( dataGridContextName, 0d );
                }

                //if the orientation is "Vertical"
                if( orientation == Orientation.Vertical )
                {
                  //and the width of the item is the largest found yet
                  if( item.DesiredSize.Width > m_desiredSizeDictionary[ dataGridContextName ] )
                  {
                    //remember the width as the largest found!
                    if( ( !rowDesiredSizeCalculated ) || !( item is Row ) )
                      m_desiredSizeDictionary[ dataGridContextName ] = item.DesiredSize.Width;
                  }

                  //offset the coordinate by the height of the item.
                  actualSize += item.DesiredSize.Height;

                  if( ( actualSize > availableSize.Height ) && ( itemsCount != 1 ) ) //the second condition is there to prevent a size 0 viewport (and to make sure at least 1 post item is generated)
                  {
                    //partially visible rows are not part of the viewport.
                    quitLoop = true;
                    busted = true;
                  }
                }
                else
                {
                  //and the Height of the item is the largest found yet
                  if( item.DesiredSize.Height > m_desiredSizeDictionary[ dataGridContextName ] )
                  {
                    //remember the Height as the largest found!
                    if( ( !rowDesiredSizeCalculated ) || !( item is Row ) )
                      m_desiredSizeDictionary[ dataGridContextName ] = item.DesiredSize.Height;
                  }

                  //offset the coordinate by the Width of the item.
                  actualSize += item.DesiredSize.Width;

                  if( ( actualSize > availableSize.Width ) && ( itemsCount != 1 ) ) //the second condition is there to prevent a size 0 viewport (and to make sure at least 1 post item is generated)
                  {
                    //partially visible rows are not part of the viewport.
                    quitLoop = true;
                    busted = true;
                  }
                }

                if( ( !rowDesiredSizeCalculated ) && ( item is Row ) )
                  rowDesiredSizeCalculated = true;
              }
            }

          }
          while( quitLoop == false );
        } //end using!

        //from this point, it is possible to calculate the Viewport and the extent sizes
        double maxOpposedSize = this.ComputeMaxOpposedSize();

        if( orientation == Orientation.Vertical )
        {
          double synchronizedExtentWidth = 0;
          DataGridScrollViewer dgsv = this.ScrollOwner as DataGridScrollViewer;
          if( dgsv != null )
          {
            synchronizedExtentWidth = dgsv.SynchronizedScrollViewersWidth;
          }

          //if the calculated width of the viewport is larger than available size,
          //use the available size as the viewport... otherwise, take the calculated size
          // If the availableSize is Infinity, use the greater of the calculated sizes 
          // (either the grid body or headers/footers, aka SynchronizedScrollViewers).
          double tmpWidth = ( Double.IsInfinity( availableSize.Width ) == false ) ? availableSize.Width : Math.Max( maxOpposedSize, synchronizedExtentWidth );
          if( m_viewport.Width != tmpWidth )
          {
            m_viewport.Width = tmpWidth;
            invalidateScrollInfo = true;
          }

          int tmpHeight = ( busted == false ) ? itemsCount : itemsCount - 1; //-1 since I generate 1 more than required
          if( m_viewport.Height != tmpHeight )
          {
            m_viewport.Height = tmpHeight;
            invalidateScrollInfo = true;
          }

          // Determine this Panel's extent
          maxOpposedSize = Math.Max( maxOpposedSize, synchronizedExtentWidth );

          if( m_extent.Width != maxOpposedSize )
          {
            m_extent.Width = maxOpposedSize;
            invalidateScrollInfo = true;
          }

          if( m_extent.Height != numberItems )
          {
            m_extent.Height = numberItems;
            invalidateScrollInfo = true;
          }

          if( ( this.HorizontalOffset + this.ViewportWidth ) > this.ExtentWidth )
          {
            m_offset.X = Math.Max( 0.0d, ( this.ExtentWidth - this.ViewportWidth ) );
            invalidateScrollInfo = true;
          }

          //this sets the desiredSize of the Panel according to the values gathered during the Measure of all elements.
          desiredSize.Height = Math.Min( availableSize.Height, actualSize );
          desiredSize.Width = Math.Min( availableSize.Width, maxOpposedSize );
        }
        else
        {
          double synchronizedExtentHeight = 0;
          DataGridScrollViewer dgsv = this.ScrollOwner as DataGridScrollViewer;
          if( dgsv != null )
          {
            synchronizedExtentHeight = dgsv.SynchronizedScrollViewersHeight;
          }

          //if the calculated Height of the viewport is larger than available size,
          //use the available size as the viewport... otherwise, take the calculated size
          // If the availableSize is Infinity, use the greater of the calculated sizes 
          // (either the grid body or headers/footers, aka SynchronizedScrollViewers).
          double tmpHeight = ( Double.IsInfinity( availableSize.Height ) == false ) ? availableSize.Height : Math.Max( maxOpposedSize, synchronizedExtentHeight );
          if( m_viewport.Height != tmpHeight )
          {
            m_viewport.Height = tmpHeight;
            invalidateScrollInfo = true;
          }

          int tmpWidth = ( busted == false ) ? itemsCount : itemsCount - 1; //-1 since I generated 1 more than required
          if( m_viewport.Width != tmpWidth )
          {
            m_viewport.Width = tmpWidth;
            invalidateScrollInfo = true;
          }

          // Determine this Panel's extent
          maxOpposedSize = Math.Max( maxOpposedSize, synchronizedExtentHeight );

          if( m_extent.Height != maxOpposedSize )
          {
            m_extent.Height = maxOpposedSize;
            invalidateScrollInfo = true;
          }

          if( m_extent.Width != numberItems )
          {
            m_extent.Width = numberItems;
            invalidateScrollInfo = true;
          }

          if( ( this.VerticalOffset + this.ViewportHeight ) > this.ExtentHeight )
          {
            m_offset.Y = Math.Max( 0.0d, ( this.ExtentHeight - this.ViewportHeight ) );
            invalidateScrollInfo = true;
          }

          //this sets the desiredSize of the Panel according to the values gathered during the Measure of all elements.
          desiredSize.Width = Math.Min( availableSize.Width, actualSize );
          desiredSize.Height = Math.Min( availableSize.Height, maxOpposedSize );
        }

        //finally, generate the pre-item!
        if( firstVisibleIndex > 0 )
        {
          GeneratorPosition preItemGeneratorPos = generator.GeneratorPositionFromIndex( firstVisibleIndex - 1 );
          int preChildIndex = ( preItemGeneratorPos.Offset == 0 ) ? preItemGeneratorPos.Index : preItemGeneratorPos.Index + 1;

          using( generator.StartAt( preItemGeneratorPos, GeneratorDirection.Backward, true ) )
          {
            UIElement item = ( UIElement )generator.GenerateNext( out newlyRealized );

            //if the items was just realized
            if( newlyRealized == true )
            {
              // Figure out if we need to insert the child at the end or somewhere in the middle
              if( preChildIndex >= children.Count )
              {
                base.AddInternalChild( item );
              }
              else
              {
                base.InsertInternalChild( preChildIndex, item );
              }

              //prepare the new item container
              generator.PrepareItemContainer( item );
            }

            //measure with the available size... 
            item.Measure( VirtualizingStackPanel.InfiniteSize );
          } //end using
        }//end if firstVisibleIndex > 0

        //set the members that will indicate to the cleaning function what items are truly used.
        m_actualFirstItem = ( firstVisibleIndex > 0 ) ? firstVisibleIndex - 1 : 0;
        m_actualLastItem = firstVisibleIndex + itemsCount - 1;

      } //end if numberItems > 0
      else
      {
        if( orientation == Orientation.Vertical )
        {
          double synchronizedWidth = 0;
          DataGridScrollViewer dgsv = this.ScrollOwner as DataGridScrollViewer;
          if( dgsv != null )
          {
            synchronizedWidth = dgsv.SynchronizedScrollViewersWidth;
          }

          m_extent.Width = synchronizedWidth;
          m_viewport.Width = availableSize.Width;
          m_extent.Height = 0;
        }
        else
        {
          double synchronizedHeight = 0;
          DataGridScrollViewer dgsv = this.ScrollOwner as DataGridScrollViewer;
          if( dgsv != null )
          {
            synchronizedHeight = dgsv.SynchronizedScrollViewersHeight;
          }

          m_extent.Height = synchronizedHeight;
          m_viewport.Height = availableSize.Height;
          m_extent.Width = 0;
        }

        invalidateScrollInfo = true;

        //indicate to the cleaning function that everything need to be cleaned!
        m_actualFirstItem = -1;
        m_actualLastItem = -1;
      }

      //if one of the parameters of the IScrollInfo interface was changed, then Invalidate the ScrollInfo owner!
      if( ( invalidateScrollInfo == true ) && ( m_owner != null ) )
      {
        m_owner.InvalidateScrollInfo();
      }

      //call this so that the Cleanup is triggered with the latest parameters 
      this.SetupCleaning();

      return desiredSize;
    }

    private double ComputeMaxOpposedSize()
    {
      double actualMax = 0d;
      foreach( double value in m_desiredSizeDictionary.Values )
      {
        if( value > actualMax )
          actualMax = value;
      }

      return actualMax;
    }

    protected override void OnRender( DrawingContext drawingContext )
    {
      base.OnRender( drawingContext );

      Brush contentBrush = this.ContentBorderBrush;
      Thickness contentThickness = this.ContentBorderThickness;
      Rect usedSurface = this.UsedSurface;

      if( ( contentBrush != null ) && ( usedSurface != Rect.Empty ) )
      {
        Pen myPen = new Pen( contentBrush, contentThickness.Left );

        GuidelineSet gls = new GuidelineSet();
        gls.GuidelinesX.Add( 0 );
        gls.GuidelinesX.Add( usedSurface.Right );
        gls.GuidelinesY.Add( 0 );
        gls.GuidelinesY.Add( usedSurface.Bottom );

        drawingContext.PushGuidelineSet( gls );

        if( contentThickness.Left > 0 )
        {
          drawingContext.DrawLine( myPen, new Point( 0 + ( contentThickness.Left / 2 ), 0 ), new Point( 0 + ( contentThickness.Left / 2 ), usedSurface.Bottom + contentThickness.Top ) );
        }

        if( contentThickness.Top > 0 )
        {
          if( myPen.Thickness != contentThickness.Top )
          {
            myPen = new Pen( contentBrush, contentThickness.Top );
          }

          drawingContext.DrawLine( myPen, new Point( 0, 0 + ( contentThickness.Top / 2 ) ), new Point( usedSurface.Right + contentThickness.Left, 0 + ( contentThickness.Top / 2 ) ) );
        }

        if( contentThickness.Right > 0 )
        {
          if( myPen.Thickness != contentThickness.Right )
          {
            myPen = new Pen( contentBrush, contentThickness.Right );
          }

          drawingContext.DrawLine( myPen, new Point( usedSurface.Right + ( contentThickness.Right / 2 ), 0 ), new Point( usedSurface.Right + ( contentThickness.Right / 2 ), usedSurface.Bottom + contentThickness.Top ) );
        }

        if( contentThickness.Bottom > 0 )
        {
          if( myPen.Thickness != contentThickness.Bottom )
          {
            myPen = new Pen( contentBrush, contentThickness.Bottom );
          }

          drawingContext.DrawLine( myPen, new Point( 0, usedSurface.Bottom + ( contentThickness.Bottom / 2 ) ), new Point( usedSurface.Right + contentThickness.Left, usedSurface.Bottom + ( contentThickness.Bottom / 2 ) ) );
        }

        drawingContext.Pop();
      }
    }

    protected override void BringIndexIntoView( int index )
    {
      ( ( ICustomVirtualizingPanel )this ).BringIntoView( index );
    }

    protected override void OnClearChildren()
    {
      base.OnClearChildren();
    }

    private void SetupCleaning()
    {
      //this function is used to setup the cleaninp of the old containers in the least possible
      //intrusive way so that measure can return without processing the cleanup inline.

      //making the Cleanup asynchronous will somehow delay a little bit the Virtualization of cells, 
      //but, were trying to reach the best compromise between recycling and generation of items.
      m_currentCleanupIndex = this.Children.Count - 1;

      if( m_cleanupOperation == null )
      {
        m_cleanupOperation = this.Dispatcher.BeginInvoke( ( ( DispatcherPriority )m_initialCleanupPriority ), new CleanupHandler( DoCleanup ) );
      }
      else
      {
        m_cleanupPriorityModifier = this.CalculateCleanupPriorityModifier();

        if( m_cleanupPriorityModifier > 0 )
        {
          m_cleanupOperation.Abort();
          m_cleanupOperation = this.Dispatcher.BeginInvoke( ( ( DispatcherPriority )m_initialCleanupPriority + m_cleanupPriorityModifier ), new CleanupHandler( DoCleanup ) );
        }
      }

    }

    private void DoCleanup()
    {
      bool finished = this.CleanupWorker();
      if( finished == false )
      {
        m_cleanupPriorityModifier = this.CalculateCleanupPriorityModifier();
        m_cleanupOperation = this.Dispatcher.BeginInvoke( ( ( DispatcherPriority )m_initialCleanupPriority + m_cleanupPriorityModifier ), new CleanupHandler( DoCleanup ) );
      }
      else
      {
        m_cleanupOperation = null;
      }
    }

    private bool CleanupWorker()
    {
      bool retval = true;

      UIElementCollection children = this.InternalChildren;
      IItemContainerGenerator generator = this.CustomItemContainerGenerator;

      if( generator != null )
      {
        int viewportSize = this.GetViewportSize();

        if( ( m_actualFirstItem == m_actualLastItem ) && ( m_actualFirstItem == -1 ) )
        {
          this.RemoveInternalChildRange( 0, children.Count );
          generator.RemoveAll();
        }
        else
        {
          int startTick = Environment.TickCount;

          while( m_currentCleanupIndex >= 0 )
          {
            GeneratorPosition childGeneratorPos = new GeneratorPosition( m_currentCleanupIndex, 0 );
            int itemIndex = generator.IndexFromGeneratorPosition( childGeneratorPos );

            if( ( itemIndex < ( m_actualFirstItem - viewportSize ) || itemIndex > ( m_actualLastItem + viewportSize ) ) && itemIndex != -1 )
            {
              DependencyObject focusScope = FocusManager.GetFocusScope( children[ m_currentCleanupIndex ] );
              DependencyObject focusedElement = FocusManager.GetFocusedElement( focusScope ) as DependencyObject;

              if( ( focusedElement == null ) || ( !TreeHelper.IsDescendantOf( focusedElement, children[ m_currentCleanupIndex ] ) ) )
              {
                RemoveInternalChildRange( m_currentCleanupIndex, 1 );
                generator.Remove( childGeneratorPos, 1 );
              }
            }

            m_currentCleanupIndex--;

            if( Environment.TickCount - startTick > CleanupThreshold )
            {
              retval = false;
              break;
            }
          }
        }
      }

      return retval;
    }

    private int CalculateCleanupPriorityModifier()
    {
      int viewportSize = this.GetViewportSize();

      //Notes on the Algo: ViewportSize * 3 is because we want to keep 1 page before and after the actual viewport.
      //Notes on the Algo: 10 - m_initialCleanupPriority is because we do not want the modifier to go beyond the scope of the enum...
      return ( int )Math.Min( Math.Floor( this.InternalChildren.Count / ( viewportSize * 3d ) ) - 1, ( 10 - m_initialCleanupPriority ) );
    }

    private int GetViewportSize()
    {
      return ( int )( ( this.Orientation == Orientation.Vertical ) ? this.ViewportHeight : this.ViewportWidth );
    }

    private Rect MakeVisible( int childIndex, int index, Rect rectangle )
    {
      this.BringIndexIntoViewHelper( childIndex, index );

      if( this.Orientation == Orientation.Vertical )
      {
        //check if the beginning of the rectangle is before the "offset"
        if( rectangle.Left < this.HorizontalOffset )
        {
          //then make the origin of the rectangle visible (independent of width)
          this.SetHorizontalOffset( rectangle.Left );
        }
        //if the right side of the rectangle is not visible
        else if( rectangle.Right > ( this.HorizontalOffset + this.ViewportWidth ) )
        {
          //and the rectangle is larger than viewport
          if( rectangle.Width > this.ViewportWidth )
          {
            //make the origin of the rectangle visible
            this.SetHorizontalOffset( rectangle.Left );
          }
          //and rectangle is not larger than viewport
          else
          {
            //align the right side of the rectangle at the rightmost edge of the viewport.
            this.SetHorizontalOffset( rectangle.Right - this.ViewportWidth );
          }
        }

      }
      else
      {
        //check if the beginning of the rectangle is before the "offset"
        if( rectangle.Top < this.VerticalOffset )
        {
          //then make the origin of the rectangle visible (independent of height)
          this.SetVerticalOffset( rectangle.Top );
        }
        //if the bottom of the rectangle is not visible
        else if( rectangle.Bottom > ( this.VerticalOffset + this.ViewportHeight ) )
        {
          //and the rectangle is higher than viewport
          if( rectangle.Height > this.ViewportHeight )
          {
            //make the origin of the rectangle visible
            this.SetVerticalOffset( rectangle.Top );
          }
          //and rectangle is not higher than viewport
          else
          {
            //align the bottom of the rectangle at the bottom edge of the viewport.
            this.SetVerticalOffset( rectangle.Bottom - this.ViewportHeight );
          }
        }
      }

      return rectangle;
    }

    private void BringIndexIntoViewHelper( int childIndex, int itemIndex )
    {
      IItemContainerGenerator generator = this.CustomItemContainerGenerator;

      if( this.Orientation == Orientation.Vertical )
      {
        //Only do stuff if the item is out of the viewport.
        if( itemIndex < this.VerticalOffset )
        {
          //this is easy, just set the item as the first one.
          this.SetVerticalOffset( itemIndex );
        }
        else if( itemIndex >= this.VerticalOffset + this.ViewportHeight )
        {
          //this is a little trickier... I need to determine the best possible offset to display the item passed.
          int desiredChildIndex = childIndex;
          int desiredOffset = itemIndex;
          double cachedActualHeight = this.ActualHeight;
          double totalHeight = 0;
          bool passedOnce = false;

          do
          {
            //add the height of the current element to the total calculated height
            totalHeight += this.Children[ desiredChildIndex ].RenderSize.Height;

            //if the calculated height is still lower that the size of the viewport (render size)
            if( totalHeight < cachedActualHeight )
            {
              int currentIndex = generator.IndexFromGeneratorPosition( new GeneratorPosition( desiredChildIndex, 0 ) );
              //if the container is not for the "next" item in line... 
              if( currentIndex != desiredOffset )
              {
                desiredOffset++;
                break;
              }
              else
              {
                desiredChildIndex--;
                desiredOffset--;
                passedOnce = true;
              }
            }
            else if( passedOnce == true )
            {
              //since we exceeded the constraint, go back one step
              desiredOffset++;
            }

            if( desiredOffset < 0 )
            {
              desiredOffset = 0;
              break;
            }

            if( desiredChildIndex < 0 )
            {
              break;
            }


          }
          while( totalHeight < cachedActualHeight );

          this.SetVerticalOffset( ( double )desiredOffset );
        }
      }
      else
      {
        //Only do stuff if the item is out of the viewport.
        if( itemIndex < this.HorizontalOffset )
        {
          //this is easy, just set the item as the first one.
          this.SetHorizontalOffset( itemIndex );
        }
        else if( itemIndex > this.HorizontalOffset + this.ViewportWidth )
        {
          //this is a little trickier... I need to determine the best possible offset to display the item passed.
          int desiredChildIndex = childIndex;
          int desiredOffset = itemIndex;
          double cachedActualWidth = this.ActualWidth;
          double totalWidth = 0;
          bool passedOnce = false;

          do
          {
            //add the height of the current element to the total calculated height
            totalWidth += this.Children[ desiredChildIndex ].RenderSize.Width;

            //if the calculated height is still lower that the size of the viewport (render size)
            if( totalWidth < cachedActualWidth )
            {
              int currentIndex = generator.IndexFromGeneratorPosition( new GeneratorPosition( desiredChildIndex, 0 ) );
              //if the container is not for the "next" item in line... 
              if( currentIndex != desiredOffset )
              {
                desiredOffset++;
                break;
              }
              else
              {
                desiredChildIndex--;
                desiredOffset--;
                passedOnce = true;
              }
            }
            else if( passedOnce == true )
            {
              //since we exceeded the constraint, go back one step
              desiredOffset++;
            }

            if( desiredOffset < 0 )
            {
              desiredOffset = 0;
              break;
            }

            if( desiredChildIndex < 0 )
            {
              break;
            }

          }
          while( totalWidth < cachedActualWidth );

          this.SetHorizontalOffset( ( double )desiredOffset );
        }
      }
    }

    private void OnRequestBringIntoView( object sender, RequestBringIntoViewEventArgs e )
    {
      // Only perform this if we are virtualizing.  
      // If not, the ScrollViewer will take care of bringing the item into view.
      if( ( this.ScrollOwner == null ) || ( !this.ScrollOwner.CanContentScroll ) )
        return;

      // Only perform this if the StableScrolling feature is enabled.
      if( !this.StableScrollingEnabled )
        return;

      // If we are explicitly setting the focus on a cell, we don't want to bring it into view.
      // A bringIntoView would cause the HorizontalOffset to change is not a wanted behavior.
      // Therefore, flag the Request as handled and do nothing.
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

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

      // Before performing calculations, ensure that the element is a UIElement.
      FrameworkElement target = e.TargetObject as FrameworkElement;

      if( ( target == null ) || ( target.IsDescendantOf( this ) == false ) )
        return;

      Orientation orientation = this.Orientation;

      //mark the routed event as handled, since under even the worst circumstances, a new event will be raised.
      e.Handled = true;

      double acceptableThreshold;
      double actualVisibleValue;

      GeneralTransform itemToPanel = target.TransformToAncestor( this );
      Point itemOrigin = itemToPanel.Transform( new Point( 0, 0 ) );

      //Calculate the VisibleWidth/Height of the object within the Viewport.
      if( orientation == Orientation.Vertical )
      {
        acceptableThreshold = this.ViewportWidth * this.StableScrollingProportion;

        //item offset is positive in the viewport.
        if( itemOrigin.X >= 0 )
        {
          //if the item is totally visible
          if( target.ActualWidth + itemOrigin.X <= this.ViewportWidth )
          {
            //this will force the function to "cancel" the BringIntoView
            actualVisibleValue = this.ViewportWidth;
          }
          else
          {
            //item is not totally visible, calculate how much place it occupies in the viewport.
            actualVisibleValue = this.ViewportWidth - itemOrigin.X;
          }
        }
        //item offset is negative
        else
        {
          actualVisibleValue = target.ActualWidth - itemOrigin.X;
          if( actualVisibleValue > this.ViewportWidth )
          {
            //limit the value of actualVisibleValue to the Viewport size (to prevent eventual bugs [even if nothing is visible on the horizon for this])
            actualVisibleValue = this.ViewportWidth;
          }
        }
      }
      else
      {
        acceptableThreshold = this.ViewportHeight * this.StableScrollingProportion;

        //item offset is positive in the viewport.
        if( itemOrigin.Y >= 0 )
        {
          //if the item is totally visible
          if( target.ActualHeight + itemOrigin.Y <= this.ViewportHeight )
          {
            //this will force the function to "cancel" the BringIntoView
            actualVisibleValue = this.ViewportHeight;
          }
          else
          {
            //item is not totally visible, calculate how much place it occupies in the viewport.
            actualVisibleValue = this.ViewportHeight - itemOrigin.Y;
          }
        }
        //item offset is negative
        else
        {
          actualVisibleValue = target.ActualHeight - itemOrigin.Y;
          if( actualVisibleValue > this.ViewportHeight )
          {
            //limit the value of actualVisibleValue to the Viewport size (to prevent eventual bugs [even if nothing is visible on the horizon for this])
            actualVisibleValue = this.ViewportHeight;
          }
        }
      }

      bool needBringIntoView = false;
      Rect newRect = new Rect( 0, 0, target.ActualWidth, target.ActualHeight );

      //After the proportion visible from the object is calculated, compare with threshold
      if( actualVisibleValue < acceptableThreshold )
      {
        needBringIntoView = true;
        //the required threshold is not visible, modify the bounds of the rectangle 
        //to bring at Max, the desired threshold.
        if( orientation == Orientation.Vertical )
        {
          if( itemOrigin.X >= 0 )
          {
            newRect.Width = Math.Min( acceptableThreshold, target.ActualWidth );
          }
          else
          {
            newRect.X = target.ActualWidth - acceptableThreshold;
            if( newRect.X < 0 )
            {
              newRect.X = 0;
            }
          }
        }
        else
        {
          if( itemOrigin.Y >= 0 )
          {
            newRect.Height = Math.Min( acceptableThreshold, target.ActualHeight );
          }
          else
          {
            newRect.Y = target.ActualHeight - acceptableThreshold;
            if( newRect.Y < 0 )
            {
              newRect.Y = 0;
            }
          }
        }//end else orientation is Horizontal
      } // end if actuallyVisible < acceptable threshold
      else
      {
        //Determine if the item is totally or partially visible on the main scrolling axis.             
        if( orientation == Orientation.Vertical )
        {
          //If the Offset is negative, then it's sure its not totally visible
          if( itemOrigin.Y < 0 )
          {
            needBringIntoView = true;
          }
          else if( ( ( itemOrigin.Y + target.DesiredSize.Height ) > this.RenderSize.Height ) && ( target.DesiredSize.Height < this.RenderSize.Height ) )
          {
            needBringIntoView = true;
          }
        }
        else
        {
          //If the Offset is negative, then it's sure its not totally visible
          if( itemOrigin.X < 0 )
          {
            needBringIntoView = true;
          }
          else if( ( ( itemOrigin.X + target.DesiredSize.Width ) > this.RenderSize.Width ) && ( target.DesiredSize.Width < this.RenderSize.Width ) )
          {
            needBringIntoView = true;
          }
        }

        //if the item need to be scrolled in view
        if( needBringIntoView == true )
        {
          //the goal is to preserve the actual opposed axis scrolling
          if( orientation == Orientation.Vertical )
          {
            //calculate the starting point of the rectangle to view.
            newRect.X = itemOrigin.X * -1;
            if( newRect.X < 0 )
            {
              newRect.X = 0;
            }

            //if the item to be brough into view is part of the elements in a TableView that do no scroll
            if( ( dataGridControl != null )
              && ( dataGridControl.GetView() is TableView )
              && ( this.ComputedCanScrollHorizontally( target ) == false ) )
            {
              //then do not change the HorizontalOffset... set the same on the object as previously availlable.
              newRect.X = this.HorizontalOffset;
            }


            //if the rectangle of the object goes beyond the Viewport
            if( newRect.Right > this.ViewportWidth )
            {
              //subtract what goes beyond!
              newRect.Width = newRect.Width - ( newRect.Width - this.ViewportWidth ) - Math.Max( itemOrigin.X, 0 );
            }
          }
          else
          {
            //calculate the staring point if the rectangle to view.
            newRect.Y = itemOrigin.Y * -1;
            if( newRect.Y < 0 )
            {
              newRect.Y = 0;
            }

            //if the rectangle of the object goes beyond the Viewport
            if( newRect.Bottom > this.ViewportHeight )
            {
              //subtract what goes beyond!
              newRect.Height = newRect.Height - ( newRect.Height - this.ViewportHeight ) - Math.Max( itemOrigin.Y, 0 );
            }
          }
        } // end if needBringIntoView == true
      } // end else 


      //Call a new BringIntoView on the target element
      if( needBringIntoView == true )
        target.BringIntoView( newRect );
    }

    private bool ComputedCanScrollHorizontally( FrameworkElement target )
    {
      Debug.Assert( target != null );

      bool retval = true;

      DependencyObject visual = target;

      do
      {
        retval = TableView.GetCanScrollHorizontally( visual );

        if( retval == true )
        {
          visual = TreeHelper.GetParent( visual );
        }
      }
      while( ( visual != null ) && ( visual != this ) && ( retval == true ) );

      return retval;
    }

    private void InvalidateMeasureHelper( Orientation orientation )
    {
      if( ( m_layoutSuspended == true ) && ( m_layoutSuspendedOrientation == orientation ) )
      {
        m_layoutInvalidated = true;
      }
      else
      {
        this.InvalidateMeasure();

        m_layoutInvalidated = false;
      }

    }

    #region ICustomVirtualizingPanel Members

    void ICustomVirtualizingPanel.BringIntoView( int index )
    {
      if( this.Orientation == Orientation.Vertical )
      {
        //if the item is the first item out of the viewport (partially visible)
        if( index == ( this.VerticalOffset + this.ViewportHeight ) )
        {
          //shift the Viewport 1 item up
          this.SetVerticalOffset( this.VerticalOffset + 1 );
        }
        else
        {
          //otherwise, if the item is below or above viewport, then bring it as the first item in the viewport
          if( ( index < this.VerticalOffset ) || ( index > this.VerticalOffset + this.ViewportHeight ) )
          {
            this.SetVerticalOffset( index );
          }
        }
      }
      else
      {
        //if the item is the first item out of the viewport (partially visible)
        if( index == ( this.HorizontalOffset + this.ViewportWidth ) )
        {
          //shift the Viewport 1 item up
          this.SetHorizontalOffset( this.HorizontalOffset + 1 );
        }
        else
        {
          //otherwise, if the item is below or above viewport, then bring it as the first item in the viewport
          if( ( index < this.HorizontalOffset ) || ( index > this.HorizontalOffset + this.ViewportWidth ) )
          {
            this.SetHorizontalOffset( index );
          }
        }
      }
    }

    #endregion

    #region IScrollInfo Members

    public bool CanHorizontallyScroll
    {
      get
      {
        return m_canHScroll;
      }
      set
      {
        m_canHScroll = value;
      }
    }

    public bool CanVerticallyScroll
    {
      get
      {
        return m_canVScroll;
      }
      set
      {
        m_canVScroll = value;
      }
    }

    public double ExtentHeight
    {
      get
      {
        return m_extent.Height;
      }
    }

    public double ExtentWidth
    {
      get
      {
        return m_extent.Width;
      }
    }

    public double HorizontalOffset
    {
      get
      {
        return m_offset.X;
      }
    }

    public void LineDown()
    {
      double verticalOffset = this.VerticalOffset;

      // Prevent scrolling if we are already showing the last item.
      if( this.ExtentHeight <= ( verticalOffset + this.ViewportHeight ) )
        return;

      if( this.Orientation == Orientation.Vertical )
      {
        this.SetVerticalOffset( verticalOffset + 1 );
      }
      else
      {
        this.SetVerticalOffset( verticalOffset + ScrollViewerHelper.PixelScrollingCount );
      }
    }

    public void LineLeft()
    {
      if( this.Orientation == Orientation.Horizontal )
      {
        this.SetHorizontalOffset( this.HorizontalOffset - 1 );
      }
      else
      {
        this.SetHorizontalOffset( this.HorizontalOffset - ScrollViewerHelper.PixelScrollingCount );
      }
    }

    public void LineRight()
    {
      double horizontalOffset = this.HorizontalOffset;

      // Prevent scrolling if we are already showing the last item.
      if( this.ExtentWidth <= ( horizontalOffset + this.ViewportWidth ) )
        return;

      if( this.Orientation == Orientation.Horizontal )
      {
        this.SetHorizontalOffset( horizontalOffset + 1 );
      }
      else
      {
        this.SetHorizontalOffset( horizontalOffset + ScrollViewerHelper.PixelScrollingCount );
      }
    }

    public void LineUp()
    {
      if( this.Orientation == Orientation.Vertical )
      {
        this.SetVerticalOffset( this.VerticalOffset - 1 );
      }
      else
      {
        this.SetVerticalOffset( this.VerticalOffset - ScrollViewerHelper.PixelScrollingCount );
      }
    }

    public Rect MakeVisible( Visual visual, Rect rectangle )
    {
      UIElementCollection children = this.InternalChildren;
      IItemContainerGenerator generator = this.CustomItemContainerGenerator;

      Rect actualRect = rectangle;

      //determine the index of the internal child for this visual
      int internalChildIndex = children.IndexOf( ( UIElement )visual );

      //if the visual is not a direct child of the panel
      if( internalChildIndex == -1 )
      {
        bool foundAncestor = false;
        //cycle through all the visual children of the panel
        foreach( UIElement element in children )
        {
          //if the visual child of the panel is the ancestor of the given visual
          if( element.IsAncestorOf( visual ) == true )
          {
            //store the visual child index
            internalChildIndex = children.IndexOf( element );

            //transform the given rectangle into the actual child coordinate system
            actualRect = visual.TransformToAncestor( element ).TransformBounds( rectangle );

            foundAncestor = true;
            break;
          }
        }

        Debug.Assert( foundAncestor == true, "Cannot find the actual child for the given Visual" );
      }

      int itemIndex = generator.IndexFromGeneratorPosition( new GeneratorPosition( internalChildIndex, 0 ) );

      if( itemIndex != -1 )
      {
        return MakeVisible( internalChildIndex, itemIndex, actualRect );
      }
      else
      {
        return actualRect;
      }
    }

    public void MouseWheelDown()
    {
      double verticalOffset = this.VerticalOffset;

      // Prevent scrolling if we are already showing the last item.
      if( this.ExtentHeight <= ( verticalOffset + this.ViewportHeight ) )
        return;

      if( this.Orientation == Orientation.Vertical )
      {
        this.SetVerticalOffset( verticalOffset + SystemParameters.WheelScrollLines );
      }
      else
      {
        this.SetVerticalOffset( verticalOffset
          + ( SystemParameters.WheelScrollLines * ScrollViewerHelper.PixelScrollingCount ) );
      }
    }

    public void MouseWheelLeft()
    {
      if( this.Orientation == Orientation.Horizontal )
      {
        this.SetHorizontalOffset( this.HorizontalOffset - SystemParameters.WheelScrollLines );
      }
      else
      {
        this.SetHorizontalOffset( this.HorizontalOffset
          - ( SystemParameters.WheelScrollLines * ScrollViewerHelper.PixelScrollingCount ) );
      }
    }

    public void MouseWheelRight()
    {
      double horizontalOffset = this.HorizontalOffset;

      // Prevent scrolling if we are already showing the last item.
      if( this.ExtentWidth <= ( horizontalOffset + this.ViewportWidth ) )
        return;

      if( this.Orientation == Orientation.Horizontal )
      {
        this.SetHorizontalOffset( horizontalOffset + SystemParameters.WheelScrollLines );
      }
      else
      {
        this.SetHorizontalOffset( horizontalOffset
          + ( SystemParameters.WheelScrollLines * ScrollViewerHelper.PixelScrollingCount ) );
      }
    }

    public void MouseWheelUp()
    {
      if( this.Orientation == Orientation.Vertical )
      {
        this.SetVerticalOffset( this.VerticalOffset - SystemParameters.WheelScrollLines );
      }
      else
      {
        this.SetVerticalOffset( this.VerticalOffset
          - ( SystemParameters.WheelScrollLines * ScrollViewerHelper.PixelScrollingCount ) );
      }
    }

    public void PageDown()
    {
      double scrollOffset = ( this.ViewportHeight + this.VerticalOffset );

      // Prevent scrolling if we are already showing the last item.
      if( this.ExtentHeight <= scrollOffset )
        return;

      this.SetVerticalOffset( scrollOffset );
    }

    public void PageLeft()
    {
      this.SetHorizontalOffset( this.HorizontalOffset - this.ViewportWidth );
    }

    public void PageRight()
    {
      double scrollOffset = ( this.HorizontalOffset + this.ViewportWidth );

      // Prevent scrolling if we are already showing the last item.
      if( this.ExtentWidth <= scrollOffset )
        return;

      this.SetHorizontalOffset( scrollOffset );
    }

    public void PageUp()
    {
      this.SetVerticalOffset( this.VerticalOffset - this.ViewportHeight );
    }

    public ScrollViewer ScrollOwner
    {
      get
      {
        return m_owner;
      }
      set
      {
        m_owner = value;
      }
    }

    public void SetHorizontalOffset( double offset )
    {
      bool invalidate = false;

      Orientation orientation = this.Orientation;

      if( Double.IsInfinity( offset ) == true )
      {
        if( offset == Double.PositiveInfinity )
        {
          if( orientation == Orientation.Horizontal )
          {
            offset = this.ExtentWidth - 1;
          }
          else
          {
            offset = this.ExtentWidth - this.ViewportWidth;
          }
        }
        else //negative infinity
        {
          offset = 0;
        }
      }
      else
      {
        double width = ( orientation == Orientation.Vertical ) ? this.ExtentWidth : this.ItemCount;

        if( offset < 0 || m_viewport.Width >= width )
        {
          offset = 0;
        }
        else
        {
          if( orientation == Orientation.Horizontal )
          {
            if( offset >= width )
            {
              offset = width - 1;
            }
          }
          else
          {
            if( offset > ( width - m_viewport.Width ) )
            {
              offset = width - m_viewport.Width;
            }
          }
        }
      }

      if( m_offset.X != offset )
      {
        m_offset.X = offset;
        invalidate = true;
      }

      if( invalidate == true )
      {
        if( m_owner != null )
          m_owner.InvalidateScrollInfo();

        this.InvalidateMeasureHelper( Orientation.Horizontal );
      }
    }

    public void SetVerticalOffset( double offset )
    {
      bool invalidate = false;

      Orientation orientation = this.Orientation;

      if( Double.IsInfinity( offset ) == true )
      {
        if( offset == Double.PositiveInfinity )
        {
          if( orientation == Orientation.Vertical )
          {
            offset = this.ExtentHeight - 1;
          }
          else
          {
            offset = this.ExtentHeight - this.ViewportHeight;
          }
        }
        else //negative infinity
        {
          offset = 0;
        }
      }
      else
      {
        double height = ( orientation == Orientation.Horizontal ) ? this.ExtentHeight : this.ItemCount;

        if( offset < 0 || m_viewport.Height >= height )
        {
          offset = 0;
        }
        else
        {
          if( orientation == Orientation.Vertical )
          {
            if( offset >= height )
            {
              offset = height - 1;
            }
          }
          else
          {
            if( offset > ( height - m_viewport.Height ) )
            {
              offset = height - m_viewport.Height;
            }
          }
        }
      }

      if( m_offset.Y != offset )
      {
        m_offset.Y = offset;
        invalidate = true;
      }

      if( invalidate == true )
      {
        if( m_owner != null )
          m_owner.InvalidateScrollInfo();

        this.InvalidateMeasureHelper( Orientation.Vertical );
      }
    }

    public double VerticalOffset
    {
      get
      {
        return m_offset.Y;
      }
    }

    public double ViewportHeight
    {
      get
      {
        return m_viewport.Height;
      }
    }

    public double ViewportWidth
    {
      get
      {
        return m_viewport.Width;
      }
    }

    #endregion

    #region IDeferableScrollInfoRefresh Members

    IDisposable IDeferableScrollInfoRefresh.DeferScrollInfoRefresh( Orientation orientation )
    {
      if( this.Orientation == orientation )
      {
        return new LayoutSuspendedHelper( this, orientation );
      }

      return null;
    }

    #endregion

    private bool m_canHScroll = false;
    private bool m_canVScroll = false;
    private ScrollViewer m_owner = null;
    private Size m_extent = new Size( 0, 0 );
    private Size m_viewport = new Size( 0, 0 );
    private Point m_offset = new Point( 0, 0 );

    private int m_actualFirstItem = -1;
    private int m_actualLastItem = -1;

    //Those 3 members are used in conjunction with the IDeferableScrollInfoRefresh interface
    private bool m_layoutSuspended; // = false
    private Orientation m_layoutSuspendedOrientation;
    private bool m_layoutInvalidated; // = false

    private DispatcherOperation m_cleanupOperation = null;
    private int m_currentCleanupIndex = -1;

    private int m_initialCleanupPriority = ( ( int )DispatcherPriority.Background );
    private int m_cleanupPriorityModifier = 0;

    private Dictionary<string, double> m_desiredSizeDictionary = new Dictionary<string, double>();
    private Dictionary<string, double> m_previousSizeDictionary = new Dictionary<string, double>();

    private delegate void CleanupHandler();

    private static readonly Thickness DefaultThickness = new Thickness();
    private static readonly Size InfiniteSize = new Size( Double.PositiveInfinity, Double.PositiveInfinity );

    private sealed class LayoutSuspendedHelper : IDisposable
    {
      public LayoutSuspendedHelper( VirtualizingStackPanel panel, Orientation orientation )
      {
        if( panel == null )
          throw new ArgumentNullException( "panel" );

        m_panel = panel;

        m_panel.m_layoutSuspendedOrientation = orientation;
        m_panel.m_layoutSuspended = true;
      }

      #region IDisposable Members

      public void Dispose()
      {
        m_panel.m_layoutSuspended = false;

        if( m_panel.m_layoutInvalidated == true )
        {
          m_panel.InvalidateMeasureHelper( m_panel.m_layoutSuspendedOrientation );
        }
      }

      #endregion

      private VirtualizingStackPanel m_panel; // = null
    }
  }
}
