using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class DateTimeUpDownEditor : TypeEditor<DateTimeUpDown>
    {
        protected override void SetValueDependencyProperty()
        {
            ValueProperty = DateTimeUpDown.ValueProperty;
        }
    }
}
