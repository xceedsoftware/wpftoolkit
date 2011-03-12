using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;

namespace Microsoft.Windows.Controls.PropertyGrid.Implementation.EditorProviders
{
    public class TextBoxEditorProvider : ITypeEditorProvider
    {
        FrameworkElement _editor;

        public TextBoxEditorProvider()
        {
            _editor = new TextBox();
        }

        public void Initialize(PropertyItem propertyItem)
        {
            ResolveBinding(propertyItem);
        }

        public FrameworkElement ResolveEditor()
        {
            return _editor;
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

            BindingOperations.SetBinding(_editor, TextBox.TextProperty, binding);
        }
    }
}
