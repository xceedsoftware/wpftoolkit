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
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid.Markup
{
  public class ThemeConverter : TypeConverter
  {
    public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType )
    {
      if( sourceType == typeof( string ) )
        return true;

      return base.CanConvertFrom( context, sourceType );
    }

    public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType )
    {
      // We support conversion from string but we don't want to convert back to string.
      // Otherwise, the DataGrid designer would persist in XAML using the attribute
      // syntax which would fail for ThemePack themes.
      if( destinationType == typeof( string ) )
        return false;

      return base.CanConvertTo( context, destinationType );
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1807:AvoidUnnecessaryStringCreation", MessageId = "stringValue" ), System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1807:AvoidUnnecessaryStringCreation", MessageId = "value" )]
    public override object ConvertFrom( ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value )
    {
      string stringValue = value as string;

      if( stringValue != null )
      {
        string themeName = stringValue.ToLowerInvariant();
        Theme theme = null;

        switch( themeName )
        {
          case "classic.systemcolor":
          case "classicsystemcolortheme":
            theme = s_classicSystemColorTheme;
            break;
          case "luna.normalcolor":
          case "lunanormalcolortheme":
            theme = s_lunaNormalColorTheme;
            break;
          case "luna.homestead":
          case "lunahomesteadtheme":
            theme = s_lunaHomesteadTheme;
            break;
          case "luna.metallic":
          case "lunametallictheme":
            theme = s_lunaMetallicTheme;
            break;
          case "aero.normalcolor":
          case "aeronormalcolortheme":
            theme = s_aeroNormalColorTheme;
            break;
          case "royale.normalcolor":
          case "royalenormalcolortheme":
            theme = s_royaleNormalColorTheme;
            break;
          case "zune.normalcolor":
          case "zunenormalcolortheme":
            theme = s_zuneNormalColorTheme;
            break;
          case "windows7":
          case "windows7theme":
            theme = s_windows7Theme;
            break;
          case "aero2.normalcolor":
          case "aero2normalcolortheme":
            theme = s_aero2NormalColorTheme;
            break;
        }

        if( theme == null )
          throw new ArgumentException( "The specified theme is invalid.", "value" );

        return theme;
      }

      return base.ConvertFrom( context, culture, value );
    }

    private static readonly Theme s_classicSystemColorTheme = new ClassicSystemColorTheme();
    private static readonly Theme s_lunaNormalColorTheme = new LunaNormalColorTheme();
    private static readonly Theme s_lunaHomesteadTheme = new LunaHomesteadTheme();
    private static readonly Theme s_lunaMetallicTheme = new LunaMetallicTheme();
    private static readonly Theme s_aeroNormalColorTheme = new AeroNormalColorTheme();
    private static readonly Theme s_royaleNormalColorTheme = new RoyaleNormalColorTheme();
    private static readonly Theme s_zuneNormalColorTheme = new ZuneNormalColorTheme();
    private static readonly Theme s_windows7Theme = new Windows7Theme();
    private static readonly Theme s_aero2NormalColorTheme = new Windows8Theme();
  }
}
