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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit.Panels
{
  public class WrapPanel : AnimationPanel
  {
    #region Orientation Property

    public static readonly DependencyProperty OrientationProperty =
      StackPanel.OrientationProperty.AddOwner( typeof( WrapPanel ),
        new FrameworkPropertyMetadata( Orientation.Horizontal, 
          new PropertyChangedCallback( WrapPanel.OnOrientationChanged ) ) );

    public Orientation Orientation
    {
      get
      {
        return _orientation;
      }
      set
      {
        base.SetValue( WrapPanel.OrientationProperty, value );
      }
    }

    private static void OnOrientationChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      WrapPanel panel = ( WrapPanel )d;
      panel._orientation = ( Orientation )e.NewValue;
      panel.InvalidateMeasure();
    }

    private Orientation _orientation;

    #endregion

    #region ItemWidth Property

    public static readonly DependencyProperty ItemWidthProperty =
      DependencyProperty.Register( "ItemWidth", typeof( double ), typeof( WrapPanel ),
        new FrameworkPropertyMetadata( double.NaN,
          new PropertyChangedCallback( WrapPanel.OnInvalidateMeasure ) ), new ValidateValueCallback( WrapPanel.IsWidthHeightValid ) );

    [TypeConverter( typeof( LengthConverter ) )]
    public double ItemWidth
    {
      get
      {
        return ( double )base.GetValue( WrapPanel.ItemWidthProperty );
      }
      set
      {
        base.SetValue( WrapPanel.ItemWidthProperty, value );
      }
    }

    #endregion

    #region ItemHeight Property

    public static readonly DependencyProperty ItemHeightProperty =
      DependencyProperty.Register( "ItemHeight", typeof( double ), typeof( WrapPanel ),
        new FrameworkPropertyMetadata( double.NaN,
          new PropertyChangedCallback( WrapPanel.OnInvalidateMeasure ) ), new ValidateValueCallback( WrapPanel.IsWidthHeightValid ) );

    [TypeConverter( typeof( LengthConverter ) )]
    public double ItemHeight
    {
      get
      {
        return ( double )base.GetValue( WrapPanel.ItemHeightProperty );
      }
      set
      {
        base.SetValue( WrapPanel.ItemHeightProperty, value );
      }
    }

    #endregion

    #region IsChildOrderReversed Property

    public static readonly DependencyProperty IsStackReversedProperty =
      DependencyProperty.Register( "IsChildOrderReversed", typeof( bool ), typeof( WrapPanel ),
        new FrameworkPropertyMetadata( false, 
          new PropertyChangedCallback( WrapPanel.OnInvalidateMeasure ) ) );

    public bool IsChildOrderReversed
    {
      get
      {
        return ( bool )this.GetValue( WrapPanel.IsStackReversedProperty );
      }
      set
      {
        this.SetValue( WrapPanel.IsStackReversedProperty, value );
      }
    }

    #endregion

    protected override Size MeasureChildrenOverride( UIElementCollection children, Size constraint )
    {
      double desiredExtent = 0;
      double desiredStack = 0;

      bool isHorizontal = ( this.Orientation == Orientation.Horizontal );
      double constraintExtent = ( isHorizontal ? constraint.Width : constraint.Height );

      double itemWidth = ItemWidth;
      double itemHeight = ItemHeight;
      double itemExtent = ( isHorizontal ? itemWidth : itemHeight );

      bool hasExplicitItemWidth = !double.IsNaN( itemWidth );
      bool hasExplicitItemHeight = !double.IsNaN( itemHeight );
      bool useItemExtent = ( isHorizontal ? hasExplicitItemWidth : hasExplicitItemHeight );

      double lineExtent = 0;
      double lineStack = 0;

      Size childConstraint = new Size( ( hasExplicitItemWidth ? itemWidth : constraint.Width ),
          ( hasExplicitItemHeight ? itemHeight : constraint.Height ) );

      bool isReversed = this.IsChildOrderReversed;
      int from = isReversed ? children.Count - 1 : 0;
      int to = isReversed ? 0 : children.Count - 1;
      int step = isReversed ? -1 : 1;

      for( int i = from, pass = 0; pass < children.Count; i += step, pass++ )
      {
        UIElement child = children[ i ] as UIElement;

        child.Measure( childConstraint );

        double childExtent = isHorizontal
            ? ( hasExplicitItemWidth ? itemWidth : child.DesiredSize.Width )
            : ( hasExplicitItemHeight ? itemHeight : child.DesiredSize.Height );
        double childStack = isHorizontal
            ? ( hasExplicitItemHeight ? itemHeight : child.DesiredSize.Height )
            : ( hasExplicitItemWidth ? itemWidth : child.DesiredSize.Width );

        if( lineExtent + childExtent > constraintExtent )
        {
          desiredExtent = Math.Max( lineExtent, desiredExtent );
          desiredStack += lineStack;
          lineExtent = childExtent;
          lineStack = childStack;

          if( childExtent > constraintExtent )
          {
            desiredExtent = Math.Max( childExtent, desiredExtent );
            desiredStack += childStack;
            lineExtent = 0;
            lineStack = 0;
          }
        }
        else
        {
          lineExtent += childExtent;
          lineStack = Math.Max( childStack, lineStack );
        }
      }

      desiredExtent = Math.Max( lineExtent, desiredExtent );
      desiredStack += lineStack;

      return isHorizontal
        ? new Size( desiredExtent, desiredStack )
        : new Size( desiredStack, desiredExtent );
    }

    protected override Size ArrangeChildrenOverride( UIElementCollection children, Size finalSize )
    {
      bool isHorizontal = ( this.Orientation == Orientation.Horizontal );
      double finalExtent = ( isHorizontal ? finalSize.Width : finalSize.Height );

      double itemWidth = this.ItemWidth;
      double itemHeight = this.ItemHeight;
      double itemExtent = ( isHorizontal ? itemWidth : itemHeight );

      bool hasExplicitItemWidth = !double.IsNaN( itemWidth );
      bool hasExplicitItemHeight = !double.IsNaN( itemHeight );
      bool useItemExtent = ( isHorizontal ? hasExplicitItemWidth : hasExplicitItemHeight );

      double lineExtent = 0;
      double lineStack = 0;
      double lineStackSum = 0;

      int from = this.IsChildOrderReversed ? children.Count - 1 : 0;
      int to = this.IsChildOrderReversed ? 0 : children.Count - 1;
      int step = this.IsChildOrderReversed ? -1 : 1;

      Collection<UIElement> childrenInLine = new Collection<UIElement>();

      for( int i = from, pass = 0; pass < children.Count; i += step, pass++ )
      {
        UIElement child = children[ i ] as UIElement;

        double childExtent = isHorizontal
            ? ( hasExplicitItemWidth ? itemWidth : child.DesiredSize.Width )
            : ( hasExplicitItemHeight ? itemHeight : child.DesiredSize.Height );
        double childStack = isHorizontal
            ? ( hasExplicitItemHeight ? itemHeight : child.DesiredSize.Height )
            : ( hasExplicitItemWidth ? itemWidth : child.DesiredSize.Width );

        if( lineExtent + childExtent > finalExtent )
        {
          this.ArrangeLineOfChildren( childrenInLine, isHorizontal, lineStack, lineStackSum, itemExtent, useItemExtent );

          lineStackSum += lineStack;
          lineExtent = childExtent;

          if( childExtent > finalExtent )
          {
            childrenInLine.Add( child );
            this.ArrangeLineOfChildren( childrenInLine, isHorizontal, childStack, lineStackSum, itemExtent, useItemExtent );
            lineStackSum += childStack;
            lineExtent = 0;
          }
          childrenInLine.Add( child );
        }
        else
        {
          childrenInLine.Add( child );
          lineExtent += childExtent;
          lineStack = Math.Max( childStack, lineStack );
        }
      }

      if( childrenInLine.Count > 0 )
      {
        this.ArrangeLineOfChildren( childrenInLine, isHorizontal, lineStack, lineStackSum, itemExtent, useItemExtent );
      }

      return finalSize;
    }

    private void ArrangeLineOfChildren( Collection<UIElement> children, bool isHorizontal, double lineStack, double lineStackSum, double itemExtent, bool useItemExtent )
    {
      double extent = 0;
      foreach( UIElement child in children )
      {
        double childExtent = ( isHorizontal ? child.DesiredSize.Width : child.DesiredSize.Height );
        double elementExtent = ( useItemExtent ? itemExtent : childExtent );
        this.ArrangeChild( child, isHorizontal ? new Rect( extent, lineStackSum, elementExtent, lineStack )
          : new Rect( lineStackSum, extent, lineStack, elementExtent ) );
        extent += elementExtent;
      }
      children.Clear();
    }

    private static void OnInvalidateMeasure( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( AnimationPanel )d ).InvalidateMeasure();
    }

    private static bool IsWidthHeightValid( object value )
    {
      double num = ( double )value;
      return ( DoubleHelper.IsNaN( num ) || ( ( num >= 0d ) && !double.IsPositiveInfinity( num ) ) );
    }
  }
}
