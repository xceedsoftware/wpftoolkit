using System;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Windows.Controls
{
    internal class DateTimeParser
    {
        #region Properties

        private DateTimeFormatInfo DateTimeFormatInfo { get; set; }

        public string Format { get; set; }

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

            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(Format))
                return false;

            var dateTimeString = ResolveDateTimeString(value, currentDate);

            if (!String.IsNullOrEmpty(dateTimeString))
                success = DateTime.TryParse(dateTimeString, DateTimeFormatInfo, DateTimeStyles.None, out result);

            if (!success)
                result = currentDate;

            return success;
        }

        private string ResolveDateTimeString(string dateTime, DateTime currentDate)
        {
            Dictionary<string, string> dateParts = GetDateParts(currentDate);
            string[] timeParts = new string[3] { currentDate.Hour.ToString(), currentDate.Minute.ToString(), currentDate.Second.ToString() };
            string designator = "";
            string[] dateTimeSeparators = new string[] { ",", " ", "-", ".", DateTimeFormatInfo.DateSeparator, DateTimeFormatInfo.TimeSeparator };

            var dateTimeParts = dateTime.Split(dateTimeSeparators, StringSplitOptions.RemoveEmptyEntries).ToList();
            var formats = Format.Split(dateTimeSeparators, StringSplitOptions.RemoveEmptyEntries).ToList();

            //something went wrong
            if (dateTimeParts.Count != formats.Count)
                return string.Empty;

            for (int i = 0; i < formats.Count; i++)
            {
                var format = formats[i];
                if (!format.Contains("ddd") && !format.Contains("GMT"))
                {
                    if (format.Contains("M"))
                        dateParts["Month"] = dateTimeParts[i];
                    else if (format.Contains("d"))
                        dateParts["Day"] = dateTimeParts[i];
                    else if (format.Contains("y"))
                    {
                        dateParts["Year"] = dateTimeParts[i];

                        if (dateParts["Year"].Length == 2)
                            dateParts["Year"] = string.Format("{0}{1}", currentDate.Year / 100, dateParts["Year"]);
                    }
                    else if (format.Contains("h") || format.Contains("H"))
                        timeParts[0] = dateTimeParts[i];
                    else if (format.Contains("m"))
                        timeParts[1] = dateTimeParts[i];
                    else if (format.Contains("s"))
                        timeParts[2] = dateTimeParts[i];
                    else if (format.Contains("t"))
                        designator = dateTimeParts[i];
                }
            }

            var date = string.Join(DateTimeFormatInfo.DateSeparator, dateParts.Select(x => x.Value).ToArray());
            var time = string.Join(DateTimeFormatInfo.TimeSeparator, timeParts);

            return String.Format("{0} {1} {2}", date, time, designator);
        }

        private Dictionary<string, string> GetDateParts(DateTime currentDate)
        {
            Dictionary<string, string> dateParts = new Dictionary<string, string>();
            var dateFormatParts = DateTimeFormatInfo.ShortDatePattern.Split(new string[] { DateTimeFormatInfo.DateSeparator }, StringSplitOptions.RemoveEmptyEntries).ToList();
            dateFormatParts.ForEach(item =>
            {
                string key = string.Empty;
                string value = string.Empty;

                if (item.Contains("M"))
                {
                    key = "Month";
                    value = currentDate.Month.ToString();
                }
                else if (item.Contains("d"))
                {
                    key = "Day";
                    value = currentDate.Day.ToString();
                }
                else if (item.Contains("y"))
                {
                    key = "Year";
                    value = currentDate.Year.ToString();
                }
                dateParts.Add(key, value);
            });
            return dateParts;
        }

        #endregion // Methods
    }
}
