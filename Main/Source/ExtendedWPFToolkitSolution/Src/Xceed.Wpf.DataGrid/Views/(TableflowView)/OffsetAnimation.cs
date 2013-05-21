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
  internal class OffsetAnimation : DoubleAnimationBase
  {
    #region CONSTRUCTORS

    public OffsetAnimation()
      : base()
    {
    }

    public OffsetAnimation( double toValue, Duration duration )
      : this()
    {
      this.To = toValue;
      this.Duration = duration;
    }

    public OffsetAnimation( double fromValue, double toValue, Duration duration )
      : this( toValue, duration )
    {
      this.From = fromValue;
    }

    #endregion CONSTRUCTORS

    #region From Property

    public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
      "From",
      typeof( Nullable<double> ),
      typeof( OffsetAnimation ) );

    public Nullable<double> From
    {
      get
      {
        return ( Nullable<double> )this.GetValue( OffsetAnimation.FromProperty );
      }
      set
      {
        this.SetValue( OffsetAnimation.FromProperty, value );
      }
    }

    #endregion From Property

    #region To Property

    public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
      "To",
      typeof( Nullable<double> ),
      typeof( OffsetAnimation ) );

    public Nullable<double> To
    {
      get
      {
        return ( Nullable<double> )this.GetValue( OffsetAnimation.ToProperty );
      }
      set
      {
        this.SetValue( OffsetAnimation.ToProperty, value );
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

    protected override double GetCurrentValueCore( double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock )
    {
      Nullable<double> to = this.To;
      Nullable<double> from = this.From;

      if( !from.HasValue )
      {
        from = defaultOriginValue;
      }

      if( !to.HasValue )
      {
        to = defaultDestinationValue;
      }

      double toValue = to.GetValueOrDefault();
      double fromValue = from.GetValueOrDefault();

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

      double animationStep = TableflowViewAnimationHelper.GetAnimationStep( fromValue, toValue, elapsedTime, totalTime );

      if( DoubleUtil.AreClose( animationStep, toValue ) )
        return toValue;

      return animationStep;
    }

    protected override Freezable CreateInstanceCore()
    {
      return new OffsetAnimation();
    }

    #endregion DoubleAnimationBase Overrides
  }
}
