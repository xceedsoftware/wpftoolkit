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
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class DropMarkAdorner : Adorner
  {
    // A null value for the pen parameter means that the adorner will not show any visual cue.
    internal DropMarkAdorner( UIElement adornedElement, Pen pen, DropMarkOrientation orientation )
      : base( adornedElement )
    {
      Debug.Assert( orientation != DropMarkOrientation.Default, "A DropMarkAdorner without a specific orientation should virtually never happen. The only known way to do this would be to have a grid element not hosted in a DataGridControl. The DropMarkAdorner should react well to this nonetheless." );
      m_pen = pen;
      m_orientation = orientation;
      m_render = ( pen != null );

      this.IsHitTestVisible = false;
    }

    #region HorizontalPosition Property

    public static readonly DependencyProperty HorizontalPositionProperty = DependencyProperty.Register(
      "HorizontalPosition",
      typeof( double ),
      typeof( DropMarkAdorner ),
      new FrameworkPropertyMetadata( 0d, new PropertyChangedCallback( DropMarkAdorner.OnHorizontalPositionChanged ) ) );

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
      var self = sender as DropMarkAdorner;
      if( self == null )
        return;

      self.Update();
    }

    #endregion

    #region Alignment Property

    internal DropMarkAlignment Alignment
    {
      get
      {
        return m_alignment;
      }
      set
      {
        if( value == m_alignment )
          return;

        m_alignment = value;

        this.Update();
      }
    }

    private DropMarkAlignment m_alignment = DropMarkAlignment.ExplicitPosition;

    #endregion

    #region AdornerLayout Private Property

    private AdornerLayer AdornerLayer
    {
      get
      {
        return ( this.Parent as AdornerLayer );
      }
    }

    #endregion

    protected override void OnRender( DrawingContext drawingContext )
    {
      base.OnRender( drawingContext );

      if( !m_render || ( m_pen == null ) )
        return;

      var renderSize = this.AdornedElement.RenderSize;

      Point startPoint = new Point( this.HorizontalPosition, 0d );
      Point endPoint;

      switch( m_orientation )
      {
        case DropMarkOrientation.Default:
        case DropMarkOrientation.Vertical:
          {
            if( m_alignment == DropMarkAlignment.Far )
            {
              startPoint.X += renderSize.Width;
            }

            endPoint = new Point( startPoint.X, renderSize.Height );
          }
          break;
        case DropMarkOrientation.Horizontal:
          {
            if( m_alignment == DropMarkAlignment.Far )
            {
              startPoint.Y += renderSize.Height;
            }

            endPoint = new Point( renderSize.Width, startPoint.Y );
          }
          break;
        default:
          return;
      }

      drawingContext.DrawLine( m_pen, startPoint, endPoint );
    }

    internal void UpdateAlignment( RelativePoint mousePosition )
    {
      var relativePosition = mousePosition.GetPoint( this.AdornedElement );
      var elementSize = this.AdornedElement.RenderSize;

      if( m_orientation == DropMarkOrientation.Horizontal )
      {
        this.Alignment = ( relativePosition.Y < elementSize.Height / 2d ) ? DropMarkAlignment.Near : DropMarkAlignment.Far;
      }
      else
      {
        this.Alignment = ( relativePosition.X < elementSize.Width / 2d ) ? DropMarkAlignment.Near : DropMarkAlignment.Far;
      }
    }

    private void Update()
    {
      if( !m_render )
        return;

      if( !this.InvalidateAdornerLayer() )
        return;

      m_render = false;
      this.LayoutUpdated += new EventHandler( this.OnLayoutUpdated );
    }

    private bool InvalidateAdornerLayer()
    {
      var adornerLayer = this.AdornerLayer;
      if( adornerLayer == null )
        return false;

      adornerLayer.Update( this.AdornedElement );
      return true;
    }

    private void OnLayoutUpdated( object sender, EventArgs e )
    {
      m_render = true;

      this.LayoutUpdated -= new EventHandler( this.OnLayoutUpdated );
      this.InvalidateVisual();
      this.InvalidateAdornerLayer();
    }

    private readonly DropMarkOrientation m_orientation;
    private readonly Pen m_pen;
    private bool m_render;
  }
}
