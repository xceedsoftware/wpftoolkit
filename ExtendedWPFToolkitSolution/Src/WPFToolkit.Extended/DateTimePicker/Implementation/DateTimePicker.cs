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

            _calendar = (Calendar)GetTemplateChild("Part_Calendar");
            _calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;
            _calendar.SelectedDate = Value ?? null;
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (Mouse.Captured is CalendarItem) 
                Mouse.Capture(null);  
        }

        protected override void OnValueChanged(DateTime? oldValue, DateTime? newValue)
        {
            if (_calendar != null && _calendar.SelectedDate.HasValue && newValue.HasValue && _calendar.SelectedDate.Value != newValue.Value)
            {
                _calendar.SelectedDate = newValue;
                _calendar.DisplayDate = newValue.Value;
            }

            base.OnValueChanged(oldValue, newValue);
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
