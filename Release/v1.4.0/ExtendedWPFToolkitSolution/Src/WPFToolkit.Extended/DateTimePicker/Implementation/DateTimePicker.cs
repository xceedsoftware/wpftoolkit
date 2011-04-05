using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
#if VS2008
using Microsoft.Windows.Controls.Primitives;
#else
using System.Windows.Controls.Primitives;
#endif

namespace Microsoft.Windows.Controls
{
    public class DateTimePicker : Control
    {
        #region Members

        private Calendar _calendar;

        #endregion //Members

        #region Properties

        #region AllowSpin

        public static readonly DependencyProperty AllowSpinProperty = DependencyProperty.Register("AllowSpin", typeof(bool), typeof(DateTimePicker), new UIPropertyMetadata(true));
        public bool AllowSpin
        {
            get { return (bool)GetValue(AllowSpinProperty); }
            set { SetValue(AllowSpinProperty, value); }
        }

        #endregion //AllowSpin

        #region Format

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register("Format", typeof(DateTimeFormat), typeof(DateTimePicker), new UIPropertyMetadata(DateTimeFormat.FullDateTime, OnFormatChanged));
        public DateTimeFormat Format
        {
            get { return (DateTimeFormat)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        private static void OnFormatChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DateTimePicker DateTimePicker = o as DateTimePicker;
            if (DateTimePicker != null)
                DateTimePicker.OnFormatChanged((DateTimeFormat)e.OldValue, (DateTimeFormat)e.NewValue);
        }

        protected virtual void OnFormatChanged(DateTimeFormat oldValue, DateTimeFormat newValue)
        {

        }

        #endregion //Format

        #region FormatString

        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(DateTimePicker), new UIPropertyMetadata(default(String), OnFormatStringChanged));
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        private static void OnFormatStringChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DateTimePicker DateTimePicker = o as DateTimePicker;
            if (DateTimePicker != null)
                DateTimePicker.OnFormatStringChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnFormatStringChanged(string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
                throw new ArgumentException("CustomFormat should be specified.", FormatString);
        }

        #endregion //FormatString

        #region IsOpen

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(DateTimePicker), new UIPropertyMetadata(false));
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        #endregion //IsOpen

        #region ShowButtonSpinner

        public static readonly DependencyProperty ShowButtonSpinnerProperty = DependencyProperty.Register("ShowButtonSpinner", typeof(bool), typeof(DateTimePicker), new UIPropertyMetadata(true));
        public bool ShowButtonSpinner
        {
            get { return (bool)GetValue(ShowButtonSpinnerProperty); }
            set { SetValue(ShowButtonSpinnerProperty, value); }
        }

        #endregion //ShowButtonSpinner

        #region TimeWatermark

        public static readonly DependencyProperty TimeWatermarkProperty = DependencyProperty.Register("TimeWatermark", typeof(object), typeof(DateTimePicker), new UIPropertyMetadata(null));
        public object TimeWatermark
        {
            get { return (object)GetValue(TimeWatermarkProperty); }
            set { SetValue(TimeWatermarkProperty, value); }
        }

        #endregion //TimeWatermark

        #region TimeWatermarkTemplate

        public static readonly DependencyProperty TimeWatermarkTemplateProperty = DependencyProperty.Register("TimeWatermarkTemplate", typeof(DataTemplate), typeof(DateTimePicker), new UIPropertyMetadata(null));
        public DataTemplate TimeWatermarkTemplate
        {
            get { return (DataTemplate)GetValue(TimeWatermarkTemplateProperty); }
            set { SetValue(TimeWatermarkTemplateProperty, value); }
        }

        #endregion //TimeWatermarkTemplate

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(DateTime?), typeof(DateTimePicker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, OnCoerceValue));
        public DateTime? Value
        {
            get { return (DateTime?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static object OnCoerceValue(DependencyObject o, object value)
        {
            DateTimePicker dateTimePicker = o as DateTimePicker;
            if (dateTimePicker != null)
                return dateTimePicker.OnCoerceValue((DateTime?)value);
            else
                return value;
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DateTimePicker dateTimePicker = o as DateTimePicker;
            if (dateTimePicker != null)
                dateTimePicker.OnValueChanged((DateTime?)e.OldValue, (DateTime?)e.NewValue);
        }

        protected virtual DateTime? OnCoerceValue(DateTime? value)
        {
            // TODO: Keep the proposed value within the desired range.
            return value;
        }

        protected virtual void OnValueChanged(DateTime? oldValue, DateTime? newValue)
        {
            if (_calendar != null && _calendar.SelectedDate.HasValue && newValue.HasValue && _calendar.SelectedDate.Value != newValue.Value)
            {
                _calendar.SelectedDate = newValue;
                _calendar.DisplayDate = newValue.Value;
            }
        }

        #endregion //Value

        #region Watermark

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(object), typeof(DateTimePicker), new UIPropertyMetadata(null));
        public object Watermark
        {
            get { return (object)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        #endregion //Watermark

        #region WatermarkTemplate

        public static readonly DependencyProperty WatermarkTemplateProperty = DependencyProperty.Register("WatermarkTemplate", typeof(DataTemplate), typeof(DateTimePicker), new UIPropertyMetadata(null));
        public DataTemplate WatermarkTemplate
        {
            get { return (DataTemplate)GetValue(WatermarkTemplateProperty); }
            set { SetValue(WatermarkTemplateProperty, value); }
        }

        #endregion //WatermarkTemplate

        #endregion //Properties

        #region Constructors

        static DateTimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimePicker), new FrameworkPropertyMetadata(typeof(DateTimePicker)));
        }

        public DateTimePicker()
        {
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _calendar = (Calendar)GetTemplateChild("Part_Calendar");
            _calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;
            _calendar.SelectedDate = Value ?? null;
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (Mouse.Captured is CalendarItem) 
                Mouse.Capture(null);  
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                case Key.Tab:
                    {
                        CloseDateTimePicker();
                        break;
                    }
            }
        }

        private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
        {
            CloseDateTimePicker();
        }

        void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var newDate = (DateTime?)e.AddedItems[0];
                Value = newDate;
            }
        }

        #endregion //Event Handlers

        #region Methods

        private void CloseDateTimePicker()
        {
            if (IsOpen)
                IsOpen = false;
            ReleaseMouseCapture();
        }

        #endregion //Methods
    }
}
