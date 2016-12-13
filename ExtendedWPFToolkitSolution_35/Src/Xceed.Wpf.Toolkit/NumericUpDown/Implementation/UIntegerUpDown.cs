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
  internal class UIntegerUpDown : CommonNumericUpDown<uint>
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
