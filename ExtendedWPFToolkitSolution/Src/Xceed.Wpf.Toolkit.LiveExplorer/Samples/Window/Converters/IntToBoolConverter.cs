/**************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Window.Converters
{
  class IntToBoolConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      int intReceived = (int)value;

      if( intReceived == 1 )
        return false;
      else
        return true;
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
