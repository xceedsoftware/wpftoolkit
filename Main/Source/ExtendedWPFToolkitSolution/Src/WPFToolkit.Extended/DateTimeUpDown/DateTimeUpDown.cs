using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace Microsoft.Windows.Controls
{
    public class DateTimeUpDown : UpDownBase
    {
        #region Members

        private List<DateTimeInfo> _dateTimeInfoList = new List<DateTimeInfo>();
        internal bool _selectionChanged = true;
        internal int _selectedDateTimeInfoIndex;

        #endregion //Members

        #region Properties

        private DateTimeFormatInfo DateTimeFormatInfo { get; set; }

        #region Format

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register("Format", typeof(DateTimeFormat), typeof(DateTimeUpDown), new UIPropertyMetadata(DateTimeFormat.ShortTime, OnFormatChanged));
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
            InitializeDateTimeInfoList();
        }

        #endregion //Format

        #endregion //Properties

        #region Constructors

        static DateTimeUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimeUpDown), new FrameworkPropertyMetadata(typeof(DateTimeUpDown)));
            ValueTypeProperty.OverrideMetadata(typeof(DateTimeUpDown), new FrameworkPropertyMetadata(typeof(Nullable<DateTime>)));
        }

        public DateTimeUpDown()
        {
            DateTimeFormatInfo = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentCulture);
        }

        protected override void OnValueChanged(object oldValue, object newValue)
        {
            //whenever the value changes we need to parse out the value into out DateTimeInfo segments so we can keep track of the individual pieces
            ParseValueIntoDateTimeInfo();
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TextBox.SelectionChanged += new RoutedEventHandler(TextBox_SelectionChanged);
        }

        #endregion //Base Class Overrides

        #region Event Hanlders

        void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_selectionChanged)
                SelectDateTimePart();
            else
                _selectionChanged = true;
        }

        #endregion //Event Hanlders

        #region Methods

        private void InitializeDateTimeInfoList()
        {
            string format = GetFormatString(Format);
            while (format.Length > 0)
            {
                int elementLength = GetGroupLengthByMask(format);

                switch (format[0])
                {
                    case '"':
                    case '\'':
                        {
                            int closingQuotePosition = format.IndexOf(format[0], 1);
                            _dateTimeInfoList.Add(new DateTimeInfo()
                            {
                                IsReadOnly = true,
                                Type = DateTimePart.Other,
                                Length = 1,
                                Content = format.Substring(1, Math.Max(1, closingQuotePosition - 1)).ToString(),
                            });
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
                            {
                                _dateTimeInfoList.Add(new DateTimeInfo()
                                {
                                    IsReadOnly = true,
                                    Type = DateTimePart.DayName,
                                    Pattern = d
                                });
                            }
                            else
                            {
                                _dateTimeInfoList.Add(new DateTimeInfo()
                                {
                                    IsReadOnly = false,
                                    Type = DateTimePart.Day,
                                    Pattern = d
                                });
                            }
                            break;
                        }
                    case 'F':
                    case 'f':
                        {
                            string f = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                f = "%" + f;

                            _dateTimeInfoList.Add(new DateTimeInfo()
                            {
                                IsReadOnly = false,
                                Type = DateTimePart.Millisecond,
                                Pattern = f
                            });
                            break;
                        }
                    case 'h':
                        {
                            string h = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                h = "%" + h;

                            _dateTimeInfoList.Add(new DateTimeInfo()
                            {
                                IsReadOnly = false,
                                Type = DateTimePart.Hour12,
                                Pattern = h
                            });
                            break;
                        }
                    case 'H':
                        {
                            string H = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                H = "%" + H;

                            _dateTimeInfoList.Add(new DateTimeInfo()
                            {
                                IsReadOnly = false,
                                Type = DateTimePart.Hour24,
                                Pattern = H
                            });
                            break;
                        }
                    case 'M':
                        {
                            string M = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                M = "%" + M;

                            if (elementLength >= 3)
                            {
                                _dateTimeInfoList.Add(new DateTimeInfo()
                                {
                                    IsReadOnly = false,
                                    Type = DateTimePart.MonthName,
                                    Pattern = M
                                });
                            }
                            else
                            {
                                _dateTimeInfoList.Add(new DateTimeInfo()
                                {
                                    IsReadOnly = false,
                                    Type = DateTimePart.Month,
                                    Pattern = M
                                });
                            }
                            break;
                        }
                    case 'S':
                    case 's':
                        {
                            string s = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                s = "%" + s;
                            _dateTimeInfoList.Add(new DateTimeInfo()
                            {
                                IsReadOnly = false,
                                Type = DateTimePart.Second,
                                Pattern = s
                            });
                            break;
                        }
                    case 'T':
                    case 't':
                        {
                            string t = format.Substring(0, elementLength);
                            if (elementLength == 1)
                            {
                                t = "%" + t;
                            }
                            _dateTimeInfoList.Add(new DateTimeInfo()
                            {
                                IsReadOnly = false,
                                Type = DateTimePart.AmPmDesignator,
                                Pattern = t
                            });
                            break;
                        }
                    case 'Y':
                    case 'y':
                        {
                            string y = format.Substring(0, elementLength);
                            if (elementLength == 1)
                            {
                                y = "%" + y;
                            }
                            _dateTimeInfoList.Add(new DateTimeInfo()
                            {
                                IsReadOnly = false,
                                Type = DateTimePart.Year,
                                Pattern = y
                            });
                            break;
                        }
                    case '\\':
                        {
                            if (format.Length >= 2)
                            {
                                _dateTimeInfoList.Add(new DateTimeInfo()
                                {
                                    IsReadOnly = true,
                                    Content = format.Substring(1, 1),
                                    Length = 1,
                                    Type = DateTimePart.Other
                                });
                                elementLength = 2;
                            }
                            break;
                        }
                    case 'g':
                        {
                            string g = format.Substring(0, elementLength);
                            if (elementLength == 1)
                            {
                                g = "%" + g;
                            }
                            _dateTimeInfoList.Add(new DateTimeInfo()
                            {
                                IsReadOnly = true,
                                Type = DateTimePart.Period,
                                Pattern = format.Substring(0, elementLength)
                            });
                            break;
                        }
                    case 'm':
                        {
                            string m = format.Substring(0, elementLength);
                            if (elementLength == 1)
                            {
                                m = "%" + m;
                            }
                            _dateTimeInfoList.Add(new DateTimeInfo()
                            {
                                IsReadOnly = false,
                                Type = DateTimePart.Minute,
                                Pattern = m
                            });
                            break;
                        }
                    case 'z':
                        {
                            string z = format.Substring(0, elementLength);
                            if (elementLength == 1)
                            {
                                z = "%" + z;
                            }
                            _dateTimeInfoList.Add(new DateTimeInfo()
                            {
                                IsReadOnly = true,
                                Type = DateTimePart.TimeZone,
                                Pattern = z
                            });
                            break;
                        }
                    default:
                        {
                            elementLength = 1;
                            _dateTimeInfoList.Add(new DateTimeInfo()
                            {
                                IsReadOnly = true,
                                Length = 1,
                                Content = format[0].ToString(),
                                Type = DateTimePart.Other
                            });
                            break;
                        }
                }

                format = format.Substring(elementLength);
            }
        }

        private static int GetGroupLengthByMask(string mask)
        {
            for (int i = 1; i < mask.Length; i++)
            {
                if (String.Compare(mask[i].ToString(), mask[0].ToString(), false) != 0)
                {
                    return i;
                }
            }
            return mask.Length;
        }

        public void ParseValueIntoDateTimeInfo()
        {
            string displayText = string.Empty;

            for (int i = 0; i < _dateTimeInfoList.Count; i++)
            {
                var charProperty = _dateTimeInfoList[i];

                if (charProperty.Pattern == null)
                {
                    _dateTimeInfoList[i].StartPosition = displayText.Length;
                    _dateTimeInfoList[i].Length = charProperty.Content.Length;
                    displayText += charProperty.Content;
                }
                else
                {
                    DateTime date = DateTime.Parse(Value.ToString());

                    _dateTimeInfoList[i].StartPosition = displayText.Length;

                    charProperty.Content = date.ToString(charProperty.Pattern, DateTimeFormatInfo);

                    _dateTimeInfoList[i].Length = charProperty.Content.Length;

                    displayText += charProperty.Content;
                }
            }
        }

        private void SelectDateTimePart()
        {
            for (int i = 0; i < _dateTimeInfoList.Count; i++)
            {
                var charProperty = _dateTimeInfoList[i];

                if ((charProperty.StartPosition <= TextBox.SelectionStart) && (TextBox.SelectionStart < (charProperty.StartPosition + charProperty.Length)))
                {
                    if (charProperty.IsReadOnly == false)
                    {
                        _selectionChanged = false;

                        for (int j = 0; j < _dateTimeInfoList.Count; j++)
                        {
                            _dateTimeInfoList[j].KeyPressCount = 0;
                        }

                        TextBox.Select(charProperty.StartPosition, charProperty.Length);
                        _selectionChanged = true;
                        _selectedDateTimeInfoIndex = i;

                        return;
                    }
                    else
                    {
                        _selectionChanged = false;

                        for (int j = 0; j < _dateTimeInfoList.Count; j++)
                        {
                            _dateTimeInfoList[j].KeyPressCount = 0;
                        }

                        TextBox.Select(charProperty.StartPosition, charProperty.Length);
                        _selectionChanged = true;
                        _selectedDateTimeInfoIndex = i;
                        return;
                    }
                }
            }
            _selectionChanged = false;
            TextBox.Select(TextBox.SelectionStart, 0);
            _selectionChanged = true;
            _selectedDateTimeInfoIndex = -1;
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
                default:
                    throw new ArgumentException("Not a supported format");
            }
        }

        #region Abstract

        protected override void OnIncrement()
        {
            DateTimeInfo info = _dateTimeInfoList[_selectedDateTimeInfoIndex];

            _selectionChanged = false;

            //TODO: refactor
            if (info.Type == DateTimePart.Year)
            {
                Value = ((DateTime)Value).AddYears(1);
            }
            else if (info.Type == DateTimePart.Month || info.Type == DateTimePart.MonthName)
            {
                Value = ((DateTime)Value).AddMonths(1);
            }
            else if (info.Type == DateTimePart.Day || info.Type == DateTimePart.DayName)
            {
                Value = ((DateTime)Value).AddDays(1);
            }
            else if (info.Type == DateTimePart.Hour12 || info.Type == DateTimePart.Hour24)
            {
                Value = ((DateTime)Value).AddHours(1);
            }
            else if (info.Type == DateTimePart.Minute)
            {
                Value = ((DateTime)Value).AddMinutes(1);
            }
            else if (info.Type == DateTimePart.Second)
            {
                Value = ((DateTime)Value).AddSeconds(1);
            }
            else if (info.Type == DateTimePart.Millisecond)
            {
                Value = ((DateTime)Value).AddMilliseconds(1);
            }
            else if (info.Type == DateTimePart.AmPmDesignator)
            {
                Value = ((DateTime)Value).AddHours(12);
            }

            //we loose our selection when the Value is set so we need to reselect it without firing the selection changed event
            TextBox.Select(info.StartPosition, info.Length);

            _selectionChanged = true;
        }

        protected override void OnDecrement()
        {
            DateTimeInfo info = _dateTimeInfoList[_selectedDateTimeInfoIndex];

            _selectionChanged = false;

            //TODO: refactor
            if (info.Type == DateTimePart.Year)
            {
                Value = ((DateTime)Value).AddYears(-1);
            }
            else if (info.Type == DateTimePart.Month || info.Type == DateTimePart.MonthName)
            {
                Value = ((DateTime)Value).AddMonths(-1);
            }
            else if (info.Type == DateTimePart.Day || info.Type == DateTimePart.DayName)
            {
                Value = ((DateTime)Value).AddDays(-1);
            }
            else if (info.Type == DateTimePart.Hour12 || info.Type == DateTimePart.Hour24)
            {
                Value = ((DateTime)Value).AddHours(-1);
            }
            else if (info.Type == DateTimePart.Minute)
            {
                Value = ((DateTime)Value).AddMinutes(-1);
            }
            else if (info.Type == DateTimePart.Second)
            {
                Value = ((DateTime)Value).AddSeconds(-1);
            }
            else if (info.Type == DateTimePart.Millisecond)
            {
                Value = ((DateTime)Value).AddMilliseconds(-1);
            }
            else if (info.Type == DateTimePart.AmPmDesignator)
            {
                Value = ((DateTime)Value).AddHours(-12);
            }

            //we loose our selection when the Value is set so we need to reselect it without firing the selection changed event
            TextBox.Select(info.StartPosition, info.Length);

            _selectionChanged = true;
        }

        protected override object ConvertTextToValue(string text)
        {
            return text;
        }

        protected override string ConvertValueToText(object value)
        {
            if (value == null) return string.Empty;

            DateTime? dt = DateTime.Parse(value.ToString(), CultureInfo.CurrentCulture);

            if (dt.HasValue)
                return dt.Value.ToString(GetFormatString(Format), CultureInfo.CurrentCulture);
            else
                return string.Empty;
        }

        #endregion //Abstract
        #endregion //Methods
    }
}
