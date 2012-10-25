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
  public class IntegerUpDown : CommonNumericUpDown<int>
  {
    #region Constructors

    static IntegerUpDown()
    {
      UpdateMetadata( typeof( IntegerUpDown ), 0, 1, int.MinValue, int.MaxValue );
    }

    public IntegerUpDown()
      : base( Int32.Parse, Decimal.ToInt32 )
    {
    }

    #endregion //Constructors

    #region Base Class Overrides

    protected override int IncrementValue( int value, int increment )
    {
      return value + increment;
    }

    protected override int DecrementValue( int value, int increment )
    {
      return value - increment;
    }

    #endregion //Base Class Overrides
  }
}
