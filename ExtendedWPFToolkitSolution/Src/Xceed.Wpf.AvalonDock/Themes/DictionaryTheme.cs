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
using System.Windows;

namespace Xceed.Wpf.AvalonDock.Themes
{
  public abstract class DictionaryTheme : Theme
  {
    #region Constructors

    public DictionaryTheme()
    {
    }

    public DictionaryTheme( ResourceDictionary themeResourceDictionary )
    {
      this.ThemeResourceDictionary = themeResourceDictionary;
    }

    #endregion

    #region Properties

    public ResourceDictionary ThemeResourceDictionary
    {
      get;
      private set;
    }

    #endregion

    #region Overrides

    public override Uri GetResourceUri()
    {
      return null;
    }

    #endregion
  }
}
