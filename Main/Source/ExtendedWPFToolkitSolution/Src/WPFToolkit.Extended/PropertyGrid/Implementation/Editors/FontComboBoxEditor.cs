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
            {
                return GetFontFamilies();
            }
            else if (propertyItem.PropertyType == typeof(FontWeight))
            {
                return GetFontWeights();
            }
            else if (propertyItem.PropertyType == typeof(FontStyle))
            {
                return GetFontStyles();
            }
            else if (propertyItem.PropertyType == typeof(FontStretch))
            {
                return GetFontStretches();
            }

            return null;
        }

        private static IList<object> GetFontFamilies()
        {
            IList<object> fontFamilies = new List<object>();

            //TODO: get all fonts
            fontFamilies.Add(new FontFamily("Arial"));
            fontFamilies.Add(new FontFamily("Courier New"));
            fontFamilies.Add(new FontFamily("Times New Roman"));
            fontFamilies.Add(new FontFamily("Batang"));
            fontFamilies.Add(new FontFamily("BatangChe"));
            fontFamilies.Add(new FontFamily("DFKai-SB"));
            fontFamilies.Add(new FontFamily("Dotum"));
            fontFamilies.Add(new FontFamily("DutumChe"));
            fontFamilies.Add(new FontFamily("FangSong"));
            fontFamilies.Add(new FontFamily("GulimChe"));
            fontFamilies.Add(new FontFamily("Gungsuh"));
            fontFamilies.Add(new FontFamily("GungsuhChe"));
            fontFamilies.Add(new FontFamily("KaiTi"));
            fontFamilies.Add(new FontFamily("Malgun Gothic"));
            fontFamilies.Add(new FontFamily("Meiryo"));
            fontFamilies.Add(new FontFamily("Microsoft JhengHei"));
            fontFamilies.Add(new FontFamily("Microsoft YaHei"));
            fontFamilies.Add(new FontFamily("MingLiU"));
            fontFamilies.Add(new FontFamily("MingLiu_HKSCS"));
            fontFamilies.Add(new FontFamily("MingLiu_HKSCS-ExtB"));
            fontFamilies.Add(new FontFamily("MingLiu-ExtB"));
            fontFamilies.Add(new FontFamily("Segoe UI"));

            return fontFamilies;
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
