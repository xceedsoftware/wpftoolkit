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
using System.Windows;

namespace Xceed.Wpf.Toolkit
{
  [CLSCompliantAttribute(false)]
  public class UIntegerUpDown : CommonNumericUpDown<uint>
  {
    #region Constructors

    static UIntegerUpDown()
    {
      UpdateMetadata( typeof( UIntegerUpDown ), ( uint )1, uint.MinValue, uint.MaxValue );
    }

    public UIntegerUpDown()
      : base( uint.TryParse, Decimal.ToUInt32, ( v1, v2 ) => v1 < v2, ( v1, v2 ) => v1 > v2 )
    {
    }

    #endregion //Constructors

    #region Base Class Overrides

    protected override uint IncrementValue( uint value, uint increment )
    {
      return ( uint )( value + increment );
    }

    protected override uint DecrementValue( uint value, uint increment )
    {
      return ( uint )( value - increment );
    }

    #endregion //Base Class Overrides
  }
}
