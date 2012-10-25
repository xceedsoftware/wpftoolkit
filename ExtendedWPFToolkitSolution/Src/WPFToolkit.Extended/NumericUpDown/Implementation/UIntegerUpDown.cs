/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

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
      UpdateMetadata( typeof( UIntegerUpDown ), default( uint ), ( uint )1, uint.MinValue, uint.MaxValue );
    }

    public UIntegerUpDown()
      : base( uint.Parse, Decimal.ToUInt32 )
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
