using System;
using System.Collections.Generic;

namespace Microsoft.Windows.Controls.PropertyGrid.Attributes
{
    public interface IItemsSource
    {
        IList<object> GetValues();
    }
}
