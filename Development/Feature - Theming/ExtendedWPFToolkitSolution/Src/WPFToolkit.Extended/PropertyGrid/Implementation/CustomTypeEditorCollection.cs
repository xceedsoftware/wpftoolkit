using System;
using System.Collections.ObjectModel;
using Microsoft.Windows.Controls.PropertyGrid.Editors;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class CustomTypeEditorCollection : ObservableCollection<ICustomTypeEditor>
    {
        public ICustomTypeEditor this[string propertyName]
        {
            get
            {
                foreach (var item in Items)
                {
                    if (item.Properties.Contains(propertyName))
                        return item;
                }

                return null;
            }
        }

        public ICustomTypeEditor this[Type targetType]
        {
            get
            {
                foreach (var item in Items)
                {
                    if (item.TargetType == targetType)
                        return item;
                }

                return null;
            }
        }
    }
}
