using System;

namespace Microsoft.Windows.Controls
{
    internal class DateTimeInfo
    {
        public string Content { get; set; }
        public string Format { get; set; }
        public bool IsReadOnly { get; set; }
        public int Length { get; set; }
        public int StartPosition { get; set; }
        public DateTimePart Type { get; set; }
    }
}