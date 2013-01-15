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
  internal class ULongUpDown : CommonNumericUpDown<ulong>
  {
    #region Constructors

    static ULongUpDown()
    {
      UpdateMetadata( typeof( ULongUpDown ), default( ulong ), ( ulong )1, ulong.MinValue, ulong.MaxValue );
    }

    public ULongUpDown()
      : base( ulong.Parse, Decimal.ToUInt64 )
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
