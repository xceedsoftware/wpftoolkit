using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Microsoft.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    public class DateTimeUpDown : UpDownBase
    {
        #region Members

        private List<DateTimeInfo> _dateTimeInfoList = new List<DateTimeInfo>();
        private DateTimeInfo _selectedDateTimeInfo;
        private bool _fireSelectionChangedEvent = true;
        private bool _isSyncingTextAndValueProperties;

        #endregion //Members

        #region Properties

        private DateTimeFormatInfo DateTimeFormatInfo { get; set; }

        //TODO: add minimum and maximum properties

        #region Format

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register("Format", typeof(DateTimeFormat), typeof(DateTimeUpDown), new UIPropertyMetadata(DateTimeFormat.FullDateTime, OnFormatChanged));
        public DateTimeFormat Format
        {
            get { return (DateTimeFormat)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        private static void OnFormatChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DateTimeUpDown dateTimeUpDown = o as DateTimeUpDown;
            if (dateTimeUpDown != null)
                dateTimeUpDown.OnFormatChanged((DateTimeFormat)e.OldValue, (DateTimeFormat)e.NewValue);
        }

        protected virtual void OnFormatChanged(DateTimeFormat oldValue, DateTimeFormat newValue)
        {
            //if using a CustomFormat then the initialization occurs on the CustomFormatString property
            if (newValue != DateTimeFormat.Custom)
                InitializeDateTimeInfoListAndParseValue();
        }

        #endregion //Format

        #region FormatString

        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(DateTimeUpDown), new UIPropertyMetadata(default(String), OnFormatStringChanged));
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        private static void OnFormatStringChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DateTimeUpDown dateTimeUpDown = o as DateTimeUpDown;
            if (dateTimeUpDown != null)
                dateTimeUpDown.OnFormatStringChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnFormatStringChanged(string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
                throw new ArgumentException("CustomFormat should be specified.", FormatString);

            InitializeDateTimeInfoListAndParseValue();
        }

        #endregion //FormatString

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(DateTime?), typeof(DateTimeUpDown), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, OnCoerceValue));
        public DateTime? Value
        {
            get { return (DateTime?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static object OnCoerceValue(DependencyObject o, object value)
        {
            DateTimeUpDown dateTimeUpDown = o as DateTimeUpDown;
            if (dateTimeUpDown != null)
                return dateTimeUpDown.OnCoerceValue((DateTime?)value);
            else
                return value;
        }

        protected virtual DateTime? OnCoerceValue(DateTime? value)
        {
            //if the user entered a string value to represent a date or time, we need to parse that string into a valid DatTime value
            if (value != null && !(value is DateTime))
            {
                return DateTime.Parse(value.ToString(), DateTimeFormatInfo);
            }

            return value;
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DateTimeUpDown dateTimeUpDown = o as DateTimeUpDown;
            if (dateTimeUpDown != null)
                dateTimeUpDown.OnValueChanged((DateTime?)e.OldValue, (DateTime?)e.NewValue);
        }

        protected virtual void OnValueChanged(DateTime? oldValue, DateTime? newValue)
        {
            //whenever the value changes we need to parse out the value into out DateTimeInfo segments so we can keep track of the individual pieces
            //but only if it is not null
            if (newValue != null)
                ParseValueIntoDateTimeInfo();

            SyncTextAndValueProperties(DateTimeUpDown.ValueProperty, newValue);

            RoutedPropertyChangedEventArgs<DateTime?> args = new RoutedPropertyChangedEventArgs<DateTime?>(oldValue, newValue);
            args.RoutedEvent = DateTimeUpDown.ValueChangedEvent;
            RaiseEvent(args);
        }

        #endregion //Value

        #endregion //Properties

        #region Constructors

        static DateTimeUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimeUpDown), new FrameworkPropertyMetadata(typeof(DateTimeUpDown)));
        }

        public DateTimeUpDown()
        {
            DateTimeFormatInfo = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentCulture);
            InitializeDateTimeInfoList();
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TextBox.SelectionChanged += TextBox_SelectionChanged;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    {
                        return;
                    }
                case Key.Delete:
                    {
                        Value = null;
                        break;
                    }
                case Key.Left:
                    {
                        PerformKeyboardSelection(-1);
                        break;
                    }
                case Key.Right:
                    {
                        PerformKeyboardSelection(1);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            base.OnPreviewKeyDown(e);
        }

        #endregion //Base Class Overrides

        #region Event Hanlders

        void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_fireSelectionChangedEvent)
                PerformMouseSelection();
            else
                _fireSelectionChangedEvent = true;
        }

        #endregion //Event Hanlders

        #region Events

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<DateTime?>), typeof(DateTimeUpDown));
        public event RoutedPropertyChangedEventHandler<DateTime?> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        #endregion //Events

        #region Methods

        #region Abstract

        protected override void OnIncrement()
        {
            if (Value != null)
                UpdateDateTime(1);
        }

        protected override void OnDecrement()
        {
            if (Value != null)
                UpdateDateTime(-1);
        }

        #endregion //Abstract

        #region Private

        private void InitializeDateTimeInfoListAndParseValue()
        {
            InitializeDateTimeInfoList();
            if (Value != null)
                ParseValueIntoDateTimeInfo();
        }

        private void InitializeDateTimeInfoList()
        {
            _dateTimeInfoList.Clear();

            string format = GetFormatString(Format);
            while (format.Length > 0)
            {
                int elementLength = GetElementLengthByFormat(format);
                DateTimeInfo info = null;

                switch (format[0])
                {
                    case '"':
                    case '\'':
                        {
                            int closingQuotePosition = format.IndexOf(format[0], 1);
                            info = new DateTimeInfo { IsReadOnly = true, Type = DateTimePart.Other, Length = 1, Content = format.Substring(1, Math.Max(1, closingQuotePosition - 1)).ToString() };
                            elementLength = Math.Max(1, closingQuotePosition + 1);
                            break;
                        }
                    case 'D':
                    case 'd':
                        {
                            string d = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                d = "%" + d;

                            if (elementLength > 2)
                                info = new DateTimeInfo { IsReadOnly = true, Type = DateTimePart.DayName, Format = d };
                            else
                                info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Day, Format = d };
                            break;
                        }
                    case 'F':
                    case 'f':
                        {
                            string f = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                f = "%" + f;

                            info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Millisecond, Format = f };
                            break;
                        }
                    case 'h':
                        {
                            string h = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                h = "%" + h;

                            info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Hour12, Format = h };
                            break;
                        }
                    case 'H':
                        {
                            string H = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                H = "%" + H;

                            info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Hour24, Format = H };
                            break;
                        }
                    case 'M':
                        {
                            string M = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                M = "%" + M;

                            if (elementLength >= 3)
                                info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.MonthName, Format = M };
                            else
                                info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Month, Format = M };
                            break;
                        }
                    case 'S':
                    case 's':
                        {
                            string s = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                s = "%" + s;

                            info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Second, Format = s };
                            break;
                        }
                    case 'T':
                    case 't':
                        {
                            string t = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                t = "%" + t;

                            info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.AmPmDesignator, Format = t };
                            break;
                        }
                    case 'Y':
                    case 'y':
                        {
                            string y = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                y = "%" + y;

                            info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Year, Format = y };
                            break;
                        }
                    case '\\':
                        {
                            if (format.Length >= 2)
                            {
                                info = new DateTimeInfo { IsReadOnly = true, Content = format.Substring(1, 1), Length = 1, Type = DateTimePart.Other };
                                elementLength = 2;
                            }
                            break;
                        }
                    case 'g':
                        {
                            string g = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                g = "%" + g;

                            info = new DateTimeInfo { IsReadOnly = true, Type = DateTimePart.Period, Format = format.Substring(0, elementLength) };
                            break;
                        }
                    case 'm':
                        {
                            string m = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                m = "%" + m;

                            info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Minute, Format = m };
                            break;
                        }
                    case 'z':
                        {
                            string z = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                z = "%" + z;

                            info = new DateTimeInfo { IsReadOnly = true, Type = DateTimePart.TimeZone, Format = z };
                            break;
                        }
                    default:
                        {
                            elementLength = 1;
                            info = new DateTimeInfo { IsReadOnly = true, Length = 1, Content = format[0].ToString(), Type = DateTimePart.Other };
                            break;
                        }
                }

                _dateTimeInfoList.Add(info);
                format = format.Substring(elementLength);
            }
        }

        private static int GetElementLengthByFormat(string format)
        {
            for (int i = 1; i < format.Length; i++)
            {
                if (String.Compare(format[i].ToString(), format[0].ToString(), false) != 0)
                {
                    return i;
                }
            }
            return format.Length;
        }

        private void ParseValueIntoDateTimeInfo()
        {
            string text = string.Empty;

            _dateTimeInfoList.ForEach(info =>
            {
                if (info.Format == null)
                {
                    info.StartPosition = text.Length;
                    info.Length = info.Content.Length;
                    text += info.Content;
                }
                else
                {
                    DateTime date = DateTime.Parse(Value.ToString());
                    info.StartPosition = text.Length;
                    info.Content = date.ToString(info.Format, DateTimeFormatInfo);
                    info.Length = info.Content.Length;
                    text += info.Content;
                }
            });
        }

        private void PerformMouseSelection()
        {
            _dateTimeInfoList.ForEach(info =>
            {
                if ((info.StartPosition <= TextBox.SelectionStart) && (TextBox.SelectionStart < (info.StartPosition + info.Length)))
                {
                    Select(info);
                    return;
                }
            });
        }

        /// <summary>
        /// Performs the keyboard selection.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <remarks>-1 = Left, 1 = Right</remarks>
        private void PerformKeyboardSelection(int direction)
        {
            DateTimeInfo info;
            int index = _dateTimeInfoList.IndexOf(_selectedDateTimeInfo);

            //make sure we stay within the selection ranges
            if ((index == 0 && direction == -1) || (index == _dateTimeInfoList.Count - 1 && direction == 1))
                return;

            //get the DateTimeInfo at the next position
            index += direction;
            info = _dateTimeInfoList[index];

            //we don't care about spaces and commas, only select valid DateTimeInfos
            while (info.Type == DateTimePart.Other)
            {
                info = _dateTimeInfoList[index += direction];
            }

            //perform selection
            Select(info);
        }

        private void Select(DateTimeInfo info)
        {
            _fireSelectionChangedEvent = false;
            TextBox.Select(info.StartPosition, info.Length);
            _fireSelectionChangedEvent = true;
            _selectedDateTimeInfo = info;
        }

        private string GetFormatString(DateTimeFormat dateTimeFormat)
        {
            switch (dateTimeFormat)
            {
                case DateTimeFormat.ShortDate:
                    return DateTimeFormatInfo.ShortDatePattern;
                case DateTimeFormat.LongDate:
                    return DateTimeFormatInfo.LongDatePattern;
                case DateTimeFormat.ShortTime:
                    return DateTimeFormatInfo.ShortTimePattern;
                case DateTimeFormat.LongTime:
                    return DateTimeFormatInfo.LongTimePattern;
                case DateTimeFormat.FullDateTime:
                    return DateTimeFormatInfo.FullDateTimePattern;
                case DateTimeFormat.MonthDay:
                    return DateTimeFormatInfo.MonthDayPattern;
                case DateTimeFormat.RFC1123:
                    return DateTimeFormatInfo.RFC1123Pattern;
                case DateTimeFormat.SortableDateTime:
                    return DateTimeFormatInfo.SortableDateTimePattern;
                case DateTimeFormat.UniversalSortableDateTime:
                    return DateTimeFormatInfo.UniversalSortableDateTimePattern;
                case DateTimeFormat.YearMonth:
                    return DateTimeFormatInfo.YearMonthPattern;
                case DateTimeFormat.Custom:
                    return FormatString;
                default:
                    throw new ArgumentException("Not a supported format");
            }
        }

        private void UpdateDateTime(int value)
        {
            _fireSelectionChangedEvent = false;
            DateTimeInfo info = _selectedDateTimeInfo;

            //this only occurs when the user manually type in a value for the Value Property
            if (info == null)
                info = _dateTimeInfoList[0];


            switch (info.Type)
            {
                case DateTimePart.Year:
                    {
                        Value = ((DateTime)Value).AddYears(value);
                        break;
                    }
                case DateTimePart.Month:
                case DateTimePart.MonthName:
                    {
                        Value = ((DateTime)Value).AddMonths(value);
                        break;
                    }
                case DateTimePart.Day:
                case DateTimePart.DayName:
                    {
                        Value = ((DateTime)Value).AddDays(value);
                        break;
                    }
                case DateTimePart.Hour12:
                case DateTimePart.Hour24:
                    {
                        Value = ((DateTime)Value).AddHours(value);
                        break;
                    }
                case DateTimePart.Minute:
                    {
                        Value = ((DateTime)Value).AddMinutes(value);
                        break;
                    }
                case DateTimePart.Second:
                    {
                        Value = ((DateTime)Value).AddSeconds(value);
                        break;
                    }
                case DateTimePart.Millisecond:
                    {
                        Value = ((DateTime)Value).AddMilliseconds(value);
                        break;
                    }
                case DateTimePart.AmPmDesignator:
                    {
                        Value = ((DateTime)Value).AddHours(value * 12);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            //we loose our selection when the Value is set so we need to reselect it without firing the selection changed event
            TextBox.Select(info.StartPosition, info.Length);
            _fireSelectionChangedEvent = true;
        }

        #endregion //Private

        protected object ConvertTextToValue(string text)
        {
            throw new NotImplementedException("ConvertTextToValue");
        }

        protected string ConvertValueToText(object value)
        {
            if (value == null) return string.Empty;

            DateTime dt = DateTime.Parse(value.ToString(), CultureInfo.CurrentCulture);
            return dt.ToString(GetFormatString(Format), CultureInfo.CurrentCulture);
        }

        protected void SyncTextAndValueProperties(DependencyProperty p, object newValue)
        {
            //prevents recursive syncing properties
            if (_isSyncingTextAndValueProperties)
                return;

            _isSyncingTextAndValueProperties = true;

            //this only occures when the user typed in the value
            if (InputBase.TextProperty == p)
            {
                string text = newValue == null ? String.Empty : newValue.ToString();
                SetValue(DateTimeUpDown.ValueProperty, ConvertTextToValue(text));
            }

            SetValue(InputBase.TextProperty, ConvertValueToText(newValue));

            _isSyncingTextAndValueProperties = false;
        }

        #endregion //Methods
    }
}
