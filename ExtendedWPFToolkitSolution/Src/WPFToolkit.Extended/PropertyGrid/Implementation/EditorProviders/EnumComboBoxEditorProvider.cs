using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Reflection;

namespace Microsoft.Windows.Controls.PropertyGrid.Implementation.EditorProviders
{
    public class EnumComboBoxEditorProvider : ITypeEditorProvider
    {
        ComboBox _comboBox;

        public EnumComboBoxEditorProvider()
        {
            _comboBox = new ComboBox();           
        }

        public void Initialize(PropertyItem propertyItem)
        {
            ResolveBinding(propertyItem);
            SetItemsSource(propertyItem);
        }

        public FrameworkElement ResolveEditor()
        {
            return _comboBox;
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

            BindingOperations.SetBinding(_comboBox, ComboBox.SelectedItemProperty, binding);
        }

        private void SetItemsSource(PropertyItem property)
        {
            _comboBox.ItemsSource = GetValues(property.PropertyType);
        }

        public static object[] GetValues(Type enumType)
        {
            List<object> values = new List<object>();

            var fields = from field in enumType.GetFields()
                         where field.IsLiteral
                         select field;

            foreach (FieldInfo field in fields)
            {
                values.Add(field.GetValue(enumType));
            }

            return values.ToArray();
        }
    }
}
