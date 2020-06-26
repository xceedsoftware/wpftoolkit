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
using System.Windows;
using System.Windows.Controls;

namespace Xceed.Wpf.Toolkit.Panels
{
  public class RandomPanel : AnimationPanel
  {
    #region MinimumWidth Property

    public static readonly DependencyProperty MinimumWidthProperty =
      DependencyProperty.Register( "MinimumWidth", typeof( double ), typeof( RandomPanel ),
        new FrameworkPropertyMetadata(
          10d,
          new PropertyChangedCallback( RandomPanel.OnMinimumWidthChanged ),
          new CoerceValueCallback( RandomPanel.CoerceMinimumWidth ) ) );

    public double MinimumWidth
    {
      get
      {
        return ( double )this.GetValue( RandomPanel.MinimumWidthProperty );
      }
      set
      {
        this.SetValue( RandomPanel.MinimumWidthProperty, value );
      }
    }

    private static void OnMinimumWidthChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      RandomPanel panel = ( RandomPanel )d;

      panel.CoerceValue( RandomPanel.MaximumWidthProperty );
      panel.InvalidateMeasure();
    }

    private static object CoerceMinimumWidth( DependencyObject d, object baseValue )
    {
      RandomPanel panel = ( RandomPanel )d;
      double value = ( double )baseValue;

      if( double.IsNaN( value ) || double.IsInfinity( value ) || ( value < 0d ) )
        return DependencyProperty.UnsetValue;

      double maximum = panel.MaximumWidth;
      if( value > maximum )
        return maximum;

      return baseValue;
    }

    #endregion

    #region MinimumHeight Property

    public static readonly DependencyProperty MinimumHeightProperty =
      DependencyProperty.Register( "MinimumHeight", typeof( double ), typeof( RandomPanel ),
        new FrameworkPropertyMetadata(
          10d,
          new PropertyChangedCallback( RandomPanel.OnMinimumHeightChanged ),
          new CoerceValueCallback( RandomPanel.CoerceMinimumHeight ) ) );

    public double MinimumHeight
    {
      get
      {
        return ( double )this.GetValue( RandomPanel.MinimumHeightProperty );
      }
      set
      {
        this.SetValue( RandomPanel.MinimumHeightProperty, value );
      }
    }

    private static void OnMinimumHeightChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      RandomPanel panel = ( RandomPanel )d;

