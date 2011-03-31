using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class DoubleUpDownEditor : TypeEditor<DoubleUpDown>
    {
        protected override void SetValueDependencyProperty()
        {
            ValueProperty = DoubleUpDown.ValueProperty;
        }
    }
}
