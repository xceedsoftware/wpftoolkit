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

namespace Xceed.Wpf.Toolkit.Media.Animation
{
  public class PennerEquation : IterativeEquation<double>
  {
    #region Constructors

    internal PennerEquation( PennerEquationDelegate pennerImpl )
    {
      _pennerImpl = pennerImpl;
    }

    #endregion

    public override double Evaluate( TimeSpan currentTime, double from, double to, TimeSpan duration )
    {
      double t = currentTime.TotalSeconds;
      double b = from;
      double c = to - from;
      double d = duration.TotalSeconds;

      return _pennerImpl( t, b, c, d );
    }

    #region Private Fields

    private readonly PennerEquationDelegate _pennerImpl;

    #endregion

    #region PennerEquationDelegate Delegate

    internal delegate double PennerEquationDelegate( double t, double b, double c, double d );

    #endregion
  }
}
