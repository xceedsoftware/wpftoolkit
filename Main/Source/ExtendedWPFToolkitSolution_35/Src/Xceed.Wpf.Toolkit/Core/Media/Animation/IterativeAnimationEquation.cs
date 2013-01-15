/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

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
