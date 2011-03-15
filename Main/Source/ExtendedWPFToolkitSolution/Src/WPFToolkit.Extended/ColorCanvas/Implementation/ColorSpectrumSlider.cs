using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Windows.Controls.Core.Utilities;

namespace Microsoft.Windows.Controls
{
    internal class ColorSpectrumSlider : Slider
    {
        #region Private Members

        private Rectangle _spectrumDisplay;
        private LinearGradientBrush _pickerBrush;

        #endregion //Private Members

        #region Constructors

        static ColorSpectrumSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorSpectrumSlider), new FrameworkPropertyMetadata(typeof(ColorSpectrumSlider)));
        }

        #endregion //Constructors

        #region Dependency Properties

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorSpectrumSlider), new PropertyMetadata(System.Windows.Media.Colors.Transparent));
        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        #endregion //Dependency Properties

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _spectrumDisplay = (Rectangle)GetTemplateChild("PART_SpectrumDisplay");
            CreateSpectrum();
            OnValueChanged(Double.NaN, Value);
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);

            Color color = ColorUtilities.ConvertHsvToRgb(360 - newValue, 1, 1);
            SelectedColor = color;
        }

        #endregion //Base Class Overrides

        #region Methods

        private void CreateSpectrum()
        {
            _pickerBrush = new LinearGradientBrush();
            _pickerBrush.StartPoint = new Point(0.5, 0);
            _pickerBrush.EndPoint = new Point(0.5, 1);
            _pickerBrush.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;

            List<Color> colorsList = ColorUtilities.GenerateHsvSpectrum();

            double stopIncrement = (double)1 / colorsList.Count;

            int i;
            for (i = 0; i < colorsList.Count; i++)
            {
                _pickerBrush.GradientStops.Add(new GradientStop(colorsList[i], i * stopIncrement));
            }

            _pickerBrush.GradientStops[i - 1].Offset = 1.0;
            _spectrumDisplay.Fill = _pickerBrush;
        }

        #endregion //Methods
    }
}
