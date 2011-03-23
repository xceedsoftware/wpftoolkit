using System;

namespace Microsoft.Windows.Controls
{
    public class TimeItem
    {
        public string Display { get; set; }
        public TimeSpan Time { get; set; }

        public TimeItem(string display, TimeSpan time)
        {
            Display = display;
            Time = time;
        }
    }
}
