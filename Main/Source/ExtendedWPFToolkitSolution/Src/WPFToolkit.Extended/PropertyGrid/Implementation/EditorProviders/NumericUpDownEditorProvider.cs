using System;
using System.Windows.Data;
using System.Windows;

namespace Microsoft.Windows.Controls.PropertyGrid.Implementation.EditorProviders
{
    public class NumericUpDownEditorProvider : ITypeEditorProvider
    {
        NumericUpDown _numericUpDown;

        public NumericUpDownEditorProvider()
        {
            _numericUpDown = new NumericUpDown();
        }

        public void Initialize(PropertyItem propertyItem)
        {
            ResolveBinding(propertyItem);
        }

        public FrameworkElement ResolveEditor()
        {
            return _numericUpDown;
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

            BindingOperations.SetBinding(_numericUpDown, NumericUpDown.ValueProperty, binding);
        }
    }
}
