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
using System.Windows.Media;

namespace Xceed.Wpf.DataGrid.Views
{
  public static class DataGridControlBackgroundBrushes
  {
    static DataGridControlBackgroundBrushes()
    {
      DataGridControlBackgroundBrushesResources resources = new DataGridControlBackgroundBrushesResources();

      s_auroraBlue = ( Brush )resources[ "auroraBlueBrush" ];
      s_auroraPink = ( Brush )resources[ "auroraPinkBrush" ];
      s_auroraRed = ( Brush )resources[ "auroraRedBrush" ];
      s_elementalBlack = ( Brush )resources[ "elementalBlackBrush" ];
      s_elementalBlue = ( Brush )resources[ "elementalBlueBrush" ];
      s_elementalSilver = ( Brush )resources[ "elementalSilverBrush" ];
      s_horizonBlue = ( Brush )resources[ "horizonBlueBrush" ];
      s_horizonOrange = ( Brush )resources[ "horizonOrangeBrush" ];
      s_nightFog = ( Brush )resources[ "nightFogBrush" ];
      s_sunrise = ( Brush )resources[ "sunriseBrush" ];
      s_sunriseBlue = ( Brush )resources[ "sunriseBlueBrush" ];
      s_sunBlack = ( Brush )resources[ "sunBlackBrush" ];
      s_sunBlue = ( Brush )resources[ "sunBlueBrush" ];
      s_sunOrange = ( Brush )resources[ "sunOrangeBrush" ];
    }

    public static Brush AuroraBlue
    {
      get
      {
        return s_auroraBlue;
      }
    }

    public static Brush AuroraPink
    {
      get
      {
        return s_auroraPink;
      }
    }

    public static Brush AuroraRed
    {
      get
      {
        return s_auroraRed;
      }
    }

    public static Brush ElementalBlack
    {
      get
      {
        return s_elementalBlack;
      }
    }

    public static Brush ElementalBlue
    {
      get
      {
        return s_elementalBlue;
      }
    }

    public static Brush ElementalSilver
    {
      get
      {
        return s_elementalSilver;
      }
    }

    public static Brush HorizonBlue
    {
      get
      {
        return s_horizonBlue;
      }
    }

    public static Brush HorizonOrange
    {
      get
      {
        return s_horizonOrange;
      }
    }

    public static Brush NightFog
    {
      get
      {
        return s_nightFog;
      }
    }

    public static Brush Sunrise
    {
      get
      {
        return s_sunrise;
      }
    }

    public static Brush SunriseBlue
    {
      get
      {
        return s_sunriseBlue;
      }
    }

    public static Brush SunBlack
    {
      get
      {
        return s_sunBlack;
      }
    }

    public static Brush SunBlue
    {
      get
      {
        return s_sunBlue;
      }
    }

    public static Brush SunOrange
    {
      get
      {
        return s_sunOrange;
      }
    }

    private static Brush s_auroraBlue;
    private static Brush s_auroraPink;
    private static Brush s_auroraRed;
    private static Brush s_elementalBlack;
    private static Brush s_elementalBlue;
    private static Brush s_elementalSilver;
    private static Brush s_horizonBlue;
    private static Brush s_horizonOrange;
    private static Brush s_nightFog;
    private static Brush s_sunrise;
    private static Brush s_sunriseBlue;
    private static Brush s_sunBlack;
    private static Brush s_sunBlue;
    private static Brush s_sunOrange;
  }
}
