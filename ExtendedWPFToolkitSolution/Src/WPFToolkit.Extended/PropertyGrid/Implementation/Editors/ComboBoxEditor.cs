using System.Collections.Generic;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public abstract class ComboBoxEditor : TypeEditor
    {
        protected override void Initialize()
        {
            Editor = new ComboBox();
            ValueProperty = ComboBox.SelectedItemProperty;
        }

        public override void Attach(PropertyItem propertyItem)
        {
            SetItemsSource(propertyItem);
            base.Attach(propertyItem);
        }

        private void SetItemsSource(PropertyItem propertyItem)
        {
            (Editor as ComboBox).ItemsSource = CreateItemsSource(propertyItem);
        }

        protected abstract IList<object> CreateItemsSource(PropertyItem propertyItem);
    }
}
