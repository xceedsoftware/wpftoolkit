using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class ColorEditor : TypeEditor<ColorPicker>
    {
        protected override void SetControlProperties()
        {
            Editor.BorderThickness = new System.Windows.Thickness(0);
            Editor.DisplayColorAndName = true;
        }
        protected override void SetValueDependencyProperty()
        {
            ValueProperty = ColorPicker.SelectedColorProperty;
        }
    }
}
