using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class DateTimeUpDownEditor : TypeEditor<DateTimeUpDown>
    {
        protected override void SetControlProperties()
        {
            Editor.BorderThickness = new System.Windows.Thickness(0);
        }
        protected override void SetValueDependencyProperty()
        {
            ValueProperty = DateTimeUpDown.ValueProperty;
        }
    }
}
