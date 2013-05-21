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
using System.Windows.Media.Animation;
using System.Windows;
using Xceed.Utils.Math;

namespace Xceed.Wpf.DataGrid.Views
{
  internal class PointAnimation : PointAnimationBase
  {
    #region CONSTRUCTORS

    public PointAnimation()
      : base()
    {
    }

    public PointAnimation( Point toValue, Duration duration )
      : this()
    {
      this.To = toValue;
      this.Duration = duration;
    }

    public PointAnimation( Point fromValue, Point toValue, Duration duration )
      : this( toValue, duration )
    {
      this.From = fromValue;
    }

    #endregion CONSTRUCTORS

    #region From Property

    public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
      "From",
      typeof( Nullable<Point> ),
      typeof( PointAnimation ) );

    public Nullable<Point> From
    {
      get
      {
        return ( Nullable<Point> )this.GetValue( PointAnimation.FromProperty );
      }
      set
      {
        this.SetValue( PointAnimation.FromProperty, value );
      }
    }

    #endregion From Property

    #region To Property

    public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
      "To",
      typeof( Nullable<Point> ),
      typeof( PointAnimation ) );

    public Nullable<Point> To
    {
      get
      {
        return ( Nullable<Point> )this.GetValue( PointAnimation.ToProperty );
      }
      set
      {
        this.SetValue( PointAnimation.ToProperty, value );
      }
    }

    #endregion To Property

    #region IsDestinationDefault Property

    public override bool IsDestinationDefault
    {
      get
      {
        return false;
      }
    }

    #endregion IsDestinationDefault Property

    #region DoubleAnimationBase Overrides

    protected override Point GetCurrentValueCore( Point defaultOriginValue, Point defaultDestinationValue, AnimationClock animationClock )
    {
      Nullable<Point> to = this.To;
      Nullable<Point> from = this.From;

      if( !from.HasValue )
      {
        from = defaultOriginValue;
      }

      if( !to.HasValue )
      {
        to = defaultDestinationValue;
      }

      Point toValue = to.GetValueOrDefault();
      Point fromValue = from.GetValueOrDefault();

      if( ( !animationClock.CurrentProgress.HasValue )
        || ( animationClock.CurrentProgress.Value == 1 ) )
      {
        return toValue;
      }

      if( animationClock.CurrentProgress.Value == 0 )
        return fromValue;

      double totalTime = animationClock.Timeline.Duration.TimeSpan.TotalMilliseconds;
      double elapsedTime = totalTime * animationClock.CurrentProgress.Value;

      if( elapsedTime >= totalTime )
        return toValue;

      Point animationStep = this.GetAnimationStep( fromValue, toValue, elapsedTime, totalTime );

      if( DoubleUtil.AreClose( animationStep.X, toValue.X ) && DoubleUtil.AreClose( animationStep.Y, toValue.Y ) )
        return toValue;

      return animationStep;
    }

    protected override Freezable CreateInstanceCore()
    {
      return new PointAnimation();
    }

    #endregion DoubleAnimationBase Overrides

    #region PRIVATE METHODS

    private Point GetAnimationStep( Point startPos, Point finalPos, double elapsedTime, double totalTime )
    {
      double x = TableflowViewAnimationHelper.GetAnimationStep( startPos.X, finalPos.X, elapsedTime, totalTime );
      double y = TableflowViewAnimationHelper.GetAnimationStep( startPos.Y, finalPos.Y, elapsedTime, totalTime );

      return new Point( x, y );
    }

    #endregion PRIVATE METHODS
  }
}
