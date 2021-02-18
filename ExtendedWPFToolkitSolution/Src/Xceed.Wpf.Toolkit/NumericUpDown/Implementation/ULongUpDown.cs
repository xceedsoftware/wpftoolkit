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

namespace Xceed.Wpf.Toolkit
{
  [CLSCompliantAttribute( false )]
  public class ULongUpDown : CommonNumericUpDown<ulong>
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
