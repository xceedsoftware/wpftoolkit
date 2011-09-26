using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class IntegerUpDownEditor : TypeEditor<IntegerUpDown>
    {
        protected override void SetControlProperties()
        {
            Editor.BorderThickness = new System.Windows.Thickness(0);
        }
        protected override void SetValueDependencyProperty()
        {
            ValueProperty = IntegerUpDown.ValueProperty;
        }
    }
}
