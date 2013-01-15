using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Microsoft.Windows.Controls.PropertyGrid.Implementation.EditorProviders
{
    public class FontComboBoxEditorProvider : ITypeEditorProvider
    {
        ComboBox _comboBox;

        public FontComboBoxEditorProvider()
        {
            _comboBox = new ComboBox();
        }

        public void Initialize(PropertyItem propertyItem)
        {
            ResolveBinding(propertyItem);
            SetItemsSource(propertyItem);
        }

        public FrameworkElement ResolveEditor()
        {
            return _comboBox;
        }

        private void ResolveBinding(PropertyItem property)
        {
            var binding = new Binding(property.Name);
            binding.Source = property.Instance;
            binding.ValidatesOnExceptions = true;
            binding.ValidatesOnDataErrors = true;

            if (property.IsWriteable)
                binding.Mode = BindingMode.TwoWay;
            else
                binding.Mode = BindingMode.OneWay;

            BindingOperations.SetBinding(_comboBox, ComboBox.SelectedItemProperty, binding);
        }

        private void SetItemsSource(PropertyItem property)
        {
            if (property.PropertyType == typeof(FontFamily))
            {
                List<FontFamily> fonts = new List<FontFamily>();
                fonts.Add(new FontFamily("Arial"));
                fonts.Add(new FontFamily("Courier New"));
                fonts.Add(new FontFamily("Times New Roman"));
                fonts.Add(new FontFamily("Batang"));
                fonts.Add(new FontFamily("BatangChe"));
                fonts.Add(new FontFamily("DFKai-SB"));
                fonts.Add(new FontFamily("Dotum"));
                fonts.Add(new FontFamily("DutumChe"));
                fonts.Add(new FontFamily("FangSong"));
                fonts.Add(new FontFamily("GulimChe"));
                fonts.Add(new FontFamily("Gungsuh"));
                fonts.Add(new FontFamily("GungsuhChe"));
                fonts.Add(new FontFamily("KaiTi"));
                fonts.Add(new FontFamily("Malgun Gothic"));
                fonts.Add(new FontFamily("Meiryo"));
                fonts.Add(new FontFamily("Microsoft JhengHei"));
                fonts.Add(new FontFamily("Microsoft YaHei"));
                fonts.Add(new FontFamily("MingLiU"));
                fonts.Add(new FontFamily("MingLiu_HKSCS"));
                fonts.Add(new FontFamily("MingLiu_HKSCS-ExtB"));
                fonts.Add(new FontFamily("MingLiu-ExtB"));
                _comboBox.ItemsSource = fonts;
            }
            else if (property.PropertyType == typeof(FontWeight))
            {
                List<FontWeight> list = new List<FontWeight>() 
                    {
                        FontWeights.Black, FontWeights.Bold, FontWeights.ExtraBlack, FontWeights.ExtraBold, 
                        FontWeights.ExtraLight, FontWeights.Light, FontWeights.Medium, FontWeights.Normal, FontWeights.SemiBold, 
                        FontWeights.Thin 
                    };
                _comboBox.ItemsSource = list;
            }
            else if (property.PropertyType == typeof(FontStyle))
            {
                List<FontStyle> list = new List<FontStyle>() 
                    {
                        FontStyles.Italic,
                        FontStyles.Normal
                    };
                _comboBox.ItemsSource = list;
            }
            else if (property.PropertyType == typeof(FontStretch))
            {
                List<FontStretch> list = new List<FontStretch>() 
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
                _comboBox.ItemsSource = list;
            }
        }
    }
}
