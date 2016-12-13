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
  internal class SByteUpDown : CommonNumericUpDown<sbyte>
  {
    #region Constructors

    static SByteUpDown()
    {
      UpdateMetadata( typeof( SByteUpDown ), ( sbyte )1, sbyte.MinValue, sbyte.MaxValue );
    }

    public SByteUpDown()
      : base( sbyte.TryParse, Decimal.ToSByte, ( v1, v2 ) => v1 < v2, ( v1, v2 ) => v1 > v2 )
    {
    }

    #endregion //Constructors

    #region Base Class Overrides

    protected override sbyte IncrementValue( sbyte value, sbyte increment )
    {
      return ( sbyte )( value + increment );
    }

    protected override sbyte DecrementValue( sbyte value, sbyte increment )
    {
      return ( sbyte )( value - increment );
    }

    #endregion //Base Class Overrides
  }
}
