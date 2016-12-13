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
using System.Windows;

namespace Xceed.Wpf.Toolkit
{
  public class LongUpDown : CommonNumericUpDown<long>
  {
    #region Constructors

    static LongUpDown()
    {
      UpdateMetadata( typeof( LongUpDown ), 1L, long.MinValue, long.MaxValue );
    }

    public LongUpDown()
      : base( Int64.TryParse, Decimal.ToInt64, ( v1, v2 ) => v1 < v2, ( v1, v2 ) => v1 > v2 )
    {
    }

    #endregion //Constructors

    #region Base Class Overrides

    protected override long IncrementValue( long value, long increment )
    {
      return value + increment;
    }

    protected override long DecrementValue( long value, long increment )
    {
      return value - increment;
    }

    #endregion //Base Class Overrides
  }
}
