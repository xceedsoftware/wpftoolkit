using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class ColorEditor : TypeEditor
    {
        protected override void Initialize()
        {
            Editor = new ColorPicker();
            ValueProperty = ColorPicker.SelectedColorProperty;
        }
    }
}
