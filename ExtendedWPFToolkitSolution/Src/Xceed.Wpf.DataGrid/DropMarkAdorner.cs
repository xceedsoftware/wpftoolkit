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
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  internal class DropMarkAdorner : Adorner
  {
    // A null value for the pen parameter means that the adorner will not show any visual cue.
    public DropMarkAdorner( UIElement adornedElement, Pen pen, DropMarkOrientation orientation )
      : base( adornedElement )
    {
      Debug.Assert( orientation != DropMarkOrientation.Default, "A DropMarkAdorner without a specific orientation should virtually never happen. The only known way to do this would be to have a grid element not hosted in a DataGridControl. The DropMarkAdorner should react well to this nonetheless." );
      m_pen = pen;
      m_orientation = orientation;

      this.IsHitTestVisible = false;
    }

    public DropMarkAlignment Alignment
    {
      get
      {
        return m_alignment;
      }
    }

    // This Property does not use the Alignment paradigm. It allows to manually set the
    // position of the drop mark.
    #region HorizontalPosition Property

    public static readonly DependencyProperty HorizontalPositionProperty = DependencyProperty.Register(
      "HorizontalPosition",
      typeof( double ),
      typeof( DropMarkAdorner ),
      new FrameworkPropertyMetadata(
        0d,
        new PropertyChangedCallback( DropMarkAdorner.OnHorizontalPositionChanged ) ) );

    public double HorizontalPosition
    {
      get
      {
        return ( double )this.GetValue( DropMarkAdorner.HorizontalPositionProperty );
      }
      set
      {
        this.SetValue( DropMarkAdorner.HorizontalPositionProperty, value );
      }
    }

    private static void OnHorizontalPositionChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DropMarkAdorner adorner = sender as DropMarkAdorner;

      if( adorner == null )
        return;

      adorner.UpdatePosition();
    }

    #endregion


    public void ForceAlignment( DropMarkAlignment alignment )
    {
      m_alignment = alignment;
    }

    public void UpdateAlignment( Point mousePosition )
    {
      DropMarkAlignment alignment = DropMarkAlignment.Near;

      if( m_orientation == DropMarkOrientation.Horizontal )
      {
        // A horizontal drop mark is either displayed at the top or at the bottom of the AdornedElement.
        Rect hitTestRect = new Rect( 0d, 0d, this.AdornedElement.RenderSize.Width, this.AdornedElement.RenderSize.Height / 2 );

        if( hitTestRect.Contains( mousePosition ) )
        {
          alignment = DropMarkAlignment.Near;
        }
        else
        {
          alignment = DropMarkAlignment.Far;
        }
      }
      else
      {
        // A vertical drop mark is either displayed at the left or at the right of the AdornedElement.
        Rect hitTestRect = new Rect( 0d, 0d, this.AdornedElement.RenderSize.Width / 2, this.AdornedElement.RenderSize.Height );

        if( hitTestRect.Contains( mousePosition ) )
        {
          alignment = DropMarkAlignment.Near;
        }
        else
        {
          alignment = DropMarkAlignment.Far;
        }
      }

      if( alignment != m_alignment )
      {
        m_alignment = alignment;
        this.UpdatePosition();
      }
    }

    protected override Size MeasureOverride( Size constraint )
    {
      this.AdornedElement.Measure( constraint );

      Size size = Size.Empty;

      if( m_pen != null )
      {
        switch( m_orientation )
        {
          case DropMarkOrientation.Default:
          case DropMarkOrientation.Vertical:
            size = new Size( m_pen.Thickness, this.AdornedElement.RenderSize.Height );
            break;
          case DropMarkOrientation.Horizontal:
            size = new Size( this.AdornedElement.RenderSize.Width, m_pen.Thickness );
            break;
          default:
            throw new DataGridInternalException();
        }
      }

      return size;
    }

    public override GeneralTransform GetDesiredTransform( GeneralTransform transform )
    {
      double offsetX = this.HorizontalPosition;
      double offsetY = 0d;

      if( m_pen != null )
      {
        switch( m_orientation )
        {
          case DropMarkOrientation.Default:
          case DropMarkOrientation.Vertical:
            offsetX -= m_pen.Thickness / 2;
            break;
          case DropMarkOrientation.Horizontal:
            offsetY -= m_pen.Thickness / 2;
            break;
        }

        if( m_alignment == DropMarkAlignment.Far )
        {
          if( m_orientation == DropMarkOrientation.Vertical )
          {
            // A "far" vertical drop mark is displayed at the right
            offsetX += this.AdornedElement.RenderSize.Width;
          }
          else
          {
            // A "far" horizontal drop mark is displayed at the bottom
            offsetY += this.AdornedElement.RenderSize.Height;
          }
        }
      }

      GeneralTransformGroup transformGroup = new GeneralTransformGroup();

      transformGroup.Children.Add( base.GetDesiredTransform( transform ) );
      transformGroup.Children.Add( new TranslateTransform( offsetX, offsetY ) );

      return transformGroup;
    }

    protected override void OnRender( DrawingContext drawingContext )
    {
      if( m_pen != null )
      {
        Size renderSize = this.RenderSize;
        Point startPoint;
        Point endPoint;

        switch( m_orientation )
        {
          case DropMarkOrientation.Default:
          case DropMarkOrientation.Vertical:
            startPoint = new Point( renderSize.Width / 2, 0d );
            endPoint = new Point( startPoint.X, renderSize.Height );
            break;
          case DropMarkOrientation.Horizontal:
            startPoint = new Point( 0d, renderSize.Height / 2 );
            endPoint = new Point( renderSize.Width, startPoint.Y );
            break;
          default:
            throw new DataGridInternalException();
        }

        drawingContext.DrawLine( m_pen, startPoint, endPoint );
      }

      base.OnRender( drawingContext );
    }

    private void UpdatePosition()
    {
      AdornerLayer adornerLayer = this.Parent as AdornerLayer;

      if( adornerLayer != null )
      {
        adornerLayer.Update( this.AdornedElement );
      }
    }

    private DropMarkAlignment m_alignment = DropMarkAlignment.ExplicitPosition;
    private Pen m_pen;
    private DropMarkOrientation m_orientation;
  }
}
