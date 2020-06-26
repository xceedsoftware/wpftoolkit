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

namespace Xceed.Wpf.Toolkit.Core.Media
{
  /// <summary>
  /// Contains system colors and configurations that can be used by the control themes.
  /// 
  /// Mainly extracted from the registry because theses values are not exposed by the standard .NET API.
  /// </summary>
  public static class WindowColors
  {
    private static Color? _colorizationMode;
    private static bool? _colorizationOpaqueBlend;

    /// <summary>
    /// Relative to the \HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM\ColorizationColor Registry key.
    /// 
    /// Gets the window chrome color.
    /// </summary>
    public static Color ColorizationColor
    {
      get
      {
        if( _colorizationMode.HasValue )
          return _colorizationMode.Value;

        try
        {
          _colorizationMode = WindowColors.GetDWMColorValue( "ColorizationColor" );
        }
        catch
        {
          // If for any reason (for example, a SecurityException for XBAP apps)
          // we cannot read the value in the registry, fall back on some color.
          _colorizationMode = Color.FromArgb(255, 175, 175, 175);
        }

        return _colorizationMode.Value;
      }
    }

    /// <summary>
    /// Relative to the \HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM\ColorizationOpaqueBlend Registry key:
    /// 
    /// Gets whether transparency is disabled.
    /// 
    /// Returns true if transparency is disabled; false otherwise.
    /// </summary>
    public static bool ColorizationOpaqueBlend
    {
      get
      {
        if( _colorizationOpaqueBlend.HasValue )
          return _colorizationOpaqueBlend.Value;

        try
        {
          _colorizationOpaqueBlend = WindowColors.GetDWMBoolValue( "ColorizationOpaqueBlend" );
        }
        catch
        {
          // If for any reason (for example, a SecurityException for XBAP apps)
          // we cannot read the value in the registry, fall back on some color.
          _colorizationOpaqueBlend = false;
        }

        return _colorizationOpaqueBlend.Value;
      }
    }

    private static int GetDWMIntValue( string keyName )
    {
      // This value is not accessible throught the standard WPF API.
      // We must dig into the registry to get the value.
      var curUser = Microsoft.Win32.Registry.CurrentUser;
      var subKey = curUser.CreateSubKey(
        @"Software\Microsoft\Windows\DWM",
        Microsoft.Win32.RegistryKeyPermissionCheck.ReadSubTree
#if VS2008
        );
#else
        ,Microsoft.Win32.RegistryOptions.None );
#endif
      return ( int )subKey.GetValue( keyName );
    }

    private static Color GetDWMColorValue( string keyName )
    {
      int value = WindowColors.GetDWMIntValue( keyName );
        byte[] bytes = BitConverter.GetBytes( value );
        return new Color()
        {
          B = bytes[ 0 ],
          G = bytes[ 1 ],
          R = bytes[ 2 ],
          A = 255
        };
    }

    private static bool GetDWMBoolValue( string keyName )
    {
      int value = WindowColors.GetDWMIntValue( keyName );
      return ( value != 0 );
    }
  }
}
