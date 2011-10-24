using System;
using System.Windows;

namespace Microsoft.Windows.Controls.Themes
{
    public static class ResourceKeys
    {
        #region Brush Keys

        public static readonly ResourceKey ControlNormalBackgroundKey = new StaticResourceKey(typeof(ResourceKeys), "ControlNormalBackgroundKey");
        public static readonly ResourceKey ControlDisabledBackgroundKey = new StaticResourceKey(typeof(ResourceKeys), "ControlDisabledBackgroundKey");
        public static readonly ResourceKey ControlNormalBorderKey = new StaticResourceKey(typeof(ResourceKeys), "ControlNormalBorderKey");
        public static readonly ResourceKey ControlMouseOverBorderKey = new StaticResourceKey(typeof(ResourceKeys), "ControlMouseOverBorderKey");
        public static readonly ResourceKey ControlFocusedBorderKey = new StaticResourceKey(typeof(ResourceKeys), "ControlFocusedBorderKey");

        public static readonly ResourceKey ButtonNormalOuterBorderKey = new StaticResourceKey(typeof(ResourceKeys), "ButtonNormalOuterBorderKey");
        public static readonly ResourceKey ButtonNormalInnerBorderKey = new StaticResourceKey(typeof(ResourceKeys), "ButtonNormalInnerBorderKey");
        public static readonly ResourceKey ButtonNormalBackgroundKey = new StaticResourceKey(typeof(ResourceKeys), "ButtonNormalBackgroundKey");

        public static readonly ResourceKey ButtonMouseOverBackgroundKey = new StaticResourceKey(typeof(ResourceKeys), "ButtonMouseOverBackgroundKey");
        public static readonly ResourceKey ButtonMouseOverOuterBorderKey = new StaticResourceKey(typeof(ResourceKeys), "ButtonMouseOverOuterBorderKey");
        public static readonly ResourceKey ButtonMouseOverInnerBorderKey = new StaticResourceKey(typeof(ResourceKeys), "ButtonMouseOverInnerBorderKey");

        public static readonly ResourceKey ButtonPressedOuterBorderKey = new StaticResourceKey(typeof(ResourceKeys), "ButtonPressedOuterBorderKey");
        public static readonly ResourceKey ButtonPressedInnerBorderKey = new StaticResourceKey(typeof(ResourceKeys), "ButtonPressedInnerBorderKey");
        public static readonly ResourceKey ButtonPressedBackgroundKey = new StaticResourceKey(typeof(ResourceKeys), "ButtonPressedBackgroundKey");

        #endregion //Brush Keys

        public static readonly ResourceKey GlyphNormalForegroundKey = new StaticResourceKey(typeof(ResourceKeys), "GlyphNormalForegroundKey");

        public static readonly ResourceKey SpinButtonCornerRadiusKey = new StaticResourceKey(typeof(ResourceKeys), "SpinButtonCornerRadiusKey");
    }
}