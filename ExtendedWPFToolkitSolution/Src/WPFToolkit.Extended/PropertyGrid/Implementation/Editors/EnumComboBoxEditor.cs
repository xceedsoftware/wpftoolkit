using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class EnumComboBoxEditor : ComboBoxEditor
    {
        protected override IList<object> CreateItemsSource(PropertyItem propertyItem)
        {
            return GetValues(propertyItem.PropertyType);
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
