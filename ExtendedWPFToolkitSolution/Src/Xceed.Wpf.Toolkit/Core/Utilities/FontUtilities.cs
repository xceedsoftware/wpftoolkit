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
using System.Windows.Media;
using System.Windows;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  internal class FontUtilities
  {
    internal static IEnumerable<FontFamily> Families
    {
      get
      {
#if !VS2008
        // Workaround for a WPF 4 bug.
        foreach( FontFamily font in Fonts.SystemFontFamilies )
        {
          try
          {
            // In WPF 4, this will throw an exception.
            var throwAcess = font.FamilyNames;
          }
          catch 
          { 
            // It throws. Go to the next font family.
            continue;
          }

          // If it does not throw, return the font.
          yield return font;
        }
#else
      return Fonts.SystemFontFamilies;
#endif
      }
    }

    internal static IEnumerable<FontWeight> Weights
    {
      get
      {
        yield return FontWeights.Black;
        yield return FontWeights.Bold;
        yield return FontWeights.ExtraBlack;
        yield return FontWeights.ExtraBold;
        yield return FontWeights.ExtraLight;
        yield return FontWeights.Light;
        yield return FontWeights.Medium;
        yield return FontWeights.Normal;
        yield return FontWeights.SemiBold;
        yield return FontWeights.Thin;
      }
    }

    internal static IEnumerable<FontStyle> Styles
    {
      get
      {
        yield return FontStyles.Italic;
        yield return FontStyles.Normal;
      }
    }

    internal static IEnumerable<FontStretch> Stretches
    {
      get
      {
        yield return FontStretches.Condensed;
        yield return FontStretches.Expanded;
        yield return FontStretches.ExtraCondensed;
        yield return FontStretches.ExtraExpanded;
        yield return FontStretches.Normal;
        yield return FontStretches.SemiCondensed;
        yield return FontStretches.SemiExpanded;
        yield return FontStretches.UltraCondensed;
        yield return FontStretches.UltraExpanded;
      }
    }
  }
}
