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
  public class ByteUpDown : CommonNumericUpDown<byte>
  {
    #region Constructors

    static ByteUpDown()
    {
      UpdateMetadata( typeof( ByteUpDown ), ( byte )1, byte.MinValue, byte.MaxValue );
      MaxLengthProperty.OverrideMetadata( typeof(ByteUpDown), new FrameworkPropertyMetadata( 3 ) );
    }

    public ByteUpDown()
      : base( Byte.TryParse, Decimal.ToByte, ( v1, v2 ) => v1 < v2, ( v1, v2 ) => v1 > v2 )
    {
    }

    #endregion //Constructors

    #region Base Class Overrides

    protected override byte IncrementValue( byte value, byte increment )
    {
      return ( byte )( value + increment );
    }

    protected override byte DecrementValue( byte value, byte increment )
    {
      return ( byte )( value - increment );
    }

    #endregion //Base Class Overrides
  }
}
