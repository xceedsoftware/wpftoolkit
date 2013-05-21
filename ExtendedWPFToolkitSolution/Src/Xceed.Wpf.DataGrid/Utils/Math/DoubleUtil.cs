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
using System.Text;

namespace Xceed.Utils.Math
{
  internal static class DoubleUtil
  {
    public static bool AreClose( double value1, double value2 )
    {
      if( value1 == value2 )
      {
        return true;
      }

      double num1 = value1 - value2;

      if( num1 < 1.53E-06 )
      {
        return ( num1 > -1.53E-06 );
      }

      return false;
    }
  }
}
