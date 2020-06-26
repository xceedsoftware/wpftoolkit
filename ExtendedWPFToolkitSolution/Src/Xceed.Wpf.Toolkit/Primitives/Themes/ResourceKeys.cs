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

using System.Windows;

namespace Xceed.Wpf.Toolkit.Themes
{
  public static class ResourceKeys
  {
    #region Brush Keys

    public static readonly ComponentResourceKey ControlNormalBackgroundKey = new ComponentResourceKey( typeof( ResourceKeys ), "ControlNormalBackgroundKey" );
    public static readonly ComponentResourceKey ControlDisabledBackgroundKey = new ComponentResourceKey( typeof( ResourceKeys ), "ControlDisabledBackgroundKey" );
    public static readonly ComponentResourceKey ControlNormalBorderKey = new ComponentResourceKey( typeof( ResourceKeys ), "ControlNormalBorderKey" );
    public static readonly ComponentResourceKey ControlMouseOverBorderKey = new ComponentResourceKey( typeof( ResourceKeys ), "ControlMouseOverBorderKey" );
    public static readonly ComponentResourceKey ControlSelectedBorderKey = new ComponentResourceKey(typeof(ResourceKeys), "ControlSelectedBorderKey");
    public static readonly ComponentResourceKey ControlFocusedBorderKey = new ComponentResourceKey( typeof( ResourceKeys ), "ControlFocusedBorderKey" );

    public static readonly ComponentResourceKey ButtonNormalOuterBorderKey = new ComponentResourceKey( typeof( ResourceKeys ), "ButtonNormalOuterBorderKey" );
    public static readonly ComponentResourceKey ButtonNormalInnerBorderKey = new ComponentResourceKey( typeof( ResourceKeys ), "ButtonNormalInnerBorderKey" );
    public static readonly ComponentResourceKey ButtonNormalBackgroundKey = new ComponentResourceKey( typeof( ResourceKeys ), "ButtonNormalBackgroundKey" );

    public static readonly ComponentResourceKey ButtonMouseOverBackgroundKey = new ComponentResourceKey( typeof( ResourceKeys ), "ButtonMouseOverBackgroundKey" );
    public static readonly ComponentResourceKey ButtonMouseOverOuterBorderKey = new ComponentResourceKey( typeof( ResourceKeys ), "ButtonMouseOverOuterBorderKey" );
    public static readonly ComponentResourceKey ButtonMouseOverInnerBorderKey = new ComponentResourceKey( typeof( ResourceKeys ), "ButtonMouseOverInnerBorderKey" );

    public static readonly ComponentResourceKey ButtonPressedOuterBorderKey = new ComponentResourceKey( typeof( ResourceKeys ), "ButtonPressedOuterBorderKey" );
    public static readonly ComponentResourceKey ButtonPressedInnerBorderKey = new ComponentResourceKey( typeof( ResourceKeys ), "ButtonPressedInnerBorderKey" );
    public static readonly ComponentResourceKey ButtonPressedBackgroundKey = new ComponentResourceKey( typeof( ResourceKeys ), "ButtonPressedBackgroundKey" );

    public static readonly ComponentResourceKey ButtonFocusedOuterBorderKey = new ComponentResourceKey( typeof( ResourceKeys ), "ButtonFocusedOuterBorderKey" );
    public static readonly ComponentResourceKey ButtonFocusedInnerBorderKey = new ComponentResourceKey( typeof( ResourceKeys ), "ButtonFocusedInnerBorderKey" );
    public static readonly ComponentResourceKey ButtonFocusedBackgroundKey = new ComponentResourceKey( typeof( ResourceKeys ), "ButtonFocusedBackgroundKey" );

    public static readonly ComponentResourceKey ButtonDisabledOuterBorderKey = new ComponentResourceKey( typeof( ResourceKeys ), "ButtonDisabledOuterBorderKey" );
    public static readonly ComponentResourceKey ButtonInnerBorderDisabledKey = new ComponentResourceKey( typeof( ResourceKeys ), "ButtonInnerBorderDisabledKey" );

    #endregion //Brush Keys

    public static readonly ComponentResourceKey GlyphNormalForegroundKey = new ComponentResourceKey( typeof( ResourceKeys ), "GlyphNormalForegroundKey" );
    public static readonly ComponentResourceKey GlyphDisabledForegroundKey = new ComponentResourceKey( typeof( ResourceKeys ), "GlyphDisabledForegroundKey" );

    public static readonly ComponentResourceKey SpinButtonCornerRadiusKey = new ComponentResourceKey( typeof( ResourceKeys ), "SpinButtonCornerRadiusKey" );

    public static readonly ComponentResourceKey SpinnerButtonStyleKey = new ComponentResourceKey( typeof( ResourceKeys ), "SpinnerButtonStyleKey" );

  }
}
