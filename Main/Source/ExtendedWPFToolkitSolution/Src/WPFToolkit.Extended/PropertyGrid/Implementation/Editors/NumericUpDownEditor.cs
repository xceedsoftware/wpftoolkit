using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class NumericUpDownEditor : TypeEditor
    {
        protected override void Initialize()
        {
            Editor = new NumericUpDown();
            ValueProperty = NumericUpDown.ValueProperty;
            SetEditorProperties();
        }

        protected virtual void SetEditorProperties()
        {
            //TODO: override in derived classes to specify custom value type
        }
    }
}
