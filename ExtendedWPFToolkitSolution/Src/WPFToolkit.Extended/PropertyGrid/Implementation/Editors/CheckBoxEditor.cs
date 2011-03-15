using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class CheckBoxEditor : TypeEditor
    {
        protected override void Initialize()
        {
            Editor = new CheckBox();
            Editor.Margin = new Thickness(5, 0, 0, 0);
            ValueProperty = CheckBox.IsCheckedProperty;
        }
    }
}
