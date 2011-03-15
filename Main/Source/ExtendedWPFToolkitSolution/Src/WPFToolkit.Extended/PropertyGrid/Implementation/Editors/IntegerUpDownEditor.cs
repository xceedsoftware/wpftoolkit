using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class IntegerUpDownEditor : NumericUpDownEditor
    {
        protected override void SetControlProperties()
        {
            Editor.Maximum = int.MaxValue;
            Editor.Minimum = int.MinValue;
            Editor.ValueType = typeof(int);
            Editor.FormatString = "F0";
        }
    }
}
