using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.Windows.Controls
{
    internal class DateTimeParser
    {
        #region Properties

        private DateTimeFormatInfo DateTimeFormatInfo { get; set; }

        public string Format { get; set; }

        private IEnumerable<string> MonthNames { get { return DateTimeFormatInfo.AbbreviatedMonthNames.Union(DateTimeFormatInfo.MonthNames); } }

        #endregion //Properties

        #region Constructors

        public DateTimeParser(DateTimeFormatInfo dateTimeFormatInfo)
        {
            DateTimeFormatInfo = dateTimeFormatInfo;
        }

        public DateTimeParser(DateTimeFormatInfo dateTimeFormatInfo, string format)
        {
            DateTimeFormatInfo = dateTimeFormatInfo;
            Format = format;
        }

        #endregion //Constructors

        #region Methods

        public bool TryParse(string value, out DateTime result, DateTime currentDate)
        {
            bool success = false;
            result = currentDate;

            if (string.IsNullOrEmpty(value))
                return false;

            //parse date
            DateTime date;
            success = TryParseDate(value, out date, currentDate);

            //parse time
            DateTime time;
            success = TryParseTime(value, out time, currentDate);

            //merge the two
            result = MergeDateAndTime(date, time);

            return success;
        }

        #region Parse Date

        public bool TryParseDate(string value, out DateTime result, DateTime currentDate)
        {
            bool success = false;
            result = currentDate;

            if (string.IsNullOrEmpty(value))
                return false;

            var dateParts = GetDateParts(ResolveDateString(value)).ToArray();

            if (dateParts.Length > 0)
            {
                var dateFormatParts = DateTimeFormatInfo.ShortDatePattern.Split(new string[] { DateTimeFormatInfo.DateSeparator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                int yearIndex = dateFormatParts.IndexOf(dateFormatParts.FirstOrDefault(e => e.Contains("y") || e.Contains("Y")));
                if (yearIndex >= 0 && yearIndex < dateParts.Length && dateParts[yearIndex].Length <= 2 && !dateParts.Any(dp => dp.Length > 2 && dp != dateParts[yearIndex]))
                {
                    if (dateParts[yearIndex].Length == 0)
                    {
                        dateParts[yearIndex] = "00";
                    }
                    else if (dateParts[yearIndex].Length == 1)
                    {
                        dateParts[yearIndex] = "0" + dateParts[yearIndex];
                    }

                    dateParts[yearIndex] = string.Format("{0}{1}", currentDate.Year / 100, dateParts[yearIndex]);
                }

                success = DateTime.TryParse(string.Join(DateTimeFormatInfo.DateSeparator, dateParts), DateTimeFormatInfo, DateTimeStyles.None, out result);
                if (!success)
                    result = currentDate;
            }

            return success;
        }

        private string ResolveDateString(string date)
        {
            string[] dateParts = new string[3]; // Month/Day/Year

            string[] dateSeparators = new string[] { ",", " ", "/", "-", "T" };

            var dates = date.Split(dateSeparators, StringSplitOptions.RemoveEmptyEntries).ToList();
            var formats = Format.Split(dateSeparators, StringSplitOptions.RemoveEmptyEntries).ToList();

            //strip out the date pieces
            for (int i = 0; i < formats.Count; i++)
            {
                var format = formats[i];
                if (!format.Equals("dddd") && !format.Contains(DateTimeFormatInfo.AMDesignator) && !format.Contains(DateTimeFormatInfo.PMDesignator))
                {
                    if (format.Contains("M"))
                        dateParts[0] = dates[i];
                    else if (format.Contains("d"))
                        dateParts[1] = dates[i];
                    else if (format.Contains("y"))
                        dateParts[2] = dates[i];
                }
            }

            return string.Join(DateTimeFormatInfo.DateSeparator, dateParts);
        }

        protected List<string> GetDateParts(string date)
        {
            var months = new Regex(GetDatePattern(), RegexOptions.IgnoreCase);
            var dateParts = months.Matches(date)
                                 .OfType<Match>()
                                 .Select(match => match.Value)
                                 .Where(s => !string.IsNullOrEmpty(s))
                                 .ToList();
            return dateParts;
        }

        protected string GetDatePattern()
        {
            var pattern = new StringBuilder(@"[0-9]+");

            foreach (var m in MonthNames.Where(m => !string.IsNullOrEmpty(m)))
            {
                pattern.AppendFormat(@"|(?<=\b|\W|[0-9_]){0}(?=\b|\W|[0-9_])", m);
            }

            return pattern.ToString();
        }

        #endregion //Parse Date

        #region Parse Time

        public bool TryParseTime(string value, out DateTime result, DateTime fallback)
        {
            bool success = false;
            result = fallback;

            if (string.IsNullOrEmpty(value))
                return false;

            return success;
        }

        private string ResolveTimeString(string time)
        {
            return string.Empty;
        }

        #endregion //Parse Time

        public static DateTime MergeDateAndTime(DateTime date, DateTime time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
        }

        #endregion // Methods
    }
}
