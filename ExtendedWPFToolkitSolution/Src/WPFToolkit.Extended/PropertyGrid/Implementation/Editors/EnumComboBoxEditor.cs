using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class EnumComboBoxEditor : TypeEditor
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
            (Editor as ComboBox).ItemsSource = GetValues(propertyItem.PropertyType);
        }

        private static object[] GetValues(Type enumType)
        {
            List<object> values = new List<object>();

            var fields = enumType.GetFields().Where(x => x.IsLiteral);
            foreach (FieldInfo field in fields)
            {
                values.Add(field.GetValue(enumType));
            }

            return values.ToArray();
        }
    }
}
