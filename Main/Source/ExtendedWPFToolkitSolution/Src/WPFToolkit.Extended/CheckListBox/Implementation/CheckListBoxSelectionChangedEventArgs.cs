using System;
using System.Windows;

namespace Microsoft.Windows.Controls
{
    public delegate void CheckListBoxSelectionChangedEventHandler(object sender, CheckListBoxSelectionChangedEventArgs e);
    public class CheckListBoxSelectionChangedEventArgs : RoutedEventArgs
    {
        public object Item { get; private set; }

        public CheckListBoxSelectionChangedEventArgs(RoutedEvent routedEvent, object source, object item)
            : base(routedEvent, source)
        {
            Item = item;
        }
    }
}