      panel.CoerceValue( RandomPanel.MaximumHeightProperty );
      panel.InvalidateMeasure();
    }

    private static object CoerceMinimumHeight( DependencyObject d, object baseValue )
    {
      RandomPanel panel = ( RandomPanel )d;
      double value = ( double )baseValue;

      if( double.IsNaN( value ) || double.IsInfinity( value ) || ( value < 0d ) )
        return DependencyProperty.UnsetValue;

      double maximum = panel.MaximumHeight;
      if( value > maximum )
        return maximum;

      return baseValue;
    }

    #endregion

    #region MaximumWidth Property

    public static readonly DependencyProperty MaximumWidthProperty =
      DependencyProperty.Register( "MaximumWidth", typeof( double ), typeof( RandomPanel ),
        new FrameworkPropertyMetadata(
          100d,
          new PropertyChangedCallback( RandomPanel.OnMaximumWidthChanged ),
          new CoerceValueCallback( RandomPanel.CoerceMaximumWidth ) ) );

    public double MaximumWidth
    {
      get
      {
        return ( double )this.GetValue( RandomPanel.MaximumWidthProperty );
      }
      set
      {
        this.SetValue( RandomPanel.MaximumWidthProperty, value );
      }
    }

    private static void OnMaximumWidthChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      RandomPanel panel = ( RandomPanel )d;

      panel.CoerceValue( RandomPanel.MinimumWidthProperty );
      panel.InvalidateMeasure();
    }

    private static object CoerceMaximumWidth( DependencyObject d, object baseValue )
    {
      RandomPanel panel = ( RandomPanel )d;
      double value = ( double )baseValue;

      if( double.IsNaN( value ) || double.IsInfinity( value ) || ( value < 0d ) )
        return DependencyProperty.UnsetValue;

      double minimum = panel.MinimumWidth;
      if( value < minimum )
        return minimum;

      return baseValue;
    }

    #endregion

    #region MaximumHeight Property

    public static readonly DependencyProperty MaximumHeightProperty =
      DependencyProperty.Register( "MaximumHeight", typeof( double ), typeof( RandomPanel ),
        new FrameworkPropertyMetadata(
          100d,
          new PropertyChangedCallback( RandomPanel.OnMaximumHeightChanged ),
          new CoerceValueCallback( RandomPanel.CoerceMaximumHeight ) ) );

    public double MaximumHeight
    {
      get
      {
        return ( double )this.GetValue( RandomPanel.MaximumHeightProperty );
      }
      set
      {
        this.SetValue( RandomPanel.MaximumHeightProperty, value );
      }
    }

    private static void OnMaximumHeightChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      RandomPanel panel = ( RandomPanel )d;

      panel.CoerceValue( RandomPanel.MinimumHeightProperty );
      panel.InvalidateMeasure();
    }

    private static object CoerceMaximumHeight( DependencyObject d, object baseValue )
    {
      RandomPanel panel = ( RandomPanel )d;
      double value = ( double )baseValue;

      if( double.IsNaN( value ) || double.IsInfinity( value ) || ( value < 0d ) )
        return DependencyProperty.UnsetValue;

      double minimum = panel.MinimumHeight;
      if( value < minimum )
        return minimum;

      return baseValue;
    }

    #endregion

    #region Seed Property

    public static readonly DependencyProperty SeedProperty =
      DependencyProperty.Register( "Seed", typeof( int ), typeof( RandomPanel ),
        new FrameworkPropertyMetadata( 0,
          new PropertyChangedCallback( RandomPanel.SeedChanged ) ) );

    public int Seed
    {
      get
      {
        return ( int )this.GetValue( RandomPanel.SeedProperty );
      }
      set
      {
        this.SetValue( RandomPanel.SeedProperty, value );
      }
    }

    private static void SeedChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
    {
      if( obj is RandomPanel )
      {
        RandomPanel owner = ( RandomPanel )obj;
        owner._random = new Random( ( int )args.NewValue );
        owner.InvalidateArrange();
      }
    }

    #endregion

    #region ActualSize Private Property

    // Using a DependencyProperty as the backing store for ActualSize.  This enables animation, styling, binding, etc...
    private static readonly DependencyProperty ActualSizeProperty =
      DependencyProperty.RegisterAttached( "ActualSize", typeof( Size ), typeof( RandomPanel ),
        new UIPropertyMetadata( new Size() ) );

    private static Size GetActualSize( DependencyObject obj )
    {
      return ( Size )obj.GetValue( RandomPanel.ActualSizeProperty );
    }

    private static void SetActualSize( DependencyObject obj, Size value )
    {
      obj.SetValue( RandomPanel.ActualSizeProperty, value );
    }

    #endregion

    protected override Size MeasureChildrenOverride( UIElementCollection children, Size constraint )
    {
      Size availableSize = new Size( double.PositiveInfinity, double.PositiveInfinity );

      foreach( UIElement child in children )
      {
        if( child == null )
          continue;

        Size childSize = new Size( 1d * _random.Next( Convert.ToInt32( MinimumWidth ), Convert.ToInt32( MaximumWidth ) ),
                                   1d * _random.Next( Convert.ToInt32( MinimumHeight ), Convert.ToInt32( MaximumHeight ) ) );

        child.Measure( childSize );
        RandomPanel.SetActualSize( child, childSize );
      }
      return new Size();
    }

    protected override Size ArrangeChildrenOverride( UIElementCollection children, Size finalSize )
    {
      foreach( UIElement child in children )
      {
        if( child == null )
          continue;

        Size childSize = RandomPanel.GetActualSize( child );

        double x = _random.Next( 0, ( int )( Math.Max( finalSize.Width - childSize.Width, 0 ) ) );
        double y = _random.Next( 0, ( int )( Math.Max( finalSize.Height - childSize.Height, 0 ) ) );

        double width = Math.Min( finalSize.Width, childSize.Width );
        double height = Math.Min( finalSize.Height, childSize.Height );

        this.ArrangeChild( child, new Rect( new Point( x, y ), new Size( width, height ) ) );
      }
      return finalSize;
    }

    #region Private Fields

    private Random _random = new Random();

    #endregion
  }
}
