using System.Collections.Generic;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public abstract class ComboBoxEditor : TypeEditor<ComboBox>
    {
        protected override void SetValueDependencyProperty()
        {
            ValueProperty = ComboBox.SelectedItemProperty;
        }

        protected override void ResolveValueBinding(PropertyItem propertyItem)
        {
            SetItemsSource(propertyItem);
            base.ResolveValueBinding(propertyItem);
        }

        protected abstract IList<object> CreateItemsSource(PropertyItem propertyItem);

        private void SetItemsSource(PropertyItem propertyItem)
        {
            Editor.ItemsSource = CreateItemsSource(propertyItem);
        }
    }
}
