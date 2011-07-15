using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections;
using System.Globalization;

namespace Microsoft.Windows.Controls
{
    public class TimePicker : Control
    {
        #region Members

        ListBox _timeListBox;
        private DateTimeFormatInfo DateTimeFormatInfo { get; set; }
        internal static readonly TimeSpan EndTimeDefaultValue = new TimeSpan(23, 59, 0);
        internal static readonly TimeSpan StartTimeDefaultValue = new TimeSpan(0, 0, 0);
        internal static readonly TimeSpan TimeIntervalDefaultValue = new TimeSpan(1, 0, 0);

        #endregion //Members

        #region Properties

        #region AllowSpin

        public static readonly DependencyProperty AllowSpinProperty = DependencyProperty.Register("AllowSpin", typeof(bool), typeof(TimePicker), new UIPropertyMetadata(true));
        public bool AllowSpin
        {
            get { return (bool)GetValue(AllowSpinProperty); }
            set { SetValue(AllowSpinProperty, value); }
        }

        #endregion //AllowSpin

        #region EndTime

        public static readonly DependencyProperty EndTimeProperty = DependencyProperty.Register("EndTime", typeof(TimeSpan), typeof(TimePicker), new UIPropertyMetadata(EndTimeDefaultValue, new PropertyChangedCallback(OnEndTimeChanged), new CoerceValueCallback(OnCoerceEndTime)));

        private static object OnCoerceEndTime(DependencyObject o, object value)
        {
            TimePicker timePicker = o as TimePicker;
            if (timePicker != null)
                return timePicker.OnCoerceEndTime((TimeSpan)value);
            else
                return value;
        }

        private static void OnEndTimeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TimePicker timePicker = o as TimePicker;
            if (timePicker != null)
                timePicker.OnEndTimeChanged((TimeSpan)e.OldValue, (TimeSpan)e.NewValue);
        }

        protected virtual TimeSpan OnCoerceEndTime(TimeSpan value)
        {
            // TODO: Keep the proposed value within the desired range.
            return value;
        }

        protected virtual void OnEndTimeChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        public TimeSpan EndTime
        {
            // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
            get
            {
                return (TimeSpan)GetValue(EndTimeProperty);
            }
            set
            {
                SetValue(EndTimeProperty, value);
            }
        }


        #endregion //EndTime

        #region Format

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register("Format", typeof(TimeFormat), typeof(TimePicker), new UIPropertyMetadata(TimeFormat.ShortTime, OnFormatChanged));
        public TimeFormat Format
        {
            get { return (TimeFormat)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        private static void OnFormatChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TimePicker timePicker = o as TimePicker;
            if (timePicker != null)
                timePicker.OnFormatChanged((TimeFormat)e.OldValue, (TimeFormat)e.NewValue);
        }

        protected virtual void OnFormatChanged(TimeFormat oldValue, TimeFormat newValue)
        {

        }

        #endregion //Format

        #region FormatString

        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(TimePicker), new UIPropertyMetadata(default(String), OnFormatStringChanged));
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        private static void OnFormatStringChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TimePicker timePicker = o as TimePicker;
            if (timePicker != null)
                timePicker.OnFormatStringChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnFormatStringChanged(string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
                throw new ArgumentException("CustomFormat should be specified.", FormatString);
        }

        #endregion //FormatString

