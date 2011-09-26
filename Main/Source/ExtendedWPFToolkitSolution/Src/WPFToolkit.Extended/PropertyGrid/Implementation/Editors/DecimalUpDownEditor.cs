using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class DecimalUpDownEditor : TypeEditor<DecimalUpDown>
    {
        protected override void SetControlProperties()
        {
            Editor.BorderThickness = new System.Windows.Thickness(0);
        }

        protected override void SetValueDependencyProperty()
        {
            ValueProperty = DecimalUpDown.ValueProperty;
        }
    }
}
