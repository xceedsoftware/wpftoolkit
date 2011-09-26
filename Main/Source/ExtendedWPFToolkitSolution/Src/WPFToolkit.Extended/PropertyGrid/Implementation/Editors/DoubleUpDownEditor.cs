using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class DoubleUpDownEditor : TypeEditor<DoubleUpDown>
    {
        protected override void SetControlProperties()
        {
            Editor.BorderThickness = new System.Windows.Thickness(0);
        }

        protected override void SetValueDependencyProperty()
        {
            ValueProperty = DoubleUpDown.ValueProperty;
        }
    }
}
