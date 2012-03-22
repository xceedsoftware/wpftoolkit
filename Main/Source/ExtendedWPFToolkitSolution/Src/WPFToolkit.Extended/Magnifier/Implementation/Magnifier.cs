/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System.Windows;
using System.Windows.Controls;

namespace Xceed.Wpf.Toolkit
{
  public class Magnifier : Control
  {
    #region Properties

    #region DefaultSize

    internal Size DefaultSize
    {
      get
      {
        switch( FrameType )
        {
          case FrameType.Circle:
            return new Size( 2 * Radius, 2 * Radius );
          case FrameType.Rectangle:
            return new Size( Width, Height );
          default:
            return Size.Empty;
        }
      }
    }

    #endregion //DefaultSize

    #region FrameWidth

    public static readonly DependencyProperty FrameWidthProperty = DependencyProperty.Register( "FrameWidth", typeof( double ), typeof( Magnifier ), new UIPropertyMetadata( 50.0 ) );
    internal double FrameWidth
    {
      get
      {
        return ( double )GetValue( FrameWidthProperty );
      }
      set
      {
        SetValue( FrameWidthProperty, value );
      }
    }

    #endregion //MagnifierWidth

    #region FrameHeight

    public static readonly DependencyProperty FrameHeightProperty = DependencyProperty.Register( "FrameHeight", typeof( double ), typeof( Magnifier ), new UIPropertyMetadata( 50.0 ) );
    internal double FrameHeight
    {
      get
      {
        return ( double )GetValue( FrameHeightProperty );
      }
      set
      {
        SetValue( FrameHeightProperty, value );
      }
    }

    #endregion //FrameHeight

    #region FrameType

    public static readonly DependencyProperty FrameTypeProperty = DependencyProperty.Register( "FrameType", typeof( FrameType ), typeof( Magnifier ), new UIPropertyMetadata( FrameType.Circle, OnFrameTypeChanged ) );
    public FrameType FrameType
    {
      get
      {
        return ( FrameType )GetValue( FrameTypeProperty );
      }
      set
      {
        SetValue( FrameTypeProperty, value );
      }
    }

    private static void OnFrameTypeChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      Magnifier m = ( Magnifier )d;
      m.OnFrameTypeChanged( ( FrameType )e.OldValue, ( FrameType )e.NewValue );
    }

    protected virtual void OnFrameTypeChanged( FrameType oldValue, FrameType newValue )
    {
      if( newValue == Xceed.Wpf.Toolkit.FrameType.Circle )
      {
        Height = DefaultSize.Height;
        Width = DefaultSize.Width;
      }

      ResolveViewBox();
    }

    #endregion //FrameType

    #region Radius

    public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register( "Radius", typeof( double ), typeof( Magnifier ), new FrameworkPropertyMetadata( 50.0, new PropertyChangedCallback( OnRadiusPropertyChanged ) ) );
    public double Radius
    {
      get
      {
        return ( double )GetValue( RadiusProperty );
      }
      set
      {
        SetValue( RadiusProperty, value );
      }
    }

    private static void OnRadiusPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      Magnifier m = ( Magnifier )d;
      m.OnRadiusChanged( e );
    }

    protected virtual void OnRadiusChanged( DependencyPropertyChangedEventArgs e )
    {
      ResolveViewBox();
    }

    #endregion //Radius

    #region Target

    public static readonly DependencyProperty TargetProperty = DependencyProperty.Register( "Target", typeof( UIElement ), typeof( Magnifier ) );
    public UIElement Target
    {
      get
      {
        return ( UIElement )GetValue( TargetProperty );
      }
      set
      {
        SetValue( TargetProperty, value );
      }
    }

    #endregion //Target

    #region ViewBox

    public static readonly DependencyProperty ViewBoxProperty = DependencyProperty.Register( "ViewBox", typeof( Rect ), typeof( Magnifier ), new FrameworkPropertyMetadata( default( Rect ) ) );
    internal Rect ViewBox
    {
      get
      {
        return ( Rect )GetValue( ViewBoxProperty );
      }
      set
      {
        SetValue( ViewBoxProperty, value );
      }
    }

    #endregion //ViewBox

    #region ZoomFactor

    public static readonly DependencyProperty ZoomFactorProperty = DependencyProperty.Register( "ZoomFactor", typeof( double ), typeof( Magnifier ), new FrameworkPropertyMetadata( 0.5, OnZoomFactorPropertyChanged, OnCoerceZoomFactorProperty ) );
    public double ZoomFactor
    {
      get
      {
        return ( double )GetValue( ZoomFactorProperty );
      }
      set
      {
        SetValue( ZoomFactorProperty, value );
      }
    }

    private static void OnZoomFactorPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      Magnifier m = ( Magnifier )d;
      m.OnZoomFactorChanged( e );
    }

    protected virtual void OnZoomFactorChanged( DependencyPropertyChangedEventArgs e )
    {
      ResolveViewBox();
    }

    private static object OnCoerceZoomFactorProperty( DependencyObject d, object value )
    {
      Magnifier m = ( Magnifier )d;
      return m.OnCoerceZoomFactor( value );
    }

    protected virtual object OnCoerceZoomFactor( object value )
    {
      double zoomFactor = ( double )value;

      if( zoomFactor > 1 )
        zoomFactor = 1;
      else if( zoomFactor < 0 )
        zoomFactor = 0;

      return zoomFactor;
    }

    #endregion //ZoomFactor

    #endregion //Properties

    #region Constructors

    /// <summary>
    /// Initializes static members of the <see cref="Magnifier"/> class.
    /// </summary>
    static Magnifier()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( Magnifier ), new FrameworkPropertyMetadata( typeof( Magnifier ) ) );
      HeightProperty.OverrideMetadata( typeof( Magnifier ), new FrameworkPropertyMetadata( 100.0 ) );
      WidthProperty.OverrideMetadata( typeof( Magnifier ), new FrameworkPropertyMetadata( 100.0 ) );
    }

    public Magnifier()
    {
      ResolveViewBox();
    }

    #endregion

    #region Base Class Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( FrameType == Xceed.Wpf.Toolkit.FrameType.Circle )
      {
        Height = DefaultSize.Height;
        Width = DefaultSize.Width;
      }

      ResolveViewBox();
    }

    #endregion // Base Class Overrides

    #region Methods

    private void ResolveViewBox()
    {
      if( !IsInitialized )
        return;

      double correction = ( BorderThickness.Bottom + BorderThickness.Left + BorderThickness.Right + BorderThickness.Top == 0 ) ? 1 : 0;

      double width = DefaultSize.Width * ZoomFactor;
      double height = DefaultSize.Height * ZoomFactor;

      FrameWidth = DefaultSize.Width - correction;
      FrameHeight = DefaultSize.Height - correction;

      ViewBox = new Rect( ViewBox.Location, new Size( width, height ) );
    }

    #endregion //Methods
  }
}
