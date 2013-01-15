using System;
using System.Windows;

namespace Microsoft.Windows.Controls
{
    public delegate void CheckListBoxCheckedChangedEventHandler(object sender, CheckListBoxCheckedChangedEventArgs e);
    public class CheckListBoxCheckedChangedEventArgs : RoutedEventArgs
    {
        public object Item { get; private set; }

        public CheckListBoxCheckedChangedEventArgs(RoutedEvent routedEvent, object source, object item)
            : base(routedEvent, source)
        {
            Item = item;
        }
    }
}
