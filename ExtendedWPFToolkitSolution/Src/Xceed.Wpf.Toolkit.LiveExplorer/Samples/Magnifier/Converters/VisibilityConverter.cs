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
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using System.Windows;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Magnifier.Converters
{
  class VisibilityConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( value is FrameType )
      {
        FrameType frameType = (FrameType)value;
        int param = int.Parse((string)parameter);
        switch( frameType )
        {
          case FrameType.Circle:
            {
              //For the radius
              if( param == 0 )
                return Visibility.Visible;
              //For the rectangle
              else if( param == 1 )
                return Visibility.Collapsed;
            }
            break;
          case FrameType.Rectangle:
            {
              //For the radius
              if( param == 0 )
                return Visibility.Collapsed;
              //For the rectangle
              else if( param == 1 )
                return Visibility.Visible;
            }
            break;
        }
      }

      return Visibility.Collapsed;
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
