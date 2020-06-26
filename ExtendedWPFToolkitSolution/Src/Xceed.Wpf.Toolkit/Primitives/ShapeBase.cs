/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit.Primitives
{
  public abstract class ShapeBase : Shape
  {
    #region Constructors

    static ShapeBase()
    {
      ShapeBase.StrokeDashArrayProperty.OverrideMetadata( typeof( ShapeBase ), new FrameworkPropertyMetadata( new PropertyChangedCallback( ShapeBase.OnStrokeChanged ) ) );
      ShapeBase.StrokeDashCapProperty.OverrideMetadata( typeof( ShapeBase ), new FrameworkPropertyMetadata( new PropertyChangedCallback( ShapeBase.OnStrokeChanged ) ) );
      ShapeBase.StrokeDashOffsetProperty.OverrideMetadata( typeof( ShapeBase ), new FrameworkPropertyMetadata( new PropertyChangedCallback( ShapeBase.OnStrokeChanged ) ) );
      ShapeBase.StrokeEndLineCapProperty.OverrideMetadata( typeof( ShapeBase ), new FrameworkPropertyMetadata( new PropertyChangedCallback( ShapeBase.OnStrokeChanged ) ) );
      ShapeBase.StrokeLineJoinProperty.OverrideMetadata( typeof( ShapeBase ), new FrameworkPropertyMetadata( new PropertyChangedCallback( ShapeBase.OnStrokeChanged ) ) );
      ShapeBase.StrokeMiterLimitProperty.OverrideMetadata( typeof( ShapeBase ), new FrameworkPropertyMetadata( new PropertyChangedCallback( ShapeBase.OnStrokeChanged ) ) );
      ShapeBase.StrokeProperty.OverrideMetadata( typeof( ShapeBase ), new FrameworkPropertyMetadata( new PropertyChangedCallback( ShapeBase.OnStrokeChanged ) ) );
      ShapeBase.StrokeStartLineCapProperty.OverrideMetadata( typeof( ShapeBase ), new FrameworkPropertyMetadata( new PropertyChangedCallback( ShapeBase.OnStrokeChanged ) ) );
      ShapeBase.StrokeThicknessProperty.OverrideMetadata( typeof( ShapeBase ), new FrameworkPropertyMetadata( new PropertyChangedCallback( ShapeBase.OnStrokeChanged ) ) );
    }

    #endregion

    #region IsPenEmptyOrUndefined Internal Property

    internal bool IsPenEmptyOrUndefined
    {
      get
      {
        double strokeThickness = this.StrokeThickness;
        return ( this.Stroke == null ) || DoubleHelper.IsNaN( strokeThickness ) || DoubleHelper.AreVirtuallyEqual( 0, strokeThickness );
      }
    }

    #endregion

    #region DefiningGeometry Protected Property

    protected abstract override Geometry DefiningGeometry
    {
      get;
    }

    #endregion

    internal virtual Rect GetDefiningGeometryBounds()
    {
      Geometry geometry = this.DefiningGeometry;

      Debug.Assert( geometry != null );

      return geometry.Bounds;
    }

    internal virtual Size GetNaturalSize()
    {
      Geometry geometry = this.DefiningGeometry;

      Debug.Assert( geometry != null );

      Rect bounds = geometry.GetRenderBounds( GetPen() );

      return new Size( Math.Max( bounds.Right, 0 ), Math.Max( bounds.Bottom, 0 ) );
    }

    internal Pen GetPen()
    {
      if( this.IsPenEmptyOrUndefined )
        return null;

      if( _pen == null )
      {
        _pen = this.MakePen();
      }

      return _pen;
    }

    internal double GetStrokeThickness()
    {
      if( this.IsPenEmptyOrUndefined )
        return 0d;

      return Math.Abs( this.StrokeThickness );
    }

    internal bool IsSizeEmptyOrUndefined( Size size )
    {
      return ( DoubleHelper.IsNaN( size.Width ) || DoubleHelper.IsNaN( size.Height ) || size.IsEmpty );
    }

    private static void OnStrokeChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( ShapeBase )d )._pen = null;
    }

    private Pen MakePen()
    {
      Pen pen = new Pen();
      pen.Brush = this.Stroke;
      pen.DashCap = this.StrokeDashCap;
      if( this.StrokeDashArray != null || this.StrokeDashOffset != 0.0 )
      {
        pen.DashStyle = new DashStyle( this.StrokeDashArray, this.StrokeDashOffset );
      }
      pen.EndLineCap = this.StrokeEndLineCap;
      pen.LineJoin = this.StrokeLineJoin;
      pen.MiterLimit = this.StrokeMiterLimit;
      pen.StartLineCap = this.StrokeStartLineCap;
      pen.Thickness = Math.Abs( this.StrokeThickness );

      return pen;
    }

    #region Private Fields

    private Pen _pen = null;

    #endregion
  }
}