        #region IsOpen

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(TimePicker), new UIPropertyMetadata(false));
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        #endregion //IsOpen

        #region Maximum



        #endregion //Maximum

        #region Minimum



        #endregion //Minimum

        #region ShowButtonSpinner

        public static readonly DependencyProperty ShowButtonSpinnerProperty = DependencyProperty.Register("ShowButtonSpinner", typeof(bool), typeof(TimePicker), new UIPropertyMetadata(true));
        public bool ShowButtonSpinner
        {
            get { return (bool)GetValue(ShowButtonSpinnerProperty); }
            set { SetValue(ShowButtonSpinnerProperty, value); }
        }

        #endregion //ShowButtonSpinner

        #region StartTime

        public static readonly DependencyProperty StartTimeProperty = DependencyProperty.Register("StartTime", typeof(TimeSpan), typeof(TimePicker), new UIPropertyMetadata(StartTimeDefaultValue, new PropertyChangedCallback(OnStartTimeChanged), new CoerceValueCallback(OnCoerceStartTime)));

        private static object OnCoerceStartTime(DependencyObject o, object value)
        {
            TimePicker timePicker = o as TimePicker;
            if (timePicker != null)
                return timePicker.OnCoerceStartTime((TimeSpan)value);
            else
                return value;
        }

        private static void OnStartTimeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TimePicker timePicker = o as TimePicker;
            if (timePicker != null)
                timePicker.OnStartTimeChanged((TimeSpan)e.OldValue, (TimeSpan)e.NewValue);
        }

        protected virtual TimeSpan OnCoerceStartTime(TimeSpan value)
        {
            // TODO: Keep the proposed value within the desired range.
            return value;
        }

        protected virtual void OnStartTimeChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        public TimeSpan StartTime
        {
            // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
            get
            {
                return (TimeSpan)GetValue(StartTimeProperty);
            }
            set
            {
                SetValue(StartTimeProperty, value);
            }
        }


        #endregion //StartTime

        #region TimeInterval

        public static readonly DependencyProperty TimeIntervalProperty = DependencyProperty.Register("TimeInterval", typeof(TimeSpan), typeof(TimePicker), new UIPropertyMetadata(TimeIntervalDefaultValue, OnTimeIntervalChanged));
        public TimeSpan TimeInterval
        {
            get { return (TimeSpan)GetValue(TimeIntervalProperty); }
            set { SetValue(TimeIntervalProperty, value); }
        }

        private static void OnTimeIntervalChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TimePicker timePicker = o as TimePicker;
            if (timePicker != null)
                timePicker.OnTimeIntervalChanged((TimeSpan)e.OldValue, (TimeSpan)e.NewValue);
        }

        protected virtual void OnTimeIntervalChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //TimeInterval

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(DateTime?), typeof(TimePicker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));
        public DateTime? Value
        {
            get { return (DateTime?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TimePicker timePicker = o as TimePicker;
            if (timePicker != null)
                timePicker.OnValueChanged((DateTime?)e.OldValue, (DateTime?)e.NewValue);
        }

        protected virtual void OnValueChanged(DateTime? oldValue, DateTime? newValue)
        {
            //TODO: refactor this
            if (newValue.HasValue && _timeListBox != null)
            {
                var items = _timeListBox.ItemsSource;
                foreach (TimeItem item in items)
                {
                    if (item.Time == newValue.Value.TimeOfDay)
                    {
                        int index = _timeListBox.Items.IndexOf(item);
                        if (_timeListBox.SelectedIndex != index)
                            _timeListBox.SelectedIndex = index;
                        break;
                    }
                }
            }

            RoutedPropertyChangedEventArgs<object> args = new RoutedPropertyChangedEventArgs<object>(oldValue, newValue);
            args.RoutedEvent = ValueChangedEvent;
            RaiseEvent(args);
        }

        #endregion //Value

        #region Watermark

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(object), typeof(TimePicker), new UIPropertyMetadata(null));
        public object Watermark
        {
            get { return (object)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        #endregion //Watermark

        #region WatermarkTemplate

        public static readonly DependencyProperty WatermarkTemplateProperty = DependencyProperty.Register("WatermarkTemplate", typeof(DataTemplate), typeof(TimePicker), new UIPropertyMetadata(null));
        public DataTemplate WatermarkTemplate
        {
            get { return (DataTemplate)GetValue(WatermarkTemplateProperty); }
            set { SetValue(WatermarkTemplateProperty, value); }
        }

        #endregion //WatermarkTemplate

        #endregion //Properties

        #region Constructors

        static TimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
        }

        public TimePicker()
        {
            DateTimeFormatInfo = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentCulture);
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _timeListBox = (ListBox)GetTemplateChild("PART_TimeListItems");
            _timeListBox.ItemsSource = GenerateTimeListItemsSource();
            _timeListBox.SelectionChanged += TimeListBox_SelectionChanged;
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
                        CloseTimePicker();
                        break;
                    }
            }
        }

        private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
        {
            CloseTimePicker();
        }

        void TimeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                TimeItem selectedTimeListItem = (TimeItem)e.AddedItems[0];
                var time = selectedTimeListItem.Time; ;
                var date = Value ?? DateTime.MinValue;

                Value = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            }

            CloseTimePicker();
        }

        #endregion //Event Handlers

        #region Events

        //Due to a bug in Visual Studio, you cannot create event handlers for nullable args in XAML, so I have to use object instead.
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(TimePicker));
        public event RoutedPropertyChangedEventHandler<object> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        #endregion //Events

        #region Methods

        private void CloseTimePicker()
        {
            if (IsOpen)
                IsOpen = false;
            ReleaseMouseCapture();
        }

        public IEnumerable GenerateTimeListItemsSource()
        {
            TimeSpan time = StartTime;
            TimeSpan endTime = EndTime;

            if (endTime <= time)
            {
                endTime = EndTimeDefaultValue;
                time = StartTimeDefaultValue;
            }

            TimeSpan timeInterval = TimeInterval;

            if (time != null && endTime != null && timeInterval != null && timeInterval.Ticks > 0)
            {
                while (time <= endTime)
                {
                    yield return new TimeItem(DateTime.MinValue.Add(time).ToString(GetTimeFormat(), CultureInfo.CurrentCulture), time);
                    time = time.Add(timeInterval);
                }
                yield break;
            }
        }

        private string GetTimeFormat()
        {
            switch (Format)
            {
                case TimeFormat.Custom:
                    return FormatString;
                case TimeFormat.LongTime:
                    return DateTimeFormatInfo.LongTimePattern;
                case TimeFormat.ShortTime:
                    return DateTimeFormatInfo.ShortTimePattern;
                default:
                    return DateTimeFormatInfo.ShortTimePattern;
            }
        }

        #endregion //Methods
    }
}
