using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class CheckBoxEditor : TypeEditor<CheckBox>
    {
        protected override void SetControlProperties()
        {
            Editor.Margin = new Thickness(5, 0, 0, 0);
        }

        protected override void SetValueDependencyProperty()
        {
            ValueProperty = CheckBox.IsCheckedProperty;
        }
    }
}
