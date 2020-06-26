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
  public class UShortUpDown : CommonNumericUpDown<ushort>
  {
    #region Constructors

    static UShortUpDown()
    {
      UpdateMetadata( typeof( UShortUpDown ), ( ushort )1, ushort.MinValue, ushort.MaxValue );
    }

    public UShortUpDown()
      : base( ushort.TryParse, Decimal.ToUInt16, ( v1, v2 ) => v1 < v2, ( v1, v2 ) => v1 > v2 )
    {
    }

    #endregion //Constructors

    #region Base Class Overrides

    protected override ushort IncrementValue( ushort value, ushort increment )
    {
      return ( ushort )( value + increment );
    }

    protected override ushort DecrementValue( ushort value, ushort increment )
    {
      return ( ushort )( value - increment );
    }

    #endregion //Base Class Overrides
  }
}
