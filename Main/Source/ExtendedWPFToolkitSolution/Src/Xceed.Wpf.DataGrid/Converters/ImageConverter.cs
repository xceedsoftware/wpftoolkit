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
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid.Converters
{
  [EditorBrowsable( EditorBrowsableState.Never )]
  [ValueConversion( typeof( object ), typeof( ImageSource ) )]
  public class ImageConverter : IValueConverter
  {
    #region IValueConverter Members

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1800:DoNotCastUnnecessarily" )]
    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( !targetType.IsAssignableFrom( typeof( ImageSource ) ) )
        return DependencyProperty.UnsetValue;

      if( ( value == null ) || ( value is ImageSource ) )
        return value;

      ImageSource imageSource = null;

      System.Drawing.Image image = value as System.Drawing.Image;

      if( image != null )
      {
        imageSource = ImageConverter.ConvertFromWinFormsImage( image );
      }
      else
      {
        byte[] byteArray = value as byte[];

        if( byteArray != null )
        {
          imageSource = ImageConverter.ConvertFromByteArray( byteArray );
        }
      }

      return imageSource;
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      return Binding.DoNothing;
    }

    #endregion

    #region PRIVATE METHODS

    private static ImageSource ConvertFromWinFormsImage( System.Drawing.Image image )
    {
      ImageSource imageSource = null;

      try
      {
        System.Drawing.ImageConverter imageConverter = new System.Drawing.ImageConverter();

        byte[] imageBytes = ( byte[] )imageConverter.ConvertTo( image, typeof( byte[] ) );

        if( imageBytes != null )
          imageSource = ImageConverter.ConvertFromByteArray( imageBytes );
      }
      catch {}

      return imageSource;
    }

    private static ImageSource ConvertFromByteArray( byte[] imageBytes )
    {
      ImageSource imageSource = null;

      try
      {
        ImageSourceConverter imageSourceConverter = new ImageSourceConverter();
        imageSource = imageSourceConverter.ConvertFrom( imageBytes ) as ImageSource;
      }
      catch
      {
      }

      return imageSource;
    }

    #endregion PRIVATE METHODS
  }
}
