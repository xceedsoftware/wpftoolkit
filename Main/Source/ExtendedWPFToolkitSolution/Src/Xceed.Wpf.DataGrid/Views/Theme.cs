/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
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
}
