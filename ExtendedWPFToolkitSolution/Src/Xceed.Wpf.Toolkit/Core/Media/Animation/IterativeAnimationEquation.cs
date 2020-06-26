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

namespace Xceed.Wpf.Toolkit.Media.Animation
{
  [TypeConverter( typeof( IterativeEquationConverter ) )]
  public class IterativeEquation<T>
  {
    #region Constructors

    public IterativeEquation( IterativeAnimationEquationDelegate<T> equation )
    {
      _equation = equation;
    }

    internal IterativeEquation()
    {
    }

    #endregion

    public virtual T Evaluate( TimeSpan currentTime, T from, T to, TimeSpan duration )
    {
      return _equation( currentTime, from, to, duration );
    }

    #region Private Fields

    private readonly IterativeAnimationEquationDelegate<T> _equation;

    #endregion
  }
}
