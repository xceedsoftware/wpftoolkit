using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Windows.Controls.PropertyGrid.Attributes;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class ItemsSourceEditor : ComboBoxEditor
    {
        private ItemsSourceAttribute _attribute;

        public ItemsSourceEditor(ItemsSourceAttribute attribute)
        {
            _attribute = attribute;
        }

        protected override IList<object> CreateItemsSource(PropertyItem propertyItem)
        {
            var instance = Activator.CreateInstance(_attribute.Type);
            return (instance as IItemsSource).GetValues();
        }
    }
}
