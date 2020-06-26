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
using System.ComponentModel;
using System.Windows;
using Xceed.Wpf.Toolkit.Media.Animation;
using Xceed.Wpf.Toolkit.Core;

namespace Xceed.Wpf.Toolkit.Panels
{
  [TypeConverter( typeof( AnimatorConverter ) )]
  public abstract class IterativeAnimator
  {
    #region Default Static Property

    public static IterativeAnimator Default
    {
      get
      {
        return _default;
      }
    }

    private static readonly IterativeAnimator _default = new DefaultAnimator();

    #endregion

    public abstract Rect GetInitialChildPlacement(
      UIElement child,
      Rect currentPlacement,
      Rect targetPlacement,
      AnimationPanel activeLayout,
      ref AnimationRate animationRate,
      out object placementArgs,
      out bool isDone );

    public abstract Rect GetNextChildPlacement(
      UIElement child,
      TimeSpan currentTime,
      Rect currentPlacement,
      Rect targetPlacement,
      AnimationPanel activeLayout,
      AnimationRate animationRate,
      ref object placementArgs,
      out bool isDone );

    #region DefaultAnimator Nested Type

    private sealed class DefaultAnimator : IterativeAnimator
    {
      public override Rect GetInitialChildPlacement( UIElement child, Rect currentPlacement, Rect targetPlacement, AnimationPanel activeLayout, ref AnimationRate animationRate, out object placementArgs, out bool isDone )
      {
        throw new InvalidOperationException( ErrorMessages.GetMessage( ErrorMessages.DefaultAnimatorCantAnimate ) );
      }

      public override Rect GetNextChildPlacement( UIElement child, TimeSpan currentTime, Rect currentPlacement, Rect targetPlacement, AnimationPanel activeLayout, AnimationRate animationRate, ref object placementArgs, out bool isDone )
      {
        throw new InvalidOperationException( ErrorMessages.GetMessage( ErrorMessages.DefaultAnimatorCantAnimate ) );
      }
    }

    #endregion
  }
}
