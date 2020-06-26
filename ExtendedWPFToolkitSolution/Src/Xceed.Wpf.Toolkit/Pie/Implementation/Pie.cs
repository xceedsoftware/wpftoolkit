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
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Primitives;
using Xceed.Wpf.Toolkit.Core;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit
{
  public sealed class Pie : ShapeBase
  {
    #region Constructors

    static Pie()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( Pie ), new FrameworkPropertyMetadata( typeof( Pie ) ) );
      // The default stretch mode of Pie is Fill
      Pie.StretchProperty.OverrideMetadata( typeof( Pie ), new FrameworkPropertyMetadata( Stretch.Fill ) );
      Pie.StrokeLineJoinProperty.OverrideMetadata( typeof( Pie ), new FrameworkPropertyMetadata( PenLineJoin.Round ) );
    }

    public Pie()
      : base()
    {
    }

    #endregion

    #region EndAngle Property

    public static readonly DependencyProperty EndAngleProperty =
      DependencyProperty.Register( "EndAngle", typeof( double ), typeof( Pie ),
        new FrameworkPropertyMetadata( 360d, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
          new PropertyChangedCallback( Pie.OnEndAngleChanged ), new CoerceValueCallback( Pie.CoerceEndAngleValue ) ) );

    public double EndAngle
    {
      get
      {
        return ( double )this.GetValue( Pie.EndAngleProperty );
      }
      set
      {
        this.SetValue( Pie.EndAngleProperty, value );
      }
    }

    private static void OnEndAngleChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( Pie )d ).OnEndAngleChanged( e );
    }

    private void OnEndAngleChanged( DependencyPropertyChangedEventArgs e )
    {
      // avoid re-entrancy
      if( this.IsUpdatingEndAngle )
        return;

      if( !( this.IsUpdatingStartAngle || this.IsUpdatingSlice || this.IsUpdatingSweepDirection ) )
      {
        switch( this.Mode )
        {
          case PieMode.Slice:
            throw new InvalidOperationException( ErrorMessages.GetMessage( "EndAngleCannotBeSetDirectlyInSlice" ) );
        }
      }

      // EndAngle, Slice, and SweepDirection are interrelated and must be kept in sync
      this.IsUpdatingEndAngle = true;
      try
      {
        if( this.Mode == PieMode.EndAngle )
        {
          this.CoerceValue( Pie.SweepDirectionProperty );
        }
        this.CoerceValue( Pie.SliceProperty );
      }
      finally
      {
        this.IsUpdatingEndAngle = false;
      }
    }

    private static object CoerceEndAngleValue( DependencyObject d, object value )
    {
      // keep EndAngle in sync with Slice and SweepDirection
      Pie pie = ( Pie )d;
      if( pie.IsUpdatingSlice || pie.IsUpdatingSweepDirection
          || ( pie.IsUpdatingStartAngle && pie.Mode == PieMode.Slice ) )
      {
        double newValue = pie.StartAngle + ( ( pie.SweepDirection == SweepDirection.Clockwise ) ? 1.0 : -1.0 ) * pie.Slice * 360;
        if( !DoubleHelper.AreVirtuallyEqual( ( double )value, newValue ) )
        {
          value = newValue;
        }
      }
      return value;
    }

    #endregion

    #region Mode Property

    public static readonly DependencyProperty ModeProperty =
      DependencyProperty.Register( "Mode", typeof( PieMode ), typeof( Pie ),
        new FrameworkPropertyMetadata( PieMode.Manual, new PropertyChangedCallback( Pie.OnModeChanged ) ) );

    public PieMode Mode
    {
      get
      {
        return ( PieMode )this.GetValue( Pie.ModeProperty );
      }
      set
      {
        this.SetValue( Pie.ModeProperty, value );
      }
    }

    private static void OnModeChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( Pie )d ).OnModeChanged( e );
    }

    private void OnModeChanged( DependencyPropertyChangedEventArgs e )
    {
      // disallow reentrancy
      if( this.IsUpdatingMode )
        return;

      this.IsUpdatingMode = true;
      try
      {
        if( this.Mode == PieMode.EndAngle )
        {
          this.CoerceValue( Pie.SweepDirectionProperty );
        }
      }
      finally
      {
        this.IsUpdatingMode = false;
      }
    }

    #endregion

    #region Slice Property

    public static readonly DependencyProperty SliceProperty =
      DependencyProperty.Register( "Slice", typeof( double ), typeof( Pie ),
        new FrameworkPropertyMetadata( 1d, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
          new PropertyChangedCallback( Pie.OnSliceChanged ), new CoerceValueCallback( Pie.CoerceSliceValue ) ), new ValidateValueCallback( Pie.ValidateSlice ) );

    public double Slice
    {
      get
      {
        return ( double )this.GetValue( Pie.SliceProperty );
      }
      set
      {
        this.SetValue( Pie.SliceProperty, value );
      }
    }

    private static void OnSliceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( Pie )d ).OnSliceChanged( e );
    }

    private void OnSliceChanged( DependencyPropertyChangedEventArgs e )
    {
      // avoid re-entrancy
      if( this.IsUpdatingSlice )
        return;

      if( !( this.IsUpdatingStartAngle || this.IsUpdatingEndAngle || this.IsUpdatingSweepDirection ) )
      {
        if( this.Mode == PieMode.EndAngle )
          throw new InvalidOperationException( ErrorMessages.GetMessage( "SliceCannotBeSetDirectlyInEndAngle" ) );
      }

      // EndAngle and Slice are interrelated and must be kept in sync
      this.IsUpdatingSlice = true;
      try
      {
        if( !( this.IsUpdatingStartAngle || this.IsUpdatingEndAngle || ( this.Mode == PieMode.Manual && this.IsUpdatingSweepDirection ) ) )
        {
          this.CoerceValue( Pie.EndAngleProperty );
        }
      }
      finally
      {
        this.IsUpdatingSlice = false;
      }
    }

    private static object CoerceSliceValue( DependencyObject d, object value )
    {
      // keep Slice in sync with EndAngle, StartAngle, and SweepDirection
      Pie pie = ( Pie )d;
      if( pie.IsUpdatingEndAngle || pie.IsUpdatingStartAngle || pie.IsUpdatingSweepDirection )
      {

        double slice = Math.Max( -360.0, Math.Min( 360.0, ( pie.EndAngle - pie.StartAngle ) ) ) / ( ( pie.SweepDirection == SweepDirection.Clockwise ) ? 360.0 : -360.0 );
        double newValue = DoubleHelper.AreVirtuallyEqual( slice, 0 ) ? 0 : ( slice < 0 ) ? slice + 1 : slice;
        if( !DoubleHelper.AreVirtuallyEqual( ( double )value, newValue ) )
        {
          value = newValue;
        }
      }
      return value;
    }

    private static bool ValidateSlice( object value )
    {
      double newValue = ( double )value;
      if( newValue < 0 || newValue > 1 || DoubleHelper.IsNaN( newValue ) )
        throw new ArgumentException( ErrorMessages.GetMessage( "SliceOOR" ) );

      return true;
    }

    #endregion

    #region StartAngle Property

    public static readonly DependencyProperty StartAngleProperty =
      DependencyProperty.Register( "StartAngle", typeof( double ), typeof( Pie ),
        new FrameworkPropertyMetadata( 360d, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
          new PropertyChangedCallback( Pie.OnStartAngleChanged ) ) );

    public double StartAngle
    {
      get
      {
        return ( double )this.GetValue( Pie.StartAngleProperty );
      }
      set
      {
        this.SetValue( Pie.StartAngleProperty, value );
      }
    }

    private static void OnStartAngleChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( Pie )d ).OnStartAngleChanged( e );
    }

    private void OnStartAngleChanged( DependencyPropertyChangedEventArgs e )
    {
      // avoid re-entrancy
      if( this.IsUpdatingStartAngle )
        return;

      // StartAngle, Slice, and SweepDirection are interrelated and must be kept in sync
      this.IsUpdatingStartAngle = true;
      try
      {
        switch( Mode )
        {
          case PieMode.Manual:
            this.CoerceValue( Pie.SliceProperty );
            break;

          case PieMode.EndAngle:
            this.CoerceValue( Pie.SweepDirectionProperty );
            this.CoerceValue( Pie.SliceProperty );
            break;

          case PieMode.Slice:
            this.CoerceValue( Pie.EndAngleProperty );
            break;
        }
      }
      finally
      {
        this.IsUpdatingStartAngle = false;
      }
    }

    #endregion

    #region SweepDirection Property

    public static readonly DependencyProperty SweepDirectionProperty =
      DependencyProperty.Register( "SweepDirection", typeof( SweepDirection ), typeof( Pie ),
        new FrameworkPropertyMetadata( ( SweepDirection )SweepDirection.Clockwise, FrameworkPropertyMetadataOptions.AffectsRender,
          new PropertyChangedCallback( Pie.OnSweepDirectionChanged ), new CoerceValueCallback( Pie.CoerceSweepDirectionValue ) ) );

    public SweepDirection SweepDirection
    {
      get
      {
        return ( SweepDirection )this.GetValue( Pie.SweepDirectionProperty );
      }
      set
      {
        this.SetValue( Pie.SweepDirectionProperty, value );
      }
    }

    private static void OnSweepDirectionChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( Pie )d ).OnSweepDirectionChanged( e );
    }

    private void OnSweepDirectionChanged( DependencyPropertyChangedEventArgs e )
    {
      // avoid re-entrancy
      if( this.IsUpdatingSweepDirection )
        return;

      // EndAngle, Slice, and SweepDirection are interrelated and must be kept in sync
      this.IsUpdatingSweepDirection = true;
      try
      {
        switch( Mode )
        {
          case PieMode.Slice:
            this.CoerceValue( Pie.EndAngleProperty );
            break;

          default:
            this.CoerceValue( Pie.SliceProperty );
            break;
        }
      }
      finally
      {
        this.IsUpdatingSweepDirection = false;
      }
    }

    private static object CoerceSweepDirectionValue( DependencyObject d, object value )
    {
      // keep SweepDirection in sync with EndAngle and StartAngle
      Pie pie = ( Pie )d;
      if( pie.IsUpdatingEndAngle || pie.IsUpdatingStartAngle || pie.IsUpdatingMode )
      {
        if( DoubleHelper.AreVirtuallyEqual( pie.StartAngle, pie.EndAngle ) )
        {
          // if the values are equal, use previously coerced value
          value = pie.SweepDirection;
        }
        else
        {
          value = ( pie.EndAngle < pie.StartAngle ) ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
        }
      }
      return value;
    }

    #endregion

    #region GeometryTransform Property

    public override Transform GeometryTransform
    {
      get
      {
        return Transform.Identity;
      }
    }

    #endregion

    #region RenderedGeometry Property

    public override Geometry RenderedGeometry
    {
      get
      {
        // for a Pie, the RenderedGeometry is the same as the DefiningGeometry
        return this.DefiningGeometry;
      }
    }

    #endregion

    #region DefiningGeometry Protected Property

    protected override Geometry DefiningGeometry
    {
      get
      {
        double slice = Slice;
        if( _rect.IsEmpty || slice <= 0 )
          return Geometry.Empty;

        if( slice >= 1 )
          return new EllipseGeometry( _rect );

        // direction of flow is determined by the SweepDirection property
        double directionalFactor = ( this.SweepDirection == SweepDirection.Clockwise ) ? 1.0 : -1.0;
        double startAngle = StartAngle;

        Point pointA = EllipseHelper.PointOfRadialIntersection( _rect, startAngle );
        Point pointB = EllipseHelper.PointOfRadialIntersection( _rect, startAngle + directionalFactor * slice * 360 );
        PathSegmentCollection segments = new PathSegmentCollection();
        segments.Add( new LineSegment( pointA, true ) );
        ArcSegment arc = new ArcSegment();
        arc.Point = pointB;
        arc.Size = new Size( _rect.Width / 2, _rect.Height / 2 );
        arc.IsLargeArc = slice > 0.5;
        arc.SweepDirection = SweepDirection;
        segments.Add( arc );
        PathFigureCollection figures = new PathFigureCollection();
        figures.Add( new PathFigure( RectHelper.Center( _rect ), segments, true ) );
        return new PathGeometry( figures );
      }
    }

    #endregion

    #region IsUpdatingEndAngle Private Property

    private bool IsUpdatingEndAngle
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.IsUpdatingEndAngle ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.IsUpdatingEndAngle ] = value;
      }
    }

    #endregion

    #region IsUpdatingMode Private Property

    private bool IsUpdatingMode
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.IsUpdatingMode ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.IsUpdatingMode ] = value;
      }
    }

    #endregion

    #region IsUpdatingSlice Private Property

    private bool IsUpdatingSlice
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.IsUpdatingSlice ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.IsUpdatingSlice ] = value;
      }
    }

    #endregion

    #region IsUpdatingStartAngle Private Property

    private bool IsUpdatingStartAngle
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.IsUpdatingStartAngle ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.IsUpdatingStartAngle ] = value;
      }
    }

    #endregion

    #region IsUpdatingSweepDirection Private Property

    private bool IsUpdatingSweepDirection
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.IsUpdatingSweepDirection ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.IsUpdatingSweepDirection ] = value;
      }
    }

    #endregion

    internal override Size GetNaturalSize()
    {
      double strokeThickness = this.GetStrokeThickness();
      return new Size( strokeThickness, strokeThickness );
    }

    internal override Rect GetDefiningGeometryBounds()
    {
      return _rect;
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
      double penThickness = this.GetStrokeThickness();
      double margin = penThickness / 2;

      _rect = new Rect( margin, margin,
          Math.Max( 0, finalSize.Width - penThickness ),
          Math.Max( 0, finalSize.Height - penThickness ) );

      switch( Stretch )
      {
        case Stretch.None:
          // empty rectangle
          _rect.Width = _rect.Height = 0;
          break;

        case Stretch.Fill:
          // already initialized for Fill
          break;

        case Stretch.Uniform:
          // largest square that fits in the final size
          if( _rect.Width > _rect.Height )
          {
            _rect.Width = _rect.Height;
          }
          else
          {
            _rect.Height = _rect.Width;
          }
          break;

        case Stretch.UniformToFill:

          // smallest square that fills the final size
          if( _rect.Width < _rect.Height )
          {
            _rect.Width = _rect.Height;
          }
          else
          {
            _rect.Height = _rect.Width;
          }
          break;
      }

      return finalSize;
    }

    protected override Size MeasureOverride( Size constraint )
    {
      if( this.Stretch == Stretch.UniformToFill )
      {
        double width = constraint.Width;
        double height = constraint.Height;

        if( Double.IsInfinity( width ) && Double.IsInfinity( height ) )
        {
          return this.GetNaturalSize();
        }
        else if( Double.IsInfinity( width ) || Double.IsInfinity( height ) )
        {
          width = Math.Min( width, height );
        }
        else
        {
          width = Math.Max( width, height );
        }

        return new Size( width, width );
      }

      return this.GetNaturalSize();
    }

    protected override void OnRender( DrawingContext drawingContext )
    {
      if( !_rect.IsEmpty )
      {
        Pen pen = this.GetPen();
        drawingContext.DrawGeometry( this.Fill, pen, this.RenderedGeometry );
      }
    }

    #region Private Fields

    private Rect _rect = Rect.Empty;
    private BitVector32 _cacheBits = new BitVector32( 0 );

    #endregion

    #region CacheBits Nested Type

    private enum CacheBits
    {
      IsUpdatingEndAngle = 0x00000001,
      IsUpdatingMode = 0x00000002,
      IsUpdatingSlice = 0x00000004,
      IsUpdatingStartAngle = 0x00000008,
      IsUpdatingSweepDirection = 0x00000010,
    }

    #endregion
  }
}
