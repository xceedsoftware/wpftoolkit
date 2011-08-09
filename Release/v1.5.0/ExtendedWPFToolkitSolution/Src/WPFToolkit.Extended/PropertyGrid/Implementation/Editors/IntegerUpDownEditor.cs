using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class IntegerUpDownEditor : TypeEditor<IntegerUpDown>
    {
        protected override void SetValueDependencyProperty()
        {
            ValueProperty = IntegerUpDown.ValueProperty;
        }
    }
}
