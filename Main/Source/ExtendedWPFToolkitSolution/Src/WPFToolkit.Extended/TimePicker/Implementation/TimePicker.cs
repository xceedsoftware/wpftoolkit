using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;

namespace Microsoft.Windows.Controls
{
    public class TimePicker : Control
    {
        internal static readonly TimeSpan EndTimeDefaultValue = new TimeSpan(23, 59, 0);
        internal static readonly TimeSpan StartTimeDefaultValue = new TimeSpan(0, 0, 0);
        internal static readonly TimeSpan TimeIntervalDefaultValue = new TimeSpan(1, 0, 0);

        #region Members

        ListBox _timeListBox;

        #endregion //Members

        #region Properties

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

        #region SelectedDate

        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register("SelectedDate", typeof(DateTime?), typeof(TimePicker), new UIPropertyMetadata(DateTime.Now, OnSelectedDateChanged));
        public DateTime? SelectedDate
        {
            get { return (DateTime?)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }

        private static void OnSelectedDateChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TimePicker timePicker = o as TimePicker;
            if (timePicker != null)
                timePicker.OnSelectedDateChanged((DateTime?)e.OldValue, (DateTime?)e.NewValue);
        }

        protected virtual void OnSelectedDateChanged(DateTime? oldValue, DateTime? newValue)
        {

        }

        #endregion //SelectedDate

        #region SelectedTime

        public static readonly DependencyProperty SelectedTimeProperty = DependencyProperty.Register("SelectedTime", typeof(TimeSpan?), typeof(TimePicker), new UIPropertyMetadata(null, OnSelectedTimeChanged));
        public TimeSpan? SelectedTime
        {
            get { return (TimeSpan?)GetValue(SelectedTimeProperty); }
            set { SetValue(SelectedTimeProperty, value); }
        }

        private static void OnSelectedTimeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TimePicker timePicker = o as TimePicker;
            if (timePicker != null)
                timePicker.OnSelectedTimeChanged((TimeSpan?)e.OldValue, (TimeSpan?)e.NewValue);
        }

        protected virtual void OnSelectedTimeChanged(TimeSpan? oldValue, TimeSpan? newValue)
        {
            var current = DateTime.Now;

            var date = SelectedDate ?? current;
            var time = SelectedTime ?? current.TimeOfDay;

            SelectedDate = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }        

        #endregion //SelectedTime

        #endregion //Properties

        #region Constructors

        static TimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
        }

        public TimePicker()
        {
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _timeListBox = (ListBox)GetTemplateChild("PART_TimeListItems");
            _timeListBox.ItemsSource = GenerateItemsSource();
            _timeListBox.SelectionChanged += new SelectionChangedEventHandler(_timeListBox_SelectionChanged);
        }

        void _timeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (e.AddedItems.Count > 0)
            //{
            //    TimeSpan newTime = (TimeSpan)e.AddedItems[0];

            //    var current = DateTime.Now;

            //    var date = this.SelectedDate.HasValue ? this.SelectedDate.Value : current;
            //    var time = this.SelectedTime.HasValue ? this.SelectedTime.Value : current.TimeOfDay;

            //    SelectedDate = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            //}

            CloseTimePicker();
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

        #endregion //Event Handlers

        #region Methods

        private void CloseTimePicker()
        {
            if (IsOpen)
                IsOpen = false;
            ReleaseMouseCapture();
        }

        private static IEnumerable GenerateItemsSource()
        {
            //TimeSpan time = this.StartTime;
            //TimeSpan endTime = this.EndTime;

            TimeSpan time = StartTimeDefaultValue;
            TimeSpan endTime = EndTimeDefaultValue;

            if (endTime <= time)
            {
                endTime = EndTimeDefaultValue;
                time = StartTimeDefaultValue;
            }

            //TimeSpan timeInterval = this.TimeInterval;
            TimeSpan timeInterval = TimeIntervalDefaultValue;

            if (time != null && endTime != null && timeInterval != null && timeInterval.Ticks > 0)
            {
                while (time <= endTime)
                {
                    yield return time;
                    time = time.Add(timeInterval);
                }
                yield break;
            }

        }

        #endregion //Methods
    }
}
