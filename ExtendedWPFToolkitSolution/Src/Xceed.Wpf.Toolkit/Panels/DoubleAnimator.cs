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
using Xceed.Wpf.Toolkit.Media.Animation;

namespace Xceed.Wpf.Toolkit.Panels
{
  public class DoubleAnimator : IterativeAnimator
  {
    #region Constructors

    public DoubleAnimator( IterativeEquation<double> equation )
    {
      _equation = equation;
    }

    #endregion

    public override Rect GetInitialChildPlacement( UIElement child, Rect currentPlacement,
        Rect targetPlacement, AnimationPanel activeLayout, ref AnimationRate animationRate,
        out object placementArgs, out bool isDone )
    {
      isDone = ( animationRate.HasSpeed && animationRate.Speed <= 0 ) || ( animationRate.HasDuration && animationRate.Duration.Ticks == 0 );
      if( !isDone )
      {
        Vector startVector = new Vector( currentPlacement.Left + ( currentPlacement.Width / 2 ), currentPlacement.Top + ( currentPlacement.Height / 2 ) );
        Vector finalVector = new Vector( targetPlacement.Left + ( targetPlacement.Width / 2 ), targetPlacement.Top + ( targetPlacement.Height / 2 ) );
        Vector distanceVector = startVector - finalVector;
        animationRate = new AnimationRate( animationRate.HasDuration ? animationRate.Duration
            : TimeSpan.FromMilliseconds( distanceVector.Length / animationRate.Speed ) );
      }
      placementArgs = currentPlacement;
      return currentPlacement;
    }

    public override Rect GetNextChildPlacement( UIElement child, TimeSpan currentTime,
        Rect currentPlacement, Rect targetPlacement, AnimationPanel activeLayout,
        AnimationRate animationRate, ref object placementArgs, out bool isDone )
    {
      Rect result = targetPlacement;
      isDone = true;
      if( _equation != null )
      {
        Rect from = ( Rect )placementArgs;
        TimeSpan duration = animationRate.Duration;
        isDone = currentTime >= duration;
        if( !isDone )
        {
          double x = _equation.Evaluate( currentTime, from.Left, targetPlacement.Left, duration );
          double y = _equation.Evaluate( currentTime, from.Top, targetPlacement.Top, duration );
          double width = Math.Max( 0, _equation.Evaluate( currentTime, from.Width, targetPlacement.Width, duration ) );
          double height = Math.Max( 0, _equation.Evaluate( currentTime, from.Height, targetPlacement.Height, duration ) );
          result = new Rect( x, y, width, height );
        }
      }
      return result;
    }

    #region Private Fields

    private readonly IterativeEquation<double> _equation; //null

    #endregion
  }
}
