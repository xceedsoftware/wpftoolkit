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
      : base( uint.Parse, Decimal.ToUInt32, ( v1, v2 ) => v1 < v2, ( v1, v2 ) => v1 > v2 )
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
