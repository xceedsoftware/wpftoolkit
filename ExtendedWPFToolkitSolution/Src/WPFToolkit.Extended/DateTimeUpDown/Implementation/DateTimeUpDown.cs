using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Microsoft.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Microsoft.Windows.Controls
{
    public class DateTimeUpDown : UpDownBase<DateTime?>
    {
        #region Members

        private List<DateTimeInfo> _dateTimeInfoList = new List<DateTimeInfo>();
        private DateTimeInfo _selectedDateTimeInfo;
        private bool _fireSelectionChangedEvent = true;

        private DateTimeParser _dateTimeParser;

        #endregion //Members

        #region Properties

        private DateTimeFormatInfo DateTimeFormatInfo { get; set; }

        #region DefaultValue

        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(DateTime), typeof(DateTimeUpDown), new UIPropertyMetadata(DateTime.Now));
        public DateTime DefaultValue
        {
            get { return (DateTime)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        #endregion //DefaultValue

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
            _dateTimeParser.Format = GetFormatString(newValue);
            InitializeDateTimeInfoListAndParseValue();

            //I forgot why I set the text here. I believe it had to deal with databinding to the format. Either way we need to make sure there is a value before we try to set the text.
            if (Value.HasValue)
                Text = ConvertValueToText();
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
                throw new ArgumentException("FormatString should be specified.", "newValue");

            _dateTimeParser.Format = newValue;
            InitializeDateTimeInfoListAndParseValue();
        }

        #endregion //FormatString

        #region Maximum



        #endregion //Maximum

        #region Minimum



        #endregion //Minimum

        #endregion //Properties

        #region Constructors

        static DateTimeUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimeUpDown), new FrameworkPropertyMetadata(typeof(DateTimeUpDown)));
        }

        public DateTimeUpDown()
        {
            DateTimeFormatInfo = DateTimeFormatInfo.GetInstance(CultureInfo);
            _dateTimeParser = new DateTimeParser(DateTimeFormatInfo, GetFormatString(Format));
            InitializeDateTimeInfoList();
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TextBox.SelectionChanged += TextBox_SelectionChanged;
        }

        protected override DateTime? CoerceValue(DateTime? value)
        {
            //TODO: implement Minimum and Maximum
            return value;
        }

        protected override void OnIncrement()
        {
            if (Value.HasValue)
                UpdateDateTime(1);
            else
                Value = DefaultValue;
        }

        protected override void OnDecrement()
        {
            if (Value.HasValue)
                UpdateDateTime(-1);
            else
                Value = DefaultValue;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    {
                        if (IsEditable)
                        {
                            _fireSelectionChangedEvent = false;
                            BindingExpression binding = BindingOperations.GetBindingExpression(TextBox, System.Windows.Controls.TextBox.TextProperty);
                            binding.UpdateSource();
                            _fireSelectionChangedEvent = true;
                            e.Handled = true;
                        }
                        return;
                    }
                default:
                    {
                        _fireSelectionChangedEvent = false;
                        break;
                    }
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnTextChanged(string previousValue, string currentValue)
        {
            //TODO: clean this up and make sure it doesn't fire recursively
            if (String.IsNullOrEmpty(currentValue))
            {
                Value = null;
                return;
            }

            DateTime current = Value.HasValue ? Value.Value : DateTime.Now;
            DateTime result;
            var success = _dateTimeParser.TryParse(currentValue, out result, current);

            SyncTextAndValueProperties(InputBase.TextProperty, result.ToString());
        }

        protected override DateTime? ConvertTextToValue(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            return DateTime.Parse(text, CultureInfo.CurrentCulture);
        }

        protected override string ConvertValueToText()
        {
            if (Value == null) return string.Empty;

            DateTime dt = DateTime.Parse(Value.ToString(), CultureInfo);
            return dt.ToString(GetFormatString(Format), CultureInfo);
        }

        protected override void SetValidSpinDirection()
        {
            //TODO: implement Minimum and Maximum
        }

        protected override void OnValueChanged(DateTime? oldValue, DateTime? newValue)
        {
            //whenever the value changes we need to parse out the value into out DateTimeInfo segments so we can keep track of the individual pieces
            //but only if it is not null
            if (newValue != null)
                ParseValueIntoDateTimeInfo();

            base.OnValueChanged(oldValue, newValue);
        }

        protected override void ValidateValue(DateTime? value)
        {
            //TODO: implement min/max
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

        #region Methods

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

            if (format == null)
                return;

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
                            info = new DateTimeInfo { IsReadOnly = true, Type = DateTimePart.Other, Length = 1, Content = format.Substring(1, Math.Max(1, closingQuotePosition - 1)) };
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

        #endregion //Methods
    }
}
