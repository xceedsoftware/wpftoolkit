/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class FontComboBoxEditor : ComboBoxEditor
  {
    protected override IList<object> CreateItemsSource( PropertyItem propertyItem )
    {
      if( propertyItem.PropertyType == typeof( FontFamily ) )
        return GetFontFamilies();
      else if( propertyItem.PropertyType == typeof( FontWeight ) )
        return GetFontWeights();
      else if( propertyItem.PropertyType == typeof( FontStyle ) )
        return GetFontStyles();
      else if( propertyItem.PropertyType == typeof( FontStretch ) )
        return GetFontStretches();

      return null;
    }

    private static IList<object> GetFontFamilies()
    {
#if !VS2008
      return Fonts.SystemFontFamilies.ToList<object>();
#else
      return Fonts.SystemFontFamilies.Cast<object>().ToList();
#endif
    }

    private static IList<object> GetFontWeights()
    {
      return new List<object>()
            {
                FontWeights.Black, 
                FontWeights.Bold, 
                FontWeights.ExtraBlack, 
                FontWeights.ExtraBold,
                FontWeights.ExtraLight, 
                FontWeights.Light, 
                FontWeights.Medium, 
                FontWeights.Normal, 
                FontWeights.SemiBold,
                FontWeights.Thin
            };
    }

    private static IList<object> GetFontStyles()
    {
      return new List<object>()
            {
                FontStyles.Italic,
                FontStyles.Normal
            };
    }

    private static IList<object> GetFontStretches()
    {
      return new List<object>()
            {
                FontStretches.Condensed,
                FontStretches.Expanded,
                FontStretches.ExtraCondensed,
                FontStretches.ExtraExpanded,
                FontStretches.Normal,
                FontStretches.SemiCondensed,
                FontStretches.SemiExpanded,
                FontStretches.UltraCondensed,
                FontStretches.UltraExpanded
            };
    }
  }
}
