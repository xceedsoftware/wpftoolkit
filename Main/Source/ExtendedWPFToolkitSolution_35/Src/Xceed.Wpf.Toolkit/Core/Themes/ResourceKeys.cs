/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;

namespace Xceed.Wpf.Toolkit.Themes
{
  public static class ResourceKeys
  {
    #region Brush Keys

    public static readonly ResourceKey ControlNormalBackgroundKey = new StaticResourceKey( typeof( ResourceKeys ), "ControlNormalBackgroundKey" );
    public static readonly ResourceKey ControlDisabledBackgroundKey = new StaticResourceKey( typeof( ResourceKeys ), "ControlDisabledBackgroundKey" );
    public static readonly ResourceKey ControlNormalBorderKey = new StaticResourceKey( typeof( ResourceKeys ), "ControlNormalBorderKey" );
    public static readonly ResourceKey ControlMouseOverBorderKey = new StaticResourceKey( typeof( ResourceKeys ), "ControlMouseOverBorderKey" );
    public static readonly ResourceKey ControlSelectedBorderKey = new StaticResourceKey(typeof(ResourceKeys), "ControlSelectedBorderKey");
    public static readonly ResourceKey ControlFocusedBorderKey = new StaticResourceKey( typeof( ResourceKeys ), "ControlFocusedBorderKey" );

    public static readonly ResourceKey ButtonNormalOuterBorderKey = new StaticResourceKey( typeof( ResourceKeys ), "ButtonNormalOuterBorderKey" );
    public static readonly ResourceKey ButtonNormalInnerBorderKey = new StaticResourceKey( typeof( ResourceKeys ), "ButtonNormalInnerBorderKey" );
    public static readonly ResourceKey ButtonNormalBackgroundKey = new StaticResourceKey( typeof( ResourceKeys ), "ButtonNormalBackgroundKey" );

    public static readonly ResourceKey ButtonMouseOverBackgroundKey = new StaticResourceKey( typeof( ResourceKeys ), "ButtonMouseOverBackgroundKey" );
    public static readonly ResourceKey ButtonMouseOverOuterBorderKey = new StaticResourceKey( typeof( ResourceKeys ), "ButtonMouseOverOuterBorderKey" );
    public static readonly ResourceKey ButtonMouseOverInnerBorderKey = new StaticResourceKey( typeof( ResourceKeys ), "ButtonMouseOverInnerBorderKey" );

    public static readonly ResourceKey ButtonPressedOuterBorderKey = new StaticResourceKey( typeof( ResourceKeys ), "ButtonPressedOuterBorderKey" );
    public static readonly ResourceKey ButtonPressedInnerBorderKey = new StaticResourceKey( typeof( ResourceKeys ), "ButtonPressedInnerBorderKey" );
    public static readonly ResourceKey ButtonPressedBackgroundKey = new StaticResourceKey( typeof( ResourceKeys ), "ButtonPressedBackgroundKey" );

    public static readonly ResourceKey ButtonFocusedOuterBorderKey = new StaticResourceKey( typeof( ResourceKeys ), "ButtonFocusedOuterBorderKey" );
    public static readonly ResourceKey ButtonFocusedInnerBorderKey = new StaticResourceKey( typeof( ResourceKeys ), "ButtonFocusedInnerBorderKey" );
    public static readonly ResourceKey ButtonFocusedBackgroundKey = new StaticResourceKey( typeof( ResourceKeys ), "ButtonFocusedBackgroundKey" );

    public static readonly ResourceKey ButtonDisabledOuterBorderKey = new StaticResourceKey( typeof( ResourceKeys ), "ButtonDisabledOuterBorderKey" );
    public static readonly ResourceKey ButtonInnerBorderDisabledKey = new StaticResourceKey( typeof( ResourceKeys ), "ButtonInnerBorderDisabledKey" );

    #endregion //Brush Keys

    public static readonly ResourceKey GlyphNormalForegroundKey = new StaticResourceKey( typeof( ResourceKeys ), "GlyphNormalForegroundKey" );
    public static readonly ResourceKey GlyphDisabledForegroundKey = new StaticResourceKey( typeof( ResourceKeys ), "GlyphDisabledForegroundKey" );

    public static readonly ResourceKey SpinButtonCornerRadiusKey = new StaticResourceKey( typeof( ResourceKeys ), "SpinButtonCornerRadiusKey" );

    public static readonly ResourceKey SpinnerButtonStyleKey = new StaticResourceKey( typeof( ResourceKeys ), "SpinnerButtonStyleKey" );

  }
}
