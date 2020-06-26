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
using System.Windows.Data;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.Core.Converters
{
  /// <summary>
  /// This converter allow to blend two colors into one based on a specified ratio
  /// </summary>
  public class ColorBlendConverter : IValueConverter
  {
    private double _blendedColorRatio = 0;

    /// <summary>
    /// The ratio of the blended color. Must be between 0 and 1.
    /// </summary>
    public double BlendedColorRatio
    {
      get { return _blendedColorRatio; }

      set
      {
        if( value < 0d || value > 1d )
          throw new ArgumentException( "BlendedColorRatio must be greater than or equal to 0 and lower than or equal to 1 " );

        _blendedColorRatio = value;
      }
    }

    /// <summary>
    /// The color to blend with the source color
    /// </summary>
    public Color BlendedColor { get; set; }

    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( value == null || value.GetType() != typeof( Color ) )
        return null;

      Color color = ( Color )value;
      return new Color()
      {
        A = this.BlendValue( color.A, this.BlendedColor.A ),
        R = this.BlendValue( color.R, this.BlendedColor.R ),
        G = this.BlendValue( color.G, this.BlendedColor.G ),
        B = this.BlendValue( color.B, this.BlendedColor.B )
      };
    }

    private byte BlendValue( byte original, byte blend )
    {
      double blendRatio = this.BlendedColorRatio;
      double sourceRatio = 1 - blendRatio;

      double result = ( ( ( double )original ) * sourceRatio ) + ( ( ( double )blend ) * blendRatio );
      result = Math.Round( result );
      result = Math.Min( 255d, Math.Max( 0d, result ) );
      return System.Convert.ToByte( result );
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
