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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace Xceed.Wpf.Toolkit.Core.Converters
{
    public class CenterTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Parameters: DesiredSize, WindowWidth, HeaderColumns
            double titleTextWidth = ((Size)values[0]).Width;
            double windowWidth = (double)values[1];

            ColumnDefinitionCollection headerColumns = (ColumnDefinitionCollection)values[2];
            double titleColWidth = headerColumns[2].ActualWidth;
            double buttonsColWidth = headerColumns[3].ActualWidth;


            // Result (1) Title is Centered across all HeaderColumns
            if ((titleTextWidth + buttonsColWidth * 2) < windowWidth)
                return 1;

            // Result (2) Title is Centered in HeaderColumns[2]
            if (titleTextWidth < titleColWidth)
                return 2;

            // Result (3) Title is Left-Aligned in HeaderColumns[2]
            return 3;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
