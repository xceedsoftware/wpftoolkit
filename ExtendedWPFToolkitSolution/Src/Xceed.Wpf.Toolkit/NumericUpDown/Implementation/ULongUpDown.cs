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
  internal class ULongUpDown : CommonNumericUpDown<ulong>
  {
    #region Constructors

    static ULongUpDown()
    {
      UpdateMetadata( typeof( ULongUpDown ), ( ulong )1, ulong.MinValue, ulong.MaxValue );
    }

    public ULongUpDown()
      : base( ulong.TryParse, Decimal.ToUInt64, ( v1, v2 ) => v1 < v2, ( v1, v2 ) => v1 > v2 )
    {
    }

    #endregion //Constructors

    #region Base Class Overrides

    protected override ulong IncrementValue( ulong value, ulong increment )
    {
      return ( ulong )( value + increment );
    }

    protected override ulong DecrementValue( ulong value, ulong increment )
    {
      return ( ulong )( value - increment );
    }

    #endregion //Base Class Overrides
  }
}
