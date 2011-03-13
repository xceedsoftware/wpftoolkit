using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class IntegerUpDownEditor : NumericUpDownEditor
    {
        protected override void SetEditorProperties()
        {
            NumericUpDown nud = (NumericUpDown)Editor;
            nud.ValueType = typeof(int);
            nud.FormatString = "F0";
        }
    }
}
