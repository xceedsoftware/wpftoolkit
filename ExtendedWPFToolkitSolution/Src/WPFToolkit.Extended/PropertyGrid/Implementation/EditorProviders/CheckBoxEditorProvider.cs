using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;

namespace Microsoft.Windows.Controls.PropertyGrid.Implementation.EditorProviders
{
    public class CheckBoxEditorProvider : ITypeEditorProvider
    {
        CheckBox _checkbox;

        public CheckBoxEditorProvider()
        {
            _checkbox = new CheckBox();
            _checkbox.Margin = new Thickness(2, 0, 0, 0);
        }

        public void Initialize(PropertyItem propertyItem)
        {
            ResolveBinding(propertyItem);
        }

        public FrameworkElement ResolveEditor()
        {
            return _checkbox;
        }

        private void ResolveBinding(PropertyItem property)
        {
            var binding = new Binding(property.Name);
            binding.Source = property.Instance;
            binding.ValidatesOnExceptions = true;
            binding.ValidatesOnDataErrors = true;

            if (property.IsWriteable)
                binding.Mode = BindingMode.TwoWay;
            else
                binding.Mode = BindingMode.OneWay;

            BindingOperations.SetBinding(_checkbox, CheckBox.IsCheckedProperty, binding);
        }


    }
}
