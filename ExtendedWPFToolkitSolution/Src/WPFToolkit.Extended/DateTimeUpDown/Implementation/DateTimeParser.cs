using System;
using System.Linq;
using System.Globalization;

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

            if (string.IsNullOrEmpty(value))
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
            //  1/1/0001 12:00:00 AM
            string[] dateParts = new string[3] { currentDate.Month.ToString(), currentDate.Day.ToString(), currentDate.Year.ToString() };
            string[] timeParts = new string[3] { currentDate.Hour.ToString(), currentDate.Minute.ToString(), currentDate.Second.ToString() };
            string designator = "AM";

            string[] dateTimeSeparators = new string[] { ",", " ", "-", DateTimeFormatInfo.DateSeparator, DateTimeFormatInfo.TimeSeparator };

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
                        dateParts[0] = dateTimeParts[i];
                    else if (format.Contains("d"))
                        dateParts[1] = dateTimeParts[i];
                    else if (format.Contains("y"))
                    {
                        dateParts[2] = dateTimeParts[i];
                        if (dateParts[2].Length == 2)
                            dateParts[2] = string.Format("{0}{1}", currentDate.Year / 100, dateParts[2]);
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

            var date = string.Join(DateTimeFormatInfo.DateSeparator, dateParts);
            var time = string.Join(DateTimeFormatInfo.TimeSeparator, timeParts);

            return String.Format("{0} {1} {2}", date, time, designator);
        }

        #endregion // Methods
    }
}
