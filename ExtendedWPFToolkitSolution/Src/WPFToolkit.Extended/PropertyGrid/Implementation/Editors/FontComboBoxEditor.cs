using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class FontComboBoxEditor : ComboBoxEditor
    {
        protected override IList<object> CreateItemsSource(PropertyItem propertyItem)
        {
            if (propertyItem.PropertyType == typeof(FontFamily))
                return GetFontFamilies();
            else if (propertyItem.PropertyType == typeof(FontWeight))
                return GetFontWeights();
            else if (propertyItem.PropertyType == typeof(FontStyle))
                return GetFontStyles();
            else if (propertyItem.PropertyType == typeof(FontStretch))
                return GetFontStretches();

            return null;
        }

        private static IList<object> GetFontFamilies()
        {
#if !VS2008
            return Fonts.SystemFontFamilies.ToList<object>();
#else
            return Fonts.SystemFontFamilies.Cast<object>().ToList();
#endif
        }

        private static IList<object> GetFontWeights()
        {
            return new List<object>()
            {
                FontWeights.Black, 
                FontWeights.Bold, 
                FontWeights.ExtraBlack, 
                FontWeights.ExtraBold,
                FontWeights.ExtraLight, 
                FontWeights.Light, 
                FontWeights.Medium, 
                FontWeights.Normal, 
                FontWeights.SemiBold,
                FontWeights.Thin
            };
        }

        private static IList<object> GetFontStyles()
        {
            return new List<object>()
            {
                FontStyles.Italic,
                FontStyles.Normal
            };
        }

        private static IList<object> GetFontStretches()
        {
            return new List<object>()
            {
                FontStretches.Condensed,
                FontStretches.Expanded,
                FontStretches.ExtraCondensed,
                FontStretches.ExtraExpanded,
                FontStretches.Normal,
                FontStretches.SemiCondensed,
                FontStretches.SemiExpanded,
                FontStretches.UltraCondensed,
                FontStretches.UltraExpanded
            };
        }
    }
}
