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
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Converters
{
  public class ValueSourceToImagePathConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      BaseValueSource bvs = ( BaseValueSource )value;

      string uriPrefix = "/WPFToolkit.Extended;component/PropertyGrid/Images/";
      string imageName = "AdvancedProperties11";

      switch( bvs )
      {
        case BaseValueSource.Inherited:
        case BaseValueSource.DefaultStyle:
        case BaseValueSource.ImplicitStyleReference:
          imageName = "Inheritance11";
          break;
        case BaseValueSource.DefaultStyleTrigger:
          break;
        case BaseValueSource.Style:
          imageName = "Style11";
          break;

        case BaseValueSource.Local:
          imageName = "Local11";
          break;
      }


      return new BitmapImage( new Uri( String.Format( "{0}{1}.png", uriPrefix, imageName ), UriKind.RelativeOrAbsolute ) );
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
