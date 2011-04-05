using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class PropertyCollection : ObservableCollection<object>
    {
        public PropertyCollection()
        {
            
        }
        public PropertyCollection(List<object> list)
            : base(list)
        {
            
        }
        public PropertyCollection(IEnumerable<object> collection)
            : base(collection)
        {
            
        }
    }
}
