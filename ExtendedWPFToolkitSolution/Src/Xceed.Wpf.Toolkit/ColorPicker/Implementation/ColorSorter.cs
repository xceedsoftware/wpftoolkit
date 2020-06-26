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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit
{
  internal class ColorSorter : IComparer
  {
    public int Compare( object firstItem, object secondItem )
    {
      if( firstItem == null || secondItem == null )
        return -1;

      ColorItem colorItem1 = ( ColorItem )firstItem;
      ColorItem colorItem2 = ( ColorItem )secondItem;

      if( (colorItem1.Color == null) || !colorItem1.Color.HasValue || 
          (colorItem2.Color == null) || !colorItem2.Color.HasValue )
        return -1;

      System.Drawing.Color drawingColor1 = System.Drawing.Color.FromArgb( colorItem1.Color.Value.A, colorItem1.Color.Value.R, colorItem1.Color.Value.G, colorItem1.Color.Value.B );
      System.Drawing.Color drawingColor2 = System.Drawing.Color.FromArgb( colorItem2.Color.Value.A, colorItem2.Color.Value.R, colorItem2.Color.Value.G, colorItem2.Color.Value.B );

      // Compare Hue
      double hueColor1 = Math.Round( ( double )drawingColor1.GetHue(), 3 );
      double hueColor2 = Math.Round( ( double )drawingColor2.GetHue(), 3 );

      if( hueColor1 > hueColor2 )
        return 1;
      else if( hueColor1 < hueColor2 )
        return -1;
      else
      {
        // Hue is equal, compare Saturation
        double satColor1 = Math.Round( ( double )drawingColor1.GetSaturation(), 3 );
        double satColor2 = Math.Round( ( double )drawingColor2.GetSaturation(), 3 );

        if( satColor1 > satColor2 )
          return 1;
        else if( satColor1 < satColor2 )
          return -1;
        else
        {
          // Saturation is equal, compare Brightness
          double brightColor1 = Math.Round( ( double )drawingColor1.GetBrightness(), 3 );
          double brightColor2 = Math.Round( ( double )drawingColor2.GetBrightness(), 3 );

          if( brightColor1 > brightColor2 )
            return 1;
          else if( brightColor1 < brightColor2 )
            return -1;
        }
      }

      return 0;
    }
  }
}
