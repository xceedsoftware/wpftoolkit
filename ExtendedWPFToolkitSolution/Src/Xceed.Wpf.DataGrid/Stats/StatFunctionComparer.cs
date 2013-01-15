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
using System.Collections.Generic;

namespace Xceed.Wpf.DataGrid.Stats
{
  internal class StatFunctionComparer : IEqualityComparer<StatFunction>
  {
    public bool Equals( StatFunction x, StatFunction y )
    {
      return StatFunction.AreEquivalents( x, y );
    }

    public int GetHashCode( StatFunction statFunction )
    {
      if( statFunction == null )
        throw new ArgumentNullException( "statFunction" );

      return statFunction.GetEquivalenceKey();
    }
  }
}
