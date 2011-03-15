using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class NumericUpDownEditor : TypeEditor<NumericUpDown>
    {
        protected override void SetValueDependencyProperty()
        {
            ValueProperty = NumericUpDown.ValueProperty;
        }
    }
}
