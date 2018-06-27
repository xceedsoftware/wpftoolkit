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
using System.Windows.Data;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Converters
{
  [ValueConversion( typeof( AnchorSide ), typeof( double ) )]
  public class AnchorSideToAngleConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      AnchorSide side = ( AnchorSide )value;
      if( side == AnchorSide.Left ||
          side == AnchorSide.Right )
        return 90.0;

      return Binding.DoNothing;
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
