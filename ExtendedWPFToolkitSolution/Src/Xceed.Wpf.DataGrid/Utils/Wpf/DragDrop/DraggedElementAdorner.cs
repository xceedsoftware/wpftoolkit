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
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Xceed.Utils.Wpf.DragDrop
{
  internal class DraggedElementAdorner : Adorner
  {
    public DraggedElementAdorner( UIElement adornedElement, AdornerLayer adornerLayer )
      : base( adornedElement )
    {
      FrameworkElement element = this.AdornedElement as FrameworkElement;
      FrameworkElement layer = adornerLayer as FrameworkElement;

      if( ( element != null ) && ( layer != null ) && ( element.FlowDirection != layer.FlowDirection ) )
      {
        m_invertXAxis = true;
      }
    }

    #region Offset Property

    public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
      "Offset",
      typeof( Point ),
      typeof( DraggedElementAdorner ),
      new FrameworkPropertyMetadata( new PropertyChangedCallback( DraggedElementAdorner.OnOffsetChanged ) ) );

    public Point Offset
    {
      get
      {
        return ( Point )this.GetValue( DraggedElementAdorner.OffsetProperty );
      }
      set
      {
        this.SetValue( DraggedElementAdorner.OffsetProperty, value );
      }
    }

    private static void OnOffsetChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DraggedElementAdorner adorner = sender as DraggedElementAdorner;

      if( adorner == null )
        return;

      adorner.UpdatePosition();
    }

    #endregion

    #region AdornedElementImage Property

    public Rectangle AdornedElementImage
    {
      get
      {
        if( m_adornedElementImage == null )
        {
          m_adornedElementImage = this.InitializeAdornedElementImage();
        }

        return m_adornedElementImage;
      }
    }

    #endregion

    public void SetOffset( Point offset )
    {
      this.Offset = offset;
    }

    public override GeneralTransform GetDesiredTransform( GeneralTransform transform )
    {
      GeneralTransformGroup transformGroup = new GeneralTransformGroup();

      transformGroup.Children.Add( base.GetDesiredTransform( transform ) );
      transformGroup.Children.Add( new TranslateTransform( ( m_invertXAxis == false ) ? this.Offset.X : -this.Offset.X, this.Offset.Y ) );

      return transformGroup;
    }

    protected virtual Rectangle InitializeAdornedElementImage()
    {
      Rectangle rectangle = new Rectangle();
      VisualBrush brush = new VisualBrush( this.AdornedElement );
      brush.Opacity = 0.75;
      brush.Stretch = Stretch.None;
      brush.AlignmentX = AlignmentX.Left;
      brush.AlignmentY = AlignmentY.Top;
      rectangle.Fill = brush;

      return rectangle;
    }

    protected override Size MeasureOverride( Size constraint )
    {
      this.AdornedElement.Measure( constraint );

      return this.AdornedElement.RenderSize;
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
      this.AdornedElementImage.Width = finalSize.Width;
      this.AdornedElementImage.Height = finalSize.Height;
      this.AdornedElementImage.Arrange( new Rect( finalSize ) );

      return finalSize;
    }

    protected override Visual GetVisualChild( int index )
    {
      if( index != 0 )
        throw new ArgumentOutOfRangeException( "index", index, "Index must be zero: a DraggedElementAdorner can only have one child." );

      return this.AdornedElementImage;
    }

    protected override int VisualChildrenCount
    {
      get
      {
        return 1;
      }
    }

    private void UpdatePosition()
    {
      AdornerLayer adornerLayer = this.Parent as AdornerLayer;

      if( adornerLayer != null )
      {
        adornerLayer.Update( this.AdornedElement );
      }
    }

    private Rectangle m_adornedElementImage;
    private bool m_invertXAxis; // = false 
  }
}
