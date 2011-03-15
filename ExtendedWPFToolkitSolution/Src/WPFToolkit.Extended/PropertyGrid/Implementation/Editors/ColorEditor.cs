using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class ColorEditor : TypeEditor<ColorPicker>
    {
        protected override void SetValueDependencyProperty()
        {
            ValueProperty = ColorPicker.SelectedColorProperty;
        }
    }
}
