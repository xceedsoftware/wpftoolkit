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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  public class StaircasePanel : Panel
  {
    #region ConnectionLineAlignment Property

    public static readonly DependencyProperty ConnectionLineAlignmentProperty =
      DependencyProperty.Register(
      "ConnectionLineAlignment",
      typeof( ConnectionLineAlignment ),
      typeof( StaircasePanel ),
      new FrameworkPropertyMetadata( ConnectionLineAlignment.RightToBottom,
        FrameworkPropertyMetadataOptions.AffectsRender ) );

    public ConnectionLineAlignment ConnectionLineAlignment
    {
      get
      {
        return ( ConnectionLineAlignment )this.GetValue( StaircasePanel.ConnectionLineAlignmentProperty );
      }
      set
      {
        this.SetValue( StaircasePanel.ConnectionLineAlignmentProperty, value );
      }
    }

    #endregion ConnectionLineAlignment Property

    #region ConnectionLineOffset Property

    public static readonly DependencyProperty ConnectionLineOffsetProperty =
      DependencyProperty.Register(
      "ConnectionLineOffset",
      typeof( double ),
      typeof( StaircasePanel ),
      new FrameworkPropertyMetadata( 5.0d, FrameworkPropertyMetadataOptions.AffectsRender ) );

    public double ConnectionLineOffset
    {
      get
      {
        return ( double )this.GetValue( StaircasePanel.ConnectionLineOffsetProperty );
      }
      set
      {
        this.SetValue( StaircasePanel.ConnectionLineOffsetProperty, value );
      }
    }

    #endregion ConnectionLineOffset Property

    #region StairHeight Property

    public static readonly DependencyProperty StairHeightProperty =
      DependencyProperty.Register(
      "StairHeight",
      typeof( double ),
      typeof( StaircasePanel ),
      new FrameworkPropertyMetadata( 10.0d,
        FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure ) );

    public double StairHeight
    {
      get
      {
        return ( double )this.GetValue( StaircasePanel.StairHeightProperty );
      }
      set
      {
        this.SetValue( StaircasePanel.StairHeightProperty, value );
      }
    }

    #endregion StairHeight Property

    #region StairSpacing Property

    public static readonly DependencyProperty StairSpacingProperty =
      DependencyProperty.Register(
      "StairSpacing",
      typeof( double ),
      typeof( StaircasePanel ),
      new FrameworkPropertyMetadata( 5.0d,
        FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure ) );

    public double StairSpacing
    {
      get
      {
        return ( double )this.GetValue( StaircasePanel.StairSpacingProperty );
      }
      set
      {
        this.SetValue( StaircasePanel.StairSpacingProperty, value );
      }
    }

    #endregion StairSpacing Property

    #region ConnectionLinePen Property

    public static readonly DependencyProperty ConnectionLinePenProperty =
      DependencyProperty.Register(
      "ConnectionLinePen",
      typeof( Pen ),
      typeof( StaircasePanel ),
      new FrameworkPropertyMetadata(
        new Pen( SystemColors.ControlTextBrush, 2d ),
        FrameworkPropertyMetadataOptions.AffectsRender ) );

    public Pen ConnectionLinePen
    {
      get
      {
        return ( Pen )this.GetValue( StaircasePanel.ConnectionLinePenProperty );
      }
      set
      {
        this.SetValue( StaircasePanel.ConnectionLinePenProperty, value );
      }
    }

    #endregion ConnectionLinePen Property

    protected override Size MeasureOverride( Size availableSize )
    {
      Size infiniteSize = new Size( double.PositiveInfinity, double.PositiveInfinity );
      Size actualRequestedSize = new Size();

      //for all children of the panel
      for( int i = 0; i < this.InternalChildren.Count; i++ )
      {
        UIElement item = this.InternalChildren[ i ];

        //measure the child with no constraints
        item.Measure( infiniteSize );

        //add the desired width of the item to the panel's requested width
        actualRequestedSize.Width += item.DesiredSize.Width;

        //if this pass is not for the last item in the collection
        if( i < ( this.InternalChildren.Count - 1 ) )
        {
          //add the spacing factor as well
          actualRequestedSize.Width += this.StairSpacing;
        }

        //calculate the remaining 
        double itemBottom = ( i * this.StairHeight ) + item.DesiredSize.Height;
        if( itemBottom > actualRequestedSize.Height )
        {
          actualRequestedSize.Height = itemBottom;
        }
      }

      return actualRequestedSize;
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
      Point offset = new Point( 0, 0 );

      Rect itemRect = new Rect( 0, 0, 0, 0 );

      //for all children of the panel
      for( int i = 0; i < this.InternalChildren.Count; i++ )
      {
        UIElement item = this.InternalChildren[ i ];

        itemRect = new Rect( offset, item.DesiredSize );

        item.Arrange( itemRect );

        //if not the last item
        if( i < ( this.InternalChildren.Count - 1 ) )
        {
          //add to the offset, so next item is positionned properly
          offset.X += item.DesiredSize.Width + this.StairSpacing;
          offset.Y += this.StairHeight;
        }
      }

      if( double.IsInfinity( finalSize.Width ) == true )
      {
        finalSize.Width = itemRect.Right;
      }

      if( double.IsInfinity( finalSize.Height ) == true )
      {
        finalSize.Height = itemRect.Bottom;
      }

      return finalSize;
    }

    protected override void OnRender( DrawingContext drawingContext )
    {
      if( this.ConnectionLinePen != null )
      {
        bool left = false;
        bool top = false;
        bool middle = false;

        switch( this.ConnectionLineAlignment )
        {
          case ConnectionLineAlignment.LeftToBottom:
            left = true;
            break;

          case ConnectionLineAlignment.RightToTop:
            top = true;
            break;

          case ConnectionLineAlignment.LeftToTop:
            left = true;
            top = true;
            break;

          case ConnectionLineAlignment.CenterToCenter:
            middle = true;
            break;

          case ConnectionLineAlignment.RightToBottom:
          default:
            break;
        }

        //cycling with count -1 because we don't want to draw a ligne after last item
        for( int i = 0; i < this.InternalChildren.Count - 1; i++ )
        {
          UIElement startItem = this.InternalChildren[ i ];
          UIElement endItem = this.InternalChildren[ i + 1 ];

          Vector startOffset = VisualTreeHelper.GetOffset( startItem );
          Vector endOffset = VisualTreeHelper.GetOffset( endItem );

          Size startSize = startItem.RenderSize;
          Size endSize = endItem.RenderSize;

          double startPointX = 0.0d;
          double startPointY = 0.0d;
          double endPointX = 0.0d;
          double endPointY = 0.0d;

          List<PathSegment> myPathSegments = null;

          GuidelineSet guidelineSet = new GuidelineSet();
          drawingContext.PushGuidelineSet( guidelineSet );

          if( middle == true )
          {
            startPointX = startOffset.X + startSize.Width + this.ConnectionLineOffset;

            startPointY = startOffset.Y + ( startSize.Height / 2 );

            endPointY = endOffset.Y + ( endSize.Height / 2 );

            endPointX = endOffset.X - this.ConnectionLineOffset;

            double deltaX = ( endPointX - startPointX );

            guidelineSet.GuidelinesX.Add( startPointX );
            guidelineSet.GuidelinesX.Add( startPointX + ( deltaX / 2 ) + 0.5d );
            guidelineSet.GuidelinesX.Add( startPointX + deltaX );
            guidelineSet.GuidelinesY.Add( startPointY );
            guidelineSet.GuidelinesY.Add( endPointY );

            myPathSegments = new List<PathSegment>( 4 ); //we know there are going be only 4 segments
            myPathSegments.Add( new LineSegment( new Point( startPointX, startPointY ), true ) );
            myPathSegments.Add( new LineSegment( new Point( startPointX + ( deltaX / 2 ), startPointY ), true ) );
            myPathSegments.Add( new LineSegment( new Point( startPointX + ( deltaX / 2 ), endPointY ), true ) );
            myPathSegments.Add( new LineSegment( new Point( startPointX + deltaX, endPointY ), true ) );

          }
          else
          {
            if( left == true )
            {
              startPointX = startOffset.X + this.ConnectionLineOffset;
            }
            else
            {
              startPointX = startOffset.X + startSize.Width - this.ConnectionLineOffset;
            }

            startPointY = startOffset.Y + startSize.Height;

            if( top == true )
            {
              endPointY = endOffset.Y + this.ConnectionLineOffset;
            }
            else
            {
              endPointY = endOffset.Y + endSize.Height - this.ConnectionLineOffset;
            }

            endPointX = endOffset.X;

            guidelineSet.GuidelinesX.Add( startPointX );
            guidelineSet.GuidelinesX.Add( endPointX );
            guidelineSet.GuidelinesY.Add( startPointY );
            guidelineSet.GuidelinesY.Add( endPointY );

            myPathSegments = new List<PathSegment>( 2 ); //we know there are going be only 2 segments
            myPathSegments.Add( new LineSegment( new Point( startPointX, endPointY ), true ) );
            myPathSegments.Add( new LineSegment( new Point( endPointX, endPointY ), true ) );
          }

          PathFigure myPathFigure = new PathFigure( new Point( startPointX, startPointY ), myPathSegments, false );

          PathGeometry myPathGeometry = new PathGeometry();
          myPathGeometry.Figures.Add( myPathFigure );

          drawingContext.DrawGeometry( null, this.ConnectionLinePen, myPathGeometry );

          // pop the context to remove the GuidelineSet
          drawingContext.Pop();
        }
      }

      base.OnRender( drawingContext );
    }

    protected override void OnChildDesiredSizeChanged( UIElement child )
    {
      base.OnChildDesiredSizeChanged( child );

      // OnRender is not automatically called when a child's size changes.
      // We have to do it manually if we want the ConnectionLines to adapt.
      this.InvalidateVisual();
    }

    protected override void OnVisualChildrenChanged( DependencyObject visualAdded, DependencyObject visualRemoved )
    {
      base.OnVisualChildrenChanged( visualAdded, visualRemoved );

      // OnRender is not automatically called when a new child is added or removed from 
      // the Panel. We have to do it manually if we want to redraw the ConnectionLines.
      this.InvalidateVisual();
    }
  }
}
