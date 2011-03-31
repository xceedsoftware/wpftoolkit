using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class DecimalUpDownEditor : TypeEditor<DecimalUpDown>
    {
        protected override void SetValueDependencyProperty()
        {
            ValueProperty = DecimalUpDown.ValueProperty;
        }
    }
}
