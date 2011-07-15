using System;
using Microsoft.Windows.Controls.Core.Converters;

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
