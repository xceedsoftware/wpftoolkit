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
using System.Windows;

namespace Xceed.Wpf.DataGrid.Views
{
  public abstract class DictionaryTheme : Theme
  {
    public DictionaryTheme()
    {
    }

    public DictionaryTheme( ResourceDictionary themeResourceDictionary )
    {
      m_themeResourceDictionary = themeResourceDictionary;
      m_themeResourceDictionaryInitialized = true;
    }

    #region ThemeResourceDictionary Property

    public ResourceDictionary ThemeResourceDictionary
    {
      get
      {
        return m_themeResourceDictionary;
      }
      set
      {
        if( m_themeResourceDictionaryInitialized )
          throw new InvalidOperationException( "An attempt was made to set the ThemeResourceDictionary property when it has already been initialized." );

        m_themeResourceDictionary = value;
      }
    }

    private ResourceDictionary m_themeResourceDictionary;
    private bool m_themeResourceDictionaryInitialized; //false

    #endregion
  }
}
