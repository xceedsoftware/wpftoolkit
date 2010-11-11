using System;

namespace Microsoft.Windows.Controls
{
    internal class DateTimeInfo
    {
        public string Content { get; set; }
        public bool? IsReadOnly { get; set; }
        public int KeyPressCount { get; set; }
        public int Length { get; set; }
        public string MonthName { get; set; }
        public string Pattern { get; set; }
        public int StartPosition { get; set; }
        public DateTimePart Type { get; set; }
    }
}