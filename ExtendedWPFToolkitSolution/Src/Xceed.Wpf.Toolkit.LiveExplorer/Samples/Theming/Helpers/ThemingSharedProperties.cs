/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2023 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections;
using System.Linq;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Theming.Helpers
{
  internal class ThemingSharedProperties
  {

    internal static string LastChoiceName { get; set; }

    internal static bool LastChoiceIsDark { get; set; }

    internal static string LastFluentAccentName { get; set; }

    internal static string LastMetroBrushName { get; set; }

    internal static void NotifyThemeChoiceSelected( ThemeChoiceViewModel themeChoice )
    {
      ThemingSharedProperties.LastChoiceName = themeChoice.BaseName;
      ThemingSharedProperties.LastChoiceIsDark = themeChoice.IsDark;
    }

    internal static ThemeChoiceViewModel GetLastThemeChoiceOrDefault(IList choices )
    {
      foreach (var choice in choices.OfType<ThemeChoiceViewModel>())
      {
        if (choice.BaseName == LastChoiceName && choice.IsDark == LastChoiceIsDark)
        {
          return choice;
        }
      }

      return choices.OfType<ThemeChoiceViewModel>().FirstOrDefault();
    }
  }
}
