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
    public class DateTimePicker : DateTimeUpDown
    {
        #region Members

        private Calendar _calendar;

        #endregion //Members

        #region Properties

        #region IsOpen

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(DateTimePicker), new UIPropertyMetadata(false));
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        #endregion //IsOpen

        #region TimeFormat

        public static readonly DependencyProperty TimeFormatProperty = DependencyProperty.Register("TimeFormat", typeof(TimeFormat), typeof(DateTimePicker), new UIPropertyMetadata(TimeFormat.ShortTime));
        public TimeFormat TimeFormat
        {
            get { return (TimeFormat)GetValue(TimeFormatProperty); }
            set { SetValue(TimeFormatProperty, value); }
        }

        #endregion //TimeFormat

        #region TimeFormatString

        public static readonly DependencyProperty TimeFormatStringProperty = DependencyProperty.Register("TimeFormatString", typeof(string), typeof(DateTimePicker), new UIPropertyMetadata(default(String)));
        public string TimeFormatString
        {
            get { return (string)GetValue(TimeFormatStringProperty); }
            set { SetValue(TimeFormatStringProperty, value); }
        }

        #endregion //TimeFormatString

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

            if (_calendar != null)
            {
                _calendar.SelectedDatesChanged -= Calendar_SelectedDatesChanged;
            }

            _calendar = GetTemplateChild("Part_Calendar") as Calendar;

            if (_calendar != null)
            {
                _calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;
                _calendar.SelectedDate = Value ?? null;
                _calendar.DisplayDate = Value ?? DateTime.Now;
            }
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (Mouse.Captured is CalendarItem)
                Mouse.Capture(null);
        }

        protected override void OnValueChanged(DateTime? oldValue, DateTime? newValue)
        {
            if (_calendar != null && _calendar.SelectedDate != newValue)
            {
                _calendar.SelectedDate = newValue;
                _calendar.DisplayDate = newValue.Value;
            }

            base.OnValueChanged(oldValue, newValue);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            //if the calendar is open then we don't want to modify the behavior of navigating the calendar control with the Up/Down keys.
            if (!IsOpen)
                base.OnPreviewKeyDown(e);
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
