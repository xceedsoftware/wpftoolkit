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
using System.Linq;
using System.Text;
using Xceed.Utils.Wpf.DragDrop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Controls;

namespace Xceed.Wpf.DataGrid.Views
{
  internal class AnimatedDraggedElementAdorner : DraggedElementAdorner
  {
    #region Constructors

    public AnimatedDraggedElementAdorner( UIElement adornedElement, AdornerLayer adornerLayer )
      : this( adornedElement, adornerLayer, false )
    {
    }

    public AnimatedDraggedElementAdorner( UIElement adornedElement, AdornerLayer adornerLayer, bool deepCopy )
      : base( adornedElement, adornerLayer )
    {
      this.DeepCopy = deepCopy;
      this.IsHitTestVisible = false;
    }

    #endregion

    #region DeepCopy Property

    public bool DeepCopy
    {
      get;
      private set;
    }

    #endregion

    #region Protected Methods

    protected override Rectangle InitializeAdornedElementImage()
    {
      Rect adornedBounds = VisualTreeHelper.GetDescendantBounds( this.AdornedElement );

      // Round the height and width of the bounds to reduce the 
      // blur effect caused by the RenderTargetBitmap. 
      // When drawing Text into RenderTargetBitmap, the ClearType 
      // reverts to grayscale causing a blur effect. If there is 
      // also an extrapolation, the blur effect will be worst.
      int roundedHeight = ( int )Math.Round( adornedBounds.Height, MidpointRounding.ToEven );
      int roundedWidth = ( int )Math.Round( adornedBounds.Width, MidpointRounding.ToEven );

      VisualBrush brush = new VisualBrush( this.AdornedElement );

      Rectangle rectangle = new Rectangle();

      // Only if we have something to adorn
      if( this.DeepCopy
        && ( ( roundedWidth > 0 ) && ( roundedHeight > 0 ) ) )
      {
        try
        {
          RenderTargetBitmap bitmap = new RenderTargetBitmap( roundedWidth,
            roundedHeight,
            96,
            96,
            PixelFormats.Pbgra32 );

          DrawingVisual drawingVisual = new DrawingVisual();

          using( DrawingContext context = drawingVisual.RenderOpen() )
          {
            Rect finalRect = new Rect( 0,
                0,
                roundedWidth,
                roundedHeight );

            context.DrawRectangle( brush, null, finalRect );
          }

          bitmap.Render( drawingVisual );

          // Ensure to set the Height and Width
          // values for the Fill does not resize the 
          // rectangle if it is larger. This also
          // reduce the blur effect.
          rectangle.Height = roundedHeight;
          rectangle.Width = roundedWidth;
          rectangle.UpdateLayout();

          // Adding BitmapScallingMode using any other BitmapScalingMode cause some
          // blur in the resulting Bitmap
          RenderOptions.SetBitmapScalingMode( rectangle, BitmapScalingMode.NearestNeighbor );
          rectangle.Fill = new ImageBrush( bitmap );

          // Translate the Top Left corner of the rectangle that
          // contains the AdornedElement
          if( !adornedBounds.Size.IsEmpty )
          {
            rectangle.RenderTransform = new TranslateTransform( adornedBounds.X, adornedBounds.Y );
          }
        }
        catch( Exception )
        {
          // Avoid any exception and use the brush itself
          rectangle.Fill = brush;
        }
      }
      else
      {
        rectangle.Fill = brush;
      }

      return rectangle;
    }

    protected override Size MeasureOverride( Size constraint )
    {
      this.AdornedElement.Measure( constraint );

      // Ensure to return the DescendantBounds to take Margins and Padding
      // into consideration
      Rect bounds = VisualTreeHelper.GetDescendantBounds( this.AdornedElement );

      return bounds.Size;
    }

    #endregion
  }
}
