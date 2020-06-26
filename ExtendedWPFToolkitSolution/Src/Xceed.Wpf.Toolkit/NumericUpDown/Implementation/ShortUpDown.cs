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
  public class ShortUpDown : CommonNumericUpDown<short>
  {
    #region Constructors

    static ShortUpDown()
    {
      UpdateMetadata( typeof( ShortUpDown ), ( short )1, short.MinValue, short.MaxValue );
    }

    public ShortUpDown()
      : base( Int16.TryParse, Decimal.ToInt16, ( v1, v2 ) => v1 < v2, ( v1, v2 ) => v1 > v2 )
    {
    }

    #endregion //Constructors

    #region Base Class Overrides

    protected override short IncrementValue( short value, short increment )
    {
      return ( short )( value + increment );
    }

    protected override short DecrementValue( short value, short increment )
    {
      return ( short )( value - increment );
    }

    #endregion //Base Class Overrides
  }
}
