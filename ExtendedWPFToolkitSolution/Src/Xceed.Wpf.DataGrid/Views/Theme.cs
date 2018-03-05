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
using System.ComponentModel;
using System.Windows;
using Xceed.Wpf.DataGrid.Markup;

namespace Xceed.Wpf.DataGrid.Views
{
  [TypeConverter( typeof( ThemeConverter ) )]
  public abstract class Theme : DependencyObject
  {
    public bool IsViewSupported( Type viewType )
    {
      return Theme.IsViewSupported( viewType, this.GetType() );
    }

    public static bool IsViewSupported( Type viewType, Type themeType )
    {
      object[] attributes = themeType.GetCustomAttributes( typeof( TargetViewAttribute ), true );

      foreach( TargetViewAttribute attribute in attributes )
      {
        if( attribute.ViewType == viewType )
          return true;
      }

      return false;
    }

    protected virtual ThemeKey CreateDefaultStyleKey( Type viewType, Type elementType )
    {
      return new ThemeKey( viewType, this.GetType(), elementType );
    }

    internal ThemeKey GetDefaultStyleKey( Type viewType, Type elementType )
    {
      return this.CreateDefaultStyleKey( viewType, elementType );
    }
  }

  [TargetView( typeof( TableflowView ) )]
  [TargetView( typeof( TableView ) )]
  public class ClassicSystemColorTheme : Theme
  {
  }

  [TargetView( typeof( TableflowView ) )]
  [TargetView( typeof( TableView ) )]
  public class LunaNormalColorTheme : Theme
  {
  }

  [TargetView( typeof( TableflowView ) )]
  [TargetView( typeof( TableView ) )]
  public class LunaHomesteadTheme : Theme
  {
  }

  [TargetView( typeof( TableflowView ) )]
  [TargetView( typeof( TableView ) )]
  public class LunaMetallicTheme : Theme
  {
  }

  [TargetView( typeof( TableflowView ) )]
  [TargetView( typeof( TableView ) )]
  public class AeroNormalColorTheme : Theme
  {
  }

  [TargetView( typeof( TableflowView ) )]
  [TargetView( typeof( TableView ) )]
  public class RoyaleNormalColorTheme : Theme
  {
  }

  [TargetView( typeof( TableflowView ) )]
  [TargetView( typeof( TableView ) )]
  public class ZuneNormalColorTheme : Theme
  {
  }

  [TargetView( typeof( TableflowView ) )]
  [TargetView( typeof( TableView ) )]
  public class Windows7Theme : Theme
  {
  }

  [TargetView( typeof( TableflowView ) )]
  [TargetView( typeof( TableView ) )]
  public class Windows8Theme : Theme
  {
  }
}
