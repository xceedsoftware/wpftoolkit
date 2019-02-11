/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/


namespace Xceed.Utils.Collections
{
  internal sealed class DoubleFenwickTree : FenwickTree<double>
  {
    #region Constructor

    public DoubleFenwickTree( int capacity )
      : base( capacity )
    {
    }

    #endregion

    protected override double Add( double x, double y )
    {
      return x + y;
    }

    protected override double Substract( double x, double y )
    {
      return x - y;
    }
  }
}
