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
  internal class UShortUpDown : CommonNumericUpDown<ushort>
  {
    #region Constructors

    static UShortUpDown()
    {
      UpdateMetadata( typeof( UShortUpDown ), default( ushort ), ( ushort )1, ushort.MinValue, ushort.MaxValue );
    }

    public UShortUpDown()
      : base( ushort.Parse, Decimal.ToUInt16 )
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
