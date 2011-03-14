using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class DateTimeUpDownEditor : TypeEditor
    {
        protected override void Initialize()
        {
            Editor = new DateTimeUpDown();
            ValueProperty = DateTimeUpDown.ValueProperty;
        }
    }
}
